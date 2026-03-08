using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class NodeSizeAndPositionTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void TagForms_ParsePositionsAndSizes()
        {
            var html = "<root>" +
                       "<a/>" +
                       "<b></b>" +
                       "<c attr></c>" +
                       "<d attr='v'/>" +
                       "<e attr=\"v\"></e>" +
                       "<f />" +
                       "</root>";
            var doc = Parse(html);

            var root = doc.QuerySelector<HtmlBaseNode>("root");
            Assert.That(root, Is.Not.Null);

            var builtInner = root.GetInnerHtml(false);
            var concatChildren = string.Concat(root.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)));
            Assert.That(builtInner, Is.EqualTo(concatChildren));

            int offset = 0;
            foreach (var child in root.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(child.Position, Is.EqualTo(offset));
                Assert.That(child.Size.TotalLength, Is.EqualTo(child.GetOuterHtml(false).Length));
                offset += child.Size.TotalLength;
            }

            Assert.That(root.Size.InnerContentLength, Is.EqualTo(builtInner.Length));
        }

        [Test]
        public void Positions_MonotonicAndContiguous_AfterModifications()
        {
            var doc = Parse(@"<ul id='list'><li id='a'></li><li id='b'></li><li id='c'></li></ul>");
            var ul = doc.QuerySelector<HtmlBaseNode>("#list");
            Assert.That(ul, Is.Not.Null);

            var injected = HtmlParser.Parse<HtmlDocumentNode>("<li id='x'>X</li>").ChildNodes.First();
            var first = ul.ChildNodes.OrderBy(x => x.Position).First();

            ul.AddChildNodeBefore(injected, first);

            // check ordering and monotonic offsets
            int pos = 0;
            foreach (var ch in ul.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(pos));
                pos += ch.Size.TotalLength;
            }

            // remove and ensure positions recalc
            var toRemove = ul.ChildNodes.First(n => n.Id == "b");
            ul.RemoveChildNode(toRemove);
            pos = 0;
            foreach (var ch in ul.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(pos));
                pos += ch.Size.TotalLength;
            }
        }

        [Test]
        public void ParentInnerHtml_Equals_ConcatOfChildrenOuterHtml()
        {
            var doc = Parse("<div id='wrap'><span id='s1'></span><span id='s2'/><p id='p1'><em></em></p></div>");
            var wrap = doc.QuerySelector<HtmlBaseNode>("#wrap");
            Assert.That(wrap.GetInnerHtml(false), Is.EqualTo(string.Concat(wrap.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)))));
        }
    }
}