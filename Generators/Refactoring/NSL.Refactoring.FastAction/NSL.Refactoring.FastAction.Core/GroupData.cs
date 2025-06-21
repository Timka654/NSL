using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSL.Refactoring.FastAction.Core
{
    public class GroupData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string ProjectName { get; set; }

        public string[] Actions { get; set; }

        public Project Project { get; set; }

        public string ProjectPath { get; set; } = default;

        public bool Disabled { get; set; } = false;

        Regex[] MatchRegexes { get; set; } = default;

        Regex[] UnMatchRegexes { get; set; } = default;


        public bool IsMatchAction(string id)
        {
            if (Actions == default)
                return true;

            if (MatchRegexes == default)
            {
                MatchRegexes = Actions
                    .Where(x=>x != "*" && !x.StartsWith("!"))
                    .Select(regex=> new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    .ToArray();
            }

            if (UnMatchRegexes == default)
            {
                UnMatchRegexes = Actions
                    .Where(x=>x != "*" && x.StartsWith("!"))
                    .Select(regex=> new Regex(regex.Substring(1), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    .ToArray();
            }

            bool result = Actions.Any(x=>x == "*") || MatchRegexes.Any(x => x.IsMatch(id));

            if (!result) return false;

            result = !UnMatchRegexes.Any(x => x.IsMatch(id));

            if (!result) return false;

            return true;
        }
    }


}
