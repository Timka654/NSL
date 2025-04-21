using NSLLibProjectFileFormatter.Solution;
using System.Text.RegularExpressions;

namespace NSLLibProjectFileFormatter.Project.CSPROJ
{

    internal partial class Formatter
    {
        private readonly string path;
        private readonly string slnPath;

        private string[] AvailableConfigurations = new string[] { "Debug", "Release", "DebugExamples", "Unity", "UnityDebug" };

        public Formatter(string path)
        {
            this.path = path;

            slnPath = Path.Combine(this.path, "NSL.sln");
        }

        public void Run()
        {
            var di = new DirectoryInfo(path);

            foreach (var d in di.GetDirectories())
            {
                if (d.Name == "Utils")
                    continue;

                processDirectory(d);
            }

            SLNBuilder.BuildSln(slnPath, projects, AvailableConfigurations);
        }

        private List<ProjectFileInfo> projects = new List<ProjectFileInfo>();

        void processDirectory(DirectoryInfo di)
        {
            foreach (var item in di.GetFiles("*.csproj", SearchOption.AllDirectories))
            {
                var relPath = Path.GetRelativePath(di.FullName, item.FullName);

                if (relPath.Contains("Templates") && relPath.Contains("content"))
                    continue;

                BuildNewProjectFile(item.FullName, File.ReadAllLines(item.FullName).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            }
        }


        void BuildNewProjectFile(string path, string[] currentContent)
        {
            var outputType = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectOutputTypeRegex()));

            var NSLTypes = FindAllLinesByRegex(currentContent, CSProjRegex.GetProjectNSLTypes()).FirstOrDefault();

            List<string> NSLProjectTypes = NSLTypes == null ? new List<string>() : NSLTypes[2].Captures.Select(x => x.Value).ToList();

            List<string> configurations = new List<string> { "DebugExamples" };

            if (Equals(outputType, "Exe")
                || HasTest(NSLProjectTypes)
                || HasExternal(NSLProjectTypes)
                || hasVsixInProjectName(path)
                || IsTestOExample(path))
            {
                if (HasExternal(NSLProjectTypes))
                {
                    configurations.AddRange(new string[] { "Debug", "Release" });

                    if (HasUnitySupport(NSLProjectTypes))
                        configurations.AddRange(new[] { "UnityDebug", "Unity" });
                }

                projects.Add(new ProjectFileInfo(path, configurations.ToArray(), Path.GetRelativePath(this.path, Path.GetDirectoryName(path) + "/.."), NSLProjectTypes));

                return;
            }

            configurations.AddRange(new string[] { "Debug", "Release" });

