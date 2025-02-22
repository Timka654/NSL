using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NSL.Refactoring.PropertyCompletion.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Shared]
    public class PropertyTypeCollectorAnalyzer : DiagnosticAnalyzer
    {
        private string CsvFilePath = "PropertyDataset.csv";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray<DiagnosticDescriptor>.Empty;

        public override void Initialize(AnalysisContext context)
        {
            Debug.WriteLine("🔥 PropertyTypeCollectorAnalyzer: Initialized");
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
        }

        private void AnalyzeProperty(SyntaxNodeAnalysisContext context)
        {
            var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            // Получаем имя и тип свойства
            string propertyName = propertyDeclaration.Identifier.Text;
            string propertyType = propertyDeclaration.Type.ToString();

            // Проверяем, существует ли файл, если нет — создаем заголовок
            if (!File.Exists(CsvFilePath))
            {
                File.WriteAllText(CsvFilePath, "PropertyName,PropertyType\n");
            }

            // Добавляем данные в CSV
            File.AppendAllText(CsvFilePath, $"{propertyName},{propertyType}\n");
        }
    }
}