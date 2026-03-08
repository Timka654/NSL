using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class AttributeAndPseudoSelectorsTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void AttributeOperators_AllKinds_Work()
        {
            var html = @"<div>
    <a data-x='pref-suf' id='a'></a>
    <a data-x='pref' id='b'></a>
    <a data-x='suf' id='c'></a>
    <a data-x='inmiddle' id='d'></a>
    <a data-empty='' id='e'></a>
</div>";
            var doc = Parse(html);

            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-x^=pref]").Select(n => n.Id).OrderBy(x => x).ToArray(), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-x$=suf]").Select(n => n.Id).OrderBy(x => x).ToArray(), Is.EqualTo(new[] { "a", "c" }));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-x*=mid]").Select(n => n.Id).ToArray(), Is.EqualTo(new[] { "d" }));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-x=pref]").Select(n => n.Id).ToArray(), Is.EqualTo(new[] { "b" }));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[data-empty]").Select(n => n.Id).ToArray(), Is.EqualTo(new[] { "e" }));

            var qdoc = Parse(@"<div><a data-q=""a b"" id='q'></a></div>");
            Assert.That(qdoc.QuerySelectorAll<HtmlBaseNode>("[data-q=\"a b\"]").Select(n => n.Id).ToArray(), Is.EqualTo(new[] { "q" }));
        }

        [Test]
        public void LangAndClassSpecialAttrHandling_Works()
        {
            var html = @"<div><p lang='en-US' id='a'></p><p lang='en' id='b'></p><p class='one two' id='c'></p></div>";
            var doc = Parse(html);

            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[lang|=en]").Select(n => n.Id).OrderBy(x => x).ToArray(), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("[class~=two]").Select(n => n.Id).ToArray(), Is.EqualTo(new[] { "c" }));
        }

        [Test]
        public void Pseudo_Has_Is_Not_OnlyFirstLast_Work()
        {
            var html = @"<div id='box'><p class='item'></p><p class='item note'></p></div><ul><li class='first'></li><li></li><li class='last'></li></ul>";
            var doc = Parse(html);

            var has = doc.QuerySelectorAll<HtmlBaseNode>("div:has(.item.note)").Select(n => n.Id).ToArray();
            Assert.That(has, Is.EqualTo(new[] { "box" }));

            var notActive = doc.QuerySelectorAll<HtmlBaseNode>("li:not(.first)").ToList();
            Assert.That(notActive.Any(n => n.Class == null || !n.Class.Contains("first")));

            var first = doc.QuerySelectorAll<HtmlBaseNode>("li:first-child").ToList();
            Assert.That(first.Count, Is.GreaterThanOrEqualTo(1));

            var last = doc.QuerySelectorAll<HtmlBaseNode>("li:last-child").ToList();
            Assert.That(last.Count, Is.GreaterThanOrEqualTo(1));
        }
    }
}