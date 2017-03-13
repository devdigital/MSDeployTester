namespace MSDeployTester.Core.Dtos
{
    using MSDeployTester.Core.Models;

    public class MsDeploySettingsDto
    {
        public string MsBuildFilePath { get; set; }

        public string AppCmdFilePath { get; set; }
  
        public MsDeploySettings ToSettings()
        {
            return new MsDeploySettings(
                this.MsBuildFilePath,
                this.AppCmdFilePath);
        }
    }
}