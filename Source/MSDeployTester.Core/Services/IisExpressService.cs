namespace MSDeployTester.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MSDeployTester.Core.Models;

    public class IisExpressService
    {
        private readonly string appCmdPath;

        private readonly ProcessService processService;

        public IisExpressService(string appCmdPath)
        {
            if (string.IsNullOrWhiteSpace(appCmdPath))
            {
                throw new ArgumentNullException(nameof(appCmdPath));
            }

            this.appCmdPath = appCmdPath;
            this.processService = new ProcessService();
        }

        public IEnumerable<IisExpressSite> GetSites()
        {
            var sites = new List<IisExpressSite>();

            this.processService.Launch(
                this.appCmdPath,
                "list site",
                m => sites.Add(this.ToSite(m)),
                m => { throw new Exception($"There was an error getting IIS Express sites: '{m}'"); });

            return sites;
        }

        public void AddSite(int id, string name, string bindings, string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(bindings))
            {
                throw new ArgumentNullException(nameof(bindings));
            }

            if (string.IsNullOrWhiteSpace(physicalPath))
            {
                throw new ArgumentNullException(nameof(physicalPath));
            }

            var addSiteParameters = new Dictionary<string, string>
            {
                { "id", id.ToString() },
                { "name", name },
                { "bindings", bindings },
                { "physicalPath", physicalPath }
            };

            string lastError = null;

            var result = this.processService.Launch(
                this.appCmdPath,
                $"add site {string.Join(" ", addSiteParameters.Select(p => $"/{p.Key}:{p.Value}"))}",
                message =>
                {
                    if (message.StartsWith("ERROR"))
                    {
                        lastError = message;
                    }
                },
                message => lastError = message);

            if (!result.IsSuccess)
            {
                throw new Exception(
                    $"There was an error adding an IIS Express site: '{lastError}', exit code: '{result.ExitCode}");
            }
        }

        public void DeleteSite(string siteName)
        {
            if (string.IsNullOrWhiteSpace(siteName))
            {
                throw new ArgumentNullException(nameof(siteName));
            }

            string lastError = null;

            var result = this.processService.Launch(
                this.appCmdPath,
                $"delete site \"{siteName}\"",
                message =>
                {
                    if (message.StartsWith("ERROR"))
                    {
                        lastError = message;
                    }
                },
                message => lastError = message);

            if (!result.IsSuccess)
            {
                throw new Exception(
                    $"There was an error deleting an IIS Express site: '{lastError}', exit code: '{result.ExitCode}");
            }
        }

        private IisExpressSite ToSite(string site)
        {
            if (string.IsNullOrWhiteSpace(site))
            {
                throw new ArgumentNullException(site);
            }

            var siteParts = site.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (siteParts.Length != 3)
            {
                throw new InvalidOperationException(
                    "Expecting appcmd.exe to return sites of 3 parts, e.g. SITE \"foo\" (...)");
            }

            var siteName = siteParts[1];
            var parenthesisPart = siteParts[2];
            var parenthesisProperties = parenthesisPart.Substring(1, parenthesisPart.Length - 2)
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            var propertiesDictionary = parenthesisProperties.ToDictionary(
                p => p.Substring(0, p.IndexOf(":", StringComparison.Ordinal)),
                p => p.Substring(p.IndexOf(":", StringComparison.Ordinal) + 1));

            return new IisExpressSite(
                id: Convert.ToInt32(propertiesDictionary["id"]),
                name: siteName.Substring(1, siteName.Length - 2),
                bindings: propertiesDictionary["bindings"],
                state: propertiesDictionary["state"]);
        }
    }
}
