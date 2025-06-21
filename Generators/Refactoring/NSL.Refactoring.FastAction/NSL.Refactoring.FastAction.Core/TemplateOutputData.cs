using System.Collections.Generic;
namespace NSL.Refactoring.FastAction.Core
{
    internal class TemplateOutputData
    {
        public TemplateData Template { get; set; }

        public Dictionary<string, string> Values { get; set; }

        public string OutputPath { get; set; }

        public string NameSpace { get; set; }

        public AppendCodeTypeEnum AppendCodeType { get; set; }
    }
}
