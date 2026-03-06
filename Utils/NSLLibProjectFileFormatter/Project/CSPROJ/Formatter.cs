using NSLLibProjectFileFormatter.Solution;
using System.Collections;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace NSLLibProjectFileFormatter.Project.CSPROJ
{

    internal partial class Formatter
    {
        private readonly string path;
        private readonly string slnPath;

        private string[] AvailableConfigurations = new string[] { "Debug", "Release", "DebugExamples", "Unity", "UnityDebug" };

        private string[] ExcludeDirectories = new string[]
        {
            "Utils",
            "DevSpace"
        };

        public Formatter(string path)
        {
            this.path = path;

            slnPath = Path.Combine(this.path, "NSL.sln");
        }

        public void Run()
        {
            var di = new DirectoryInfo(path);

            processDirectory(di);
            //foreach (var d in di.GetDirectories())
            //{
            //    if (ExcludeDirectories.Contains(d.Name))
            //        continue;

            //    processDirectory(d);
            //}

            SLNBuilder.BuildSln(slnPath, projects, AvailableConfigurations);
        }

        private List<ProjectFileInfo> projects = new List<ProjectFileInfo>();

        enum QuestionResultEnum
        {
            N = 1,
            Y = 2,
            A = 4,
            //Variant1
        }

        QuestionResultEnum Question(Action questionContent, string? Y = null, string? N = null, string? A = null, bool AAnswer = false)
        {

            while (true)
            {
                questionContent();

                if (AAnswer)
                    return QuestionResultEnum.Y;

                if (A != null)
                    Console.WriteLine($"Input 'A' {A}");

                if (Y != null)
                    Console.WriteLine($"Input 'Y' {Y}");

                if (N != null)
                    Console.WriteLine($"Input 'N' {N}");

                var a = Console.ReadLine().Trim();

                if (Enum.TryParse<QuestionResultEnum>(a, true, out var result))
                {
                    return result;
                }

                Console.WriteLine($"Invalid answer! \"{a}\" is not supported");
            }
        }

        bool allincorrectrename = false;
        bool allincorrectdir = false;

        void processDirectory(DirectoryInfo di)
        {
            List<string> clearProjectList = new List<string>();

            foreach (var item in di.GetFiles("*.csproj", SearchOption.AllDirectories))
            {

                var relPath = Path.GetRelativePath(di.FullName, item.FullName);

                if (ExcludeDirectories.Any(relPath.StartsWith))
                    continue;

                if (relPath.Contains("Templates") && relPath.Contains("content"))
                    continue;

                var fname = item.Name;

                if (!fname.StartsWith("NSL."))
                {
                    var fixfname = $"NSL.{fname}";

                    var r = Question(() =>
                    {
                        Console.WriteLine($"Missed project NSL. prefix in project name '{fname}'");
                    }, $"for rename to '{fixfname}'", $"for skip", "for rename all", allincorrectrename);

                    if (r != QuestionResultEnum.N)
                    {
                        allincorrectrename = allincorrectrename || r == QuestionResultEnum.A;

                        var mpath = Path.Combine(item.DirectoryName, fixfname);

                        File.Move(item.FullName, mpath);
                        relPath = Path.GetRelativePath(di.FullName, mpath);
                        fname = fixfname;
                    }
                }



                var vdname = Path.GetFileNameWithoutExtension(fname);

                var epath = Path.Combine(vdname, fname);

                string fileName = Path.GetFileName(relPath);
                string projectName = Path.GetFileNameWithoutExtension(fileName);

                string parentDirectory = Path.GetFileName(Path.GetDirectoryName(relPath));

                string[] validSuffixes = [".Server", ".Client"];

                bool isExactMatch = string.Equals(parentDirectory, projectName, StringComparison.OrdinalIgnoreCase);

                bool isSharedFolderMatch = validSuffixes.Any(suffix =>
                    projectName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(parentDirectory, projectName[..^suffix.Length], StringComparison.OrdinalIgnoreCase)
                );

                if (!isExactMatch && !isSharedFolderMatch)
                {
                    var seppath = relPath.Count(x => x == Path.DirectorySeparatorChar) > 1 ? relPath.Substring(relPath.IndexOf(Path.DirectorySeparatorChar) + 1) : relPath;

                    var r = Question(() =>
                    {
                        Console.WriteLine($"Project has invalid path \ncurrent \"{seppath}\"\nvalid   \"{epath}\"");
                    }, $"for rename to '{epath}'", $"for skip", "for rename all", allincorrectdir);

                    if (r != QuestionResultEnum.N)
                    {
                        allincorrectdir = allincorrectdir || r == QuestionResultEnum.A;

                        var mpath = Path.Combine(item.Directory.Parent.FullName, vdname);
                        relPath = Path.GetRelativePath(di.FullName, Path.Combine(mpath, fname));
                        Directory.Move(item.Directory.FullName, mpath);

                    }

                }

                //if (fname.Contains("NSL.Refactoring") && fname.Contains(".UI"))
                //    continue;

                clearProjectList.Add(Path.Combine(di.FullName, relPath));
            }

            foreach (var item in clearProjectList)
            {
                BuildNewProjectFile(item);
            }
        }

        void BuildNewProjectFile(string path)
        {
            var doc = XDocument.Load(path); 
            
            XNamespace ns = doc.Root.GetDefaultNamespace();

            var NSLTypes = doc.Descendants(ns + "NSLProjectTypes").SingleOrDefault();

            if (NSLTypes == null) throw new Exception($"<NSLProjectTypes> is not defined in project '{path}'");

            var outputType = doc.Descendants(ns + "OutputType").SingleOrDefault()?.Value;

            List<string> NSLProjectTypes = NSLTypes.Value.Split(';').Select(x => x.Trim()).ToList();

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

            var sdk = doc.Root.Attribute("Sdk")?.Value;

            if (sdk != null)
            {
                bool unityOnly = hasUnityInProjectName(path);
                bool aspNetOnly = isOnlyAspNetProject(path, sdk);


                var description = doc.Descendants(ns + "Description").SingleOrDefault()?.Value;
                var rootNamespace = doc.Descendants(ns + "RootNamespace").SingleOrDefault()?.Value;

                var authors = doc.Descendants(ns + "Authors").SingleOrDefault()?.Value;
                var suppressDependenciesWhenPacking = doc.Descendants(ns + "SuppressDependenciesWhenPacking").SingleOrDefault()?.Value;

                var isRoslyn = bool.TryParse(doc.Descendants(ns + "IsRoslynComponent").SingleOrDefault()?.Value, out var rv) && rv;

                var isTemplate = HasTemplateType(NSLProjectTypes);

                var projectRefs = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "ProjectReference")
                    .ToArray();

                var frameworkRefs = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "FrameworkReference")
                    .ToArray();

                var unityRef = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "Reference")
                    .Where(x => x.Attribute("Include")?.Value == "UnityEngine")
                    .SingleOrDefault();


                var contentItems = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "Content")
                    .Where(x => x.Descendants(ns + "CopyToOutputDirectory").Any())
                    .ToArray();

                var packagesRefs = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "PackageReference")
                    .ToArray();

                var compileItems = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "Compile")
                    .ToArray();

                var embeddedResourceItems = doc.Descendants(ns + "ItemGroup")
                    .Descendants(ns + "EmbeddedResource")
                    .ToArray();

                bool analyzerPackage = false;

                bool analyzerUtils = false;

                CSProjBuilder tb = new CSProjBuilder();

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

                        var tf = "net10.0";

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
                        .WritePropertyItem("PackageId", Path.GetFileName(path).Replace(".Package.csproj", ""), analyzerPackage)
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

                    foreach (var group in frameworkRefs.GroupBy(x => x.Parent))
                    {
                        tb.AppendLine().WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });
                    }

                    foreach (var group in packagesRefs.GroupBy(x => x.Parent))
                    {
                        tb.AppendLine().WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });
                    }

                    foreach (var group in projectRefs.GroupBy(x => x.Parent))
                    {
                        tb.AppendLine()
                          .WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });
                    }

                    foreach (var group in compileItems.GroupBy(x => x.Parent))
                    {
                        tb.AppendLine()
                          .WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });
                    }


                    foreach (var group in embeddedResourceItems.GroupBy(x => x.Parent))
                    {
                        tb.AppendLine()
                          .WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });
                    }

                    if (analyzerPackage)
                        tb.AppendLine()
                        .WriteItemGroup(() =>
                            tb.AppendLine("<None Include=\"$(OutputPath)\\*NSL.*.dll\" Pack=\"true\" PackagePath=\"analyzers/dotnet/cs\" />"));

                    if (unityRef != null)
                        tb.AppendLine()
                        .WriteItemGroup(() =>
                        {
                            tb.AppendLine(unityRef.ToString());
                        });

                    foreach (var group in contentItems.GroupBy(x => x.Parent))
                        tb.AppendLine()
                        .WriteItemGroup(group.Key, () =>
                        {
                            foreach (var item in group)
                            {
                                tb.AppendLine(item.ToString());
                            }
                        });

                });

                string v = tb.ToString();

                File.WriteAllText(path, v);
            }

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
