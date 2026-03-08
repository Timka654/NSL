using System.Linq;
using HtmlDocumentDev;
using NUnit.Framework;
using NSL.HTMLProcessor;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class AttributePositionTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void AddingAttributes_UpdatesPositionsAndOpenNodeLength()
        {
            var doc = Parse("<div></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            var beforeOpen = div.Size.OpenNodeLength;

            // add first attribute
            div.SetAttributeValue("a", "1");
            var aAttr = div.FindAttribute("a");
            Assert.That(aAttr, Is.Not.Null);
            Assert.That(aAttr.Position.HasValue, Is.True);
            Assert.That(div.Attributes.Count, Is.EqualTo(1));
            Assert.That(div.Size.OpenNodeLength, Is.GreaterThan(beforeOpen));

            var posA = aAttr.Position.Value;

            // add second attribute -> position must be after first
            div.SetAttributeValue("b", "2");
            var bAttr = div.FindAttribute("b");
            Assert.That(bAttr, Is.Not.Null);
            Assert.That(bAttr.Position.HasValue, Is.True);
            Assert.That(bAttr.Position.Value, Is.GreaterThan(posA));
            Assert.That(div.Attributes.Count, Is.EqualTo(2));
            Assert.That(div.Size.OpenNodeLength, Is.GreaterThan(beforeOpen));
        }

        [Test]
        public void RemovingAttribute_RecalculatesOpenNodeLength_And_RemovesAttribute()
        {
            var doc = Parse("<div></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");

            // add two attributes
            div.SetAttributeValue("x", "v1");
            div.SetAttributeValue("y", "v2");

            var beforeOpen = div.Size.OpenNodeLength;
            var xAttr = div.FindAttribute("x");
            Assert.That(xAttr, Is.Not.Null);

            // remove first attribute
            div.RemoveAttribute(xAttr);

            Assert.That(div.FindAttribute("x"), Is.Null);
            Assert.That(div.Attributes.Count, Is.EqualTo(1));

            // open node length must decrease (or at least not increase)
            Assert.That(div.Size.OpenNodeLength, Is.LessThanOrEqualTo(beforeOpen));

            // remaining attribute still has valid position
            var remaining = div.Attributes.First();
            Assert.That(remaining.Position.HasValue, Is.True);
        }

        [Test]
        public void AttributeWithoutValue_ParsedAndContributesToSizes()
        {
            var doc = Parse("<div disabled></div>");
            var div = doc.QuerySelector<HtmlBaseNode>("div");

            var attr = div.FindAttribute("disabled");
            Assert.That(attr, Is.Not.Null);
            Assert.That(attr.Position.HasValue, Is.True);

            var divHtml = div.GetOuterHtml(false);

            // TotalLength должен соответствовать OuterHtml длине
            Assert.That(div.Size.TotalLength, Is.EqualTo(divHtml.Length));

            // OpenNodeLength учитывает атрибут
            Assert.That(div.Size.OpenNodeLength, Is.GreaterThan(div.NodeName.Length + 1));
        }
    }
}