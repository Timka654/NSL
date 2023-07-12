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

        [GeneratedRegex(@"<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")+>([\S\s]+?)</ItemGroup>")]
        public partial Regex GetItemGroupWithConditionRegex();

        [GeneratedRegex(@"\s*<Description>([\S\s]*)</Description>")]
        public partial Regex GetProjectDescriptionRegex();

        [GeneratedRegex(@"\s*<OutputType>([\S\s]*)</OutputType>")]
        public partial Regex GetProjectOutputTypeRegex();

        [GeneratedRegex(@"\s*<Authors>([\S\s]*)</Authors>")]
        public partial Regex GetProjectAuthorsRegex();

        [GeneratedRegex(@"\s*<IsRoslynComponent>\s*([\s\S]*)\s*</IsRoslynComponent>")]
        public partial Regex GetProjectIsRoslynRegex();

        [GeneratedRegex(@"<ItemGroup>\s*<Reference\s*Include\s*=\s*""UnityEngine""\s*>\s*<HintPath>([\s\S]+?)</HintPath>\s*</Reference>\s*</ItemGroup>")]
        public partial Regex GetProjectUnityRefRegex();

        private readonly string path;

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

            if (Equals(outputType, "Exe"))
                return;

            bool unityOnly = isOnlyUnityProject(path);

            TemplateBuilder tb = new TemplateBuilder();

            var sdk = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectSdkRegex()));

            var description = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectDescriptionRegex()));
            var authors = GetGroupValue(FindGroupsByRegex(currentContent, GetProjectAuthorsRegex()));

            var isRoslyn = bool.TryParse(GetGroupValue(FindGroupsByRegex(currentContent, GetProjectIsRoslynRegex())), out var rv) && rv;

            var fileContent = string.Join(Environment.NewLine, currentContent);

            var projectRefs = FindAllByRegex(fileContent, GetProjectReferenceRegex());

            var projectRefsWithConditions = FindAllByRegex(fileContent, GetItemGroupWithConditionRegex());

            var unityRef = FindAllByRegex(fileContent, GetProjectUnityRefRegex()).FirstOrDefault();

            tb.WriteProjectRoot(sdk, () =>
            {
                tb.WritePropertyGroup(() =>
                {
                    List<string> configurations = new List<string> { "Debug", "Release", "DebugExamples" };

                    var tf = "net7.0";

                    if (!unityOnly)
                    {
                        configurations.AddRange(new[] { "Unity", "UnityDebug" });
                    }
                    else if (!isOnlyAspNetProject(path, sdk))
                    {
                        configurations = new List<string>(new[] { "Unity", "UnityDebug" });
                        tf = "netstandard2.0";
                    }


                    if (isRoslyn)
                        tf = "netstandard2.0";

                    tb.AppendLine($"<TargetFramework>{tf}</TargetFramework>")
                      .AppendLine($"<Configurations>{string.Join(';', configurations)}</Configurations>")
                      .AppendLine("<AllowUnsafeBlocks>true</AllowUnsafeBlocks>")
                      .AppendLine("<Nullable>disable</Nullable>");

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

                tb.AppendLine()
                .WritePropertyGroup("'$(Configuration)'=='UnityDebug'", () =>
                {
                    if (!unityOnly)
                        tb.AppendLine("<AssemblyName>Unity.$(MSBuildProjectName)</AssemblyName>");
                    if (!isRoslyn)
                        tb.AppendLine("<TargetFramework>netstandard2.0</TargetFramework>");
                    tb.AppendLine("<DefineConstants>DEBUG;TRACE</DefineConstants>");
                });

                if (!unityOnly && !isRoslyn)
                    tb.AppendLine()
                    .WritePropertyGroup("'$(Configuration)'=='Unity'", () =>
                    {
                        if (!unityOnly)
                            tb.AppendLine("<AssemblyName>Unity.$(MSBuildProjectName)</AssemblyName>");
                        if (!isRoslyn)
                            tb.AppendLine("<TargetFramework>netstandard2.0</TargetFramework>");
                    });

                if (projectRefsWithConditions.Any())
                    foreach (Match item in projectRefsWithConditions)
                    {
                        tb.AppendLine()
                        .WriteItemGroup(item.Groups[3].Value, () =>
                        {
                            tb.AppendLine(item.Groups[4].Value);
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
                            tb.AppendLine($"<HintPath>{GetGroupValue(unityRef)}</HintPath>");
                            tb.PrevTab();
                        });
                }

            });

            string v = tb.ToString();

            File.WriteAllText(path, v);

            return;


            // todo write
        }

        private bool isOnlyUnityProject(string path)
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
