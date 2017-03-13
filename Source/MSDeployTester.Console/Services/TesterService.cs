namespace MSDeployTester.Console.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using MSDeployTester.Console.Options;
    using MSDeployTester.Core.Dtos;
    using MSDeployTester.Core.Logging;
    using MSDeployTester.Core.Models;
    using MSDeployTester.Core.Services;

    public class TesterService
    {
        private readonly ILogger logger;

        private readonly MsDeploySettings settings;

        public TesterService(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;

            this.settings = new SettingsService().Parse<MsDeploySettingsDto>("settings.json").ToSettings();
        }

        [ImportMany]
        private IEnumerable<IValuesProvider> ValueProviders { get; set; }

        public void Package(PackageOptions options)
        {
            try
            {
                var deployableProjects = this.BuildAndPublishSolution(
                    build: !options.ExcludeBuild,
                    parameteriseConnectionStrings: options.ParameteriseConnectionStrings,
                    solutionFile: options.SolutionFile,
                    platform: options.Platform,
                    configuration: options.Configuration);

                var date = DateTime.Now;
                var applicationPath = new ApplicationService().GetApplicationDirectory();
                var fileCopyService = new FileService();

                foreach (var deployableProject in deployableProjects)
                {
                    var packagePath = Path.Combine(
                        deployableProject.Folder,
                        $@"obj\{options.Configuration}\Package\PackageTmp");

                    var webConfigPath = Path.Combine(packagePath, "Web.config");

                    var outputFolder = Path.Combine(
                        applicationPath,
                        $@"output\{date:yyyy-MM-dd}\{deployableProject.ProjectName}\build");

                    fileCopyService.CopyFile(webConfigPath, outputFolder);
                }

                this.logger.Information("Package complete.");
            }
            catch (Exception exception)
            {
                this.logger.Error($"An error occurred packaging: {exception}");
                throw;
            }
        }

        public void Deploy(DeployOptions options)
        {
            try
            {
                var deployableProjects = this.BuildAndPublishSolution(
                    build: !options.ExcludeBuild,
                    parameteriseConnectionStrings: options.ParameteriseConnectionStrings,
                    solutionFile: options.SolutionFile,
                    platform: options.Platform,
                    configuration: options.Configuration);

                var date = DateTime.Now;
                var applicationPath = new ApplicationService().GetApplicationDirectory();
                var fileService = new FileService();

                // Get list of IIS Express sites
                var iisExpressService = new IisExpressService(this.settings.AppCmdPath);
                var sites = iisExpressService.GetSites().ToList();
                var nextSiteId = sites.Max(s => s.Id) + 1;

                foreach (var deployableProject in deployableProjects)
                {                 
                    this.logger.Information($"Deploying '{deployableProject.ProjectName}'");

                    // Create a new IIS Express site
                    var newSiteName = Guid.NewGuid().ToString().Replace("-", string.Empty);
                    var newSitePhysicalPath = Path.Combine(applicationPath, $@"output\sites\{newSiteName}");

                    iisExpressService.AddSite(
                        id: nextSiteId,
                        name: newSiteName,
                        bindings: $"http:/*:80:www.{newSiteName}.com",
                        physicalPath: newSitePhysicalPath);

                    // Copy package folder to application folder                    
                    var outputProjectFolder = Path.Combine(
                        applicationPath,
                        $@"output\{date:yyyy-MM-dd}\{deployableProject.ProjectName}");

                    var packageFolder = Path.Combine(
                        deployableProject.Folder,
                        $@"obj\{options.Configuration}\Package");

                    var outputPackageFolder = Path.Combine(outputProjectFolder, "package");
                    fileService.CopyFiles(packageFolder, "*.*", outputPackageFolder);

                    // Replace SetParameters.xml values including IIS Application name
                    var pluginsFolder = Path.Combine(applicationPath, "plugins");
                    var valuesProvider = this.GetValuesProvider(pluginsFolder, options.ValueProviderId);

                    if (valuesProvider == null)
                    {
                        throw new InvalidOperationException(
                            $"The values provider '{options.ValueProviderId}' was not located. "
                            + $"Check that the value provider assemblies are within a subfolder of the plugins folder at '{pluginsFolder}'");
                    }

                    if (string.IsNullOrWhiteSpace(options.ValueProviderOptions))
                    {
                        throw new InvalidOperationException("Unexpected empty value provider options provided");
                    }

                    var setParametersValues = new List<SetParameter>
                    {
                        SetParameter.AsKeyed("IIS Web Application Name", newSiteName)
                    };
                    
                    var values = valuesProvider.GetValues(options.ValueProviderOptions).ToList();
                    if (values.GroupBy(v => v.Key).Any(g => g.Count() > 1))
                    {
                        throw new InvalidOperationException(
                            $"Value provider '{options.ValueProviderId}' returned one or more parameters that do not have distinct keys");
                    }

                    foreach (var value in values)
                    {
                        var existingParameter = 
                            setParametersValues.FirstOrDefault(p => p.Key == value.Key);

                        if (existingParameter != null)
                        {
                            continue;
                        }

                        setParametersValues.Add(value);
                    }

                    var setParametersFilePath = Path.Combine(
                        outputPackageFolder,
                        $"{deployableProject.ProjectName}.SetParameters.xml");

                    // TODO: make pre/post prefix configurable
                    const string PrePostFix = "__";
                    var setParametersService = new SetParametersService();
                    setParametersService.TokenReplacement(
                        setParametersFilePath,
                        setParametersValues,
                        PrePostFix);

                    // Run MSDeploy deploy.cmd to deploy to the new IIS Express site
                    var deployScriptFilePath =
                        $"{setParametersFilePath.Replace("SetParameters.xml", string.Empty)}deploy.cmd";

                    setParametersService.Deploy(deployScriptFilePath, "/Y /L");

                    // Copy final Web.config file to deploy folder
                    var webConfigPath = Path.Combine(newSitePhysicalPath, "Web.config");
                    var deployFolder = Path.Combine(outputProjectFolder, "deploy");
                    fileService.CopyFile(webConfigPath, deployFolder);

                    // Clean up temporary IIS Express site
                    // fileCopyService.DeleteFolder(newSitePhysicalPath);
                    iisExpressService.DeleteSite(newSiteName);

                    nextSiteId++;
                }

                this.logger.Information("Deploy complete.");
            }
            catch (Exception exception)
            {
                this.logger.Error($"An error occurred deploying: {exception}");
                throw;
            }
        }

        private IValuesProvider GetValuesProvider(string pluginsFolder, string valueProviderId)
        {
            if (string.IsNullOrWhiteSpace(valueProviderId))
            {
                throw new ArgumentNullException(nameof(valueProviderId));
            }

            try
            {
                var catalog = new AggregateCatalog();

                // Iterate over all subfolders in the plugins directory
                foreach (var path in Directory.EnumerateDirectories(pluginsFolder, "*", SearchOption.TopDirectoryOnly))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(path));
                }

                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                return this.ValueProviders.FirstOrDefault(v => v.Id == valueProviderId);
            }
            catch (ReflectionTypeLoadException exception)
            {
                var message = string.Join(" ", (object[])exception.LoaderExceptions);
                throw new Exception(
                    $"There was an exception loading plugins: {message}");
            }
        }

        private IEnumerable<VsProject> BuildAndPublishSolution(
            bool build,
            bool parameteriseConnectionStrings,
            string solutionFile, 
            string platform, 
            string configuration)
        {
            var buildService = new MsBuildService();

            if (build)
            {
                buildService.BuildSolution(
                    this.settings.BuildPath,
                    solutionFile,
                    platform,
                    configuration,
                    deployOnBuild: true,
                    parameteriseConnectionStrings: parameteriseConnectionStrings,
                    informationHandler: m => this.logger.Information(m),
                    errorHandler: m => this.logger.Error(m));
            }

            return buildService.GetDeployableProjectPaths(
                solutionFile,
                configuration).ToList();
        }
    }
}