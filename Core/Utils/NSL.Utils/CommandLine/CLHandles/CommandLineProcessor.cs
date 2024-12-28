using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Utils.CommandLine.CLHandles
{
    public abstract class CommandLineProcessor
    {
        protected virtual Dictionary<string, CommandLineProcessor> ChildCommands { get; } = new Dictionary<string, CommandLineProcessor>();

        protected virtual Dictionary<string, CommandLineArgument> Arguments { get; } = new Dictionary<string, CommandLineArgument>();

        protected virtual string[] HelpHandleCommands { get; set; } = new[] { "help", "?" };

        public virtual string Command { get; }

        public virtual string Description { get; set; }

        public virtual string ShortDescription { get; set; }

        protected void AddCommands(params CommandLineProcessor[] commands)
        {
            foreach (var command in commands)
            {
                ChildCommands.Add(command.Command, command);
            }
        }

        protected void AddArguments(params CommandLineArgument[] args)
        {
            foreach (var arg in args)
            {
                Arguments.Add(arg.ArgName, arg);
            }
        }

        protected CommandLineArgument<T> CreateArgument<T>(string argName, CommandLineArgument<T>.CommandArgumentHandler handler)
        {
            return new CommandLineArgument<T>(argName, handler);
        }

        protected async Task<(bool Success, CommandLineArgumentValues Values, CommandLineReadException error)> ReadArguments(CommandLineArgsReader reader)
        {
            var result = new CommandLineArgumentValues();

            foreach (var arg in Arguments)
            {
                try
                {

                    if (!await result.TryRead(reader, arg.Value) && !arg.Value.Optional)
                    {
                        return (false, null, new CommandLineReadException($"cannot found or read required argument \"{arg.Key}\".") { Argument = arg.Value });
                    }

                }
                catch (Exception ex)
                {
                    return (false, null, new CommandLineReadException($"cannot read argument \"{arg.Key}\". Error - {ex.ToString()}.") { Argument = arg.Value });
                }
            }

            return (true, result, null);
        }

        public virtual async Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader)
        {
            if (await TryRunHelp(reader))
                return CommandReadStateEnum.HelpInvoked;

            var result = await TryRunNext(reader);

            if (result.CurrentState == CommandReadStateEnum.FinishPath)
            {
                var args = await ReadArguments(reader);

                if (!args.Success)
                    return await InvalidArgsHandle(reader, args.error);

                return await ProcessCommand(reader, args.Values);
            }

            if (result.CurrentState == CommandReadStateEnum.InvalidPath)
            {
                return await InvalidPathHandle(reader);
            }

            return result.ResultState ?? result.CurrentState;
        }

        public virtual Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CommandLineArgumentValues values)
        {
            return Task.FromResult(CommandReadStateEnum.Success);
        }

        protected async Task<(CommandReadStateEnum CurrentState, CommandReadStateEnum? ResultState)> TryRunNext(CommandLineArgsReader reader)
        {
            var getResult = TryGetNext(reader);

            if (getResult.Result == CommandReadStateEnum.Success)
            {
                reader.TryNext();
                return (CommandReadStateEnum.Success, await getResult.Next.ProcessCommand(reader));
            }

            return (getResult.Result, null);
        }
        protected (CommandReadStateEnum Result, CommandLineProcessor Next) TryGetNext(CommandLineArgsReader reader)
        {
            if (!reader.IsPath())
                return (CommandReadStateEnum.FinishPath, null);

            if (!ChildCommands.TryGetValue(reader.GetKey(), out var command))
            {
                return (CommandReadStateEnum.InvalidPath, null);
            }

            return (CommandReadStateEnum.Success, command);
        }

        protected async Task<bool> TryRunHelp(CommandLineArgsReader reader)
        {
            if (HelpHandleCommands == null)
                return false;

            var key = reader.GetKey();

            if (HelpHandleCommands.Contains(key))
                return await ProcessHelp(reader, CommandReadStateEnum.Success, null);

            return false;
        }


        public virtual Task<bool> ProcessHelp(CommandLineArgsReader reader, CommandReadStateEnum state, CommandLineReadException exception)
        {
            if (state != CommandReadStateEnum.Success)
            {
                var command = string.Join(" ", Enumerable.Range(0, reader.Index + (state == CommandReadStateEnum.InvalidPath ? 1 : 0 )).Select(i => reader.Args.At(i).Key));
                
                DisplayConssoleWithColor(() =>
                {
                    if (state == CommandReadStateEnum.InvalidPath)
                    {
                        Console.WriteLine($"Could not execute because the specified command \"{command}\" was not found.");
                    }
                    else if (state == CommandReadStateEnum.InvalidArgument)
                    {
                        Console.WriteLine($"Could not execute because the specified command \"{command}\" {exception.Message}.");
                    }
                }, ConsoleColor.Red);
            }

            var content = GetHelpContent();

            if (content != default)
            {
                Console.WriteLine(content);

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }


        protected virtual async Task<CommandReadStateEnum> InvalidArgsHandle(CommandLineArgsReader reader, CommandLineReadException exception)
        {
            if (await ProcessHelp(reader, CommandReadStateEnum.InvalidArgument, exception))
                return CommandReadStateEnum.InvalidArgumentHelpInvoked;

            return CommandReadStateEnum.InvalidArgument;
        }

        protected virtual async Task<CommandReadStateEnum> InvalidPathHandle(CommandLineArgsReader reader)
        {
            if (await ProcessHelp(reader, CommandReadStateEnum.InvalidPath, null))
                return CommandReadStateEnum.InvalidPathHelpInvoked;

            return CommandReadStateEnum.InvalidPath;
        }

        protected virtual string GetHelpContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Command: {Command}");

            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLine();
                sb.AppendLine($"{Description}");
            }

            if (Arguments.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"{Command} argument list:");

                var argMSize = Arguments.Max(x => x.Key.Length) + 1;

                foreach (var arg in Arguments)
                {
                    sb.AppendLine($"{(arg.Key + (arg.Value.Optional ? string.Empty : "*")).PadRight(argMSize)} - {(arg.Value.Description ?? "<NO DESCRIPTIOM>")}");
                }

                if (Arguments.Any(x => x.Value.Optional))
                    sb.AppendLine("* - marked required arguments");
            }

            if (ChildCommands.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"{Command} command list:");

                var argMSize = ChildCommands.Max(x => x.Key.Length);

                foreach (var arg in ChildCommands)
                {
                    sb.AppendLine($"{arg.Key.PadRight(argMSize)} - {(arg.Value.ShortDescription ?? arg.Value.Description ?? "<NO DESCRIPTIOM>")}");
                }
            }

            return sb.ToString();
        }

        protected void DisplayConssoleWithColor(Action action, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            action();

            Console.ForegroundColor = ConsoleColor.Gray;

        }
    }

    public class CommandLineProcessor<TTHIS> : CommandLineProcessor
        where TTHIS : CommandLineProcessor<TTHIS>, new()
    {
        private static CommandLineProcessor instance;

        public static CommandLineProcessor Instance => instance = instance ?? new TTHIS();
    }

    public class CommandLineReadException : Exception
    {
        public CommandLineReadException(string message) : base(message)
        {
        }

        public CommandLineArgument Argument { get; set; }
    }
}
