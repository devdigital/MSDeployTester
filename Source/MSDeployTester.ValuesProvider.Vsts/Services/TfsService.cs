namespace MSDeployTester.ValuesProvider.Vsts.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.Services.Client;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
    using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Contracts;
    using Microsoft.VisualStudio.Services.WebApi;

    using MSDeployTester.Core.Extensions;

    internal class TfsService
    {
        private readonly Uri tfsUri;

        public TfsService(string tfsUri)
        {
            if (string.IsNullOrWhiteSpace(tfsUri))
            {
                throw new ArgumentNullException(nameof(tfsUri));
            }

            this.tfsUri = new Uri(tfsUri);
        }

        public async Task<IDictionary<string, string>> GetReleaseEnvironmentVariables(
            string projectId,
            string releaseDefinitionName,
            string environmentName)
        {
            if (string.IsNullOrWhiteSpace(projectId))
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (string.IsNullOrWhiteSpace(releaseDefinitionName))
            {
                throw new ArgumentNullException(nameof(releaseDefinitionName));
            }

            if (string.IsNullOrWhiteSpace(environmentName))
            {
                throw new ArgumentNullException(nameof(environmentName));
            }
            
            var credentials = new VssClientCredentials
            {
                Storage = new VssClientCredentialStorage()
            };

            var connection = new VssConnection(this.tfsUri, credentials);

            var client = connection.GetClient<ReleaseHttpClient>();

            var releaseDefinitions = await client.GetReleaseDefinitionsAsync(
                projectId, 
                searchText: null, 
                expand: ReleaseDefinitionExpands.Environments | ReleaseDefinitionExpands.Variables);

            var releaseDefinition = releaseDefinitions.FirstOrDefault(r => r.Name == releaseDefinitionName);
            if (releaseDefinition == null)
            {
                throw new InvalidOperationException(
                    $"Release definition '{releaseDefinitionName}' not found in project '{projectId}'");
            }

            var environment = releaseDefinition.Environments.FirstOrDefault(e => e.Name == environmentName);
            if (environment == null)
            {
                throw new InvalidOperationException(
                    $"Release environment '{environmentName}' not found in release '{releaseDefinitionName}' in project '{projectId}'");
            }

            var releaseVariables = releaseDefinition.Variables.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Value);

            var environmentVariables = environment.Variables.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Value);

            return releaseVariables.MergeLeft(environmentVariables);
        }
    }
}