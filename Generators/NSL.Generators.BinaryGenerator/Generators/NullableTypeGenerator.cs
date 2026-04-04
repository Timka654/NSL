using Microsoft.CodeAnalysis;
using NSL.Generators.Utils;
using NSL.SocketCore.Utils.Buffer;
using System.Linq;

namespace NSL.Generators.BinaryGenerator.Generators
{
    internal class NullableTypeGenerator
    {
        public static string GetReadLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            CodeBuilder rb = new CodeBuilder();

            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            //GenDebug.Break();

            var typedArgs = ((INamedTypeSymbol)type).TypeArguments;

            var genericType = typedArgs.FirstOrDefault();

            if (genericType == null || !genericType.IsValueType)
                return default;

            const string readLambdaParam = "__nb";
            var outerIOPath = context.IOPath;
            context.IOPath = readLambdaParam;
            var readSegment = BinaryReadMethodsGenerator.GetValueReadSegment(genericType, context, path);
            context.IOPath = outerIOPath;

            return $"{outerIOPath}.{nameof(InputPacketBuffer.ReadNullable)}(static {readLambdaParam} => {{ return {readSegment}; }})";
        }

        public static string GetWriteLine(ISymbol parameter, BinaryGeneratorContext context, string path)
        {
            var type = parameter.GetTypeSymbol();

            if (!type.NullableAnnotation.Equals(NullableAnnotation.Annotated))
                return default;

            var typedArgs = ((INamedTypeSymbol)type).TypeArguments;

            var genericType = typedArgs.FirstOrDefault();

            if (genericType == null || !genericType.IsValueType)
                return default;

            const string writeLambdaBuf = "__nb";
            const string writeLambdaVal = "__nv";
            var outerIOPath = context.IOPath;
            context.IOPath = writeLambdaBuf;
            var writeSegment = BinaryWriteMethodsGenerator.BuildParameterWriter(genericType, context, writeLambdaVal);
            context.IOPath = outerIOPath;

            return $"{outerIOPath}.{nameof(OutputPacketBuffer.WriteNullable)}({path}, static ({writeLambdaBuf}, {writeLambdaVal}) => {{ {writeSegment} }});";
        }
    }
}
