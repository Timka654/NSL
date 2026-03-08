using HtmlDocumentDev;

namespace NSL.HTMLProcessor
{
    public class HtmlTableColNode : HtmlBaseNode
    { 
        public override bool HasBody { get => false; set { } }
    }

    public class HtmlImgNode : HtmlBaseNode
    {
        public override bool HasBody { get => false; set { } }
    }
}
