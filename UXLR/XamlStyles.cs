namespace UXLR
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using UXLRCore;
    using System.Linq;

    public static class XamlStyles
    {
        public static IEnumerable<SearchContent> Process(params string[] styleFiles)
        {
            var styles = new List<string>();
            XNamespace rootNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
            foreach (var stylePath in styleFiles)
            {
                var styleFile = new FileInfo(stylePath);
                if (!styleFile.Exists)
                {
                    throw new UXLRException($"Cannot find style file {stylePath}", ExitCode.StyleFileNotFound);
                }

                var xml = "";
                using (var reader = styleFile.OpenText())
                {
                    xml = reader.ReadToEnd();
                }

                var xDoc = XDocument.Parse(xml);
                var styleNodes = xDoc.Descendants(rootNamespace + "Style");
                foreach (var styleNode in styleNodes)
                {
                    if (!styleNode.Attributes(xNamespace + "Key").Any())
                    {
                        continue;
                    }

                    var name = styleNode.Attribute(xNamespace + "Key").Value;
                    if (styles.Contains(name))
                    {
                        continue;
                    }

                    styles.Add(name);
                }
            }

            return styles.Select(_ => new SearchContent(_, SearchContentType.BasedOnStyle))
                    .Concat(styles.Select(_ => new SearchContent(_, SearchContentType.StyleKey)));
        }
    }
}