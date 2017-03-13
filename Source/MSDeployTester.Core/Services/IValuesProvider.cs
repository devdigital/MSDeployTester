namespace MSDeployTester.Core.Services
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;

    using MSDeployTester.Core.Models;

    [InheritedExport(typeof(IValuesProvider))]
    public interface IValuesProvider
    {
        string Id { get; }

        IEnumerable<SetParameter> GetValues(string options);
    }
}
