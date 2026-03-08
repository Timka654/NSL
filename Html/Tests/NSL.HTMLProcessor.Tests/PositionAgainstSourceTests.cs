using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class PositionAgainstSourceTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void Nodes_OuterHtml_Matches_SourceSubstrings()
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

            foreach (var child in root.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(child.DocumentOuterStartOffset.HasValue, Is.True, $"DocumentOuterStartOffset missing for {child.NodeName}");
                var start = child.DocumentOuterStartOffset.Value;
                var len = child.Size.TotalLength;
                // границы в исходном документе
                var sourceSlice = html.Substring(start, len).Replace(" ", string.Empty);
                // сохранённый исходный HTML узла
                var nodeHtml = child.GetOuterHtml(true).Replace(" ", string.Empty);
                Assert.That(nodeHtml, Is.EqualTo(sourceSlice), $"OuterHtml mismatch for node {child.NodeName}");
            }

            // Внутреннее содержимое родителя должно совпадать с конкатенацией исходных фрагментов детей
            var rootInnerFromSource = html.Substring(root.DocumentInnerStartOffset ?? 0, root.Size.InnerContentLength).Replace(" ", string.Empty);
            var concatChildren = string.Concat(root.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(true))).Replace(" ", string.Empty);
            Assert.That(concatChildren, Is.EqualTo(rootInnerFromSource));
        }

        [Test]
        public void Attributes_AbsolutePositions_Match_SourceSlices()
        {
            var html = "<div a='1' b c=\"3\"></div>";
            var doc = Parse(html);
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            // root document offset for div must exist
            Assert.That(div.DocumentOuterStartOffset.HasValue, Is.True);

            foreach (var attr in div.Attributes.OrderBy(x => x.Position))
            {
                Assert.That(attr.Position.HasValue, Is.True, $"Attribute {attr.Key} has no position");
                var absoluteAttrStart = div.DocumentOuterStartOffset.Value + attr.Position.Value;
                // attribute size is available as attr.Size (internal int), use BuildHtml(true) to get exact source text
                var built = attr.BuildHtml(true);
                // guard against out-of-range if parser didn't set sizes
                Assert.That(absoluteAttrStart + built.Length <= html.Length, Is.True, "Attribute slice out of bounds");
                var slice = html.Substring(absoluteAttrStart, built.Length);
                Assert.That(built, Is.EqualTo(slice), $"Attribute HTML mismatch for {attr.Key}");
            }
        }

        [Test]
        public void DocumentOffsets_AreMonotonic_And_SiblingSlices_DoNotOverlap()
        {
            var html = "<ul><li id='a'></li><li id='b'></li><li id='c'></li></ul>";
            var doc = Parse(html);
            var lis = doc.QuerySelectorAll<HtmlBaseNode>("li").ToList();
            for (int i = 0; i < lis.Count - 1; i++)
            {
                var cur = lis[i];
                var next = lis[i + 1];
                Assert.That(cur.DocumentOuterStartOffset.HasValue, Is.True);
                Assert.That(next.DocumentOuterStartOffset.HasValue, Is.True);

                var curEnd = cur.DocumentOuterEndOffset ?? (cur.DocumentOuterStartOffset + cur.Size.TotalLength);
                var nextStart = next.DocumentOuterStartOffset.Value;

                Assert.That(curEnd <= nextStart, Is.True, $"Nodes overlap: {cur.Id} and {next.Id}");
            }

            // убедимся, что общая конкатенация листов равна внутреннему содержимому ul в исходном тексте
            var ul = doc.QuerySelector<HtmlBaseNode>("ul");
            Assert.That(ul, Is.Not.Null);
            var ulInnerFromSource = html.Substring(ul.DocumentInnerStartOffset ?? 0, ul.Size.InnerContentLength);
            var concat = string.Concat(ul.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(true)));
            Assert.That(concat, Is.EqualTo(ulInnerFromSource));
        }
    }
}