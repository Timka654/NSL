using System.Collections.Generic;
using System.Threading.Tasks;

namespace NSL.Utils.CommandLine.CLHandles
{
    public class CommandLineArgumentValues
    {
        Dictionary<string, object> Values = new Dictionary<string, object>();

        public async Task<bool> TryRead(CommandLineArgsReader reader, CommandLineArgument arg)
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

        public bool GetValue<TResult>(string name, out TResult result)
        {
            if (!Values.TryGetValue(name, out var _result))
            {
                result = default;
                return false;
            }

            result = (TResult)_result;

            return true;
        }
    }
}
