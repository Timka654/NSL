using System.Linq;
using NUnit.Framework;

namespace NSL.HTMLProcessor.Tests
{
    [TestFixture]
    public class SelectorEngineAdditionalTests
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

        private HtmlDocumentNode Parse(string html = null)
        {
            return HtmlParser.Parse<HtmlDocumentNode>(html ?? Html, new HtmlParserOptions() { SkipEmptyTextNodes = true });
        }

        [Test]
        public void FindNodes_RespectsRecursiveFlag()
        {
            var doc = Parse();
            var box = doc.QuerySelector<HtmlBaseNode>("#box");

            var shallow = box.FindNodes(n => n.NodeName == "p", recursive: false).ToList();
            Assert.That(shallow.Count, Is.EqualTo(3));

            var deep = box.FindNodes(n => n.NodeName == "p", recursive: true).ToList();
            Assert.That(deep.Count, Is.EqualTo(3));
        }

        [Test]
        public void SiblingNavigation_WorksCorrectly()
        {
            var doc = Parse();
            var s1 = doc.QuerySelectorAll<HtmlBaseNode>("#siblings li").FirstOrDefault(n => n.Id == "s1");
            var s2 = doc.QuerySelectorAll<HtmlBaseNode>("#siblings li").FirstOrDefault(n => n.Id == "s2");
            var s3 = doc.QuerySelectorAll<HtmlBaseNode>("#siblings li").FirstOrDefault(n => n.Id == "s3");

            Assert.That(s1.NextSibling, Is.EqualTo(s2));
            Assert.That(s2.NextSibling, Is.EqualTo(s3));
            Assert.That(s3.NextSibling, Is.Null);

            var following = s1.FollowingSiblings().ToList();
            Assert.That(following.Count, Is.EqualTo(2));
            Assert.That(following[0], Is.EqualTo(s2));
            Assert.That(following[1], Is.EqualTo(s3));
        }

        [Test]
        public void AttributeSelectors_ContainsWord_And_EqualsOrStartsWith()
        {
            var html = @"<div><p lang='en-US' id='a'></p><p lang='en' id='b'></p><p class='one two' id='c'></p></div>";
            var doc = Parse(html);

            var langMatches = doc.QuerySelectorAll<HtmlBaseNode>("[lang|=en]").ToList();
            Assert.That(langMatches.Count, Is.EqualTo(2));
            Assert.That(langMatches.Select(n => n.Id).OrderBy(s => s).ToArray(), Is.EqualTo(new[] { "a", "b" }));

            var classMatches = doc.QuerySelectorAll<HtmlBaseNode>("[class~=two]").ToList();
            Assert.That(classMatches.Count, Is.EqualTo(1));
            Assert.That(classMatches[0].Id, Is.EqualTo("c"));
        }

        [Test]
        public void PseudoSelectors_Has_Is_Not_Working()
        {
            var doc = Parse();

            var hasDiv = doc.QuerySelectorAll<HtmlBaseNode>("div:has(.item.note)").ToList();
            Assert.That(hasDiv.Count, Is.EqualTo(1));
            Assert.That(hasDiv[0].Id, Is.EqualTo("box"));

            var isMatches = doc.QuerySelectorAll<HtmlBaseNode>("p:is(.item, [data-active])").ToList();
            Assert.That(isMatches.Count, Is.GreaterThanOrEqualTo(2));

            var notActive = doc.QuerySelectorAll<HtmlBaseNode>("li:not(.active)").ToList();
            var paginationLis = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li").ToList();
            Assert.That(notActive.Intersect(paginationLis).Count(), Is.EqualTo(3));
        }

        [Test]
        public void NthSelectors_OddEven_And_OfType()
        {
            var doc = Parse();

            var odd = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li:nth-child(odd)").ToList();
            Assert.That(odd.Count, Is.EqualTo(2));

            var even = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination > li:nth-child(even)").ToList();
            Assert.That(even.Count, Is.EqualTo(2));

            var html = @"<div id='mix'><p></p><span></span><p></p><p></p><span></span></div>";
            var mixed = Parse(html);
            var secondP = mixed.QuerySelectorAll<HtmlBaseNode>("#mix p:nth-of-type(2)").ToList();
            Assert.That(secondP.Count, Is.EqualTo(1));
            Assert.That(secondP[0].NodeName.ToLower(), Is.EqualTo("p"));
        }

