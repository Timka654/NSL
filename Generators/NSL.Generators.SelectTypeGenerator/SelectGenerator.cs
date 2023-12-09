﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.SelectTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace NSL.Generators.SelectTypeGenerator
{
    [Generator]
    internal class SelectGenerator : ISourceGenerator
    {
        #region ISourceGenerator

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is SelectAttributeSyntaxReceiver methodSyntaxReceiver)
            {
                ProcessFillTypes(context, methodSyntaxReceiver);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new SelectAttributeSyntaxReceiver());
        }

        #endregion

        private void ProcessFillTypes(GeneratorExecutionContext context, SelectAttributeSyntaxReceiver methodSyntaxReceiver)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            foreach (var item in methodSyntaxReceiver.SelectTypes)
            {
                ProcessSelectToType(context, item);
            }
        }

        private void ProcessSelectToType(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            var typeClass = type as ClassDeclarationSyntax;

            var typeSem = context.Compilation.GetSemanticModel(typeClass.SyntaxTree);

            var classBuilder = new CodeBuilder();

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL Select. Please don't change this file");
            });

            classBuilder.CreateStaticClass(typeClass, $"{typeClass.GetClassName()}_Selection", () =>
            {
                var attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(SelectGenerateAttributeFullName))
                .ToArray();

                foreach (var attr in attrbs)
                {
                    //if (!Debugger.IsAttached)
                    //    Debugger.Launch();

                    var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

                    var selectArgs = attr.ArgumentList.Arguments;

                    string[] models = selectArgs.Select(x => x.GetAttributeParameterValue<string>(typeSem)).ToArray();

                    var members = typeSymb.GetAllMembers();

                    var methods = new List<string>();

                    foreach (var item in models)
                    {
                        CreateMethods(methods, typeSymb, FilterSymbols(members, item), item);
                    }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                    classBuilder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, methods));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
                }
            });

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            // Visual studio have lag(or ...) cannot show changes sometime
            //#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //System.IO.File.WriteAllText($@"C:\Work\temp\{typeClass.GetTypeClassName()}.selectgen.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            context.AddSource($"{typeClass.GetTypeClassName()}.selectgen.cs", classBuilder.ToString());
        }

        private IEnumerable<ISymbol> FilterSymbols(IEnumerable<ISymbol> symbols, string model)
            => symbols.Where(x =>
         {
             var a = x.GetAttributes().FirstOrDefault(n => n.AttributeClass.Name == SelectGenerateIncludeAttributeFullName);

             if (a == null)
                 return false;

             if (a.ConstructorArguments.SelectMany(n => n.Values).Any(n => (n.Value as string).Equals(model)))
                 return true;

             return false;
         });

        private void CreateMethods(List<string> methods, ITypeSymbol type, IEnumerable<ISymbol> members, string model)
        {

            var sMembers = new List<string>();

            ReadMembers(members, sMembers, model, "x");

            methods.Add(CreateMethod(type, sMembers, model, "IEnumerable").ToString());
            methods.Add(CreateMethod(type, sMembers, model, "IQueryable").ToString());

        }

        private CodeBuilder CreateMethod(ITypeSymbol type, IEnumerable<string> sMembers, string model, string collectionType)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            //methodBuilder.AppendLine($"{(declaration.HasInternalModifier() ? "internal" : "public")} {toType.Name} Select{model}Create()");

            //methodBuilder.AppendLine("{");

            //methodBuilder.NextTab();



            //methodBuilder.PrevTab();

            //methodBuilder.AppendLine("}");

            //methodBuilder.AppendLine();

            methodBuilder.AppendLine($"public static {collectionType}<dynamic> Select{model}(this {collectionType}<{type.Name}> toSelect)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            methodBuilder.AppendLine("return toSelect.Select(x=> new {");

            methodBuilder.NextTab();


#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
            methodBuilder.AppendLine(CombineMembers(sMembers));
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов

            methodBuilder.PrevTab();

            methodBuilder.AppendLine("});");

            methodBuilder.PrevTab();

            methodBuilder.AppendLine("}");

            return methodBuilder;
        }

        private string CombineMembers(IEnumerable<string> members)
        {

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
            return string.Join($",{Environment.NewLine}", members);
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
        }

        private string GetProxyModel(ISymbol item, string model)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            string proxyModel = default;

            var attributes = item.GetAttributes();

            var proxyAttribs = attributes.Where(x => x.AttributeClass.Name == SelectGenerateProxyAttributeFullName).ToArray();

            var fromModel = proxyAttribs.FirstOrDefault(x => x.ConstructorArguments.Length == 2 && x.ConstructorArguments.First().Value == model);

            if (fromModel != null)
                proxyModel = (string)fromModel.ConstructorArguments[1].Value;
            else
            {
                var toModel = proxyAttribs.FirstOrDefault(x => x.ConstructorArguments.Length == 1);

                if (toModel != null)
                    proxyModel = (string)toModel.ConstructorArguments.First().Value;
            }

            return proxyModel ?? model;
        }

        private void ReadMembers(IEnumerable<ISymbol> members, List<string> sMembers, string model, string path)
        {
            string itemModel;

            foreach (var item in members)
            {
                ITypeSymbol memberType;
                //var fMember = toMembers.FirstOrDefault(x => x.Name.Equals(item.Name) && x.DeclaredAccessibility == Accessibility.Public);

                //if (fMember == default || fMember is IPropertySymbol fps && fps.SetMethod == default)
                //    continue;

                if (item is IPropertySymbol ps)
                {
                    //var ignore = ps.GetAttributes()
                    //.Where(x => x.AttributeClass.Name.Equals(SelectGenerateIgnoreAttributeFullName))
                    //.Any(q => q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(toType.MetadataName)));

                    //if (ignore)
                    //    continue;

                    if (ps.GetMethod == null)
                        continue;

                    memberType = ps.Type;
                }
                else if (item is IFieldSymbol fs)
                {
                    //var ignore = fs.GetAttributes()
                    //.Where(x => x.AttributeClass.Name.Equals(SelectGenerateIgnoreAttributeFullName))
                    //.Any(q => q.ConstructorArguments.Any(x => (x.Value as INamedTypeSymbol).MetadataName.Equals(toType.MetadataName)));

                    //if (ignore)
                    //    continue;
                    memberType = fs.Type;
                }
                else
                    continue;

                string typeName = default;

                INamedTypeSymbol type = memberType as INamedTypeSymbol;

                if (type != null)
                    typeName = type.MetadataName;
                else
                {
                    var arrType = memberType as IArrayTypeSymbol;

                    typeName = $"{arrType.ElementType.Name}[]";
                }


                if (memberType is IArrayTypeSymbol arrt)
                {
                    var amembers = FilterSymbols(arrt.ElementType.GetAllMembers(), model);

                    if (amembers.Any())
                    {
                        itemModel = GetProxyModel(item, model);

                        int n = 0;
                        string p = string.Empty;
                        while (path.Contains(p = $"x{n++}")) { }

                        var amem = new List<string>();

                        ReadMembers(amembers, amem, itemModel, p);

                        if (!Equals(model, itemModel))
                            sMembers.Add($"// Proxy model merge from \"{model}\" to \"{itemModel}\"");

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
                        sMembers.Add($"{item.Name} = {path}.{item.Name} == null ? null : {path}.{item.Name}.Select({p} => new {{{Environment.NewLine}{CombineMembers(amem.Select(x => $"\t{x}"))}{Environment.NewLine}}})");
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
                    }
                    else
                    {
                        sMembers.Add($"{path}.{item.Name}");
                    }


                    continue;
                }
                else if (type.MetadataName.Equals(typeof(List<>).Name) || type.MetadataName.Equals(typeof(IList<>).Name))
                {
                    var pType = type.TypeArguments.First();

                    var amembers = FilterSymbols(pType.GetAllMembers(), model);

                    if (amembers.Any())
                    {
                        itemModel = GetProxyModel(item, model);

                        int n = 0;
                        string p = string.Empty;
                        while (path.Contains(p = $"x{n++}")) { }

                        var amem = new List<string>();

                        ReadMembers(amembers, amem, itemModel, p);

                        if (!Equals(model, itemModel))
                            sMembers.Add($"// Proxy model merge from \"{model}\" to \"{itemModel}\"");

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
                        sMembers.Add($"{item.Name} = {path}.{item.Name} == null ? null : {path}.{item.Name}.Select({p} => new {{{Environment.NewLine}{CombineMembers(amem.Select(x => $"\t{x}"))}{Environment.NewLine}}})");
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
                    }
                    else
                    {
                        sMembers.Add($"{path}.{item.Name}");
                    }


                    continue;
                }


                itemModel = GetProxyModel(item, model);

                var memMembers = FilterSymbols(memberType.GetAllMembers(), itemModel);

                //if (item.Name.StartsWith("AbcModel1"))
                //{
                //    if (!Debugger.IsAttached)
                //        Debugger.Launch();

                //    Debugger.Break();
                //}

                if (memMembers.Any())
                {
                    var nMembers = new List<string>();

                    ReadMembers(memMembers, nMembers, itemModel, $"{path}.{item.Name}");

                    if (!Equals(model, itemModel))
                        sMembers.Add($"// Proxy model merge from \"{model}\" to \"{itemModel}\"");

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
                    sMembers.Add($"{item.Name} = {path}.{item.Name} == null ? null : new {{{Environment.NewLine}{CombineMembers(nMembers.Select(x => $"\t{x}"))}{Environment.NewLine}}}");
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
                }
                else
                    sMembers.Add($"{path}.{item.Name}");
            }
        }

        private readonly string SelectGenerateAttributeFullName = typeof(SelectGenerateAttribute).Name;
        private readonly string SelectGenerateIncludeAttributeFullName = typeof(SelectGenerateIncludeAttribute).Name;
        private readonly string SelectGenerateProxyAttributeFullName = typeof(SelectGenerateProxyAttribute).Name;
    }
}
