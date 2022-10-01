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

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal class ReadMethodsGenerator
    {
        public static string BuildParameterReader(MethodDecl methodDecl, MethodDeclarationSyntax method, ParameterSyntax parameter, SemanticModel semanticModel)
        {
            CodeBuilder pb = new CodeBuilder();

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);

            string valueReader = GetValueReadSegment(parameterSymbol, semanticModel, null);

            pb.AppendLine(valueReader);

            return pb.ToString();
        }

        public static string GetValueReadSegment(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            string valueReader = GetBaseTypeReadLine(parameter, semanticModel, path);

            if (valueReader == default)
            {
                valueReader = GetNullableTypeReadLine(parameter, semanticModel, path);

                if (valueReader == default)
                {
                    valueReader = GetClassReadLine(parameter, semanticModel, path);

                    if (valueReader == default)
                        valueReader = GetStructReadLine(parameter, semanticModel, path);
                }
            }

            if (valueReader == default)
                return $"({parameter.GetTypeSymbol().Name})default;";

            var linePrefix = GetLinePrefix(parameter, path);

            if (linePrefix == default)
                return valueReader;

            return $"{linePrefix}{valueReader}{(valueReader.EndsWith(";") ? string.Empty : ";")}";
        }

        private static string BuildNullableTypeDef(IFieldSymbol field)
        {
            if (!field.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return field.Type.Name;

            if(!field.Type.IsValueType)
                return field.Type.Name;

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var genericType = ((INamedTypeSymbol)field.Type).TypeArguments.First();

            return $"{genericType.Name}?";
        }

        private static string GetNullableTypeReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            CodeBuilder rb = new CodeBuilder();

            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;



            return $"dataPacket.{nameof(InputPacketBuffer.ReadNullable)}(()=>{{ return {GetValueReadSegment(genericType, semanticModel, path)}; }})";
        }

        private static string GetBaseTypeReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!readTypeHandlers.TryGetValue(type.Name, out var tReadLine))
                return default;

            return $"dataPacket.{tReadLine}()";
        }

        private static void AddTypeMemberReadLine(ISymbol member, CodeBuilder rb, string path, SemanticModel semanticModel)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();

                    rb.AppendLine($"{path} = {GetValueReadSegment(ptype, semanticModel, path)};");

                    rb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();

                rb.AppendLine($"{path} = {GetValueReadSegment(ftype, semanticModel, path)};");

                rb.AppendLine();
            }
        }

        private static string GetClassReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder rb = new CodeBuilder();


            rb.AppendLine($"dataPacket.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {{");

            rb.NextTab();

            rb.AppendLine();

            rb.AppendLine($"var data = new {type.Name}();");

            rb.AppendLine();

            var members = type.GetMembers();


            path = "data";

            foreach (var member in members)
            {
                AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name), semanticModel);
            }

            //foreach (var item in typeSymbol.GetMembers())
            //{

            //}

            rb.AppendLine("return data;");

            rb.PrevTab();

            rb.AppendLine("})");

            return rb.ToString();// test only
        }

        private static string GetStructReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            var rb = new CodeBuilder();

            if (type.IsTupleType)
            {
                //if (!Debugger.IsAttached)
                //    Debugger.Launch();

                var nts = type as INamedTypeSymbol;

                rb.AppendLine($@"new {type.Name}<{string.Join(", ", nts.TupleElements.Select(x => BuildNullableTypeDef(x)))}>();");
            }
            else
                rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            path = parameter.GetName(path) ?? default;

            var members = type.GetMembers();


            foreach (var member in members)
            {
                AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name), semanticModel);
            }




            return rb.ToString();// test only
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


        private static Dictionary<string, string> readTypeHandlers = new Dictionary<string, string>()
        {
            {
                typeof(bool).Name,
                nameof(InputPacketBuffer.ReadBool)
            },
            {
                typeof(byte).Name,
                nameof(InputPacketBuffer.ReadByte)
            },
            {
                typeof(short).Name,
                nameof(InputPacketBuffer.ReadInt16)
            },
            {
                typeof(int).Name,
                nameof(InputPacketBuffer.ReadInt32)
            },
            {
                typeof(long).Name,
                nameof(InputPacketBuffer.ReadInt64)
            },
            {
                typeof(ushort).Name,
                nameof(InputPacketBuffer.ReadUInt16)
            },
            {
                typeof(uint).Name,
                nameof(InputPacketBuffer.ReadUInt32)
            },
            {
                typeof(ulong).Name,
                nameof(InputPacketBuffer.ReadUInt64)
            },
            {
                typeof(float).Name,
                nameof(InputPacketBuffer.ReadFloat)
            },
            {
                typeof(double).Name,
                nameof(InputPacketBuffer.ReadDouble)
            },
            {
                typeof(string).Name,
                nameof(InputPacketBuffer.ReadString32)
            },
            {
                typeof(DateTime).Name,
                nameof(InputPacketBuffer.ReadDateTime)
            },
            {
                typeof(TimeSpan).Name,
                nameof(InputPacketBuffer.ReadTimeSpan)
            },
        };

        public static string BuildNameHandle(IEnumerable<MethodDecl> methodDecl)
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

        public static string BuildMethod(MethodDecl methodDecl)
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

                foreach (var item in mov.ParameterList.Parameters)
                {
                    mb.AppendLine(BuildParameterReader(methodDecl, mov, item, semanticModel));
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

                    mb.AppendLine(WriteMethodsGenerator.BuildParameterWriter(movSymbol.ReturnType, "result"));

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
