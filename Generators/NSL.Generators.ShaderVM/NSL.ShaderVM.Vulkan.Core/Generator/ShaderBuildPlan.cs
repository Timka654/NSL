using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace NSL.ShaderVM.Vulkan
{
    public partial class VulkanSourceGenerator
    {
    /// <summary>
    /// Pre-computed build plan for GLSL code generation.
    /// Built by DependencyCollector BEFORE any code is emitted.
    /// Contains the full dependency graph: which methods to emit, in what order, with what GLSL names.
    /// </summary>
    private class ShaderBuildPlan
    {
        /// <summary>Entry method (Main).</summary>
        public MethodPlanEntry EntryMethod { get; set; }

        /// <summary>Managed methods in topological emission order (dependencies BEFORE dependents).</summary>
        public List<MethodPlanEntry> ManagedMethods { get; set; } = new List<MethodPlanEntry>();

        /// <summary>Inline methods indexed by C# method name.</summary>
        public Dictionary<string, MethodPlanEntry> InlineMethods { get; set; } = new Dictionary<string, MethodPlanEntry>(StringComparer.Ordinal);

        /// <summary>Core methods: C# method name → GLSL expression template (e.g., "exp(x)").</summary>
        public Dictionary<string, string> CoreMethodTemplates { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Qualified key (ContainingType.MethodName) → GLSL function name. Prevents name conflicts between classes.</summary>
        public Dictionary<string, string> MethodNameMap { get; set; } = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Track used GLSL function names to detect and resolve overloads.</summary>
        public HashSet<string> UsedGlslNames { get; set; } = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Register a GLSL name and disambiguate if it's already taken (overload).</summary>
        public string RegisterGlslName(string preferred, IMethodSymbol method)
        {
            // Core/GLSL built-in names (exp, sqrt, etc.) are always allowed — they map to real GLSL functions
            bool isCore = false;
            foreach (var attr in method.GetAttributes())
            {
                var an = attr.AttributeClass?.Name;
                if ((an == "ShaderFunctionAttribute" || an == "ShaderFunction") && GetShaderFunctionKind(attr) == 0 /* Core */)
                { isCore = true; break; }
            }
            if (isCore) return preferred;

            if (UsedGlslNames.Add(preferred))
                return preferred;

            // Name collision — append _2, _3, ...
            for (int i = 2; ; i++)
            {
                string candidate = preferred + "_" + i;
                if (UsedGlslNames.Add(candidate))
                    return candidate;
            }
        }

        private static ShaderFunctionKind GetShaderFunctionKind(AttributeData attr)
        {
            foreach (var na in attr.NamedArguments)
            {
                if (na.Key == "Kind" && na.Value.Value is int kv) return (ShaderFunctionKind)kv;
            }
            return ShaderFunctionKind.Core;
        }

        /// <summary>Find an inline entry by its IMethodSymbol.</summary>
        public MethodPlanEntry FindInlineBySymbol(IMethodSymbol symbol)
        {
            foreach (var entry in InlineMethods.Values)
            {
                if (SymbolEqualityComparer.Default.Equals(entry.Symbol, symbol))
                    return entry;
            }
            return null;
        }
    }

    private class MethodPlanEntry
    {
        public enum MethodKind { Core, Managed, Inline }

        public IMethodSymbol Symbol { get; set; }
        public MethodKind Kind { get; set; }

        /// <summary>GLSL function name (e.g., "dev_test2", "pcg_next").</summary>
        public string GlslName { get; set; }

        /// <summary>For Core: full GLSL template (e.g., "exp(x)"). Null for Managed/Inline.</summary>
        public string GlslTemplate { get; set; }

        /// <summary>C# syntax of the method declaration. Null for Core (extern).</summary>
        public MethodDeclarationSyntax Syntax { get; set; }

        /// <summary>SemanticModel for the syntax tree containing this method.</summary>
        public SemanticModel SemanticModel { get; set; }
    }
    } // partial class VulkanSourceGenerator
}
