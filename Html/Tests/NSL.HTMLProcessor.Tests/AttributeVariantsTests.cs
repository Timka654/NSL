using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class AttributeVariantsTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void MixedQuotesAndUnquotedAttributes_AreParsedCorrectly()
        {
            var html1 = "<a a=1 b=\"2\" c='3'></a>";
            var html2 = "<a a=1 b=\"2\" c='3' />";

            var doc1 = Parse(html1);
            var a1 = doc1.QuerySelector<HtmlBaseNode>("a");
            Assert.That(a1, Is.Not.Null);

            var attrs1 = a1.Attributes.OrderBy(x => x.Position).ToList();
            Assert.That(attrs1.Count, Is.EqualTo(3));

            Assert.That(attrs1[0].Key, Is.EqualTo("a"));
            Assert.That(attrs1[0].Value, Is.EqualTo("1"));
            Assert.That(attrs1[0].HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));
            Assert.That(attrs1[0].BuildHtml(true), Is.EqualTo("a=1"));

            Assert.That(attrs1[1].Key, Is.EqualTo("b"));
            Assert.That(attrs1[1].Value, Is.EqualTo("2"));
            Assert.That(attrs1[1].HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Double));
            Assert.That(attrs1[1].BuildHtml(true), Is.EqualTo("b=\"2\""));

            Assert.That(attrs1[2].Key, Is.EqualTo("c"));
            Assert.That(attrs1[2].Value, Is.EqualTo("3"));
            Assert.That(attrs1[2].HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Single));
            Assert.That(attrs1[2].BuildHtml(true), Is.EqualTo("c='3'"));

            // self-closing variant
            var doc2 = Parse(html2);
            var a2 = doc2.QuerySelector<HtmlBaseNode>("a");
            Assert.That(a2, Is.Not.Null);

            var attrs2 = a2.Attributes.OrderBy(x => x.Position).ToList();
            Assert.That(attrs2.Count, Is.EqualTo(3));

            var a2outerHtml = a2.GetOuterHtml(false);

            // sizes consistent with builder
            Assert.That(a1.Size.TotalLength, Is.EqualTo(a1.GetOuterHtml(false).Length));
            Assert.That(a2.Size.TotalLength, Is.EqualTo(a2outerHtml.Length));
        }

        [Test]
        public void AttributeOrderPositions_AreMonotonic_AndMatchSourceSlicesWhenTrimmed()
        {
            var html = "<div a=1 b='2' c=\"3\"></div>";
            var doc = Parse(html);
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            // positions should increase
            var attrs = div.Attributes.OrderBy(x => x.Position).ToList();
            for (int i = 0; i < attrs.Count - 1; i++)
            {
                Assert.That(attrs[i].Position < attrs[i + 1].Position, Is.True);
            }

            // Compare attribute html with source slice (normalize spaces)
            foreach (var attr in attrs)
            {
                Assert.That(attr.Position.HasValue, Is.True);
                var absStart = div.DocumentOuterStartOffset.Value + attr.Position.Value;
                var built = attr.BuildHtml(true);
                var slice = html.Substring(absStart, built.Length);
                Assert.That(built.Replace(" ", string.Empty), Is.EqualTo(slice.Replace(" ", string.Empty)));
            }
        }

        //[Test]
        //public void SelfClosingVariants_ReportConsistentSizes()
        //{
        //    var variants = new[]
        //    {
        //        "<br>",
        //        "<br/>",
        //        "<br />"
        //    };

        //    foreach (var html in variants)
        //    {
        //        var doc = Parse(html);
        //        var br = doc.QuerySelectorAll<HtmlBaseNode>("br").FirstOrDefault();
        //        Assert.That(br, Is.Not.Null);

        //        // TotalLength must match builder output length
        //        Assert.That(br.Size.TotalLength, Is.EqualTo(br.GetOuterHtml(false).Length));
        //        Assert.That(br.NodeName.ToLower(), Is.EqualTo("br"));
        //    }
        //}

        [Test]
        public void AttributesWithoutValue_AndEmptyValue_AreHandled()
        {
            var html = "<input disabled readonly=''></input>";
            var doc = Parse(html);
            var input = doc.QuerySelector<HtmlBaseNode>("input");
            Assert.That(input, Is.Not.Null);

            var disabled = input.FindAttribute("disabled");
            var ro = input.FindAttribute("readonly");

            Assert.That(disabled, Is.Not.Null);
            // attribute without value -> Value == null
            Assert.That(disabled.Value, Is.Null);
            Assert.That(disabled.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));
            Assert.That(disabled.BuildHtml(true), Is.EqualTo("disabled"));

            Assert.That(ro, Is.Not.Null);
            // empty string preserved
            Assert.That(ro.Value, Is.EqualTo(string.Empty));
            Assert.That(ro.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Single));
            Assert.That(ro.BuildHtml(true), Is.EqualTo("readonly=''"));

            var outerHtml = input.GetOuterHtml(true);
            // parent sizes consistent
            Assert.That(input.Size.TotalLength, Is.EqualTo(outerHtml.Length));
        }
    }
}