﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.BinaryTypeIOGenerator.Models;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    [Generator]
    internal class BinaryIOTypeGenerator : ISourceGenerator
    {
        private void ProcessBinaryIOTypes(GeneratorExecutionContext context, BinaryIOAttributeSyntaxReceiver methodSyntaxReceiver)
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();
#endif

            foreach (var type in methodSyntaxReceiver.BinaryIOTypes)
            {
                ProcessBinaryIOType(context, type);
            }
        }

        private static string[] requiredUsings = new string[] {
            "using NSL.SocketCore;",
            "using NSL.SocketCore.Utils.Buffer;",
            "using System.Linq;"
        };

        private void ProcessBinaryIOType(GeneratorExecutionContext context, TypeDeclarationSyntax type)
        {
            var classDecl = type as ClassDeclarationSyntax;

            if (!type.Modifiers.Any(x => x.ValueText.Equals("partial")))
                return;


            var classIdentityName = type.Identifier.Text;

            var ns = type.Parent as NamespaceDeclarationSyntax;

            var methods = type.Members.Select(x => x as MethodDeclarationSyntax).Where(x => x != null).ToArray();

            CodeBuilder classBuilder = new CodeBuilder();

            classBuilder.AppendComment(() =>
            {
                classBuilder.AppendLine($"Auto Generated by NSL BinaryIO. Please dont change this file");
                classBuilder.AppendLine($"Project must have reference \"NSL.SocketCore\" library for normal working");
            });

            var usings = UpdateUsingDirectives(type.SyntaxTree);

            foreach (var u in usings)
            {
                classBuilder.AppendLine(u.ToString());
            }

            foreach (var u in requiredUsings)
            {
                if (!usings.Any(eu => eu.ToString().Equals(u)))
                    classBuilder.AppendLine(u);
            }

            classBuilder.AppendLine();

            var generic = classDecl.TypeParameterList?.Parameters.Any() == true ? $"<{string.Join(",", classDecl.TypeParameterList.Parameters.Select(x => x.Identifier.Text))}>" : string.Empty;


            classBuilder.AppendLine($"{classDecl.GetClassFullModifier()} class {classDecl.GetClassName()}{generic}");

            classBuilder.NextTab();

            foreach (var c in classDecl.ConstraintClauses)
            {
                classBuilder.AppendLine(c.ToString());
            }

            classBuilder.PrevTab();

            classBuilder.AppendLine("{");

            classBuilder.NextTab();

            var tSym = context.Compilation.GetSemanticModel(classDecl.SyntaxTree);

            foreach (var method in methods)
            {
                if (!method.Modifiers.Any(x => x.ValueText.Equals("partial")))
                    continue;

                var attrList = method.AttributeLists.SelectMany(x => x.Attributes).ToArray();

                var methodReadAttribute = attrList
                    .FirstOrDefault(x => x.GetAttributeFullName().Equals(ReadMethodAttributeFullName));

                var methodWriteAttribute = attrList
                    .FirstOrDefault(x => x.GetAttributeFullName().Equals(WriteMethodAttributeFullName));

                if (
                    (methodReadAttribute != null && methodWriteAttribute != null) ||
                    (methodReadAttribute == null && methodWriteAttribute == null)
                   )
                    continue;

                string forGroup = null;

                var args = (methodWriteAttribute ?? methodReadAttribute).ArgumentList?.Arguments;

                var argsMap = args?
                    .Select(x => (x.NameEquals?.Name.ToString(), x.GetAttributeParameterValue<string>(tSym)))
                    .Where(x => x.Item1 != null)
                    .ToDictionary(x => x.Item1, x => x.Item2);

                argsMap?.TryGetValue("For", out forGroup);

                var methodInfo = new MethodInfoModel()
                {
                    IOType = methodReadAttribute == null ? IOTypeEnum.Write : IOTypeEnum.Read,
                    ForGroup = forGroup?.Trim('\"') ?? "*",
                    Parameters = method.ParameterList.Parameters.Select(x => new parametermodel
                    {
                        name = x.Identifier.ValueText,
                        typeName = ((IdentifierNameSyntax)x.Type).Identifier.Text
                        //type = ((IdentifierNameSyntax)x.Type)
                    }).ToList(),
                    ClassDeclarationSyntax = classDecl,
                    MethodModifier = string.Join(" ", method.Modifiers.Select(x => x.Text)),
                    MethodName = method.GetMethodName(),
                    ReadType = classDecl.GetClassName()
                };

                CodeBuilder methodBuilder = new CodeBuilder();

                switch (methodInfo.IOType)
                {
                    case IOTypeEnum.Read:
                        ProcessReadMethod(methodBuilder, methodInfo, tSym);
                        break;
                    case IOTypeEnum.Write:
                        ProcessWriteMethod(methodBuilder, methodInfo, tSym);
                        break;
                    default:
                        break;
                }


                classBuilder.AppendLine(methodBuilder.ToString());
            }



            var ioMethodsAttributes = classDecl.AttributeLists
                .SelectMany(x => x.Attributes)
                .Where(x => x.GetAttributeFullName().Equals(IOMethodsForAttributeFullName)).ToArray();

            var full = ioMethodsAttributes
                .SelectMany(x =>
                    x.ArgumentList?.Arguments.Select(n => n.GetAttributeParameterValue<string>(tSym)) ?? Enumerable.Repeat("*", 1))
                .ToArray();



            foreach (var attribute in full)
            {
                var methodInfo = new MethodInfoModel()
                {
                    IOType = IOTypeEnum.Write,
                    ForGroup = attribute,
                    Parameters = new List<parametermodel>() {
                    new parametermodel(){  name = "packet", typeName = typeof(OutputPacketBuffer).Name }
                    },
                    ClassDeclarationSyntax = classDecl,
                    MethodModifier = "public",
                    ReadType = classDecl.GetClassName()
                };

                var name = attribute == "*" ? "Full" : attribute;

                methodInfo.MethodName = $"Write{name}To";

                CodeBuilder methodBuilder = new CodeBuilder();
                ProcessWriteMethod(methodBuilder, methodInfo, tSym);
                classBuilder.AppendLine(methodBuilder.ToString());

                methodInfo.MethodModifier = "public static";
                methodInfo.Parameters = new List<parametermodel>()
                {
                    new parametermodel(){ name = "value", typeName = methodInfo.ReadType },
                    methodInfo.Parameters[0]
                };

                methodBuilder = new CodeBuilder();
                ProcessWriteMethod(methodBuilder, methodInfo, tSym);
                classBuilder.AppendLine(methodBuilder.ToString());

                methodInfo.IOType = IOTypeEnum.Read;
                methodInfo.MethodName = $"Read{name}From";
                methodInfo.Parameters = new List<parametermodel>()
                {
                    new parametermodel(){  name = "data", typeName = typeof(InputPacketBuffer).Name }
                };

                methodBuilder = new CodeBuilder();
                ProcessReadMethod(methodBuilder, methodInfo, tSym);
                classBuilder.AppendLine(methodBuilder.ToString());
            }



            classBuilder.PrevTab();

            classBuilder.AppendLine("}");

            string outputValue = classBuilder.ToString();

            if (ns != null)
            {
                var nsBuilder = new CodeBuilder();

                nsBuilder.AppendLine($"namespace {ns.Name.ToString()}");
                nsBuilder.AppendLine("{");

                nsBuilder.NextTab();

                nsBuilder.AppendLine(outputValue);

                nsBuilder.PrevTab();

                nsBuilder.AppendLine("}");

                outputValue = nsBuilder.ToString();
            }
            // Visual studio have lag(or ...) cannot show changes any time
            //#if DEVELOP
            //#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            //            System.IO.File.WriteAllText($@"C:\Work\temp\{classIdentityName}.binaryio.cs", outputValue);
            //#pragma warning restore RS1035 // Do not use APIs banned for analyzers
            //#endif

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            context.AddSource($"{classIdentityName}.binaryio.cs", outputValue);
        }


        private void ProcessReadMethod(CodeBuilder methodBuilder, MethodInfoModel methodInfo, SemanticModel tSym)
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

            if (bufferParam.name != "__packet")
            {
                methodBuilder.AppendLine($"var dataPacket = {bufferParam.name};");
                methodBuilder.AppendLine();
            }

            var bgContext = new BinaryTypeIOGeneratorContext()
            {
                For = methodInfo.ForGroup,
                SemanticModel = tSym,
                ProcessingType = classDecl.GetClassName(),
                ReadCurrentTypeMethodName = methodInfo.MethodName,
                IsStatic = methodInfo.MethodModifier.Contains("static")
            };

            methodBuilder.AppendLine($"return {BinaryReadMethodsGenerator.GetValueReadSegment(tSym.GetDeclaredSymbol(classDecl), bgContext, default)};");

            methodBuilder.PrevTab();
            methodBuilder.AppendLine("}");
        }

        private void ProcessWriteMethod(CodeBuilder methodBuilder, MethodInfoModel methodInfo, SemanticModel tSym)
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

            if (bufferParam.name != "__packet")
            {
                methodBuilder.AppendLine($"var __packet = {bufferParam.name};");
                methodBuilder.AppendLine();
            }

            var bgContext = new BinaryTypeIOGeneratorContext()
            {
                For = methodInfo.ForGroup,
                SemanticModel = tSym,
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

        #region ISourceGenerator

        public void Execute(GeneratorExecutionContext context)
        {
            //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.BinaryGenerator", StringComparison.OrdinalIgnoreCase)))
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(
            //    new DiagnosticDescriptor(
            //        "SG0001",
            //        "Not found reference",
            //        $"Cannot find reference \"NSL.Generators.BinaryGenerator\"",
            //        "none",
            //        DiagnosticSeverity.Error,
            //        true,
            //        $"Cannot find reference \"NSL.Generators.BinaryGenerator\""), Location.None));
            //}
            //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.Utils", StringComparison.OrdinalIgnoreCase)))
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(
            //    new DiagnosticDescriptor(
            //        "SG0001",
            //        "Not found reference",
            //        $"Cannot find reference \"NSL.Generators.Utils\"",
            //        "none",
            //        DiagnosticSeverity.Error,
            //        true, 
            //        $"Cannot find reference \"NSL.Generators.Utils\""), Location.None));
            //}
            //if (!context.Compilation.ReferencedAssemblyNames.Any(ai => ai.Name.Equals("NSL.Generators.BinaryTypeIOGenerator.Attributes", StringComparison.OrdinalIgnoreCase)))
            //{
            //    context.ReportDiagnostic(Diagnostic.Create(
            //    new DiagnosticDescriptor(
            //        "SG0001",
            //        "Not found reference",
            //        $"Cannot find reference \"NSL.Generators.BinaryTypeIOGenerator.Attributes\"",
            //        "none",
            //        DiagnosticSeverity.Error,
            //        true,
            //        $"Cannot find reference \"NSL.Generators.BinaryTypeIOGenerator.Attributes\""), Location.None));
            //}

            if (context.SyntaxReceiver is BinaryIOAttributeSyntaxReceiver methodSyntaxReceiver)
            {
                ProcessBinaryIOTypes(context, methodSyntaxReceiver);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
                new BinaryIOAttributeSyntaxReceiver());
        }

        #endregion

        private readonly string ReadMethodAttributeFullName = typeof(BinaryIOReadMethodAttribute).Name;

        private readonly string WriteMethodAttributeFullName = typeof(BinaryIOWriteMethodAttribute).Name;

        private readonly string IOMethodsForAttributeFullName = typeof(BinaryIOMethodsForAttribute).Name;
    }
}
