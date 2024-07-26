using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using NSL.Generators.BinaryTypeIOGenerator.Attributes;
using NSL.Generators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;

namespace NSL.Generators.BinaryTypeIOGenerator
{
    internal class NSLBIOGeneratorContext : BinaryTypeIOGeneratorContext
    {
        public override bool IsIgnore(ISymbol symbol, string path)
        {


            //#if DEBUG
            //            if (path.EndsWith("t1") && string.Equals(For, null))
            //            {
            //                GenDebug.Break();
            //            }
            //#endif

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

        Stack<(string, Func<string, bool>)> tree = new Stack<(string, Func<string, bool>)>();

        public override void CloseTypeEntry(ISymbol symbol, string path)
        {
            var d = tree.Pop();

            For = d.Item1;

            ModelSelector = d.Item2;
        }

        public override void OpenTypeEntry(ISymbol symbol, string path)
        {
            tree.Push((For, ModelSelector));

            var proxyAttributes = symbol.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(NSLBIOProxyAttribute));

            //#if DEBUG

            //            if (For == "a2")
            //            {
            //                if (path.EndsWith("b2p") || path.EndsWith("t3"))
            //                {
            //                    GenDebug.Break();
            //                }
            //            }

            //#endif


            //if (path.EndsWith("a2"))
            //{
            //    GenDebug.Break();
            //}

            var map = proxyAttributes.Select(x =>
            {
                var args = x.ConstructorArguments;

                if (args.Length == 1)
                    return ((string)args[0].Value, (string[])null);

                return ((string)args[0].Value, args[1].Values.Select(n => (string)n.Value).ToArray());
            }).ToArray();

            var newFor = map.FirstOrDefault(x => x.Item2?.Contains(For) == true);

            if (newFor == default)
            {
                var globalModel = map.FirstOrDefault(x => x.Item2 == null);

                if (globalModel != default)
                    For = globalModel.Item1;
            }
            else
                For = newFor.Item1;

            if (ModelSelector("<!!_NSLBIOFULL_!!>") == true)
                return;

            var type = symbol.GetTypeSymbol();

            var joinAttributes = type.GetAttributes()
                .Where(x => x.AttributeClass.Name == nameof(NSLBIOModelJoinAttribute)).ToArray();

            var joins = joinAttributes.Select(x =>
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

                if (joins.TryGetValue(For, out var jns))
                    actualModels = actualModels
                                        .Concat(jns)
                                        .GroupBy(x => x)
                                        .Select(x => x.Key)
                                        .ToArray();
            }

            ModelSelector = modelSelector;
        }
    }
}
