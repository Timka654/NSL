using System;
using System.Collections.Generic;

namespace NSL.HTMLProcessor
{
    internal partial class HtmlParser<TElement>
        where TElement : HtmlBaseNode, new()
    {
        public static Dictionary<string, Func<ParseItem, HtmlBaseNode>> KnowableTags = new()
        {
            { "html", (item) => new HtmlDocumentNode()  },
            { "!doctype", (item) => new HtmlDoctypeNode() },
            { "link", (item) => new HtmlLinkNode() },
            { "meta", (item) => new HtmlMetaNode() },
            { "img", (item) => new HtmlImgNode() },
            { "script", (item) => new HtmlScriptNode() },
            { "col", (item) => new HtmlTableColNode() },
            { "br", (item) => new HtmlBrNode() },
            { "input", (item) => new HtmlInputNode() },
        };

        string content = default;
        HtmlParserOptions options = default!;

        ReadOnlySpan<char> contentSpan => content.AsSpan();

        private int BeginReadHtml(ParseItem parent, int start)
        {
            var childrenFill = parent.EnclosedItems != null;


            var contentOffset = start;

            while (contentOffset < content.Length) // eof
            {
                var c = content[contentOffset++];

                if (c == '<')
                {
                    if (content[contentOffset] == '/')
                    {
                        if (contentOffset - start > 1) AppendTextContentNode(parent, start, contentOffset - 1);
                        if (!childrenFill)
                        {
                            var newContentOffset = TryCloseElement(parent, contentOffset - 1, false);

                            if (newContentOffset == contentOffset)
                                continue;

                            return newContentOffset;
                        }

                        if (parent.EnclosedItems?.TryPop(out var enclosedItem) == true)
                        {
                            var newContentOffset = TryCloseElement(enclosedItem, contentOffset - 1, false);

                            if (newContentOffset == contentOffset)
                                continue;

                            parent.Node.parseAppendChildNode(enclosedItem.Node);

                            start = newContentOffset;
                        }

                        // Skip the closing-tag content — do not fall through to element/comment handling
                        continue;
                    }

                    if (!parent.Node.AllowSpecialCharacters)
                    {
                        if (contentOffset - start > 1) AppendTextContentNode(parent, start, contentOffset - 1);

                        // Check for HTML comment: <!--
                        if (contentOffset + 2 < content.Length
                            && content[contentOffset]     == '!'
                            && content[contentOffset + 1] == '-'
                            && content[contentOffset + 2] == '-')
                        {
                            contentOffset = BeginReadComment(parent, contentOffset + 3); // skip past "!--"
                        }
                        else
                        {
                            contentOffset = BeginReadElement(parent, contentOffset);
                        }

                        start = contentOffset;

                        continue;
                    }
                }

                if (contentSpan[start..contentOffset].EndsWith("!--") && !parent.Node.AllowSpecialCharacters)
                {
                    // dead code path — comments are handled above via <!-- detection
                }
            }

            return contentOffset;
        }

        private int TryCloseElement(ParseItem item, int start, bool closed)
        {
            var contentOffset = start;

            if (!closed)
            {
                while (contentOffset < content.Length) // eof
                {
                    var c = content[contentOffset++];

                    if (c == '>')
                    {
                        break;
                    }
                }
                var t = contentSpan[start..contentOffset].ToString();
                var tag = contentSpan[(start + 2)..(contentOffset - 1)].ToString();

                var closeTag = item.Node.CloseTag ?? item.Node.NodeName;

                if (tag != closeTag)
                {
                    if (options.SkipMissedOpenedTag) return contentOffset;

                    throw new Exception($"Invalid syntax (offset: {contentOffset}, line: {contentSpan[..contentOffset].Count('\n')}, position: {start}). Cannot close element \"{item.Node.NodeName}\" - not found start node");
                }
            }

            item.InnerEndOffset = start;
            item.OuterEndOffset = contentOffset;

            item.Node.Size = new NodeSize()
            {
                InnerContentLength = (item.InnerEndOffset - item.InnerStartOffset).Value,
                OpenNodeLength = (item.InnerStartOffset - item.OuterStartOffset).Value,
                CloseNodeLength = (item.OuterEndOffset - item.InnerEndOffset).Value,
            };

            return contentOffset;
        }

        private int BeginReadElement(ParseItem parent, int start)
        {

            var contentOffset = start;

            var currentItem = new ParseItem() { OuterStartOffset = start - 1 };


            bool haveAttributes = false;

            // read name
            while (contentOffset < content.Length) // eof
            {
                var c = content[contentOffset++];

                if (c == '>')
                {
                    break;
                }

                if (char.IsWhiteSpace(c))
                {
                    haveAttributes = true;
                    break;
                }
            }

            var closeOffset = contentSpan[contentOffset - 2] == '/' ? 2 : 1;

            var tag = contentSpan[start..(contentOffset - closeOffset)].ToString().ToLower();

            if (KnowableTags.TryGetValue(tag, out var kta))
                currentItem.Node = kta(currentItem);
            else
                currentItem.Node = new HtmlBaseNode();

            currentItem.Node.NodeName = tag;

            if (haveAttributes)
            {
#if DEBUG
                var prev = contentOffset;
#endif

                contentOffset = ReadAttributes(currentItem, contentOffset);
            }

            currentItem.Node.Size = new NodeSize()
            {
                InnerContentLength = 0,
                OpenNodeLength = contentOffset - start + 1,
                CloseNodeLength = 0,
            };

            currentItem.InnerStartOffset = contentOffset;

            if (!(content[contentOffset - 2] == '/' && content[contentOffset - 1] == '>'))
            {
                currentItem.Node.HasBody = true;

                // Always call BeginReadHtml even if HasBody setter was a no-op (void elements
                // like <input> may still be followed by a spurious closing tag e.g. </input>).
                contentOffset = BeginReadHtml(currentItem, contentOffset);

                // If HasBody was reset to false (void element with no-op setter), the spurious
                // closing tag made TryCloseElement overwrite the size. Recalculate correctly.
                if (!currentItem.Node.HasBody)
                    currentItem.Node.RecalculateSize();
            }

            currentItem.InnerEndOffset = contentOffset;

            if (currentItem.InnerStartOffset == currentItem.InnerEndOffset)
            {
                var size = currentItem.Node.Size;

                size.InnerContentLength = (currentItem.InnerEndOffset - currentItem.InnerStartOffset).Value;

                currentItem.Node.Size = size;
            }

            parent.Node.parseAppendChildNode(currentItem.Node);


            return contentOffset;
        }

        private int ReadAttributes(ParseItem item, int start)
        {

            var contentOffset = start;

            char c = default;

            while (contentOffset < content.Length) // eof
            {
                var nameStart = contentOffset;
                var fchar = false;
                //name
                while (contentOffset < content.Length) // eof
                {
                    c = content[contentOffset++];

                    if (!fchar && !char.IsWhiteSpace(c))
                    {
                        nameStart = contentOffset - 1;
                        fchar = true;
                    }

                    if (c == '>')
                    {
                        var coffset = (content[contentOffset - 2] == '/' ? 2 : 1);

                        if (content[contentOffset - 2] != '/')
                        {
                            item.Node.HasBody = true;
                        }

                        AppendAttribute(item, nameStart, contentOffset, coffset);

                        return contentOffset;
                    }

                    if (fchar && (c == '=' || char.IsWhiteSpace(c)))
                    {
                        break;
                    }
                }

                var nameEnd = contentOffset - 1;

#if DEBUG
                var attrName = content[nameStart..nameEnd];
#endif


                bool finish = false;

                if (char.IsWhiteSpace(c))
                {
                    while (contentOffset < content.Length)
                    {
                        c = content[contentOffset++];
                        if (char.IsWhiteSpace(c))
                            continue;

                        if (c != '=')
                        {
                            contentOffset -= 2;
                            finish = true;
                            break;
                        }
                        else
                            break;
                    }
                }


                var valueStart = contentOffset;

                var quoteValue = HtmlAttributeQuoteType.None;

                bool hv = false;
                //value
                while (contentOffset < content.Length) // eof
                {

                    if (finish)
                    {
                        break;
                    }

                    c = content[contentOffset++];

                    if (!hv && !char.IsWhiteSpace(c))
                    {
                        hv = true;
                        valueStart = contentOffset - 1;

                        quoteValue = c switch
                        {
                            '"' => HtmlAttributeQuoteType.Double,
                            '\'' => HtmlAttributeQuoteType.Single,
                            _ => HtmlAttributeQuoteType.None,
                        };

                        if (quoteValue != HtmlAttributeQuoteType.None) continue;
                    }

                    finish = c switch
                    {
                        '>' => true,
                        '"' when quoteValue == HtmlAttributeQuoteType.Double => true,
                        '\'' when quoteValue == HtmlAttributeQuoteType.Single => true,
                        ' ' when quoteValue == HtmlAttributeQuoteType.None => true,
                        '\t' when quoteValue == HtmlAttributeQuoteType.None => true,
                        '\n' when quoteValue == HtmlAttributeQuoteType.None => true,
                        _ => false,
                    };
                }

                if (valueStart == contentOffset)
                    AppendAttribute(item, nameStart, nameEnd, 0);
                else
                {
                    var coffset = (c == '>' && content[contentOffset - 2] == '/' ? 2 : 1);

                    AppendAttribute(item, nameStart, contentOffset, coffset, valueStart, nameEnd, quoteValue);

                    if (c == '>') return contentOffset;
                }

                //contentOffset++;


            }

            return contentOffset;
        }

        private void AppendAttribute(ParseItem item, int astart, int aend, int coffset)
        {
            var keyeoff = aend - coffset;

            if (keyeoff == astart) return;

            item.Node._AppendAttribute(new HtmlAttribute()
            {
                Key = contentSpan[astart..keyeoff].ToString(),
                Position = astart,
                Size = aend - astart - coffset,
                HasQuote = HtmlAttributeQuoteType.None,
                Parent = item.Node,
            });
        }

        private void AppendAttribute(ParseItem item, int astart, int aend, int coffset, int vstart, int nend, HtmlAttributeQuoteType quote)
        {
            if (quote > HtmlAttributeQuoteType.None) aend += 1;

            var value = contentSpan[vstart..(aend - coffset)].ToString();

            if (quote > HtmlAttributeQuoteType.None)
                value = value[1..^1];

            item.Node._AppendAttribute(new HtmlAttribute()
            {
                Key = contentSpan[astart..nend].ToString(),
                Position = astart,
                Size = aend - astart - coffset,
                Value = value,
                HasQuote = quote
            });
        }
        private void AppendTextContentNode(ParseItem item, int from, int to)
        {
            var content = contentSpan[from..to].ToString();

            if (options.SkipEmptyTextNodes && string.IsNullOrWhiteSpace(content)) return;

            item.Node.parseAppendChildNode(new HtmlTextNode()
            {
                InnerHtmlContent = content,
                Position = from,
                Size = new NodeSize() { InnerContentLength = to - from },
            });
        }

        private int BeginReadComment(ParseItem item, int start)
        {
            // start is positioned right after "<!--"
            // Scan for "-->" using O(1) char comparison per step
            var contentOffset = start;

            while (contentOffset + 2 < content.Length)
            {
                if (content[contentOffset] == '-' && content[contentOffset + 1] == '-' && content[contentOffset + 2] == '>')
                {
                    var commentText = contentSpan[start..contentOffset].ToString();
                    contentOffset += 3; // consume "-->"

                    item.Node.parseAppendChildNode(new HtmlCommentNode()
                    {
                        InnerHtmlContent = commentText,
                        Size = new NodeSize() { OpenNodeLength = 4, CloseNodeLength = 3, InnerContentLength = commentText.Length }
                    });

                    return contentOffset;
                }

                contentOffset++;
            }

            return content.Length;
        }

        public TElement parseDoc(string content, HtmlParserOptions options)
        {
            var rootNode = new TElement() { };

            var rootItem = new ParseItem() { Node = rootNode, InnerStartOffset = 0 };

            this.options = options;
            this.content = content.Replace("\r\n", "\n");

            BeginReadHtml(rootItem, 0);

            return rootNode;

        }
    }

    internal class ParseItem
    {
        public int OuterStartOffset { get; set; }

        public int OuterEndOffset { get; set; }


        public int? InnerStartOffset { get; set; }

        public int? InnerEndOffset { get; set; }

        public HtmlBaseNode Node { get; set; }

        public Stack<ParseItem>? EnclosedItems { get; set; }
    }

    public class HtmlParser
    {
        public static TElement Parse<TElement>(string content, HtmlParserOptions options = null)
            where TElement : HtmlBaseNode, new()
        {
            var parser = new HtmlParser<TElement>();

            return parser.parseDoc(content, options ?? HtmlParserOptions.Instance);
        }
    }
}
