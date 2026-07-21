using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using NSL.ShaderVM.Vulkan.Attributes;

namespace NSL.ShaderVM.Vulkan
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class VulkanAnalyzer : DiagnosticAnalyzer
    {
        public const string InvalidParameterTypeId = "NSL_ShaderVM001";
        public const string InvalidReturnTypeId = "NSL_ShaderVM002";
        public const string InvalidLocalTypeId = "NSL_ShaderVM003";
        public const string NonShaderTypeWillBeRemovedId = "NSL_ShaderVM004";
        public const string NonShaderMethodNotConvertedId = "NSL_ShaderVM005";
        public const string MinVersionNotSatisfiedId = "NSL_ShaderVM021";
        public const string UnmappedFieldId = "NSL_ShaderVM022";
        public const string TypeWidenedId = "NSL_ShaderVM023";
        public const string ForEachUnsupportedId = "NSL_ShaderVM024";
        public const string UnsizedLocalArrayId = "NSL_ShaderVM025";
        public const string GotoUnsupportedId = "NSL_ShaderVM026";
        public const string ArrayInitializerUnsupportedId = "NSL_ShaderVM027";
        public const string DefaultParamUnsupportedId = "NSL_ShaderVM028";
        public const string ManagedArrayParamId = "NSL_ShaderVM029";
        public const string InvalidFieldTypeId = "NSL_ShaderVM010";
        public const string MissingMainId = "NSL_ShaderVM011";
        public const string InvalidTargetVersionId = "NSL_ShaderVM020";

        public static readonly DiagnosticDescriptor InvalidParameterType = new DiagnosticDescriptor(
            InvalidParameterTypeId, "Shader: invalid parameter type",
            "Parameter '{0}' has type '{1}' not shader-compatible.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor InvalidReturnType = new DiagnosticDescriptor(
            InvalidReturnTypeId, "Shader: invalid return type",
            "Return type '{0}' not shader-compatible.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor InvalidLocalType = new DiagnosticDescriptor(
            InvalidLocalTypeId, "Shader: invalid local variable type",
            "Variable '{0}' has type '{1}' not shader-compatible.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor NonShaderTypeWillBeRemoved = new DiagnosticDescriptor(
            NonShaderTypeWillBeRemovedId, "Shader: type will be removed from output",
            "Expression of type '{0}' has no [ShaderType] attribute and will be removed from shader output.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor NonShaderMethodNotConverted = new DiagnosticDescriptor(
            NonShaderMethodNotConvertedId, "Shader: method has no shader mapping",
            "Method '{0}' has no [ShaderFunction] attribute and will be output as-is in GLSL.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor InvalidFieldType = new DiagnosticDescriptor(
            InvalidFieldTypeId, "Shader: invalid field type",
            "Field '{0}' has type '{1}' not shader-compatible.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor MissingMain = new DiagnosticDescriptor(
            MissingMainId, "Shader: missing Main method",
            "Shader class '{0}' must have a public void Main(IExecutionContext) method.", "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor InvalidTargetVersion = new DiagnosticDescriptor(
            InvalidTargetVersionId, "Shader: unknown target version",
            "Target version '{0}' is not a known Vulkan version. Known: vulkan1.0, vulkan1.1, vulkan1.2, vulkan1.3.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor MinVersionNotSatisfied = new DiagnosticDescriptor(
            MinVersionNotSatisfiedId, "Shader: version requirement not met",
            "Feature requires MinVersion '{0}' but target is '{1}' — will be disabled in output.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor UnmappedField = new DiagnosticDescriptor(
            UnmappedFieldId, "Shader: field has no storage mapping",
            "Field '{0}' has no [ShaderBuffer], [ShaderUniform], or [ShaderShared] attribute and is not const/static-readonly — it will be missing from GLSL output.", "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public static readonly DiagnosticDescriptor TypeWidened = new DiagnosticDescriptor(
            TypeWidenedId, "Shader: type widened for GLSL compatibility",
            "Type '{0}' does not exist in GLSL — widened to '{1}'.", "NSL.ShaderVM", DiagnosticSeverity.Info, true);

        public static readonly DiagnosticDescriptor ForEachUnsupported = new DiagnosticDescriptor(
            ForEachUnsupportedId, "Shader: foreach not supported",
            "foreach is not supported in GLSL — use for/while loop instead. GLSL arrays (SSBO) have no runtime .length().", "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor UnsizedLocalArray = new DiagnosticDescriptor(
            UnsizedLocalArrayId, "Shader: local array requires explicit size",
            "Local array '{0}' has no explicit size — GLSL requires fixed-size arrays (e.g., int arr[16]). Use 'new {1}[N]' instead of '[]'.", "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor GotoUnsupported = new DiagnosticDescriptor(
            GotoUnsupportedId, "Shader: goto not supported",
            "goto is not supported in GLSL — use loop control (break/continue) or restructure.", "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor ArrayInitializerUnsupported = new DiagnosticDescriptor(
            ArrayInitializerUnsupportedId, "Shader: array initializer not supported",
            "Local array initializer '{{ ... }}' is not valid GLSL — use 'new T[N]' with explicit size instead.", "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor DefaultParamUnsupported = new DiagnosticDescriptor(
            DefaultParamUnsupportedId, "Shader: default parameter values not allowed",
            "Default value on parameter '{0}' is forbidden in shader methods — GLSL has no default arguments. Remove '= ...'.",
            "NSL.ShaderVM", DiagnosticSeverity.Error, true);

        public static readonly DiagnosticDescriptor ManagedArrayParam = new DiagnosticDescriptor(
            ManagedArrayParamId, "Shader: Managed function has array parameter",
            "Managed function '{0}' has array parameter '{1}' ({2}) which is not valid GLSL — arrays as function parameters require a fixed size. Use Kind = ShaderFunctionKind.Inline instead, or pass the buffer globally.",
            "NSL.ShaderVM", DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(InvalidParameterType, InvalidReturnType, InvalidLocalType,
                NonShaderTypeWillBeRemoved, NonShaderMethodNotConverted, InvalidFieldType, MissingMain,
                InvalidTargetVersion, MinVersionNotSatisfied, UnmappedField, TypeWidened, ForEachUnsupported, UnsizedLocalArray, GotoUnsupported, ArrayInitializerUnsupported, DefaultParamUnsupported, ManagedArrayParam);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext ctx)
        {
            var cls = (ClassDeclarationSyntax)ctx.Node;
            if (!HasShaderEntry(cls) || HasShaderIgnore(cls)) return;

            string targetVersion = ExtractTargetVersion(ctx, cls);

            bool hasMain = false;
            foreach (var member in cls.Members)
            {
                if (member is MethodDeclarationSyntax method
                    && method.Identifier.Text == "Main"
                    && !HasShaderIgnore(method))
                {
                    hasMain = true;
                    break;
                }
            }

            if (!hasMain)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(MissingMain, cls.Identifier.GetLocation(), cls.Identifier.Text));
                return;
            }

            foreach (var member in cls.Members)
            {
                if (member is FieldDeclarationSyntax field)
                {
                    bool hasAttr = HasShaderFieldAttr(field);
                    bool isConst = field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ConstKeyword));
                    bool isStaticReadonly = field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword))
                                         && field.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ReadOnlyKeyword));
                    bool isIgnored = HasShaderIgnore(field);

                    if (isIgnored) continue;

                    if (!hasAttr && !isConst && !isStaticReadonly)
                    {
                        // Field has no NSL.ShaderVM attribute and is not const/static-readonly — will be missing from GLSL
                        foreach (var v in field.Declaration.Variables)
                            ctx.ReportDiagnostic(Diagnostic.Create(UnmappedField, v.GetLocation(), v.Identifier.Text));
                        continue;
                    }

                    if (hasAttr)
                    {
                        foreach (var v in field.Declaration.Variables)
                        {
                            var ti = ctx.SemanticModel.GetTypeInfo(field.Declaration.Type);
                            if (ti.Type != null && !IsCompatible(ti.Type) && !IsArrayOfCompatible(ti.Type) && !IsShaderIgnoreType(ti.Type))
                                ctx.ReportDiagnostic(Diagnostic.Create(InvalidFieldType, v.GetLocation(), v.Identifier.Text, ti.Type.Name));
                        }
                    }
                }
                else if (member is MethodDeclarationSyntax method && !HasShaderIgnore(method))
                {
                    AnalyzeShaderMethod(ctx, method, targetVersion);
                }
            }
        }

        private string ExtractTargetVersion(SyntaxNodeAnalysisContext ctx, ClassDeclarationSyntax cls)
        {
            foreach (var a in cls.AttributeLists.SelectMany(al => al.Attributes))
            {
                var an = a.Name.ToString();
                if (an != VulkanShaderEntryAttribute.ShortName && an != VulkanShaderEntryAttribute.ShortName + "Attribute")
                    continue;

                foreach (var arg in a.ArgumentList?.Arguments ?? Enumerable.Empty<AttributeArgumentSyntax>())
                {
                    if (arg.NameEquals == null) continue;
                    if (arg.NameEquals.Name.Identifier.Text != "TargetVersion") continue;

                    string v = arg.Expression.ToString().Trim('"');
                    if (string.IsNullOrEmpty(v)) continue;

                    if (!VulkanVersions.IsKnownVersion(v))
                        ctx.ReportDiagnostic(Diagnostic.Create(InvalidTargetVersion, arg.GetLocation(), v));

                    return v;
                }
            }
            return ShaderTargetVersion.Default;
        }

        private void AnalyzeShaderMethod(SyntaxNodeAnalysisContext ctx, MethodDeclarationSyntax method, string targetVersion)
        {
            var sym = ctx.SemanticModel.GetDeclaredSymbol(method);
            if (sym == null) return;

            if (sym.ReturnType.SpecialType != SpecialType.System_Void && !IsCompatible(sym.ReturnType))
                ctx.ReportDiagnostic(Diagnostic.Create(InvalidReturnType, method.ReturnType.GetLocation(), sym.ReturnType.Name));

            foreach (var p in sym.Parameters)
            {
                if (IsShaderIgnoreType(p.Type)) continue;

                // NSL.ShaderVM028: default parameter values — GLSL doesn't support them
                if (p.HasExplicitDefaultValue)
                    ctx.ReportDiagnostic(Diagnostic.Create(DefaultParamUnsupported, p.Locations.FirstOrDefault() ?? method.GetLocation(), p.Name));

                // NSL.ShaderVM023: type widening warning
                string widened = GetWidenedType(p.Type);
                if (widened != null)
                    ctx.ReportDiagnostic(Diagnostic.Create(TypeWidened, p.Locations.FirstOrDefault() ?? method.GetLocation(), p.Type.Name, widened));

                if (IsCompatible(p.Type)) continue;

                ctx.ReportDiagnostic(Diagnostic.Create(InvalidParameterType, method.ParameterList.GetLocation(), p.Name, p.Type.Name));
                ctx.ReportDiagnostic(Diagnostic.Create(NonShaderTypeWillBeRemoved, method.ParameterList.GetLocation(), p.Type.Name));
            }

            if (method.Body != null)
                AnalyzeBodyExpressions(ctx, method.Body, targetVersion);
        }

        private void AnalyzeBodyExpressions(SyntaxNodeAnalysisContext ctx, BlockSyntax body, string targetVersion)
        {
            // NSL.ShaderVM025: local arrays must have explicit size
            foreach (var localDecl in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                var typeInfo = ctx.SemanticModel.GetTypeInfo(localDecl.Declaration.Type);
                if (typeInfo.Type is IArrayTypeSymbol arrType)
                {
                    foreach (var v in localDecl.Declaration.Variables)
                    {
                        // Unsized local array: no initializer, or initializer is null/empty
                        bool isUnsized = v.Initializer == null
                            || v.Initializer.Value is Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax lit
                                && lit.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression);
                        if (isUnsized)
                            ctx.ReportDiagnostic(Diagnostic.Create(UnsizedLocalArray,
                                v.GetLocation(), v.Identifier.Text, arrType.ElementType.Name));
                    }
                }
            }
            foreach (var localDecl in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                var typeInfo = ctx.SemanticModel.GetTypeInfo(localDecl.Declaration.Type);
                if (typeInfo.Type == null) continue;
                if (IsShaderIgnoreType(typeInfo.Type)) continue;

                // NSL.ShaderVM023: type widening
                string widened = GetWidenedType(typeInfo.Type);
                if (widened != null)
                    ctx.ReportDiagnostic(Diagnostic.Create(TypeWidened, localDecl.Declaration.Type.GetLocation(), typeInfo.Type.Name, widened));

                if (IsCompatible(typeInfo.Type)) continue;

                foreach (var v in localDecl.Declaration.Variables)
                    ctx.ReportDiagnostic(Diagnostic.Create(NonShaderTypeWillBeRemoved, v.GetLocation(), typeInfo.Type.Name));
            }

            // NSL.ShaderVM024: foreach not supported in GLSL
            foreach (var forEach in body.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ForEachStatementSyntax>())
                ctx.ReportDiagnostic(Diagnostic.Create(ForEachUnsupported, forEach.ForEachKeyword.GetLocation()));

            // NSL.ShaderVM026: goto not supported in GLSL
            foreach (var gotoStmt in body.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.GotoStatementSyntax>())
                ctx.ReportDiagnostic(Diagnostic.Create(GotoUnsupported, gotoStmt.GetLocation()));

            // NSL.ShaderVM027: array initializer { ... } on local arrays — not valid GLSL
            foreach (var localDecl in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                foreach (var v in localDecl.Declaration.Variables)
                {
                    if (v.Initializer?.Value is Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax)
                        ctx.ReportDiagnostic(Diagnostic.Create(ArrayInitializerUnsupported, v.Initializer.GetLocation()));
                }
            }

            foreach (var invocation in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var symbolInfo = ctx.SemanticModel.GetSymbolInfo(invocation);
                var methodSymbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                if (methodSymbol != null)
                {
                    if (!HasShaderMapping(methodSymbol))
                        ctx.ReportDiagnostic(Diagnostic.Create(NonShaderMethodNotConverted, invocation.GetLocation(), methodSymbol.Name));

                    // NSL.ShaderVM029: calling a Managed function that has array parameters
                    if (methodSymbol is IMethodSymbol ms2 && IsShaderManagedFunction(ms2))
                    {
                        foreach (var p in ms2.Parameters)
                        {
                            if (p.Type is IArrayTypeSymbol arrType)
                                ctx.ReportDiagnostic(Diagnostic.Create(ManagedArrayParam, invocation.GetLocation(), ms2.Name, p.Name, $"{GlslTypeMapper.MapType(arrType.ElementType.Name)}[]"));
                        }
                    }

                    // NSL.ShaderVM021: проверка MinVersion
                    string minVer = ExtractMinVersion(methodSymbol.GetAttributes());
                    if (!string.IsNullOrEmpty(minVer) && !string.IsNullOrEmpty(targetVersion)
                        && !VulkanVersions.Satisfies(targetVersion, minVer))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(MinVersionNotSatisfied, invocation.GetLocation(), minVer, targetVersion));
                    }
                }

                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    var ti = ctx.SemanticModel.GetTypeInfo(arg.Expression);
                    if (ti.Type != null && !IsCompatible(ti.Type) && !IsShaderIgnoreType(ti.Type))
                        ctx.ReportDiagnostic(Diagnostic.Create(NonShaderTypeWillBeRemoved, arg.GetLocation(), ti.Type.Name));
                }
            }

            foreach (var creation in body.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            {
                var ti = ctx.SemanticModel.GetTypeInfo(creation);
                if (ti.Type != null && !IsCompatible(ti.Type) && !IsShaderIgnoreType(ti.Type))
                {
                    ctx.ReportDiagnostic(Diagnostic.Create(NonShaderTypeWillBeRemoved, creation.GetLocation(), ti.Type.Name));

                    // MinVersion проверка для [ShaderType]
                    string minVer = ExtractMinVersion(ti.Type.GetAttributes());
                    if (!string.IsNullOrEmpty(minVer) && !string.IsNullOrEmpty(targetVersion)
                        && !VulkanVersions.Satisfies(targetVersion, minVer))
                    {
                        ctx.ReportDiagnostic(Diagnostic.Create(MinVersionNotSatisfied, creation.GetLocation(), minVer, targetVersion));
                    }
                }
            }

            foreach (var memberAccess in body.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            {
                if (memberAccess.Parent is InvocationExpressionSyntax) continue;

                var memberSymbol = ctx.SemanticModel.GetSymbolInfo(memberAccess).Symbol;
                if (memberSymbol != null)
                {
                    var ma = memberSymbol.GetAttributes();
                    if (ma.Any(a => a.AttributeClass?.Name == "ShaderFieldAttribute" || a.AttributeClass?.Name == "ShaderField"))
                    {
                        // NSL.ShaderVM021: MinVersion на [ShaderField]
                        string minVer = ExtractMinVersion(ma);
                        if (!string.IsNullOrEmpty(minVer) && !string.IsNullOrEmpty(targetVersion)
                            && !VulkanVersions.Satisfies(targetVersion, minVer))
                        {
                            ctx.ReportDiagnostic(Diagnostic.Create(MinVersionNotSatisfied, memberAccess.GetLocation(), minVer, targetVersion));
                        }
                        continue;
                    }
                    if (ma.Any(a => a.AttributeClass?.Name == "ShaderFunctionAttribute" || a.AttributeClass?.Name == "ShaderFunction"))
                        continue;

                    // NSL.ShaderVM005: property/field without [ShaderFunction]/[ShaderField] on non-[ShaderType] containing type
                    // e.g., arr.Length — Length is not [ShaderFunction], System.Array is not [ShaderType]
                    var containingType = memberSymbol.ContainingType;
                    if (containingType != null && !IsShaderIgnoreType(containingType))
                    {
                        bool containingIsShaderType = containingType.GetAttributes()
                            .Any(a => a.AttributeClass?.Name == "ShaderTypeAttribute" || a.AttributeClass?.Name == "ShaderType");
                        if (!containingIsShaderType)
                            ctx.ReportDiagnostic(Diagnostic.Create(NonShaderMethodNotConverted, memberAccess.GetLocation(), memberSymbol.Name));
                    }
                }

                var exprType = ctx.SemanticModel.GetTypeInfo(memberAccess.Expression);
                if (exprType.Type != null && !IsCompatible(exprType.Type) && !IsShaderIgnoreType(exprType.Type))
                    ctx.ReportDiagnostic(Diagnostic.Create(NonShaderTypeWillBeRemoved, memberAccess.GetLocation(), exprType.Type.Name));
            }
        }

        // ── helpers ──

        private static string ExtractMinVersion(System.Collections.Immutable.ImmutableArray<AttributeData> attrs)
        {
            foreach (var na in attrs.SelectMany(a => a.NamedArguments))
                if (na.Key == "MinVersion") return na.Value.Value?.ToString();
            return null;
        }

        private static bool IsShaderIgnoreType(ITypeSymbol type)
            => type.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ShaderIgnoreAttribute"
                || a.AttributeClass?.Name == "ShaderIgnore");

        private static bool IsCompatible(ITypeSymbol t)
        {
            if (GlslTypeMapper.IsShaderType(t.Name) || GlslTypeMapper.IsShaderType(t.ToString()))
                return true;
            if (t.GetAttributes().Any(a =>
                a.AttributeClass?.Name == "ShaderTypeAttribute"
                || a.AttributeClass?.Name == "ShaderType"))
                return true;
            return false;
        }

        private static bool IsArrayOfCompatible(ITypeSymbol t) =>
            t is IArrayTypeSymbol arr && IsCompatible(arr.ElementType);

        /// <summary>Check if a method has [ShaderFunction(Kind = Managed)] — these are emitted as standalone GLSL functions.</summary>
        private static bool IsShaderManagedFunction(IMethodSymbol method)
        {
            foreach (var attr in method.GetAttributes())
            {
                var an = attr.AttributeClass?.Name;
                if (an != "ShaderFunctionAttribute" && an != "ShaderFunction") continue;
                foreach (var na in attr.NamedArguments)
                {
                    if (na.Key == "Kind" && na.Value.Value is int kv && kv == 1) return true; // ShaderFunctionKind.Managed = 1
                    // Backward compat
                    if (na.Key == "CSharpCode" && na.Value.Value is bool b && b) return true;
                }
            }
            return false;
        }

        /// <summary>Returns the GLSL type name if the C# type is widened (e.g., byte→int), null otherwise.</summary>
        private static string GetWidenedType(ITypeSymbol t)
        {
            string csName = t.Name;
            string glsl = GlslTypeMapper.MapType(csName);
            // If the mapped type differs from the C# type name AND it's one of the small integer types
            if (glsl != csName && (csName == "Byte" || csName == "SByte" || csName == "Int16" || csName == "UInt16"
                || csName == "byte" || csName == "sbyte" || csName == "short" || csName == "ushort"))
                return glsl;
            return null;
        }

        private static bool HasShaderEntry(ClassDeclarationSyntax cls) =>
            cls.AttributeLists.SelectMany(al => al.Attributes).Any(a =>
                a.Name.ToString() == VulkanShaderEntryAttribute.ShortName || a.Name.ToString() == VulkanShaderEntryAttribute.ShortName + "Attribute");

        private static bool HasShaderIgnore(MemberDeclarationSyntax m) =>
            m.AttributeLists.SelectMany(al => al.Attributes).Any(a =>
                a.Name.ToString() == "ShaderIgnore" || a.Name.ToString() == "ShaderIgnoreAttribute");

        private static bool HasShaderIgnore(ClassDeclarationSyntax c) =>
            c.AttributeLists.SelectMany(al => al.Attributes).Any(a =>
                a.Name.ToString() == "ShaderIgnore" || a.Name.ToString() == "ShaderIgnoreAttribute");

        private static bool HasShaderMapping(ISymbol methodSymbol)
        {
            if (methodSymbol == null) return true;
            var attrs = methodSymbol.GetAttributes();
            if (attrs.Any(a => a.AttributeClass?.Name == "ShaderFunctionAttribute" || a.AttributeClass?.Name == "ShaderFunction"))
                return true;
            if (attrs.Any(a => a.AttributeClass?.Name == "ShaderIgnoreAttribute" || a.AttributeClass?.Name == "ShaderIgnore"))
                return true;
            if (methodSymbol.ContainingType != null && IsShaderIgnoreType(methodSymbol.ContainingType))
                return true;
            if (methodSymbol.ContainingType != null && IsShaderClassType(methodSymbol.ContainingType))
                return true;
            return false;
        }

        private static bool IsShaderClassType(ITypeSymbol type)
            => type.GetAttributes().Any(a =>
                a.AttributeClass?.Name == VulkanShaderEntryAttribute.ShortName
                || a.AttributeClass?.Name == VulkanShaderEntryAttribute.ShortName + "Attribute");

        private static bool HasShaderFieldAttr(FieldDeclarationSyntax field) =>
            field.AttributeLists.SelectMany(al => al.Attributes).Any(a =>
                a.Name.ToString() == "ShaderBuffer" || a.Name.ToString() == "ShaderBufferAttribute"
                || a.Name.ToString() == "ShaderUniform" || a.Name.ToString() == "ShaderUniformAttribute"
                || a.Name.ToString() == "ShaderPushConstant" || a.Name.ToString() == "ShaderPushConstantAttribute"
                || a.Name.ToString() == "ShaderShared" || a.Name.ToString() == "ShaderSharedAttribute");
    }
}
