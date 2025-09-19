using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSL.Generators.Utils
{
    public class CodeBuilder
    {
        private StringBuilder sb = new StringBuilder();

        public int Length => sb.Length;

        public byte Tabs { get; private set; }

        public void NextTab()
            => ++Tabs;

        public void PrevTab()
        {
            if (Tabs == 0)
            {
                return;
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

        public void AppendComment(string line)
        {
            AppendLine($"/* {line} */");
        }

        public void AppendSummary(Action<CodeBuilder> commentAction)
        {
            AppendSummaryLine("<summary>");

            commentAction(this);

            AppendSummaryLine("</summary>");
        }

        public void AppendSummaryLine(string line)
        {
            AppendLine($"/// {line}");
        }

        public void AppendSummaryLines(params string[] lines)
        {
            foreach (var item in lines)
            {
                AppendSummaryLine(item);
            }
        }

        public void AppendComment(Action commentAction)
            => AppendComment(builder => commentAction());

        public void AppendSummary(Action commentAction)
            => AppendSummary(builder => commentAction());


        public void AppendTabContent(Action tabAction)
        {
            NextTab();

            tabAction();

            PrevTab();
        }

        public void AppendBodyTabContent(Action tabAction)
        {
            AppendLine("{");
            AppendTabContent(tabAction);
            AppendLine("}");
        }

        public void CreateClass(Action<CodeBuilder> bodyBuild, string name, string @namespace = null, IEnumerable<string> requiredUsings = null, Action<CodeBuilder> beforeClassDef = null)
        {
            var body = new CodeBuilder();

            bodyBuild(body);

            //var @namespace = classDecl.Parent as NamespaceDeclarationSyntax;

            //var usings = classDecl.GetTypeClassUsingDirectives();

            //AddUsings(usings);

            if (requiredUsings != null)
            {
                //requiredUsings = requiredUsings.Except(usings);

                AddUsings(requiredUsings);
            }

            AppendLine();

            //var genericParams = classDecl.GetTypeGenericParameters();

            //var declTypeLine = "";

            //if (genericParams?.Any() == true)
            //    declTypeLine = $"<{string.Join(",", genericParams)}>";

            //declTypeLine = $"{classDecl.GetClassFullModifier()} class {classDecl.GetClassName()}{declTypeLine}";

            var declTypeLine = $"public class {name}";


            if (@namespace != null)
            {
                AppendLine($"namespace {@namespace}");
                AppendLine("{");

                NextTab();
            }
            beforeClassDef?.Invoke(this);
            AppendLine(declTypeLine);

            //NextTab();

            //foreach (var c in classDecl.ConstraintClauses)
            //{
            //    AppendLine(c.ToString());
            //}

            //PrevTab();

            AppendLine("{");

            NextTab();

            AppendLine(body.ToString());

            PrevTab();

            AppendLine("}");

            PrevTab();

            if (@namespace != null)
            {
                PrevTab();

                AppendLine("}");
            }
        }

        public void CreatePartialClass(TypeDeclarationSyntax classDecl, Action<CodeBuilder> bodyBuild, IEnumerable<string> requiredUsings = null, Action<CodeBuilder> beforeClassDef = null)
        {
            var body = new CodeBuilder();

            bodyBuild(body);

            var @namespace = classDecl.Parent as NamespaceDeclarationSyntax;

            var usings = classDecl.GetTypeClassUsingDirectives();

            AddUsings(usings);

            if (requiredUsings != null)
            {
                requiredUsings = requiredUsings.Except(usings);

                AddUsings(requiredUsings);
            }

            AppendLine();

            var genericParams = classDecl.GetTypeGenericParameters();

            var declTypeLine = "";

            if (genericParams?.Any() == true)
                declTypeLine = $"<{string.Join(",", genericParams)}>";


            string type = string.Empty;

            if (classDecl is ClassDeclarationSyntax)
                type = "class";
            else if (classDecl is InterfaceDeclarationSyntax)
                type = "interface";
            else if (classDecl is StructDeclarationSyntax)
                type = "struct";

            declTypeLine = $"{classDecl.GetClassFullModifier().Replace("abstract ", string.Empty)} {type} {classDecl.GetClassName()}{declTypeLine}";


            if (@namespace != null)
            {
                AppendLine($"namespace {@namespace.Name.ToString()}");
                AppendLine("{");

                NextTab();
            }
            beforeClassDef?.Invoke(this);
            AppendLine(declTypeLine);

            NextTab();

            foreach (var c in classDecl.ConstraintClauses)
            {
                AppendLine(c.ToString());
            }

            PrevTab();

            AppendLine("{");

            NextTab();

            AppendLine(body.ToString());

            PrevTab();

            AppendLine("}");

            PrevTab();

            if (@namespace != null)
            {
                PrevTab();

                AppendLine("}");
            }
        }

        public void CreateStaticClass(TypeDeclarationSyntax classDecl, string className, Action bodyBuild, IEnumerable<string> requiredUsings = null, string @namespace = null, Action<CodeBuilder> beforeClassDef = null)
        {
            if (@namespace == null)
                @namespace = classDecl.TryGetNamespace();

            var usings = classDecl.GetTypeClassUsingDirectives();

            AddUsings(usings);

            if (requiredUsings != null)
            {
                requiredUsings = requiredUsings.Except(usings);

                AddUsings(requiredUsings);
            }

            AppendLine();

            var genericParams = classDecl.GetTypeGenericParameters();

            var declTypeLine = "";

            if (genericParams?.Any() == true)
                declTypeLine = $"<{string.Join(",", genericParams)}>";

            string type = string.Empty;

            if (classDecl is ClassDeclarationSyntax)
                type = "class";
            else if (classDecl is InterfaceDeclarationSyntax)
                type = "interface";
            else if (classDecl is StructDeclarationSyntax)
                type = "struct";

            declTypeLine = $"{classDecl.GetClassFullModifier(new string[] { "static" }, new string[] { "partial", "abstract" })} {type} {className}{declTypeLine}";

            if (@namespace != null)
            {
                AppendLine($"namespace {@namespace}");
                AppendLine("{");

                NextTab();
            }

            beforeClassDef.Invoke(this);
            AppendLine(declTypeLine);

            NextTab();

            foreach (var c in classDecl.ConstraintClauses)
            {
                AppendLine(c.ToString());
            }

            PrevTab();

            AppendLine("{");

            NextTab();

            bodyBuild();

            PrevTab();

            AppendLine("}");

            PrevTab();

            if (@namespace != null)
            {
                PrevTab();

                AppendLine("}");
            }
        }


        public void AddUsings(IEnumerable<string> usings)
        {
            foreach (var item in usings)
            {
                AddUsing(item);
            }
        }

        public void AddUsing(string @using)
            => AppendLine($"using {@using};");

    }
}
