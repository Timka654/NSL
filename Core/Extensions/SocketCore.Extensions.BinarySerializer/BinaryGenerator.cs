using NSL.Extensions.BinarySerializer.Attributes;
using NSL.SocketCore.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NSL.SocketCore.Extensions.BinarySerializer
{
    internal static class DictionaryExtensions
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.ContainsKey(key))
                return false;
            d.Add(key, value);
            return true;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
   (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

    }

    //[Generator]
    public class BinaryGenerator /*: ISourceGenerator*/
    {
        public static string Generate()
        {
            return Generate(Assembly.GetCallingAssembly());
        }

        private static void LoadDefaultTypes()
        {
            var l = new List<Type>();

            var tout = typeof(OutputPacketBuffer);
            var tin = typeof(InputPacketBuffer);


            foreach (var item in tout.GetMethods()
                .Where(x => x.Name.StartsWith("Write") && x.GetParameters().Length == 1)
                .Select(x => new KeyValuePair<Type, string>(x.GetParameters()[0].ParameterType, $"p.{x.Name}({{value}})")))
            {
                WriteFragmentsDefaultTypes.TryAdd(item.Key, item.Value);
                if(!l.Contains(item.Key))
                l.Add(item.Key);
            }

            foreach (var item in tin.GetMethods()
                .Where(x => x.Name.StartsWith("Read") && x.GetParameters().Length == 0)
                .Select(x => new KeyValuePair<Type, string>(x.ReturnType, $"p.{x.Name}()")))
            {
                ReadFragmentsDefaultTypes.TryAdd(item.Key, item.Value);
                if (!l.Contains(item.Key))
                    l.Add(item.Key);
            }

            ExistsDefaultTypes = l;
        }

        private static List<Type> ExistsDefaultTypes;
        private static Dictionary<Type, string> ReadFragmentsDefaultTypes = new Dictionary<Type, string>() {
            { typeof(string), "p.ReadString32()" },
        };
        private static Dictionary<Type, string> WriteFragmentsDefaultTypes = new Dictionary<Type, string>()
        {
            { typeof(string), "p.WriteString32({value})" }
        };


        public static string Generate(Assembly assembly)
        {
            var types = assembly.GetTypes().Select(x => new { attr = x.GetCustomAttributes<BinaryNetworkTypeAttribute>().FirstOrDefault(), type = x }).Where(x => x.attr != null).OrderBy(x=>x.type.Name).ToArray();

            LoadDefaultTypes();

            LoadedTypes.Clear();
            ExistsNamespaces.Clear();
            Fragments.Clear();

            ExistsNamespaces.Add("System");
            ExistsNamespaces.Add("SocketCore.Utils");
            ExistsNamespaces.Add("SocketCore.Utils.Buffer");
            //sb.AppendLine("using SocketCore.Utils.Buffer;")
            foreach (var item in types)
            {
                LoadType(item.type);
            }
            return BuildScript();
        }

        private static string BuildScript()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Join("\r\n", ExistsNamespaces.Select(x => $"using {x};")));

            sb.AppendLine();

            sb.AppendLine($"public static class BuilderStorageType");
            sb.AppendLine("{");

            foreach (var item in Fragments)
            {
                sb.AppendLine(item.ToString());
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static List<Type> LoadedTypes = new List<Type>();

        private static List<string> ExistsNamespaces = new List<string>();

        private static List<StringBuilder> Fragments = new List<StringBuilder>();

        private static void AddNameSpace(Type t)
        {
            if (ExistsNamespaces.Contains(t.Namespace))
                return;
            ExistsNamespaces.Add(t.Namespace);
        }

        private static bool AddClass(Type t)
        {
            if (LoadedTypes.Contains(t))
                return false;

            LoadedTypes.Add(t);
            return true;
        }

        private static void LoadType(Type t)
        {
            if (!AddClass(t))
                return;

            AddNameSpace(t);

            var schemes = t.GetProperties()
                .Select(x => new { scheme = x.GetCustomAttributes<BinarySchemeAttribute>(), property = x })
                .Where(x => x.scheme.Any())
                .SelectMany(x => x.scheme.Select(s => new { s.Scheme, x.property }))
                .OrderBy(x => x.property.Name)
                .GroupBy(x => x.Scheme).ToList();

            foreach (var s in schemes)
            {
                if (s.Any() == false)
                    continue;
                StringBuilder sr = new StringBuilder();
                StringBuilder sw = new StringBuilder();

                sr.AppendLine($"\tpublic static {t.Name} Read{t.Name}_{s.Key}(this InputPacketBuffer p) {{");

                sr.AppendLine($"\t\t{t.Name} result = new {t.Name}();");

                sw.AppendLine($"\tpublic static void Write{t.Name}_{s.Key}(this {t.Name} value, OutputPacketBuffer p) {{");

                foreach (var p in s.DistinctBy(x=>x.property.Name))
                {
                    WriteProp(sr,sw, s.Key, p.property);
                }

                sr.AppendLine("\t\treturn result;");
                sr.AppendLine("\t}");
                sw.AppendLine("\t}");

                var tr = sr.ToString();
                var tw = sw.ToString();

                Fragments.Add(sr);
                Fragments.Add(sw);
            }
            //InputPacketBuffer b = new InputPacketBuffer();
            //OutputPacketBuffer bo = new OutputPacketBuffer();
        }

        private static string GetReadClassFragment(Type t, string scheme) => $"p.ReadNullableClass<{t.Name}>(()=>p.Read{t.Name}_{scheme}())";
        private static string GetReadClassListFragment(Type t, string scheme) => $"p.ReadNullableClass<List<{t.Name}>>(()=>p.Read{t.Name}List_{scheme}())";
        private static string GetReadClassMapFragment(Type t, Type t2, string scheme) => $"p.ReadNullableClass<Dictionary<{t.Name},{t2.Name}>>(()=>p.Read{t.Name}{t2.Name}Map_{scheme}())";
        private static string GetWriteClassFragment(Type t, string scheme, string valuePath) => $"p.WriteNullableClass<{t.Name}>({valuePath},()=>{valuePath}.Write{t.Name}_{scheme}(p))";
        private static string GetWriteClassListFragment(Type t, string scheme, string valuePath) => $"p.WriteNullableClass<List<{t.Name}>>({valuePath},()=>{valuePath}.Write{t.Name}List_{scheme}(p))";
        private static string GetWriteClassMapFragment(Type t, Type t2, string scheme, string valuePath) => $"p.WriteNullableClass<Dictionary<{t.Name},{t2.Name}>>({valuePath}, ()=>{valuePath}.Write{t.Name}{t2.Name}Map_{scheme}(p))";

        private static string GetReadPrimitiveFragment(Type t) => $"{ReadFragmentsDefaultTypes[t]}";
        private static string GetWritePrimitiveFragment(Type t, string valuePath) => $"{WriteFragmentsDefaultTypes[t].Replace("{value}", $"value.{valuePath}")}";

        private static string GetWritePrimitiveFragment(Type t) => $"{WriteFragmentsDefaultTypes[t]}";

        private static void WriteProp(StringBuilder sr, StringBuilder sw, string scheme, PropertyInfo p)
        {
            var propType = p.PropertyType;

            sr.AppendLine(GetReadCode(scheme, propType, $"result.{p.Name}"));
            sw.AppendLine(GetWriteCode(scheme, propType, $"value.{p.Name}"));
        }

        private static string FormatReadValue(string code, string valuePath)
        {
            if (string.IsNullOrEmpty(valuePath))
                return code;
            return $"{valuePath} = {code};";
        }

        private static string FormatWriteValue(string code, string valuePath)
        {
            if (string.IsNullOrEmpty(valuePath))
                return code;
            return $"{code.Replace("{value}", valuePath)};";
        }

        private static string GetReadCode(string scheme, Type propType, string valuePath)
        {
            Type[] tempTypes = null;

            bool nvaluePath = string.IsNullOrEmpty(valuePath);


            if (propType.IsClass && !ExistsDefaultTypes.Contains(propType))
            {
                if (typeof(System.Collections.IList).IsAssignableFrom(propType))
                {
                    tempTypes = propType.GetGenericArguments();
                    LoadType(tempTypes[0]);
                    LoadListType(tempTypes[0], scheme);

                    return FormatReadValue(GetReadClassListFragment(tempTypes[0], scheme), $"\t\t{valuePath}");
                }
                else if (typeof(System.Collections.IDictionary).IsAssignableFrom(propType))
                {
                    tempTypes = propType.GetGenericArguments();

                    LoadType(tempTypes[0]);
                    LoadType(tempTypes[1]);

                    LoadDictType(tempTypes[0], tempTypes[1], scheme);

                    return FormatReadValue(GetReadClassMapFragment(tempTypes[0], tempTypes[1], scheme), $"\t\t{valuePath}");

                }
                else
                {
                    LoadType(propType);

                    return $"\t\t{FormatReadValue(GetReadClassFragment(propType, scheme), valuePath)}";
                }
            }
            else if (propType.IsEnum)
            {
                AddNameSpace(propType);
                var realType = propType.GetEnumUnderlyingType();
                return $"\t\t{FormatReadValue($"({propType.Name})Enum.ToObject(typeof({propType.Name}),{GetReadPrimitiveFragment(realType)})", valuePath)}";
            }
            else
            {
                //string line = "";

                Type baseType = propType;

                if (Nullable.GetUnderlyingType(propType) != null)
                {
                    baseType = propType.GetGenericArguments()[0];
                    //line += $"if(\t\t{GetReadPrimitiveFragment(typeof(bool))})\r\n";
                }

                if (!ExistsDefaultTypes.Contains(baseType))
                    throw new Exception();

                var readFragment = GetReadPrimitiveFragment(baseType);

                if (baseType == propType)
                    return $"\t\t{FormatReadValue(readFragment, valuePath)}";

                return $"\t\t{FormatReadValue($"p.ReadNullable(()=>{readFragment})", valuePath)}";
            }
        }

        private static string GetWriteCode(string scheme, Type propType, string valuePath)
        {
            Type[] tempTypes = null;

            bool nvaluePath = string.IsNullOrEmpty(valuePath);


            if (propType.IsClass && !ExistsDefaultTypes.Contains(propType))
            {
                if (typeof(System.Collections.IList).IsAssignableFrom(propType))
                {
                    tempTypes = propType.GetGenericArguments();
                    LoadType(tempTypes[0]);
                    LoadListType(tempTypes[0], scheme);

                    return FormatWriteValue(GetWriteClassListFragment(tempTypes[0], scheme, valuePath),valuePath);
                }
                else if (typeof(System.Collections.IDictionary).IsAssignableFrom(propType))
                {
                    tempTypes = propType.GetGenericArguments();

                    LoadType(tempTypes[0]);
                    LoadType(tempTypes[1]);

                    LoadDictType(tempTypes[0], tempTypes[1], scheme);

                    return FormatWriteValue(GetWriteClassMapFragment(tempTypes[0], tempTypes[1], scheme, valuePath), $"\t\t{valuePath}");

                }
                else
                {
                    LoadType(propType);

                    return $"\t\t{FormatWriteValue(GetWriteClassFragment(propType, scheme, valuePath), valuePath)}";
                }
            }
            else if (propType.IsEnum)
            {
                AddNameSpace(propType);
                var realType = propType.GetEnumUnderlyingType();

                return $"\t\t{FormatWriteValue(GetWritePrimitiveFragment(realType), $"({realType.Name}){valuePath}")}";
            }
            else
            {
                Type baseType = propType;

                if (Nullable.GetUnderlyingType(propType) != null)
                {
                    baseType = propType.GetGenericArguments()[0];
                }

                if (!ExistsDefaultTypes.Contains(baseType))
                    throw new Exception();

                var writeFragment = GetWritePrimitiveFragment(baseType);

                if (baseType == propType)
                    return $"\t\t{FormatWriteValue(writeFragment, valuePath)}";

                return $"\t\t{FormatWriteValue($"p.WriteNullable({valuePath},()=>{writeFragment})", valuePath +".Value")}";
            }
            return "";
        }


        private static List<Tuple<Type, string>> LoadedListTypes = new List<Tuple<Type, string>>();

        private static bool AddListClass(Type t, string scheme)
        {
            var tt = new Tuple<Type, string>(t, scheme);

            if (LoadedListTypes.Contains(tt))
                return false;

            LoadedListTypes.Add(tt);
            return true;
        }

        private static void LoadListType(Type t, string scheme)
        {
            if (!AddListClass(t, scheme))
                return;

            AddNameSpace(typeof(List<>));

            string lName = "_" + Guid.NewGuid().ToString().Replace("-", "");

            var s = new StringBuilder();

            s.AppendLine($"\tpublic static List<{t.Name}> Read{t.Name}List_{scheme}(this InputPacketBuffer p)");

            s.AppendLine("\t{");
            s.AppendLine($"\t\tint {lName} = {ReadFragmentsDefaultTypes[typeof(int)]};");

            s.AppendLine($"\t\tvar temp = new List<{t.Name}>({lName});");

            s.AppendLine($"\t\tfor (int i = 0; i < {lName}; i++)");
            s.AppendLine("\t\t{");
            s.AppendLine($"\t\t\ttemp.Add({GetReadCode(scheme, t, $"")});");
            s.AppendLine("\t\t}");
            s.AppendLine("\t\treturn temp;");
            s.Append("\t}");

            Fragments.Add(s);

            s = new StringBuilder();

            s.AppendLine($"\tpublic static void Write{t.Name}List_{scheme}(this List<{t.Name}> value, OutputPacketBuffer p)");

            s.AppendLine("\t{");

            s.AppendLine($"\t\tp.WriteInt32(value.Count);");

            s.AppendLine($"\t\tforeach (var item in value)");
            s.AppendLine("\t\t{");
            s.AppendLine($"\t\t\t{GetWriteCode(scheme, t, $"item")}");
            s.AppendLine("\t\t}");
            s.Append("\t}");

            Fragments.Add(s);
        }



        private static List<Tuple<Type, Type, string>> LoadedDictTypes = new List<Tuple<Type, Type, string>>();

        private static bool AddDictClass(Type t, Type t2, string scheme)
        {
            var tt = new Tuple<Type, Type, string>(t, t2, scheme);

            if (LoadedDictTypes.Contains(tt))
                return false;

            LoadedDictTypes.Add(tt);
            return true;
        }


        private static void LoadDictType(Type t, Type t2, string scheme)
        {
            if (!AddDictClass(t, t2, scheme))
                return;

            AddNameSpace(typeof(Dictionary<,>));
            string lName = "_" + Guid.NewGuid().ToString().Replace("-", "");

            var s = new StringBuilder();

            s.AppendLine($"\tpublic static Dictionary<{t.Name},{t2.Name}> Read{t.Name}{t2.Name}Map_{scheme}(this InputPacketBuffer p)");

            s.AppendLine("\t{");
            s.AppendLine($"\t\tint {lName} = {ReadFragmentsDefaultTypes[typeof(int)]};");

            s.AppendLine($"\t\tvar temp = new Dictionary<{t.Name},{t2.Name}>({lName});");

            s.AppendLine($"\t\tfor (int i = 0; i < {lName}; i++)");
            s.AppendLine("\t\t{");
            s.AppendLine($"\t\t\ttemp.Add({GetReadCode(scheme, t, $"")}, {GetReadCode(scheme, t2, $"")});");
            s.AppendLine("\t\t}");
            s.AppendLine("\t\treturn temp;");
            s.Append("\t}");

            Fragments.Add(s);

            s = new StringBuilder();

            s.AppendLine($"\tpublic static void Write{t.Name}{t2.Name}Map_{scheme}(this Dictionary<{t.Name},{t2.Name}> value, OutputPacketBuffer p)");

            s.AppendLine("\t{");

            s.AppendLine($"\t\tp.WriteInt32(value.Count);");

            s.AppendLine($"\t\tforeach (var item in value)");
            s.AppendLine("\t\t{");
            s.AppendLine($"\t\t\t{GetWriteCode(scheme, t, $"item.Key")}");
            s.AppendLine($"\t\t\t{GetWriteCode(scheme, t2, $"item.Value")}");
            s.AppendLine("\t\t}");
            s.Append("\t}");

            Fragments.Add(s);
        }

        //public void Initialize(GeneratorInitializationContext context)
        //{
        //    context.RegisterForSyntaxNotifications(new SyntaxReceiverCreator(() => new BinarySyntaxReceiver()));
        //}

        //public void Execute(GeneratorExecutionContext context)
        //{
        //    if (!(context.SyntaxReceiver is BinarySyntaxReceiver receiver))
        //        return;           
            
        //    // Create property for target name at
        //    CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;

        //    INamedTypeSymbol attributeSymbol = context.Compilation.GetTypeByMetadataName("AutoNotify.AutoNotifyAttribute");

        //    foreach (var cl in receiver.CandidateClasses)
        //    {
        //        SemanticModel model = context.Compilation.GetSemanticModel(cl.SyntaxTree);
        //        var classSymb = model.GetDeclaredSymbol(cl);
        //        if (classSymb.GetAttributes().Any(x => x.Equals(attributeSymbol)))
        //        {
        //            classSymb.GetMembers().First().
        //        }
        //    }

        //}


        //// Helper method
        //private bool HasAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol attributeSymbol)
        //{
        //    foreach (var attribute in typeSymbol.GetAttributes())
        //    {
        //        if (attribute.AttributeClass?.Equals(attributeSymbol, SymbolEqualityComparer.Default) == true)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }

    //internal class BinarySyntaxReceiver : ISyntaxReceiver
    //{
    //    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

    //    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    //    {
    //        if (syntaxNode is ClassDeclarationSyntax cds)
    //        {
    //            if (cds.AttributeLists.Count > 0)
    //            {
    //                CandidateClasses.Add(cds);
    //            }
    //        }
    //    }
    //}
}
