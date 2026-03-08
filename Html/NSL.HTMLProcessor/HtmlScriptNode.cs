using HtmlDocumentDev;

namespace NSL.HTMLProcessor
{
    public class HtmlScriptNode : HtmlBaseNode
    {
        public override bool CanHaveAttributes => true;

        public override bool AllowSpecialCharacters => true;
    }
}
