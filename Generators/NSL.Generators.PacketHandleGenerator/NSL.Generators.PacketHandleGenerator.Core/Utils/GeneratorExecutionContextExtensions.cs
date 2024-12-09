using Microsoft.CodeAnalysis;
using System.Linq;

namespace NSL.Generators.PacketHandleGenerator.Utils
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void ShowPHDiagnostics(this GeneratorExecutionContext genContext, string code, string message, DiagnosticSeverity diagnosticSeverity, params Location[] locations)
        {
            var l = locations.First();

            genContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(code, message, message, "NSLPH", diagnosticSeverity, true), l, locations.Skip(1)));
        }
    }
}
