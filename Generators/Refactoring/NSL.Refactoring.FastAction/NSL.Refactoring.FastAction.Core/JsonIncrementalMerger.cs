using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace NSL.Refactoring.FastAction.Core
{
    internal static class JsonIncrementalMerger
    {
        public static JsonObject MergeJsonFiles(IEnumerable<string> filePaths)
        {
            var result = new JsonObject();

            foreach (var path in filePaths)
            {
                var text = File.ReadAllText(path);
                var node = JsonNode.Parse(text, documentOptions: new System.Text.Json.JsonDocumentOptions()
                {
                    AllowTrailingCommas = true,
                    CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                }) as JsonObject;
                if (node != null)
                {
                    MergeObjects(result, node);
                }
            }

            return result;
        }

        private static void MergeObjects(JsonObject target, JsonObject source)
        {
            foreach (var kv in source)
            {
                var parsed = ParseKey(kv.Key);
                var key = parsed.Item1;
                var modifier = parsed.Item2;
                var value = kv.Value;

                if (modifier == "r" || !target.ContainsKey(key))
                {
                    target[key] = value != null ? value.DeepClone() : null;
                    continue;
                }

                var oldValue = target[key];
                if (oldValue is JsonObject && value is JsonObject)
                {
                    MergeObjects((JsonObject)oldValue, (JsonObject)value);
                }
                else if (oldValue is JsonArray && value is JsonArray)
                {
                    MergeArrays((JsonArray)oldValue, (JsonArray)value, modifier);
                }
                else
                {
                    target[key] = value != null ? value.DeepClone() : null;
                }
            }
        }

        private static void MergeArrays(JsonArray target, JsonArray source, string modifier)
        {
            if (source.Count > 0 && source[0] is JsonObject && ((JsonObject)source[0]).ContainsKey("Id"))
            {
                var targetIndex = new Dictionary<string, JsonObject>();
                foreach (var item in target)
                {
                    var obj = item as JsonObject;
                    if (obj != null && obj["Id"] != null)
                    {
                        targetIndex[obj["Id"].ToString()] = obj;
                    }
                }

                foreach (var srcItem in source)
                {
                    var srcObj = srcItem as JsonObject;
                    if (srcObj == null || srcObj["Id"] == null)
                        continue;

                    var id = srcObj["Id"].ToString();
                    if (targetIndex.ContainsKey(id))
                    {
                        MergeObjects(targetIndex[id], srcObj);
                    }
                    else
                    {
                        target.Add(srcObj.DeepClone());
                    }
                }
            }
            else if (modifier == "a")
            {
                foreach (var item in source)
                {
                    target.Add(item != null ? item.DeepClone() : null);
                }
            }
            else
            {
                target.Clear();
                foreach (var item in source)
                {
                    target.Add(item != null ? item.DeepClone() : null);
                }
            }
        }

        private static Tuple<string, string> ParseKey(string key)
        {
            if (key.EndsWith("$r"))
                return Tuple.Create(key.Substring(0, key.Length - 2), "r");
            if (key.EndsWith("$a"))
                return Tuple.Create(key.Substring(0, key.Length - 2), "a");
            return Tuple.Create(key, "a");
        }
    }


}
