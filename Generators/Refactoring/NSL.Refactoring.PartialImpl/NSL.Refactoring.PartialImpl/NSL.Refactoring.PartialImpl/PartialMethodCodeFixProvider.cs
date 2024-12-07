using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.CodeAnalysis.CodeRefactorings;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Formatting;
using NSL.Generators.Utils;

namespace NSL.Refactoring.PartialImpl
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(PartialMethodsRefactoringProvider))]
    [Shared]
    public class PartialMethodsRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;
            //GenDebug.Break();
            // Найти класс, под курсором
            var node = root.FindNode(context.Span);
            var classDeclaration = node.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration == null) return;

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
            if (semanticModel == null) return;

            // Получить символ класса
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
            if (classSymbol == null || !classSymbol.IsPartial()) return;

            // Собрать все методы во всех partial частях
            var allPartialDeclarations = classSymbol.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax(context.CancellationToken))
                .OfType<ClassDeclarationSyntax>();

            // Найти все partial методы без реализации
            var partialMethods = GetUnimplementedPartialMethods(allPartialDeclarations);

            if (partialMethods.Count == 0) return;

            // Добавить действие в контекстное меню
            context.RegisterRefactoring(
                CodeAction.Create(
                    title: "Generate implementations for partial methods",
                    createChangedDocument: c => ImplementPartialMethodsAsync(document, classDeclaration, partialMethods, c),
                    equivalenceKey: "GeneratePartialMethods"));
        }

        private async Task<Document> ImplementPartialMethodsAsync(Document document, ClassDeclarationSyntax classDeclaration, List<MethodDeclarationSyntax> partialMethods, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            // Создать реализации для каждого метода
            var newMethods = partialMethods.Select(method =>
            {
                var generatedBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("throw new NotImplementedException();"));

                return method
                    .WithBody(generatedBody)
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.None))
                    .WithAdditionalAnnotations(Formatter.Annotation); // Для автоматического форматирования
            }).ToArray();

            // Добавить методы в класс
            var updatedClass = classDeclaration.AddMembers(newMethods);

            // Обновить дерево синтаксиса
            var newRoot = root.ReplaceNode(classDeclaration, updatedClass);
            return document.WithSyntaxRoot(newRoot);
        }

        private List<MethodDeclarationSyntax> GetUnimplementedPartialMethods(IEnumerable<ClassDeclarationSyntax> partialDeclarations)
        {
            // Собираем все partial методы по сигнатурам
            var allPartialMethods = partialDeclarations
                .SelectMany(c => c.Members.OfType<MethodDeclarationSyntax>())
                .Where(m => m.Modifiers.Any(SyntaxKind.PartialKeyword))
                .ToList();

            // Оставляем только те методы, которые не реализованы
            var unimplementedMethods = allPartialMethods
                .Where(method =>
                {
                    // Проверяем, реализован ли этот метод хотя бы в одной части
                    return !allPartialMethods.Any(otherMethod =>
                        method.AreMethodsEquivalent(otherMethod) && otherMethod.IsMethodImplemented());
                })
                .Distinct()
                .ToList();

            return unimplementedMethods;
        }
    }

    internal static class Utils
    {
        public static bool IsPartial(this ISymbol symbol)
        {
            if (symbol == null)
                return false;

            // Проверяем модификаторы на наличие partial
            return symbol.DeclaringSyntaxReferences
                .Select(syntaxRef => syntaxRef.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .Any(typeSyntax => typeSyntax.Modifiers.Any(SyntaxKind.PartialKeyword));
        }

        public static bool IsMethodImplemented(this MethodDeclarationSyntax method)
        {
            // Метод реализован, если у него есть тело или стрелочное выражение
            return method.Body != null || method.ExpressionBody != null;
        }
        public static bool AreMethodsEquivalent(this MethodDeclarationSyntax method1, MethodDeclarationSyntax method2)
        {
            // Сравниваем имена
            if (method1.Identifier.Text != method2.Identifier.Text)
                return false;

            // Сравниваем возвращаемые типы
            if (!method1.ReturnType.IsEquivalentTo(method2.ReturnType))
                return false;

            // Сравниваем параметры
            var parameters1 = method1.ParameterList.Parameters;
            var parameters2 = method2.ParameterList.Parameters;
            if (parameters1.Count != parameters2.Count)
                return false;

            for (int i = 0; i < parameters1.Count; i++)
            {
                if (!parameters1[i].Type.IsEquivalentTo(parameters2[i].Type))
                    return false;
            }

            // Сравниваем модификаторы
            var modifiers1 = new HashSet<SyntaxKind>(method1.Modifiers.Select(m => m.Kind()));
            var modifiers2 = new HashSet<SyntaxKind>(method2.Modifiers.Select(m => m.Kind()));

            return modifiers1.SetEquals(modifiers2);
        }

    }
}