using System.Text;

namespace NSL.HTMLProcessor
{
    internal delegate void ModifyHtmlAttributeDelegate(HtmlAttribute node, int? oldPos, int oldSize);

    public class HtmlAttribute
    {
        private string? _value;
        private string key;
        private HtmlAttributeQuoteType hasQuote = HtmlAttributeQuoteType.None;

        public int? Position { get => position; internal set { var pos = position; position = value; OnModified(this, pos, size); } }

        public int Size { get => size; internal set { var s = size; size = value; OnModified(this, Position, s); } }

        public HtmlBaseNode Parent { get; internal set; }

        public string Key { get => key; set { key = value; modify(); } }

        public string? Value { get => _value; set { _value = value; modify(); } }

        public HtmlAttributeQuoteType HasQuote { get => hasQuote; set { hasQuote = value; modify(); } }

        public int? DocumentStartOffset => (Parent?.DocumentOuterStartOffset ?? 0) + Position;

        public int? DocumentValueStartOffset => DocumentStartOffset + Key.Length + 2;

        public int? DocumentEndOffset => DocumentStartOffset + Size;

        internal event ModifyHtmlAttributeDelegate OnModified = (i, p, s) => { };
        private int size;
        private int? position;

        private void modify()
        {
            Size = BuildHtml().Length;
        }

        internal void _SetPosition(int pos)
            => position = pos;

        public string BuildHtml(bool saveSourceOffsets = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Key);

            if (Value != null)
            {
                sb.Append('=');

                if (HasQuote == HtmlAttributeQuoteType.Single)
                    sb.Append($"'{Value}'");
                else if(HasQuote == HtmlAttributeQuoteType.Double)
                    sb.Append($"\"{Value}\"");
                else
                    sb.Append(Value);
            }

            return sb.ToString();
        }

        public void Remove()
        {
            Parent.RemoveAttribute(this);
        }
    }

    public enum HtmlAttributeQuoteType
    {
        None,
        Single,
        Double
    }
}
