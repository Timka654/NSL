﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.SelectTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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
            //GenDebug.Break();

            foreach (var item in methodSyntaxReceiver.SelectTypes)
            {
                ProcessSelectToType(context, item);
            }
        }

        private void ProcessSelectToType(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            var typeClass = type as ClassDeclarationSyntax;

            var typeSem = context.Compilation.GetSemanticModel(typeClass.SyntaxTree);

            var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

            var members = typeSymb.GetAllMembers();

            var classBuilder = new CodeBuilder();

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL Select. Please don't change this file");
            });

            var originNamespace = (typeClass.Parent as NamespaceDeclarationSyntax)?.Name.ToString();

            List<string> namespaces = new List<string>();

            if (originNamespace != null)
            {
                namespaces.Add(originNamespace);
                namespaces.Add("System.Collections.Generic");
            }

            classBuilder.CreateStaticClass(typeClass, $"{typeClass.GetClassName()}_Selection", () =>
            {
                var attrbs = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(SelectGenerateAttributeFullName))
                .ToArray();

                var typeSelectModels = attrbs
                .SelectMany(x => x.ArgumentList.Arguments.Select(n => n.GetAttributeParameterValue<string>(typeSem)))
                .GroupBy(x => x)
                .Select(x => x.Key)
                .ToArray();

                var joins = typeClass.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(SelectGenerateModelJoinAttributeFullName))
                .Select(x => x.ArgumentList)
                .ToArray();
#if DEBUG
                //GenDebug.Break();
