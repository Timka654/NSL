using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Generators.FillTypeGenerator.Utils
{
    public static class GeneratorExecutionContextExtensions
    {
        public static void ShowFillTypeDiagnostics(this GeneratorExecutionContext genContext, string code, string message, DiagnosticSeverity diagnosticSeverity, params Location[] locations)
        {
            var l = locations.First();

            genContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(code, message, message, "NSLFT", diagnosticSeverity, true), l, locations.Skip(1)));
        }
    }
}
