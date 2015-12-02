namespace UXLR
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using UXLRCore;

    public static class XamlKeys
    {
        public static string[] DefaultIgnoreProperties = { "Text" };

        public static Dictionary<string, string> ParseResource(string file)
        {
            var localisationResourceFile = new FileInfo(file);
            if (!localisationResourceFile.Exists)
            {
                throw new UXLRException($"Resource file {file} not found", ExitCode.ResourceFileNotFound);
            }

            var xml = "";
            using (var reader = localisationResourceFile.OpenText())
            {
                xml = reader.ReadToEnd();
            }

            var xDoc = XDocument.Parse(xml);
            var dataNodes = xDoc.Descendants("data");

            return dataNodes.ToDictionary(_ => _.Attribute("name").Value, _ => _.Descendants("value").First().Value);
        }

        public static IEnumerable<SearchContent> Process(string[] ignoreProperties, params string[] localisationResources)
        {
            if (ignoreProperties == null)
            {
                ignoreProperties = DefaultIgnoreProperties;
            }

            var uids = new List<string>();
            foreach (var localisationResource in localisationResources)
            {
                var localisation = ParseResource(localisationResource);

                foreach (var dataNode in localisation)
                {
                    var name = dataNode.Key;
                    var dot = name.IndexOf('.');
                    if (dot <= 0)
                    {
                        continue;
                    }

                    var uid = name.Substring(0, dot);
                    if (uids.Contains(uid))
                    {
                        continue;
                    }

                    var property = name.Substring(dot + 1);
                    if (ignoreProperties.Contains(property))
                    {
                        continue;
                    }

                    uids.Add(uid);
                }
            }

            return uids.Select(_ => new SearchContent(_, SearchContentType.ResourceID));
        }

        public static IEnumerable<SearchContent> SearchLocalisationValues(string[] localisationFiles, IEnumerable<SearchContent> searchPieces)
        {
            var input = new List<SearchContent>(searchPieces);
            foreach (var resource in localisationFiles)
            {
                var localisation = ParseResource(resource);
                foreach (var item in localisation)
                {
                    var filesToSearchFor = input.Where(_ => _.ContentType == SearchContentType.Image).ToArray();
                    foreach (var image in filesToSearchFor)
                    {
                        if (item.Value.IndexOf(image.Query(), StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            input.Remove(image);
                        }
                    }
                }
            }

            return input;
        }
    }
}