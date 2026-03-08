using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class NodeModificationPositionTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void AddChild_BeforeAfter_Prepend_UpdatePositionsAndSizes()
        {
            var doc = Parse("<ul id='list'><li id='a'>A</li><li id='b'>B</li></ul>");
            var ul = doc.QuerySelector<HtmlBaseNode>("#list");
            Assert.That(ul, Is.Not.Null);

            var originalChildren = ul.ChildNodes.OrderBy(x => x.Position).ToList();
            Assert.That(originalChildren.Count, Is.EqualTo(2));

            var injected = HtmlParser.Parse<HtmlDocumentNode>("<li id='x'>X</li>").ChildNodes.First();

            // insert after first (A)
            ul.AddChildNodeAfter(injected, originalChildren[0]);

            // positions monotonic and contiguous
            int offset = 0;
            foreach (var ch in ul.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(offset));
                Assert.That(ch.Size.TotalLength, Is.GreaterThan(0));
                offset += ch.Size.TotalLength;
            }

            // prepend a new node
            var prep = HtmlParser.Parse<HtmlDocumentNode>("<li id='p'>P</li>").ChildNodes.First();
            ul.PrependChildNode(prep);

            // check prepend placed at position 0 and others moved
            var ordered = ul.ChildNodes.OrderBy(x => x.Position).ToList();
            Assert.That(ordered.First().Id, Is.EqualTo("p"));
            offset = 0;
            foreach (var ch in ordered)
            {
                Assert.That(ch.Position, Is.EqualTo(offset));
                offset += ch.Size.TotalLength;
            }

            // inner html equals concat of children outer html
            var concat = string.Concat(ul.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)));
            Assert.That(ul.GetInnerHtml(false), Is.EqualTo(concat));
        }

        [Test]
        public void ReplaceChildNode_ReplacesAndKeepsPositionsConsistent()
        {
            var doc = Parse("<ul id='list'><li id='a'>A</li><li id='b'>B</li><li id='c'>C</li></ul>");
            var ul = doc.QuerySelector<HtmlBaseNode>("#list");

            var toReplace = ul.ChildNodes.First(n => n.Id == "b");
            var replacement = HtmlParser.Parse<HtmlDocumentNode>("<li id='y'>Y</li>").ChildNodes.First();

            var posBefore = ul.ChildNodes.OrderBy(x => x.Position).Select(x => x.Position).ToArray();

            ul.ReplaceChildNode(replacement, toReplace);

            var ordered = ul.ChildNodes.OrderBy(x => x.Position).ToList();

            // replacement present and other nodes still form contiguous positions
            Assert.That(ordered.Any(n => n.Id == "y"));
            int offset = 0;
            foreach (var ch in ordered)
            {
                Assert.That(ch.Position, Is.EqualTo(offset));
                offset += ch.Size.TotalLength;
            }

            // ensure document offsets are monotonic
            var docOffsets = ordered.Select(n => n.DocumentOuterStartOffset).ToList();
            for (int i = 0; i < docOffsets.Count - 1; i++)
                Assert.That(docOffsets[i] <= docOffsets[i + 1]);
        }

        [Test]
        public void RemoveChildNode_RecalculatesPositionsAndParentSize()
        {
            var doc = Parse("<ul id='list'><li id='a'>A</li><li id='b'>B</li><li id='c'>C</li></ul>");
            var ul = doc.QuerySelector<HtmlBaseNode>("#list");
            Assert.That(ul, Is.Not.Null);

            // initial contiguous positions
            int pos = 0;
            foreach (var ch in ul.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(pos));
                pos += ch.Size.TotalLength;
            }

            // remove middle node
            var toRemove = ul.ChildNodes.First(n => n.Id == "b");
            ul.RemoveChildNode(toRemove);

            // remaining children positions recomputed and contiguous
            pos = 0;
            foreach (var ch in ul.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(pos));
                pos += ch.Size.TotalLength;
            }

            // parent inner content length equals concat of children outer lengths
            var concat = string.Concat(ul.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)));
            Assert.That(ul.Size.InnerContentLength, Is.EqualTo(concat.Length));
        }
    }
}