using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Extensions.RPC.Generator.Utils;
using System.Diagnostics;
using NSL.Extensions.RPC.Generator.Models;
using NSL.Extensions.RPC.Generator.Generators.Handlers;
using NSL.Extensions.RPC.Generator.Attributes;
using System.Reflection;

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal delegate string CustomTypeHandle(INamedTypeSymbol type, MethodContextModel methodContext, string path);
    internal delegate string GenerateHandle(ISymbol type, MethodContextModel methodContext, string path, IEnumerable<string> ignoreMembers);

    internal class ReadMethodsGenerator
    {
        private static List<GenerateHandle> generators = new List<GenerateHandle>();

        static ReadMethodsGenerator()
        {
            //generators.Add(TaskTypeGenerator.GetReadLine);
            generators.Add(CustomTypeGenerator.GetReadLine);
            generators.Add(ArrayTypeGenerator.GetReadLine);
            generators.Add(BaseTypeGenerator.GetReadLine);
            generators.Add(NullableTypeGenerator.GetReadLine);
            generators.Add(ClassTypeGenerator.GetReadLine);
            generators.Add(StructTypeGenerator.GetReadLine);
        }

        public static string BuildParameterReader(ParameterSyntax parameterSyntax, MethodContextModel mcm)
        {
            CodeBuilder pb = new CodeBuilder();
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var parameterSymbol = mcm.SemanticModel.GetDeclaredSymbol(parameterSyntax);

            mcm.CurrentParameter = parameterSyntax;

            mcm.CurrentParameterSymbol = parameterSymbol;

            string valueReader = GetValueReadSegment(parameterSymbol, mcm, null, RPCGenerator.GetParameterIgnoreMembers(parameterSymbol, mcm));

            pb.AppendLine(valueReader);

            return pb.ToString();
        }

        public static string GetValueReadSegment(ISymbol parameter, MethodContextModel methodContext, string path, IEnumerable<string> ignoreMembers = null)
        {
            string valueReader = default;

            if (ignoreMembers == null || !ignoreMembers.Any(v => v.Equals("*")))
            {
                foreach (var gen in generators)
                {
                    valueReader = gen(parameter, methodContext, path, ignoreMembers);

                    if (valueReader != default)
                        break;
                }
            }
            //else if (!Debugger.IsAttached)
            //    Debugger.Launch();

            if (valueReader == default)
                valueReader = $"({parameter.GetTypeSymbol().Name})default;";

            var linePrefix = GetLinePrefix(parameter, path);

            if (linePrefix == default)
                return valueReader;

            return $"{linePrefix}{valueReader}{(valueReader.EndsWith(";") ? string.Empty : ";")}";
        }

        public static string BuildNullableTypeDef(IFieldSymbol field)
        {
            if (!field.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return field.Type.Name;

            if (!field.Type.IsValueType)
                return field.Type.Name;

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var genericType = ((INamedTypeSymbol)field.Type).TypeArguments.First();

            return $"{genericType.Name}?";
        }

        public static void AddTypeMemberReadLine(ISymbol member, CodeBuilder rb, string path, MethodContextModel methodContext)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (RPCGenerator.IsIgnoreMember(member, methodContext))
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();

                    rb.AppendLine($"{path} = {GetValueReadSegment(ptype, methodContext, path)};");

                    rb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();

                rb.AppendLine($"{path} = {GetValueReadSegment(ftype, methodContext, path)};");

                rb.AppendLine();
            }
        }

        private static string GetLinePrefix(ISymbol symbol, string path)
        {
            string name = default;

            if (symbol is IParameterSymbol param)
                name = param.Name;


            if (name == null)
                return default;

            if (path != default)
            {
                return $"{path}.{name} = ";
            }

            return $"var {name} = ";
        }



        public static string BuildNameHandle(IEnumerable<MethodDeclModel> methodDecl)
        {
            var mb = new CodeBuilder();

            mb.AppendLine("public override void InvokeMethod(InputPacketBuffer dataPacket)");
            mb.AppendLine("{");

            mb.NextTab(); // 1

            mb.AppendLine($"string name = dataPacket.ReadString16();");
            mb.AppendLine();

            mb.AppendLine($"switch (name)");
            mb.AppendLine($"{{");



            mb.NextTab(); // 2
            foreach (var method in methodDecl)
            {
                mb.AppendLine($"case \"{method.Name}\":");
                mb.NextTab(); // 3
                mb.AppendLine($"{RPCGenerator.GetMethodRPCHandleName(method.Name)}(dataPacket);");
                mb.AppendLine($"break;");
                mb.PrevTab(); // 2
            }

            mb.AppendLine($"default:");
            mb.NextTab(); // 3
            mb.AppendLine($"throw new System.Exception($\"RPC method \\\"{{name}}\\\" not exists on remote\");");
            mb.AppendLine($"break;");

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

            mb.AppendLine("byte argCount = dataPacket.ReadByte();");

            mb.AppendLine();

            mb.AppendLine($"Guid rid = dataPacket.ReadGuid();");

            mb.AppendLine();

            mb.AppendLine("switch (argCount)");
            mb.AppendLine("{");

            mb.NextTab(); // 2

            var classSemanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(methodDecl.Class.Class.SyntaxTree);

            foreach (var mov in methodDecl.Overrides)
            {
                mb.AppendLine($"case {mov.DeclSyntax.ParameterList.Parameters.Count}:");

                mb.NextTab(); // 3

                mb.AppendLine("{");

                mb.NextTab(); // 4

                List<string> parameters = new List<string>();

                var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(mov.DeclSyntax.SyntaxTree);

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

                    mb.AppendLine(BuildParameterReader(item, p));
                    mb.AppendLine();

                    parameters.Add(item.Identifier.Text);
                }


                //if (!Debugger.IsAttached)
                //    Debugger.Launch();


                if (movSymbol.ReturnsVoid)
                {
                    BuildTrySegment(mb, b =>
                    {
                        mb.AppendLine($"base.{methodDecl.Name}({string.Join(", ", parameters)});");
                    });
                }
                else
                {

                    var rType = movSymbol.ReturnType;
                    bool trun = false;
                    if (p.IsTask && rType is INamedTypeSymbol nts && nts.TypeArguments.Any())
                    {
                        rType = nts.TypeArguments.First();

                        //if (!Debugger.IsAttached)
                        //    Debugger.Break();

                        trun = true;

                        mb.AppendLine("System.Threading.Tasks.Task.Run(async () => {");
                        mb.NextTab();
                    }

                    BuildTrySegment(mb, b =>
                    {
                        mb.AppendLine($"var result = {(trun? "await" : string.Empty)} base.{methodDecl.Name}({string.Join(", ", parameters)});");

                        mb.AppendLine();

                        mb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(rType, p, "result", null));
                    });

                    if (trun)
                    {
                        mb.PrevTab();
                        mb.AppendLine("});");
                    }

                }

                mb.PrevTab(); // 3


                mb.AppendLine("}");
                mb.AppendLine("break;");

                mb.PrevTab(); // 2
            }


            mb.AppendLine($"default:");
            mb.NextTab(); // 3
            mb.AppendLine($"throw new System.Exception($\"RPC method \\\"{methodDecl.Name}\\\" not have override with {{argCount}} args count on remote\");");
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

    }
}
