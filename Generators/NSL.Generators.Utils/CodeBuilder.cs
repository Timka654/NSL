using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NSL.Generators.Utils
{
    public class CodeBuilder
    {
        private StringBuilder sb = new StringBuilder();

        public byte Tabs { get; private set; }

        public void NextTab()
            => ++Tabs;

        public void PrevTab()
        {
            if (Tabs == 0)
            {
                if (!Debugger.IsAttached)
                    Debugger.Launch();
                else
                    Debugger.Break();

                throw new Exception();
            }
            --Tabs;
        }

        public override string ToString()
            => sb.ToString().TrimEnd('\r', '\n');

        public string GetText()
            => ToString();

        public void AppendLine()
        {
            AppendLine($"{string.Concat(Enumerable.Range(0, Tabs).Select(i => $"\t"))}");
        }

        public void AppendLine(string text)
        {
            string tabs = string.Concat(Enumerable.Range(0, Tabs).Select(i => $"\t"));

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            text = text.Replace(Environment.NewLine, $"{Environment.NewLine}{tabs}");
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

            sb.AppendLine($"{tabs}{text}");
        }

        public void AppendLine(byte span, string text)
        {
            AppendLine($"{string.Concat(Enumerable.Range(0, span).Select(i => $"\t"))}{text}");
        }

        public void AppendLine(byte span)
        {
            AppendLine($"{string.Concat(Enumerable.Range(0, span).Select(i => $"\t"))}");
        }

        public void AppendLineFormat(string format, params object[] args)
        {
            AppendLine(string.Format(format, args));
        }

        public void AppendLineFormat(byte span, string format, params object[] args)
        {
            AppendLine(span, string.Format(format, args));
        }

        public void AppendComment(Action<CodeBuilder> commentAction)
        {
            AppendLine("/*");

            commentAction(this);

            AppendLine("*/");
        }

        public void AppendComment(Action commentAction)
            => AppendComment(builder => commentAction());
    }
}
