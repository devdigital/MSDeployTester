namespace MSDeployTester.Core.Models
{
    using System;

    public class VsProject
    {
        public VsProject(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            this.Path = path;
            this.Folder = System.IO.Path.GetDirectoryName(this.Path);
            this.ProjectFile = System.IO.Path.GetFileName(this.Path);
            this.ProjectName = System.IO.Path.GetFileNameWithoutExtension(this.ProjectFile);
        }

        public string Path { get; }

        public string Folder { get; }

        public string ProjectFile { get; }

        public string ProjectName { get; }
    }
}