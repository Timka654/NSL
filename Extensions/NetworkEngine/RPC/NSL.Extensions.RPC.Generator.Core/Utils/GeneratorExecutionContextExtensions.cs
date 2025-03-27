using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Extensions.RPC.Generator.Utils
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void ShowRPCGenDiagnostics(this SourceProductionContext genContext, string code, string message, DiagnosticSeverity diagnosticSeverity, params Location[] locations)
        {
            var l = locations.First();

            genContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(code, message, message, "NSLRPC", diagnosticSeverity, true), l, locations.Skip(1)));
        }
    }
}
