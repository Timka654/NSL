using System;
using System.Collections.Generic;

namespace NSL.Refactoring.FastAction.Core
{
    public class TemplateData
    {
        public string Name { get; set; }

        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();

        public string[] Usings { get; set; } = Array.Empty<string>();

        public string[] Clone { get; set; }

        public TemplateFileData[] Files { get; set; }

        public int ParentDepth { get; set; } = 0;

        public string OutputType { get; set; }
    }


}
