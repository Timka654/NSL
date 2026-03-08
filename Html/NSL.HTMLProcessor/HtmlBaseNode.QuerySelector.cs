using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.HTMLProcessor
{
    // ============================================================
    // BASE HTML NODE INTERFACE (адаптируй под свой HtmlBaseNode)
    // ============================================================
    public partial class HtmlBaseNode
    {
        public IEnumerable<HtmlBaseNode> FindNodes(Func<HtmlBaseNode, bool> predicate, bool recursive)
        {
            foreach (var c in ChildNodes)
            {
                if (predicate(c))
                    yield return c;

                if (recursive)
                {
                    foreach (var sub in c.FindNodes(predicate, true))
                        yield return sub;
                }
            }
        }

        public HtmlBaseNode? NextSibling =>
            Parent == null ? null :
            Parent.ChildNodes.SkipWhile(x => x != this).Skip(1).FirstOrDefault();

        public IEnumerable<HtmlBaseNode> FollowingSiblings()
        {
            if (Parent == null)
                yield break;

            bool started = false;
            foreach (var c in Parent.ChildNodes)
            {
                if (!started)
                {
                    if (c == this)
                        started = true;
                    continue;
                }
                yield return c;
            }
        }
    }

    // ============================================================
    // TOKENIZER
    // ============================================================
    internal enum TokenType
    {
        Tag,
        Id,
        Class,
        Attr,
        Pseudo,
        Combinator,
        Comma
    }

    internal record Token(TokenType Type, string Value, string? Extra = null);

    internal static class SelectorTokenizer
    {
        public static List<Token> Tokenize(string selector)
        {
            var tokens = new List<Token>();
            int i = 0;

            selector = selector.Trim();

            while (i < selector.Length)
            {
                char c = selector[i];

                // SKIP SPACES → descendant combinator
                if (char.IsWhiteSpace(c))
                {
                    if (tokens.Last().Type == TokenType.Combinator) { ++i; continue; }

                    // Skip whitespace but DO NOT emit combinator until we know it's not followed by > + ~
                    int j = i;
                    while (j < selector.Length && char.IsWhiteSpace(selector[j])) j++;

                    // If next non-space char is a combinator – do NOT emit ' '
                    if (j < selector.Length && (selector[j] == '>' || selector[j] == '+' || selector[j] == '~'))
                    {
                        i = j; // skip whitespace entirely
                        continue;
                    }

                    // Otherwise: descendant combinator
                    tokens.Add(new Token(TokenType.Combinator, " "));
                    i = j;
                    continue;
                }

                if (c == ',')
                {
                    tokens.Add(new Token(TokenType.Comma, ","));
                    i++;
                    continue;
                }

                if (c == '>' || c == '+' || c == '~')
                {
                    tokens.Add(new Token(TokenType.Combinator, c.ToString()));
                    i++;
                    continue;
                }

                if (c == '#')
                {
                    i++;
                    int start = i;
                    while (i < selector.Length && IsNameChar(selector[i])) i++;

                    var name = selector.Substring(start, i - start);
                    tokens.Add(new Token(TokenType.Id, name));
                    continue;
                }

                if (c == '.')
                {
                    i++;
                    int start = i;
                    while (i < selector.Length && IsNameChar(selector[i])) i++;

                    var name = selector.Substring(start, i - start);
                    tokens.Add(new Token(TokenType.Class, name));
                    continue;
                }

                if (c == '[')
                {
                    int start = i;
                    i++;
                    int depth = 1;
                    while (i < selector.Length && depth > 0)
                    {
                        if (selector[i] == '[') depth++;
                        else if (selector[i] == ']') depth--;
                        i++;
                    }

                    string chunk = selector.Substring(start, i - start);
                    tokens.Add(new Token(TokenType.Attr, chunk));
                    continue;
                }

                if (c == ':')
                {
                    int start = i;
                    i++;

                    int nameStart = i;
                    while (i < selector.Length && IsNameChar(selector[i])) i++;

                    var pseudoName = selector.Substring(nameStart, i - nameStart);

                    if (i < selector.Length && selector[i] == '(')
                    {
                        string content = ReadParenthesized(selector, ref i);
                        tokens.Add(new Token(TokenType.Pseudo, pseudoName, content));
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Pseudo, pseudoName));
                    }
                    continue;
                }

                if (char.IsLetter(c) || c == '*')
                {
                    int start = i;
                    i++;
                    while (i < selector.Length && IsNameChar(selector[i])) i++;

                    var tag = selector.Substring(start, i - start);
                    tokens.Add(new Token(TokenType.Tag, tag));
                    continue;
                }

                i++;
            }

            return tokens;
        }

        private static bool IsNameChar(char c) =>
            char.IsLetterOrDigit(c) || c == '-' || c == '_';

        private static string ReadParenthesized(string s, ref int i)
        {
            int start = i + 1;
            int depth = 1;
            i++;

            while (i < s.Length && depth > 0)
            {
                if (s[i] == '(') depth++;
                else if (s[i] == ')') depth--;
                i++;
            }

            return s.Substring(start, i - start - 1);
        }
    }

    // ============================================================
    // SELECTOR STRUCTURES
    // ============================================================

    public enum Combinator
    {
        None,
        Descendant,
        Child,
        Adjacent,
        Sibling
    }

    public enum AttributeMatchMode
    {
        Exists,
        Equals,
        StartsWith,
        EndsWith,
        Contains,
        ContainsWord,
        EqualsOrStartsWith
    }

    public record AttributeSelector(string Name, string? Value, AttributeMatchMode Mode);

    public enum PseudoType
    {
        FirstChild,
        LastChild,
        OnlyChild,
        NthChild,
        NthOfType
    }

    public class PseudoSelector
    {
        public PseudoType Type;
        public int A;
        public int B;
    }

    public class SelectorStep
    {
        public Combinator Combinator = Combinator.None;

        public string? Tag;
        public string? Id;
        public List<string>? Classes;
        public List<AttributeSelector>? Attributes;

        public List<SelectorGroup>? Not;
        public List<SelectorGroup>? Is;
        public List<SelectorGroup>? Where;
        public List<SelectorGroup>? Has;

        public List<PseudoSelector>? Pseudo;
    }

    public class SelectorGroup
    {
        public List<SelectorStep> Steps = new();
    }

    // ============================================================
    // MAIN ENGINE
    // ============================================================

    public static class CssSelectorEngine
    {
        // ---------------------------------------------------------
        // PUBLIC API
        // ---------------------------------------------------------

        public static IEnumerable<T> QuerySelectorAll<T>(HtmlBaseNode root, string selector)
            where T : HtmlBaseNode
        {
            var groups = ParseSelector(selector);

            var result = new List<HtmlBaseNode>();
            foreach (var g in groups)
                result.AddRange(ApplySteps(root, g.Steps));

            return result.Where(x => x is T).Cast<T>();
        }

        public static T? QuerySelector<T>(HtmlBaseNode root, string selector)
            where T : HtmlBaseNode =>
            QuerySelectorAll<T>(root, selector).FirstOrDefault();

        // ---------------------------------------------------------
        // SELECTOR PARSER (based on tokens)
        // ---------------------------------------------------------

        private static List<SelectorGroup> ParseSelector(string selector)
        {
            var tokens = SelectorTokenizer.Tokenize(selector);
            
            var groups = new List<SelectorGroup>();

            if (tokens.LastOrDefault()?.Type == TokenType.Combinator
                || tokens.FirstOrDefault()?.Type == TokenType.Combinator)
            {
                return groups;
            }

            var currentGroup = new SelectorGroup();
            groups.Add(currentGroup);

            var currentStep = new SelectorStep();
            currentGroup.Steps.Add(currentStep);

            foreach (var t in tokens)
            {
                switch (t.Type)
                {
                    case TokenType.Comma:
                        currentGroup = new SelectorGroup();
                        groups.Add(currentGroup);

                        currentStep = new SelectorStep();
                        currentGroup.Steps.Add(currentStep);
                        break;

                    case TokenType.Combinator:
                        var next = new SelectorStep();
                        next.Combinator = t.Value switch
                        {
                            ">" => Combinator.Child,
                            "+" => Combinator.Adjacent,
                            "~" => Combinator.Sibling,
                            " " => Combinator.Descendant,
                            _ => Combinator.Descendant
                        };

                        currentStep = next;
                        currentGroup.Steps.Add(next);
                        break;

                    case TokenType.Tag:
                        currentStep.Tag = t.Value == "*" ? null : t.Value;
                        break;

                    case TokenType.Id:
                        currentStep.Id = t.Value;
                        break;

                    case TokenType.Class:
                        currentStep.Classes ??= new();
                        currentStep.Classes.Add(t.Value);
                        break;

                    case TokenType.Attr:
                        currentStep.Attributes ??= new();
                        currentStep.Attributes.Add(ParseAttribute(t.Value));
                        break;

                    case TokenType.Pseudo:
                        HandlePseudo(t, currentStep);
                        break;
                }
            }

            return groups;
        }

        private static AttributeSelector ParseAttribute(string raw)
        {
            // raw = [name=value]
            string s = raw.Trim('[', ']');

            // find operator (=, ^=, *=, ~=, $=, |=)
            string[] ops = { "^=", "$=", "*=", "~=", "|=", "=" };

            foreach (var op in ops)
            {
                int idx = s.IndexOf(op);
                if (idx != -1)
                {
                    string name = s.Substring(0, idx).Trim();
                    string value = s.Substring(idx + op.Length).Trim().Trim('"', '\'');

                    return new(name, value, op switch
                    {
                        "=" => AttributeMatchMode.Equals,
                        "^=" => AttributeMatchMode.StartsWith,
                        "$=" => AttributeMatchMode.EndsWith,
                        "*=" => AttributeMatchMode.Contains,
                        "~=" => AttributeMatchMode.ContainsWord,
                        "|=" => AttributeMatchMode.EqualsOrStartsWith,
                        _ => AttributeMatchMode.Exists
                    });
                }
            }

            return new AttributeSelector(s.Trim(), null, AttributeMatchMode.Exists);
        }

        private static void HandlePseudo(Token t, SelectorStep step)
        {
            step.Pseudo ??= new();

            switch (t.Value)
            {
                case "not":
                    step.Not ??= new();
                    step.Not.AddRange(ParseSelector(t.Extra!));
                    break;

                case "is":
                    step.Is ??= new();
                    step.Is.AddRange(ParseSelector(t.Extra!));
                    break;

                case "where":
                    step.Where ??= new();
                    step.Where.AddRange(ParseSelector(t.Extra!));
                    break;

                case "has":
                    step.Has ??= new();
                    step.Has.AddRange(ParseSelector(t.Extra!));
                    break;

                case "first-child":
                    step.Pseudo.Add(new PseudoSelector { Type = PseudoType.FirstChild });
                    break;

                case "last-child":
                    step.Pseudo.Add(new PseudoSelector { Type = PseudoType.LastChild });
                    break;

                case "only-child":
                    step.Pseudo.Add(new PseudoSelector { Type = PseudoType.OnlyChild });
                    break;

                case "nth-child":
                    step.Pseudo.Add(ParseNth(t.Extra!, PseudoType.NthChild));
                    break;

                case "nth-of-type":
                    step.Pseudo.Add(ParseNth(t.Extra!, PseudoType.NthOfType));
                    break;

                default:
                    break;
            }
        }

        // ---------------------------------------------------------
        // nth parser
        // ---------------------------------------------------------

        private static PseudoSelector ParseNth(string expr, PseudoType type)
        {
            expr = expr.Replace(" ", "");

            if (expr == "odd") return new() { Type = type, A = 2, B = 1 };
            if (expr == "even") return new() { Type = type, A = 2, B = 0 };

            if (expr.Contains("n"))
            {
                var parts = expr.Split('n');
                int a = parts[0] == "" || parts[0] == "+" ? 1 :
                        parts[0] == "-" ? -1 : int.Parse(parts[0]);

                int b = (parts.Length > 1 && parts[1] != "") ? int.Parse(parts[1]) : 0;

                return new PseudoSelector { Type = type, A = a, B = b };
            }

            return new PseudoSelector { Type = type, A = 0, B = int.Parse(expr) };
        }

        // ---------------------------------------------------------
        // APPLY STEPS
        // ---------------------------------------------------------
        private static IEnumerable<HtmlBaseNode> ApplySteps(HtmlBaseNode root, List<SelectorStep> steps)
        {
            IEnumerable<HtmlBaseNode> current = new[] { root };

            bool isFirstStep = true;

            foreach (var step in steps)
            {
                var check = BuildCheck(step);

                if (isFirstStep)
                {
                    // первый шаг — ищем элемент в любом месте документа
                    current = current.SelectMany(n => n.FindNodes(check, true)).Where(x => check.GetInvocationList().All(i => ((Func<HtmlBaseNode, bool>)i)(x)));
                    isFirstStep = false;
                    continue;
                }

                current = step.Combinator switch
                {
                    Combinator.None =>
                        current.Where(x => check.GetInvocationList().All(i => ((Func<HtmlBaseNode, bool>)i)(x))),

                    Combinator.Descendant =>
                        current.SelectMany(n => n.FindNodes(x => check.GetInvocationList().All(i => ((Func<HtmlBaseNode, bool>)i)(x)), true)),

                    Combinator.Child =>
                        current.SelectMany(n => n.ChildNodes.Where(x=>check.GetInvocationList().All(i=>((Func<HtmlBaseNode, bool>)i)(x)))),

                    Combinator.Adjacent =>
                        current.SelectMany(n =>
                            n.NextSibling != null && check.GetInvocationList().All(i => ((Func<HtmlBaseNode, bool>)i)(n.NextSibling))
                                ? new[] { n.NextSibling }
                                : Array.Empty<HtmlBaseNode>()),

                    Combinator.Sibling =>
                        current.SelectMany(n => n.FollowingSiblings().Where(x => check.GetInvocationList().All(i => ((Func<HtmlBaseNode, bool>)i)(x)))),

                    _ => current
                };
            }

            return current;
        }


        // ---------------------------------------------------------
        // BUILD CHECK FUNCTION FOR A STEP
        // ---------------------------------------------------------

        private static Func<HtmlBaseNode, bool> BuildCheck(SelectorStep step)
        {
            Func<HtmlBaseNode, bool> f = _ => true;

            if (step.Tag != null)
                f += n => string.Equals(n.NodeName, step.Tag, StringComparison.OrdinalIgnoreCase);

            if (step.Id != null)
                f += n => n.Id == step.Id;

            if (step.Classes != null)
                f += n => n.Class != null &&
                    step.Classes.All(c => n.Class.Any(cc => string.Equals( cc,c, StringComparison.OrdinalIgnoreCase)));

            if (step.Attributes != null)
                f += n => step.Attributes.All(a =>
                {
                    if (!n.TryGetAttributeValue(a.Name, out var v)) return false;

                    return a.Mode switch
                    {
                        AttributeMatchMode.Exists => true,
                        AttributeMatchMode.Equals => v == a.Value,
                        AttributeMatchMode.StartsWith => v.StartsWith(a.Value),
                        AttributeMatchMode.EndsWith => v.EndsWith(a.Value),
                        AttributeMatchMode.Contains => v.Contains(a.Value),
                        AttributeMatchMode.ContainsWord => v.Split(' ').Contains(a.Value),
                        AttributeMatchMode.EqualsOrStartsWith => v == a.Value || v.StartsWith(a.Value + "-"),
                        _ => false
                    };
                });

            if (step.Not != null)
                f += n => step.Not.All(g => !MatchSingle(n, g));

            if (step.Is != null)
                f += n => step.Is.Any(g => MatchGroup(n, g));

            if (step.Where != null)
                f += n => step.Where.Any(g => MatchGroup(n, g));

            if (step.Has != null)
                f += n => step.Has.Any(g =>
                    n.FindNodes(child => MatchGroup(child, g), true).Any());

            if (step.Pseudo != null)
                f += n => step.Pseudo.All(p => CheckPseudo(n, p));

            return f;
        }
        private static bool MatchSingle(HtmlBaseNode node, SelectorGroup group)
        {
            IEnumerable<HtmlBaseNode> current = new[] { node };

            foreach (var step in group.Steps)
            {
                var check = BuildCheck(step);

                switch (step.Combinator)
                {
                    case Combinator.None:
                        // применяем фильтр только к текущему узлу, не ходим по дереву
                        current = current.Where(check);
                        break;

                    case Combinator.Descendant:
                    case Combinator.Child:
                    case Combinator.Adjacent:
                    case Combinator.Sibling:
                        // combinator внутри :not() недопустим — такой узел не совпадает
                        return false;
                }

                if (!current.Any())
                    return false;
            }

            return true;
        }

        private static bool MatchGroup(HtmlBaseNode node, SelectorGroup group)
        {
            IEnumerable<HtmlBaseNode> current = new[] { node };

            foreach (var step in group.Steps)
            {
                var check = BuildCheck(step);

                current = step.Combinator switch
                {
                    Combinator.None =>
                        current.Where(check),

                    Combinator.Descendant =>
                        current.SelectMany(n => n.FindNodes(check, true)),

                    Combinator.Child =>
                        current.SelectMany(n => n.ChildNodes.Where(check)),

                    Combinator.Adjacent =>
                        current.Where(n => n.NextSibling != null && check(n.NextSibling))
                               .Select(n => n.NextSibling!),

                    Combinator.Sibling =>
                        current.SelectMany(n => n.FollowingSiblings().Where(check)),

                    _ => current
                };

                if (!current.Any())
                    return false;
            }

            return true;
        }

        // ---------------------------------------------------------
        // PSEUDO CHECKS
        // ---------------------------------------------------------

        private static bool CheckPseudo(HtmlBaseNode node, PseudoSelector p)
        {
            return p.Type switch
            {
                PseudoType.FirstChild =>
                    node.Parent != null && node.Parent.ChildNodes.First() == node,

                PseudoType.LastChild =>
                    node.Parent != null && node.Parent.ChildNodes.Last() == node,

                PseudoType.OnlyChild =>
                    node.Parent != null && node.Parent.ChildNodes.Count == 1,

                PseudoType.NthChild =>
                    CheckNth(ChildIndex(node), p.A, p.B),

                PseudoType.NthOfType =>
                    CheckNth(TypeIndex(node), p.A, p.B),

                _ => false
            };
        }

        private static int ChildIndex(HtmlBaseNode node) =>
            node.Parent?.ChildNodes.OrderBy(x => x.Position.Value).ToList().IndexOf(node) + 1 ?? 0;

        private static int TypeIndex(HtmlBaseNode node)
        {
            if (node.Parent == null) return 0;

            return node.Parent.ChildNodes
                .Where(n => n.NodeName == node.NodeName)
                .OrderBy(x => x.Position.Value)
                .ToList()
                .IndexOf(node) + 1;
        }

        private static bool CheckNth(int pos, int a, int b)
        {
            if (pos == 0) return false;

            if (a == 0) return pos == b;

            return (pos - b) % a == 0 && (pos - b) / a >= 0;
        }
    }
}
