using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.ShaderVM.Vulkan
{
    /// <summary>
    /// Walks from the entry method (Main), discovers all referenced methods,
    /// classifies them (Core/Managed/Inline), and produces a topologically-sorted
    /// emission plan. This is Phase 1 — no GLSL code is generated here.
    /// </summary>
    public partial class VulkanSourceGenerator
    {
    private static class DependencyCollector
    {
        /// <summary>
        /// Build a complete ShaderBuildPlan starting from the Main method.
        /// </summary>
        public static ShaderBuildPlan BuildPlan(
            MethodInfo mainMethod,
            ShaderClassInfo cls,
            List<string> debugLog)
        {
            var plan = new ShaderBuildPlan();
            var sm = cls.SemanticModel;
            var classDecl = cls.ClassDeclaration;
            var targetVersion = cls.TargetVersion ?? ShaderTargetVersion.Default;

            // ── Step 0: Index all methods in the shader class by C# name ──
            var classMethodIndex = new Dictionary<string, MethodDeclarationSyntax>(StringComparer.Ordinal);
            foreach (var member in classDecl.Members)
            {
                if (member is MethodDeclarationSyntax md && md.Identifier.Text != "Main")
                    classMethodIndex[md.Identifier.Text] = md;
            }

            // ── Step 1: Register Main as entry ──
            plan.EntryMethod = new MethodPlanEntry
            {
                Symbol = null, // Main doesn't have a symbol in our MethodInfo
                Kind = MethodPlanEntry.MethodKind.Managed,
                GlslName = "main",
                Syntax = mainMethod.BodySyntax?.Parent as MethodDeclarationSyntax,
                SemanticModel = sm
            };
            plan.MethodNameMap["Main"] = "main";

            // ── Step 2: Walk dependencies from Main body ──
            // BFS queue: method symbols we need to resolve
            var queue = new Queue<IMethodSymbol>();
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            var managedOrder = new List<MethodPlanEntry>();

            // Discover calls in Main's body
            if (mainMethod.BodySyntax != null)
            {
                DiscoverCalls(mainMethod.BodySyntax, sm, classDecl, classMethodIndex, targetVersion,
                    plan, queue, visited, debugLog);
            }

            // BFS: process discovered methods
            while (queue.Count > 0)
            {
                var methodSym = queue.Dequeue();
                if (!visited.Add(methodSym)) continue;

                var (kind, glslName, glslTemplate) = ClassifyMethod(methodSym, targetVersion);

                // Get syntax — may fail for methods from other assemblies or metadata
                MethodDeclarationSyntax methodSyntax = null;
                SemanticModel methodTreeModel = sm;
                try
                {
                    var syntaxRef = methodSym.DeclaringSyntaxReferences.FirstOrDefault();
                    methodSyntax = syntaxRef?.GetSyntax() as MethodDeclarationSyntax;
                    if (methodSyntax != null)
                        methodTreeModel = ((CSharpCompilation)sm.Compilation).GetSemanticModel(methodSyntax.SyntaxTree);
                }
                catch
                {
                    // Syntax tree not in compilation — use the shader class's SemanticModel
                    methodSyntax = null;
                    methodTreeModel = sm;
                }

                var entry = new MethodPlanEntry
                {
                    Symbol = methodSym,
                    Kind = kind,
                    GlslName = glslName,
                    GlslTemplate = glslTemplate,
                    Syntax = methodSyntax,
                    SemanticModel = methodTreeModel
                };

                // Disambiguate overloads — GLSL doesn't support them
                glslName = plan.RegisterGlslName(glslName, methodSym);
                entry.GlslName = glslName;

                // Use qualified key (ContainingType.MethodName) to prevent name conflicts between classes
                string qKey = QualifiedKey(methodSym);

                switch (kind)
                {
                    case MethodPlanEntry.MethodKind.Core:
                        // Core: register name mapping, no body needed
                        plan.CoreMethodTemplates[qKey] = glslTemplate;
                        if (!plan.MethodNameMap.ContainsKey(qKey))
                            plan.MethodNameMap[qKey] = glslName;
                        break;

                    case MethodPlanEntry.MethodKind.Managed:
                        managedOrder.Add(entry);
                        if (!plan.MethodNameMap.ContainsKey(qKey))
                            plan.MethodNameMap[qKey] = glslName;
                        // Discover transitive dependencies from this method's body
                        if (methodSyntax?.Body != null)
                        {
                            try
                            {
                                DiscoverCalls(methodSyntax.Body, methodTreeModel, classDecl, classMethodIndex, targetVersion,
                                    plan, queue, visited, debugLog);
                            }
                            catch { /* body in a tree we can't analyze — skip transitive deps */ }
                        }
                        break;

                    case MethodPlanEntry.MethodKind.Inline:
                        plan.InlineMethods[qKey] = entry;
                        if (methodSyntax?.Body != null)
                        {
                            try
                            {
                                DiscoverCalls(methodSyntax.Body, methodTreeModel, classDecl, classMethodIndex, targetVersion,
                                    plan, queue, visited, debugLog);
                            }
                            catch { /* body in a tree we can't analyze — skip transitive deps */ }
                        }
                        break;
                }
            }

            // Topological sort for managed methods: dependencies before dependents
            plan.ManagedMethods = TopologicalSort(managedOrder);

            return plan;
        }

        /// <summary>
        /// Scan a method body for all invocation expressions and resolve them to IMethodSymbols.
        /// </summary>
        private static void DiscoverCalls(
            BlockSyntax body,
            SemanticModel sm,
            ClassDeclarationSyntax classDecl,
            Dictionary<string, MethodDeclarationSyntax> classMethodIndex,
            string targetVersion,
            ShaderBuildPlan plan,
            Queue<IMethodSymbol> queue,
            HashSet<ISymbol> visited,
            List<string> debugLog)
        {
            foreach (var inv in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                IMethodSymbol resolved = null;

                try
                {
                    // 1. Direct call: MethodName(args)
                    if (inv.Expression is IdentifierNameSyntax idName)
                    {
                        resolved = ResolveMethodSymbol(idName, sm, classDecl, classMethodIndex, targetVersion);
                    }
                    // 2. Member access: ClassName.MethodName(args) or instance.Method(args)
                    else if (inv.Expression is MemberAccessExpressionSyntax ma)
                    {
                        resolved = ResolveMethodSymbol(ma, sm, classDecl, classMethodIndex, targetVersion);
                    }
                }
                catch { /* resolution failed — skip this invocation */ }

                if (resolved != null && !visited.Contains(resolved))
                {
                    queue.Enqueue(resolved);
                }
            }
        }

        /// <summary>
        /// Resolve an IdentifierName (direct call like DevTest1(1)) to an IMethodSymbol.
        /// Handles same-class methods, type-qualified calls, and external references.
        /// </summary>
        private static IMethodSymbol ResolveMethodSymbol(
            IdentifierNameSyntax idName,
            SemanticModel sm,
            ClassDeclarationSyntax classDecl,
            Dictionary<string, MethodDeclarationSyntax> classMethodIndex,
            string targetVersion)
        {
            // Try semantic model first (handles same-class calls, using directives, etc.)
            try
            {
                var sym = sm.GetSymbolInfo(idName).Symbol as IMethodSymbol;
                if (sym != null) return sym;
            }
            catch { /* node not in this syntax tree */ }

            // Fallback: look up in shader class methods by name
            if (classMethodIndex.TryGetValue(idName.Identifier.Text, out var methodSyntax))
            {
                try
                {
                    var treeModel = ((CSharpCompilation)sm.Compilation).GetSemanticModel(methodSyntax.SyntaxTree);
                    return treeModel.GetDeclaredSymbol(methodSyntax);
                }
                catch { /* syntax tree not in compilation */ }
            }

            return null;
        }

        /// <summary>
        /// Resolve a MemberAccess (like TestShared.DevTest2(2)) to an IMethodSymbol.
        /// </summary>
        private static IMethodSymbol ResolveMethodSymbol(
            MemberAccessExpressionSyntax ma,
            SemanticModel sm,
            ClassDeclarationSyntax classDecl,
            Dictionary<string, MethodDeclarationSyntax> classMethodIndex,
            string targetVersion)
        {
            // Direct symbol resolution on the full MemberAccess
            SymbolInfo symInfo;
            try { symInfo = sm.GetSymbolInfo(ma); } catch { symInfo = default; }
            if (symInfo.Symbol is IMethodSymbol ms1) return ms1;

            // Try resolving just the method name portion
            try { symInfo = sm.GetSymbolInfo(ma.Name); } catch { symInfo = default; }
            if (symInfo.Symbol is IMethodSymbol ms2) return ms2;

            // Fallback: resolve type from expression, then find the member
            ITypeSymbol exprType = null;
            try { exprType = sm.GetTypeInfo(ma.Expression).Type; } catch { /* node not in tree */ }
            if (exprType != null)
            {
                foreach (var member in exprType.GetMembers(ma.Name.Identifier.Text))
                {
                    if (member is IMethodSymbol ms3) return ms3;
                }
            }

            // Static: find type in file root
            if (ma.Expression is IdentifierNameSyntax idExpr)
            {
                try
                {
                    var fileRoot = classDecl.SyntaxTree.GetRoot();
                    var typeDecl = fileRoot.DescendantNodes()
                        .OfType<TypeDeclarationSyntax>()
                        .FirstOrDefault(t => t.Identifier.Text == idExpr.Identifier.Text);
                    if (typeDecl != null)
                    {
                        var treeModel = ((CSharpCompilation)sm.Compilation).GetSemanticModel(typeDecl.SyntaxTree);
                        var typeSym = treeModel.GetDeclaredSymbol(typeDecl);
                        if (typeSym != null)
                        {
                            foreach (var member in typeSym.GetMembers(ma.Name.Identifier.Text))
                            {
                                if (member is IMethodSymbol ms4) return ms4;
                            }
                        }
                    }
                }
                catch { /* file root or tree not available */ }
            }

            return null;
        }

        /// <summary>
        /// Classify a method symbol as Core/Managed/Inline based on [ShaderFunction] attribute.
        /// Methods without any [ShaderFunction] attribute are treated as Managed (primary auto-map).
        /// </summary>
        private static (MethodPlanEntry.MethodKind Kind, string GlslName, string GlslTemplate) ClassifyMethod(
            IMethodSymbol method, string targetVersion)
        {
            foreach (var attr in method.GetAttributes())
            {
                var an = attr.AttributeClass?.Name;
                if (an != "ShaderFunctionAttribute" && an != "ShaderFunction") continue;

                var kind = GetShaderFunctionKind(attr);
                string template = attr.ConstructorArguments.Length > 0
                    ? attr.ConstructorArguments[0].Value?.ToString()
                    : null;
                string glslName = !string.IsNullOrEmpty(template) ? ExtractGlslName(template) : ToSnake(method.Name);

                // Check MinVersion
                string minVer = GetNamedArgString(attr, "MinVersion");
                if (!string.IsNullOrEmpty(minVer) && !VulkanVersions.Satisfies(targetVersion, minVer))
                    return (MethodPlanEntry.MethodKind.Managed, glslName, null); // Below min version — treat as ignored

                switch (kind)
                {
                    case ShaderFunctionKind.Core:
                        return (MethodPlanEntry.MethodKind.Core, glslName, template);
                    case ShaderFunctionKind.Inline:
                        return (MethodPlanEntry.MethodKind.Inline, glslName, null);
                    case ShaderFunctionKind.Managed:
                        return (MethodPlanEntry.MethodKind.Managed, glslName, null);
                }
            }

            // No [ShaderFunction] → primary auto-map (Managed)
            return (MethodPlanEntry.MethodKind.Managed, ToSnake(method.Name), null);
        }

        /// <summary>
        /// Topological sort: methods that are called by others come first.
        /// Uses a simple DFS on the call graph.
        /// </summary>
        private static List<MethodPlanEntry> TopologicalSort(List<MethodPlanEntry> entries)
        {
            if (entries.Count <= 1) return entries;

            var entryBySymbol = new Dictionary<ISymbol, MethodPlanEntry>(SymbolEqualityComparer.Default);
            foreach (var e in entries)
                entryBySymbol[e.Symbol] = e;

            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            var result = new List<MethodPlanEntry>();
            var inStack = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

            void Dfs(IMethodSymbol sym)
            {
                if (!visited.Add(sym)) return;
                inStack.Add(sym);

                // Find all calls from this method's body
                MethodPlanEntry entry;
                entryBySymbol.TryGetValue(sym, out entry);
                if (entry?.Syntax?.Body != null && entry.SemanticModel != null)
                {
                    try
                    {
                        foreach (var inv in entry.Syntax.Body.DescendantNodes().OfType<InvocationExpressionSyntax>())
                        {
                            var calledSym = entry.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                            if (calledSym != null && entryBySymbol.ContainsKey(calledSym) && !inStack.Contains(calledSym))
                                Dfs(calledSym);
                        }
                    }
                    catch { /* Syntax tree not in compilation — skip dependency discovery */ }
                }

                inStack.Remove(sym);
                if (entryBySymbol.TryGetValue(sym, out var e))
                    result.Add(e);
            }

            foreach (var e in entries)
                Dfs(e.Symbol);

            return result;
        }

        // ── Helpers (duplicated from GlslAstRewriter to avoid coupling) ──

        private static ShaderFunctionKind GetShaderFunctionKind(AttributeData attr)
        {
            foreach (var na in attr.NamedArguments)
            {
                if (na.Key == "Kind" && na.Value.Value is int kv) return (ShaderFunctionKind)kv;
                if (na.Key == "Inline" && na.Value.Value is bool ib && ib) return ShaderFunctionKind.Inline;
                if (na.Key == "CSharpCode" && na.Value.Value is bool cb && cb) return ShaderFunctionKind.Managed;
            }
            return ShaderFunctionKind.Core;
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

        internal static string ToSnake(string n)
        {
            if (string.IsNullOrEmpty(n)) return n;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < n.Length; i++)
            {
                char c = n[i];
                if (char.IsUpper(c)) { if (i > 0) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>Build a qualified key for MethodNameMap: "ContainingType.MethodName".
        /// This prevents name conflicts between methods from different classes.</summary>
        private static string QualifiedKey(IMethodSymbol method)
        {
            string typeName = method.ContainingType?.Name ?? "";
            return string.IsNullOrEmpty(typeName) ? method.Name : typeName + "." + method.Name;
        }
    }
    }
}
