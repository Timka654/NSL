using System;
using System.Collections.Generic;

namespace NSL.Refactoring.FastAction.Core
{
    public class ActionData
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool GenerateSummaryRefLink { get; set; }

        public bool GenerateUsingRefLink { get; set; } = true;

        public string OutputType { get; set; } = "files";

        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public TemplateData[] Template { get; set; }

        public string[] Types { get; set; }

        public ConditionData[] Conditions { get; set; } = Array.Empty<ConditionData>();

        public string[] Errors { get; set; }

        public string OutputRelativePath { get; set; } = default;

        public string ActionPath { get; set; } = "$group_name$\\\\\\$action_name$";

    }
}
