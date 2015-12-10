namespace UXLR
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using UXLRCore;

    internal class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("UXLR");
            Console.WriteLine(" \"Clean up your room\" - your Mom");
            var config = new RunConfig();
            for (var count = 0; count < args.Length - 1; count++)
            {
                if (args[count].Equals("/R", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.RootFolder = args[count];
                    continue;
                }

                if (args[count].Equals("/B", StringComparison.OrdinalIgnoreCase))
                {
                    config.Beep = true;
                    continue;
                }

                if (args[count].Equals("/L", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.LocalisationResources = args[count].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    config.CleanLocalisations = true;
                    continue;
                }

                if (args[count].Equals("/S", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.StyleResources = args[count].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    config.CleanStyles = true;
                    continue;
                }

                if (args[count].Equals("/LI", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.LocalisationPropertiesToIgnore = args[count].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    continue;
                }

                if (args[count].Equals("/I", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.ImageFolders = args[count].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    config.CleanImages = true;
                    continue;
                }

                if (args[count].Equals("/IE", StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                    config.ImageExtensions = args[count].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    continue;
                }

                if (args[count].Equals("/IR", StringComparison.OrdinalIgnoreCase))
                {
                    config.ImageRecurse = true;
                    continue;
                }

                if (args[count].Equals("/IIR", StringComparison.OrdinalIgnoreCase))
                {
                    config.ImageSearchInResources = true;
                    continue;
                }
            }

            if (string.IsNullOrWhiteSpace(config.RootFolder) || (!config.CleanLocalisations && !config.CleanStyles && !config.CleanImages))
            {
                ShowHelp();
                return (int)ExitCode.ShowHelp;
            }

            if (config.CleanLocalisations && (config.LocalisationResources == null || config.LocalisationResources.Length == 0))
            {
                ShowHelp();
                return (int)ExitCode.ShowHelp;
            }

            if (config.CleanStyles && (config.StyleResources == null || config.StyleResources.Length == 0))
            {
                ShowHelp();
                return (int)ExitCode.ShowHelp;
            }

            if (config.CleanImages && (config.ImageFolders == null || config.ImageFolders.Length == 0))
            {
                ShowHelp();
                return (int)ExitCode.ShowHelp;
            }

            if (config.ImageSearchInResources && (!config.CleanImages || !config.CleanLocalisations))
            {
                ShowHelp();
                return (int)ExitCode.ShowHelp;
            }

            try
            {
                return Run(config);
            }
            catch (UXLRException ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return (int)ex.ExitCode;
            }
        }

        private static string CommaSeperated(string[] input) => input.Aggregate((curr, next) => curr + (curr.Length > 0 ? ", " : "") + next);

        private static void PrintResult(IEnumerable<SearchContent> searchPieces, SearchContentType contentType, string title, string nothingFoundMessage)
        {
            var resources = searchPieces.Where(_ => contentType.HasFlag(_.ContentType));
            if (resources.Any())
            {
                Console.WriteLine(title);
                foreach (var missing in resources.OrderBy(_ => _.Raw))
                {
                    Console.WriteLine("\t" + missing.Raw);
                }
            }
            else
            {
                Console.WriteLine(nothingFoundMessage);
            }
        }

        private static int Run(RunConfig config)
        {
            IEnumerable<SearchContent> searchPieces = new Collection<SearchContent>();
            if (config.CleanLocalisations)
            {
                var keys = XamlKeys.Process(config.LocalisationPropertiesToIgnore, config.LocalisationResources);
                searchPieces = searchPieces.Concat(keys);
            }

            if (config.CleanStyles)
            {
                var styles = XamlStyles.Process(config.StyleResources);
                searchPieces = searchPieces.Concat(styles);
            }

            if (config.CleanImages)
            {
                var images = Images.Process(config.ImageExtensions, config.ImageRecurse, config.ImageFolders);
                searchPieces = searchPieces.Concat(images);
            }

            var xamlFiles = XamlFiles.FindXamlFiles(config.RootFolder);
            searchPieces = XamlFiles.Search(xamlFiles, searchPieces);

            if (config.ImageSearchInResources && config.CleanImages && config.CleanLocalisations)
            {
                searchPieces = XamlKeys.SearchLocalisationValues(config.LocalisationResources, searchPieces);
            }

            if (config.CleanLocalisations)
            {
                PrintResult(searchPieces, SearchContentType.ResourceID, "Resources with not match in XAML files", "No used resources :)");
            }

            if (config.CleanStyles)
            {
                PrintResult(searchPieces, SearchContentType.BasedOnStyle | SearchContentType.StyleKey, "Styles with no match in XAML files", "No used styles :)");
            }

            if (config.CleanImages)
            {
                PrintResult(searchPieces, SearchContentType.Image, "Images with no match in XAML files", "No used images :)");
            }

            if (config.Beep)
            {
                Console.Beep();
            }

            return (int)ExitCode.Success;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Scans your XAML files for mess to help you clean it up.");
            Console.WriteLine();
            Console.WriteLine("UniversalCleaner.exe /R rootDirectory [/L localisationResources] [/S styleResources] [/LI properties] [/I imageFolders] [/IE imageExtensions] [/B]");
            Console.WriteLine();
            var messages = new Dictionary<string, string>
            {
                { "/R rootDirectory", "Path to the root folder to search from." },
                { "/L localisationResources", "Enables finding of unused localisation keys. localisationResources is a comma seperated list of resw files used for localisation." },
                { "/LI", $"When used with /L, provides a comma seperated list of properties to assume are used elsewhere and thus ignored. Default properties are: {CommaSeperated(XamlKeys.DefaultIgnoreProperties)}" },
                { "/S styleResources", "Enables finding of unused files. styleResources is a comma seperated list of xaml files used for styles." },
                { "/I", "Enables searching for unused images. imageFolders is a comma seperate list of folders to look in." },
                { "/IE", $"When used with /I, provides a comma seperated list of file extensions to consider an image. Default extensions are: {CommaSeperated(Images.DefaultExtensions)}" },
                { "/IR", "When used with /I, instructs the tool to check in the image folder and all sub-folders." },
                { "/IIR", "When used with /I AND /L, instructs the tool to check for image use in location resources as well as XAML." },
                { "/B", "Beep when done." }
            };

            WriteAlignedMessages(messages);

            Console.WriteLine("  Examples:");
            Console.WriteLine();
            var examples = new Dictionary<string, string>
            {
                {"\"/R .\\ /L .\\en\\resource.resw,.\\\fr\\resource.resw\"","Will find all resource keys in the two resw files provided and search in all xaml files in current folder and any subfolders." }
            };

            WriteAlignedMessages(examples, 4);
        }

        private static void WriteAlignedMessages(Dictionary<string, string> messages, int keyOffset = 2)
        {
            var valueOffSet = messages.OrderByDescending(_ => _.Key.Length).First().Key.Length + 2 + keyOffset;
            var maxMessageLength = Console.WindowWidth - valueOffSet - 2;
            foreach (var message in messages)
            {
                Console.CursorLeft = 0;
                Console.Write(message.Key.PadLeft(keyOffset + message.Key.Length, ' '));
                Console.CursorLeft = valueOffSet;

                if (message.Value.Length > maxMessageLength)
                {
                    var print = "";
                    var words = message.Value.Split(' ');
                    for (var counter = 0; counter < words.Length; counter++)
                    {
                        if (print.Length + 1 + words[counter].Length > maxMessageLength)
                        {
                            Console.CursorLeft = valueOffSet;
                            Console.WriteLine(print);
                            print = "";
                        }

                        print += words[counter] + " ";
                    }

                    Console.CursorLeft = valueOffSet;
                    Console.WriteLine(print);
                }
                else
                {
                    Console.WriteLine(message.Value);
                }
            }

            Console.CursorLeft = 0;
        }

        private class RunConfig
        {
            public bool Beep { get; set; }

            public bool CleanImages { get; internal set; }

            public bool CleanLocalisations { get; set; }

            public bool CleanStyles { get; set; }

            public string[] ImageExtensions { get; internal set; }

            public string[] ImageFolders { get; internal set; }

            public bool ImageRecurse { get; internal set; }

            public bool ImageSearchInResources { get; internal set; }

            public string[] LocalisationPropertiesToIgnore { get; set; }

            public string[] LocalisationResources { get; set; }

            public string RootFolder { get; set; }

            public string[] StyleResources { get; set; }
        }
    }
}