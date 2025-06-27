using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Text;
using NSL.Generators.Utils;
using NSL.Refactoring.FastAction.UI;
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
using System.Xml.Linq;

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
                                            Directory.Delete(baseCachePath, true);

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

            var typeDecl = getTypedNode<TypeDeclarationSyntax>(node, getNodeDepth(node));

            var classDecl = typeDecl as ClassDeclarationSyntax;

            var structDecl = typeDecl as StructDeclarationSyntax;


            var enumDecl = getTypedNode<EnumDeclarationSyntax>(node, getNodeDepth(node));

            var enumMemberDecl = getTypedNode<EnumMemberDeclarationSyntax>(node, getNodeDepth(node));

            var namespaceDecl = getTypedNode<NamespaceDeclarationSyntax>(node, getNodeDepth(node));


            var parameterNode = getTypedNode<ParameterSyntax>(node, getNodeDepth(node));

            var attributeNode = getTypedNode<AttributeSyntax>(node, getNodeDepth(node));

            var memberDecl = getTypedNode<MemberDeclarationSyntax>(node, getNodeDepth(node));

            var propertyDecl = memberDecl as PropertyDeclarationSyntax;

            var fieldDecl = memberDecl as FieldDeclarationSyntax;

            var methodDecl = memberDecl as MethodDeclarationSyntax;

            string memberType = propertyDecl?.Type?.ToString()
                ?? fieldDecl?.Declaration?.Type?.ToString()
                ?? typeDecl?.Identifier.ToString()
                ?? enumDecl?.Identifier.ToString()
                ?? parameterNode?.Type.ToString();

            var baseType = node as SimpleBaseTypeSyntax;

            var nodeName = baseType?.ToString()
                ?? typeDecl?.GetClassName()
                ?? namespaceDecl?.Name.ToString()
                ?? enumDecl?.Identifier.ToString()
                ?? attributeNode?.ToString()
                ?? node.ToString();

            node = attributeNode
                ?? parameterNode
                ?? (SyntaxNode)fieldDecl
                ?? (SyntaxNode)methodDecl
                ?? (SyntaxNode)propertyDecl
                ?? (SyntaxNode)enumDecl
                ?? (SyntaxNode)classDecl
                ?? (SyntaxNode)enumDecl
                ?? (SyntaxNode)namespaceDecl
                ?? node;



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

                    typeAllow = typeAllow || action.Types.Contains("parameter") && parameterNode != null;

                    typeAllow = typeAllow || action.Types.Contains("base_type") && baseType != null;

                    if (!typeAllow) continue;

                    var matches = new Dictionary<string, string>();

                    bool validateConditions()
                    {
                        bool validateNamespace(ConditionData condition)
                        {
                            if (condition.Namespace != default)
                            {
                                var ns = node.GetParentNamespace()?.Name.ToString();

                                if (condition.OptionalExists && ns == default)
                                    return true;

                                Dictionary<string, string> nnmatch = null;

                                if (ns == default || !condition.IsMatch(condition.Namespace, ns, out nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }
                            return true;
                        }

                        bool validateProperty(ConditionData condition)
                        {
                            if (condition.PropertyDeclaration != default)
                            {
                                if (condition.OptionalExists && propertyDecl == default)
                                    return true;

                                Dictionary<string, string> nnmatch = null;

                                if (propertyDecl == default || !condition.IsMatch(condition.PropertyDeclaration, propertyDecl.ToString(), out nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }
                            return true;
                        }

                        bool validateMemberType(ConditionData condition)
                        {
                            if (condition.MemberType != default)
                            {
                                if (condition.OptionalExists && memberType == default)
                                    return true;

                                Dictionary<string, string> nnmatch = null;

                                if (memberType == default || !condition.IsMatch(condition.MemberType, memberType, out nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }
                            return true;
                        }

                        bool validateField(ConditionData condition)
                        {
                            if (condition.FieldDeclaration != default)
                            {
                                if (condition.OptionalExists && fieldDecl == default)
                                    return true;

                                Dictionary<string, string> nnmatch = null;

                                if (propertyDecl == default || !condition.IsMatch(condition.FieldDeclaration, fieldDecl.ToString(), out nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                if (nnmatch != null)
                                    foreach (var item in nnmatch)
                                    {
                                        matches[item.Key] = item.Value;
                                    }
                            }

                            return true;
                        }

                        bool validateFilePath(ConditionData condition)
                        {
                            if (condition.FilePath != default)
                            {
                                if (condition.OptionalExists && document.FilePath == default)
                                    return true;


                                if (!condition.IsMatch(condition.FilePath, document.FilePath, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }

                        bool validateNodeName(ConditionData condition)
                        {
                            if (condition.NodeName != default)
                            {
                                if (condition.OptionalExists && nodeName == default)
                                    return true;

                                if (!condition.IsMatch(condition.NodeName, nodeName, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }

                        bool validateProjectPath(ConditionData condition)
                        {
                            if (condition.ProjectPath != default)
                            {
                                if (condition.OptionalExists && document.Project.FilePath == default)
                                    return true;

                                if (!condition.IsMatch(condition.ProjectPath, document.Project.FilePath, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }

                        bool validateProjectName(ConditionData condition)
                        {
                            if (condition.ProjectName != default)
                            {
                                if (condition.OptionalExists && document.Project.Name == default)
                                    return true;

                                if (!condition.IsMatch(condition.ProjectName, document.Project.Name, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }

                        bool validateSolutionPath(ConditionData condition)
                        {
                            if (condition.SolutionPath != default)
                            {
                                if (condition.OptionalExists && document.Project.Solution.FilePath == default)
                                    return true;

                                if (!condition.IsMatch(condition.SolutionPath, document.Project.Solution.FilePath, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }

                        bool validateSolutionName(ConditionData condition)
                        {
                            if (condition.SolutionName != default)
                            {
                                var name = Path.GetFileName(document.Project.Solution.FilePath);

                                name = name.Substring(0, name.LastIndexOf('.'));

                                if (condition.OptionalExists && name == default)
                                    return true;

                                if (!condition.IsMatch(condition.SolutionName, name, out var nnmatch))
                                {
                                    if (!condition.OptionalCondition)
                                        return false;
                                }

                                foreach (var item in nnmatch)
                                {
                                    matches[item.Key] = item.Value;
                                }
                            }

                            return true;
                        }


                        foreach (var condition in action.Conditions)
                        {
                            if (!validateNamespace(condition)) return false;
                            if (!validateProperty(condition)) return false;
                            if (!validateField(condition)) return false;
                            if (!validateMemberType(condition)) return false;
                            if (!validateFilePath(condition)) return false;
                            if (!validateNodeName(condition)) return false;
                            if (!validateProjectPath(condition)) return false;
                            if (!validateProjectName(condition)) return false;
                            if (!validateSolutionPath(condition)) return false;
                            if (!validateSolutionName(condition)) return false;
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
                            templateValues["safeitemname"] = NormalizeName(outputName);

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

        static Dictionary<Guid, FastActionCacheData> cache = new Dictionary<Guid, FastActionCacheData>();
    }
}
