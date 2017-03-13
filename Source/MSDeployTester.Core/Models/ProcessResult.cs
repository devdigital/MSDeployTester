namespace MSDeployTester.Core.Models
{
    public class ProcessResult
    {
        private ProcessResult(bool isSuccess, int exitCode)
        {
            this.IsSuccess = isSuccess;
            this.ExitCode = exitCode;
        }

        public static ProcessResult Success()
        {
            return new ProcessResult(isSuccess: true, exitCode: 0);
        }

        public static ProcessResult Failure(int exitCode)
        {
            return new ProcessResult(isSuccess: false, exitCode: exitCode);
        }

        public bool IsSuccess { get; }

        public int ExitCode { get; }
    }
}