namespace MSDeployTester.Core.Logging
{
    public interface ILogger
    {
        void Information(string message);

        void Error(string message);
    }
}