using System;
using System.Text;

namespace NSL.HTMLProcessor
{
    public partial class DocumentNode : HtmlBaseNode
    {
        public HtmlDoctypeNode Doctype { get; set; }

        public override bool HasBody { get => true; set { } }

        public override bool CanHaveAttributes => false;

        public override int? Position { get => 0; internal set { } }

        public override void AddAttribute(HtmlAttribute currentAttribute)
        {
            throw new NotImplementedException();
        }

        public override string BuildHtml(bool saveSourceOffsets)
        {
            var sb = new StringBuilder();

            foreach (var item in ChildNodes)
            {
                sb.Append(item.BuildHtml(saveSourceOffsets));
            }

            return sb.ToString();
        }

        public static DocumentNode Parse(string s)
            => HtmlParser.Parse<DocumentNode>(s);

        public DocumentNode() : base()
        {
            NodeName = string.Empty;
        }
    }
}
