using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.HttpEndPointGenerator.Utils
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void ShowHttpEndPointDiagnostics(this GeneratorExecutionContext genContext, string code, string message, DiagnosticSeverity diagnosticSeverity, params Location[] locations)
        {
            var l = locations.First();

            genContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(code, message, message, "NSLHTTPEP", diagnosticSeverity, true), l, locations.Skip(1)));
        }
    }
}
