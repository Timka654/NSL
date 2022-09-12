using System;
using System.Linq;
using System.Text;

namespace NSL.Extensions.RPC.Generator
{
    internal class CodeBuilder
    {
        private StringBuilder sb = new StringBuilder();

        public byte Tabs { get; private set; }

        public void NextTab()
            => ++Tabs;

        public void PrevTab()
        {
            if (Tabs == 0)
                throw new Exception();

            --Tabs;
        }

        public override string ToString()
            => sb.ToString().TrimEnd('\r', '\n');

        public void AppendLine()
        {
            AppendLine($"{string.Concat(Enumerable.Range(0, Tabs).Select(i => $"\t"))}");
        }

        public void AppendLine(string text)
        {
            string tabs = string.Concat(Enumerable.Range(0, Tabs).Select(i => $"\t"));

            text = text.Replace(Environment.NewLine, $"{Environment.NewLine}{tabs}");

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
    }
}
