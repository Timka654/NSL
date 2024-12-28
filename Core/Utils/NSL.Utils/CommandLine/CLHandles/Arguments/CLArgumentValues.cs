using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Utils.CommandLine.CLHandles.Arguments
{
    public class CLArgumentValues
    {
        Dictionary<string, object> Values = new Dictionary<string, object>();

        public async Task<bool> TryRead(CommandLineArgsReader reader, CLArgument arg)
        {
            if (!reader.Args.ContainsKey(arg.ArgName))
            {
                if (arg.Optional)
                    return true;

                return false;
            }
            var result = await arg.TryRead(reader);

            if (result.Success)
                Values.Add(arg.ArgName, result.Result);

            return result.Success;
        }

        public bool ContainsArg(string name)
        {
            return Values.ContainsKey(name);
        }

        public TResult GetValue<TResult>(string name, TResult defaultValue = default)
        {
            TryGetValue(name, out var result, defaultValue);

            return result;
        }

        public bool TryGetValue<TResult>(string name, out TResult result, TResult defaultValue = default)
        {
            if (!Values.TryGetValue(name, out var _result))
            {
                result = defaultValue;
                return false;
            }

            result = (TResult)_result;

            return true;
        }
    }
}
