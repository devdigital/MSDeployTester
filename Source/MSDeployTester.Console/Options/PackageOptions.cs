namespace MSDeployTester.Console.Options
{
    using CommandLine;

    [Verb("package", HelpText = "Build and publish all web projects in a solution and deploy using a parameters source")]
    public class PackageOptions : CommonOptions
    {
    }
}