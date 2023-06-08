using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Generators.BinaryGenerator;

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal class WriteMethodsGenerator
    {
        internal static string BuildWriteMethods(MethodDeclModel methodDecl)
        {
            CodeBuilder cb = new CodeBuilder();

            foreach (var mov in methodDecl.Overrides)
            {
                if (!mov.DeclSyntax.Modifiers.Any(x => x.Text.Equals("virtual")))
                    continue;

                var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(mov.DeclSyntax.SyntaxTree);

                var returnTypeInfo = semanticModel.GetTypeInfo(mov.DeclSyntax.ReturnType);

                //if (!Debugger.IsAttached)
                //    Debugger.Launch();

                MethodContextModel mcm = new MethodContextModel()
                {
                    Method = methodDecl,
                    SemanticModel = semanticModel,
                    methodSyntax = mov.DeclSyntax,
                    IsAsync = mov.DeclSyntax.Modifiers.Any(x => x.ValueText.Equals("async")),
                    IsTask = returnTypeInfo.Type.GetMembers().Any(x => x.MetadataName.Equals("GetAwaiter"))
                };

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


            var movSymbol = mcm.SemanticModel.GetDeclaredSymbol(mcm.methodSyntax);




            var rType = movSymbol.ReturnType;
            bool trun = false;
            if (mcm.IsTask && rType is INamedTypeSymbol nts)
            {
                if (nts.TypeArguments.Any())
                    rType = nts.TypeArguments.First();
                else
                    rType = default;

                //if (!Debugger.IsAttached)
                //    Debugger.Launch();

                trun = true;

                cb.AppendLine($"{(mcm.IsAsync ? ((rType != null ? "return " : String.Empty) + "await") : "return")} System.Threading.Tasks.Task.Run(() => {{");
                cb.NextTab();
            }


            if (movSymbol.ReturnsVoid || rType == default)
            {
                cb.AppendLine($"Processor.SendWait(__packet);");
            }
            else
            {

                cb.AppendLine($"var dataPacket = Processor.SendWithResultData(__packet);");

                cb.AppendLine();

                var sr = ReadMethodsGenerator.GetValueReadSegment(rType, mcm, "result");

                cb.AppendLine($"var result = {sr.TrimEnd(';')};");

                cb.AppendLine();

                cb.AppendLine($"dataPacket.Dispose();");

                cb.AppendLine();

                cb.AppendLine($"return result;");
            }

            if (trun)
            {
                cb.PrevTab();
                cb.AppendLine("});");
            }

            cb.PrevTab();

            cb.AppendLine("}");
            return cb.ToString();
        }

        public static string BuildParameterWriter(ISymbol item, MethodContextModel mcm, string path, IEnumerable<string> ignoreMembers)
            => BinaryWriteMethodsGenerator.BuildParameterWriter(item, binaryContext, path, ignoreMembers);


        public static void AddTypeMemberWriteLine(ISymbol member, MethodContextModel mcm, CodeBuilder cb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (RPCGenerator.IsIgnoreMember(member))
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

        private static RPCBinaryGeneratorContext binaryContext = new RPCBinaryGeneratorContext();
    }
}
