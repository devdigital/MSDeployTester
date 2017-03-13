namespace MSDeployTester.Console.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using MSDeployTester.Core.Logging;

    internal class ConsoleLogger : ILogger
    {
        private readonly List<string> errors;

        public ConsoleLogger()
        {
            this.errors = new List<string>();
        }

        public bool HasErrors => this.errors.Any();

        public void DisplayErrors()
        {
            foreach (var error in this.errors)
            {
                this.DisplayError(error);
            }
        }

        public void Information(string message)
        {
            Console.WriteLine(message);
        }

        public void Error(string message)
        {
            this.errors.Add(message);
            this.DisplayError(message);
        }

        private void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}