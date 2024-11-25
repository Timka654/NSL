﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.FillTypeGenerator.Attributes;
using NSL.Generators.FillTypeGenerator.Utils;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.FillTypeGenerator
{

    [Generator]
    internal class FillTypeGenerator : ISourceGenerator
    {
        #region ISourceGenerator

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is FillTypeAttributeSyntaxReceiver methodSyntaxReceiver)
            {
                ProcessFillTypes(context, methodSyntaxReceiver);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new FillTypeAttributeSyntaxReceiver());
        }

        #endregion

        private void ProcessFillTypes(GeneratorExecutionContext context, FillTypeAttributeSyntaxReceiver methodSyntaxReceiver)
        {
            //GenDebug.Break();

            foreach (var item in methodSyntaxReceiver.FillTypeTypes)
            {
                try
                {
                    ProcessFillToType(context, item);
                }
                catch (Exception ex)
                {
                    context.ShowFillTypeDiagnostics($"NSLFT002", $"Error - {ex} on type {item.Identifier.Text}", DiagnosticSeverity.Error, item.GetLocation());
                }
            }
        }

        private static string[] requiredUsings = new string[] { "System.Linq" };

        private void ProcessFillToType(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            if (!type.HasPartialModifier())
            {
                context.ShowFillTypeDiagnostics("NSLFT000", "Type must have a partial modifier", DiagnosticSeverity.Error, type.GetLocation());

                return;
            }
            //GenDebug.Break();

            var typeClass = type as ClassDeclarationSyntax;

            //Debug.WriteLine($"FillTypeGenerator -> {typeClass.Identifier.Text}");

            var typeSem = context.Compilation.GetSemanticModel(typeClass.SyntaxTree);

            var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

            var classBuilder = new CodeBuilder();

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL FillType. Please don't change this file");
            });

            classBuilder.CreatePartialClass(typeClass, () =>
            {
                var attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(FillTypeGenerateAttributeFullName))
                .ToArray();

                //if (attrbs.Any(x => !x.ArgumentList.Arguments.Any()))
                //    GenDebug.Break();

                var attrbsGrouped = attrbs.GroupBy(x => x.ArgumentList.Arguments.First().GetAttributeTypeParameterValueSymbol(typeSem), TypeSymbolEqualityComparer.Instance);


                foreach (var attr in attrbsGrouped)
                {
                    ProcessAttribute(context, classBuilder, attr, typeSymb, typeSem, true);
                }

                attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(FillTypeFromGenerateAttributeFullName))
                .ToArray();

                //if (attrbs.Any(x => !x.ArgumentList.Arguments.Any()))
                //    GenDebug.Break();

                attrbsGrouped = attrbs.GroupBy(x => x.ArgumentList.Arguments.First().GetAttributeTypeParameterValueSymbol(typeSem), TypeSymbolEqualityComparer.Instance);


                foreach (var attr in attrbsGrouped)
                {
                    ProcessAttribute(context, classBuilder, attr, typeSymb, typeSem, false);
                }

            }, requiredUsings);

            // Visual studio have lag(or ...) cannot show changes sometime
            //#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //System.IO.File.WriteAllText($@"C:\Work\temp\{typeClass.GetTypeClassName()}.filltype.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            //GenDebug.Break();

            context.AddSource($"{typeClass.GetTypeClassName()}.filltype.cs", classBuilder.ToString());
        }

        private void ProcessAttribute(GeneratorExecutionContext context, CodeBuilder classBuilder, IGrouping<ITypeSymbol, AttributeSyntax> attr, ITypeSymbol typeSymb, SemanticModel typeSem, bool dir)
        {
            var haveAll = attr.Any(x => x.ArgumentList.Arguments.Count() == 1);

            var models = attr.SelectMany(x => x.ArgumentList.Arguments
                .Skip(1)
                .Select(n => n.GetAttributeParameterValue<string>(typeSem)))
            .GroupBy(x => x)
            .Select(x => x.Key)
            .ToArray();

            if (models.Contains(null))
                haveAll = false;

            var toType = attr.Key;

            //if (!toType.DeclaringSyntaxReferences.Any())
            //    GenDebug.Break();

            bool isInternal = (toType.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as TypeDeclarationSyntax)?.HasInternalModifier() ?? false;

            if (haveAll)
                classBuilder.AppendLine(CreateMethod(context, isInternal, typeSymb, toType, null, dir).ToString());

            if (models.Any())
            {
                var methods = new List<string>();

                foreach (var item in models)
                {
                    methods.Add(CreateMethod(context, isInternal, typeSymb, toType, item, dir).ToString());
                }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                classBuilder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, methods));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers declaration.HasInternalModifier()
            }
        }

        private CodeBuilder CreateMethod(GeneratorExecutionContext context, bool @internal, ITypeSymbol fromType, ITypeSymbol toType, string model, bool dir)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            methodBuilder.AppendLine($"{(@internal ? "internal" : "public")} void Fill{model}{(dir ? "To" : "From")}({toType.Name} {(dir ? "to" : "from")}Fill)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            List<string> memberLines = new List<string>();

            if (dir)
                FillMembers(context, memberLines, fromType, toType, model, "toFill", null, dir, 0);
            else
                FillMembers(context, memberLines, fromType, toType, model, null, "fromFill", dir, 0);

            foreach (var ml in memberLines)
            {
                methodBuilder.AppendLine($"{ml};");
            }

            methodBuilder.PrevTab();

            methodBuilder.AppendLine("}");

            return methodBuilder;
        }

        private IEnumerable<ISymbol> FilterSymbols(IEnumerable<ISymbol> symbols, string model)
        {
            if (model == default)
                return symbols;

            return symbols.Where(x =>
            {
                var a = x.GetAttributes().FirstOrDefault(n => n.AttributeClass.Name == (FillTypeGenerateIncludeAttributeFullName));

                if (a == null)
                    return false;

                if (a.ConstructorArguments.SelectMany(n => n.Values).Any(n => (n.Value as string).Equals(model)))
                    return true;

                return false;
            });
        }

        private string GetProxyModel(ISymbol item, string model)
        {
            //GenDebug.Break();

            var attributes = item.GetAttributes();

            var proxyAttribs = attributes.Where(x => x.AttributeClass.Name == FillTypeGenerateProxyAttributeFullName).ToArray();

            var fromModel = proxyAttribs.FirstOrDefault(x => x.ConstructorArguments.Length == 2 && x.ConstructorArguments.First().Value == model);

            if (fromModel != null)
                return (string)fromModel.ConstructorArguments[1].Value;
            else
            {
                //GenDebug.Break();

                var toModel = proxyAttribs.FirstOrDefault(x => x.ConstructorArguments.Length == 1);

                if (toModel != null)
                {
                    return (string)toModel.ConstructorArguments.First().Value;
                }
            }

            return model;
        }

        private ITypeSymbol GetFromTypeForFill(ISymbol fromItem, ITypeSymbol toType, bool dir)
        {
            if (fromItem is IPropertySymbol ps)
            {
                var ignore = !dir ? false : ps.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(FillTypeGenerateIgnoreAttributeFullName))
                .Any(q => !q.ConstructorArguments.Any() || q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(toType.MetadataName)));

                if (!ignore)
                {
                    //GenDebug.Break();

                    if (ps.GetMethod != null)
                        return ps.Type;
                }
            }
            else if (fromItem is IFieldSymbol fs)
            {
                var ignore = !dir ? false : fs.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(FillTypeGenerateIgnoreAttributeFullName))
                .Any(q => !q.ConstructorArguments.Any() || q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(toType.MetadataName)));

                if (!ignore)
                    return fs.Type;
            }

            return default;
        }

        private ITypeSymbol GetToTypeForFill(ISymbol toItem, ITypeSymbol fromType, bool dir)
        {
            if (toItem is IPropertySymbol ps)
            {
                var ignore = dir ? false : ps.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(FillTypeGenerateIgnoreAttributeFullName))
                .Any(q => !q.ConstructorArguments.Any() || q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(fromType.MetadataName)));

                if (!ignore)
                {
                    //GenDebug.Break();

                    if (ps.GetMethod != null)
                        return ps.Type;
                }
            }
            else if (toItem is IFieldSymbol fs)
            {
                var ignore = dir ? false : fs.GetAttributes()
                .Where(x => x.AttributeClass.Name.Equals(FillTypeGenerateIgnoreAttributeFullName))
                .Any(q => !q.ConstructorArguments.Any() || q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(fromType.MetadataName)));

                if (!ignore)
                    return fs.Type;
            }

            return default;
        }

        private ITypeSymbol GetCollectionItemType(ITypeSymbol type)
        {
            if (type.Name == stringFullName)
                return default;

            if (type is IArrayTypeSymbol arrt)
                return arrt.ElementType;

            if ((type.MetadataName.Equals(typeof(List<>).Name)
                || type.MetadataName.Equals(typeof(IList<>).Name)) && type is INamedTypeSymbol nt)
                return nt.TypeArguments.First();

            return default;
        }

        private string[] basicArrayTypes = new string[] {
            typeof(byte).Name,
            typeof(char).Name,
            typeof(sbyte).Name,
            typeof(ushort).Name,
            typeof(short).Name,
            typeof(uint).Name,
            typeof(int).Name,
            typeof(ulong).Name,
            typeof(long).Name,
            typeof(string).Name,
            typeof(Guid).Name
        };

        private string GetCollectionLinqConvertMethod(ITypeSymbol type)
        {
            if (type is IArrayTypeSymbol arrt)
                return "ToArray";

            if ((type.MetadataName.Equals(typeof(List<>).Name)
                || type.MetadataName.Equals(typeof(IList<>).Name)) && type is INamedTypeSymbol nt)
                return "ToList";

            return default;
        }

        private void FillMembers(GeneratorExecutionContext context, List<string> codeLines, ITypeSymbol fromType, ITypeSymbol toType, string model, string fillPath, string readPath, bool dir, int t)
        {
            if (!dir)
            {
                (toType, fromType) = (fromType, toType);
            }

            var fromMembers = FilterSymbols(fromType.GetAllMembers(), model);
            var toMembers = (IEnumerable<ISymbol>)toType.GetAllMembers();


            var tabPrefix = string.Concat(Enumerable.Repeat("\t", t));

            string itemModel;

            foreach (var fromItem in fromMembers)
            {
                if (fromItem is IPropertySymbol fprop && fprop.GetMethod == null)
                    continue;

                var toItem = toMembers.FirstOrDefault(x => x.Name.Equals(fromItem.Name) && x.DeclaredAccessibility == Accessibility.Public);

                if (toItem == default)
                    continue;

                if (toItem is IPropertySymbol tprop && tprop.SetMethod == null)
                    continue;

                ITypeSymbol memberFromType = GetFromTypeForFill(fromItem, toType, dir);

                ITypeSymbol memberToType = GetToTypeForFill(toItem, fromType, dir);

                if (memberFromType == null || memberToType == null)
                    continue;

                string mFillPath = $"{tabPrefix}{string.Join(".", fillPath, fromItem.Name).TrimStart('.')} = ";

                string codeFragment = default;

                var arrayItemTypeFrom = GetCollectionItemType(memberFromType);
                var arrayItemTypeTo = GetCollectionItemType(memberToType);

                if (arrayItemTypeFrom != default)
                {
                    itemModel = GetProxyModel(fromItem, model);

                    int n = 0;

                    string p = string.Empty;

                    if (readPath == null)
                        p = $"x{n}";
                    else
                        while (readPath.Contains(p = $"x{n++}")) { }

                    if (basicArrayTypes.Contains(arrayItemTypeFrom.Name) || arrayItemTypeFrom.TypeKind == TypeKind.Enum)
                    {
                        codeFragment = $"{string.Join(".", readPath, fromItem.Name).TrimStart('.')}?.Select({p} => {p}).{GetCollectionLinqConvertMethod(memberToType)}()";
                    }
                    else
                    {
                        var amem = new List<string>();

                        FillMembers(context, amem, arrayItemTypeFrom, arrayItemTypeTo, itemModel, null, p, dir, 1);

                        if (!Equals(model, itemModel))
                            codeLines.Add($"// Proxy model merge from \"{model}\" to \"{itemModel}\"");

                        //GenDebug.Break();

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов

                        codeFragment = $"{string.Join(".", readPath, fromItem.Name).TrimStart('.')}?.Select({p} => new {arrayItemTypeTo} {{{Environment.NewLine}" +
                            string.Join($",{Environment.NewLine}", amem) +
                            $"{Environment.NewLine}}}).{GetCollectionLinqConvertMethod(memberToType)}()";

#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов

                        //                        codeLines.Add($"{item.Name} = {path}.{item.Name} == null ? null : {path}.{item.Name}.Select({p} => new {{{Environment.NewLine}{CombineMembers(amem.Select(x => $"\t{x}"))}{Environment.NewLine}}})");

                    }
                }
                else
                {

                    var conversation = context.Compilation.ClassifyCommonConversion(memberFromType, memberToType);

                    if (!conversation.Exists)
                    {

                        var msg = $"Cannot fill \"{toItem.Name}\" value from {fromType.Name}, members types must be equals, or can be cast, or must be marked for ignore";

                        context.ShowFillTypeDiagnostics("NSLFT001"
                            , msg
                            , DiagnosticSeverity.Error
                            , toItem.Locations.ToArray());

                        //GenDebug.Break();

                        continue;
                    }

                    codeFragment = string.Join(".", readPath, fromItem.Name).TrimStart('.');

                    if (conversation.IsNumeric)
                        codeFragment = $"({memberToType.GetTypeFullName()}){codeFragment}";
                    else if (conversation.IsImplicit) { }

                }
                //else
                //    GenDebug.Break();

                var result = $"{mFillPath}{codeFragment}";

                if (!codeLines.Contains(result))
                    codeLines.Add(result);
            }
        }


        internal static readonly string FillTypeGenerateAttributeFullName = typeof(FillTypeGenerateAttribute).Name;
        internal static readonly string FillTypeFromGenerateAttributeFullName = typeof(FillTypeFromGenerateAttribute).Name;
        internal static readonly string FillTypeGenerateIgnoreAttributeFullName = typeof(FillTypeGenerateIgnoreAttribute).Name;
        internal static readonly string FillTypeGenerateIncludeAttributeFullName = typeof(FillTypeGenerateIncludeAttribute).Name;
        internal static readonly string FillTypeGenerateProxyAttributeFullName = typeof(FillTypeGenerateProxyAttribute).Name;
        internal static readonly string stringFullName = typeof(string).Name;
    }
}
