using Microsoft.CodeAnalysis.CSharp.Syntax;
using NSL.Extensions.RPC.Generator.Declarations;
using NSL.Extensions.RPC.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection.Metadata;
using Microsoft.CodeAnalysis;
using NSL.SocketCore.Utils.Buffer;
using System.Data.Common;
using System.IO;

namespace NSL.Extensions.RPC.Generator.Generators
{
    internal class WriteMethodsGenerator
    {
        internal static string BuildWriteMethods(MethodDecl methodDecl)
        {
            CodeBuilder cb = new CodeBuilder();

            foreach (var mov in methodDecl.Overrides)
            {
                if (!mov.Modifiers.Any(x => x.Text.Equals("virtual")))
                    continue;

                var semanticModel = methodDecl.Class.Context.Compilation.GetSemanticModel(mov.SyntaxTree);

                cb.AppendLine(BuildWriteMethod(methodDecl, mov, semanticModel));
            }

            return cb.ToString();
        }

        private static string BuildWriteMethod(MethodDecl method, MethodDeclarationSyntax decl, Microsoft.CodeAnalysis.SemanticModel semanticModel)
        {
            CodeBuilder cb = new CodeBuilder();

            if (!Debugger.IsAttached)
                Debugger.Launch();

            var modText = decl.Modifiers.Remove(decl.Modifiers.First(x => x.Text.Equals("virtual"))).Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword)).ToString();

            cb.AppendLine($"{modText} {decl.ReturnType} {method.Name}({string.Join(",", decl.ParameterList.Parameters.Select(x => $"{x.Type} {x.Identifier.Text}"))})");
            cb.AppendLine("{");

            cb.NextTab();

            cb.AppendLine($"var __packet = new OutputPacketBuffer();");

            foreach (var parameter in decl.ParameterList.Parameters)
            {
                var parameterSymbol = semanticModel.GetDeclaredSymbol(parameter);

                string path = parameter.Identifier.Text;

                cb.AppendLine();

                cb.AppendLine(BuildParameterWriter(parameterSymbol, path));
            }

            cb.AppendLine();

            cb.AppendLine($"NetworkClient.Network.Send(__packet);");

            cb.PrevTab();

            cb.AppendLine("}");
            return cb.ToString();
        }

        private static string BuildParameterWriter(ISymbol item, string path)
        {
            var writerLine = GetBaseTypeWriteLine(item, path);

            if (writerLine == default)
            {
                writerLine = GetNullableTypeWriteLine(item, path);

                if (writerLine == default)
                {
                    writerLine = GetClassWriteLine(item,path);

                    if(writerLine == default)
                        writerLine = GetStructWriteLine(item, path);
                }
            }

            return writerLine ?? ""; //debug only
        }

        private static string GetStructWriteLine(ISymbol item, string path)
        {
            var type = item.GetTypeSymbol();

            if (!type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();

            var members = type.GetMembers();

            foreach (var member in members)
            {
                AddTypeMemberWriteLine(member, cb, string.Join(".", path, member.Name));
            }

            return cb.ToString();
        }

        private static string GetClassWriteLine(ISymbol item, string path)
        {
            var type = item.GetTypeSymbol();

            if (type.IsValueType)
                return default;

            CodeBuilder cb = new CodeBuilder();


            cb.AppendLine($"__packet.{nameof(OutputPacketBuffer.WriteNullableClass)}({path}, ()=> {{");

            cb.NextTab();

            cb.AppendLine();

            var members = type.GetMembers();


            foreach (var member in members)
            {
                AddTypeMemberWriteLine(member, cb, string.Join(".", path, member.Name));
            }

            cb.PrevTab();

            cb.AppendLine("});");


            return cb.ToString();
        }

        private static void AddTypeMemberWriteLine(ISymbol member, CodeBuilder cb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();
                    cb.AppendLine(BuildParameterWriter(ptype, path));

                    cb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();
                cb.AppendLine(BuildParameterWriter(ftype, path));
                cb.AppendLine();
            }
        }

        private static string GetBaseTypeWriteLine(ISymbol item, string path)
        {
            var type = item.GetTypeSymbol();

            if (writeTypeHandlers.TryGetValue(type.Name, out var baseValueWriter))
                return $"__packet.{baseValueWriter}({path});";

            return default;
        }

        private static string GetNullableTypeWriteLine(ISymbol parameter, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            var genericType = ((INamedTypeSymbol)type).TypeArguments.First();

            if (!genericType.IsValueType)
                return default;

            return $"__packet.{nameof(OutputPacketBuffer.WriteNullable)}({path},()=>{{ {BuildParameterWriter(genericType, $"{path}.Value")} }});";
        }



        private static Dictionary<string, string> writeTypeHandlers = new Dictionary<string, string>()
        {
            {
                typeof(bool).Name,
                nameof(OutputPacketBuffer.WriteBool)
            },
            {
                typeof(byte).Name,
                nameof(OutputPacketBuffer.WriteByte)
            },
            {
                typeof(short).Name,
                nameof(OutputPacketBuffer.WriteInt16)
            },
            {
                typeof(int).Name,
                nameof(OutputPacketBuffer.WriteInt32)
            },
            {
                typeof(long).Name,
                nameof(OutputPacketBuffer.WriteInt64)
            },
            {
                typeof(ushort).Name,
                nameof(OutputPacketBuffer.WriteUInt16)
            },
            {
                typeof(uint).Name,
                nameof(OutputPacketBuffer.WriteUInt32)
            },
            {
                typeof(ulong).Name,
                nameof(OutputPacketBuffer.WriteUInt64)
            },
            {
                typeof(float).Name,
                nameof(OutputPacketBuffer.WriteFloat)
            },
            {
                typeof(double).Name,
                nameof(OutputPacketBuffer.WriteDouble)
            },
            {
                typeof(string).Name,
                nameof(OutputPacketBuffer.WriteString32)
            },
            {
                typeof(DateTime).Name,
                nameof(OutputPacketBuffer.WriteDateTime)
            },
            {
                typeof(TimeSpan).Name,
                nameof(OutputPacketBuffer.WriteTimeSpan)
            },
        };
    }
}
