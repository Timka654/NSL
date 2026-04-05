using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryGenerator.Utils;
using NSL.Generators.BinaryTypeIOGenerator.Shared;
using NSL.Generators.BinaryTypeIOGenerator.Core.Models;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator.Core
{
    [Generator]
    internal class NSLBIOTypeGenerator : IIncrementalGenerator
    {

        #region ISourceGenerator

        //public void Execute(GeneratorExecutionContext context)
        //{
        //    //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.BinaryGenerator", StringComparison.OrdinalIgnoreCase)))
        //    //{
        //    //    context.ReportDiagnostic(Diagnostic.Create(
        //    //    new DiagnosticDescriptor(
        //    //        "SG0001",
        //    //        "Not found reference",
        //    //        $"Cannot find reference \"NSL.Generators.BinaryGenerator\"",
        //    //        "none",
        //    //        DiagnosticSeverity.Error,
        //    //        true,
        //    //        $"Cannot find reference \"NSL.Generators.BinaryGenerator\""), Location.None));
        //    //}
        //    //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.Utils", StringComparison.OrdinalIgnoreCase)))
        //    //{
        //    //    context.ReportDiagnostic(Diagnostic.Create(
        //    //    new DiagnosticDescriptor(
        //    //        "SG0001",
        //    //        "Not found reference",
        //    //        $"Cannot find reference \"NSL.Generators.Utils\"",
        //    //        "none",
        //    //        DiagnosticSeverity.Error,
        //    //        true, 
        //    //        $"Cannot find reference \"NSL.Generators.Utils\""), Location.None));
        //    //}
        //    //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.BinaryTypeIOGenerator.Attributes", StringComparison.OrdinalIgnoreCase)))
        //    //{
        //    //    context.ReportDiagnostic(Diagnostic.Create(
        //    //    new DiagnosticDescriptor(
        //    //        "SG0001",
        //    //        "Not found reference",
        //    //        $"Cannot find reference \"NSL.Generators.BinaryTypeIOGenerator.Attributes\"",
        //    //        "none",
        //    //        DiagnosticSeverity.Error,
        //    //        true,
        //    //        $"Cannot find reference \"NSL.Generators.BinaryTypeIOGenerator.Attributes\""), Location.None));
        //    //}

        //    if (context.SyntaxReceiver is NSLBIOAttributeSyntaxReceiver methodSyntaxReceiver)
        //    {
        //        ProcessNSLBIOTypes(context, methodSyntaxReceiver);
        //    }
        //}

        //public void Initialize(GeneratorInitializationContext context)
        //{
        //    context.RegisterForSyntaxNotifications(() =>
        //        new NSLBIOAttributeSyntaxReceiver());
        //}

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var pip = context.SyntaxProvider.CreateSyntaxProvider(
                NSLBIOAttributeSyntaxReceiver.OnVisitSyntaxNode,
                (syntax, _) => syntax);

            context.RegisterSourceOutput(pip, ProcessNSLBIOTypes);
        }

        #endregion
        private void ProcessNSLBIOTypes(SourceProductionContext context, GeneratorSyntaxContext item)
        {
#if DEBUG
            //GenDebug.Break();
#endif
            //var stopwatch = Stopwatch.StartNew();

            //foreach (var item in types)
            //{
                var @class = (ClassDeclarationSyntax)item.Node;

                try
                {
                    ProcessNSLBIOType(context, item, @class);
                }
                catch (Exception ex)
                {
                    context.ShowBIODiagnostics($"NSLBIO002", $"Error - {ex} on type {@class.Identifier.Text}", DiagnosticSeverity.Error, @class.GetLocation());
                }
            //}

            //stopwatch.Stop();

            //context.ReportDiagnostic(Diagnostic.Create(
            //    new DiagnosticDescriptor(
            //        id: "NSLBIO666",
            //        title: "Generator Performance",
            //        messageFormat: $"[{nameof(NSLBIOTypeGenerator)}] executed in {stopwatch.ElapsedMilliseconds} ms.",
            //        category: "Performance",
            //        DiagnosticSeverity.Info,
            //        isEnabledByDefault: true),
            //    Location.None));
        }

        private void ProcessNSLBIOType(SourceProductionContext sourceContext, GeneratorSyntaxContext context, TypeDeclarationSyntax type)
        {
            if (!type.HasPartialModifier())
            {
                sourceContext.ShowBIODiagnostics("NSLBIO000", "Type must have a partial modifier",DiagnosticSeverity.Error,  type.GetLocation());
                return;
            }

            var typeClass = type as ClassDeclarationSyntax;

            var typeSem = context.SemanticModel;

            var typeSymb = typeSem.GetDeclaredSymbol(type) as ITypeSymbol;

            var codeBuilder = new CodeBuilder();

            codeBuilder.AppendComment(() =>
            {
                codeBuilder.AppendLine($"Auto Generated by NSL NSLBIO. Please don't change this file");
                codeBuilder.AppendLine($"Project must have reference \"NSL.SocketCore\" library for normal working");
            }); 
            
            List<string> requiredUsings = new List<string> {
                "NSL.SocketCore",
                "NSL.SocketCore.Utils.Buffer",
                "System.Linq",
                "System"
            };

            codeBuilder.CreatePartialClass((CSharpCompilation)context.SemanticModel.Compilation, typeClass, classBuilder =>
            {
                var typeAttributes = typeClass.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.GetAttributeFullName().Equals(NSLBIOTypeAttributeFullName)).ToArray();

                //GenDebug.Break();
                
                var typeModels = typeAttributes
                    .SelectMany(x =>
                    {
                        var models = x.ArgumentList?.Arguments.Select(n => n.GetAttributeParameterValue<string>(typeSem));

                        if (models == null)
                            return Enumerable.Repeat<string>("<!!_NSLBIOFULL_!!>", 1);
                        return models;
                    })
                    .GroupBy(x => x).Select(x => x.Key)
                    .ToArray();


                var joinAttributes = typeClass.AttributeLists
                    .SelectMany(x => x.Attributes)
                    .Where(x => x.GetAttributeFullName().Equals(NSLBIOModelJoinAttributeFullName)).ToArray();

                var joins = joinAttributes.Select(x =>
                {
                    var args = x.ArgumentList.Arguments;

                    return (args[0].GetAttributeParameterValue<string>(typeSem), args.Skip(1).Select(n => n.GetAttributeParameterValue<string>(typeSem)).ToArray());
                }).ToDictionary(x => x.Item1, x => x.Item2);


                foreach (var _model in typeModels)
                {
                    var model = _model;

                    var actualModels = Enumerable.Repeat(model, 1).ToArray();

                    Func<string, bool> modelSelector = mname => mname == null;

                    var name = model == null ? "All" : model;

                    if (string.Equals("<!!_NSLBIOFULL_!!>", model))
                    {
                        name = "Full";
                        model = null;
                        modelSelector = mname => true;
                    }
                    else if (model != null)
                    {
                        modelSelector = mname => actualModels.Contains(mname);

                        if (joins.TryGetValue(model, out var jns))
                            actualModels = actualModels
                                                .Concat(jns)
                                                .GroupBy(x => x)
                                                .Select(x => x.Key)
                                                .ToArray();
                    }

                    var methodInfo = new MethodInfoModel()
                    {
                        IOType = IOTypeEnum.Write,
                        ForGroup = model,
                        ModelSelector = modelSelector,
                        Parameters = new List<parametermodel>() {
                            new parametermodel(){  name = "packet", typeName = typeof(OutputPacketBuffer).Name }
                        },
                        ClassDeclarationSyntax = typeClass,
                        MethodModifier = "public",
                        ReadType = typeClass.GetClassName()
                    };

                    methodInfo.MethodName = $"Write{name}To";

                    CodeBuilder methodBuilder = new CodeBuilder();
                    ProcessWriteMethod(methodBuilder, methodInfo, typeSem, sourceContext);
                    classBuilder.AppendLine(methodBuilder.ToString());

                    //continue;

                    methodInfo.MethodModifier = "public static";
                    methodInfo.Parameters = new List<parametermodel>()
                    {
                        new parametermodel(){ name = "value", typeName = methodInfo.ReadType },
                        methodInfo.Parameters[0]
                    };

                    methodBuilder = new CodeBuilder();
                    ProcessWriteMethod(methodBuilder, methodInfo, typeSem, sourceContext);
                    classBuilder.AppendLine(methodBuilder.ToString());

                    methodInfo.IOType = IOTypeEnum.Read;
                    methodInfo.MethodName = $"Read{name}From";
                    methodInfo.Parameters = new List<parametermodel>()
                    {
                        new parametermodel(){  name = "data", typeName = typeof(InputPacketBuffer).Name }
                    };

                    methodBuilder = new CodeBuilder();
                    ProcessReadMethod(methodBuilder, methodInfo, typeSem, sourceContext);
                    classBuilder.AppendLine(methodBuilder.ToString());
                }

            }, requiredUsings, beforeClassDef: builder => builder.AppendSummary(b =>
            {
                b.AppendSummaryLine($"Generate for <see cref=\"{typeSymb.GetTypeSeeCRef()}\"/>");
            }));

            // Visual studio have lag(or ...) cannot show changes any time
            //#if DEVELOP
            //#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //            System.IO.File.WriteAllText($@"C:\Work\temp\{classIdentityName}.binaryio.cs", outputValue);
            //#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

#if DEBUG
            //GenDebug.Break();
#endif
            sourceContext.AddSource($"{typeClass.GetTypeClassName()}.binaryio.cs", codeBuilder.ToString());

            // DTO generation: for each NSLBIOType attribute with Dto=true, generate a standalone DTO class
            var dtoAttrs = typeSymb.GetAttributes()
                .Where(x => x.AttributeClass.Name == NSLBIOTypeAttributeFullName)
                .Where(x =>
                {
                    var dtoArg = x.NamedArguments.FirstOrDefault(n => n.Key == "Dto");
                    return dtoArg.Value.Value is bool b && b;
                });

            foreach (var dtoAttr in dtoAttrs)
            {
                var dtoSuffix = dtoAttr.NamedArguments.FirstOrDefault(n => n.Key == "DtoSuffix").Value.Value as string ?? "BIO";

                var attrModels = dtoAttr.ConstructorArguments
                    .SelectMany(a => a.Values)
                    .Select(v => (string)v.Value)
                    .ToArray();

                if (attrModels.Length == 0)
                {
                    // Full model (no arguments)
                    GenerateBIODto(sourceContext, type, typeSymb, null, "Full", isFull: true, dtoSuffix, typeSem);
                }
                else
                {
                    foreach (var model in attrModels)
                        GenerateBIODto(sourceContext, type, typeSymb, model, model, isFull: false, dtoSuffix, typeSem);
                }
            }
        }


        private void ProcessReadMethod(CodeBuilder methodBuilder, MethodInfoModel methodInfo, SemanticModel tSym, SourceProductionContext context)
        {
            var classDecl = methodInfo.ClassDeclarationSyntax;

            methodBuilder.AppendLine($"{methodInfo.MethodModifier} {methodInfo.ReadType} {methodInfo.MethodName}({string.Join(", ", methodInfo.Parameters.Select(x => $"{x.typeName} {x.name}"))})");
            methodBuilder.AppendLine("{");
            methodBuilder.NextTab();

            var bufferParam = methodInfo.Parameters.FirstOrDefault(x => x.typeName.Equals(typeof(InputPacketBuffer).Name));

            if (bufferParam == null)
            {
                methodBuilder.AppendLine($"throw new NotImplementedException(\"Required parameter with type \\\"{typeof(InputPacketBuffer).Name}\\\" not found\");");
                return;
            }

            var bgContext = new NSLBIOGeneratorContext()
            {
                For = methodInfo.ForGroup,
                ModelSelector = methodInfo.ModelSelector,
                SemanticModel = tSym,
                Context = context,
                IOPath = bufferParam.name,
                ProcessingType = classDecl.GetClassName(),
                ReadCurrentTypeMethodName = methodInfo.MethodName,
                IsStatic = methodInfo.MethodModifier.Contains("static")
            };

            methodBuilder.AppendLine($"return {BinaryReadMethodsGenerator.GetValueReadSegment(tSym.GetDeclaredSymbol(classDecl), bgContext, default)};");

            methodBuilder.PrevTab();
            methodBuilder.AppendLine("}");
        }
        private void GenerateBIODto(SourceProductionContext sourceContext, TypeDeclarationSyntax typeDecl,
            ITypeSymbol typeSymb, string model, string modelName, bool isFull, string dtoSuffix, SemanticModel typeSem)
        {
            var members = typeSymb.GetAllMembers()
                .Where(m => !m.IsStatic && (m is IPropertySymbol || m is IFieldSymbol))
                .Where(m =>
                {
                    if (m is IPropertySymbol ps && ps.GetMethod == null) return false;
                    return BIOIsMemberIncluded(m, model, isFull);
                })
                .ToArray();

            if (!members.Any())
                return;

            var className = BuildBIODtoName(typeSymb.Name, modelName, dtoSuffix);
            var @namespace = typeDecl.TryGetNamespace();
            var usings = (typeDecl.GetTypeClassUsingDirectives() ?? new string[0]).ToList();

            if (!usings.Contains("System.Collections.Generic"))
                usings.Add("System.Collections.Generic");

            var codeBuilder = new CodeBuilder();

            codeBuilder.AppendComment(() =>
            {
                codeBuilder.AppendLine("Auto Generated by NSL NSLBIO DTO. Please don't change this file");
            });

            codeBuilder.CreateClass(classBuilder =>
            {
                foreach (var member in members)
                {
                    var rawType = member.GetTypeSymbol();
                    if (rawType == null) continue;

                    string genericDef = "{0}";
                    ITypeSymbol elementType = rawType;

                    if (rawType is IArrayTypeSymbol arrType)
                    {
                        elementType = arrType.ElementType;
                        genericDef = "{0}[]";
                    }
                    else if (rawType is INamedTypeSymbol nt &&
                             (nt.MetadataName == "List`1" || nt.MetadataName == "IList`1"))
                    {
                        elementType = nt.TypeArguments.First();
                        genericDef = "List<{0}>";
                    }

                    var childModel = GetBIOProxyModel(member, model, isFull);

                    var nestedDtoName = GetBIONestedDtoTypeFullName(elementType, childModel, isFull);

                    string typeStr = nestedDtoName != null
                        ? string.Format(genericDef, nestedDtoName)
                        : rawType.GetTypeFullName();

                    classBuilder.AppendLine($"public {typeStr} {member.Name} {{ get; set; }}");
                }
            }, className, @namespace, usings, beforeClassDef: builder => builder.AppendSummary(b =>
            {
                b.AppendSummaryLine($"DTO generated for <see cref=\"{typeSymb.GetTypeSeeCRef()}\"/> model \"{modelName}\"");
            }));

#pragma warning disable RS1035
            sourceContext.AddSource($"{className}.biodto.cs", codeBuilder.ToString());
#pragma warning restore RS1035
        }

        private static bool BIOIsMemberIncluded(ISymbol member, string model, bool isFull)
        {
            if (isFull) return true;

            foreach (var attr in member.GetAttributes().Where(x => x.AttributeClass.Name == "NSLBIOIncludeAttribute"))
            {
                var models = attr.ConstructorArguments
                    .SelectMany(a => a.Values)
                    .Select(v => (string)v.Value)
                    .ToArray();

                if (models.Length == 0 || models.Contains(model))
                    return true;
            }

            return false;
        }

        private static string GetBIOProxyModel(ISymbol member, string currentModel, bool isFull)
        {
            if (isFull) return null;

            foreach (var attr in member.GetAttributes().Where(x => x.AttributeClass.Name == "NSLBIOProxyAttribute"))
            {
                var args = attr.ConstructorArguments;
                if (args.Length == 1)
                    return (string)args[0].Value;

                var fromModels = args[1].Values.Select(v => (string)v.Value).ToArray();
                if (fromModels.Contains(currentModel))
                    return (string)args[0].Value;
            }

            return currentModel;
        }

        private static string GetBIONestedDtoTypeFullName(ITypeSymbol elementType, string childModel, bool parentIsFull)
        {
            foreach (var attr in elementType.GetAttributes())
            {
                if (attr.AttributeClass.Name != "NSLBIOTypeAttribute") continue;

                var dtoArg = attr.NamedArguments.FirstOrDefault(n => n.Key == "Dto");
                if (!(dtoArg.Value.Value is bool isDtoEnabled) || !isDtoEnabled) continue;

                var attrModels = attr.ConstructorArguments
                    .SelectMany(a => a.Values)
                    .Select(v => (string)v.Value)
                    .ToArray();

                bool modelMatches = attrModels.Length == 0
                    ? childModel == null || parentIsFull
                    : childModel != null && attrModels.Contains(childModel);

                if (!modelMatches) continue;

                var suffix = attr.NamedArguments.FirstOrDefault(n => n.Key == "DtoSuffix").Value.Value as string ?? "BIO";
                var modelName = attrModels.Length == 0 ? "Full" : childModel;
                return BuildBIODtoFullName(elementType.GetTypeFullName(false), modelName, suffix);
            }

            return null;
        }

        private static string BuildBIODtoName(string typeName, string modelName, string suffix)
        {
            if (typeName.EndsWith("Model"))
                typeName = typeName.Substring(0, typeName.Length - "Model".Length);
            return $"{typeName}Dto{modelName}{suffix}Model";
        }

        private static string BuildBIODtoFullName(string typeFullName, string modelName, string suffix)
        {
            if (typeFullName.EndsWith("Model"))
                typeFullName = typeFullName.Substring(0, typeFullName.Length - "Model".Length);
            return $"{typeFullName}Dto{modelName}{suffix}Model";
        }
        private void ProcessWriteMethod(CodeBuilder methodBuilder, MethodInfoModel methodInfo, SemanticModel tSym, SourceProductionContext context)
        {
            //var method = methodInfo.MethodDeclarationSyntax;
            var classDecl = methodInfo.ClassDeclarationSyntax;

            methodBuilder.AppendLine($"{methodInfo.MethodModifier} void {methodInfo.MethodName}({string.Join(", ", methodInfo.Parameters.Select(x => $"{x.typeName} {x.name}"))})");
            methodBuilder.AppendLine("{");
            methodBuilder.NextTab();

            var typeParam = methodInfo.Parameters.FirstOrDefault(x => x.typeName.Equals(methodInfo.ReadType));

            var needObject = methodInfo.MethodModifier.Contains("static");

            var existsObject = typeParam != null;

            if (!existsObject && needObject)
            {
                methodBuilder.AppendLine($"throw new NotImplementedException(\"Required parameter with type \\\"{methodInfo.ReadType}\\\" not found\");");
                return;
            }


            var bufferParam = methodInfo.Parameters.FirstOrDefault(x => x.typeName.Equals(typeof(OutputPacketBuffer).Name));

            if (bufferParam == null)
            {
                methodBuilder.AppendLine($"throw new NotImplementedException(\"Required parameter with type \\\"{typeof(OutputPacketBuffer).Name}\\\" not found\");");
                return;
            }

            var typeParamName = existsObject ? typeParam.name : "this";

            //if (bufferParam.name != "__packet")
            //{
            //    methodBuilder.AppendLine($"var __packet = {bufferParam.name};");
            //    methodBuilder.AppendLine();
            //}

            var bgContext = new NSLBIOGeneratorContext()
            {
                For = methodInfo.ForGroup,
                ModelSelector = methodInfo.ModelSelector,
                SemanticModel = tSym,
                Context = context,
                IOPath = bufferParam.name,
                ProcessingType = classDecl.GetClassName(),
                WriteCurrentTypeMethodName = methodInfo.MethodName,
                IsStatic = methodInfo.MethodModifier.Contains("static")
            };

            methodBuilder.AppendLine(BinaryWriteMethodsGenerator.BuildParameterWriter(tSym.GetDeclaredSymbol(classDecl), bgContext, typeParamName));


            methodBuilder.PrevTab();
            methodBuilder.AppendLine("}");
        }

        private SyntaxList<UsingDirectiveSyntax> UpdateUsingDirectives(SyntaxTree originalTree)
        {
            var rootNode = originalTree.GetRoot() as CompilationUnitSyntax;
            return rootNode.Usings;
        }

        internal static readonly string NSLBIOTypeAttributeFullName = typeof(NSLBIOTypeAttribute).Name;
        internal static readonly string NSLBIOModelJoinAttributeFullName = typeof(NSLBIOModelJoinAttribute).Name;
    }
}
