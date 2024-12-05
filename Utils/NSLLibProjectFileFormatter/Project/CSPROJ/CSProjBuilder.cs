using System.Text;

namespace NSLLibProjectFileFormatter.Project.CSPROJ
{
    class CSProjBuilder
    {
        private byte tab;

        private StringBuilder sb = new StringBuilder();

        public CSProjBuilder NextTab() { ++tab; return this; }

        public CSProjBuilder PrevTab() { --tab; return this; }

        public CSProjBuilder AppendLine()
        {
            sb.AppendLine();

            return this;
        }

        public CSProjBuilder AppendLine(bool writeCondition)
        {
            if (writeCondition)
                return AppendLine();

            return this;
        }


        public CSProjBuilder AppendLine(string content)
        {
            var pref = string.Concat(Enumerable.Repeat('\t', tab).ToArray());

            var lines = content.Split(Environment.NewLine).Select(x => $"{pref}{x}").ToArray();

            sb.AppendLine(string.Join(Environment.NewLine, lines));
            return this;
        }

        public override string ToString()
            => sb.ToString();



        public CSProjBuilder WriteProjectRoot(string sdk, Action body)
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

        public CSProjBuilder WritePropertyGroup(Action body)
        {
            AppendLine($"<PropertyGroup>")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</PropertyGroup>");

            return this;
        }

        public CSProjBuilder WriteItemGroup(Action body)
        {
            AppendLine($"<ItemGroup>")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</ItemGroup>");

            return this;
        }

        public CSProjBuilder WriteTarget(string name, Action body, bool writeCondition)
        {
            if (writeCondition)
                return WriteTarget(name, body);

            return this;
        }

        public CSProjBuilder WriteTarget(string name, Action body)
        {
            AppendLine($"<Target Name=\"{name}\">")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</Target>");

            return this;
        }

        public CSProjBuilder WriteItemGroup(Action body, bool writeCondition)
        {
            if (writeCondition)
                return WriteItemGroup(body);

            return this;
        }

        public CSProjBuilder WriteItemGroup(IEnumerable<string> props, Action body)
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

        public CSProjBuilder WriteItemGroup(string condition, Action body)
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

        public CSProjBuilder WritePropertyGroup(string condition, Action body)
        {
            AppendLine($"<PropertyGroup Condition=\"{condition}\">")
                .NextTab();

            body();

            PrevTab()
                .AppendLine("</PropertyGroup>");

            return this;
        }

        public CSProjBuilder WritePropertyItem(string name, object value)
        {
            AppendLine($"<{name}>{value}</{name}>");

            return this;
        }

        public CSProjBuilder WritePropertyItem(string name, bool value)
        {
            AppendLine($"<{name}>{(value == true ? "true" : "false")}</{name}>");

            return this;
        }

        public CSProjBuilder WritePropertyItem(string name, object value, bool writeCondition)
        {
            if (writeCondition)
                WritePropertyItem(name, value);

            return this;
        }

        public CSProjBuilder WritePropertyItem(string name, bool value, bool writeCondition)
        {
            if (writeCondition)
                WritePropertyItem(name, value);

            return this;
        }

    }
}