using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Extensions.RPC.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NSL.SocketCore.Utils.Buffer;
using NSL.Extensions.RPC.Generator.Models;
using System.Diagnostics;
using System.Reflection.Metadata;
using NSL.Extensions.RPC.Generator.Generators.Handlers;
using System.Data.Common;

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal class WriteMethodsGenerator
    {

        private static List<GenerateHandle> generators = new List<GenerateHandle>();

        static WriteMethodsGenerator()
        {
            generators.Add(CustomTypeGenerator.GetWriteLine);
            generators.Add(ArrayTypeGenerator.GetWriteLine);
            generators.Add(BaseTypeGenerator.GetWriteLine);
            generators.Add(NullableTypeGenerator.GetWriteLine);
            generators.Add(ClassTypeGenerator.GetWriteLine);
            generators.Add(StructTypeGenerator.GetWriteLine);
        }


        internal static string BuildWriteMethods(MethodDeclModel methodDecl)
        {
            CodeBuilder cb = new CodeBuilder();

            foreach (var mov in methodDecl.Overrides)
            {
                if (!mov.DeclSyntax.Modifiers.Any(x => x.Text.Equals("virtual")))
                    continue;

                var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(mov.DeclSyntax.SyntaxTree);

                MethodContextModel mcm = new MethodContextModel() { Method = methodDecl, SemanticModel = semanticModel, methodSyntax = mov.DeclSyntax };

                cb.AppendLine(BuildWriteMethod(mcm));
            }

            return cb.ToString();
        }

        //private static string BuildWriteMethod(MethodDecl method, MethodDeclarationSyntax decl, Microsoft.CodeAnalysis.SemanticModel semanticModel)
        private static string BuildWriteMethod(MethodContextModel mcm)
        {
            CodeBuilder cb = new CodeBuilder();

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var modText = mcm.methodSyntax.Modifiers.Remove(mcm.methodSyntax.Modifiers.First(x => x.Text.Equals("virtual"))).Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword)).ToString();

            cb.AppendLine($"{modText} {mcm.methodSyntax.ReturnType} {mcm.Method.Name}({string.Join(", ", mcm.methodSyntax.ParameterList.Parameters.Select(x => $"{x.Type} {x.Identifier.Text}"))})");
            cb.AppendLine("{");

            cb.NextTab();


            cb.AppendLine($"var __packet = Processor.CreateCall(GetContainerName(), \"{mcm.methodSyntax.Identifier.Text}\", {mcm.methodSyntax.ParameterList.Parameters.Count});");

            foreach (var parameter in mcm.methodSyntax.ParameterList.Parameters)
            {
                var parameterSymbol = mcm.SemanticModel.GetDeclaredSymbol(parameter);

                mcm.CurrentParameter = parameter;

                mcm.CurrentParameterSymbol = parameterSymbol;

                string path = parameter.Identifier.Text;

                cb.AppendLine();

                cb.AppendLine(BuildParameterWriter(parameterSymbol, mcm, path, RPCGenerator.GetParameterIgnoreMembers(parameterSymbol, mcm)));
            }

            cb.AppendLine();


            //if (!Debugger.IsAttached)
            //    Debugger.Launch();


            var symbol = mcm.SemanticModel.GetDeclaredSymbol(mcm.methodSyntax);
            if (symbol.ReturnsVoid)
            {
                cb.AppendLine($"Processor.SendWait(__packet);");
            }
            else
            {

                cb.AppendLine($"var dataPacket = Processor.SendWithResultData(__packet);");

                cb.AppendLine();

                var sr = ReadMethodsGenerator.GetValueReadSegment(symbol.ReturnType, mcm, "result");

                cb.AppendLine($"var result = {sr.TrimEnd(';')};");

                cb.AppendLine();

                cb.AppendLine($"dataPacket.Dispose();");

                cb.AppendLine();

                cb.AppendLine($"return result;");
            }

            cb.PrevTab();

            cb.AppendLine("}");
            return cb.ToString();
        }

        public static string BuildParameterWriter(ISymbol item, MethodContextModel mcm, string path, IEnumerable<string> ignoreMembers)
        {
            string writerLine = default;

            if (ignoreMembers == null || !ignoreMembers.Any(x => x.Equals("*")))
            {
                foreach (var gen in generators)
                {
                    writerLine = gen(item, mcm, path, ignoreMembers);

                    if (writerLine != default)
                        break;
                }
            }

            return writerLine ?? ""; //debug only
        }


        public static void AddTypeMemberWriteLine(ISymbol member, MethodContextModel mcm, CodeBuilder cb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (RPCGenerator.IsIgnoreMember(member, mcm))
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();
                    cb.AppendLine(BuildParameterWriter(ptype, mcm, path, null));

                    cb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();
                cb.AppendLine(BuildParameterWriter(ftype, mcm, path, null));
                cb.AppendLine();
            }
        }
    }
}
