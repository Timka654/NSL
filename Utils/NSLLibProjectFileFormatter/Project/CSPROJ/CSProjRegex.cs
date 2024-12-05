using System.Text.RegularExpressions;

namespace NSLLibProjectFileFormatter.Project.CSPROJ
{
    internal partial class CSProjRegex
    {
        [GeneratedRegex(@"\s*<Project\sSdk\s*=\s*""(\S*)""\s*>")]
        public static partial Regex GetProjectSdkRegex();

        [GeneratedRegex(@"<ItemGroup>(\s*<ProjectReference(\s*(\S+)\s*=\s*""([\s\S]+?)"")\s*/>)+\s*</ItemGroup>")]
        public static partial Regex GetProjectReferenceRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<PackageReference(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</PackageReference>\s*)*</ItemGroup>)+")]
        public static partial Regex GetPackageReferenceRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<Compile(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</Compile>\s*)*</ItemGroup>)+")]
        public static partial Regex GetCompileRegex();

        [GeneratedRegex(@"(<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>(\s*<EmbeddedResource(\s*([\S]+?)\s*=\s*""([\s\S]+?)""\s*)+)(/>\s*|>(\s*<([\S]+?)>\s*([\S\s]+?)</([\S]+)>)+\s*</EmbeddedResource>\s*)*</ItemGroup>)+")]
        public static partial Regex GetEmbeddedResourceRegex();

        [GeneratedRegex(@"<ItemGroup(\s*(\S*)\s*=\s*""([\S\s]+?)"")*>([\S\s]+?)</ItemGroup>")]
        public static partial Regex GetItemGroupWithConditionRegex();

        [GeneratedRegex(@"\s*<Description>([\S\s]*)</Description>")]
        public static partial Regex GetProjectDescriptionRegex();

        [GeneratedRegex(@"\s*<RootNamespace>([\S\s]*)</RootNamespace>")]
        public static partial Regex GetProjectRootNamespaceRegex();

        [GeneratedRegex(@"\s*<OutputType>([\S\s]*)</OutputType>")]
        public static partial Regex GetProjectOutputTypeRegex();

        [GeneratedRegex(@"\s*<Authors>([\S\s]*)</Authors>")]
        public static partial Regex GetProjectAuthorsRegex();

        [GeneratedRegex(@"\s*<SuppressDependenciesWhenPacking>([\S\s]*)</SuppressDependenciesWhenPacking>")]
        public static partial Regex GetProjectSuppressDependenciesWhenPackingRegex();

        [GeneratedRegex(@"\s*<NSLProjectTypes>(([\S\s]+?)(;|\b))+</NSLProjectTypes>")]
        public static partial Regex GetProjectNSLTypes();

        [GeneratedRegex(@"\s*<IsRoslynComponent>\s*([\s\S]*)\s*</IsRoslynComponent>")]
        public static partial Regex GetProjectIsRoslynRegex();

        [GeneratedRegex(@"<ItemGroup>\s*<Reference\s*Include\s*=\s*""UnityEngine""\s*>\s*<HintPath>([\s\S]+?)</HintPath>\s*</Reference>\s*</ItemGroup>")]
        public static partial Regex GetProjectUnityRefRegex();

        [GeneratedRegex(@"\s*<ItemGroup>(\s*<Content Include=""([\S]+)"">\s*<CopyToOutputDirectory>([\S]+)</CopyToOutputDirectory>\s*</Content>\s*)+</ItemGroup>\s*")]
        public static partial Regex GetProjectContentItemGroupRegex();

    }
}
