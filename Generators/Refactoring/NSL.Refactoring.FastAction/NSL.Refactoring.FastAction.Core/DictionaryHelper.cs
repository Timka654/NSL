using System.Collections.Generic;

namespace NSL.Refactoring.FastAction.Core
{
    public static class DictionaryHelper
    {
        public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                return false;

            dictionary[key] = value;

            return true;
        }
    }
}
