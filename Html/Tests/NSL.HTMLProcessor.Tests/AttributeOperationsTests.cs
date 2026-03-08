using System.Linq;
using HtmlDocumentDev;
using NUnit.Framework;
using NSL.HTMLProcessor;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class AttributeOperationsTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void SetAttributeValue_CreatesAttributeWithoutQuotes_AndUpdatesOpenNodeLength()
        {
            var doc = Parse("<div></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            var beforeOpen = div.Size.OpenNodeLength;

            // create via SetAttributeValue
            div.SetAttributeValue("k", "v");
            var attr = div.FindAttribute("k");
            Assert.That(attr, Is.Not.Null);
            Assert.That(attr.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));
            Assert.That(attr.BuildHtml(true), Is.EqualTo("k=v"));

            Assert.That(div.Attributes.Count, Is.EqualTo(1));
            Assert.That(div.Size.OpenNodeLength, Is.GreaterThan(beforeOpen));
            Assert.That(attr.Position.HasValue, Is.True);
        }

        [Test]
        public void AddAttribute_WithQuoteType_PreservesQuoteAndSetsPosition()
        {
            var doc = Parse("<div></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");

            var beforeOpen = div.Size.OpenNodeLength;

            var a = new HtmlAttribute() { Key = "x", Value = "1", HasQuote = HtmlAttributeQuoteType.Double };
            div.AddAttribute(a);

            Assert.That(div.Attributes.Contains(a));
            Assert.That(a.Position.HasValue, Is.True);
            Assert.That(a.BuildHtml(true), Is.EqualTo("x=\"1\""));
            Assert.That(div.Size.OpenNodeLength, Is.GreaterThan(beforeOpen));

            // absolute document offset exists if parent has DocumentOuterStartOffset
            Assert.That(div.DocumentOuterStartOffset.HasValue, Is.True);
            var absStart = div.DocumentOuterStartOffset.Value + a.Position.Value;
            Assert.That(absStart + a.Size <= div.DocumentOuterStartOffset.Value + div.Size.OpenNodeLength + div.Size.InnerContentLength + div.Size.CloseNodeLength);
        }

        [Test]
        public void RemovingAttribute_RepositionsRemainingAttributes_And_RecalculatesOpenLength()
        {
            var doc = Parse("<div></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");

            // add two attributes
            div.SetAttributeValue("a", "1");
            div.SetAttributeValue("b", "2");

            var a = div.FindAttribute("a");
            var b = div.FindAttribute("b");
            Assert.That(a, Is.Not.Null);
            Assert.That(b, Is.Not.Null);

            var posA = a.Position.Value;
            var posB = b.Position.Value;

            var openBeforeRemove = div.Size.OpenNodeLength;

            // remove first
            div.RemoveAttribute(a);

            Assert.That(div.FindAttribute("a"), Is.Null);
            Assert.That(div.Attributes.Count, Is.EqualTo(1));

            var remaining = div.Attributes.First();
            Assert.That(remaining.Key, Is.EqualTo("b"));
            Assert.That(remaining.Position.HasValue, Is.True);
            Assert.That(remaining.Position.Value, Is.LessThanOrEqualTo(posB));

            Assert.That(div.Size.OpenNodeLength, Is.LessThanOrEqualTo(openBeforeRemove));
        }

        [Test]
        public void ParsedAttributes_PreserveQuoteType_FromSource()
        {
            var doc = Parse("<div a='1' b=\"2\" c=3 d></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            var a = div.FindAttribute("a");
            var b = div.FindAttribute("b");
            var c = div.FindAttribute("c");
            var d = div.FindAttribute("d");

            Assert.That(a, Is.Not.Null);
            Assert.That(a.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Single));
            Assert.That(a.BuildHtml(true), Is.EqualTo("a='1'"));

            Assert.That(b, Is.Not.Null);
            Assert.That(b.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Double));
            Assert.That(b.BuildHtml(true), Is.EqualTo("b=\"2\""));

            Assert.That(c, Is.Not.Null);
            Assert.That(c.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));
            Assert.That(c.BuildHtml(true), Is.EqualTo("c=3"));

            Assert.That(d, Is.Not.Null);
            Assert.That(d.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));
            Assert.That(d.BuildHtml(true), Is.EqualTo("d"));
        }
    }
}