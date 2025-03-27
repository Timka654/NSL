using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Generators.Utils;
using System.Reflection;
using NSL.Generators.BinaryGenerator;
using NSL.SocketCore.Utils.Buffer;
using NSL.Extensions.RPC.Generator.Attributes;

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal class ReadMethodsGenerator
    {
        public static string BuildParameterReader(ParameterSyntax parameterSyntax, MethodContextModel mcm)
        {
            CodeBuilder pb = new CodeBuilder();
            //GenDebug.Break();



            var parameterSymbol = mcm.SemanticModel.GetDeclaredSymbol(parameterSyntax);

            mcm.CurrentParameter = parameterSyntax;

            mcm.CurrentParameterSymbol = parameterSymbol;

            binaryContext.SemanticModel = mcm.SemanticModel;
            binaryContext.IgnorePaths = RPCGenerator.GetParameterIgnoreMembers(parameterSymbol, mcm).ToArray();

            string valueReader = GetValueReadSegment(parameterSymbol, mcm, null);

            pb.AppendLine(valueReader);

            return pb.ToString();
        }

        public static string GetValueReadSegment(ISymbol parameter, MethodContextModel methodContext, string path)
            => BinaryReadMethodsGenerator.GetValueReadSegment(parameter, binaryContext, path);

        public static string BuildNameHandle(IEnumerable<MethodDeclModel> methodDecl)
        {
            var mb = new CodeBuilder();

            mb.AppendLine($"public override void InvokeMethod({nameof(InputPacketBuffer)} dataPacket)");
            mb.AppendLine("{");

            mb.NextTab(); // 1

            mb.AppendLine($"var name = dataPacket.{nameof(InputPacketBuffer.ReadInt32)}();");
            mb.AppendLine();

            mb.AppendLine($"switch (name)");
            mb.AppendLine($"{{");



            mb.NextTab(); // 2
            foreach (var method in methodDecl)
            {
                mb.AppendLine($"case {HasherUtils.GetInt32HashCode(method.Name)}:");
                mb.NextTab(); // 3
                mb.AppendLine($"{RPCGenerator.GetMethodRPCHandleName(method.Name)}(dataPacket);");
                mb.AppendLine($"break;");
                mb.PrevTab(); // 2
            }

            mb.AppendLine($"default:");
            mb.NextTab(); // 3
            mb.AppendLine($"throw new System.Exception($\"RPC method \\\"{{name}}\\\" not exists on remote\");");

            mb.PrevTab(); // 2

            mb.PrevTab(); // 1

            mb.AppendLine("}");

            mb.PrevTab(); // 0

            mb.AppendLine("}");

            return mb.ToString();
        }

        public static string BuildMethod(MethodDeclModel methodDecl)
        {
            CodeBuilder mb = new CodeBuilder();

            mb.AppendLine($"private void {RPCGenerator.GetMethodRPCHandleName(methodDecl.Name)}(InputPacketBuffer dataPacket)");
            mb.AppendLine("{");


            mb.NextTab(); // 1
            mb.AppendLine($"var paramHash = dataPacket.{nameof(InputPacketBuffer.ReadInt32)}();");

            mb.AppendLine($"Guid rid = dataPacket.ReadGuid();");

            mb.AppendLine();

            mb.AppendLine("switch (paramHash)");
            mb.AppendLine("{");

            mb.NextTab(); // 2

            var classSemanticModel = methodDecl.Class.SemanticModel;

            foreach (var mov in methodDecl.Overrides)
            {
                mb.AppendLine($"case {HasherUtils.GetInt32HashCode(mov.DeclSyntax.ParameterList.Parameters.Select(x => x.ToFullString()).ToArray())}:");

                mb.AppendLine("{");

                mb.NextTab(); // 4

                List<string> parameters = new List<string>();

                var semanticModel = methodDecl.Class.SemanticModel;

                var returnTypeInfo = semanticModel.GetTypeInfo(mov.DeclSyntax.ReturnType);

                var movSymbol = classSemanticModel.GetDeclaredSymbol(mov.DeclSyntax);

                var p = new MethodContextModel()
                {
                    Method = methodDecl,
                    methodSyntax = mov.DeclSyntax,
                    SemanticModel = semanticModel,
                    IsAsync = mov.DeclSyntax.Modifiers.Any(x => x.ValueText.Equals("async")),
                    IsTask = returnTypeInfo.Type.GetMembers().Any(x => x.MetadataName.Equals("GetAwaiter"))
                };

                foreach (var item in mov.DeclSyntax.ParameterList.Parameters)
                {
                    var attributes = item.AttributeLists
                        .SelectMany(n => n.Attributes)
                        .Where(x => x.GetAttributeFullName().Equals(RPCCustomMemberIgnoreAttributeFullName)).ToArray();

                    if (attributes.Any(x => x.ArgumentList == null || x.ArgumentList.Arguments.Any(b => b.GetAttributeParameterValue<string>(semanticModel).Equals("*"))))
                    {
                        mb.AppendLine($"{item.Type} {item.Identifier} = default;");
                    }
                    else
                    {
                        mb.AppendLine(BuildParameterReader(item, p));
                        mb.AppendLine();
                    }

                    parameters.Add(item.Identifier.Text);
                }


                BuildTrySegment(mb, b =>
                {
                    if (movSymbol.ReturnsVoid)
                    {
                        mb.AppendLine($"base.{methodDecl.Name}({string.Join(", ", parameters)});");
                    }
                    else
                    {

                        bool asyncClose = false;

                        var rType = movSymbol.ReturnType as INamedTypeSymbol;

                        if (p.IsTask)
                        {
                            asyncClose = true;

                            mb.AppendLine("System.Threading.Tasks.Task.Run(async () => {");

                            mb.NextTab(); // 5

                            if (rType.TypeArguments.Any())
                            {
                                rType = rType.TypeArguments.First() as INamedTypeSymbol;

                                mb.AppendLine($"var result = await base.{methodDecl.Name}({string.Join(", ", parameters)});");
                            }
                            else
                            {
                                rType = null;

                                mb.AppendLine($"await base.{methodDecl.Name}({string.Join(", ", parameters)});");
                            }
                        }
                        else
                        {

                            mb.NextTab(); // 5
                            mb.AppendLine($"var result = base.{methodDecl.Name}({string.Join(", ", parameters)});");
                        }

                        if (rType != null)
                        {
                            mb.AppendLine();

                            mb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(rType, p, "result"));
                        }

                        if (asyncClose)
                        {
                            mb.PrevTab(); // 5

                            mb.AppendLine("});");
                        }

                    }
                });

                mb.PrevTab(); // 4


                mb.AppendLine("}");
                mb.AppendLine("break;");

                mb.PrevTab(); // 3
            }


            mb.AppendLine($"default:");
            mb.NextTab(); // 3
            mb.AppendLine($"throw new System.Exception($\"RPC method \\\"{methodDecl.Name}\\\" not have override with {{paramHash}} parameter hash on remote\");");
            mb.AppendLine($"break;");

            mb.PrevTab(); // 2



            mb.PrevTab(); //1

            mb.AppendLine("}");


            mb.PrevTab(); // 0

            mb.AppendLine("}");

            return mb.ToString();
        }




        private static string BuildTrySegment(CodeBuilder cb, Action<CodeBuilder> code)
        {

            cb.AppendLine($"OutputPacketBuffer __packet = default;");

            cb.AppendLine("try {");

            cb.NextTab();

            cb.AppendLine($"__packet = Processor.CreateAnswer(rid);");

            code(cb);

            cb.AppendLine();

            cb.AppendLine($"Processor.SendAnswer(__packet);");

            cb.PrevTab();

            cb.AppendLine("}");

            cb.AppendLine("catch (Exception ex)");

            cb.AppendLine("{");

            cb.NextTab();

            cb.AppendLine($"__packet = Processor.CreateException(rid, ex);");

            cb.AppendLine($"Processor.SendAnswer(__packet);");

            cb.AppendLine($"throw;");

            cb.PrevTab();

            cb.AppendLine("}");

            return cb.ToString();
        }

        private static RPCBinaryGeneratorContext binaryContext = new RPCBinaryGeneratorContext()
        {
            IOPath = "dataPacket"
        };

        private static readonly string RPCCustomMemberIgnoreAttributeFullName = typeof(RPCCustomMemberIgnoreAttribute).Name;

    }
}
