using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.SelectGenerator.Utils
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void ShowSelectDiagnostics(this SourceProductionContext genContext, string code, string message, DiagnosticSeverity diagnosticSeverity, params Location[] locations)
        {
            var l = locations.First();

            genContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(code, message, message, "NSLSELECT", diagnosticSeverity, true), l, locations.Skip(1)));
        }
    }
}
