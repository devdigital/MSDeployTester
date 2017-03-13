namespace MSDeployTester.Console
{
    using System;

    using CommandLine;

    using MSDeployTester.Console.Logging;
    using MSDeployTester.Console.Options;
    using MSDeployTester.Console.Services;

    public class Program
    {
        public static int Main(string[] args)
        {            
            var testerService = new TesterService(new ConsoleLogger());

            return
                Parser.Default.ParseArguments<PackageOptions, DeployOptions>(args)
                    .MapResult(
                        (PackageOptions options) => Run(() => testerService.Package(options)),
                        (DeployOptions options) => Run(() => testerService.Deploy(options)),
                        errors => 1);                       
        }

        private static int Run(Action action)
        {
            try
            {
                action();
                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
}
