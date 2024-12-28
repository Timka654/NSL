using System.Threading.Tasks;

namespace NSL.Utils.CommandLine.CLHandles.Arguments
{
    public class CLArgument<T> : CLArgument
    {
        public delegate Task<(bool Success, T Result)> CommandArgumentHandler(CommandLineArgsReader reader, string name);

        private CommandArgumentHandler Handler { get; }

        public CLArgument(string argName, CommandArgumentHandler handler) : base(argName)
        {
            Handler = handler;
        }

        public async Task<(bool Success, T Result)> TryReadTyped(CommandLineArgsReader reader)
        {
            return await Handler(reader, ArgName);
        }

        public override async Task<(bool Success, object Result)> TryRead(CommandLineArgsReader reader)
        {
            var result = await TryReadTyped(reader);

            return (result.Success, result.Result);
        }
    }

    public abstract class CLArgument
    {
        public string ArgName { get; }

        public bool Optional { get; set; }

        public string Description { get; set; }

        public CLArgument(string argName)
        {
            ArgName = argName;
        }

        public CLArgument WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public CLArgument WithOptional(bool optional = true)
        {
            Optional = optional;
            return this;
        }

        public async Task<(bool Success, T Result)> TryCast<T>(CommandLineArgsReader reader)
        {
            return await (this as CLArgument<T>).TryReadTyped(reader);
        }

        public abstract Task<(bool Success, object Result)> TryRead(CommandLineArgsReader reader);
    }
}
