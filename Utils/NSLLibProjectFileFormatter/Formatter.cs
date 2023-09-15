using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        [GeneratedRegex(@"<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>([\S\s]+?)</ItemGroup>")]
        public partial Regex GetItemGroupWithConditionRegex();

        [GeneratedRegex(@"\s*<Description>([\S\s]*)</Description>")]
        public partial Regex GetProjectDescriptionRegex();

        [GeneratedRegex(@"\s*<OutputType>([\S\s]*)</OutputType>")]
        public partial Regex GetProjectOutputTypeRegex();

        [GeneratedRegex(@"\s*<Authors>([\S\s]*)</Authors>")]
        public partial Regex GetProjectAuthorsRegex();

        [GeneratedRegex(@"\s*<NSLProjectTypes>(([\S\s]+?)(;|\b))+</NSLProjectTypes>")]
        public partial Regex GetProjectNSLTypes();

        [GeneratedRegex(@"\s*<IsRoslynComponent>\s*([\s\S]*)\s*</IsRoslynComponent>")]
        public partial Regex GetProjectIsRoslynRegex();

        [GeneratedRegex(@"<ItemGroup>\s*<Reference\s*Include\s*=\s*""UnityEngine""\s*>\s*<HintPath>([\s\S]+?)</HintPath>\s*</Reference>\s*</ItemGroup>")]
        public partial Regex GetProjectUnityRefRegex();

        [GeneratedRegex(@"\s*<ItemGroup>(\s*<Content Include=""([\S]+)"">\s*<CopyToOutputDirectory>([\S]+)</CopyToOutputDirectory>\s*</Content>\s*)+</ItemGroup>\s*")]
        public partial Regex GetProjectContentItemGroupRegex();

        private readonly string path;

        private readonly string UnityRefPath;

        public Formatter(string path)
        {
            this.path = path;
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
        }

        void processDirectory(DirectoryInfo di)
        {
            foreach (var item in di.GetFiles("*.csproj", SearchOption.AllDirectories))
            {
                if (item.Name.Contains(".Test") || item.Name.Contains("Example"))
                    continue;

                BuildNewProjectFile(item.FullName, File.ReadAllLines(item.FullName).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
            }
        }


        void BuildNewProjectFile(string path, string[] currentContent)
        {
            var outputType = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectOutputTypeRegex()));

            var NSLTypes = FindAllLinesByRegex(currentContent, GetProjectNSLTypes()).FirstOrDefault();

            List<string> NSLProjectTypes = NSLTypes == null ? new List<string>() : NSLTypes[2].Captures.Select(x => x.Value).ToList();

            if (Equals(outputType, "Exe") || HasTest(NSLProjectTypes) || HasExternal(NSLProjectTypes))
                return;

            bool unityOnly = hasUnityInProjectName(path);

            TemplateBuilder tb = new TemplateBuilder();

            var sdk = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectSdkRegex()));

            var description = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectDescriptionRegex()));

            var authors = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectAuthorsRegex()));

            var isRoslyn = bool.TryParse(GetGroupValue(FindGroupsByRegex(currentContent, GetProjectIsRoslynRegex())), out var rv) && rv;

            var fileContent = string.Join(Environment.NewLine, currentContent);

            var projectRefs = FindAllByRegex(fileContent, GetProjectReferenceRegex());

            //var projectRefsWithConditions = FindAllByRegex(fileContent, GetItemGroupWithConditionRegex());

            var unityRef = FindAllByRegex(fileContent, GetProjectUnityRefRegex()).FirstOrDefault();

            var contentItems = FindAllByRegex(fileContent, GetProjectContentItemGroupRegex());

            var packagesRefs = FindAllByRegex(fileContent, GetPackageReferenceRegex());

            tb.WriteProjectRoot(sdk, () =>
            {
                tb.WritePropertyGroup(() =>
                {

                    if (!NSLProjectTypes.Any())
                    {
                        if (unityOnly)
                        {
                            NSLProjectTypes.Add("UnityTarget");
                            NSLProjectTypes.Add("UnitySupport");
                        }
                        else if (isOnlyAspNetProject(path, sdk))
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


                    List<string> configurations = new List<string> { "Debug", "Release", "DebugExamples" };

                    var tf = "net7.0";

                    if (!unityOnly)
                    {
                        configurations.AddRange(new[] { "Unity", "UnityDebug" });
                    }
                    else if (!isOnlyAspNetProject(path, sdk))
                    {
                        configurations = new List<string>(new[] { "Unity", "UnityDebug" });
                        tf = "netstandard2.1";
                    }


                    if (HasAnalyzer(NSLProjectTypes) || HasAnalyzerUtils(NSLProjectTypes))
                        tf = "netstandard2.0";

                    tb.AppendLine($"<NSLProjectTypes>{string.Join(';', NSLProjectTypes)}</NSLProjectTypes>")
                    .AppendLine();


                    tb.AppendLine($"<TargetFramework>{tf}</TargetFramework>")
                      .AppendLine($"<Configurations>{string.Join(';', configurations)}</Configurations>")
                      .AppendLine("<AllowUnsafeBlocks>true</AllowUnsafeBlocks>")
                      .AppendLine("<Nullable>disable</Nullable>");

                    if (isOnlyAspNetProject(path, sdk))
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

                    tb.AppendLine();

                    tb.AppendLine("<Version>$(VersionSuffix)</Version>");

                    if (authors != null)
                        tb.AppendLine($"<Authors>{authors}</Authors>");
                    else
                        tb.AppendLine("<Authors>Relife87</Authors>");

                    if (description != null)
                        tb.AppendLine($"<Description>{description}</Description>");
                });

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

                            tb.AppendLine($"<PackageReference {string.Join(" ", packageProps.Select(x => x.Value.Trim()).ToArray())} {(packageBodyProps.Any() ? ">" : "/>")}");

                            if (packageBodyProps.Any())
                            {
                                tb.NextTab();

                                foreach (Capture bodyProp in packageBodyProps)
                                {
                                    tb.AppendLine(bodyProp.Value.Trim());
                                }

                                tb.PrevTab()
                                .AppendLine("</PackageReference>");
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

            return;


            // todo write
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
            return types.Contains("UnityTarget");
        }

        private bool hasUnityInProjectName(string path)
        {
            var name = new FileInfo(path).Name;

            return name.Contains("Unity", StringComparison.OrdinalIgnoreCase);
        }

        private bool isOnlyAspNetProject(string path, string sdk)
        {
            var name = new FileInfo(path).Name;

            return name.Contains("AspNet", StringComparison.OrdinalIgnoreCase) || sdk.Equals("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase);
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
