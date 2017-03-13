namespace MSDeployTester.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using MSDeployTester.Core.Models;

    public class MsBuildService
    {
        public void BuildSolution(
            string buildExePath,
            string solutionFilePath, 
            string platform, 
            string configuration, 
            bool deployOnBuild,
            bool parameteriseConnectionStrings,
            Action<string> informationHandler = null,
            Action<string> errorHandler = null)
        {
            var buildParameters = new Dictionary<string, string>
            {                
                { "DeployOnBuild", deployOnBuild ? "True" : "False" },
                { "AutoParameterizationWebConfigConnectionStrings", parameteriseConnectionStrings.ToString() },
                { "platform", $"\"{platform}\"" },
                { "configuration", $"\"{configuration}\"" },
                { "VisualStudioVersion", "\"14.0\"" }
            };

            var buildArguments =
                $"\"{solutionFilePath}\" /nologo /nr:false {string.Join(" ", buildParameters.Select(p => $"/p:{p.Key}={p.Value}"))}";

            new ProcessService().Launch(
                buildExePath, 
                buildArguments, 
                informationHandler,
                errorHandler);
        }

        public IEnumerable<VsProject> GetDeployableProjectPaths(string solutionFilePath, string configuration)
        {
            if (string.IsNullOrWhiteSpace(solutionFilePath))
            {
                throw new ArgumentNullException(nameof(solutionFilePath));
            }

            if (string.IsNullOrWhiteSpace(configuration))
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var solutionFolder = Path.GetDirectoryName(solutionFilePath);
            if (string.IsNullOrWhiteSpace(solutionFolder))
            {
                throw new InvalidOperationException("Unexpected missing solution folder");
            }

            var packageFolder = $@"\obj\{configuration}\Package";
            var projectFolders = Directory.GetDirectories(
                solutionFolder,
                "*.*",
                SearchOption.AllDirectories)          
                .Where(p => p.EndsWith(packageFolder))
                .Select(p => p.Replace(packageFolder, string.Empty))
                .ToList();

            return projectFolders.Select(
                f =>
                    {
                        var projectFiles = Directory.GetFiles(f, "*.*proj");
                        if (projectFiles.Length != 1)
                        {
                            throw new InvalidOperationException(
                                $"Unexpected number of projects (*.*proj) found in '{f}'");
                        }

                        return new VsProject(projectFiles[0]);
                    });
        }
    }
}
