namespace MSDeployTester.Core.Services
{
    using System;
    using System.IO;

    using Newtonsoft.Json;

    public class SettingsService
    {
        public TSetting Parse<TSetting>(string settingsFile)
        {
            if (string.IsNullOrWhiteSpace(settingsFile))
            {
                throw new ArgumentNullException(nameof(settingsFile));
            }

            var settingsPathContent = File.ReadAllText(settingsFile);
            return JsonConvert.DeserializeObject<TSetting>(settingsPathContent);
        }
    }
}