using HtmlDocumentDev;
using System.Text;

namespace NSL.HTMLProcessor
{
    public class HtmlDoctypeNode : HtmlBaseNode
    {
        public override bool HasBody { get => false; set { } }

        public override string BuildHtml(bool saveSourceOffsets = true)
        {
            var sb = new StringBuilder();

            sb.Append('<');

            sb.Append(NodeName);

            sb.Append(" ");

            sb.Append(string.Join(' ', Attributes.OrderBy(x => x.Position).Select(x => x.BuildHtml())));

            sb.Append('>');

            return sb.ToString();
        }
    }
}
