using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NSL.Refactoring.FastAction.Core
{
    public class ConditionData
    {
        public string Namespace { get; set; }

        public string NodeName { get; set; }

        public string FilePath { get; set; }

        public string ProjectName { get; set; }

        public string ProjectPath { get; set; }

        public string SolutionName { get; set; }

        public string SolutionPath { get; set; }

        public string PropertyDeclaration { get; set; }

        public string FieldDeclaration { get; set; }

        public Regex Regex { get; private set; } = default;

        public bool Optional { get; set; }


        public bool IsMatch(string regex, string text, out Dictionary<string, string> values)
        {
            if (Regex == default)
            {
                Regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            var match = Regex.Match(text);

            values = new Dictionary<string, string>();

            if (match.Success)
            {
                foreach (var gname in Regex.GetGroupNames())
                {
                    values[gname] = match.Groups[gname].Value;
                }
            }

            return match.Success;
        }

    }


}
