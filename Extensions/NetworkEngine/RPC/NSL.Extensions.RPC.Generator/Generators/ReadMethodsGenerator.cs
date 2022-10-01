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

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal delegate string CustomTypeHandle(INamedTypeSymbol type, MethodContextModel methodContext, string path);
    internal delegate string GenerateHandle(ISymbol type, MethodContextModel methodContext, string path);

    internal class ReadMethodsGenerator
    {
        private static List<GenerateHandle> generators = new List<GenerateHandle>();

        static ReadMethodsGenerator()
        {
            generators.Add(CustomTypeGenerator.GetReadLine);
            generators.Add(ArrayTypeGenerator.GetReadLine);
            generators.Add(BaseTypeGenerator.GetReadLine);
            generators.Add(NullableTypeGenerator.GetReadLine);
            generators.Add(ClassTypeGenerator.GetReadLine);
            generators.Add(StructTypeGenerator.GetReadLine);
        }

        public static string BuildParameterReader(ParameterSyntax parameterSyntax, MethodContextModel parameter)
        {
            CodeBuilder pb = new CodeBuilder();
            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var parameterSymbol = parameter.SemanticModel.GetDeclaredSymbol(parameterSyntax);

            string valueReader = GetValueReadSegment(parameterSymbol, parameter, null);

            pb.AppendLine(valueReader);

            return pb.ToString();
        }

        public static string GetValueReadSegment(ISymbol parameter, MethodContextModel methodContext, string path)
        {
            string valueReader = default;

            foreach (var gen in generators)
            {
                valueReader = gen(parameter, methodContext, path);

                if (valueReader != default)
                    break;
            }

            if (valueReader == default)
                return $"({parameter.GetTypeSymbol().Name})default;";

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

            mb.AppendLine("switch (argCount)");
            mb.AppendLine("{");

            mb.NextTab(); // 2

            var classSemanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(methodDecl.Class.Class.SyntaxTree);

            foreach (var mov in methodDecl.Overrides)
            {
                mb.AppendLine($"case {mov.ParameterList.Parameters.Count}:");

                mb.NextTab(); // 3

                mb.AppendLine("{");

                mb.NextTab(); // 4

                List<string> parameters = new List<string>();

                var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(mov.SyntaxTree);

                var movSymbol = classSemanticModel.GetDeclaredSymbol(mov);

                var p = new MethodContextModel()
                {
                    Method = methodDecl,
                    methodSyntax = mov,
                    SemanticModel = semanticModel
                };

                foreach (var item in mov.ParameterList.Parameters)
                {

                    mb.AppendLine(BuildParameterReader(item, p));
                    mb.AppendLine();

                    parameters.Add(item.Identifier.Text);
                }


                //if (!Debugger.IsAttached)
                //    Debugger.Launch();


                if (movSymbol.ReturnsVoid)
                {

                    mb.AppendLine("try {");

                    mb.NextTab();

                    mb.AppendLine($"var __packet = Processor.CreateAnswer(dataPacket.ReadGuid());");

                    mb.AppendLine();

                    mb.AppendLine($"base.{methodDecl.Name}({string.Join(", ", parameters)});");

                    mb.AppendLine();

                    mb.AppendLine($"Processor.SendAnswer(__packet);");

                    mb.PrevTab();

                    mb.AppendLine("}");

                    mb.AppendLine("catch (Exception ex)");

                    mb.AppendLine("{");

                    mb.NextTab();

                    mb.AppendLine($"var __packet = Processor.CreateException(dataPacket.ReadGuid(), ex);");

                    mb.AppendLine($"Processor.SendAnswer(__packet);");

                    mb.AppendLine($"throw;");

                    mb.PrevTab();

                    mb.AppendLine("}");
                }
                else
                {

                    mb.AppendLine("try {");

                    mb.NextTab();

                    mb.AppendLine($"var result = base.{methodDecl.Name}({string.Join(", ", parameters)});");

                    mb.AppendLine();

                    mb.AppendLine($"var __packet = Processor.CreateAnswer(dataPacket.ReadGuid());");

                    mb.AppendLine();

                    mb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(movSymbol.ReturnType, p, "result"));

                    mb.AppendLine();

                    mb.AppendLine($"Processor.SendAnswer(__packet);");

                    mb.PrevTab();

                    mb.AppendLine("}");

                    mb.AppendLine("catch (Exception ex)");

                    mb.AppendLine("{");

                    mb.NextTab();

                    mb.AppendLine($"var __packet = Processor.CreateException(dataPacket.ReadGuid(), ex);");

                    mb.AppendLine($"Processor.SendAnswer(__packet);");

                    mb.AppendLine($"throw;");

                    mb.PrevTab();

                    mb.AppendLine("}");
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

    }
}
