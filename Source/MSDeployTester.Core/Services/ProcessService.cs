namespace MSDeployTester.Core.Services
{
    using System;
    using System.Diagnostics;

    using MSDeployTester.Core.Models;

    public class ProcessService
    {
        public ProcessResult Launch(
            string filePath, 
            string arguments, 
            Action<string> runningInformationHandler = null,
            Action<string> runningErrorHandler = null)
        {
            runningInformationHandler = runningInformationHandler ?? (message => { });
            runningErrorHandler = runningErrorHandler ?? (message => { });
            
            var process = new Process
            {
                StartInfo =
                {
                    FileName = filePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            process.OutputDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data))
                {
                    return;
                }

                runningInformationHandler(args.Data);
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(args.Data))
                {
                    return;
                }

                runningErrorHandler(args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            return process.ExitCode == 0 
                ? ProcessResult.Success() 
                : ProcessResult.Failure(process.ExitCode);
        }
    }
}