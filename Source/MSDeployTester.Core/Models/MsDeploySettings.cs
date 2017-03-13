namespace MSDeployTester.Core.Models
{
    using System;

    public class MsDeploySettings
    {
        public MsDeploySettings(string buildPath, string appCmdPath)
        {
            if (string.IsNullOrWhiteSpace(buildPath))
            {
                throw new ArgumentNullException(nameof(buildPath));
            }

            if (string.IsNullOrWhiteSpace(appCmdPath))
            {
                throw new ArgumentNullException(nameof(appCmdPath));
            }

            this.BuildPath = buildPath;
            this.AppCmdPath = appCmdPath;
        }

        public string BuildPath { get; }

        public string AppCmdPath { get; }
    }
}