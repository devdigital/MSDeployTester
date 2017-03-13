namespace MSDeployTester.Core.Models
{
    using System;

    public class IisExpressSite
    {
        public IisExpressSite(
            int id,
            string name,
            string bindings,
            string state)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(bindings))
            {
                throw new ArgumentNullException(nameof(bindings));
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new ArgumentNullException(nameof(state));
            }

            this.Id = id;
            this.Name = name;
            this.Bindings = bindings;
            this.State = state;
        }
        
        public int Id { get; }

        public string Name { get; }

        public string Bindings { get; }

        public string State { get; }
    }
}