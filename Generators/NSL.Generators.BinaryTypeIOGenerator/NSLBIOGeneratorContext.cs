using Microsoft.CodeAnalysis;
using NSL.Generators.BinaryGenerator.Utils;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class NSLBIOGeneratorContext : BinaryTypeIOGeneratorContext
    {
        public override bool IsIgnore(ISymbol symbol, string path)
        {
            if (ModelSelector("<!!_NSLBIOFULL_!!>"))
                return false;

            var modelAttributes = symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(NSLBIOIncludeAttribute));

            var models = modelAttributes
                .SelectMany(x =>
                {
                    var args = x.ConstructorArguments;

                    var oargs = args.SelectMany(b => b.Values.Select(d => (string)d.Value));

                    if (!oargs.Any())
                        return Enumerable.Repeat<string>(null, 1);

                    return oargs;
                })
                .ToArray();

            if (models.Any(x => ModelSelector(x)))
                return false;

            return true;
        }

        Stack<(string, Func<string, bool>, ISymbol)> tree = new Stack<(string, Func<string, bool>, ISymbol)>();

        public override void CloseTypeEntry(ISymbol symbol, string path)
        {
            var d = tree.Pop();

            For = d.Item1;

            ModelSelector = d.Item2;


            typePath.RemoveAt(typePath.Count - 1);
        }

        List<string> typePath = new List<string>();

        public override bool OpenTypeEntry(ISymbol __symbol, string path, CodeBuilder codeBuilder, bool read)
        {
            var symbol = CurrentMember ?? __symbol;

            var type = symbol.GetTypeSymbol();


            typePath.Add(type.ToString());

            var cycl = FindCycle(typePath);

            if (cycl.Item1)
            {
                var spath = tree.ToArray().Reverse().Select(x => x.Item3.Name).ToArray();


                var snpath = string.Join(".", spath.Skip(cycl.Item2));

                //if (string.IsNullOrEmpty(snpath))
                //    GenDebug.Break();

                string msg = $"Found cycle starts path {snpath}";

                var loc = CurrentPath.First().Item1.Locations;


                Context.ShowBIODiagnostics("NSLBIO001", msg, DiagnosticSeverity.Error, loc.ToArray());
                //codeBuilder.AppendComment($"Error {path}  {cycl.Item2} {cycl.Item3}");

                if (read)
                    codeBuilder.AppendLine($"return default({type.GetTypeFullName()});");

                return false;
            }


            base.CurrentPath.Add((symbol, path));


            tree.Push((For, ModelSelector, symbol));

            For = GetFor(symbol, For, codeBuilder);

            codeBuilder.AppendComment($"Final model for path = '{path}' - '{For}'");

            if (ModelSelector("<!!_NSLBIOFULL_!!>") == true)
                return true;

            var joinAttributes = type.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(NSLBIOModelJoinAttribute)).ToArray();

            var joinMap = joinAttributes.Select(x =>
            {
                var args = x.ConstructorArguments;

                //GenDebug.Break();

                return ((string)args[0].Value, args[1].Values.Select(n => (string)n.Value).ToArray());
            }).ToDictionary(x => x.Item1, x => x.Item2);


            var actualModels = Enumerable.Repeat(For, 1).ToArray();

            Func<string, bool> modelSelector = mname => mname == null;

            if (For != null)
            {
                modelSelector = mname => actualModels.Contains(mname);

                if (joinMap.TryGetValue(For, out var jns))
                    actualModels = actualModels
                                        .Concat(jns)
                                        .GroupBy(x => x)
                                        .Select(x => x.Key)
                                        .ToArray();

            }

            codeBuilder.AppendComment($"Models selector for {For} - [{string.Join(", ", actualModels)}]");

            ModelSelector = modelSelector;

            return true;
        }

        static (bool, int, string) FindCycle(List<string> paths)
        {
            Dictionary<string, int> seen = new Dictionary<string, int>();

            for (int i = 0; i < paths.Count; i++)
            {
                for (int length = 1; length <= (paths.Count - i) / 2; length++)
                {
                    string subsequence = string.Join(".", paths.GetRange(i, length));
                    if (seen.ContainsKey(subsequence))
                    {
                        int startIndex = seen[subsequence];
                        string cycle = string.Join(", ", paths.GetRange(startIndex, length));
                        return (true, startIndex, cycle);
                    }
                    seen[subsequence] = i;
                }
            }

            return (false, -1, null);
        }

        public override string GetExistsReadHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder)
        {
            if (path != default)
            {
                var type = symbol.GetTypeSymbol();

                //GenDebug.Break();

                var models = type.GetAttributes()
                    .Where(x => x.AttributeClass.MetadataName == NSLBIOTypeGenerator.NSLBIOTypeAttributeFullName)
                    .SelectMany(x => x.ConstructorArguments.SelectMany(n => n.Values))
                    .GroupBy(x => (string)x.Value)
                    .Select(x => x.Key)
                    .ToArray();

                var _for = GetFor(CurrentMember ?? symbol, For, codeBuilder);

                if (models.Contains(_for))
                    return $"{type.GetTypeFullName(false)}.Read{_for}From(dataPacket);";
            }

            return default;
        }

        public override string GetExistsWriteHandleCode(ISymbol symbol, string path, CodeBuilder codeBuilder)
        {
            if (path != "this" && path != "value")
            {
                var type = symbol.GetTypeSymbol();

                //GenDebug.Break();

                var models = type.GetAttributes()
                    .Where(x => x.AttributeClass.MetadataName == NSLBIOTypeGenerator.NSLBIOTypeAttributeFullName)
                    .SelectMany(x => x.ConstructorArguments.SelectMany(n => n.Values))
                    .GroupBy(x => (string)x.Value)
                    .Select(x => x.Key)
                    .ToArray();

                var _for = GetFor(CurrentMember ?? symbol, For, codeBuilder);

                if (models.Contains(_for))
                    return $"{type.GetTypeFullName(false)}.Write{_for}To({path}, __packet);";
            }

            return default;
        }

        private static string GetFor(ISymbol symbol, string For, CodeBuilder codeBuilder)
        {
            var proxyAttributes = symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(NSLBIOProxyAttribute));

            var proxyMap = proxyAttributes.Select(x =>
            {
                var args = x.ConstructorArguments;

                if (args.Length == 1)
                    return ((string)args[0].Value, (string[])null);

                return ((string)args[0].Value, args[1].Values.Select(n => (string)n.Value).ToArray());
            }).ToArray();

            var newFor = proxyMap.FirstOrDefault(x => x.Item2?.Contains(For) == true);

            if (newFor == default)
            {
                var globalModel = proxyMap.FirstOrDefault(x => x.Item2 == null);

                if (globalModel != default)
                {
                    codeBuilder.AppendComment($"Not found special proxy model for '{For}' - use default '{globalModel.Item1}'");

                    For = globalModel.Item1;
                }
                else
                    codeBuilder.AppendComment($"Not found special and default model proxy, use current model - '{For}'");
            }
            else
            {
                codeBuilder.AppendComment($"Found proxy for '{For}' - '{newFor.Item1}'");

                For = newFor.Item1;
            }

            return For;
        }
    }
}
