namespace UXLR
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using UXLRCore;

    public static class XamlKeys
    {
        public static IEnumerable<SearchContent> Process(string[] ignoreProperties, params string[] localisationResources)
        {
            var uids = new List<string>();
            foreach (var localisationResource in localisationResources)
            {
                var localisationResourceFile = new FileInfo(localisationResource);
                if (!localisationResourceFile.Exists)
                {
                    throw new UXLRException($"Resource file {localisationResource} not found", ExitCode.ResourceFileNotFound);
                }

                var xml = "";
                using (var reader = localisationResourceFile.OpenText())
                {
                    xml = reader.ReadToEnd();
                }

                var xDoc = XDocument.Parse(xml);
                var dataNodes = xDoc.Descendants("data");
                foreach (var dataNode in dataNodes)
                {
                    var name = dataNode.Attribute("name").Value;
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
    }
}