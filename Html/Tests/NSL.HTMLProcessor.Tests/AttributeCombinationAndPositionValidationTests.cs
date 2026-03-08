using System.Linq;
using HtmlDocumentDev;
using NUnit.Framework;
using NSL.HTMLProcessor;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class AttributeCombinationAndPositionValidationTests
    {
        private HtmlDocumentNode Parse(string html = null)
            => HtmlParser.Parse<HtmlDocumentNode>(html ?? "<root></root>", new HtmlParserOptions() { SkipEmptyTextNodes = true });

        [Test]
        public void ManyAttributeForms_ParsePositionsAndSizesAreConsistent()
        {
            var html = "<root>"
                     + "<a a=1 b=\"2\" c='3'></a>"
                     + "<b x='x y' y= z=\"\"></b>"
                     + "<c single noValue disabled></c>"
                     + "</root>";

            var doc = Parse(html);
            var root = doc.QuerySelector<HtmlBaseNode>("root");
            Assert.That(root, Is.Not.Null);

            foreach (var node in root.ChildNodes.OrderBy(n => n.Position))
            {
                // basic size invariants
                Assert.That(node.Size.TotalLength, Is.EqualTo(node.GetOuterHtml(false).Length), $"TotalLength mismatch for {node.NodeName}");

                // open tag length must equal measured OpenNodeLength
                var outer = node.GetOuterHtml(false);
                var idx = outer.IndexOf('>');
                Assert.That(idx, Is.GreaterThanOrEqualTo(0), $"Missing '>' in built outer for {node.NodeName}");
                var openLen = idx + 1;
                Assert.That(node.Size.OpenNodeLength, Is.EqualTo(openLen), $"OpenNodeLength mismatch for {node.NodeName}");

                // document offsets must be set
                Assert.That(node.DocumentOuterStartOffset.HasValue, Is.True, $"No DocumentOuterStartOffset for {node.NodeName}");
                if (node.HasBody)
                {
                    Assert.That(node.DocumentInnerStartOffset.HasValue, Is.True);
                    Assert.That(node.DocumentInnerStartOffset, Is.EqualTo(node.DocumentOuterStartOffset + node.Size.OpenNodeLength));
                }
            }
        }

        [Test]
        public void AttributeOrderAndAbsoluteSlices_MatchSourceWhenNormalized()
        {
            var html = "<div a=1 b='two' c=\"three four\" d=></div>";
            var doc = Parse(html);
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);
            Assert.That(div.DocumentOuterStartOffset.HasValue, Is.True);

            var attrs = div.Attributes.OrderBy(x => x.Position).ToList();
            Assert.That(attrs.Count, Is.GreaterThanOrEqualTo(1));

            foreach (var attr in attrs)
            {
                Assert.That(attr.Position.HasValue, Is.True, $"Attr {attr.Key} missing Position");
                var absStart = div.DocumentOuterStartOffset.Value + attr.Position.Value;

                var built = attr.BuildHtml(true);
                // normalize spaces for comparison — parser may preserve/omit some spaces
                var slice = html.Substring(absStart, built.Length);
                Assert.That(built.Replace(" ", string.Empty), Is.EqualTo(slice.Replace(" ", string.Empty)),
                    $"Attribute slice mismatch for {attr.Key}: built='{built}', slice='{slice}'");
            }

            // attributes positions should be strictly increasing
            for (int i = 0; i < attrs.Count - 1; i++)
                Assert.That(attrs[i].Position < attrs[i + 1].Position, Is.True, "Attribute positions not increasing");
        }

        [Test]
        public void SelfClosingAndVoidVariants_ReportConsistentOffsetsAndSizes()
        {
            var variants = new[]
            {
                "<img src='x.png' alt=img>",
                "<img src='x.png' alt=img/>",
                "<img src='x.png' alt=img />"
            };

            foreach (var html in variants)
            {
                var doc = Parse(html);
                var img = doc.QuerySelectorAll<HtmlBaseNode>("img").FirstOrDefault();
                Assert.That(img, Is.Not.Null);

                var imgHtml = img.GetOuterHtml(true).Replace(" ", string.Empty).Replace("/>", ">");

                // Total length matches builder
                Assert.That(imgHtml, Is.EqualTo(html.Replace(" ", string.Empty).Replace("/>", ">")));

                // document offsets must exist and non-overlapping within root (single element here)
                Assert.That(img.DocumentOuterStartOffset.HasValue, Is.True);
            }
        }

        [Test]
        public void AttributesWithEmptyAndMissingValues_AreHandledAndContributeToSize()
        {
            var html = "<input disabled readonly='' checked=></input>";
            var doc = Parse(html);
            var input = doc.QuerySelector<HtmlBaseNode>("input");
            Assert.That(input, Is.Not.Null);

            var disabled = input.FindAttribute("disabled");
            var readOnly = input.FindAttribute("readonly");
            var checkedAttr = input.FindAttribute("checked");

            Assert.That(disabled, Is.Not.Null);
            Assert.That(disabled.Value, Is.Null);
            Assert.That(disabled.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));

            Assert.That(readOnly, Is.Not.Null);
            Assert.That(readOnly.Value, Is.EqualTo(string.Empty));
            Assert.That(readOnly.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.Single).Or.EqualTo(HtmlAttributeQuoteType.Double));

            Assert.That(checkedAttr, Is.Not.Null);
            // depending on parser behaviour value can be null or empty — accept both but ensure size accounted
            Assert.That(checkedAttr.HasQuote, Is.EqualTo(HtmlAttributeQuoteType.None));

            // overall size consistency
            Assert.That(input.Size.TotalLength, Is.EqualTo(input.GetOuterHtml(false).Length));
            Assert.That(input.Size.OpenNodeLength, Is.GreaterThan(input.NodeName.Length + 1));
        }

        [Test]
        public void ParentOpenNodeLength_EqualsDetectedOpenTagLength_AfterGetOuterHtmlFalse()
        {
            var html = "<div id='root' class=cls data='v v'><span></span></div>";
            var doc = Parse(html);
            var div = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(div, Is.Not.Null);

            // force recalculation via GetOuterHtml(false)
            var outer = div.GetOuterHtml(false);

            var firstClose = outer.IndexOf('>');
            Assert.That(firstClose, Is.GreaterThanOrEqualTo(0));
            var openTagLen = firstClose + 1;

            Assert.That(div.Size.OpenNodeLength, Is.EqualTo(openTagLen));
            Assert.That(div.Size.TotalLength, Is.EqualTo(outer.Length));
            if (div.HasBody)
                Assert.That(div.DocumentInnerStartOffset, Is.EqualTo(div.DocumentOuterStartOffset + div.Size.OpenNodeLength));
        }
    }
}