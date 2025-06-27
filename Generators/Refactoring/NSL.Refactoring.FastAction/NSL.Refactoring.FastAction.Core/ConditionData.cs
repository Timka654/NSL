using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NSL.Refactoring.FastAction.Core
{
    public class ConditionData
    {
        public string[] Namespace { get; set; }

        public string[] NodeName { get; set; }

        public string[] FilePath { get; set; }

        public string[] ProjectName { get; set; }

        public string[] ProjectPath { get; set; }

        public string[] SolutionName { get; set; }

        public string[] SolutionPath { get; set; }

        public string[] PropertyDeclaration { get; set; }

        public string[] FieldDeclaration { get; set; }

        public string[] MemberType { get; set; }

        private Regex[] Regexes { get; set; }

        public bool OptionalCondition { get; set; }

        public bool OptionalExists { get; set; }

        public bool IsMatch(string[] regex, string text, out Dictionary<string, string> values)
        {
            if (Regexes == default)
            {
                Regexes = regex
                    .Select(x => new Regex(x, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    .ToArray();
            }

            foreach (var item in Regexes)
            {
                var match = item.Match(text);

                values = new Dictionary<string, string>();

                if (match.Success)
                {
                    foreach (var gname in item.GetGroupNames())
                    {
                        values[gname] = match.Groups[gname].Value;
                    }

                    return true;
                }
            }

            values = default;

            return false;
        }


    }


}
