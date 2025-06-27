using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.FastAction.Core
{
    internal class ExecutionActionContext
    {
        public Solution Solution { get; private set; }

        public Project SourceProject { get; private set; }
        public Project SharedProject { get; private set; }

        public Document SourceDocument { get; private set; }

        public SyntaxNode SourceSyntax { get; private set; }
        public SyntaxAnnotation SourceSyntaxAnnotation { get; private set; }

        public CompilationUnitSyntax Root { get; private set; }
        public SemanticModel SemanticModel { get; private set; }
        public SyntaxEditor SourceSyntaxEditor { get; private set; }

        ProjectId sharedProjectId;

        public async Task WithSourceDocumentAsync(Document newDoc, CancellationToken cancellationToken)
        {
            SourceDocument = newDoc;
            Solution = newDoc.Project.Solution;
            await UpdateProjectsAsync(Solution, cancellationToken);
            //await RefreshAsync(cancellationToken);
        }

        public void SetSharedProjectId(ProjectId id)
        {
            sharedProjectId = id;
        }

        public async Task SetSourceSyntaxAsync(SyntaxNode node, CancellationToken cancellationToken)
        {
            SourceSyntaxAnnotation = new SyntaxAnnotation();
            SourceSyntax = node.WithAdditionalAnnotations(SourceSyntaxAnnotation);
            var n = Root.ReplaceNode(node, SourceSyntax);
            await UpdateProjectsAsync(SourceDocument.WithSyntaxRoot(n).Project.Solution, cancellationToken);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            Root = (CompilationUnitSyntax)await SourceDocument.GetSyntaxRootAsync(cancellationToken);
            SemanticModel = await SourceDocument.GetSemanticModelAsync(cancellationToken);
            SourceSyntaxEditor = new SyntaxEditor(Root, SourceDocument.Project.Solution.Workspace);

            if (SourceSyntaxAnnotation != null)
                SourceSyntax = Root.GetAnnotatedNodes(SourceSyntaxAnnotation).FirstOrDefault();
        }

        public async Task UpdateProjectsAsync(Solution updatedSolution, CancellationToken cancellationToken)
        {
            Solution = updatedSolution;
            SourceProject = Solution.GetProject(SourceDocument.Id.ProjectId);
            SharedProject = Solution.GetProject(sharedProjectId);
            SourceDocument = updatedSolution.GetDocument(SourceDocument.Id);
            await RefreshAsync(cancellationToken);
        }

        public async Task<Document> AddFileToSharedAsync(
    string name,
    string content,
    string filePath,
    bool format = true,
    CancellationToken cancellationToken = default)
        {
            // Уже есть документ с этим путём?
            if (SharedProject.Documents.Any(x => x.FilePath == filePath))
                return null;

            var newDoc = SharedProject.AddDocument(name, content, filePath: filePath);

            if (format)
            {
                try
                {
                    newDoc = await Microsoft.CodeAnalysis.Formatting.Formatter.FormatAsync(
                        newDoc, newDoc.Project.Solution.Workspace.Options, cancellationToken);
                }
                catch { /* опционально логировать */ }
            }

            // Актуализируем Solution и SharedProject
            await UpdateProjectsAsync(newDoc.Project.Solution, cancellationToken);

            return newDoc;
        }

        public async Task AddUsingDirectiveAsync(string namespaceName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                return;

            var root = await SourceDocument.GetSyntaxRootAsync(cancellationToken);

            if (!(root is CompilationUnitSyntax cus))
                return;

            if (cus.Usings.Any(u => u.Name.ToString() == namespaceName))
                return;

            var updated = cus.AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(namespaceName))
                             .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

            var newDoc = SourceDocument.WithSyntaxRoot(updated);
            await UpdateProjectsAsync(newDoc.Project.Solution, cancellationToken);
            await WithSourceDocumentAsync(newDoc, cancellationToken);
        }

        public async Task AddProjectReferenceIfMissingAsync(CancellationToken cancellationToken)
        {
            if (SourceProject.Id == SharedProject.Id)
                return;

            if (SourceProject.ProjectReferences.Any(r => r.ProjectId == SharedProject.Id))
                return;

            var updated = SourceProject.AddProjectReference(new ProjectReference(SharedProject.Id));

            await UpdateProjectsAsync(updated.Solution, cancellationToken);
        }

        public async Task InsertTemplateCodeAsync(
            TemplateData template,
            Dictionary<string, string> valuesCollection,
            AppendCodeTypeEnum type,
            CancellationToken cancellationToken = default)
        {
            var forNode = SourceSyntax;

            // Поднимаемся на указанную глубину
            for (int i = 0; i < template.ParentDepth && forNode?.Parent != null; i++)
                forNode = forNode.Parent;

            if (forNode == null)
                throw new InvalidOperationException("Целевой узел вставки не найден");

            var editor = SourceSyntaxEditor;

            valuesCollection.TryGetValue("fa_repeat", out string repeatValue);

            var repeatCount = int.TryParse(repeatValue, out var count) ? count : 1;

            valuesCollection.Remove("fa_repeat_num");

            for (int r = 0; r < repeatCount; r++)
            {
                valuesCollection["fa_repeat_num"] = r.ToString();

                foreach (var file in template.Files)
                {
                    var fcontent = file.Content;

                    foreach (var v in valuesCollection)
                        fcontent = fcontent.Replace($"${v.Key}$", v.Value);

                    // Пытаемся вставить как Member
                    SyntaxNode insertContent = type == AppendCodeTypeEnum.BaseList ? (SyntaxNode)SyntaxFactory.ParseMemberDeclaration(fcontent)
                        : await SyntaxFactory.ParseSyntaxTree(fcontent).GetRootAsync(cancellationToken);

                    var nodes = insertContent.ChildNodes().ToList();

                    var insertAttributes = nodes.OfType<AttributeListSyntax>();
                    var isAttributes = insertAttributes.Count() == nodes.Count;

                    if (isAttributes)
                    {
                        editor.AddAttributes(forNode, insertAttributes);
                        continue;
                    }

                    switch (type)
                    {
                        case AppendCodeTypeEnum.OuterBefore:
                        case AppendCodeTypeEnum.Replace:
                            editor.InsertBefore(forNode, nodes);
                            break;

                        case AppendCodeTypeEnum.OuterAfter:
                            editor.InsertAfter(forNode, nodes);
                            break;

                        case AppendCodeTypeEnum.InnerBefore:
                            editor.InsertMembers(forNode, 0, nodes);
                            break;

                        case AppendCodeTypeEnum.InnerAfter:
                            editor.InsertMembers(forNode, forNode.ChildNodes().Count(), nodes);
                            break;

                        case AppendCodeTypeEnum.BaseList:
                            if (forNode is TypeDeclarationSyntax t)
                            {
                                var existingBaseTypes = t.BaseList?.Types.Select(x => x.Type.ToString()).ToArray() ?? Array.Empty<string>();
                                foreach (var n in nodes)
                                {
                                    if (!existingBaseTypes.Contains(n.ToString()))
                                        editor.AddBaseType(t, n);
                                }
                            }
                            break;
                    }
                }
            }

            if (type == AppendCodeTypeEnum.Replace)
                editor.RemoveNode(forNode);

            // Применяем изменения
            Root = (CompilationUnitSyntax)editor.GetChangedRoot();

            // Добавляем using-ы из шаблона
            if (template.Usings.Any())
            {
                var currentUsings = Root.Usings.Select(x => x.Name.ToString()).ToArray();
                var newUsings = template.Usings
                    .Where(u => !currentUsings.Contains(u))
                    .Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))
                        .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));

                Root = Root.AddUsings(newUsings.ToArray());
            }

            var newDoc = SourceDocument.WithSyntaxRoot(Root);

            try
            {
                newDoc = await Microsoft.CodeAnalysis.Formatting.Formatter.FormatAsync(newDoc, newDoc.Project.Solution.Workspace.Options, cancellationToken);
            }
            catch { }

            await WithSourceDocumentAsync(newDoc, cancellationToken);
        }



    }
}
