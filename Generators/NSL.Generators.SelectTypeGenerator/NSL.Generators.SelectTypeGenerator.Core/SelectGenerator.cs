using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.SelectGenerator.Utils;
using NSL.Generators.SelectTypeGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NSL.Generators.SelectTypeGenerator
{

    [Generator]
    internal class SelectGenerator : IIncrementalGenerator
    {
        #region ISourceGenerator

        //public void Execute(GeneratorExecutionContext context)
        //{
        //    if (context.SyntaxReceiver is SelectAttributeSyntaxReceiver methodSyntaxReceiver)
        //    {
        //        ProcessFillTypes(context, methodSyntaxReceiver);
        //    }
        //}


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pip = context.SyntaxProvider.CreateSyntaxProvider(
                SelectAttributeSyntaxReceiver.OnVisitSyntaxNode,
                (syntax, _) => syntax);

            context.RegisterSourceOutput(pip, ProcessSelectTypes);
        }

        //public void Initialize(GeneratorInitializationContext context)
        //{
        //    context.RegisterForSyntaxNotifications(() =>
        //        new SelectAttributeSyntaxReceiver());
        //}

        #endregion

        private void ProcessSelectTypes(SourceProductionContext context, GeneratorSyntaxContext node)
        {
            //var stopwatch = Stopwatch.StartNew();

            List<string> fnames = new List<string>();
            //GenDebug.Break(true);
            //GenDebug.Break();
            //foreach (var group in types.GroupBy(x =>
            //{
            //    var type = x.Node as TypeDeclarationSyntax;

            //    return $"{type.TryGetNamespace()}{type.GetClassName()}";
            //}))
            //{
            //var node = group.First();

            //var key = group.Key;
            var type = node.Node as TypeDeclarationSyntax;
            var key = $"{type.TryGetNamespace()}{type.GetClassName()}";
            var semanticModel = node.SemanticModel;
                var typeDecl = node.Node as TypeDeclarationSyntax;
                var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl) as ITypeSymbol;


                List<GenAttribute> attributes = new List<GenAttribute>();

                var typeAttributes = typeSymbol.GetAttributes();


                foreach (var attr in typeAttributes.Where(x => x.AttributeClass.GetTypeFullName(false) == SelectGenerateAttributeFullName))
                {
                    var models = attr.ConstructorArguments.First().Values
                        .Select(x => (string)x.Value)
                        .ToArray();

                    var dto = (bool?)((TypedConstant?)attr.GetNamedArgumentValue(nameof(SelectGenerateAttribute.Dto)))?.Value ?? false;

                    var typed = (bool?)((TypedConstant?)attr.GetNamedArgumentValue(nameof(SelectGenerateAttribute.Typed)))?.Value ?? false;

                    var a = new GenAttribute()
                    {
                        Models = models,
                        Dto = dto,
                        Typed = typed,
                    };

                    attributes.Add(a);
                }

                var gtype = new GenTypeGroup()
                {
                    TypeSymbol = typeSymbol,
                    TypeDeclaration = typeDecl,
                    Namespace = typeDecl.TryGetNamespace(),
                    Name = key.Replace('.', '_'),
                    Members = typeSymbol.GetAllMembers(),
                    Attributes = attributes.GroupBy(x => (x.Typed, x.Dto))
                    .Select(x => new GenAttribute()
                    {
                        Dto = x.Key.Dto,
                        Typed = x.Key.Typed,
                        Models = x.SelectMany(i => i.Models).GroupBy(g => g).Select(k => k.Key).ToArray()
                    })
                    .ToArray()
                };

                try
                {
                    ProcessSelectToType(context, gtype, fnames);
                }
                catch (Exception ex)
                {
                    context.ShowSelectDiagnostics($"NSLSELECT002", $"Error - {ex} on type {gtype.TypeDeclaration.Identifier.Text}", DiagnosticSeverity.Error, node.Node.GetLocation());
                }
            //}

            //stopwatch.Stop();

            //context.ReportDiagnostic(Diagnostic.Create(
            //    new DiagnosticDescriptor(
            //        id: "NSLSELECT666",
            //        title: "Generator Performance",
            //        messageFormat: $"[{nameof(SelectGenerator)}] executed in {stopwatch.ElapsedMilliseconds} ms.",
            //        category: "Performance",
            //        DiagnosticSeverity.Info,
            //        isEnabledByDefault: true),
            //    Location.None));
        }

        private void ProcessSelectToType(SourceProductionContext sourceContext, GenTypeGroup gtype,
            List<string> fnames)
        {
            var classBuilder = new CodeBuilder();

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL Select. Please don't change this file");
            });

            List<string> namespaces = new List<string>();

            if (gtype.Namespace != default)
            {
                namespaces.Add(gtype.Namespace);
            }

            namespaces.Add("System.Collections.Generic");

            List<SelectGenContext> genContexts = new List<SelectGenContext>();

            classBuilder.CreateStaticClass(gtype.TypeDeclaration, $"{gtype.Name}_Selection", () =>
            {
                var methods = new List<string>();

                foreach (var typedModels in gtype.Attributes)
                {
                    foreach (var item in typedModels.Models)
                    {
                        var mjoins = GetJoinModels(gtype.TypeSymbol, item).Prepend(item);

                        SelectGenContext genContext = typedModels.Dto ? new SelectGenDTOContext() : new SelectGenContext();

                        genContext.Type = gtype.TypeSymbol;
                        genContext.Symbols = FilterSymbols(gtype.Members, mjoins, typedModels.Typed);
                        genContext.Typed = typedModels.Typed;
                        genContext.Model = item;

                        CreateMethods(methods, genContext);

                        genContexts.Add(genContext);
                    }
                }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                classBuilder.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, methods));
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

            }, namespaces, @namespace: "System.Linq", beforeClassDef: builder => builder.AppendSummary(b =>
             {
                 b.AppendSummaryLine($"Generate for <see cref=\"{gtype.TypeSymbol.GetTypeSeeCRef()}\"/>");

             }));


            //GenDebug.Break();

            // Visual studio have lag(or ...) cannot show changes sometime
            //#if DEVELOP
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //System.IO.File.WriteAllText($@"C:\Work\temp\{typeClass.GetTypeClassName()}.selectgen.cs", classBuilder.ToString());
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif


            var fname = $"{gtype.Name}.selectgen.cs";

            fnames.Add(fname);

            GenerateDtos(sourceContext, genContexts, fnames);

            var code = classBuilder.ToString();


            sourceContext.AddSource(fname, code);
        }

        void GenerateDtos(SourceProductionContext sourceContext, IEnumerable<SelectGenContext> items, List<string> fnames)
        {
            foreach (var item in items)
            {
                if (item.SubTypeList != null)
                    GenerateDtos(sourceContext, item.SubTypeList, fnames);

                if (item is SelectGenDTOContext dto && item.Symbols.Any())
                {
                    CodeBuilder codeBuilder = new CodeBuilder();
                    //GenDebug.Break(true);

                    var fileClassDecls = dto.Type.DeclaringSyntaxReferences
                        .Select(x => x.SyntaxTree.GetRoot() as CompilationUnitSyntax)
                        .SelectMany(x => x.DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>())
                        .ToArray();

                    var classDecls = fileClassDecls.Where(c => c.GetFullName() == dto.Type.GetTypeFullName(false))
                        .ToArray();


                    var classDecl = classDecls.FirstOrDefault();

                    if (classDecl != null)
                    {
                        var className = dto.GetTypeName();

                        var fname = $"{className}.dtogen.cs";

                        if (fnames.Contains(fname))
                        {
                            continue;
                        }

                        fnames.Add(fname);

                        var @namespace = classDecl.GetNamespace();

                        var usings = classDecls.SelectMany(x => x.GetTypeClassUsingDirectives()).ToArray();



                        codeBuilder.CreateClass(classBuilder =>
                        {
                            foreach (var member in dto.Symbols)
                            {
                                string typeDef = "";
                                var typeSymb = member.GetTypeSymbol();
                                var type = dto.SubTypeList?.FirstOrDefault(x => x.OriginType == typeSymb);

                                if (type != null)
                                {
                                    typeDef = string.Format(type.GenericDefinition, type.GetTypeIdentifier());
                                    //GenDebug.Break(true);
                                }
                                else
                                    typeDef = typeSymb.GetTypeFullName();

                                classBuilder.AppendLine($"public {typeDef} {member.GetName(default)} {{ get; set; }}");
                            }
                        }, className, @namespace, usings, beforeClassDef: builder => builder.AppendSummary(b =>
                        {
                            b.AppendSummaryLine($"Generate for <see cref=\"{dto.Type.GetTypeSeeCRef()}\"/>");
                        }));

                        //GenDebug.Break(true);

                        var code = codeBuilder.ToString();


                        sourceContext.AddSource(fname, code);
                    }
                }
            }
        }

        private IEnumerable<ISymbol> FilterSymbols(IEnumerable<ISymbol> symbols, IEnumerable<string> models, bool typed)
            => symbols.Where(symbol =>
         {
             if (typed && symbol is IPropertySymbol ps && (ps.SetMethod == null || ps.GetMethod == null))
                 return false;

             var attributes = symbol
             .GetAttributes()
             .Where(n => n.AttributeClass.Name == SelectGenerateIncludeAttributeFullName)
             .SelectMany(b => b.ConstructorArguments)
             .SelectMany(b => b.Values)
             .ToArray();

             if (attributes.Any(attr => models.Any(model => string.Equals(attr.Value as string, model))))
                 return true;

             return false;
         });

        private void CreateMethods(List<string> methods, SelectGenContext genContext)
        {

            var sMembers = new List<string>();

            ReadMembers(sMembers, "___x", genContext);

            methods.Add(CreateSelectMethod(sMembers, "IEnumerable", genContext).ToString());
            methods.Add(CreateSelectMethod(sMembers, "IQueryable", genContext).ToString());
            methods.Add(CreateCastMethod(sMembers, genContext).ToString());

        }

        private CodeBuilder CreateCastMethod(IEnumerable<string> sMembers, SelectGenContext genContext)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            //methodBuilder.AppendLine($"{(declaration.HasInternalModifier() ? "internal" : "public")} {toType.Name} Select{model}Create()");

            //methodBuilder.AppendLine("{");

            //methodBuilder.NextTab();



            //methodBuilder.PrevTab();

            //methodBuilder.AppendLine("}");

            //methodBuilder.AppendLine();


            var rType = genContext.Typed ? genContext.GetTypeIdentifier() : "dynamic";

            methodBuilder.AppendLine($"public static {rType} To{(genContext.Typed ? "Typed" : string.Empty)}{genContext.Model}(this {genContext.Type.GetTypeFullName()} ___x)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            if (genContext.Typed)
                methodBuilder.AppendLine($"return new {rType} {{");
            else
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

        private CodeBuilder CreateSelectMethod(IEnumerable<string> sMembers, string collectionType, SelectGenContext genContext)
        {
            CodeBuilder methodBuilder = new CodeBuilder();

            var rType = genContext.Typed ? genContext.GetTypeIdentifier() : "dynamic";

            methodBuilder.AppendLine($"public static {collectionType}<{rType}> Select{(genContext.Typed ? "Typed" : string.Empty)}{genContext.Model}(this {collectionType}<{genContext.Type.GetTypeFullName()}> ___toSelect)");

            methodBuilder.AppendLine("{");

            methodBuilder.NextTab();

            if (genContext.Typed)
                methodBuilder.AppendLine($"return ___toSelect.Select(___x=> new {genContext.GetTypeIdentifier()} {{");
            else
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
                     .Where(x => x.AttributeClass.GetTypeFullName(false) == SelectGenerateModelJoinAttributeFullName)
                     .ToDictionary(x => (x.ConstructorArguments.First().Value as string), x => new JoinAttributeData
                     {
                         Models = x.ConstructorArguments.ElementAt(1).Values.Select(q => q.Value as string).ToArray(),
                         Recursive = x.NamedArguments
                         .Where(n => n.Key == nameof(SelectGenerateModelJoinAttribute.Recursive))
                         .Select(n => (bool)n.Value.Value)
                         .DefaultIfEmpty(true)
                         .FirstOrDefault()
                     });

            var result = GetJoinModels(joined, model)
             .GroupBy(x => x)
             .Select(x => x.Key)
             .ToArray();

            return result;
        }

        private IEnumerable<string> GetJoinModels(Dictionary<string, JoinAttributeData> map, string model)
        {
            if (!map.TryGetValue(model, out var joined)) return Enumerable.Empty<string>();

            IEnumerable<string> result = joined.Models;

            if (joined.Recursive)
                foreach (var item in joined.Models)
                {
                    result = result.Concat(GetJoinModels(map, item));
                }

            return result;
        }

        private string GetProxyModel(ISymbol item, string model)
        {
            //GenDebug.Break();

            string proxyModel = default;

            var attributes = item.GetAttributes();

            var proxyAttribs = attributes.Where(x => x.AttributeClass.Name == SelectGenerateProxyAttributeFullName).ToArray();

            var fromModel = proxyAttribs.FirstOrDefault(x => x.ConstructorArguments.Length == 2
            && Regex.IsMatch(model, ((string)x.ConstructorArguments.First().Value)));

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

        //private void ReadCollection(List<string> sMembers, ISymbol item, IEnumerable<ISymbol> amembers, string model, string itemModel, string path, bool typed, ITypeSymbol itemType, string toSegment)        
        private void ReadCollection(List<string> sMembers, string path, SelectGenContext context, string toSegment)
        {
            if (context.Symbols.Any())
            {

#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов
                //GenDebug.Break(true);
                string readSegment = default;
                if (HaveModel(context.Model, context))
                {
                    readSegment = $"{path}.{context.MemberName}.Select{(context.Typed ? "Typed" : string.Empty)}{context.Model}()";
                }
                else
                {
                    string p = GetTempPathNode(path);

                    var rType = context.Typed ? context.GetTypeIdentifier() : string.Empty;

                    var amem = new List<string>();

                    ReadMembers(amem, p, context);

                    readSegment = $"{path}.{context.MemberName}.Select({p} => new {rType} {{{Environment.NewLine}{CombineMembers(amem.Select(x => $"\t{x}"))}{Environment.NewLine}}})";
                }

                sMembers.Add($"{context.MemberName} = {path}.{context.MemberName} == null ? null : {readSegment}{toSegment}");
#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов
            }
            else
                sMembers.Add($"{context.MemberName} = {path}.{context.MemberName}");
        }

        private string GetTempPathNode(string path)
        {
            int n = 0;
            string p = string.Empty;
            while (path.Contains(p = $"x{n++}")) { }

            return p;
        }


        private void ReadMembers(List<string> sMembers, string path, SelectGenContext genContext)
        {
            foreach (var item in genContext.Symbols)
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

                string collectionCastFragment = default;
                string genericDefinition = "{0}";

                ITypeSymbol originType = memberType;

                if (memberType is IArrayTypeSymbol arrt)
                {
                    memberType = arrt.ElementType;
                    collectionCastFragment = ".ToArray()";
                    genericDefinition = "{0}[]";
                }
                else if ((memberType is INamedTypeSymbol namedType) && (memberType.MetadataName.Equals(typeof(List<>).Name) || memberType.MetadataName.Equals(typeof(IList<>).Name)))
                {
                    memberType = namedType.TypeArguments.First();
                    collectionCastFragment = ".ToList()";
                    genericDefinition = "List<{0}>";
                }

                SelectGenContext itemGenContext = genContext is SelectGenDTOContext ? new SelectGenDTOContext() : new SelectGenContext();

                itemGenContext.Model = GetProxyModel(item, genContext.Model);

                IEnumerable<string> joinedModels = GetJoinModels(memberType, itemGenContext.Model).Prepend(itemGenContext.Model);

                itemGenContext.Parent = genContext;
                itemGenContext.Symbols = FilterSymbols(memberType.GetAllMembers(), joinedModels, genContext.Typed);
                itemGenContext.Type = memberType;
                itemGenContext.OriginType = originType;
                itemGenContext.Typed = genContext.Typed;
                itemGenContext.MemberName = item.Name;
                itemGenContext.GenericDefinition = genericDefinition;

                AddChildContext(genContext, itemGenContext);

                if (collectionCastFragment != default)
                {
                    sMembers.Add($"// Join {itemGenContext.Model} to [{string.Join(",", joinedModels)}]");

                    if (!Equals(genContext.Model, itemGenContext.Model))
                        sMembers.Add($"// Proxy model merge from \"{genContext.Model}\" to \"{itemGenContext.Model}\"");

                    ReadCollection(sMembers, path, itemGenContext, collectionCastFragment);

                    continue;
                }

                if (itemGenContext.Symbols.Any())
                {
                    sMembers.Add($"// Join {itemGenContext.Model} to [{string.Join(",", joinedModels)}]");

                    if (!Equals(genContext.Model, itemGenContext.Model))
                        sMembers.Add($"// Proxy Model merge from \"{genContext.Model}\" to \"{itemGenContext.Model}\"");


#pragma warning disable RS1035 // Не использовать API, запрещенные для анализаторов

                    string readSegment = default;

                    if (HaveModel(itemGenContext.Model, itemGenContext))
                    {
                        readSegment = $"{path}.{item.Name}.To{(genContext.Typed ? "Typed" : string.Empty)}{itemGenContext.Model}()";
                    }
                    else
                    {
                        var nMembers = new List<string>();

                        ReadMembers(nMembers, $"{path}.{item.Name}", itemGenContext);

                        var valueGetter = genContext.Typed ? $"new {itemGenContext.GetTypeIdentifier(false)}" : $"new ";

                        readSegment = $"{valueGetter} {{{Environment.NewLine}{CombineMembers(nMembers.Select(x => $"\t{x}"))}{Environment.NewLine}}}";
                    }


                    sMembers.Add($"{item.Name} = {path}.{item.Name} == null ? null : {readSegment}");

#pragma warning restore RS1035 // Не использовать API, запрещенные для анализаторов

                    continue;
                }

                if (genContext.Typed)
                {
                    sMembers.Add($"{item.Name} = {path}.{item.Name}");

                    continue;
                }

                sMembers.Add($"{path}.{item.Name}");
            }
        }

        private bool HaveModel(string model, SelectGenContext context)
        {
            return context.Type.GetAttributes().Where(x =>
            {
                if (!x.AttributeClass.Name.Equals(SelectGenerateAttributeFullName))
                    return false;

                if (x.ConstructorArguments.Where(a => a.Values.Any(v => v.Value == model)).Any() && Equals(x.GetNamedArgumentValue("Typed") ?? false, context.Typed))
                    return true;

                return false;
            }).Any();
        }

        void AddChildContext(SelectGenContext genContext, SelectGenContext childGenContext)
        {
            var curContext = childGenContext.Parent;

            while (curContext != null)
            {
                if (curContext.Model == childGenContext.Model && SymbolEqualityComparer.Default.Equals(curContext.Type, childGenContext.Type))
                {
                    string pathBuilder = $"({childGenContext.Type.Name}){childGenContext.Model}@{childGenContext.MemberName}";

                    curContext = childGenContext.Parent;


                    while (curContext != null)
                    {
                        pathBuilder = $"({curContext.Type.Name}){curContext.Model}@{curContext.MemberName} -> {pathBuilder}";

                        curContext = curContext.Parent;
                    }

                    throw new Exception($"Found cycle on generate {pathBuilder}");
                }

                curContext = curContext.Parent;
            }

            if (!childGenContext.Symbols.Any())
                return;

            if (genContext.SubTypeList == null)
                genContext.SubTypeList = new List<SelectGenContext>();

            genContext.SubTypeList.Add(childGenContext);
        }

        private readonly string SelectGenerateAttributeFullName = typeof(SelectGenerateAttribute).FullName;
        private readonly string SelectGenerateIncludeAttributeFullName = typeof(SelectGenerateIncludeAttribute).Name;
        private readonly string SelectGenerateProxyAttributeFullName = typeof(SelectGenerateProxyAttribute).Name;
        private readonly string SelectGenerateModelJoinAttributeFullName = typeof(SelectGenerateModelJoinAttribute).FullName;
    }

    internal struct JoinAttributeData
    {
        public string[] Models { get; set; }

        public bool Recursive { get; set; }
    }
}
