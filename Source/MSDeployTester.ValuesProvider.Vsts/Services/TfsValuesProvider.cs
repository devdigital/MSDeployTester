namespace MSDeployTester.ValuesProvider.Vsts.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using MSDeployTester.Core.Models;
    using MSDeployTester.Core.Services;
    using MSDeployTester.ValuesProvider.Vsts.Extensions;
    using MSDeployTester.ValuesProvider.Vsts.Models;

    public class TfsValuesProvider : IValuesProvider
    {
        public string Id => "tfs";

        public IEnumerable<SetParameter> GetValues(string options)
        {
            var tfsOptions = options.ToObject<TfsOptions>();            
            var tfsService = new TfsService(tfsOptions.Uri);

            var environmentVariables = tfsService.GetReleaseEnvironmentVariables(
                tfsOptions.Project,
                tfsOptions.Release,
                tfsOptions.Environment).Result;

            return environmentVariables.Select(v => SetParameter.AsToken(v.Key, v.Value)).ToList();
        }
    }
}