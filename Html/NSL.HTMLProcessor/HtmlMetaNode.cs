using HtmlDocumentDev;

namespace NSL.HTMLProcessor
{
    public class HtmlMetaNode : HtmlBaseNode
    {
        public override bool HasBody { get => false; set { } }
    }
}
