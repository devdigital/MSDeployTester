namespace MSDeployTester.Core.Services
{
    using System;
    using System.IO;

    public class FileService
    {
        // Adapted from https://msdn.microsoft.com/en-us/library/bb762914.aspx
        public void CopyFolder(string sourceFolder, string destinationFolder, bool copySubFolders)
        {
            // Get the subdirectories for the specified directory.
            var folder = new DirectoryInfo(sourceFolder);
            if (!folder.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"Source folder does not exist or could not be found: '{sourceFolder}'");
            }

            var folders = folder.GetDirectories();

            // If the destination folder doesn't exist, create it.
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Get the files in the folder and copy them to the new location.
            var files = folder.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(destinationFolder, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubFolders)
            {
                return;
            }

            foreach (var subFolder in folders)
            {
                var temppath = Path.Combine(destinationFolder, subFolder.Name);
                this.CopyFolder(subFolder.FullName, temppath, copySubFolders: true);
            }
        }

        public void CopyFile(string sourceFile, string destinationFolder)
        {
            // If the destination folder doesn't exist, create it.
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var sourceFileName = Path.GetFileName(sourceFile);
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                throw new InvalidOperationException("The source file name could not be determined");
            }

            File.Copy(
                sourceFile, 
                Path.Combine(destinationFolder, sourceFileName),
                overwrite: true);
        }

        public void DeleteFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                throw new InvalidOperationException($"Attempting to delete a folder '{folder}' that doesn't exist");
            }

            Directory.Delete(folder, recursive: true);
        }

        public void CopyFiles(string sourceFolder, string searchPattern, string destinationFolder)
        {
            // If the destination folder doesn't exist, create it.
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var sourceFiles = Directory.GetFiles(sourceFolder, searchPattern);
            foreach (var sourceFile in sourceFiles)
            {
                if (string.IsNullOrWhiteSpace(sourceFile))
                {
                    continue;
                }

                File.Copy(
                    sourceFile, 
                    Path.Combine(destinationFolder, Path.GetFileName(sourceFile)), 
                    overwrite: true);
            }
        }
    }
}