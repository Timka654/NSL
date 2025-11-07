using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using Microsoft.CodeAnalysis.Text;
using NSL.Generators.Utils;
using NSL.Refactoring.FastAction.UI;
using System;
using System.Collections.Concurrent;
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
using System.Xml.Linq;

namespace NSL.Refactoring.FastAction.Core
{
    internal class FastActionBuilder
    {
        const string settingsFileName = "FastActionOptions.json";

        const string templatesUrl = "https://pubstorage.mtvworld.net/templates/fa/";

        static DateTime latestBaseCache = default;

        static JsonSerializerOptions jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        static string baseCachePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "NSL",
        "NSL.Refactoring.FastAction");

        static Dictionary<string, string> sourcePaths = new Dictionary<string, string>();

        static async Task<IEnumerable<string>> TryLoadSources(string[] sources)
        {
            if (latestBaseCache < DateTime.UtcNow.AddMinutes(-5))
            {
                var sourcesCachePath = Path.Combine(FastActionBuilder.baseCachePath, "sources.json");

                if (File.Exists(sourcesCachePath))
                {
                    sourcePaths = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(sourcesCachePath), jsonOptions) ?? new Dictionary<string, string>();
                }

                bool any = false;

                foreach (var _source in sources)
                {
                    if (!sourcePaths.TryGetValue(_source, out var path))
                    {
                        any = true;

                        path = sourcePaths[_source] = Guid.NewGuid().ToString("N");
                    }


                    path = Path.Combine(baseCachePath, path);

                    Directory.CreateDirectory(path);

                    var versionPath = Path.Combine(path, "version.json");

                    path = Path.Combine(path, "RoslynTemplatesCache");


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
                                                if (Directory.Exists(path))
                                                    Directory.Delete(path, true);

                                                za.ExtractToDirectory(path);
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
                }

                if (any)
                    File.WriteAllText(sourcesCachePath, JsonSerializer.Serialize(sourcePaths));
            }


