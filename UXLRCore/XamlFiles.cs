namespace UXLRCore
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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

        public static void Search(IEnumerable<FileInfo> xamlFiles, IEnumerable<SearchContent> searchQueries)
        {
            foreach (var xamlFile in xamlFiles)
            {
                var content = "";
                using (var xamlContent = xamlFile.OpenText())
                {
                    content = xamlContent.ReadToEnd();
                }

                foreach (var searchQuery in searchQueries)
                {
                    searchQuery.Found(content.Contains(searchQuery.Query()), xamlFile.FullName);
                }
            }
        }

        private static void FindXamlFiles(DirectoryInfo directory, List<FileInfo> xamlFiles)
        {
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