#endif

                var methods = new List<string>();

                foreach (var item in typeSelectModels)
                {
                    var mjoins = joins
                    .Where(x => x.Arguments.First().GetAttributeParameterValue<string>(typeSem).Equals(item))
                    .SelectMany(x => x.Arguments.Skip(1).Select(n => n.GetAttributeParameterValue<string>(typeSem)))
                    .GroupBy(x => x)
                    .Select(x => x.Key)
                    .Append(item)
                    .ToArray();

                    CreateMethods(methods, typeSymb, FilterSymbols(members, mjoins), item);
                }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                classBuilder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, methods));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

            }, namespaces, @namespace: "System.Linq");

            //GenDebug.Break();

            // Visual studio have lag(or ...) cannot show changes sometime
            //#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //System.IO.File.WriteAllText($@"C:\Work\temp\{typeClass.GetTypeClassName()}.selectgen.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            context.AddSource($"{typeClass.GetTypeClassName()}.selectgen.cs", classBuilder.ToString());
        }

        private IEnumerable<ISymbol> FilterSymbols(IEnumerable<ISymbol> symbols, IEnumerable<string> joinedArr)
            => symbols.Where(x =>
         {
             var a = x
             .GetAttributes()
             .Where(n => n.AttributeClass.Name == SelectGenerateIncludeAttributeFullName)
             .SelectMany(b => b.ConstructorArguments)
             .SelectMany(b => b.Values)
             .ToArray();

             if (a.Any(n => joinedArr.Contains(n.Value as string)))
                 return true;

             return false;
         });

        private void CreateMethods(List<string> methods, ITypeSymbol type, IEnumerable<ISymbol> members, string model)
        {

            var sMembers = new List<string>();

            ReadMembers(members, sMembers, model, "___x");

            methods.Add(CreateMethod(type, sMembers, model, "IEnumerable").ToString());
            methods.Add(CreateMethod(type, sMembers, model, "IQueryable").ToString());
            methods.Add(CreateMethod(type, sMembers, model).ToString());

        }

        private CodeBuilder CreateMethod(ITypeSymbol type, IEnumerable<string> sMembers, string model)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            //methodBuilder.AppendLine($"{(declaration.HasInternalModifier() ? "internal" : "public")} {toType.Name} Select{model}Create()");

            //methodBuilder.AppendLine("{");

            //methodBuilder.NextTab();



            //methodBuilder.PrevTab();

            //methodBuilder.AppendLine("}");

            //methodBuilder.AppendLine();

            methodBuilder.AppendLine($"public static dynamic To{model}(this {type.GetTypeFullName()} ___x)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            methodBuilder.AppendLine("return new {");

            methodBuilder.NextTab();


#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
            methodBuilder.AppendLine(CombineMembers(sMembers));
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов

            methodBuilder.PrevTab();

            methodBuilder.AppendLine("};");

            methodBuilder.PrevTab();

            methodBuilder.AppendLine("}");

            return methodBuilder;
        }

        private CodeBuilder CreateMethod(ITypeSymbol type, IEnumerable<string> sMembers, string model, string collectionType)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            methodBuilder.AppendLine($"public static {collectionType}<dynamic> Select{model}(this {collectionType}<{type.GetTypeFullName()}> ___toSelect)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            methodBuilder.AppendLine("return ___toSelect.Select(___x=> new {");

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

        private string[] GetJoinModels(ISymbol item, string model)
        {
            var attributes = item.GetAttributes();

            var joined = attributes
                        .Where(x => x.AttributeClass.Name == SelectGenerateModelJoinAttributeFullName)
                        .Where(x => (x.ConstructorArguments.First().Value as string).Equals(model))
                        .SelectMany(x => x.ConstructorArguments.ElementAt(1).Values.Select(q => q.Value as string))
                        .Append(model)
                        .ToArray();

            return joined;
        }
        private string GetProxyModel(ISymbol item, string model)
        {
            //GenDebug.Break();

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

        private void ReadCollection(List<string> sMembers, ISymbol item, IEnumerable<ISymbol> amembers, string model, string itemModel, string path)
        {
            if (amembers.Any())
            {

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
                sMembers.Add($"{path}.{item.Name}");
        }

        private void ReadMembers(IEnumerable<ISymbol> members, List<string> sMembers, string model, string path)
        {
            string itemModel;

            foreach (var item in members)
            {
                ITypeSymbol memberType;

                if (item is IPropertySymbol ps)
                {

                    if (ps.GetMethod == null)
                        continue;

                    memberType = ps.Type;
                }
                else if (item is IFieldSymbol fs)
                {
                    memberType = fs.Type;
                }
                else
                    continue;

                string typeName = default;

                IEnumerable<string> joined;

                INamedTypeSymbol type = memberType as INamedTypeSymbol;

                if (type != null)
                    typeName = type.MetadataName;
                else
                {
                    var arrType = memberType as IArrayTypeSymbol;

                    typeName = $"{arrType.ElementType.Name}[]";
                }

                //#if DEBUG
                //   GenDebug.Break();
                //#endif

                //if (item.Name.StartsWith("JP"))
                //{
                //    GenDebug.Break();
                //}

                if (memberType is IArrayTypeSymbol arrt)
                {
                    itemModel = GetProxyModel(item, model);

                    joined = GetJoinModels(arrt.ElementType, itemModel);

                    sMembers.Add($"// Join {itemModel} to [{string.Join(",", joined)}]");

                    var amembers = FilterSymbols(arrt.ElementType.GetAllMembers(), joined);

                    ReadCollection(sMembers, item, amembers, model, itemModel, path);

                    continue;
                }
                else if (type.MetadataName.Equals(typeof(List<>).Name) || type.MetadataName.Equals(typeof(IList<>).Name))
                {
                    var pType = type.TypeArguments.First();

                    itemModel = GetProxyModel(item, model);

                    joined = GetJoinModels(pType, itemModel);

                    sMembers.Add($"// Join {itemModel} to [{string.Join(",", joined)}]");

                    var amembers = FilterSymbols(pType.GetAllMembers(), joined);

                    ReadCollection(sMembers, item, amembers, model, itemModel, path);

                    continue;
                }


                itemModel = GetProxyModel(item, model);

                joined = GetJoinModels(memberType, itemModel);

                var memMembers = FilterSymbols(memberType.GetAllMembers(), joined);

                if (memMembers.Any())
                {
                    sMembers.Add($"// Join {itemModel} to [{string.Join(",", joined)}]");

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
        private readonly string SelectGenerateModelJoinAttributeFullName = typeof(SelectGenerateModelJoinAttribute).Name;
    }
}