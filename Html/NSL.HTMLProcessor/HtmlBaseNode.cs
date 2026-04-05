using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.HTMLProcessor
{
    internal delegate void ModifyHtmlBaseNodeDelegate(HtmlBaseNode node, int? oldPos, NodeSize oldSize);

    public partial class HtmlBaseNode
    {
        public HtmlBaseNode Parent { get; internal set; }

        public HtmlDocumentNode DocumentNode { get; internal set; }

        public IReadOnlyList<HtmlAttribute> Attributes => attributes;

        private List<HtmlAttribute> attributes = new List<HtmlAttribute>();

        public IOrderedEnumerable<HtmlBaseNode> OrderedChildNodes => childNodes.OrderBy(x => x.position);

        public IReadOnlyList<HtmlBaseNode> ChildNodes => childNodes;

        private List<HtmlBaseNode> childNodes = new();

        private string? nodeName;
        private int? position;
        private NodeSize size;

        public string InnerHtmlContent { get; internal set; }

        public string? NodeName { get => nodeName; set { nodeName = value; calculatePosition(); } }

        public virtual bool HasBody { get; set; }

        public virtual bool AllowSpecialCharacters { get; } = false;

        public virtual string? CloseTag { get; } = default;

        public string? Id => GetAttributeValue("id");

        public string[]? Class => getClass();

        private string[]? getClass()
        {
            if (!TryGetAttributeValue("class", out var cls))
                return null;

            return cls.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }

        public virtual int? Position
        {
            get => position; internal set
            {
                if (position == value)
                    return;

                var p = position;
                position = value;
                OnModified(this, p, size);
            }
        }

        public virtual NodeSize Size
        {
            get => size; internal set
            {
                if (size.Equals(value))
                    return;

                var s = size;
                size = value;
                OnModified(this, position, s);
            }
        }

        internal event ModifyHtmlBaseNodeDelegate OnModified = (e, p, s) => { };

        internal void parseSetPosition(int pos)
            => Position = pos;

        public int? DocumentOuterStartOffset => (Parent?.DocumentInnerStartOffset ?? 0) + Position;

        public int? DocumentOuterEndOffset => DocumentOuterStartOffset + Size.OuterEndOffset;

        public int? DocumentInnerStartOffset => HasBody ? DocumentOuterStartOffset + Size.OpenNodeLength : null;

        public int? DocumentInnerEndOffset => DocumentInnerStartOffset + Size.InnerContentLength;

        public virtual bool CanHaveAttributes => true;

        internal void RecalculateSize() => calculatePosition();

        private void calculatePosition()
        {
            // Sum children sizes arithmetically — no HTML strings built
            var contentLen = 0;
            foreach (var child in childNodes)
                contentLen += child.Size.TotalLength;

            var openNodeLen = 0;
            var closeNodeLen = 0;

            if (!string.IsNullOrEmpty(NodeName))
            {
                // Each attribute contributes its size + 1 leading space
                // e.g. <div class="a" id="b"> → " class=\"a\" id=\"b\"" = sum(sizes) + count
                var attrsTotal = attributes.Count > 0
                    ? attributes.Sum(x => x.Size) + attributes.Count
                    : 0;

                if (HasBody)
                {
                    openNodeLen  = NodeName.Length + attrsTotal + 2; // <name attrs>
                    closeNodeLen = NodeName.Length + 3;              // </name>
                }
                else
                {
                    openNodeLen = NodeName.Length + attrsTotal + 4;  // <name attrs />
                }
            }

            Size = new NodeSize()
            {
                OpenNodeLength   = openNodeLen,
                CloseNodeLength  = closeNodeLen,
                InnerContentLength = contentLen
            };
        }


        public void Remove()
            => Parent?.RemoveChildNode(this);


        #region ChildNodes

        public void RemoveChildNode(HtmlBaseNode node)
        {
            if (node.Parent != this)
                return;

            childNodes.Remove(node);

            node.OnModified -= Node_OnModified;

            moveNodes(-node.Size.TotalLength, node.Position.Value, null);

            node.Parent = null;

            node.Position = null;
        }

        /// <summary>
        /// for parse
        /// </summary>
        /// <param name="node"></param>
        internal void parseAppendChildNode(HtmlBaseNode node)
        {
            node.Parent = this;

            // O(1): read last child's position+size instead of summing all children
            node.Position = childNodes.Count > 0
                ? childNodes[childNodes.Count - 1].Position!.Value + childNodes[childNodes.Count - 1].Size.TotalLength
                : 0;

            childNodes.Add(node);

            node.OnModified += Node_OnModified;
        }

        private int GetCurrentOffset()
            => childNodes.Count > 0
                ? childNodes[childNodes.Count - 1].Position!.Value + childNodes[childNodes.Count - 1].Size.TotalLength
                : 0;

        public void PrependChildNode(HtmlBaseNode node)
        {
            AddChildNode(node, 0);
        }

        public void AddChildNode(HtmlBaseNode node)
        {
            AddChildNode(node, GetCurrentOffset());
        }

        public void ReplaceChildNode(HtmlBaseNode node, HtmlBaseNode forReplace)
        {
            if (forReplace.Parent != this)
                throw new InvalidOperationException();

            var pos = forReplace.Position;

            RemoveChildNode(forReplace);

            AddChildNode(node, pos.Value);
        }

        public void AddChildNodeAfter(HtmlBaseNode node, HtmlBaseNode after)
        {
            if (after.Parent != this)
                throw new InvalidOperationException();

            AddChildNode(node, after.Position.Value + after.Size.TotalLength);
        }

        public void AddChildNodeBefore(HtmlBaseNode node, HtmlBaseNode before)
        {
            if (before.Parent != this)
                throw new InvalidOperationException();

            AddChildNode(node, before.Position.Value);
        }

        private void AddChildNode(HtmlBaseNode node, int startOffset)
        {
            if (ChildNodes.Contains(node))
                return;

            if (node.Parent != null)
                node.Parent.RemoveChildNode(node);

            if (!ChildNodes.Any())
                HasBody = true;

            node.Parent = this;

            node.Position = startOffset;

            childNodes.Add(node);

            moveNodes(node.Size.TotalLength, startOffset, node);
            node.OnModified += Node_OnModified;
        }

        private void Node_OnModified(HtmlBaseNode node, int? oldPos, NodeSize oldSize)
        {
            if (node.Position != oldPos) return;

            moveNodes(node.size.OuterEndOffset - oldSize.OuterEndOffset, node.position.Value, node);
        }

        private void moveNodes(int offset, int startFrom, HtmlBaseNode except)
        {
            bool any = false;

            foreach (var item in ChildNodes.Where(x => x != except && x.Position >= startFrom))
            {
                any = true;

                item.Position += offset;
            }

            if (any)
                calculatePosition();
        }

        #endregion

        #region Attributes

        public HtmlAttribute FindAttribute(string key)
        {
            return Attributes.FirstOrDefault(x => x.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
        }

        public HtmlAttribute SetAttributeValue(string key, string? value)
        {
            var attr = FindAttribute(key);

            if (attr == null)
            {
                AddAttribute(attr = new HtmlAttribute() { Key = key, Value = value });
                return attr;
            }

            attr.Value = value;

            return attr;
        }

        public string? GetAttributeValue(string key, string defaultValue = null)
            => FindAttribute(key)?.Value ?? defaultValue;

        public TValue? GetAttributeValue<TValue>(string key)
            where TValue : IConvertible
        {
            var str = GetAttributeValue(key);
            if (str == null) return default;
            return (TValue)Convert.ChangeType(str, typeof(TValue));
        }

        public bool TryGetAttributeValue(string key, out string? value)
        {
            var attr = FindAttribute(key);
            if (attr == null)
            {
                value = default;
                return false;
            }

            value = attr.Value;

            return true;
        }

        private void moveAttributes(int offset, int startFrom, HtmlAttribute except)
        {
            foreach (var item in Attributes.Where(x => x != except && x.Position >= startFrom))
            {
                item.Position += offset;
            }

            calculatePosition();
        }

        public void RemoveAttribute(HtmlAttribute currentAttribute)
        {
            if (currentAttribute.Parent != this)
                return;

            currentAttribute.Parent = null;

            attributes.Remove(currentAttribute);

            moveAttributes(-(currentAttribute.Size + 1), currentAttribute.Position.Value, null);

            currentAttribute.Position = null;

            currentAttribute.OnModified -= CurrentAttribute_OnModified;
        }

        internal void _AppendAttribute(HtmlAttribute currentAttribute)
        {
            currentAttribute.Parent = this;

            attributes.Add(currentAttribute);

            currentAttribute.OnModified += CurrentAttribute_OnModified;
        }

        private void CurrentAttribute_OnModified(HtmlAttribute node, int? oldPos, int oldSize)
        {
            calculatePosition();
        }

        public virtual void AddAttribute(HtmlAttribute currentAttribute)
        {
            currentAttribute.Parent?.RemoveAttribute(currentAttribute);

            currentAttribute.Parent = this;

            currentAttribute.Position = (Attributes.Any() ? Attributes.Max(x => x.Position + x.Size) : (NodeName.Length + 1)) + 1;

            attributes.Add(currentAttribute);

            calculatePosition();

            currentAttribute.OnModified += CurrentAttribute_OnModified;
        }

        #endregion


        /// <summary>
        /// For create instance try use builder
        /// </summary>
        public HtmlBaseNode()
        {

        }

        public string GetInnerHtml(bool saveSourceOffsets)
        {
            var content = string.Concat(ChildNodes.OrderBy(x => x.Position).Select(x => x.BuildHtml(saveSourceOffsets)));
            if (!saveSourceOffsets)
            {
                var size = Size;
                size.InnerContentLength = content.Length;
                Size = size;
            }

            return content;
        }

        public string GetOuterHtml(bool saveSourceOffsets)
        {
            int? openElementLen = null;
            int? contentLen = null;
            int? closeElementLen = null;

            var sb = new StringBuilder();

            sb.Append('<');

            sb.Append(NodeName);

            if (Attributes.Any())
            {
                sb.Append(" ");

                sb.Append(string.Join(' ', Attributes.OrderBy(x => x.Position).Select(x => x.BuildHtml(saveSourceOffsets))));
            }

            if (HasBody)
            {
                sb.Append($">");

                if (!saveSourceOffsets)
                    openElementLen = sb.Length;

                sb.Append(GetInnerHtml(saveSourceOffsets));

                if (!saveSourceOffsets)
                    contentLen = sb.Length - openElementLen;

                sb.Append($"</{NodeName}>");

                if (!saveSourceOffsets)
                    closeElementLen = sb.Length - openElementLen - contentLen;
            }
            else
            {
                sb.Append(" />");

                if (!saveSourceOffsets)
                    openElementLen = sb.Length;
            }

            if (!saveSourceOffsets)
            {
                var size = Size;
                size.OpenNodeLength = openElementLen ?? 0;
                size.InnerContentLength = contentLen ?? 0;
                size.CloseNodeLength = closeElementLen ?? 0;
                Size = size;
            }

            return sb.ToString();
        }

        public virtual string BuildHtml(bool saveSourceOffsets = true)
            => GetOuterHtml(saveSourceOffsets);

        public override string ToString()
            => $"{NodeName}({GetType().Name})";

        public T? QuerySelector<T>(string querySelector)
            where T : HtmlBaseNode
            => CssSelectorEngine.QuerySelectorAll<T>(this, querySelector).FirstOrDefault();

        public IEnumerable<T> QuerySelectorAll<T>(string querySelector)
            where T : HtmlBaseNode
            => CssSelectorEngine.QuerySelectorAll<T>(this, querySelector);

        private record SearchQueryAttributeData(string? name, string? value);

    }
}
