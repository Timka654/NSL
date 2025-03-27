namespace NSLLibProjectFileFormatter.Solution
{
    class SLNBuilder
    {
        private static SolutionProjectPath ProcessSolutionFolder(Dictionary<string, SolutionProjectPath> pathes, string pathDir)
        {
            if (!pathes.TryGetValue(pathDir, out var path))
            {
                path = new SolutionProjectPath() { Dir = pathDir.Split(Path.DirectorySeparatorChar).Last() };
                pathes.Add(pathDir, path);

                var pi = pathDir.LastIndexOf(Path.DirectorySeparatorChar);

                if (pi > -1)
                {
                    pathDir = pathDir.Substring(0, pi);

                    var cdir = ProcessSolutionFolder(pathes, pathDir);

                    cdir.Pathes.TryAdd(pathDir, path);

                    path.Parent = cdir;
                }
            }

            return path;
        }

        private static string[] Archs = new string[] {
            "Any CPU",
            "x64",
            "x86"
        };

        private const string BasicProjectTypeId = "9A19103F-16F7-4668-BE54-9A1E7A4F7556";
        private const string VsixProjectTypeId = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";
        private const string FolderProjectTypeId = "2150E333-8FDC-42A3-9474-1A3956D46DE8";

        public static void BuildSln(string solutionPath, IEnumerable<ProjectFileInfo> projects, string[] availableProfiles)
        {
            availableProfiles = availableProfiles.OrderBy(x => x).ToArray();

            var slnHeader = "Microsoft Visual Studio Solution File, Format Version 12.00\n# Visual Studio Version 17\nVisualStudioVersion = 17.12.35521.163\nMinimumVisualStudioVersion = 10.0.40219.1\n";
            var slnProjects = new List<string>();
            var slnConfigs = new List<string>();
            var slnProjectConfigs = new List<string>();
            //var folderGuids = new Dictionary<string, string>();
            var nestedProjects = new List<string>();

            Dictionary<string, SolutionProjectPath> pathes = new Dictionary<string, SolutionProjectPath>();

            List<SolutionProjectPath> cpDirs = new();

            foreach (var projectPath in projects)
            {
                string projectName = Path.GetFileNameWithoutExtension(projectPath.Path);
                string relativePath = Path.GetRelativePath(Path.GetDirectoryName(solutionPath), projectPath.Path);


                var dir = ProcessSolutionFolder(pathes, projectPath.dir);

                var proj = new SolutionProject()
                {
                    Info = projectPath,
                    Id = Guid.NewGuid(),
                    Path = relativePath,
                    Name = projectName
                };

                dir.Projects.Add(proj);
            }

            foreach (var projectPath in pathes.SelectMany(x => x.Value.Projects))
            {
                var isVsix = projectPath.Name.Contains(".Vsix");

                slnProjects.Add($@"Project(""{{{(isVsix ? VsixProjectTypeId : BasicProjectTypeId)}}}"") = ""{projectPath.Name}"", ""{projectPath.Path}"", ""{{{projectPath.UppedId}}}""
EndProject");

                foreach (var profile in availableProfiles)
                {
                    var targetProfile = profile;

                    if (isVsix)
                    {
                        targetProfile = profile.Contains("Debug") ? $"Debug" : "Release";
                    }

                    foreach (var arch in Archs)
                    {
                        slnProjectConfigs.Add($@"{{{projectPath.UppedId}}}.{profile}|{arch}.ActiveCfg = {targetProfile}|Any CPU");
                        if (projectPath.Info.Profiles.Contains(profile))
                            slnProjectConfigs.Add($@"{{{projectPath.UppedId}}}.{profile}|{arch}.Build.0 = {targetProfile}|Any CPU");
                    }
                }
            }

            foreach (var item in pathes.Values)
            {
                slnProjects.Add($@"Project(""{{{FolderProjectTypeId}}}"") = ""{item.Dir}"", ""{item.Dir}"", ""{{{item.UpperId}}}""
EndProject");
            }

            foreach (var dir in pathes.Values)
            {
                if (dir.Parent != null)
                    nestedProjects.Add($@"{{{dir.UpperId}}} = {{{dir.Parent.UpperId}}}");

                foreach (var proj in dir.Projects)
                {
                    nestedProjects.Add($@"{{{proj.UppedId}}} = {{{dir.UpperId}}}");
                }
            }

            foreach (var profile in availableProfiles)
            {
                foreach (var arch in Archs)
                {
                    slnConfigs.Add($@"{profile}|{arch} = {profile}|{arch}");
                }
            }

            var slnContent = slnHeader +
                             string.Join("\n", slnProjects) +
                             "\nGlobal\n" +
                             "\tGlobalSection(SolutionConfigurationPlatforms) = preSolution\n" +
                             string.Join("\n", slnConfigs.Select(cfg => $"\t\t{cfg}")) +
                             "\n\tEndGlobalSection\n" +
                             "\n\tGlobalSection(ProjectConfigurationPlatforms) = postSolution\n" +
                             string.Join("\n", slnProjectConfigs.Select(cfg => $"\t\t{cfg}")) +
                             "\n\tEndGlobalSection\n" +
                             "\tGlobalSection(NestedProjects) = preSolution\n" +
                             string.Join("\n", nestedProjects.Select(x => $"\t\t{x}")) +
                             "\n\tEndGlobalSection\n" +
                             "EndGlobal";

            File.WriteAllText(solutionPath, slnContent);
            Console.WriteLine($"Solution file created at: {solutionPath}");
        }
    }
}
