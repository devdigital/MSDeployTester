namespace MSDeployTester.Console.Options
{
    using CommandLine;

    public class CommonOptions
    {
        [Option('s', "solution", Required = true, HelpText = "The path to your solution (.sln) file")]
        public string SolutionFile { get; set; }

        [Option('p', "platform", Required = true, HelpText = "The platform to build, e.g. \"Any CPU\"")]
        public string Platform { get; set; }

        [Option('c', "configuration", Required = true, HelpText = "The configuration to build, e.g. \"Release\"")]
        public string Configuration { get; set; }

        [Option('x', "excludeBuild", Required = false, Default = false, HelpText = "Flag if to exclude the build of the solution before copying packages to output folder (default false)")]
        public bool ExcludeBuild { get; set; }

        [Option("parameteriseConnectionStrings", Required = false, Default = false, HelpText = "Flag if to automatically parameterise the connection strings")]
        public bool ParameteriseConnectionStrings { get; set; }
    }
}