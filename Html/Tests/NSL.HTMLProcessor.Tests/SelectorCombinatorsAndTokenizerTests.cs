using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class SelectorCombinatorsAndTokenizerTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void Combinator_SpacingVariants_AreEquivalent()
        {
            var html = "<ul><li></li><li></li></ul>";
            var doc = Parse(html);

            var noSpace = doc.QuerySelectorAll<HtmlBaseNode>("ul>li").ToList();
            var single = doc.QuerySelectorAll<HtmlBaseNode>("ul > li").ToList();
            var many = doc.QuerySelectorAll<HtmlBaseNode>("ul   >   li").ToList();

            Assert.That(noSpace.Count, Is.EqualTo(2));
            Assert.That(noSpace.Count, Is.EqualTo(single.Count));
            Assert.That(noSpace.Count, Is.EqualTo(many.Count));
        }

        [Test]
        public void Tokenizer_CombinatorEdges_DoNotThrow_AndReturnEmpty()
        {
            var doc = Parse("<div><li></li></div>");

            Assert.DoesNotThrow(() => {
                var r1 = doc.QuerySelectorAll<HtmlBaseNode>("> li").ToList();
                Assert.That(r1.Count, Is.EqualTo(0));
            });

            Assert.DoesNotThrow(() => {
                var r2 = doc.QuerySelectorAll<HtmlBaseNode>("div >").ToList();
                Assert.That(r2.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void GroupSelectors_Comma_CombineResults()
        {
            var html = "<div><a id='a'></a><span id='s'></span></div>";
            var doc = Parse(html);

            var both = doc.QuerySelectorAll<HtmlBaseNode>("a, span").ToList();
            Assert.That(both.Count, Is.EqualTo(2));
            Assert.That(both.Select(n => n.Id).OrderBy(x => x).ToArray(), Is.EquivalentTo(new[] { "a", "s" }));
        }

        [Test]
        public void UniversalSelector_SelectsAllUnderRoot()
        {
            var doc = Parse("<div><a></a><span></span></div>");
            var all = doc.QuerySelectorAll<HtmlBaseNode>("*").ToList();
            Assert.That(all.Count >= 2, Is.True);
        }
    }
}