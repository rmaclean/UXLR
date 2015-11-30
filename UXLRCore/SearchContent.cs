namespace UXLRCore
{
    using System;

    public enum SearchContentType
    {
        StyleKey,
        BasedOnStyle,
        ResourceID
    }

    public class SearchContent
    {
        public SearchContent(string raw, SearchContentType contentType)
        {
            Raw = raw;
            ContentType = contentType;
        }

        public bool ContentFound { get; private set; }

        public SearchContentType ContentType { get; }

        public string FileName { get; private set; }

        public string Raw { get; }

        public string Query()
        {
            switch (ContentType)
            {
                case SearchContentType.StyleKey:
                    return $"Style=\"{{StaticResource {Raw}}}\"";

                case SearchContentType.BasedOnStyle:
                    return $"BasedOn=\"{{StaticResource {Raw}}}\"";

                case SearchContentType.ResourceID:
                    return $"x:uid=\"{Raw}\"";
            }

            throw new Exception();
        }

        internal void Found(bool found, string fileName)
        {
            ContentFound = found;
            FileName = fileName;
        }
    }
}