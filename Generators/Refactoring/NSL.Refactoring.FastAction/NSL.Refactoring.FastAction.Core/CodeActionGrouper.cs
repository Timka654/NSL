using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NSL.Refactoring.FastAction.Core
{
    internal static class CodeActionGrouper
    {
        static readonly string[] GroupSeparator = { "\\\\\\" };
        public static IEnumerable<CodeAction> GroupActions(IEnumerable<RefactoringAction> actions)
        {
            var tree = new GroupNode(string.Empty);

            foreach (var item in actions.GroupBy(x => string.Join(GroupSeparator[0], x.Path.Split(GroupSeparator, StringSplitOptions.RemoveEmptyEntries).SkipLast(1))))
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    tree.Actions.AddRange(item.Select(x => PreviewedCodeAction.Create(x.Path, x.Action)).Cast<CodeAction>());
                    continue;
                }

                var path = item.Key.Split(GroupSeparator, StringSplitOptions.RemoveEmptyEntries);

                var s = string.Empty;

                var i = 0;

                GroupNode n;

                do
                {
                    s += $"{path[i]}{GroupSeparator[0]}";
                    n = tree.Children.GetOrCreate(path[i++], () => new GroupNode(path[i - 1]));
                } while (path.Length > i);

                n.Actions.AddRange(item.Select(x => PreviewedCodeAction.Create(x.Path.Substring(s.Length), x.Action)).Cast<CodeAction>());
            }

            return BuildCodeActions(tree);
        }

        static IEnumerable<TElement> SkipLast<TElement>(this IEnumerable<TElement> e, int len)
        => e.Take(e.Count() - len);

        static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, Func<TValue> create)
        {
            if (d.TryGetValue(key, out var v))
                return v;

            v = create();

            d.Add(key, v);

            return v;
        }

        private class GroupNode
        {
            public Dictionary<string, GroupNode> Children { get; } = new Dictionary<string, GroupNode>();

            public List<CodeAction> Actions { get; } = new List<CodeAction>();

            public string Name { get; }

            public GroupNode(string name)
            {
                Name = name;
            }
        }


        private static IEnumerable<CodeAction> BuildCodeActions(GroupNode node)
        {
            List<CodeAction> result = new List<CodeAction>();

            if (node.Children.Any())
            {
                foreach (var item in node.Children)
                {
                    var group = CodeAction.Create(
                            item.Key,
                            ImmutableArray.Create(new ReadOnlySpan<CodeAction>(BuildCodeActions(item.Value).ToArray())), true
                            );

                    result.Add(group);
                }
            }

            if (node.Actions.Any())
                result.AddRange(node.Actions.ToArray());

            return result;
        }
    }
}