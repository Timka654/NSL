﻿using System.Text;

namespace NSLLibProjectFileFormatter
{
    class TemplateBuilder
    {
        private byte tab;

        private StringBuilder sb = new StringBuilder();

        public TemplateBuilder NextTab() { ++tab; return this; }

        public TemplateBuilder PrevTab() { --tab; return this; }

        public TemplateBuilder AppendLine()
        {
            sb.AppendLine();
            return this;
        }


        public TemplateBuilder AppendLine(string content)
        {
            var pref = string.Concat(Enumerable.Repeat('\t', tab).ToArray());

            var lines = content.Split(Environment.NewLine).Select(x => $"{pref}{x}").ToArray();

            sb.AppendLine(string.Join(Environment.NewLine, lines));
            return this;
        }

        public override string ToString()
            => sb.ToString();



        public TemplateBuilder WriteProjectRoot(string sdk, Action body)
        {
            AppendLine($"<Project Sdk=\"{sdk}\">")
                .AppendLine()
                .NextTab();

            body();

            PrevTab()
                .AppendLine()
                .AppendLine("</Project>");

            return this;
        }

        public TemplateBuilder WritePropertyGroup(Action body)
        {
            AppendLine($"<PropertyGroup>")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</PropertyGroup>");

            return this;
        }

        public TemplateBuilder WriteItemGroup(Action body)
        {
            AppendLine($"<ItemGroup>")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</ItemGroup>");

            return this;
        }
        public TemplateBuilder WriteItemGroup(IEnumerable<string> props, Action body)
        {
            if (props?.Any() != true)
                return WriteItemGroup(body);

            AppendLine($"<ItemGroup {string.Join(" ", props)}>")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</ItemGroup>");

            return this;
        }

        public TemplateBuilder WriteItemGroup(string condition, Action body)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return WriteItemGroup(body);
            }

            AppendLine($"<ItemGroup Condition=\"{condition}\">")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</ItemGroup>");

            return this;
        }

        public TemplateBuilder WritePropertyGroup(string condition, Action body)
        {
            AppendLine($"<PropertyGroup Condition=\"{condition}\">")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</PropertyGroup>");

            return this;
        }

    }
}