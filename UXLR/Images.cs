namespace UXLR
{
    using UXLRCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System;
    using System.Text.RegularExpressions;
    public static class Images
    {
        public static string[] DefaultExtensions = {
            ".png",".jpg",".jpeg",".bmp"
        };

        private static readonly Regex ScaleRegEx = new Regex("(?<prefix>.+\\.)(?<scale>scale-\\d+\\.)(?<ext>.+)");

        public static IEnumerable<SearchContent> Process(string[] extensions, bool recurseFolders, params string[] imageFolders)
        {
            if (extensions == null)
            {
                extensions = DefaultExtensions;
            }

            var images = new List<string>();
            foreach (var folder in imageFolders)
            {
                var directory = new DirectoryInfo(folder);
                if (!directory.Exists)
                {
                    throw new UXLRException($"Image folder now found {folder}", ExitCode.ImageFolderNotFound);
                }

                FindImages(images, directory, recurseFolders, extensions);
            }

            return images.Select(_ => new SearchContent(_, SearchContentType.Image));
        }

        private static void FindImages(List<string> images, DirectoryInfo directory, bool recurseFolders, string[] extensions)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                if (!extensions.Contains(file.Extension))
                {
                    continue;
                }

                var name = file.Name;
                name = ScaleRegEx.Replace(name, "${prefix}${ext}");

                if (images.Contains(name))
                {
                    continue;
                }

                images.Add(name);
            }

            if (recurseFolders)
            {
                foreach (var folder in directory.EnumerateDirectories())
                {
                    FindImages(images, folder, recurseFolders, extensions);
                }
            }
        }
    }
}