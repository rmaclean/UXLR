namespace UXLRCore
{
    using System;

    [Flags]
    public enum SearchContentType
    {
        StyleKey = 1,
        BasedOnStyle = 2,
        ResourceID = 4,
        Image = 8
    }

    public class SearchContent
    {
        public SearchContent(string raw, SearchContentType contentType)
        {
            Raw = raw;
            ContentType = contentType;
        }

        public SearchContentType ContentType { get; }

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
                    return $"x:Uid=\"{Raw}\"";

                case SearchContentType.Image:
                    return Raw;
            }

            throw new Exception();
        }
    }
}