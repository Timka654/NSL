using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Text;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NSL.Refactoring.FastAction.Core
{
    internal class FastActionBuilder
    {
        const string settingsFileName = "FastActionOptions.json";

        const string templatesUrl = "https://pubstorage.mtvworld.net/templates/fa/";

        static DateTime latestBaseCache = default;

        static string baseCachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "mr_mtv",
        "nsl_fa",
        "RoslynTemplatesCache");

        static List<string> baseCache = new List<string>();

        static async Task<List<string>> GetBasePathes()
        {
            if (latestBaseCache < DateTime.UtcNow.AddMinutes(-5))
            {
                Directory.CreateDirectory(baseCachePath);

                var versionPath = Path.Combine(baseCachePath, "version.json");


                latestBaseCache = DateTime.UtcNow;

                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(4);

                    try
                    {
                        var response = await httpClient.GetAsync(templatesUrl + "version.json").ConfigureAwait(false);

                        var remoteVersionContent = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : default;

                        if (remoteVersionContent != default)
                        {
                            var remoteVersion = string.Empty;
                            var localVersion = string.Empty;

                            if ((JsonNode.Parse(remoteVersionContent) as JsonObject).TryGetPropertyValue("version", out var remoteVersionJ))
                                remoteVersion = remoteVersionJ.GetValue<string>();

                            if (File.Exists(versionPath))
                            {
                                if ((JsonNode.Parse(File.ReadAllText(versionPath)) as JsonObject).TryGetPropertyValue("version", out var localVersionJ))
                                    localVersion = localVersionJ.GetValue<string>();
                            }

                            if (remoteVersion != localVersion)
                            {
                                response = await httpClient.GetAsync(templatesUrl + "templates.zip").ConfigureAwait(false);

                                if (response.IsSuccessStatusCode)
                                {
                                    using (var s = await response.Content.ReadAsStreamAsync())
                                    {
                                        using (var za = new ZipArchive(s))
                                        {
                                            za.ExtractToDirectory(baseCachePath);
                                        }
                                    }

                                    File.WriteAllText(versionPath, remoteVersionContent);
                                }
                            }
                        }
                    }
                    catch (HttpRequestException)
                    {

                    }
                }

                var epath = Path.Combine(baseCachePath, settingsFileName);

                if (File.Exists(epath))
                    baseCache = new List<string>() { epath };

            }

            return baseCache;
        }

        internal static async Task<IEnumerable<RefactoringAction>> BuildActions(Document document, TextSpan span, CancellationToken cancellationToken)
        {
            var pid = document.Project.Id.Id;


            if (!cache.TryGetValue(pid, out var actionData) || actionData.Data.PreventCache)
            {
                var projectPath = Path.GetDirectoryName(document.Project.FilePath);
                var solutionPath = Path.GetDirectoryName(document.Project.Solution.FilePath);

                List<string> pathes = new List<string>();

                string path = default;

                pathes.AddRange(await GetBasePathes().ConfigureAwait(false));


                path = Path.Combine(solutionPath, settingsFileName);

                if (File.Exists(path))
                    pathes.Add(path);

                path = Path.Combine(projectPath, settingsFileName);

                if (File.Exists(path))
                    pathes.Add(path);

                if (!pathes.Any())
                    return Array.Empty<RefactoringAction>();

                try
                {
                    var actionsContent = JsonIncrementalMerger.MergeJsonFiles(pathes);

                    actionData = new FastActionCacheData
                    {
                        SolutionPath = solutionPath,
                        ProjectPath = projectPath,
                        Data = actionsContent.Deserialize<FastActionData>(new JsonSerializerOptions()
                        {
                            ReadCommentHandling = JsonCommentHandling.Skip,
                            AllowTrailingCommas = true
                        })
                    };

                    cache[pid] = actionData;

                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            if (actionData == null)
                return Array.Empty<RefactoringAction>();


            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var node = root.FindNode(span);

            if (node == null)
                return Array.Empty<RefactoringAction>();

            int getNodeDepth()
            {
                if (node is IdentifierNameSyntax)
                    return 1;
                if (node is PredefinedTypeSyntax)
                    return 1;
                if (node is AccessorListSyntax)
                    return 1;
                if (node is AccessorDeclarationSyntax)
                    return 2;

                return 0;
            }

            TValue getTypedNode<TValue>(int depth)
                where TValue : SyntaxNode
            {
                var n = node;
                for (int i = 0; i < depth; i++)
                {
                    if (n is TValue)
                        return n as TValue;

                    n = n.Parent;
                }

                return n as TValue;
            }

            var typeDecl = getTypedNode<TypeDeclarationSyntax>(getNodeDepth());

            var classDecl = typeDecl as ClassDeclarationSyntax;

            var structDecl = typeDecl as StructDeclarationSyntax;


            var enumDecl = getTypedNode<EnumDeclarationSyntax>(getNodeDepth());

            var enumMemberDecl = getTypedNode<EnumMemberDeclarationSyntax>(getNodeDepth());

            var namespaceDecl = getTypedNode<NamespaceDeclarationSyntax>(getNodeDepth());


            var attributeNode = getTypedNode<AttributeSyntax>(getNodeDepth());

            var memberDecl = getTypedNode<MemberDeclarationSyntax>(getNodeDepth());

            var propertyDecl = memberDecl as PropertyDeclarationSyntax;

            var fieldDecl = memberDecl as FieldDeclarationSyntax;

            var methodDecl = memberDecl as MethodDeclarationSyntax;

            var baseType = node as SimpleBaseTypeSyntax;

            var nodeName = baseType?.ToString()
                ?? typeDecl?.GetClassName()
                ?? namespaceDecl?.Name.ToString()
                ?? enumDecl?.Identifier.ToString()
                ?? attributeNode?.ToString()
                ?? node.ToString();


            var actions = new List<RefactoringAction>();

            foreach (var _group in actionData.Data.Groups)
            {
                var group = _group;

                if (group.Disabled)
                    continue;

                if (group.ProjectPath == null)
                {
                    group.Project = document.Project.Solution.Projects.FirstOrDefault(x => x.Name.Equals(group.ProjectName));

                    if (group.Project == null)
                        continue;

                    group.ProjectPath = Path.GetDirectoryName(group.Project.FilePath);
                }

                foreach (var _action in actionData.Data.Actions)
                {
                    var action = _action;

                    if (group.Actions != null)
                    {
                        if (!group.IsMatchAction(action.Id))
                            continue;
                    }

                    var typeAllow = action.Types == null || action.Types.Contains("*");


                    typeAllow = typeAllow || action.Types.Contains("namespace_decl") && namespaceDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("type_decl") && typeDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("class_decl") && classDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("struct_decl") && structDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("method_decl") && methodDecl != null;


                    typeAllow = typeAllow || action.Types.Contains("enum_member") && enumMemberDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("enum_decl") && enumDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("member_decl") && memberDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("property_decl") && propertyDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("field_decl") && fieldDecl != null;

                    typeAllow = typeAllow || action.Types.Contains("attribute") && attributeNode != null;

                    typeAllow = typeAllow || action.Types.Contains("base_type") && baseType != null;

                    if (!typeAllow) continue;

                    var matches = new Dictionary<string, string>();

                    bool validateConditions()
                    {
                        foreach (var condition in action.Conditions)
                        {
                            if (condition.Namespace != default)
                            {
                                var ns = node.GetParentNamespace()?.Name.ToString();

                                Dictionary<string, string> nnmatch = null;

                                if (ns == default || !condition.IsMatch(condition.Namespace, ns, out nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }

                            if (condition.PropertyDeclaration != default)
                            {
                                Dictionary<string, string> nnmatch = null;

                                if (propertyDecl == default || !condition.IsMatch(condition.PropertyDeclaration, propertyDecl.ToString(), out nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }

                            if (condition.FieldDeclaration != default)
                            {
                                Dictionary<string, string> nnmatch = null;

                                if (propertyDecl == default || !condition.IsMatch(condition.FieldDeclaration, fieldDecl.ToString(), out nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }

                            if (condition.FilePath != default)
                            {
                                if (!condition.IsMatch(condition.FilePath, document.FilePath, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            if (condition.NodeName != default)
                            {
                                if (!condition.IsMatch(condition.NodeName, nodeName, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            if (condition.ProjectPath != default)
                            {
                                if (!condition.IsMatch(condition.ProjectPath, document.Project.FilePath, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            if (condition.ProjectName != default)
                            {
                                if (!condition.IsMatch(condition.ProjectName, document.Project.Name, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            if (condition.SolutionPath != default)
                            {
                                if (!condition.IsMatch(condition.SolutionPath, document.Project.Solution.FilePath, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            if (condition.SolutionName != default)
                            {
                                var name = Path.GetFileName(document.Project.Solution.FilePath);
                                name = name.Substring(0, name.LastIndexOf('.'));
                                if (!condition.IsMatch(condition.SolutionName, name, out var nnmatch))
                                {
                                    if (!condition.Optional)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }
                        }

                        return true;
                    }

                    if (!validateConditions())
                        continue;


                    var actionValuesCollection = new Dictionary<string, string>(action.Values);


                    foreach (var match in matches)
                    {
                        foreach (var item in actionValuesCollection.ToArray())
                        {
                            actionValuesCollection[item.Key] = actionValuesCollection[item.Key].Replace($"${match.Key}$", match.Value);
                        }
                    }

                    var outputDirPath = group.ProjectPath;

                    if (action.OutputRelativePath != default)
                        outputDirPath = Path.Combine(outputDirPath, action.OutputRelativePath);

                    actionValuesCollection["group_name"] = group.Name;

                    string summary = string.Empty;

                    if (action.GenerateSummaryRefLink && document.TryGetSyntaxTree(out var tree))
                    {
                        var documentRoot = await tree.GetRootAsync();

                        var tsymb = (await document.GetSemanticModelAsync()).GetDeclaredSymbol(node) as ITypeSymbol;

                        if (tsymb != null)
                            summary = $"Generate for <see cref=\"{tsymb.GetTypeSeeCRef()}\"/>";
                    }

                    actionValuesCollection["gen_summary_link"] = summary;



                    var outputTemplates = new List<TemplateOutputData>();

                    foreach (var _template in action.Template)
                    {
                        var template = _template;

                        if (template.Files == default)
                        {
                            var folderPath = Path.Combine(actionData.SolutionPath, "NSLFATemplates", template.Name);

                            var tfiles = new List<TemplateFileData>();

                            if (Directory.Exists(folderPath))
                                template.Files = Directory.GetFiles(folderPath).Select(item => new TemplateFileData()
                                {
                                    Name = Path.GetFileName(item),
                                    Content = File.ReadAllText(item)

                                }).ToArray();
                            else
                            {
                                var baseFolderPath = Path.Combine(baseCachePath, "NSLFATemplates", template.Name);

                                if (!Directory.Exists(baseFolderPath))
                                    throw new DirectoryNotFoundException($"Template folder not found: {baseFolderPath} or {folderPath}");

                                template.Files = Directory.GetFiles(baseFolderPath).Select(item => new TemplateFileData()
                                {
                                    Name = Path.GetFileName(item),
                                    Content = File.ReadAllText(item)
                                }).ToArray();

                            }
                        }

                        var templateValues = FillMap(new Dictionary<string, string>(template.Values), actionValuesCollection);

                        var templateOutputPath = outputDirPath;

                        if (templateValues.TryGetValue("name", out var outputName))
                            templateValues["safeitemname"] = outputName;

                        if (action.OutputType.StartsWith("files"))
                        {

                            if (templateValues.TryGetValue("name", out outputName))
                            {
                                outputName += "s";

                                string getNextPath(string path)
                                {
                                    var di = new DirectoryInfo(path);
                                    foreach (var item in di.GetDirectories().OrderByDescending(x => x.Name.Length))
                                    {
                                        if (outputName.Contains(item.Name))
                                            return getNextPath(item.FullName);
                                    }

                                    return path;
                                }

                                templateOutputPath = getNextPath(templateOutputPath);
                            }

                            if (!action.OutputType.Contains("_merge"))
                            {
                                bool anyUnavailableFile = false;

                                foreach (var file in template.Files)
                                {
                                    var fname = file.Name;

                                    foreach (var v in templateValues)
                                    {
                                        fname = fname.Replace($"${v.Key}$", v.Value);
                                    }

                                    var endPath = Path.Combine(outputDirPath, fname);

                                    anyUnavailableFile = anyUnavailableFile || !File.Exists(endPath);
                                }

                                if (!anyUnavailableFile)
                                    continue;
                            }
                        }


                        var @namespace = group.Project.DefaultNamespace;

                        var relativeOutputPath = PathHelper.GetRelativePath(group.ProjectPath, templateOutputPath);

                        @namespace = string.Join(".", @namespace, relativeOutputPath
                            .Replace('\\', '.')
                            .Replace('/', '.'))
                            .TrimEnd('.');

                        while (@namespace.IndexOf("..") > -1)
                        {
                            @namespace = @namespace.Replace("..", ".");
                        }

                        var snamespace = @namespace.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < snamespace.Length; i++)
                        {
                            if (char.IsNumber(snamespace[i][0]))
                                snamespace[i] = $"_{snamespace[i]}";
                        }

                        @namespace = string.Join(".", snamespace);

                        templateValues.TryAdd("namespace", @namespace);
                        templateValues.TryAdd("rootnamespace", @namespace);


                        var appendCodeType = AppendCodeTypeEnum.Replace;

                        string outputType = template.OutputType ?? action.OutputType;

                        if (outputType == "files")
                        {
                            appendCodeType = AppendCodeTypeEnum.Files;
                        }
                        else if (outputType.StartsWith("code"))
                        {
                            if (outputType.Contains("_fragment"))
                            {
                                if (outputType.EndsWith("_replace"))
                                {
                                }

                                if (outputType.EndsWith("_outer_before"))
                                    appendCodeType = AppendCodeTypeEnum.OuterBefore;

                                if (outputType.EndsWith("_outer_after"))
                                    appendCodeType = AppendCodeTypeEnum.OuterAfter;

                                if (outputType.EndsWith("_inner_before"))
                                    appendCodeType = AppendCodeTypeEnum.InnerBefore;

                                if (outputType.EndsWith("_inner_after"))
                                    appendCodeType = AppendCodeTypeEnum.InnerAfter;

                                if (outputType.EndsWith("_base_list"))
                                    appendCodeType = AppendCodeTypeEnum.BaseList;
                            }
                        }
                        else
                        {
                            throw new Exception($"action: {action.Name}, template {template.Name} have invalid final output type \"{outputType}\"");
                        }


                        outputTemplates.Add(new TemplateOutputData()
                        {
                            NameSpace = @namespace,
                            OutputPath = templateOutputPath,
                            Template = template,
                            Values = templateValues,
                            AppendCodeType = appendCodeType
                        });
                    }

                    if (!outputTemplates.Any())
                        continue;

                    var actionName = action.Name;
                    var groupName = group.Name;

                    foreach (var v in actionValuesCollection)
                    {
                        actionName = actionName.Replace($"${v.Key}$", v.Value);
                        groupName = groupName.Replace($"${v.Key}$", v.Value);
                    }

                    actionValuesCollection["action_name"] = actionName;
                    actionValuesCollection["group_name"] = groupName;

                    var actionPath = action.ActionPath ?? "$group_name$\\\\\\$action_name$";

                    foreach (var v in actionValuesCollection)
                    {
                        actionPath = actionPath.Replace($"${v.Key}$", v.Value);
                    }


                    actions.Add(new RefactoringAction()
                    {
                        Action = async (cts, isPreview) =>
                        {
                            Solution cSol = default;

                            var solEditor = new SolutionEditor(document.Project.Solution);

                            var docEditor = await solEditor.GetDocumentEditorAsync(document.Id);

                            //

                            foreach (var item in outputTemplates)
                            {
                                if (item.AppendCodeType == AppendCodeTypeEnum.Files)
                                    cSol = await Files(node, group, action, item.Values, item.NameSpace, item.OutputPath, document, item.Template, cSol, cts);
                                else
                                    cSol = await CodeFragment(memberDecl ?? typeDecl ?? node, item.AppendCodeType, group, action, item.Values, item.NameSpace, item.OutputPath, document, item.Template, cSol, cts);
                            }

                            return cSol;
                        },
                        Path = actionPath
                    });
                }
            }

            return actions;
        }

        static async Task<Solution> Files(
            SyntaxNode forNode,
            GroupData groupData,
            ActionData actionData,
            Dictionary<string, string> valuesCollection,
            string ns,
            string outputPath,
            Document sourceDoc,
            TemplateData template,
            Solution solution,
            CancellationToken cancellationToken)
        {
            await LoadTemplateCloneValues(forNode, valuesCollection, sourceDoc, template, cancellationToken);

            valuesCollection = FillMap(valuesCollection, valuesCollection);

            Solution s = solution ?? groupData.Project.Solution;

            var sharedProj = s.GetProject(groupData.Project.Id);

            foreach (var file in template.Files)
            {
                var fname = file.Name;
                var fcontent = file.Content;

                foreach (var v in valuesCollection)
                {
                    fname = fname.Replace($"${v.Key}$", v.Value);
                    fcontent = fcontent.Replace($"${v.Key}$", v.Value);
                }

                var endPath = Path.Combine(outputPath, fname);

                if (sharedProj.Documents.Any(x => Equals(x.FilePath, endPath)))
                    continue;

                //srcContent = srcContent.Replace("$gen_summary$", summary);

                var doc = sharedProj.AddDocument(fname, fcontent, filePath: endPath);


                try
                {
                    doc = await Microsoft.CodeAnalysis.Formatting.Formatter.FormatAsync(doc, doc.Project.Solution.Workspace.Options, cancellationToken);

                }
                catch (Exception)
                {
                }

                sharedProj = doc.Project;

                s = sharedProj.Solution;
            }

            if (actionData.GenerateUsingRefLink)
            {
                //link

                var srcProj = s.GetProject(sourceDoc.Project.Id);

                if (!srcProj.ProjectReferences.Any(x => x.ProjectId == sharedProj.Id)
                    && !srcProj.Id.Equals(sharedProj.Id)
                    && !sharedProj.AllProjectReferences.Any(x => x.ProjectId.Equals(srcProj.Id)))
                    srcProj = srcProj.AddProjectReference(new ProjectReference(sharedProj.Id));

                sourceDoc = srcProj.GetDocument(sourceDoc.Id);

                var synRoot = await sourceDoc.GetSyntaxRootAsync(cancellationToken);

                if (synRoot is CompilationUnitSyntax cus)
                {
                    if (cus.Members.FirstOrDefault() is NamespaceDeclarationSyntax cns && cns.Name.ToString() == ns)
                    {

                    }
                    else if (!cus.Usings.Any(x => Equals(x.Name.ToString(), ns)))
                    {
                        var us = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));

                        cus = cus.AddUsings(us);

                        srcProj = sourceDoc.WithSyntaxRoot(cus).Project;
                    }
                }

                s = srcProj.Solution;
            }

            return s;
        }

        static async Task<Solution> CodeFragment(
            SyntaxNode forNode,
            AppendCodeTypeEnum type,
            GroupData groupData,
            ActionData actionData,
            Dictionary<string, string> valuesCollection,
            string ns,
            string outputPath,
            Document sourceDoc,
            TemplateData template,
            Solution solution,
            CancellationToken cancellationToken)
        {
            await LoadTemplateCloneValues(forNode, valuesCollection, sourceDoc, template, cancellationToken);

            valuesCollection = FillMap(valuesCollection, valuesCollection);

            Solution s = solution ?? groupData.Project.Solution;

            var p = s.GetProject(sourceDoc.Project.Id);

            sourceDoc = p.GetDocument(sourceDoc.Id);


            var sharedProj = s.GetProject(groupData.Project.Id);

            var root = ((CompilationUnitSyntax)await sourceDoc.GetSyntaxRootAsync(cancellationToken));


            for (int i = 0; i < template.ParentDepth; i++)
            {
                forNode = forNode.Parent;
            }

            var editor = new SyntaxEditor(root, sourceDoc.Project.Solution.Workspace.Services);

            foreach (var file in template.Files)
            {
                var fcontent = file.Content;

                foreach (var v in valuesCollection)
                {
                    fcontent = fcontent.Replace($"${v.Key}$", v.Value);
                }

                SyntaxNode insertContent = SyntaxFactory.ParseMemberDeclaration(fcontent);
                insertContent = insertContent ?? SyntaxFactory.ParseCompilationUnit(fcontent);

                var nodes = insertContent.ChildNodes().ToList();

                var insertAttributes = nodes.OfType<AttributeListSyntax>();

                var isAttributes = insertAttributes.Count() == nodes.Count;
                if (isAttributes)
                {
                    editor.AddAttributes(forNode, insertAttributes);

                    continue;
                }

                if (type == AppendCodeTypeEnum.OuterBefore
                    || type == AppendCodeTypeEnum.Replace)
                    editor.InsertBefore(forNode, nodes);
                else if (type == AppendCodeTypeEnum.OuterAfter)
                    editor.InsertAfter(forNode, nodes);
                else if (type == AppendCodeTypeEnum.InnerBefore)
                    editor.InsertMembers(forNode, 0, nodes);
                else if (type == AppendCodeTypeEnum.InnerAfter)
                    editor.InsertMembers(forNode, forNode.ChildNodes().Count() - 1, nodes);
                else if (type == AppendCodeTypeEnum.BaseList)
                {
                    var t = forNode as TypeDeclarationSyntax;

                    var btl = t?.BaseList;

                    var baseTypes = btl?.Types.Select(x => x.Type.ToString()).ToArray();

                    foreach (var n in nodes)
                    {
                        if (baseTypes?.Contains(n.ToString()) != true)
                            editor.AddBaseType(forNode, n);
                    }
                }
            }

            if (type == AppendCodeTypeEnum.Replace)
                editor.RemoveNode(forNode);


            root = (CompilationUnitSyntax)editor.GetChangedRoot();

            if (template.Usings.Any())
            {
                var rootChilds = root.ChildNodes();

                var docUsings = rootChilds.OfType<UsingDirectiveSyntax>().ToImmutableArray();

                SyntaxNode nsDeclarations = docUsings.FirstOrDefault();

                nsDeclarations = nsDeclarations ?? rootChilds.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

                nsDeclarations = nsDeclarations ?? rootChilds.OfType<TypeDeclarationSyntax>().FirstOrDefault();

                if (nsDeclarations != null)
                {
                    var usings = template.Usings.ToImmutableArray();

                    var dc = docUsings.Select(x => x.Name.ToString()).ToImmutableArray();

                    root = root.AddUsings(
                    usings.Except(dc).Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))
                                        .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)).ToArray()
                    );
                }

            }

            sourceDoc = sourceDoc.WithSyntaxRoot(root);

            try
            {
                sourceDoc = await Microsoft.CodeAnalysis.Formatting.Formatter.FormatAsync(sourceDoc, sourceDoc.Project.Solution.Workspace.Options, cancellationToken);

            }
            catch (Exception)
            {
            }

            sharedProj = sourceDoc.Project;

            s = sharedProj.Solution;

            return s;
        }

        static async Task LoadTemplateCloneValues(
            SyntaxNode forNode,
            Dictionary<string, string> valuesCollection,
            Document sourceDoc,
            TemplateData template,
            CancellationToken cancellationToken)
        {

            SemanticModel semanticModel = default;

            var contentProperties = string.Empty;

            if (template.Clone?.Contains("properties") == true)
            {
                semanticModel = semanticModel ?? await sourceDoc.GetSemanticModelAsync(cancellationToken);

                var td = forNode.GetParentOfType<TypeDeclarationSyntax>();

                contentProperties = await GetContentProperties(td, semanticModel, false);
            }

            valuesCollection["content_properties"] = contentProperties;

            var contentMethods = string.Empty;

            if (template.Clone?.Contains("method_declarations") == true)
            {
                semanticModel = semanticModel ?? await sourceDoc.GetSemanticModelAsync(cancellationToken);

                var td = forNode.GetParentOfType<TypeDeclarationSyntax>();

                contentMethods = await GetContentMethodDeclarations(td, semanticModel, false);
            }

            valuesCollection["content_methods"] = contentMethods;

            contentProperties = string.Empty;

            if (template.Clone?.Contains("deep_properties") == true)
            {
                semanticModel = semanticModel ?? await sourceDoc.GetSemanticModelAsync(cancellationToken);

                var td = forNode.GetParentOfType<TypeDeclarationSyntax>();

                contentProperties = await GetContentProperties(td, semanticModel, true);
            }

            valuesCollection["content_deep_properties"] = contentProperties;

            contentMethods = string.Empty;

            if (template.Clone?.Contains("deep_method_declarations") == true)
            {
                semanticModel = semanticModel ?? await sourceDoc.GetSemanticModelAsync(cancellationToken);

                var td = forNode.GetParentOfType<TypeDeclarationSyntax>();

                contentMethods = await GetContentMethodDeclarations(td, semanticModel, true);
            }

            valuesCollection["content_deep_methods"] = contentMethods;
        }

        static async Task<string> GetContentMethodDeclarations(TypeDeclarationSyntax srcClassNode, SemanticModel semanticModel, bool deep)
        {
            var t = semanticModel.GetDeclaredSymbol(srcClassNode) as ITypeSymbol;

            var methods = (deep ? t.GetAllMembers() : t.GetMembers().ToArray())
                .Where(x => x is IMethodSymbol && !x.Name.StartsWith("."))
                .Select(x => x as IMethodSymbol)
                .ToArray();

            var methodsCode = methods.Select(x =>
            {
                var m = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(x.ReturnType.Name), x.Name);

                m = m.AddParameterListParameters(x.Parameters.Where(p => p.DeclaringSyntaxReferences.Any()).Select(p => p.DeclaringSyntaxReferences.First().GetSyntax() as ParameterSyntax).ToArray());

                return m;
            })
                .Select(x => x.NormalizeWhitespace().ToFullString());

            return string.Join($";{Environment.NewLine}", methodsCode) + (methodsCode.Any() ? ";" : string.Empty);
        }

        static async Task<string> GetContentProperties(TypeDeclarationSyntax srcClassNode, SemanticModel semanticModel, bool deep)
        {
            var t = semanticModel.GetDeclaredSymbol(srcClassNode) as ITypeSymbol;

            var properties = (deep ? t.GetAllMembers() : t.GetMembers().ToArray())
                .Where(x => x is IPropertySymbol)
                .Select(x => x as IPropertySymbol)
                .ToArray();

            var propsCode = properties.Select(x => SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName(x.Type.GetTypeFullName()), x.Name)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
            )).Select(x => x.NormalizeWhitespace().ToFullString());

            return string.Join(Environment.NewLine, propsCode);
        }

        static Dictionary<string, string> FillMap(Dictionary<string, string> items, Dictionary<string, string> from)
        {
            foreach (var fitem in from.ToArray())
            {
                foreach (var key in items.Keys.ToArray())
                {
                    items[key] = items[key].Replace($"${fitem.Key}$", fitem.Value);
                }

                items.TryAdd(fitem.Key, fitem.Value);
            }

            return items;
        }

        static Dictionary<Guid, FastActionCacheData> cache = new Dictionary<Guid, FastActionCacheData>();
    }

    internal enum AppendCodeTypeEnum
    {
        InnerBefore,
        InnerAfter,
        OuterBefore,
        OuterAfter,
        BaseList,
        Replace,
        Files
    }

    public static class EditorExtensions
    {
        public static void AddAttributes(this SyntaxEditor e, SyntaxNode n, IEnumerable<AttributeListSyntax> attributes)
        {
            foreach (var item in attributes)
            {
                e.AddAttribute(n, item);
            }
        }
    }
}
