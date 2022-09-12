﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Linq;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace NSL.Extensions.RPC.Generator
{
    internal class ReadMethodsGenerator
    {
        private static string BuildParameterReader(MethodDecl methodDecl, MethodDeclarationSyntax method, ParameterSyntax parameter)
        {
            CodeBuilder pb = new CodeBuilder();

            //if (!Debugger.IsAttached)
            //    Debugger.Launch();

            var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(method.SyntaxTree);

            var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);

            string valueReader = GetValueReadSegment(parameterSymbol, semanticModel, null);

            pb.AppendLine(valueReader);

            return pb.ToString();
        }

        private static string GetValueReadSegment(ISymbol parameter, SemanticModel semanticModel, string path)
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
                return $"({GetTypeSymbol(parameter).Name})default;";

            var linePrefix = GetLinePrefix(parameter, path);

            if (linePrefix == default)
                return valueReader;

            return $"{linePrefix}{valueReader}{(valueReader.EndsWith(";") ? string.Empty : ";")}";
        }

        private static string GetNullableTypeReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            CodeBuilder rb = new CodeBuilder();

            var type = GetTypeSymbol(parameter);

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            if (!Debugger.IsAttached)
                Debugger.Launch();

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;



            return $"packet.{nameof(InputPacketBuffer.ReadNullable)}(()=>{{ return {GetValueReadSegment(genericType, semanticModel, path)}; }})";
        }

        private static string GetBaseTypeReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            var type = GetTypeSymbol(parameter);

            if (!readTypeHandlers.TryGetValue(type.Name, out var tReadLine))
                return default;

            return $"packet.{tReadLine}()";
        }

        private static void AddTypeMemberReadLine(ISymbol member, CodeBuilder rb, string path, SemanticModel semanticModel)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = GetTypeSymbol(ps);

                    rb.AppendLine($"{path} = {GetValueReadSegment(ptype, semanticModel, path)};");

                    rb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = GetTypeSymbol(fs);

                rb.AppendLine($"{path} = {GetValueReadSegment(ftype, semanticModel, path)};");

                rb.AppendLine();
            }
        }

        private static string GetClassReadLine(ISymbol parameter, SemanticModel semanticModel, string path)
        {
            var type = GetTypeSymbol(parameter);

            if (type.IsValueType)
                return default;

            CodeBuilder rb = new CodeBuilder();


            rb.AppendLine($"packet.{nameof(InputPacketBuffer.ReadNullableClass)}(() => {{");

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
            var type = GetTypeSymbol(parameter);

            if (!type.IsValueType)
                return default;

            var rb = new CodeBuilder();


            rb.AppendLine($"new {type.Name}();");

            rb.AppendLine();

            path = GetName(parameter, path) ?? default;

            var members = type.GetMembers();


            foreach (var member in members)
            {
                AddTypeMemberReadLine(member, rb, string.Join(".", path, member.Name), semanticModel);
            }




            return rb.ToString();// test only
        }

        private static string GetName(ISymbol symbol, string path)
        {
            if (symbol is IParameterSymbol param)
                return param.Name;
            if (symbol is IFieldSymbol fi)
                return fi.Name;
            if (symbol is IPropertySymbol prop)
                return prop.Name;

            return path;
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

        private static ITypeSymbol GetTypeSymbol(ISymbol symbol)
        {
            if (symbol is IParameterSymbol param)
                return param.Type;

            if (symbol is IPropertySymbol pr)
                return pr.Type;

            if (symbol is IFieldSymbol fi)
                return fi.Type;

            if (symbol is INamedTypeSymbol typedSymbol)
                return typedSymbol;

            return default;
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

            mb.AppendLine("public override void InvokeMethod(InputPacketBuffer data)");
            mb.AppendLine("{");

            mb.NextTab(); // 1

            mb.AppendLine($"string name = data.ReadString16();");
            mb.AppendLine();

            mb.AppendLine($"switch (name)");
            mb.AppendLine($"{{");


            mb.NextTab(); // 2
            foreach (var method in methodDecl)
            {
                mb.AppendLine($"case \"{method.Name}\":");
                mb.NextTab(); // 3
                mb.AppendLine($"{RPCGenerator.GetMethodRPCHandleName(method.Name)}(data);");
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

            mb.AppendLine($"private void {RPCGenerator.GetMethodRPCHandleName(methodDecl.Name)}(InputPacketBuffer packet)");
            mb.AppendLine("{");

            mb.NextTab(); // 1

            mb.AppendLine("byte argCount = packet.ReadByte();");

            mb.AppendLine();

            mb.AppendLine("switch (argCount)");
            mb.AppendLine("{");

            mb.NextTab(); // 2

            foreach (var mov in methodDecl.Overrides)
            {
                mb.AppendLine($"case {mov.ParameterList.Parameters.Count}:");


                mb.NextTab(); // 3

                mb.AppendLine("{");

                mb.NextTab(); // 4

                List<string> parameters = new List<string>();

                foreach (var item in mov.ParameterList.Parameters)
                {
                    mb.AppendLine(BuildParameterReader(methodDecl, mov, item));
                    mb.AppendLine();

                    parameters.Add(item.Identifier.Text);
                }

                mb.AppendLine($"{methodDecl.Name}({string.Join(", ", parameters)});");

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
