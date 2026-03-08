namespace NSL.HTMLProcessor
{
    public class HtmlParserOptions
    {
        public static HtmlParserOptions Instance { get; } = new HtmlParserOptions();

        public bool SkipMissedOpenedTag { get; set; }

        public bool SkipEmptyTextNodes { get; set; } = true;
    }
}
