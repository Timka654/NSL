using System;

namespace NSL.HTMLProcessor
{
    public class HtmlTextNode : HtmlBaseNode
    {
        public override bool HasBody { get => false; set { } }

        public override bool CanHaveAttributes => false;
        public HtmlTextNode() : base()
        {
            NodeName = string.Empty;
        }

        public override void AddAttribute(HtmlAttribute currentAttribute)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
            => $"{base.ToString()} - {InnerHtmlContent}";

        public override string BuildHtml(bool saveSourceOffsets = true)
        {
            return base.InnerHtmlContent;
        }
    }
}