        [Test]
        public void Combinator_Whitespace_BehavesLikeDescendant()
        {
            var doc = Parse();

            var withSingleSpace = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination a").ToList();
            var withDoubleSpace = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination  a").ToList();

            Assert.That(withSingleSpace.Count, Is.EqualTo(withDoubleSpace.Count));
        }

        [Test]
        public void NodeSize_Struct_Calculations_And_Equals()
        {
            var html = "<div><span></span><span></span></div>";
            var doc = Parse(html);

            var spans = doc.QuerySelectorAll<HtmlBaseNode>("span").ToList();
            Assert.That(spans.Count, Is.EqualTo(2));

            var s1 = spans[0];
            var s2 = spans[1];

            Assert.That(s1.Size.Equals(s2.Size), Is.True);

            Assert.That(s1.Size.TotalLength, Is.EqualTo(s1.GetOuterHtml(false).Length));
            Assert.That(s2.Size.TotalLength, Is.EqualTo(s2.GetOuterHtml(false).Length));

            Assert.That(s1.Size.InnerStartOffset, Is.EqualTo(s1.Size.OuterStartOffset + s1.Size.OpenNodeLength));
            Assert.That(s1.Size.InnerEndOffset, Is.EqualTo(s1.Size.InnerStartOffset + s1.Size.InnerContentLength));

            var parent = doc.QuerySelector<HtmlBaseNode>("div");
            Assert.That(parent.Size.Equals(s1.Size), Is.False);
        }

