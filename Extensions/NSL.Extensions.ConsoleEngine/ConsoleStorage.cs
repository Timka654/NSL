using NSL.SocketCore.Utils;
using System;
using System.Collections.Concurrent;

namespace NSL.Extensions.ConsoleEngine
{
    public class ConsoleStorage<T>
        where T : INetworkClient
    {
        protected ConcurrentDictionary<string, Func<T, string[],string>> command_map;

        internal ConsoleStorage()
        {
            command_map = new ConcurrentDictionary<string, Func<T, string[],string>>();
        }

        public virtual void AddCommand(string command, Func<T, string[],string> commandArgs)
        {
            command_map.TryAdd(command.ToLower(), commandArgs);
        }

        public virtual void RemoveCommand(string command)
        {
            command_map.TryRemove(command.ToLower(),out var r);
        }

        public virtual Func<T, string[], string> GetCommand(string command)
        {
            command_map.TryGetValue(command.ToLower(), out var r);

            return r;
        }
    }
}
