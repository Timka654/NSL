using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.Utils;
using NSL.ShaderVM.Vulkan.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace NSL.ShaderVM.Vulkan
{
    [Generator]
    public partial class VulkanSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var shaderClasses = ctx.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (n, _) => n is ClassDeclarationSyntax c && HasShaderEntry(c),
                    transform: (gctx, _) => TransformClass(gctx))
                .Where(x => x != null).Select((x, _) => x)
                .Collect();

            ctx.RegisterSourceOutput(shaderClasses, GenerateAll);
        }

        private static bool HasShaderEntry(ClassDeclarationSyntax cls)
            => cls.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString() == VulkanShaderEntryAttribute.ShortName
                    || a.Name.ToString() == VulkanShaderEntryAttribute.ShortName + "Attribute");

        private static bool HasIgnore(SyntaxList<AttributeListSyntax> lists)
            => lists
                .SelectMany(al => al.Attributes)
                .Any(a => a.Name.ToString() == "ShaderIgnore"
                    || a.Name.ToString() == "ShaderIgnoreAttribute");

        private static ShaderClassInfo TransformClass(GeneratorSyntaxContext gctx)
        {
            var cls = (ClassDeclarationSyntax)gctx.Node;

            if (HasIgnore(cls.AttributeLists)) return null;

            string shaderName = null;
            string targetVersion = null;
            bool uniformsViaSsbo = false;
            uint lx = 1, ly = 1, lz = 1;

            foreach (var a in cls.AttributeLists.SelectMany(al => al.Attributes))
            {
                var an = a.Name.ToString();
                if (an != VulkanShaderEntryAttribute.ShortName && an != VulkanShaderEntryAttribute.ShortName + "Attribute") continue;

                foreach (var arg in a.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>())
                {
                    if (arg.NameEquals == null) continue;
                    string n = arg.NameEquals.Name.Identifier.Text;
                    string v = arg.Expression.ToString().Trim('"');
                    switch (n)
                    {
                        case "ShaderName": shaderName = v; break;
                        case "LocalSizeX": uint.TryParse(v, out lx); break;
                        case "LocalSizeY": uint.TryParse(v, out ly); break;
                        case "LocalSizeZ": uint.TryParse(v, out lz); break;
                        case "TargetVersion": targetVersion = v; break;
                        case "UniformsViaSSBO": bool.TryParse(v, out uniformsViaSsbo); break;
                    }
                }
            }

            string entryName = shaderName ?? ToSnake(cls.Identifier.Text);

            var buffers = new List<FieldInfo>();
            var uniforms = new List<FieldInfo>();
            var pushConstants = new List<FieldInfo>();
            var shared = new List<FieldInfo>();
            var consts = new List<ConstFieldInfo>();
            var methods = new List<MethodInfo>();

            foreach (var member in cls.Members)
            {
                if (member is FieldDeclarationSyntax field)
                {
                    bool isConst = field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ConstKeyword));
                    bool isStaticReadonly = field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword))
                                         && field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword));

                    foreach (var v in field.Declaration.Variables)
                    {
                        // Collect const / static readonly fields
                        if (isConst || isStaticReadonly)
                        {
                            string value = v.Initializer?.Value?.ToString() ?? "";
                            // Strip C# suffixes from numeric values (f, d, m, ul)
                            if (v.Initializer?.Value is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax lit)
                                value = StripNumericSuffixLit(lit.Token.Text);
                            consts.Add(new ConstFieldInfo
                            {
                                Name = v.Identifier.Text,
                                TypeName = field.Declaration.Type.ToString(),
                                Value = value
                            });
                            continue;
                        }

                        var fi = new FieldInfo { Name = v.Identifier.Text, TypeName = field.Declaration.Type.ToString() };
                        foreach (var a in field.AttributeLists.SelectMany(al => al.Attributes))
                        {
                            string an = a.Name.ToString();
                            if (an == "ShaderBuffer" || an == "ShaderBufferAttribute")
                            {
                                foreach (var arg in a.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>())
                                {
                                    if (arg.NameEquals == null) continue;
                                    string pn = arg.NameEquals.Name.Identifier.Text;
                                    string pv = arg.Expression.ToString().Trim('"');
                                    switch (pn)
                                    {
                                        case "Binding": int.TryParse(pv, out var b); fi.Binding = b; break;
                                        case "Set": int.TryParse(pv, out var s); fi.Set = s; break;
                                        case "ReadOnly": bool.TryParse(pv, out var ro); fi.ReadOnly = ro; break;
                                    }
                                }
                                buffers.Add(fi);
                            }
                            else if (an == "ShaderUniform" || an == "ShaderUniformAttribute")
                                uniforms.Add(fi);
                            else if (an == "ShaderPushConstant" || an == "ShaderPushConstantAttribute")
                                pushConstants.Add(fi);
                            else if (an == "ShaderShared" || an == "ShaderSharedAttribute")
                            {
                                foreach (var arg in a.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>())
                                {
                                    if (arg.NameEquals == null) continue;
                                    string pn = arg.NameEquals.Name.Identifier.Text;
                                    string pv = arg.Expression.ToString().Trim('"');
                                    if (pn == "Size") int.TryParse(pv, out fi.Size);
                                }
                                // If Size not set via attribute, read from C# initializer (new float[N])
                                if (fi.Size == 0 && v.Initializer?.Value is ArrayCreationExpressionSyntax ac
                                    && ac.Type?.RankSpecifiers.Count > 0)
                                {
                                    foreach (var sz in ac.Type.RankSpecifiers[0].Sizes)
                                    {
                                        if (sz is LiteralExpressionSyntax lit && int.TryParse(lit.Token.ValueText, out var size))
                                            fi.Size = size;
                                    }
                                }
                                shared.Add(fi);
                            }
                        }
                    }
                }
                else if (member is MethodDeclarationSyntax method && !HasIgnore(method.AttributeLists))
                    methods.Add(new MethodInfo
                    {
                        Name = method.Identifier.Text,
                        ReturnType = method.ReturnType.ToString(),
                        BodySyntax = method.Body,
                        ParameterListSyntax = method.ParameterList
                    });
            }

            return new ShaderClassInfo
            {
                ClassDeclaration = cls,
                SemanticModel = gctx.SemanticModel,
                ShaderName = entryName,
                LocalSizeX = lx,
                LocalSizeY = ly,
                LocalSizeZ = lz,
                TargetVersion = targetVersion ?? ShaderTargetVersion.Default,
                UniformsViaSSBO = uniformsViaSsbo,
                Buffers = buffers,
                Uniforms = uniforms,
                PushConstants = pushConstants,
                Shared = shared,
                Consts = consts,
                StructDeclarations = CollectShaderStructs(methods, gctx.SemanticModel),
                Methods = methods
            };
        }

        /// <summary>Scan method bodies for [ShaderType] struct usage and collect unique declarations.</summary>
        private static List<ShaderStructInfo> CollectShaderStructs(List<MethodInfo> methods, SemanticModel sm)
        {
            // GLSL built-in type names — these must NOT be emitted as struct declarations
            var glslBuiltins = new HashSet<string>(StringComparer.Ordinal)
            {
                "vec2", "vec3", "vec4", "ivec2", "ivec3", "ivec4",
                "mat2", "mat3", "mat4",
                "float16_t", "f16vec2", "f16vec4",
                "int64_t", "uint64_t"
            };

            var result = new List<ShaderStructInfo>();
            var seen = new HashSet<string>();

            foreach (var m in methods)
            {
                if (m.BodySyntax == null) continue;
                foreach (var objCreation in m.BodySyntax.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
                {
                    var typeInfo = sm.GetTypeInfo(objCreation.Type);
                    if (typeInfo.Type == null) continue;

                    foreach (var attr in typeInfo.Type.GetAttributes())
                    {
                        var attrName = attr.AttributeClass?.Name;
                        if (attrName == "ShaderTypeAttribute" || attrName == "ShaderType")
                        {
                            string glslName = typeInfo.Type.Name;
                            foreach (var na in attr.NamedArguments)
                            {
                                if (na.Key == "Name" && na.Value.Value is string s && !string.IsNullOrEmpty(s))
                                    glslName = s;
                            }

                            // Skip GLSL built-ins (vec3, mat4, float16_t, etc.)
                            if (glslBuiltins.Contains(glslName)) break;
                            if (!seen.Add(glslName)) break;

                            var fields = new List<(string, string)>();
                            foreach (var member in typeInfo.Type.GetMembers())
                            {
                                if (member is IFieldSymbol fs && !fs.IsStatic)
                                {
                                    string fieldGlslType = GlslTypeMapper.MapType(fs.Type.Name);
                                    fields.Add((fieldGlslType, fs.Name));
                                }
                            }

                            result.Add(new ShaderStructInfo { GlslName = glslName, Fields = fields });
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private void GenerateAll(SourceProductionContext context, ImmutableArray<ShaderClassInfo> classes)
        {
            int idx = 0;
            foreach (var cls in classes)
            {
                CodeBuilder cb = new CodeBuilder();
                try
                {
                    cb.CreatePartialClass((CSharpCompilation)cls.SemanticModel.Compilation, cls.ClassDeclaration, b =>
                    {

                        List<string> log = null;
#if DEBUG

                        log = new List<string>();
#endif

                        string glsl = GenerateGlsl(cls,log);
                        var tabs = b.Tabs;
                        while (b.Tabs > 0) b.PrevTab();
                        b.AppendContent($"{new string('\t', tabs)}public const string GlslSource = @\"{glsl}\";\n");
                        while (b.Tabs < tabs) b.NextTab();

                        if (log != null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor("NSL_ShaderVM_DIAG", "NSL.ShaderVM Generator diagnostics", string.Join("\n", log), "NSL.ShaderVM", DiagnosticSeverity.Warning, true),
                                cls.ClassDeclaration.GetLocation()));
                        }
                    });
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor("NSL_ShaderVM_ERR", "NSL.ShaderVM Generator Error", $"Generator crashed: {ex.Message}", "NSL.ShaderVM", DiagnosticSeverity.Error, true),
                        cls.ClassDeclaration.GetLocation()));
                }
                string hintName = $"{cls.ClassDeclaration.Identifier.Text}_{cls.ShaderName}_{idx++}.generated.cs";
                context.AddSource(hintName, cb.ToString());
            }
        }

        private static string GenerateGlsl(ShaderClassInfo cls, List<string> log)
        {
            var sb = new StringBuilder();

            string ver = VulkanVersions.ToGlslVersion(cls.TargetVersion);
            sb.AppendLine($"#version {ver}");
            foreach (var e in VulkanVersions.GetExtensions(cls.TargetVersion))
                sb.AppendLine($"#extension {e} : enable");
            sb.AppendLine($"layout(local_size_x = {cls.LocalSizeX}, local_size_y = {cls.LocalSizeY}, local_size_z = {cls.LocalSizeZ}) in;");
            sb.AppendLine();

            var bufferNames = new Dictionary<string, string>();
            if (cls.Buffers.Count > 0)
            {
                foreach (var b in cls.Buffers)
                {
                    string q = b.ReadOnly ? "readonly " : "";
                    string t = MapGlsl(StripArraySuffix(b.TypeName));
                    string member = b.Name;
                    sb.AppendLine($"layout(set = {b.Set}, binding = {b.Binding}) {q}buffer {b.Name}Buf {{ {t} {member}[]; }};");
                    bufferNames[b.Name] = member;
                }
            }

            if (cls.Uniforms.Count > 0)
            {
                if (cls.UniformsViaSSBO)
                {
                    // SSBO mode: uniforms are a readonly buffer
                    int uniformBinding = cls.Buffers.Count;
                    sb.AppendLine($"layout(set = 0, binding = {uniformBinding}) readonly buffer ParamsBuf {{");
                    foreach (var u in cls.Uniforms)
                        sb.AppendLine($"    {MapGlsl(u.TypeName)} {u.Name};");
                    sb.AppendLine("} params;");
                }
                else
                {
                    // push_constant mode (default)
                    sb.AppendLine("layout(push_constant) uniform Params {");
                    foreach (var u in cls.Uniforms)
                        sb.AppendLine($"    {MapGlsl(u.TypeName)} {u.Name};");
                    sb.AppendLine("} params;");
                }
            }

            if (cls.PushConstants.Count > 0)
            {
                sb.AppendLine("layout(push_constant) uniform PC {");
                foreach (var p in cls.PushConstants)
                    sb.AppendLine($"    {MapGlsl(p.TypeName)} {p.Name};");
                sb.AppendLine("} pc;");
            }

            if (cls.Shared.Count > 0)
            {
                foreach (var s in cls.Shared)
                {
                    string t = MapGlsl(StripArraySuffix(s.TypeName));
                    string member = s.Name;
                    string sizePart = s.Size > 0 ? $"[{s.Size}]" : "[]";
                    sb.AppendLine($"shared {t} {member}{sizePart};");
                }
            }

            if (cls.Consts.Count > 0)
            {
                foreach (var c in cls.Consts)
                {
                    string glslType = MapGlsl(c.TypeName);
                    sb.AppendLine($"const {glslType} {c.Name} = {c.Value};");
                }
            }

            if (cls.StructDeclarations.Count > 0)
            {
                foreach (var s in cls.StructDeclarations)
                {
                    sb.AppendLine($"struct {s.GlslName}");
                    sb.AppendLine("{");
                    foreach (var (fieldType, fieldName) in s.Fields)
                        sb.AppendLine($"    {fieldType} {fieldName};");
                    sb.AppendLine("};");
                }
            }
            sb.AppendLine();

            // ══════════════════════════════════════════════════════════
            // Phase 1: Build dependency plan (classifies Core/Managed/Inline, topological sort)
            // ══════════════════════════════════════════════════════════
            var mainMethod = cls.Methods.FirstOrDefault(x => x.Name == "Main");
            if (mainMethod == null || mainMethod.BodySyntax == null)
                return sb.ToString().TrimEnd('\r', '\n');

            var plan = DependencyCollector.BuildPlan(mainMethod, cls, log);

            var uniformNames = new HashSet<string>(cls.Uniforms.Select(u => u.Name));
            var pushConstantNames = new HashSet<string>(cls.PushConstants.Select(p => p.Name));
            string targetVersion = cls.TargetVersion;
            var emittedSignatures = new HashSet<string>();

            // ══════════════════════════════════════════════════════════
            // Phase 2 + 3: For each method: Expand inline → Rewrite to GLSL → Emit
            // ══════════════════════════════════════════════════════════

            // Process Main first
            {
                var entry = plan.EntryMethod;
                var inlineExpander = new InlineExpander(plan);
                var expandedBody = inlineExpander.ExpandBlock(mainMethod.BodySyntax);

                var removedParamNames = GetRemovedParamNames(mainMethod, cls.SemanticModel);
                var removedParamTypes = GetRemovedParamTypes(mainMethod, cls.SemanticModel);
                var rewriter = new GlslAstRewriter(plan, uniformNames, pushConstantNames, bufferNames, cls.SemanticModel, targetVersion, removedParamNames, log, removedParamTypes);

                string methodSig = EmitMethodSignature(mainMethod, cls.SemanticModel);
                sb.AppendLine(methodSig);
                sb.AppendLine("{");
                var rewrittenBody = (BlockSyntax)rewriter.Visit(expandedBody);
                EmitRewrittenStatements(rewrittenBody, sb);
                sb.AppendLine("}");
            }

            // Process managed methods (already topologically sorted)
            foreach (var entry in plan.ManagedMethods)
            {
                if (entry.Syntax == null) continue;
                if (entry.Syntax.Body == null && entry.Syntax.ExpressionBody == null) continue;

                // Get the right SemanticModel for this method's syntax tree
                var treeModel = entry.SemanticModel ?? cls.SemanticModel;

                // Build GLSL signature
                string glslName = entry.GlslName;
                string returnType = entry.Symbol.ReturnsVoid ? "void" : GlslTypeMapper.MapType(entry.Symbol.ReturnType.Name);
                var paramParts = new List<string>();
                foreach (var p in entry.Symbol.Parameters)
                {
                    if (p.GetAttributes().Any(a => a.AttributeClass?.Name == "ShaderIgnoreAttribute" || a.AttributeClass?.Name == "ShaderIgnore"))
                        continue;
                    if (IsShaderIgnoreType(p.Type))
                        continue;

                    if (p.Type is IArrayTypeSymbol arrType)
                    {
                        string elemType = GlslTypeMapper.MapType(arrType.ElementType.Name);
                        paramParts.Add($"{elemType} {p.Name}[]");
                    }
                    else
                    {
                        string pt = GlslTypeMapper.MapType(p.Type.Name);
                        paramParts.Add($"{pt} {p.Name}");
                    }
                }
                string sig = $"{returnType} {glslName}({string.Join(", ", paramParts)})";
                if (!emittedSignatures.Add(sig)) continue;

                // Phase 2: Expand inline calls
                var inlineExpander = new InlineExpander(plan, inlineIdBase: 100 * plan.ManagedMethods.IndexOf(entry));

                sb.AppendLine(sig);
                sb.AppendLine("{");
                if (entry.Syntax.Body != null)
                {
                    var expandedBody = inlineExpander.ExpandBlock(entry.Syntax.Body);
                    var rewriter = new GlslAstRewriter(plan, uniformNames, pushConstantNames, bufferNames, treeModel, targetVersion, debugLog: log);
                    var rewritten = (BlockSyntax)rewriter.Visit(expandedBody);
                    EmitRewrittenStatements(rewritten, sb);
                }
                else if (entry.Syntax.ExpressionBody != null)
                {
                    var rewriter = new GlslAstRewriter(plan, uniformNames, pushConstantNames, bufferNames, treeModel, targetVersion, debugLog: log);
                    var rewritten = (ExpressionSyntax)rewriter.Visit(entry.Syntax.ExpressionBody.Expression);
                    if (rewritten != null)
                        sb.AppendLine($"    return {rewritten.ToFullString().Trim()};");
                }
                sb.AppendLine("}");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }

        private static HashSet<string> GetRemovedParamNames(MethodInfo m, SemanticModel sm)
        {
            var names = new HashSet<string>();
            if (m.ParameterListSyntax == null) return names;
            foreach (var p in m.ParameterListSyntax.Parameters)
            {
                if (HasShaderIgnoreAttr(p.AttributeLists)) continue;
                string typeName = p.Type.ToString();
                var pt = sm.GetTypeInfo(p.Type).Type;
                bool isIgnored = pt != null && IsShaderIgnoreType(pt);
                bool isUnmappable = !GlslTypeMapper.IsShaderType(typeName);
                if (isIgnored || isUnmappable)
                    names.Add(p.Identifier.Text);
            }
            return names;
        }

        /// <summary>Precompute removed-param name → ITypeSymbol mapping for disconnected-node resolution.</summary>
        private static Dictionary<string, ITypeSymbol> GetRemovedParamTypes(MethodInfo m, SemanticModel sm)
        {
            var result = new Dictionary<string, ITypeSymbol>();
            if (m.ParameterListSyntax == null) return result;
            foreach (var p in m.ParameterListSyntax.Parameters)
            {
                if (HasShaderIgnoreAttr(p.AttributeLists)) continue;
                string typeName = p.Type.ToString();
                var pt = sm.GetTypeInfo(p.Type).Type;
                bool isIgnored = pt != null && IsShaderIgnoreType(pt);
                bool isUnmappable = !GlslTypeMapper.IsShaderType(typeName);
                if ((isIgnored || isUnmappable) && pt != null)
                    result[p.Identifier.Text] = pt;
            }
            return result;
        }

        private static bool IsShaderIgnoreType(ITypeSymbol type)
            => type.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ShaderIgnoreAttribute" || a.AttributeClass?.Name == "ShaderIgnore");

        private static bool HasShaderIgnoreAttr(SyntaxList<AttributeListSyntax> lists)
            => lists.SelectMany(al => al.Attributes).Any(a =>
                a.Name.ToString() == "ShaderIgnore" || a.Name.ToString() == "ShaderIgnoreAttribute");

        private static string StripArraySuffix(string typeName)
        {
            if (typeName.EndsWith("[]")) return typeName.Substring(0, typeName.Length - 2);
            return typeName;
        }

        private static string EmitMethodSignature(MethodInfo m, SemanticModel sm)
        {
            string funcName = m.Name == "Main" ? "main" : ToSnake(m.Name);
            string returnType = m.Name == "Main" ? "void" : MapGlsl(m.ReturnType);

            var keptParams = new List<string>();
            if (m.ParameterListSyntax != null)
            {
                foreach (var p in m.ParameterListSyntax.Parameters)
                {
                    if (HasShaderIgnoreAttr(p.AttributeLists)) continue;
                    string typeName = p.Type.ToString();
                    var pt = sm.GetTypeInfo(p.Type).Type;
                    if (pt != null && IsShaderIgnoreType(pt)) continue;
                    if (!GlslTypeMapper.IsShaderType(typeName)) continue;
                    string glslType = MapGlsl(typeName);
                    keptParams.Add($"{glslType} {p.Identifier.Text}");
                }
            }

            return $"{returnType} {funcName}({string.Join(", ", keptParams)})";
        }

        private static void EmitRewrittenStatements(BlockSyntax rewrittenBody, StringBuilder msb)
        {
            if (rewrittenBody == null) return;
            var normalizedBody = rewrittenBody.NormalizeWhitespace();
            string bodyText = normalizedBody.ToFullString();

            var lines = bodyText.Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .Where(l => l.TrimStart().Length > 0)
                .ToArray();

            if (lines.Length < 2) return;
            int start = 1, end = lines.Length - 1;
            for (int i = start; i < end; i++)
            {
                string l = lines[i];
                // Strip // comments and non-ASCII that would break Shaderc
                // l = StripLineComment(l);
                l = SanitizeAscii(l);
                if (string.IsNullOrWhiteSpace(l)) continue;

                // Strip C# numeric suffixes not valid in GLSL (f, d, m, ul)
                l = Regex.Replace(l, @"(\d+\.?\d*|\d*\.?\d+)([eE][+-]?\d+)?[fFdDmM]\b", "$1$2");
                l = Regex.Replace(l, @"\b(\d+)[uU][lL]\b", "$1");

                string trimmed = l.TrimStart();
                int leadingSpaces = l.Length - l.TrimStart().Length;
                int tabs = leadingSpaces / 4;
                string indent = new string('\t', tabs);
                msb.AppendLine(indent + trimmed);
            }
        }

        // /// <summary>Remove // single-line comments from a GLSL line.</summary>
        // private static string StripLineComment(string line)
        // {
        //     int idx = line.IndexOf("//");
        //     return idx >= 0 ? line.Substring(0, idx) : line;
        // }

        /// <summary>Replace non-ASCII chars with space (Shaderc rejects them).</summary>
        private static string SanitizeAscii(string line)
        {
            var sb = new StringBuilder(line.Length);
            foreach (char c in line)
                sb.Append(c <= 127 ? c : ' ');
            return sb.ToString();
        }

        private static string MapGlsl(string cs) => GlslTypeMapper.MapType(cs);

        /// <summary>Strip C# numeric suffixes (f, d, m, ul) from literal token text, preserving hex.</summary>
        private static string StripNumericSuffixLit(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Skip hex literals
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

        internal static string ToSnake(string n)
        {
            if (string.IsNullOrEmpty(n)) return n;
            var sb = new StringBuilder();
            for (int i = 0; i < n.Length; i++)
            {
                char c = n[i];
                if (char.IsUpper(c)) { if (i > 0) sb.Append('_'); sb.Append(char.ToLowerInvariant(c)); }
                else sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