            return sourcePaths.Select(x => Path.Combine(baseCachePath, x.Value, "RoslynTemplatesCache", settingsFileName));
        }

        static string NormalizeName(string name)
        {
            var rchars = name
                .Where(x => char.IsSymbol(x) && !char.IsDigit(x) && x != '_')
                .ToArray();

            foreach (var c in rchars)
            {
                name = name.Replace(c, '_');
            }

            return name;
        }

        static int getNodeDepth(SyntaxNode n)
        {
            if (n is IdentifierNameSyntax)
                return 1;
            if (n is PredefinedTypeSyntax)
                return 1;
            if (n is AccessorListSyntax)
                return 1;
            if (n is AccessorDeclarationSyntax)
                return 2;
            if (n is TypeArgumentListSyntax)
                return 4;

            return 0;
        }

        static TValue getTypedNode<TValue>(SyntaxNode node, int depth)
            where TValue : SyntaxNode
        {
            var n = node;
            while (true)
            {
                for (int i = 0; i < depth; i++)
                {
                    if (n is TValue)
                        return n as TValue;

                    n = n.Parent;
                }

                if (n != null && !(n is TValue))
                {
                    depth = getNodeDepth(n);
                    if (depth == 0)
                        break;
                }
                else
                    break;
            }
            return n as TValue;
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

                var packagesSource = Path.Combine(solutionPath, "NSL.Refactoring.FastAction.Sources.json");

                if (File.Exists(packagesSource))
                {
                    var packages = JsonSerializer.Deserialize<string[]>(File.ReadAllText(packagesSource), jsonOptions);

                    pathes.AddRange((await TryLoadSources(packages).ConfigureAwait(false)));
                }
                else
                    pathes.AddRange((await TryLoadSources(new[] { templatesUrl }).ConfigureAwait(false)));

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
                    actionData = cache.GetOrAdd(pid, (id) =>
                    {
                        var actionsContent = JsonIncrementalMerger.MergeJsonFiles(pathes);

                        return new FastActionCacheData
                        {
                            SolutionPath = solutionPath,
                            ProjectPath = projectPath,
                            Data = actionsContent.Deserialize<FastActionData>(new JsonSerializerOptions()
                            {
                                ReadCommentHandling = JsonCommentHandling.Skip,
                                AllowTrailingCommas = true
                            })
                        };
                    });

                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            if (actionData == null)
                return Array.Empty<RefactoringAction>();


            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var node2 = root.FindNode(span);

            if (node2 == null)
                return Array.Empty<RefactoringAction>();

            var typeDecl = getTypedNode<TypeDeclarationSyntax>(node2, getNodeDepth(node2));

            var classDecl = typeDecl as ClassDeclarationSyntax;

            var structDecl = typeDecl as StructDeclarationSyntax;


            var enumDecl = getTypedNode<EnumDeclarationSyntax>(node2, getNodeDepth(node2));

            var enumMemberDecl = getTypedNode<EnumMemberDeclarationSyntax>(node2, getNodeDepth(node2));

            var namespaceDecl = getTypedNode<NamespaceDeclarationSyntax>(node2, getNodeDepth(node2));


            var parameterNode = getTypedNode<ParameterSyntax>(node2, getNodeDepth(node2));

            var attributeNode = getTypedNode<AttributeSyntax>(node2, getNodeDepth(node2));

            var memberDecl = getTypedNode<MemberDeclarationSyntax>(node2, getNodeDepth(node2));

            var propertyDecl = memberDecl as PropertyDeclarationSyntax;

            var fieldDecl = memberDecl as FieldDeclarationSyntax;

            var methodDecl = memberDecl as MethodDeclarationSyntax;

            string memberType2 = propertyDecl?.Type?.ToString()
                ?? fieldDecl?.Declaration?.Type?.ToString()
                ?? typeDecl?.Identifier.ToString()
                ?? enumDecl?.Identifier.ToString()
                ?? parameterNode?.Type.ToString();

            var baseType2 = node2 as SimpleBaseTypeSyntax;

            var nodeName2 = baseType2?.ToString()
                ?? typeDecl?.GetClassName()
                ?? namespaceDecl?.Name.ToString()
                ?? enumDecl?.Identifier.ToString()
                ?? attributeNode?.ToString()
                ?? node2.ToString();

            var node3 = attributeNode
                ?? parameterNode
                ?? (SyntaxNode)fieldDecl
                ?? (SyntaxNode)methodDecl
                ?? (SyntaxNode)propertyDecl
                ?? (SyntaxNode)enumDecl
                ?? (SyntaxNode)structDecl
                ?? (SyntaxNode)classDecl
                ?? (SyntaxNode)namespaceDecl
                ?? node2;

            FASessionData data = new FASessionData(new (FastActionDataTypeEnum, object)[]
            {
                (FastActionDataTypeEnum.MemberType, memberType2 ),
                (FastActionDataTypeEnum.PropertyMemberType, propertyDecl?.Type?.ToString() ),
                (FastActionDataTypeEnum.FieldMemberType, fieldDecl?.Declaration?.Type?.ToString() ),
                (FastActionDataTypeEnum.Type, typeDecl?.Identifier.ToString() ),
                (FastActionDataTypeEnum.EnumType, enumDecl?.Identifier.ToString() ),
                (FastActionDataTypeEnum.ParameterType, parameterNode?.Type.ToString() ),

                (FastActionDataTypeEnum.BaseNodeName, nodeName2 ),
                (FastActionDataTypeEnum.TypeName, typeDecl?.GetClassName() ),
                (FastActionDataTypeEnum.NamespaceName, namespaceDecl?.Name.ToString() ),
                (FastActionDataTypeEnum.EnumName, enumDecl?.Identifier.ToString() ),
                (FastActionDataTypeEnum.AttributeName, attributeNode?.ToString() ),
                (FastActionDataTypeEnum.NodeName, node3.ToString() ),


                (FastActionDataTypeEnum.Attribute, attributeNode ),
                (FastActionDataTypeEnum.Parameter, parameterNode ),
                (FastActionDataTypeEnum.EnumMember, enumMemberDecl ),
                (FastActionDataTypeEnum.Field, fieldDecl ),
                (FastActionDataTypeEnum.Method, methodDecl ),
                (FastActionDataTypeEnum.Property, propertyDecl ),
                (FastActionDataTypeEnum.Enum, enumDecl ),
                (FastActionDataTypeEnum.Class, classDecl ),
                (FastActionDataTypeEnum.Struct, structDecl ),
                (FastActionDataTypeEnum.Namespace, namespaceDecl ),
                (FastActionDataTypeEnum.Node, node2 ),
                (FastActionDataTypeEnum.Member, memberDecl),
                (FastActionDataTypeEnum.BaseType, baseType2),

            });



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

                    if (!data.ValidateTypeCondition(action)) 
                        continue;

                    if (!data.ValidateConditions(document, action, out var matches))
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
                            templateValues["safeitemname"] = NormalizeName(outputName);

                        if (action.OutputType.StartsWith("files"))
                        {

                            if (templateValues.TryGetValue("name", out outputName))
                            {
                                outputName += "s";

                                string getNextPath(string path)
                                {
                                    var di = new DirectoryInfo(path);

                                    IEnumerable<DirectoryInfo> directions = di.GetDirectories();

                                    foreach (var _pp in action.PathPriority)
                                    {
                                        var pp = _pp;

                                        Func<DirectoryInfo, bool> ppf = (di2) => true;

                                        if (pp.StartsWith("*") && pp.EndsWith("*"))
                                        {
                                            pp = pp.Trim('*');
                                            ppf = x => x.Name.Contains(pp);
                                        }
                                        else if (pp.StartsWith("*"))
                                        {
                                            pp = pp.Trim('*');
                                            ppf = x => x.Name.EndsWith(pp);
                                        }
                                        else if (pp.EndsWith("*"))
                                        {
                                            pp = pp.Trim('*');
                                            ppf = x => x.Name.StartsWith(pp);
                                        }
                                        else
                                        {
                                            ppf = x => x.Name == pp;
                                        }

                                        if (directions is IOrderedEnumerable<DirectoryInfo> o)
                                            directions = o.ThenByDescending(ppf);
                                        else
                                            directions = directions.OrderByDescending(ppf);
                                    }

                                    foreach (var item in directions)
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
                            var context = new ExecutionActionContext()
                            {
                            };

                            context.SetSharedProjectId(group.Project.Id);
                            await context.WithSourceDocumentAsync(document, cts);

                            await context.SetSourceSyntaxAsync(memberDecl ?? typeDecl ?? namespaceDecl ?? attributeNode ?? enumMemberDecl ?? enumDecl ?? node, cancellationToken);

                            Dictionary<string, string> actionInputValues = new Dictionary<string, string>();
                            if (action.InputValues?.Any() == true)
                            {
                                if (isPreview)
                                {
                                    actionInputValues = action.InputValues.ToDictionary(x => x.Name, x => x.DefaultValue);
                                }
                                else
                                {
                                    actionInputValues = RefactorUI.PromptUserForInputValues(action.InputValues);

                                    if (actionInputValues == null)
                                        return default;
                                }
                            }

                            foreach (var item in outputTemplates)
                            {
                                await LoadTemplateCloneValues(context, item.Values, item.Template, cancellationToken);


                                Dictionary<string, string> templateInputValues = new Dictionary<string, string>();
                                if (item.Template.InputValues?.Any() == true)
                                {
                                    if (isPreview)
                                    {
                                        templateInputValues = item.Template.InputValues.ToDictionary(x => x.Name, x => x.DefaultValue);
                                    }
                                    else
                                    {
                                        templateInputValues = RefactorUI.PromptUserForInputValues(action.InputValues);

                                        if (templateInputValues == null)
                                            return default;
                                    }
                                }


                                var values = new Dictionary<string, string>(item.Values);

                                values = FillMap(values, actionInputValues);
                                values = FillMap(values, templateInputValues);
                                values = FillMap(values, item.Values);

                                if (item.AppendCodeType == AppendCodeTypeEnum.Files)
                                    await Files(group, action, values, item.NameSpace, item.OutputPath, item.Template, context, cts);
                                else
                                    await CodeFragment(item.AppendCodeType, group, action, values, item.NameSpace, item.OutputPath, item.Template, context, cts);
                            }

                            return context.Solution;
                        },
                        Path = actionPath
                    });
                }
            }

            return actions
                .OrderBy(x => x.Path)
                .GroupBy(x => x.Path)
                .Select(x => x.First())
                .ToArray();
        }

        static async Task Files(
            GroupData groupData,
            ActionData actionData,
            Dictionary<string, string> valuesCollection,
            string ns,
            string outputPath,
            TemplateData template,
            ExecutionActionContext context,
            CancellationToken cancellationToken)
        {
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

                await context.AddFileToSharedAsync(fname, fcontent, endPath, format: true, cancellationToken);
            }

            // Привязка проекта и добавление using
            if (actionData.GenerateUsingRefLink)
            {
                await context.AddProjectReferenceIfMissingAsync(cancellationToken);
                await context.AddUsingDirectiveAsync(ns, cancellationToken);
            }
        }

        static async Task CodeFragment(
            AppendCodeTypeEnum type,
            GroupData groupData,
            ActionData actionData,
            Dictionary<string, string> valuesCollection,
            string ns,
            string outputPath,
            TemplateData template,
            ExecutionActionContext context,
            CancellationToken cancellationToken)
        {
            await context.InsertTemplateCodeAsync(template, valuesCollection, type, cancellationToken);
        }

        static async Task LoadTemplateCloneValues(
            ExecutionActionContext context,
            Dictionary<string, string> valuesCollection,
            TemplateData template,
            CancellationToken cancellationToken)
        {
            SemanticModel semanticModel = context.SemanticModel;

            var td = context.SourceSyntax.GetParentOfType<TypeDeclarationSyntax>();

            var contentProperties = string.Empty;
            var contentDeepProperties = string.Empty;
            var contentMethods = string.Empty;
            var contentDeepMethods = string.Empty;

            if (template.Clone != null)
            {
                if (context.SourceSyntax is TypeDeclarationSyntax)
                {
                    if (template.Clone.Contains("properties"))
                        contentProperties = await GetContentProperties(td, semanticModel, false);
                    if (template.Clone.Contains("method_declarations"))
                        contentMethods = await GetContentMethodDeclarations(td, semanticModel, false);
                    if (template.Clone.Contains("deep_properties"))
                        contentDeepProperties = await GetContentProperties(td, semanticModel, true);
                    if (template.Clone.Contains("deep_method_declarations"))
                        contentDeepMethods = await GetContentMethodDeclarations(td, semanticModel, true);
                }
            }

            valuesCollection["content_properties"] = contentProperties;
            valuesCollection["content_deep_properties"] = contentDeepProperties;

            valuesCollection["content_methods"] = contentMethods;
            valuesCollection["content_deep_methods"] = contentDeepMethods;
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
                    items[key] = items[key]?.Replace($"${fitem.Key}$", fitem.Value);
                }

                items.TryAdd(fitem.Key, fitem.Value);
            }

            return items;
        }

        static ConcurrentDictionary<Guid, FastActionCacheData> cache = new ConcurrentDictionary<Guid, FastActionCacheData>();
    }
}
