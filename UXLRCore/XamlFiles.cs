namespace UXLRCore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class XamlFiles
    {
        public static IEnumerable<FileInfo> FindXamlFiles(string rootDirectory)
        {
            var xamlFilesRoot = new DirectoryInfo(rootDirectory);
            if (!xamlFilesRoot.Exists)
            {
                throw new UXLRException($"Root directory {rootDirectory} not found", ExitCode.RootDirectoryNotFound);
            }

            var xamlFiles = new List<FileInfo>();
            FindXamlFiles(xamlFilesRoot, xamlFiles);
            return xamlFiles;
        }

        public static IEnumerable<SearchContent> Search(IEnumerable<FileInfo> xamlFiles, IEnumerable<SearchContent> searchQueries)
        {
            var input = new List<SearchContent>(searchQueries);
            foreach (var xamlFile in xamlFiles)
            {
                var content = "";
                using (var xamlContent = xamlFile.OpenText())
                {
                    content = xamlContent.ReadToEnd();
                }

                var contentToFind = input.ToArray();
                foreach (var searchQuery in contentToFind)
                {                    
                    if (content.IndexOf(searchQuery.Query(), StringComparison.OrdinalIgnoreCase) > -1)
                    {
                        input.Remove(searchQuery);
                    }
                }
            }

            return input;
        }

        private static void FindXamlFiles(DirectoryInfo directory, List<FileInfo> xamlFiles)
        {
            if (directory.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) || directory.Name.Equals("obj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var file in directory.EnumerateFiles("*.xaml"))
            {
                xamlFiles.Add(file);
            }

            foreach (var dir in directory.EnumerateDirectories())
            {
                FindXamlFiles(dir, xamlFiles);
            }
        }
    }
}