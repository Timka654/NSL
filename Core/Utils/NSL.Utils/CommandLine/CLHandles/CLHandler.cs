using NSL.Utils.CommandLine.CLHandles.Arguments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSL.Utils.CommandLine.CLHandles
{
    public abstract class CLHandler
    {
        protected virtual Dictionary<string, CLHandler> ChildCommands { get; } = new Dictionary<string, CLHandler>();

        protected virtual Dictionary<string, CLArgument> Arguments { get; } = new Dictionary<string, CLArgument>();

        protected virtual string[] HelpHandleCommands { get; set; } = new[] { "help", "?" };

        public virtual string Command { get; }

        public virtual string Description { get; set; }

        public virtual string ShortDescription { get; set; }

        protected void AddCommands(params CLHandler[] commands)
        {
            foreach (var command in commands)
            {
                ChildCommands.Add(command.Command, command);
            }
        }

        protected void AddArguments(params CLArgument[] args)
        {
            foreach (var arg in args)
            {
                Arguments.Add(arg.ArgName, arg);
            }
        }

        protected CLArgument<T> CreateArgument<T>(string argName, CLArgument<T>.CommandArgumentHandler handler)
        {
            return new CLArgument<T>(argName, handler);
        }

        protected async Task<(bool Success, CLArgumentValues Values, CLReadException error)> ReadArguments(CommandLineArgsReader reader)
        {
            var result = new CLArgumentValues();

            foreach (var arg in Arguments)
            {
                try
                {

                    if (!await result.TryRead(reader, arg.Value) && !arg.Value.Optional)
                    {
                        return (false, null, new CLReadException($"cannot found or read required argument \"{arg.Key}\".") { Argument = arg.Value });
                    }

                }
                catch (Exception ex)
                {
                    return (false, null, new CLReadException($"cannot read argument \"{arg.Key}\". Error - {ex.ToString()}.") { Argument = arg.Value });
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

        public virtual Task<CommandReadStateEnum> ProcessCommand(CommandLineArgsReader reader, CLArgumentValues values)
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
        protected (CommandReadStateEnum Result, CLHandler Next) TryGetNext(CommandLineArgsReader reader)
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


        public virtual Task<bool> ProcessHelp(CommandLineArgsReader reader, CommandReadStateEnum state, CLReadException exception)
        {
            if (state != CommandReadStateEnum.Success)
            {
                var command = string.Join(" ", Enumerable.Range(0, reader.Index + (state == CommandReadStateEnum.InvalidPath ? 1 : 0)).Select(i => reader.Args.At(i).Key));

                WriteToConsoleWithColor(() =>
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


        protected virtual async Task<CommandReadStateEnum> InvalidArgsHandle(CommandLineArgsReader reader, CLReadException exception)
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

        protected void WriteToConsoleWithColor(Action action, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            action();

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        /// <summary>
        /// Generate arguments for command from <see cref="NSL.Utils.CommandLine.CLHandles.Arguments.CLArgumentAttribute"/> attached to this class type
        /// </summary>
        /// <returns></returns>
        protected CLArgument[] SelectArguments()
        {
            var thisType = this.GetType();

            var argType = typeof(CLArgument<>);

            var delegateType = typeof(CLArgument<>.CommandArgumentHandler);

            var readMethod = typeof(CLHandler).GetMethod(nameof(__readArg), BindingFlags.Static | BindingFlags.NonPublic);
            var containsMethod = typeof(CLHandler).GetMethod(nameof(__containsArg), BindingFlags.Static | BindingFlags.NonPublic);

            List<CLArgument> args = new List<CLArgument>();

            foreach (var item in thisType.GetCustomAttributes<CLArgumentAttribute>())
            {
                var type = item.Type;

                Delegate itemReadMethod;

                if (Equals(type, typeof(CLContainsType)))
                {
                    type = typeof(bool);
                    var itemDelegateType = delegateType.MakeGenericType(type);
                    itemReadMethod = containsMethod.CreateDelegate(itemDelegateType);
                }
                else
                {

                    var itemDelegateType = delegateType.MakeGenericType(type);
                    itemReadMethod = readMethod.MakeGenericMethod(type).CreateDelegate(itemDelegateType);

                }


                var itemArg = argType.MakeGenericType(type);


                var arg = (CLArgument)Activator.CreateInstance(itemArg, new object[] { item.Name, itemReadMethod });

                if (item.Optional)
                    arg.WithOptional();

                arg.Description = item.Description;

                args.Add(arg);
            }

            return args.ToArray();
        }

        private static ConcurrentDictionary<Type, Action<CLHandler, CLArgumentValues>> setActions = new ConcurrentDictionary<Type, Action<CLHandler, CLArgumentValues>>();

        /// <summary>
        /// Sets arguments to this instance from <see cref="CLArgumentValues"/> by <see cref="CLArgumentValueAttribute"/> and <see cref="CLArgumentExistsAttribute"/> attributes
        /// </summary>
        /// <param name="values"></param>
        protected void ProcessingAutoArgs(CLArgumentValues values)
        {
            var thisType = this.GetType();

            if (!setActions.TryGetValue(thisType, out var setAction))
            {
                var props = thisType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Select(x => new
                    {
                        property = x,
                        ValueSet = x.GetCustomAttribute<CLArgumentValueAttribute>(),
                        ExistsSet = x.GetCustomAttribute<CLArgumentExistsAttribute>(),
                    })
                    .Where(x => x.ExistsSet != null || x.ValueSet != null)
                    .ToArray();

                setAction = (i, v) => { };

                var getAction = typeof(CLArgumentValues).GetMethod(nameof(CLArgumentValues.GetValue));

                foreach (var _item in props)
                {
                    var item = _item;

                    if (item.ValueSet != null)
                    {
                        var pGetAction = getAction.MakeGenericMethod(item.property.PropertyType);
                        setAction += (i, args) =>
                        {
                            var v = pGetAction.Invoke(args, new object[] { item.ValueSet.Name, item.ValueSet.DefaultValue });
                            item.property.SetValue(i, v);
                        };
                    }
                    else if (item.ExistsSet != null)
                    {
                        setAction += (i, args) =>
                        {
                            item.property.SetValue(i, args.ContainsArg(item.ExistsSet.Name));
                        };
                    }

                }
                setActions[thisType] = setAction;
            }

            setAction(this, values);
        }

        protected CLHandler[] SelectSubCommands<TAttribute>(string name, bool useInstance)
            where TAttribute : CLHandleSelectAttribute
            => SelectSubCommands<TAttribute>(Assembly.GetCallingAssembly(), name, useInstance);
        protected CLHandler[] SelectSubCommands<TAttribute>(Assembly assembly, string name, bool useInstance)
            where TAttribute : CLHandleSelectAttribute
        {
            var handles = assembly.GetTypes().Select(x => new { type = x, attributes = x.GetCustomAttribute<TAttribute>() })
                .Where(x => x.attributes != null && x.attributes.Name == name)
                .ToArray();

            List<CLHandler> result = new List<CLHandler>();

            foreach (var item in handles)
            {
                if (useInstance)
                {
                    var instanceType = typeof(CLHandler<>).MakeGenericType(item.type);

                    var instanceProp = instanceType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);

                    result.Add((CLHandler)instanceProp.GetValue(null));
                }
                else
                {
                    result.Add((CLHandler)Activator.CreateInstance(item.type));
                }
            }

            return result.ToArray();
        }

        private static Task<(bool Success, TResult Result)> __readArg<TResult>(CommandLineArgsReader reader, string name)
        {
            return Task.FromResult((reader.Args.TryGetOutValue<TResult>(name, out var r), r));
        }

        private static Task<(bool Success, bool Result)> __containsArg(CommandLineArgsReader reader, string name)
        {
            return Task.FromResult((true, reader.Args.ContainsKey(name)));
        }
    }

    public sealed class CLHandler<TTHIS>
        where TTHIS : CLHandler, new()
    {
        private static CLHandler instance;

        public static CLHandler Instance => instance = instance ?? new TTHIS();
    }

    public class CLReadException : Exception
    {
        public CLReadException(string message) : base(message)
        {
        }

        public CLArgument Argument { get; set; }
    }
}
