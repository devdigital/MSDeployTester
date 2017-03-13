namespace MSDeployTester.Console.Services
{
    using System.IO;
    using System.Reflection;

    public class ApplicationService
    {
        public string GetApplicationDirectory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(location);
        }
    }
}
