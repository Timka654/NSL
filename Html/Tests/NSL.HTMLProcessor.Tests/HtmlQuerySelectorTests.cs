using System;
using System.Linq;
using HtmlDocumentDev;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class HtmlQuerySelectorTests
    {
        private const string Html = @"
<ul class=""pagination"" id=""main-list"">
    <li class=""prev disabled""><span>&laquo;</span></li>
    <li class=""active""><a href=""#"" data-num=""1"">1</a></li>
    <li><a href=""#"" data-num=""2"">2</a></li>
    <li class=""next""><a href=""#"" data-num=""3"">&raquo;</a></li>
</ul>

<div id=""box"">
    <p class=""item"">Hello</p>
    <p class=""item note"">World</p>
    <span>Free</span>
    <p data-active=""1"">Yes</p>
</div>

<ul id=""siblings"">
    <li id=""s1""></li>
    <li id=""s2""></li>
    <li id=""s3""></li>
</ul>
";

        private HtmlDocumentNode Parse()
        {
            // Если у тебя другой метод — адаптируй здесь
            return HtmlParser.Parse<HtmlDocumentNode>(Html, new HtmlParserOptions() { SkipEmptyTextNodes = true });
        }

        // ---------------------------------------------------------
        // 1. CSS selector tests
        // ---------------------------------------------------------

        [Test]
        public void CssSelectors_WorkCorrectly()
        {
            var doc = Parse();

            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("li").ToList(), Has.Count.EqualTo(7));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("li.active").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("a[data-num=2]").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination a").ToList(), Has.Count.EqualTo(3));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li").ToList(), Has.Count.EqualTo(4));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li:not([class])").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("li.active + li").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("li.active ~ li").ToList(), Has.Count.EqualTo(2));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li:nth-child(3)").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("p[data-active]").ToList(), Has.Count.EqualTo(1));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("a[href^=#]").ToList(), Has.Count.EqualTo(3));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("#s1 ~ li").ToList(), Has.Count.EqualTo(2));
        }

        // ---------------------------------------------------------
        // 2. NodeSize offsets basic test
        // ---------------------------------------------------------

        [Test]
        public void NodeSize_OffsetsAreCorrect()
        {
            var doc = Parse();

            var li = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li").ElementAt(2);

            string outer = li.GetOuterHtml(false);
            string inner = li.GetInnerHtml(false);
            var s = li.Size;

            Assert.That(s.TotalLength, Is.EqualTo(outer.Length));
            Assert.That(s.InnerContentLength, Is.EqualTo(inner.Length));

            Assert.That(s.OuterStartOffset, Is.EqualTo(0));
            Assert.That(s.OuterEndOffset, Is.EqualTo(s.TotalLength));

            Assert.That(s.InnerStartOffset, Is.EqualTo(s.OpenNodeLength));
            Assert.That(s.InnerEndOffset, Is.EqualTo(s.InnerStartOffset + s.InnerContentLength));
        }

        // ---------------------------------------------------------
        // 3. Document offsets ordering
        // ---------------------------------------------------------

        [Test]
        public void DocumentOffsets_AreMonotonic()
        {
            var doc = Parse();
            var lis = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li").ToList();

            for (int i = 0; i < lis.Count - 1; i++)
            {
                Assert.That(lis[i].DocumentOuterEndOffset <= lis[i + 1].DocumentOuterStartOffset, Is.True);
            }
        }

        // ---------------------------------------------------------
        // 4. Node modifications recalc offsets
        // ---------------------------------------------------------

        [Test]
        public void ModifyNodes_RecalculateOffsets()
        {
            var doc = Parse();
            var ul = doc.QuerySelector<HtmlBaseNode>("ul.pagination");

            var injected = HtmlParser.Parse<HtmlDocumentNode>("<li>XXX</li>").ChildNodes.First();
            var first = ul.ChildNodes.First();

            ul.AddChildNodeBefore(injected, first);

            Assert.That(ul.ChildNodes.OrderBy(x=>x.Position).First(), Is.EqualTo(injected));
            Assert.That(ul.ChildNodes[1].DocumentOuterStartOffset > injected.DocumentOuterEndOffset, Is.True);
        }

        // ---------------------------------------------------------
        // 5. Tree structure (parent/children)
        // ---------------------------------------------------------

        [Test]
        public void TreeStructure_IsCorrect()
        {
            var doc = Parse();
            var ul = doc.QuerySelector<HtmlBaseNode>("ul.pagination");

            Assert.That(ul.ChildNodes.ToList(), Has.Count.EqualTo(4));

            foreach (var li in ul.ChildNodes)
                Assert.That(li.Parent, Is.EqualTo(ul));
        }

        // ---------------------------------------------------------
        // 6. NodeSize vs BuildHtml
        // ---------------------------------------------------------

        [Test]
        public void NodeSize_Matches_HtmlBuilder()
        {
            var doc = Parse();

            foreach (var node in doc.QuerySelectorAll<HtmlBaseNode>("*"))
            {
                string outer = node.GetOuterHtml(false);
                Assert.That(node.Size.TotalLength, Is.EqualTo(outer.Length));
            }
        }

        // ---------------------------------------------------------
        // 7. Stress / Edge case tests
        // ---------------------------------------------------------

        [Test]
        public void Stress_InvalidSelectors_DoNotCrash()
        {
            var doc = Parse();

            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination >").ToList(), Has.Count.EqualTo(0));
            Assert.That(
                doc.QuerySelectorAll<HtmlBaseNode>("ul li").Count(),
                Is.EqualTo(doc.QuerySelectorAll<HtmlBaseNode>("ul  li").Count())
            );
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("> li").ToList(), Has.Count.EqualTo(0));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-num=]").ToList(), Has.Count.EqualTo(0));
        }
    }
}
