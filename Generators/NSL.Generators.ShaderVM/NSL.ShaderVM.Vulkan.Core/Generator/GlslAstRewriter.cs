using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.ShaderVM.Vulkan
{
    public partial class VulkanSourceGenerator
    {
    /// <summary>
    /// Phase 3: Rewrites C# AST to GLSL-compatible syntax.
    /// Handles: type mapping, identifier rewriting (uniforms/params/buffers),
    /// cast expressions, object creation, literal cleanup, attribute stripping.
    /// 
    /// Does NOT handle inline expansion or dependency collection — those are
    /// handled by InlineExpander (Phase 2) and DependencyCollector (Phase 1).
    /// </summary>
    private class GlslAstRewriter : CSharpSyntaxRewriter
    {
        private readonly ShaderBuildPlan _plan;
        private readonly HashSet<string> _uniforms;
        private readonly HashSet<string> _pushConstants;
        private readonly Dictionary<string, string> _bufferMembers;
        private readonly SemanticModel _semanticModel;
        private readonly string _targetVersion;
        private readonly HashSet<string> _removedParamNames;
        private readonly Dictionary<string, ITypeSymbol> _removedParamTypes;
        private readonly List<string> _debugLog;

        public GlslAstRewriter(
            ShaderBuildPlan plan,
            HashSet<string> uniforms,
            HashSet<string> pushConstants,
            Dictionary<string, string> bufferMembers,
            SemanticModel semanticModel,
            string targetVersion = null,
            HashSet<string> removedParamNames = null,
            List<string> debugLog = null,
            Dictionary<string, ITypeSymbol> removedParamTypes = null)
        {
            _plan = plan;
            _uniforms = uniforms ?? new HashSet<string>();
            _pushConstants = pushConstants ?? new HashSet<string>();
            _bufferMembers = bufferMembers ?? new Dictionary<string, string>();
            _semanticModel = semanticModel;
            _targetVersion = targetVersion ?? ShaderTargetVersion.Default;
            _removedParamNames = removedParamNames ?? new HashSet<string>();
            _removedParamTypes = removedParamTypes ?? new Dictionary<string, ITypeSymbol>();
            _debugLog = debugLog;
        }

        public IReadOnlyList<string> DebugLog => _debugLog?.AsReadOnly();

        private void Log(string msg) => _debugLog?.Add(msg);

        /// <summary>Safe wrapper — GetTypeInfo fails if node is not in the semantic model's syntax tree
        /// (e.g., after InlineExpander creates synthetic nodes).</summary>
        private TypeInfo SafeGetTypeInfo(SyntaxNode node)
        {
            try { return _semanticModel.GetTypeInfo(node); }
            catch { return default; }
        }

        /// <summary>Safe wrapper — GetSymbolInfo fails if node is not in the semantic model's syntax tree.</summary>
        private SymbolInfo SafeGetSymbolInfo(SyntaxNode node)
        {
            try { return _semanticModel.GetSymbolInfo(node); }
            catch { return default; }
        }

        // ══════════════════════════════════════════════════════════════
        // Identifier rewriting: uniform.x → params.x, param → pc.x
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            string name = node.Identifier.Text;
            if (_uniforms.Contains(name))
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("params"), node);
            if (_pushConstants.Contains(name))
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("pc"), node);
            if (_bufferMembers.ContainsKey(name)) return node;
            return base.VisitIdentifierName(node);
        }

        // ══════════════════════════════════════════════════════════════
        // Method call resolution: look up in plan.MethodNameMap
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // 1. Direct call: MethodName(args)
            if (node.Expression is IdentifierNameSyntax idName)
            {
                string csName = idName.Identifier.Text;

                // Try simple name first (same-class methods registered with simple key)
                if (_plan.MethodNameMap.TryGetValue(csName, out string glslName))
                {
                    var args = (ArgumentListSyntax)Visit(node.ArgumentList);
                    return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(glslName), args);
                }

                // Try qualified key via semantic model (cross-class: ShaderLib.Sqrt)
                var sym = SafeGetSymbolInfo(idName).Symbol as IMethodSymbol;
                if (sym != null)
                {
                    string qKey = (sym.ContainingType?.Name ?? "") + "." + sym.Name;
                    if (_plan.MethodNameMap.TryGetValue(qKey, out glslName))
                    {
                        var args = (ArgumentListSyntax)Visit(node.ArgumentList);
                        return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(glslName), args);
                    }
                    // Core function from external assembly — resolve from [ShaderFunction] attribute
                    glslName = ResolveGlslFunctionName(sym);
                    if (glslName != null)
                    {
                        var args = (ArgumentListSyntax)Visit(node.ArgumentList);
                        return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(glslName), args);
                    }
                }
            }

            // 2. Member access: ClassName.MethodName(args) or instance.Method(args)
            if (node.Expression is MemberAccessExpressionSyntax ma)
            {
                var kind = ResolveMemberKind(ma);
                if (kind.GlslName != null)
                {
                    var args = (ArgumentListSyntax)Visit(node.ArgumentList);
                    return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(kind.GlslName), args);
                }
            }

            return base.VisitInvocationExpression(node);
        }

        // ══════════════════════════════════════════════════════════════
        // Member access: ctx.GlobalInvocationIdX → gl_GlobalInvocationID.x
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // Skip if parent is an invocation — handled by VisitInvocationExpression
            if (node.Parent is InvocationExpressionSyntax)
                return base.VisitMemberAccessExpression(node);

            var kind = ResolveMemberKind(node);
            if (kind.GlslName != null)
            {
                return SyntaxFactory.ParseExpression(kind.GlslName);
            }

            return base.VisitMemberAccessExpression(node);
        }

        // ══════════════════════════════════════════════════════════════
        // Type mapping
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var typeInfo = SafeGetTypeInfo(node.Type);
            if (typeInfo.Type != null)
            {
                if (IsIgnoredType(typeInfo.Type))
                    return null;
                if (IsShaderTypeBelowMinVersion(typeInfo.Type))
                    return null;
            }

            // GLSL constructors don't use 'new': vec3(1,2,3) not new vec3(1,2,3)
            string typeName = node.Type.ToString();
            string glslType = typeInfo.Type != null
                ? ResolveGlslType(typeInfo.Type, typeName)
                : GlslTypeMapper.MapType(typeName);

            var args = (ArgumentListSyntax)base.Visit(node.ArgumentList);
            if (args == null)
                return SyntaxFactory.IdentifierName(glslType);
            var identifiers = args.Arguments
                .Select(a => a.Expression)
                .Where(e => e != null)
                .Select(SyntaxFactory.Argument)
                .ToArray();
            var glslArgs = identifiers.Length == 0
                ? SyntaxFactory.ArgumentList()
                : SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(identifiers));
            return SyntaxFactory.InvocationExpression(SyntaxFactory.IdentifierName(glslType), glslArgs);
        }

        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var typeInfo = SafeGetTypeInfo(node.Type);
            string glslType = typeInfo.Type != null
                ? ResolveGlslType(typeInfo.Type, node.Type.ToString())
                : GlslTypeMapper.MapType(node.Type.ToString());
            var expr = (ExpressionSyntax)Visit(node.Expression);
            if (expr == null) return null;
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(glslType),
                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(expr))));
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var visited = (VariableDeclarationSyntax)base.VisitVariableDeclaration(node);
            if (visited == null) return null;

            string mappedType;
            if (node.Type.IsVar)
            {
                var typeInfo = SafeGetTypeInfo(node.Type);
                if (typeInfo.Type != null)
                    mappedType = ResolveGlslType(typeInfo.Type, typeInfo.Type.Name);
                else
                    return visited;
            }
            else
            {
                var typeInfo = SafeGetTypeInfo(node.Type);
                mappedType = typeInfo.Type != null
                    ? ResolveGlslType(typeInfo.Type, node.Type.ToString())
                    : GlslTypeMapper.MapType(node.Type.ToString());
            }

            if (mappedType != visited.Type.ToString())
                return visited.WithType(SyntaxFactory.ParseTypeName(mappedType));

            return visited;
        }

        // ══════════════════════════════════════════════════════════════
        // Literal cleanup: strip C# numeric suffixes
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                string cleaned = StripNumericSuffix(node.Token.Text);
                if (cleaned != node.Token.Text)
                    return SyntaxFactory.LiteralExpression(node.Kind(), SyntaxFactory.ParseToken(cleaned));
            }
            return base.VisitLiteralExpression(node);
        }

        // ══════════════════════════════════════════════════════════════
        // Parameter / Attribute stripping
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitParameterList(ParameterListSyntax node)
        {
            if (_removedParamNames.Count == 0)
                return base.VisitParameterList(node);
            var filtered = node.Parameters.Where(p =>
                !_removedParamNames.Contains(p.Identifier.Text)).ToArray();
            if (filtered.Length == node.Parameters.Count) return base.VisitParameterList(node);
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(filtered));
        }

        public override SyntaxNode VisitAttributeList(AttributeListSyntax node)
        {
            if (node.Attributes.Any(a => IsAttribute(a, "ShaderIgnore"))) return null;
            var filtered = node.Attributes.Where(a => !IsAnyAttribute(a)).ToArray();
            return filtered.Length == 0 ? null : SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(filtered));
        }

        // ══════════════════════════════════════════════════════════════
        // Removed symbol detection (ShaderIgnore context parameters)
        // ══════════════════════════════════════════════════════════════

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            var visitedExpr = Visit(node.Expression);
            if (visitedExpr == null) return null;
            if (visitedExpr is StatementSyntax stmt) return stmt;

            var rewritten = node.WithExpression((ExpressionSyntax)visitedExpr);
            if (UsesRemovedSymbol(rewritten)) return null;
            if (rewritten.Expression == null) return null;

            // Remove orphaned inline temp vars used as standalone statements (_inlN_r;)
            if (rewritten.Expression is IdentifierNameSyntax id &&
                id.Identifier.Text.StartsWith("_inl") && id.Identifier.Text.EndsWith("_r"))
                return null;

            return rewritten;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            var stmts = new SyntaxList<StatementSyntax>();
            bool anyChanged = false;
            foreach (var stmt in node.Statements)
            {
                var visited = (StatementSyntax)Visit(stmt);
                if (visited != null)
                {
                    stmts = stmts.Add(visited);
                    if (!ReferenceEquals(visited, stmt))
                        anyChanged = true;
                }
                else
                {
                    anyChanged = true; // statement removed
                }
            }
            // If nothing changed, return original to preserve syntax tree links
            if (!anyChanged && stmts.Count == node.Statements.Count)
                return node;
            return node.WithStatements(stmts);
        }

        public override SyntaxNode VisitForStatement(ForStatementSyntax node)
        {
            var body = (StatementSyntax)Visit(node.Statement);
            if (body == null) body = SyntaxFactory.Block();

            var condition = node.Condition != null ? (ExpressionSyntax)Visit(node.Condition) : null;

            // If nothing changed, return original to preserve syntax tree links
            bool bodyUnchanged = ReferenceEquals(body, node.Statement);
            bool condUnchanged = condition == null || ReferenceEquals(condition, node.Condition);
            if (bodyUnchanged && condUnchanged)
                return node;

            var newFor = node;
            if (!bodyUnchanged)
                newFor = newFor.WithStatement(body);
            if (condition != null && !condUnchanged)
                newFor = newFor.WithCondition(condition);
            return newFor;
        }

        public override SyntaxNode VisitWhileStatement(WhileStatementSyntax node)
        {
            var condition = (ExpressionSyntax)Visit(node.Condition);
            var body = (StatementSyntax)Visit(node.Statement);
            if (body == null) body = SyntaxFactory.Block();

            bool bodyUnchanged = ReferenceEquals(body, node.Statement);
            bool condUnchanged = ReferenceEquals(condition, node.Condition);
            if (bodyUnchanged && condUnchanged)
                return node;

            return node.WithCondition(condition ?? node.Condition).WithStatement(body);
        }

        public override SyntaxNode VisitDoStatement(DoStatementSyntax node)
        {
            var body = (StatementSyntax)Visit(node.Statement);
            if (body == null) body = SyntaxFactory.Block();
            var condition = (ExpressionSyntax)Visit(node.Condition);

            bool bodyUnchanged = ReferenceEquals(body, node.Statement);
            bool condUnchanged = ReferenceEquals(condition, node.Condition);
            if (bodyUnchanged && condUnchanged)
                return node;

            return node.WithStatement(body).WithCondition(condition ?? node.Condition);
        }

        public override SyntaxNode VisitEqualsValueClause(EqualsValueClauseSyntax node)
        {
            var value = Visit(node.Value);
            if (value == null) return null;
            return node.WithValue((ExpressionSyntax)value);
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var rewritten = (LocalDeclarationStatementSyntax)base.VisitLocalDeclarationStatement(node);
            if (rewritten == null) return null;
            if (UsesRemovedSymbol(rewritten)) return null;
            // Strip declarations without initializers (GLSL requires initialization at declaration)
            // BUT keep inline-generated declarations (_inlN_xxx) — they are initialized later in do/while blocks
            if (rewritten.Declaration.Variables.All(v => v.Initializer == null)
                && !rewritten.Declaration.Variables.Any(v => v.Identifier.Text.StartsWith("_inl")))
                return null;
            return rewritten;
        }

        public override SyntaxNode VisitElementAccessExpression(ElementAccessExpressionSyntax node) =>
            base.VisitElementAccessExpression(node);

        public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
        {
            if (node.WhenNotNull is MemberBindingExpressionSyntax mb)
            {
                var expr = (ExpressionSyntax)base.Visit(node.Expression);
                var name = SyntaxFactory.IdentifierName(mb.Name.Identifier.Text);
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, expr, name);
            }
            if (node.WhenNotNull is InvocationExpressionSyntax inv
                && inv.Expression is MemberBindingExpressionSyntax mb2)
            {
                var expr = (ExpressionSyntax)base.Visit(node.Expression);
                var ma = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    expr, SyntaxFactory.IdentifierName(mb2.Name.Identifier.Text));
                var args = (ArgumentListSyntax)base.Visit(inv.ArgumentList);
                return SyntaxFactory.InvocationExpression(ma, args);
            }
            return base.VisitConditionalAccessExpression(node);
        }

        public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
        {
            var filtered = node.Arguments
                .Where(a => !IsStandaloneContextArg(a) && !IsIgnoredObjectCreation(a))
                .ToArray();
            if (filtered.Length == node.Arguments.Count) return base.VisitArgumentList(node);
            var visited = filtered.Select(a => (ArgumentSyntax)base.Visit(a))
                .Where(a => a != null && a.Expression != null).ToArray();
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(visited));
        }

        // ══════════════════════════════════════════════════════════════
        // Member resolution helpers
        // ══════════════════════════════════════════════════════════════

        private (string GlslName, bool IsIgnore) ResolveMemberKind(MemberAccessExpressionSyntax node)
        {
            string memberName = node.Name.Identifier.Text;
            var expr = node.Expression;

            // 1. Resolve expression type and find the member via semantic model
            var ti = SafeGetTypeInfo(expr);
            if (ti.Type != null)
            {
                foreach (var member in ti.Type.GetMembers(memberName))
                {
                    var result = TryResolveMember(member);
                    if (result != null) return result.Value;
                }
            }

            // 2. Direct symbol resolution
            var si = SafeGetSymbolInfo(node);
            var sym = si.Symbol ?? si.CandidateSymbols.FirstOrDefault();
            if (sym != null)
            {
                var result = TryResolveMember(sym);
                if (result != null) return result.Value;
            }

            // 3. Static: find type in file root (same-file type references)
            if (expr is IdentifierNameSyntax idExpr)
            {
                try
                {
                    var fileRoot = _semanticModel.SyntaxTree?.GetRoot();
                    if (fileRoot != null)
                    {
                        var typeDecl = fileRoot.DescendantNodes()
                            .OfType<TypeDeclarationSyntax>()
                            .FirstOrDefault(t => t.Identifier.Text == idExpr.Identifier.Text);
                        if (typeDecl != null)
                        {
                            var treeModel = ((CSharpCompilation)_semanticModel.Compilation)
                                .GetSemanticModel(typeDecl.SyntaxTree);
                            var typeSym = treeModel.GetDeclaredSymbol(typeDecl);
                            if (typeSym != null)
                            {
                                foreach (var member in typeSym.GetMembers(memberName))
                                {
                                    var result = TryResolveMember(member);
                                    if (result != null) return result.Value;
                                }
                            }
                        }
                    }
                }
                catch { /* file root or tree not available */ }
            }

            // 4. Fallback: if expression is a removed param (like ctx), search its precomputed type
            // This handles the case where SafeGetTypeInfo fails because nodes were disconnected
            // from the syntax tree by InlineExpander.
            if (expr is IdentifierNameSyntax idFallback && _removedParamTypes.TryGetValue(idFallback.Identifier.Text, out var paramType))
            {
                // Search plan's MethodNameMap first
                foreach (var kv in _plan.MethodNameMap)
                {
                    if (kv.Key.EndsWith("." + memberName) || kv.Key == memberName)
                        return (kv.Value, false);
                }

                // Search the precomputed type for [ShaderField]/[ShaderFunction] members
                foreach (var member in paramType.GetMembers(memberName))
                {
                    var result = TryResolveMember(member);
                    if (result != null) return result.Value;
                }
            }

            return (null, false);
        }

        private (string GlslName, bool IsIgnore)? TryResolveMember(ISymbol symbol)
        {
            var attrs = symbol.GetAttributes();

            // Check symbol's own attributes first (priority over containing type)
            foreach (var a in attrs)
            {
                var name = a.AttributeClass?.Name;
                string minVer = GetNamedArgString(a, "MinVersion");

                if (name == "ShaderFunctionAttribute" || name == "ShaderFunction")
                {
                    string tmpl = GetAttrStringArg(a, 0);
                    if (!string.IsNullOrEmpty(tmpl))
                    {
                        if (!VulkanVersions.Satisfies(_targetVersion, minVer))
                            return (null, true);
                        return (ExtractGlslName(tmpl), false);
                    }
                }
                else if (name == "ShaderFieldAttribute" || name == "ShaderField")
                {
                    string n = GetAttrStringArg(a, 0);
                    if (!string.IsNullOrEmpty(n))
                    {
                        if (!VulkanVersions.Satisfies(_targetVersion, minVer))
                            return (null, true);
                        return (n, false);
                    }
                }
                else if (name == "ShaderIgnoreAttribute" || name == "ShaderIgnore")
                {
                    return (null, true);
                }
            }

            // Check if the member's containing type is [ShaderIgnore]
            var ct = symbol.ContainingType;
            while (ct != null)
            {
                if (IsIgnoredType(ct))
                    return (null, true);
                ct = ct.ContainingType;
            }

            // For methods: check plan's MethodNameMap (qualified key)
            if (symbol is IMethodSymbol methodSym)
            {
                string qKey = methodSym.ContainingType?.Name + "." + methodSym.Name;
                if (_plan.MethodNameMap.TryGetValue(qKey, out var glslName))
                    return (glslName, false);
                // Also try simple name (same-class methods may be registered with simple key)
                if (_plan.MethodNameMap.TryGetValue(methodSym.Name, out glslName))
                    return (glslName, false);
            }

            return null;
        }

        /// <summary>Extract GLSL function name from a method's [ShaderFunction] attribute.</summary>
        private static string ResolveGlslFunctionName(IMethodSymbol method)
        {
            foreach (var attr in method.GetAttributes())
            {
                var name = attr.AttributeClass?.Name;
                if (name == "ShaderFunctionAttribute" || name == "ShaderFunction")
                {
                    string tmpl = attr.ConstructorArguments.FirstOrDefault().Value?.ToString();
                    if (!string.IsNullOrEmpty(tmpl))
                        return ExtractGlslName(tmpl);
                }
            }
            return null;
        }

        // ══════════════════════════════════════════════════════════════
        // Helpers
        // ══════════════════════════════════════════════════════════════

        private bool UsesRemovedSymbol(SyntaxNode node)
        {
            if (_removedParamNames.Count == 0) return false;
            return node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Any(id =>
            {
                if (!_removedParamNames.Contains(id.Identifier.Text)) return false;
                // If this identifier is the expression part of a MemberAccess (ctx.Foo),
                // it should NOT cause removal — the MemberAccess should have been resolved.
                // Only remove if it's standalone or used in a non-member-access context.
                if (id.Parent is MemberAccessExpressionSyntax ma && ma.Expression == id)
                    return false;
                return true;
            });
        }

        private bool IsStandaloneContextArg(ArgumentSyntax arg)
        {
            if (arg.Expression is IdentifierNameSyntax idExpr && !(idExpr.Parent is MemberAccessExpressionSyntax))
                return _removedParamNames.Contains(idExpr.Identifier.Text);
            return false;
        }

        private bool IsIgnoredObjectCreation(ArgumentSyntax arg)
        {
            if (arg.Expression is ObjectCreationExpressionSyntax objCreation)
            {
                var typeInfo = SafeGetTypeInfo(objCreation.Type);
                return typeInfo.Type != null && IsIgnoredType(typeInfo.Type);
            }
            return false;
        }

        private bool IsShaderTypeBelowMinVersion(ITypeSymbol type)
        {
            foreach (var a in type.GetAttributes())
            {
                var name = a.AttributeClass?.Name;
                if (name == "ShaderTypeAttribute" || name == "ShaderType")
                {
                    string minVer = GetNamedArgString(a, "MinVersion");
                    if (!string.IsNullOrEmpty(minVer) && !VulkanVersions.Satisfies(_targetVersion, minVer))
                        return true;
                }
            }
            return false;
        }

        private static bool IsIgnoredType(ITypeSymbol type)
            => type.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ShaderIgnoreAttribute" || a.AttributeClass?.Name == "ShaderIgnore");

        private static string ResolveGlslType(ITypeSymbol type, string fallbackName)
        {
            foreach (var a in type.GetAttributes())
            {
                var name = a.AttributeClass?.Name;
                if (name == "ShaderTypeAttribute" || name == "ShaderType")
                {
                    string glslName = GetNamedArgString(a, "Name");
                    if (!string.IsNullOrEmpty(glslName)) return glslName;
                }
            }
            return GlslTypeMapper.MapType(fallbackName);
        }

        private static string GetAttrStringArg(AttributeData attr, int index)
        {
            if (attr.ConstructorArguments.Length > index)
            {
                var val = attr.ConstructorArguments[index].Value;
                if (val is string s && !string.IsNullOrEmpty(s)) return s;
            }
            return null;
        }

        private static string GetNamedArgString(AttributeData attr, string name)
        {
            foreach (var na in attr.NamedArguments)
                if (na.Key == name && na.Value.Value is string s && !string.IsNullOrEmpty(s)) return s;
            return null;
        }

        private static string ExtractGlslName(string t)
        {
            int p = t.IndexOf('(');
            return p >= 0 ? t.Substring(0, p) : t;
        }

        private static string StripNumericSuffix(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Length >= 2 && text[0] == '0' && (text[1] == 'x' || text[1] == 'X'))
                return text;
            char last = text[text.Length - 1];
            if (last == 'f' || last == 'F' || last == 'd' || last == 'D' || last == 'm' || last == 'M')
            {
                string s = text.Substring(0, text.Length - 1);
                if (s.Length > 0 && (s[s.Length - 1] == 'u' || s[s.Length - 1] == 'U'))
                    s = s.Substring(0, s.Length - 1);
                return s;
            }
            if (text.Length > 1 && "ul" == text.Substring(text.Length - 2).ToLowerInvariant())
                return text.Substring(0, text.Length - 2);
            return text;
        }

        private static bool IsAnyAttribute(AttributeSyntax attr)
        {
            string n = attr.Name.ToString();
            return n == "ShaderIgnore" || n == "ShaderIgnoreAttribute" || n == "ShaderBuffer" || n == "ShaderBufferAttribute"
                || n == "ShaderUniform" || n == "ShaderUniformAttribute" || n == "ShaderShared" || n == "ShaderSharedAttribute"
                || n == "ShaderFunction" || n == "ShaderFunctionAttribute" || n == "ShaderField" || n == "ShaderFieldAttribute"
                || n == "ShaderType" || n == "ShaderTypeAttribute" || n == "ShaderEntry" || n == "ShaderEntryAttribute"
                || n == "ShaderCall" || n == "ShaderCallAttribute" || n == "VulkanShaderEntry" || n == "VulkanShaderEntryAttribute";
        }

        private static bool IsAttribute(AttributeSyntax attr, string sn)
        {
            string n = attr.Name.ToString();
            return n == sn || n == sn + "Attribute";
        }
    }
    } // partial class VulkanSourceGenerator
}
