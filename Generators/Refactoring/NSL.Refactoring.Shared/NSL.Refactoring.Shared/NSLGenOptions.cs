using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NSL.Refactoring.Shared
{
    internal class NSLGenOptions
    {
        public static Dictionary<string, string> TryLoadOptions(Project proj)
        {
            Dictionary<string, string> options = null;

            var sol_dir = Path.GetDirectoryName(proj.Solution.FilePath);

            var confPath = Path.Combine(sol_dir, "NSLGen.options");

            if (File.Exists(confPath))
            {
                options = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));
            }

            var proj_dir = Path.GetDirectoryName(proj.FilePath);

            confPath = Path.Combine(proj_dir, "NSLGen.options");

            if (File.Exists(confPath))
            {
                var poptions = File.ReadAllLines(confPath).Select(x => x.Split('=')).ToDictionary(x => x[0], x => string.Join("=", x.Skip(1)));

                if (options == null)
                    options = poptions;
                else
                    foreach (var item in poptions)
                    {
                        options[item.Key] = item.Value;
                    }
            }

            return options;
        }
    }
}
