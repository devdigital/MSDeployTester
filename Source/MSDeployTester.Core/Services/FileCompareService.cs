namespace MSDeployTester.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using DiffPlex;
    using DiffPlex.DiffBuilder;
    using DiffPlex.DiffBuilder.Model;

    using MSDeployTester.Core.Models;

    // See https://github.com/mmanela/diffplex
    // and https://github.com/pocketberserker/Diff.Match.Patch for diffing libraries
    public class FileCompareService
    {
        public IEnumerable<Difference> Compare(string fileOnePath, string fileTwoPath)
        {
            if (string.IsNullOrWhiteSpace(fileOnePath))
            {
                throw new ArgumentNullException(nameof(fileOnePath));
            }

            if (string.IsNullOrWhiteSpace(fileTwoPath))
            {
                throw new ArgumentNullException(nameof(fileTwoPath));
            }

            var fileOneContents = File.ReadAllText(fileOnePath);
            var fileTwoContents = File.ReadAllText(fileTwoPath);

            var diffModel = new SideBySideDiffBuilder(new Differ())
                .BuildDiffModel(fileOneContents, fileTwoContents);

            var oldTextDifferences = diffModel.OldText.Lines.Where(l => l.Type != ChangeType.Unchanged);
            var newTextDifferences = diffModel.NewText.Lines.Where(l => l.Type != ChangeType.Unchanged);

            var differences = oldTextDifferences.Concat(newTextDifferences).ToList();

            return Enumerable.Empty<Difference>();
        }
    }
}