        [Test]
        public void Tokenizer_CombinatorEdges_ReturnsNoMatches()
        {
            var doc = Parse();

            Assert.DoesNotThrow(() => {
                var res = doc.QuerySelectorAll<HtmlBaseNode>("> li").ToList();
                Assert.That(res.Count, Is.EqualTo(0));
            });

            Assert.DoesNotThrow(() => {
                var res = doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination >").ToList();
                Assert.That(res.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Combinator_Spacing_Variants_AreEquivalent()
        {
            var html = "<ul><li></li><li></li></ul>";
            var doc = Parse(html);

            var a = doc.QuerySelectorAll<HtmlBaseNode>("ul>li").ToList();
            var b = doc.QuerySelectorAll<HtmlBaseNode>("ul > li").ToList();
            var c = doc.QuerySelectorAll<HtmlBaseNode>("ul   >   li").ToList();

            Assert.That(a.Count, Is.EqualTo(2));
            Assert.That(a.Count, Is.EqualTo(b.Count));
            Assert.That(a.Count, Is.EqualTo(c.Count));
        }

        [Test]
        public void AttributeOperators_AllKinds_WorkCorrectly()
        {
            var html = @"<div>
    <a data-x='pref-suf' id='a'></a>
    <a data-x='pref' id='b'></a>
    <a data-x='suf' id='c'></a>
    <a data-x='inmiddle' id='d'></a>
    <a data-empty='' id='e'></a>
</div>";
            var doc = Parse(html);

            var starts = doc.QuerySelectorAll<HtmlBaseNode>("[data-x^=pref]").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(starts, Is.EqualTo(new[] { "a", "b" }));

            var ends = doc.QuerySelectorAll<HtmlBaseNode>("[data-x$=suf]").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(ends, Is.EqualTo(new[] { "a", "c" }));

            var contains = doc.QuerySelectorAll<HtmlBaseNode>("[data-x*=mid]").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(contains, Is.EqualTo(new[] { "d" }));

            var eq = doc.QuerySelectorAll<HtmlBaseNode>("[data-x=pref]").Select(n => n.Id).ToArray();
            Assert.That(eq, Is.EqualTo(new[] { "b" }));

            var exists = doc.QuerySelectorAll<HtmlBaseNode>("[data-empty]").Select(n => n.Id).ToArray();
            Assert.That(exists, Is.EqualTo(new[] { "e" }));

            var quotedHtml = @"<div><a data-q=""a b"" id='q'></a></div>";
            var qdoc = Parse(quotedHtml);
            var qres = qdoc.QuerySelectorAll<HtmlBaseNode>("[data-q=\"a b\"]").Select(n => n.Id).ToArray();
            Assert.That(qres, Is.EqualTo(new[] { "q" }));
        }

        [Test]
        public void UniversalSelector_SelectsAllElementsUnderRoot()
        {
            var doc = Parse("<div><a></a><span></span></div>");
            var alls = doc.QuerySelectorAll<HtmlBaseNode>("*").ToList();
            Assert.That(alls.Count >= 2, Is.True);
        }

        [Test]
        public void NthSelectors_ComplexExpressions()
        {
            var html = "<ul>" +
                       "<li id='i1'></li><li id='i2'></li><li id='i3'></li>" +
                       "<li id='i4'></li><li id='i5'></li><li id='i6'></li></ul>";
            var doc = Parse(html);

            var resOdd = doc.QuerySelectorAll<HtmlBaseNode>("ul > li:nth-child(2n+1)").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(resOdd, Is.EqualTo(new[] { "i1", "i3", "i5" }));

            var res3n = doc.QuerySelectorAll<HtmlBaseNode>("ul > li:nth-child(3n)").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(res3n, Is.EqualTo(new[] { "i3", "i6" }));

            var resEven = doc.QuerySelectorAll<HtmlBaseNode>("ul > li:nth-child(2n)").Select(n => n.Id).OrderBy(x => x).ToArray();
            Assert.That(resEven, Is.EqualTo(new[] { "i2", "i4", "i6" }));

            var exact4 = doc.QuerySelectorAll<HtmlBaseNode>("ul > li:nth-child(4)").Select(n => n.Id).ToArray();
            Assert.That(exact4, Is.EqualTo(new[] { "i4" }));
        }

        // ---------- Новые тесты: позиции и формы тэгов ----------

        [Test]
        public void TagForms_ParsePositionsAndSizes()
        {
            // разные формы — самозакрывающийся, с телом, атрибут без значения, разные кавычки, пробелы
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

            // сумма OuterHtml дочерних должна равняться InnerHtml root
            var builtInner = root.GetInnerHtml(false);
            var concatChildren = string.Concat(root.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)));
            Assert.That(builtInner, Is.EqualTo(concatChildren));

            // проверим что для каждого child Position соответствует накопленному offset'у и Size совпадает с OuterHtml.Length
            int offset = 0;
            foreach (var child in root.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(child.Position, Is.EqualTo(offset));
                Assert.That(child.Size.TotalLength, Is.EqualTo(child.GetOuterHtml(false).Length));
                offset += child.Size.TotalLength;
            }

            // родительский InnerContentLength должен совпадать с суммой InnerContentLength дочерних + Open/Close тегов handled by parent size calc
            Assert.That(root.Size.InnerContentLength, Is.EqualTo(builtInner.Length));
        }

        [Test]
        public void Positions_AreMonotonicAndMatchSizes_InComplexTree()
        {
            var html = "<div id='wrap'><span id='s1'></span><span id='s2'/><p id='p1'><em></em></p></div>";
            var doc = Parse(html);

            var wrap = doc.QuerySelector<HtmlBaseNode>("#wrap");
            Assert.That(wrap, Is.Not.Null);

            // children positions monotonic and contiguous according to Size.TotalLength
            int pos = 0;
            foreach (var ch in wrap.ChildNodes.OrderBy(x => x.Position))
            {
                Assert.That(ch.Position, Is.EqualTo(pos));
                Assert.That(ch.Size.TotalLength, Is.GreaterThan(0));
                pos += ch.Size.TotalLength;
            }

            // внутреннее содержимое родителя = конкатенация children OuterHtml
            Assert.That(wrap.GetInnerHtml(false), Is.EqualTo(string.Concat(wrap.ChildNodes.OrderBy(x => x.Position).Select(x => x.GetOuterHtml(false)))));
        }

        [Test]
        public void Stress_InvalidSelectors_DoNotThrow_And_TreatedConsistently()
        {
            var doc = Parse();

            Assert.DoesNotThrow(() => doc.QuerySelectorAll<HtmlBaseNode>("ul.pagination >").ToList());
            Assert.DoesNotThrow(() => doc.QuerySelectorAll<HtmlBaseNode>("> li").ToList());
            Assert.That(doc.QuerySelectorAll<HtmlBaseNode>("ul li").Count(), Is.EqualTo(doc.QuerySelectorAll<HtmlBaseNode>("ul  li").Count()));
        }
    }
}