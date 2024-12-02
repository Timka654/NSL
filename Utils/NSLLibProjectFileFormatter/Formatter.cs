using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NSLLibProjectFileFormatter
{
    internal partial class Formatter
    {
        [GeneratedRegex(@"\s*<Project\sSdk\s*=\s*""(\S*)""\s*>")]
        public partial Regex GetProjectSdkRegex();

        [GeneratedRegex(@"<ItemGroup>(\s*<ProjectReference(\s*(\S+)\s*=\s*""([\s\S]+?)"")\s*/>)+\s*</ItemGroup>")]
        public partial Regex GetProjectReferenceRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<PackageReference(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</PackageReference>\s*)*</ItemGroup>)+")]
        public partial Regex GetPackageReferenceRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<Compile(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</Compile>\s*)*</ItemGroup>)+")]
        public partial Regex GetCompileRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<EmbeddedResource(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</EmbeddedResource>\s*)*</ItemGroup>)+")]
        public partial Regex GetEmbeddedResourceRegex();

        [GeneratedRegex(@"<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>([\S\s]+?)</ItemGroup>")]
        public partial Regex GetItemGroupWithConditionRegex();

        [GeneratedRegex(@"\s*<Description>([\S\s]*)</Description>")]
        public partial Regex GetProjectDescriptionRegex();

        [GeneratedRegex(@"\s*<OutputType>([\S\s]*)</OutputType>")]
        public partial Regex GetProjectOutputTypeRegex();

        [GeneratedRegex(@"\s*<Authors>([\S\s]*)</Authors>")]
        public partial Regex GetProjectAuthorsRegex();

        [GeneratedRegex(@"\s*<SuppressDependenciesWhenPacking>([\S\s]*)</SuppressDependenciesWhenPacking>")]
        public partial Regex GetProjectSuppressDependenciesWhenPackingRegex();

        [GeneratedRegex(@"\s*<NSLProjectTypes>(([\S\s]+?)(;|\b))+</NSLProjectTypes>")]
        public partial Regex GetProjectNSLTypes();

        [GeneratedRegex(@"\s*<IsRoslynComponent>\s*([\s\S]*)\s*</IsRoslynComponent>")]
        public partial Regex GetProjectIsRoslynRegex();

        [GeneratedRegex(@"<ItemGroup>\s*<Reference\s*Include\s*=\s*""UnityEngine""\s*>\s*<HintPath>([\s\S]+?)</HintPath>\s*</Reference>\s*</ItemGroup>")]
        public partial Regex GetProjectUnityRefRegex();

        [GeneratedRegex(@"\s*<ItemGroup>(\s*<Content Include=""([\S]+)"">\s*<CopyToOutputDirectory>([\S]+)</CopyToOutputDirectory>\s*</Content>\s*)+</ItemGroup>\s*")]
        public partial Regex GetProjectContentItemGroupRegex();




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
            var outputType = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectOutputTypeRegex()));

            var NSLTypes = FindAllLinesByRegex(currentContent, GetProjectNSLTypes()).FirstOrDefault();

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

                projects.Add(new ProjectFileInfo(path, configurations.ToArray(), Path.GetRelativePath(this.path, Path.GetDirectoryName(path) + "/..")));

                return;
            }

            configurations.AddRange(new string[] { "Debug", "Release" });

            var sdk = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectSdkRegex()));

            bool unityOnly = hasUnityInProjectName(path);
            bool aspNetOnly = isOnlyAspNetProject(path, sdk);

            TemplateBuilder tb = new TemplateBuilder();

            var description = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectDescriptionRegex()));

            var authors = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectAuthorsRegex()));
            var suppressDependenciesWhenPacking = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectSuppressDependenciesWhenPackingRegex()));

            var isRoslyn = bool.TryParse(GetGroupValue(FindGroupsByRegex(currentContent, GetProjectIsRoslynRegex())), out var rv) && rv;

            var isTemplate = HasTemplateType(NSLProjectTypes);

            var fileContent = string.Join(Environment.NewLine, currentContent);

            var projectRefs = FindAllByRegex(fileContent, GetProjectReferenceRegex());

            //var projectRefsWithConditions = FindAllByRegex(fileContent, GetItemGroupWithConditionRegex());

            var unityRef = FindAllByRegex(fileContent, GetProjectUnityRefRegex()).FirstOrDefault();

            var contentItems = FindAllByRegex(fileContent, GetProjectContentItemGroupRegex());

            var packagesRefs = FindAllByRegex(fileContent, GetPackageReferenceRegex());

            var compileItems = FindAllByRegex(fileContent, GetCompileRegex());

            var embeddedResourceItems = FindAllByRegex(fileContent, GetEmbeddedResourceRegex());

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


                    var tf = "net8.0";

                    if (HasUnityTarget(NSLProjectTypes))
                        configurations.Clear();

                    if (HasUnitySupport(NSLProjectTypes))
                        configurations.AddRange(new[] { "UnityDebug", "Unity" });


                    if (HasAnalyzer(NSLProjectTypes) || HasAnalyzerUtils(NSLProjectTypes))
                        tf = "netstandard2.0";

                    tb.AppendLine($"<NSLProjectTypes>{string.Join(';', NSLProjectTypes)}</NSLProjectTypes>")
                    .AppendLine();


                    tb.AppendLine($"<TargetFramework>{tf}</TargetFramework>")
                      .AppendLine($"<Configurations>{string.Join(';', AvailableConfigurations)}</Configurations>")
                      .AppendLine("<AllowUnsafeBlocks>true</AllowUnsafeBlocks>")
                      .AppendLine("<Nullable>disable</Nullable>");

                    if (aspNetOnly)
                        tb.AppendLine("<IsPackable>true</IsPackable>")
                        .AppendLine("<OutputType>Library</OutputType>");

                    tb.AppendLine();

                    tb.AppendLine("<PublishRepositoryUrl>true</PublishRepositoryUrl>")
                      .AppendLine("<EmbedUntrackedSources>true</EmbedUntrackedSources>")
                      .AppendLine("<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>")
                      .AppendLine("<GenerateBindingRedirectsOutputType>true</GenerateBindingRedirectsOutputType>");

                    if (isRoslyn)
                    {
                        tb.AppendLine()
                          .AppendLine("<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>")
                          .AppendLine("<IsRoslynComponent>true</IsRoslynComponent>");
                    }

                    if (suppressDependenciesWhenPacking != null)
                    {
                        tb.AppendLine($"<SuppressDependenciesWhenPacking>{suppressDependenciesWhenPacking}</SuppressDependenciesWhenPacking>");
                    }

                    if (isTemplate)
                    {
                        tb.AppendLine("""
                            <PackageType>Template</PackageType>
                            <IncludeContentInPack>true</IncludeContentInPack>
                            <IncludeBuildOutput>false</IncludeBuildOutput>
                            <ContentTargetFolders>content</ContentTargetFolders>
                            <NoWarn>$(NoWarn);NU5128</NoWarn>
                            <NoDefaultExcludes>true</NoDefaultExcludes>
                            <LocalizeTemplates>false</LocalizeTemplates>
                            """);
                    }

                    tb.AppendLine();

                    tb.AppendLine("<Version>$(VersionSuffix)</Version>");

                    if (authors != null)
                        tb.AppendLine($"<Authors>{authors}</Authors>");
                    else
                        tb.AppendLine("<Authors>Relife87</Authors>");

                    if (description != null)
                        tb.AppendLine($"<Description>{description}</Description>");
                });


                if (isTemplate)
                    tb.AppendLine("""
                    <ItemGroup>
                        <Content Include="**\*\.template.config\template.json" />
                    </ItemGroup>
                    """);

                tb.AppendLine()
                .WritePropertyGroup("'$(Configuration)'=='Debug'", () =>
                    tb.AppendLine("<PackageId>$(MSBuildProjectName)_Debug</PackageId>")
                );

                if (HasUnitySupport(NSLProjectTypes))
                {
                    tb.AppendLine()
                    .WritePropertyGroup("'$(Configuration)'=='UnityDebug'", () =>
                    {
                        if (!HasUnityTarget(NSLProjectTypes))
                            tb.AppendLine("<AssemblyName>Unity.$(MSBuildProjectName)</AssemblyName>");

                        if (!HasAnalyzerUtils(NSLProjectTypes))
                            tb.AppendLine("<TargetFramework>netstandard2.1</TargetFramework>");

                        tb.AppendLine("<DefineConstants>DEBUG;TRACE</DefineConstants>");
                    });

                    if (!HasUnityTarget(NSLProjectTypes) || !HasAnalyzerUtils(NSLProjectTypes))
                        tb.AppendLine()
                        .WritePropertyGroup("'$(Configuration)'=='Unity'", () =>
                        {
                            if (!HasUnityTarget(NSLProjectTypes))
                                tb.AppendLine("<AssemblyName>Unity.$(MSBuildProjectName)</AssemblyName>");

                            if (!HasAnalyzerUtils(NSLProjectTypes))
                                tb.AppendLine("<TargetFramework>netstandard2.1</TargetFramework>");
                        });

                }

                //if (projectRefsWithConditions.Any())
                //    foreach (Match item in projectRefsWithConditions)
                //    {
                //        tb.AppendLine()
                //        .WriteItemGroup(item.Groups[3].Value, () =>
                //        {
                //            tb.AppendLine(item.Groups[4].Value.Trim());
                //        });
                //    }

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
                            //var pr = item.Captures.Select(x => x.Value.TrimEnd()).ToArray();
                            //foreach (var item in t0.gr)
                            //{

                            //}
                            //tb.AppendLine($"<ProjectReference {string.Join(" ", pr)} />");
                            //else
                            //    tb.AppendLine($"<ProjectReference Include=\"{p}\" OutputItemType=\"{t}\" />");

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


                if (isRoslyn)
                    tb.AppendLine().WriteItemGroup(() =>
                        tb.AppendLine("<None Include=\"$(OutputPath)\\*NSL.*.dll\" Pack=\"true\" PackagePath=\"analyzers/dotnet/cs\" Visible=\"false\" />")
                    );

                if (unityRef != null)
                {
                    tb.AppendLine()
                        .WriteItemGroup(() =>
                        {
                            tb.AppendLine("<Reference Include=\"UnityEngine\">");
                            tb.NextTab();
                            tb.AppendLine($"<HintPath>{GetGroupValue(unityRef.Groups)}</HintPath>");
                            tb.PrevTab();
                            tb.AppendLine("</Reference>");
                        });
                }

                if (contentItems.Any())
                    tb.AppendLine()
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
                    });

            });

            string v = tb.ToString();

            File.WriteAllText(path, v);

            projects.Add(new ProjectFileInfo(path, configurations.ToArray(), Path.GetRelativePath(this.path, Path.GetDirectoryName(path) + "/..")));
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

        public bool HasAnalyzerUtils(List<string> types)
            => HasAnalyzer(types) || types.Contains("AnalyzerUtils");

        public bool HasUnityTarget(List<string> types)
        {
            return types.Contains("UnityTarget");
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
