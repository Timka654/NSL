using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Linq;
using NSL.Generators.Utils;
using NSL.Generators.BinaryGenerator.Generators;

namespace NSL.Generators.BinaryGenerator
{
    internal delegate string CustomTypeHandle(INamedTypeSymbol type, BinaryGeneratorContext context, string path);
    internal delegate string GenerateHandle(ISymbol type, BinaryGeneratorContext context, string path);

    public class BinaryReadMethodsGenerator
    {
        private static List<GenerateHandle> generators = new List<GenerateHandle>();

        static BinaryReadMethodsGenerator()
        {
            //generators.Add(TaskTypeGenerator.GetReadLine);
            generators.Add(CustomTypeGenerator.GetReadLine);
            generators.Add(ArrayTypeGenerator.GetReadLine);
            generators.Add(EnumTypeGenerator.GetReadLine);
            generators.Add(BaseTypeGenerator.GetReadLine);
            generators.Add(NullableTypeGenerator.GetReadLine);
            generators.Add(ClassTypeGenerator.GetReadLine);
            generators.Add(StructTypeGenerator.GetReadLine);
        }

        //public static string GetValueReadSegment(ISymbol parameter, BinaryGeneratorContext context, string path, IEnumerable<string> ignoreMembers = null)
        public static string GetValueReadSegment(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            string valueReader = default;

            valueReader = context.GetExistsReadHandleCode(parameter, path);

            if (valueReader == default)
            {
                foreach (var gen in generators)
                {
                    valueReader = gen(parameter, context, path);

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

        public static void AddTypeMemberReadLine(ISymbol member, BinaryGeneratorContext context, CodeBuilder rb, string path)
        {
            if (member.DeclaredAccessibility.HasFlag(Accessibility.Public) == false || member.IsStatic)
                return;

            if (context.IsIgnore(member, path))
                return;

            if (member is IPropertySymbol ps)
            {
                if (ps.SetMethod != null)
                {
                    var ptype = ps.GetTypeSymbol();

                    rb.AppendLine($"{path} = {GetValueReadSegment(ptype, context, path)};");

                    rb.AppendLine();
                }
            }
            else if (member is IFieldSymbol fs)
            {
                var ftype = fs.GetTypeSymbol();

                rb.AppendLine($"{path} = {GetValueReadSegment(ftype, context, path)};");

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
