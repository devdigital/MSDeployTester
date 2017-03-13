namespace MSDeployTester.Core.Logging
{
    public class NullLogger : ILogger
    {
        public void Information(string message)
        {            
        }

        public void Error(string message)
        {         
        }
    }
}
