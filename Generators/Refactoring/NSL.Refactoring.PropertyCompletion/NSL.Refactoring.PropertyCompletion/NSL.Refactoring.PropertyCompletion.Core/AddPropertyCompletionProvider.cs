using System;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace NSL.Refactoring.PropertyCompletion.Core
{
    [ExportCompletionProvider(nameof(AddPropertyCompletionProvider), LanguageNames.CSharp)]
    [Shared]
    public class AddPropertyCompletionProvider : CompletionProvider
    {
        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var document = context.Document;
            var position = context.Position;
            var cancellationToken = context.CancellationToken;
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var token = root.FindToken(position);
            var node = token.Parent;

            // Проверяем, что вводится идентификатор внутри класса
            if (node == null || !(node is IdentifierNameSyntax)) return;
            if (!IsInsideClass(node)) return;

            var identifierText = token.ValueText; // Получаем вводимый текст

            var properties = ImmutableDictionary<string, string>.Empty
                .Add("InsertText", identifierText);

            // Создаем `CompletionItem`
            var completionItem = CompletionItem.Create(
                displayText: $"Создать свойство",
                filterText: identifierText,  // Фильтрация по текущему тексту
                sortText: $"000_CreateProperty", // Делаем сортировку приоритетной
                properties: properties, // Добавляем `InsertText`
                rules: CompletionItemRules.Default.WithMatchPriority(MatchPriority.Preselect) // Высокий приоритет
                    .WithSelectionBehavior(CompletionItemSelectionBehavior.HardSelection)
            );

            context.AddItem(completionItem);// Принудительно обновляем автодополнение после каждого ввода символа

        }

        public override async Task<CompletionChange> GetChangeAsync(Microsoft.CodeAnalysis.Document document, CompletionItem item, char? commitKey, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            var position = item.Span.Start;
            var token = root?.FindToken(position);

            if (token == null) return CompletionChange.Create(new TextChange());

            var propertyName = token.Value;
            var propertyCode = $"public {await PredictPropertyTypeAsync(propertyName.ValueText)} {propertyName} {{ get; set; }}\n";

            return CompletionChange.Create(new TextChange(new TextSpan(position, propertyName.Text.Length), propertyCode));
        }

        private static bool IsInsideClass(SyntaxNode node)
        {
            return node.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().Any();
        }

        private async Task<string> PredictPropertyTypeAsync(string propertyName)
        {
            return "object";
            //// Проверяем, существует ли модель
            //if (!File.Exists("PropertyTypeModel.zip"))
            //    return "int"; // Если нет модели, возвращаем тип по умолчанию

            //return PropertyPrediction.PredictType(propertyName);
        }
    }
}