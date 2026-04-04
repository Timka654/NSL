using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.RpcGenerator.Core.Utils
{
    internal static class GeneratorContextExtensions
    {
        public static void ShowRpcDiagnostics(this SourceProductionContext context, string code, string message, DiagnosticSeverity severity, params Location[] locations)
        {
            var location = locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor(code, message, message, "NSLRpc", severity, true),
                location,
                locations.Skip(1)));
        }
    }
}