            var sdk = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectSdkRegex()));

            bool unityOnly = hasUnityInProjectName(path);
            bool aspNetOnly = isOnlyAspNetProject(path, sdk);


            CSProjBuilder tb = new CSProjBuilder();

            var description = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectDescriptionRegex()));
            var rootNamespace = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectRootNamespaceRegex()));

            var authors = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectAuthorsRegex()));
            var suppressDependenciesWhenPacking = GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectSuppressDependenciesWhenPackingRegex()));

            var isRoslyn = bool.TryParse(GetGroupValue(FindGroupsByRegex(currentContent, CSProjRegex.GetProjectIsRoslynRegex())), out var rv) && rv;

            var isTemplate = HasTemplateType(NSLProjectTypes);

            var fileContent = string.Join(Environment.NewLine, currentContent);

            var projectRefs = FindAllByRegex(fileContent, CSProjRegex.GetProjectReferenceRegex());

            //var projectRefsWithConditions = FindAllByRegex(fileContent, GetItemGroupWithConditionRegex());

            var unityRef = FindAllByRegex(fileContent, CSProjRegex.GetProjectUnityRefRegex()).FirstOrDefault();

            var contentItems = FindAllByRegex(fileContent, CSProjRegex.GetProjectContentItemGroupRegex());

            var packagesRefs = FindAllByRegex(fileContent, CSProjRegex.GetPackageReferenceRegex());

            var compileItems = FindAllByRegex(fileContent, CSProjRegex.GetCompileRegex());

            var embeddedResourceItems = FindAllByRegex(fileContent, CSProjRegex.GetEmbeddedResourceRegex());

            bool analyzerPackage = false;

            bool analyzerUtils = false;

            tb.WriteProjectRoot(sdk, () =>
            {
                tb.WritePropertyGroup(() =>
                {
                    if (!NSLProjectTypes.Any())
                    {
                        if (unityOnly && aspNetOnly)
                            throw new Exception($"{path} cannot contains multiple '*Only' types");

                        if (unityOnly)
                        {
                            NSLProjectTypes.Add("UnityTarget");
                            NSLProjectTypes.Add("UnitySupport");
                        }
                        else if (aspNetOnly)
                        {
                            NSLProjectTypes.Add("ASPTarget");
                        }
                        else
                        {
                            NSLProjectTypes.Add("UnitySupport");
                        }

                        if (isRoslyn)
                        {
                            NSLProjectTypes.Add("Analyzer");
                        }

                        if (unityRef != null)
                            NSLProjectTypes.Add("UnityReference");
                    }

                    var tf = "net9.0";

                    if (HasUnityTarget(NSLProjectTypes))
                        configurations.Clear();

                    if (HasUnitySupport(NSLProjectTypes))
                        configurations.AddRange(new[] { "UnityDebug", "Unity" });


                    analyzerPackage = HasAnalyzerPackageTarget(NSLProjectTypes);
                    analyzerUtils = HasAnalyzerUtils(NSLProjectTypes);
                    var analyzerCore = HasAnalyzerCoreTarget(NSLProjectTypes);

                    if (analyzerUtils)
                        tf = "netstandard2.0";

                    tb.WritePropertyItem("NSLProjectTypes", string.Join(';', NSLProjectTypes))
                    .AppendLine();


                    tb.WritePropertyItem("TargetFramework", tf)
                      .WritePropertyItem("Configurations", string.Join(';', AvailableConfigurations))
                      .WritePropertyItem("AllowUnsafeBlocks", true)
                      .WritePropertyItem("Nullable", "disable")
                      .WritePropertyItem("RootNamespace", rootNamespace, rootNamespace != null);


                    tb.AppendLine(analyzerPackage)
                    //.WritePropertyItem("IncludeBuildOutput", false, analyzerPackage)
                    .WritePropertyItem("DevelopmentDependency", true, analyzerPackage)
                    .WritePropertyItem("NoPackageAnalysis", true, analyzerPackage)
                    /*.WritePropertyItem("TargetsForTfmSpecificContentInPackage", "$(TargetsForTfmSpecificContentInPackage);_AddAnalyzersToOutput", analyzerPackage)*/;


                    tb.WritePropertyItem("IsPackable", true, aspNetOnly)
                    .WritePropertyItem("OutputType", "Library", aspNetOnly);

                    tb.AppendLine();

                    tb.WritePropertyItem("PublishRepositoryUrl", true)
                      .WritePropertyItem("EmbedUntrackedSources", true)
                      .WritePropertyItem("AutoGenerateBindingRedirects", true)
                      .WritePropertyItem("GenerateBindingRedirectsOutputType", true);

                    tb.AppendLine(isRoslyn)
                      .WritePropertyItem("EnforceExtendedAnalyzerRules", true, isRoslyn)
                      .WritePropertyItem("IsRoslynComponent", true, isRoslyn)
                      .WritePropertyItem("IncludeBuildOutput", false, analyzerPackage);

                    tb.WritePropertyItem("SuppressDependenciesWhenPacking", suppressDependenciesWhenPacking, suppressDependenciesWhenPacking != null);


                    if (isTemplate)
                    {
                        tb.WritePropertyItem("PackageType", "Template")
                            .WritePropertyItem("IncludeContentInPack", true)
                            .WritePropertyItem("IncludeBuildOutput", false)
                            .WritePropertyItem("ContentTargetFolders", "content")
                            .WritePropertyItem("NoWarn", "$(NoWarn);NU5128")
                            .WritePropertyItem("NoDefaultExcludes", true)
                            .WritePropertyItem("LocalizeTemplates", false);
                    }

                    tb.AppendLine()
                    .WritePropertyItem("PackageId", Path.GetFileName(path).Replace(".Package.csproj",""), analyzerPackage)
                    .WritePropertyItem("Version", "$(VersionSuffix)")

                    .WritePropertyItem("Authors", authors, authors != null)
                    .WritePropertyItem("Authors", "Relife87", authors == null)

                    .WritePropertyItem($"Description", description, description != null);


                    if (analyzerUtils && analyzerCore)
                    {
                        tb.AppendLine()
                            .WritePropertyItem("IsPackable", false);
                    }
                    else if (HasUnpacking(NSLProjectTypes))
                        tb.AppendLine()
                            .WritePropertyItem("IsPackable", false);
                });


                tb.WriteItemGroup(() => tb.AppendLine("<Content Include=\"**\\*\\.template.config\\template.json\" />"), isTemplate)
                .AppendLine()
                .WritePropertyGroup("'$(Configuration)'=='Debug'", () =>
                    tb.WritePropertyItem("PackageId", "$(MSBuildProjectName)_Debug")
                );

                if (HasUnitySupport(NSLProjectTypes))
                {
                    tb.AppendLine()
                    .WritePropertyGroup("'$(Configuration)'=='UnityDebug'", () => tb
                        .WritePropertyItem("AssemblyName", "Unity.$(MSBuildProjectName)", !HasUnityTarget(NSLProjectTypes))
                        .WritePropertyItem("TargetFramework", "netstandard2.1", !HasAnalyzerUtils(NSLProjectTypes))
                        .WritePropertyItem("DefineConstants", "DEBUG;TRACE")
                    );

                    if (!HasUnityTarget(NSLProjectTypes) || !HasAnalyzerUtils(NSLProjectTypes))
                        tb.AppendLine()
                        .WritePropertyGroup("'$(Configuration)'=='Unity'", () => tb
                            .WritePropertyItem("AssemblyName", "Unity.$(MSBuildProjectName)", !HasUnityTarget(NSLProjectTypes))
                            .WritePropertyItem("TargetFramework", "netstandard2.1", !HasAnalyzerUtils(NSLProjectTypes))
                        );

                }

                if (packagesRefs.Any())
                    foreach (Match item in packagesRefs)
                    {
                        var igroups = item.Groups;

                        tb.WriteItemGroup(item.Groups[2].Captures.Select(x => x.Value.Trim()), () =>
                        {
                            var packageProps = igroups[6].Captures;

                            var packageBodyProps = igroups[10].Captures;

                            tb.AppendLine($"<PackageReference {string.Join(" ", packageProps.Select(x => x.Value.Replace("\t", string.Empty).Trim()).ToArray())} {(packageBodyProps.Any() ? ">" : "/>")}");

                            if (packageBodyProps.Any())
                            {
                                tb.NextTab();

                                foreach (Capture bodyProp in packageBodyProps)
                                {
                                    tb.AppendLine(bodyProp.Value.Trim());
                                }

                                tb.PrevTab().AppendLine("</PackageReference>");
                            }

                        });
                    }

                if (projectRefs.Any())
                    tb.AppendLine()
                      .WriteItemGroup(() =>
                    {
                        foreach (Match item in projectRefs)
                        {
                            Match t0 = item.Captures.First() as Match;

                            var t1 = t0.Groups[2];

                            foreach (Capture inc in t1.Captures)
                            {
                                tb.AppendLine($"<ProjectReference {inc.Value.TrimStart()} />");
                            }
                        }
                    });


                if (compileItems.Any())
                    foreach (Match item in compileItems)
                    {
                        var igroups = item.Groups;

                        tb.WriteItemGroup(item.Groups[2].Captures.Select(x => x.Value.Trim()), () =>
                        {
                            var packageProps = igroups[6].Captures;

                            var packageBodyProps = igroups[10].Captures;

                            tb.AppendLine($"<Compile {string.Join(" ", packageProps.Select(x => x.Value.Replace("\t", string.Empty).Trim()).ToArray())} {(packageBodyProps.Any() ? ">" : "/>")}");

                            if (packageBodyProps.Any())
                            {
                                tb.NextTab();

                                foreach (Capture bodyProp in packageBodyProps)
                                {
                                    tb.AppendLine(bodyProp.Value.Trim());
                                }

                                tb.PrevTab().AppendLine("</Compile>");
                            }

                        });
                    }


                if (embeddedResourceItems.Any())
                    foreach (Match item in embeddedResourceItems)
                    {
                        var igroups = item.Groups;

                        tb.WriteItemGroup(item.Groups[2].Captures.Select(x => x.Value.Trim()), () =>
                        {
                            var packageProps = igroups[6].Captures;

                            var packageBodyProps = igroups[10].Captures;

                            tb.AppendLine($"<EmbeddedResource {string.Join(" ", packageProps.Select(x => x.Value.Replace("\t", string.Empty).Trim()).ToArray())} {(packageBodyProps.Any() ? ">" : "/>")}");

                            if (packageBodyProps.Any())
                            {
                                tb.NextTab();

                                foreach (Capture bodyProp in packageBodyProps)
                                {
                                    tb.AppendLine(bodyProp.Value.Trim());
                                }

                                tb.PrevTab().AppendLine("</EmbeddedResource>");
                            }

                        });
                    }


                tb.AppendLine(analyzerPackage)
                //.WriteTarget("_AddAnalyzersToOutput",()=> tb
                .WriteItemGroup(() =>
                    tb.AppendLine("<None Include=\"$(OutputPath)\\*NSL.*.dll\" Pack=\"true\" PackagePath=\"analyzers/dotnet/cs\" />")
                    //.AppendLine("<None Include=\"$(OutputPath)\\*NSL.*.Shared.dll\" Pack=\"true\" PackagePath=\"lib/$(TargetFramework)\" />")
                , analyzerPackage)/*, analyzerPackage)*/;

                tb.AppendLine(unityRef != null)
                    .WriteItemGroup(() =>
                    {
                        tb.AppendLine("<Reference Include=\"UnityEngine\">");
                        tb.NextTab();
                        tb.AppendLine($"<HintPath>{GetGroupValue(unityRef.Groups)}</HintPath>");
                        tb.PrevTab();
                        tb.AppendLine("</Reference>");
                    }, unityRef != null);

                tb.AppendLine(contentItems.Any())
                .WriteItemGroup(() =>
                {
                    foreach (Match item in contentItems)
                    {
                        Group pathGroup = item.Groups[2];
                        Group typeGroup = item.Groups[3];


                        tb.AppendLine($"<Content Include=\"{pathGroup.Value}\">").NextTab();
                        tb.AppendLine($"<CopyToOutputDirectory>{typeGroup.Value}</CopyToOutputDirectory>");
                        tb.PrevTab().AppendLine($"</Content>");

                    }
                }, contentItems.Any());

            });

            string v = tb.ToString();

            File.WriteAllText(path, v);

            projects.Add(new ProjectFileInfo(path, configurations.ToArray(), Path.GetRelativePath(this.path, Path.GetDirectoryName(path) + "/.."), NSLProjectTypes));
        }

        public bool HasUnitySupport(List<string> types)
        {
            if (!types.Contains("UnitySupport"))
                return HasUnityTarget(types);

            return true;
        }

        public bool HasExternal(List<string> types)
            => types.Contains("External");

        public bool HasTest(List<string> types)
            => types.Contains("Test");

        public bool HasAnalyzer(List<string> types)
            => types.Contains("Analyzer");

        public bool HasUnpacking(List<string> types)
            => types.Contains("Unpacking");

        public bool HasAnalyzerUtils(List<string> types)
            => HasAnalyzer(types) || HasAnalyzerPackageTarget(types) || types.Contains("AnalyzerUtils");

        public bool HasUnityTarget(List<string> types)
        {
            return types.Contains("UnityTarget");
        }

        public bool HasAnalyzerPackageTarget(List<string> types)
        {
            return types.Contains("AnalyzerPackage");
        }

        public bool HasAnalyzerCoreTarget(List<string> types)
        {
            return types.Contains("AnalyzerCore");
        }


        public bool HasAnalyzerSharedTarget(List<string> types)
        {
            return types.Contains("AnalyzerShared");
        }

        public bool HasASPTarget(List<string> types)
        {
            return types.Contains("ASPTarget");
        }

        public bool HasTemplateType(List<string> types)
        {
            return types.Contains("Template");
        }

        private bool IsVsix(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains(".Vsix", StringComparison.OrdinalIgnoreCase);
        }

        private bool hasUnityInProjectName(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains("Unity", StringComparison.OrdinalIgnoreCase);
        }

        private bool hasVsixInProjectName(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains(".Vsix", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsTestOExample(string path)
        {
            if (!IsExample(path))
                return IsTest(path);

            return true;
        }

        private bool IsExample(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains("Example");

        }

        private bool IsTest(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains(".Test");
        }

        private bool isOnlyAspNetProject(string path, string sdk)
        {
            var name = new FileInfo(path).Name;

            return name.Contains("AspNet", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Blazor", StringComparison.OrdinalIgnoreCase)
                || sdk.Equals("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase)
                || sdk.Equals("Microsoft.NET.Sdk.Razor", StringComparison.OrdinalIgnoreCase);
        }

        private string GetGroupValue(GroupCollection collection, int idx = 1) // first group by default
            => collection != null && collection.Count > idx ? collection[idx].Value : null;

        private GroupCollection[] FindAllLinesByRegex(string[] lines, Regex reg)
        {
            var foundLines = lines.Where(x => reg.IsMatch(x));

            return foundLines.Select(x => reg.Match(x).Groups).ToArray();
        }
        private MatchCollection FindAllByRegex(string lines, Regex reg)
        {
            return reg.Matches(lines);
        }

        private GroupCollection FindGroupsByRegex(string[] lines, Regex reg)
        {
            var foundLine = lines.FirstOrDefault(x => reg.IsMatch(x));

            if (foundLine == null) return null;

            return reg.Match(foundLine).Groups;
        }

    }
}
