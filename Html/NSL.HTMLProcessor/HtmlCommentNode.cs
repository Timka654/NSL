using HtmlDocumentDev;
using System.Text;

namespace NSL.HTMLProcessor
{
    public class HtmlCommentNode : HtmlBaseNode
    {
        public override bool HasBody { get => false; set { } }

        public override bool CanHaveAttributes => false;

        public override string? CloseTag => "-->";

        public override bool AllowSpecialCharacters => true;

        public override void AddAttribute(HtmlAttribute currentAttribute)
        {
            throw new NotImplementedException();
        }

        public override string BuildHtml(bool saveSourceOffsets = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"<!--{InnerHtmlContent}-->");

            return sb.ToString();
        }
    }
}
