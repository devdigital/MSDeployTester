namespace MSDeployTester.Core.Models
{
    using System;

    public class SetParameter
    {
        private SetParameter(string key, string value, bool isKeyed)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.Key = key;
            this.Value = value;
            this.IsKeyed = isKeyed;
            this.IsToken = !isKeyed;
        }

        public static SetParameter AsKeyed(string key, string value)
        {
            return new SetParameter(key, value, isKeyed: true);
        }

        public static SetParameter AsToken(string key, string value)
        {
            return new SetParameter(key, value, isKeyed: false);
        }

        public string Key { get; }

        public string Value { get; }

        public bool IsKeyed { get; }

        public bool IsToken { get; }
    }
}