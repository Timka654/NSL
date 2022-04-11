using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Extensions.NetScript
{
    public class NetScriptCompileException : Exception
    {
        public NetScriptCompileException(string message) : base(message)
        {
        }

        internal NetScriptCompileException(string message, Core core, IEnumerable<Diagnostic> exs) : base(BuildMessage(message,core,exs))
        {
            Exceptions = new string[exs.Count()];
            int i = 0;
            foreach (Diagnostic item in exs)
            {
                string ex = item.ToString();

                int si = ex.IndexOf('(') + 1;
                int ei = ex.IndexOf(',');

                int end = ex.IndexOf(')');

                if (si == -1 || ei == -1 || end == -1)
                {
                    Exceptions[i++] = ex;
                    continue;
                }

                int line = Convert.ToInt32(ex.Substring(si, ei - si)) - core.usings.Count - core.defines.Count;
                if (line <= 0)
                {
                    line = core.usings.Count + core.defines.Count + line;

                    ex = $"file:\"using_fragment\" line:{line} {ex.Substring(end + 1, ex.Length - end - 1)}";

                }
                else if (core.GlobalCodeStartLine <= line + core.usings.Count + core.defines.Count && core.GlobalCodeEndLine >= line + core.usings.Count + core.defines.Count)
                {
                    ex = $"file:\"global_fragment\" line:{line + core.usings.Count + core.defines.Count } {ex.Substring(end + 1, ex.Length - end - 1)}";
                }
                else
                {
                    var f = core.Fragments.First(x => x.Start <= line && x.End >= line);

                    ex = $"file:{f.FileName} line:{line - f.Start } {ex.Substring(end + 1, ex.Length - end - 1)}";
                }
                Exceptions[i++] = ex;
            }
        }

        private static string BuildMessage(string message, Core core, IEnumerable<Diagnostic> exs)
        {
            var Exceptions = new string[exs.Count()];
            int i = 0;
            foreach (Diagnostic item in exs)
            {
                string ex = item.ToString();

                int si = ex.IndexOf('(') + 1;
                int ei = ex.IndexOf(',');

                int end = ex.IndexOf(')');

                if (si == -1 || ei == -1 || end == -1)
                {
                    Exceptions[i++] = ex;
                    continue;
                }

                int line = Convert.ToInt32(ex.Substring(si, ei - si)) - core.usings.Count - core.defines.Count;
                if (line <= 0)
                {
                    line = core.usings.Count + core.defines.Count + line;

                    ex = $"file:\"using_fragment\" line:{line} {ex.Substring(end + 1, ex.Length - end - 1)}";

                }
                else if (core.GlobalCodeStartLine <= line + core.usings.Count + core.defines.Count && core.GlobalCodeEndLine >= line + core.usings.Count + core.defines.Count)
                {
                    ex = $"file:\"global_fragment\" line:{line + core.usings.Count + core.defines.Count } {ex.Substring(end + 1, ex.Length - end - 1)}";
                }
                else
                {
                    var f = core.Fragments.First(x => x.Start <= line && x.End >= line);

                    ex = $"file:{f.FileName} line:{line - f.Start } {ex.Substring(end + 1, ex.Length - end - 1)}";
                }
                Exceptions[i++] = ex;
            }

            return string.Join("\r\n",Exceptions.Prepend(message));
        }

        public string[] Exceptions { get; set; }
    }
}
