﻿#define DEVELOP

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Extensions.RPC.Generator.Attributes;
using NSL.Extensions.RPC.Generator.Comparers;
using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Extensions.RPC.Generator.Generators;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Generators.BinaryGenerator;
using NSL.Generators.Utils;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NSL.Extensions.RPC.Generator
{
    [Generator]
    internal class RPCGenerator : ISourceGenerator
    {
        internal static ClassDeclComparer classDeclComparer = new ClassDeclComparer();
        internal static MethodDeclarationSyntaxComparer methodDeclarationSyntaxComparer = new MethodDeclarationSyntaxComparer();

        internal static string GetClassFullModifier(ClassDeclarationSyntax classDecl)
            => string.Join(" ", classDecl.Modifiers.Select(x => x.Text));

        internal static string GetClassName(ClassDeclarationSyntax classDecl)
            => @classDecl.Identifier.Text;

        internal static string GetClassRPCHandleName(ClassDeclarationSyntax classDecl)
            => $"{GetClassName(classDecl)}RPCRepository";

        internal static string GetMethodName(MethodDeclarationSyntax methodDecl)
            => methodDecl.Identifier.Text;

        internal static string GetMethodRPCHandleName(MethodDeclarationSyntax methodDecl)
            => GetMethodRPCHandleName(GetMethodName(methodDecl));

        internal static string GetMethodRPCHandleName(string methodName)
            => $"__{methodName}RPCRecvHandle";

        private void ProcessRPCMethods(GeneratorExecutionContext context, RPCMethodAttributeSyntaxReceiver syntaxReceiver)
        {

            var data = syntaxReceiver.RPCMethods
                .Select(x => new
                {
                    Class = (ClassDeclarationSyntax)x.Parent,
                    Method = x
                })
                .GroupBy(x => new ClassDeclModel { Class = x.Class, Context = context }, classDeclComparer)

                .Select(item =>
                {
                    item.Key.Methods = item.GroupBy(b => b.Method, methodDeclarationSyntaxComparer)
                            .Select(j => new MethodDeclModel()
                            {
                                Class = item.Key,
                                Name = j.Key.Identifier.Text,
                                Overrides = j.Select(t => new MethodOverrideDeclModel() { DeclSyntax = t.Method }).ToArray()
                            });

                    return item.Key;
                })
                .ToArray();


            foreach (var classDecl in data)
            {
                BuildClass(context, classDecl);
            }
        }

        private void ProcessRPCTypeHandlers(GeneratorExecutionContext context, RPCMethodAttributeSyntaxReceiver handleSyntaxReceiver)
        {
            foreach (var method in handleSyntaxReceiver.RPCMethods)
            {
                var m = context.Compilation.GetSemanticModel(method.SyntaxTree) as IMethodSymbol;
            }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            if (context.SyntaxReceiver is RPCMethodAttributeSyntaxReceiver methodSyntaxReceiver)
                ProcessRPCMethods(context, methodSyntaxReceiver);
        }

        private SyntaxList<UsingDirectiveSyntax> UpdateUsingDirectives(SyntaxTree originalTree)
        {
            var rootNode = originalTree.GetRoot() as CompilationUnitSyntax;
            return rootNode.Usings;
        }
        private void BuildClass(GeneratorExecutionContext context, ClassDeclModel classDecl)
        {
            var ns = classDecl.Class.Parent as NamespaceDeclarationSyntax;

            var classIdentityName = GetClassName(classDecl.Class);

            CodeBuilder classBuilder = new CodeBuilder();

            classBuilder.AppendLine("/*");

            classBuilder.AppendLine($"Auto Generated by NSL RPC. Please dont change this file");
            classBuilder.AppendLine($"Project must have reference \"NSL.SocketCore\" library for normal working");
            classBuilder.AppendLine($"Project must have reference \"NSL.Extensions.RPC\" library for normal working");

            classBuilder.AppendLine("*/");

            classBuilder.AppendLine($"using NSL.SocketCore;");
            classBuilder.AppendLine($"using NSL.SocketCore.Utils.Buffer;");
            classBuilder.AppendLine($"using NSL.Extensions.RPC;");

            var usings = UpdateUsingDirectives(classDecl.Class.SyntaxTree);

            foreach (var u in usings)
            {
                classBuilder.AppendLine(u.ToString());
            }

            classBuilder.AppendLine();

            var classSemanticModel = classDecl.Compilation.GetSemanticModel(classDecl.Class.SyntaxTree);

            classDecl.ClassSymbol = classSemanticModel.GetDeclaredSymbol(classDecl.Class);

            var generic = classDecl.Class.TypeParameterList?.Parameters.Any() == true ? $"<{string.Join(",", classDecl.Class.TypeParameterList.Parameters.Select(x => x.Identifier.Text))}>" : string.Empty;


            classBuilder.AppendLine($"{GetClassFullModifier(classDecl.Class)} class {GetClassRPCHandleName(classDecl.Class)}{generic} : {classIdentityName}{generic}");

            classBuilder.NextTab();

            foreach (var c in classDecl.Class.ConstraintClauses)
            {
                classBuilder.AppendLine(c.ToString());
            }

            classBuilder.PrevTab();

            classBuilder.AppendLine("{");

            classBuilder.NextTab();

            foreach (var method in classDecl.Methods)
            {

                classBuilder.AppendLine(WriteMethodsGenerator.BuildWriteMethods(method));
                classBuilder.AppendLine();
                classBuilder.AppendLine(ReadMethodsGenerator.BuildMethod(method));
            }

            classBuilder.AppendLine();

            classBuilder.AppendLine(ReadMethodsGenerator.BuildNameHandle(classDecl.Methods));

            classBuilder.AppendLine();

            classBuilder.AppendLine(BuildContainerNameOverride(classDecl));

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
#if DEVELOP
            System.IO.File.WriteAllText($@"C:\Work\temp\{classIdentityName}.rpcgen.cs", outputValue);
#endif

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            context.AddSource($"{classIdentityName}.rpcgen.cs", outputValue);
        }

        private string BuildContainerNameOverride(ClassDeclModel classDecl)
        {
            var classNameSymbolDisplayFormat = new SymbolDisplayFormat(
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return $"public override string GetContainerName() => \"{classDecl.ClassSymbol.ToDisplayString(classNameSymbolDisplayFormat)}\";";
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //context.RegisterForSyntaxNotifications(() =>
            //    new RPCMethodAttributeSyntaxReceiver<RPCTypeHandleAttribute>());
            context.RegisterForSyntaxNotifications(() =>
                new RPCMethodAttributeSyntaxReceiver());
        }


        static string IgnoreAttributeMetadataName = typeof(RPCMemberIgnoreAttribute).Name;

        public static bool IsIgnoreMember(ISymbol member)
        {
            var attr = member.GetAttributes();

            return attr.Any(x => x.AttributeClass.MetadataName.Equals(IgnoreAttributeMetadataName));
        }

        static INamedTypeSymbol CustomMemberIgnoreOriginType;

        public static IEnumerable<string> GetParameterIgnoreMembers(ISymbol parameter, MethodContextModel methodContext)
        {
            var attrbs = parameter.GetAttributes();

            if (CustomMemberIgnoreOriginType == default)
                CustomMemberIgnoreOriginType = methodContext.Compilation.GetTypeByMetadataName(typeof(RPCCustomMemberIgnoreAttribute).FullName);

            var attr = attrbs.FirstOrDefault(x => x.AttributeClass.Equals(CustomMemberIgnoreOriginType));

            if (attr == null)
                return new List<string>();
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            if (!attr.ConstructorArguments.Any())
                return Enumerable.Range(0, 1).Select(x => "*").ToArray();
            //attr.NamedArguments

            var arg = attr.ConstructorArguments.First();

            return arg.Values.Select(x => (string)x.Value).ToArray();
        }
    }
}
