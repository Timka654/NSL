using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using gls.Data.Basic.Storages;
using SocketServer.Utils;
using SocketServer.Utils.Buffer;

namespace Utils.Helper.Console
{
    public class ConsoleManager : ConsoleStorage
    {
        public ConsoleManager()
        {
            Utils.Logger.ConsoleLogger.WriteFormat(Utils.Logger.LoggerLevel.Info, $"ConsoleManager Loaded");
        }

        public string InvokeCommand(INetworkClient client, string text)
        {
            var cmd = ParseCommand(text);
            var act = GetValue(cmd[0].ToLower());

            if (act == null)
            {
                return "NotFound";
            }

            if (cmd.Length - 1 <= 0)
                return act.Invoke(client,null);

            var args = new string[cmd.Length - 1];

            Array.Copy(cmd, 1, args, 0, cmd.Length - 1);
            return act.Invoke(client,args);
        }

        private string[] ParseCommand(string text)
        {
            return text.Split(" ");
        }
    }
}
