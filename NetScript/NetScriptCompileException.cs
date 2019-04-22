using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetScript
{
    public class NetScriptCompileException : Exception
    {
        public NetScriptCompileException(string message) : base(message)
        {

        }

        internal NetScriptCompileException(string message, Core core, IEnumerable<Diagnostic> exs) : base(message)
        {
            Exceptions = new string[exs.Count()];
            int i = 0;
            foreach (Diagnostic item in exs)
            {
                string ex = item.ToString();

                int si = ex.IndexOf('(') + 1;
                int ei = ex.IndexOf(',');

                int end = ex.IndexOf(')');

                int line = Convert.ToInt32(ex.Substring(si, ei - si)) - core.usings.Count;

                var f = core.Fragments.First(x => x.Start <= line && x.End >= line);

                ex = $"file:{f.FileName} line:{line - f.Start} {ex.Substring(end + 1, ex.Length - end - 1)}"; 

                Exceptions[i++] = ex;
            }
        }

        public string[] Exceptions { get; set; }
    }
}
