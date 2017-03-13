namespace MSDeployTester.Console.Options
{
    using CommandLine;

    [Verb("deploy", HelpText = "Build and publish all web projects in a solution")]
    public class DeployOptions : CommonOptions
    {
        [Option("valueProviderId", Required = true, HelpText = "The id of a SetParameters value provider")]
        public string ValueProviderId { get; set; }

        [Option("valueProviderOptions", Required = true, HelpText = "The options (JSON format) to pass to the value provider")]
        public string ValueProviderOptions { get; set; }        
    }
}