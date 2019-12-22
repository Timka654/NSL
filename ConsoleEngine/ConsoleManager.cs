﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SocketCore.Utils;
using SocketCore.Utils.Buffer;

namespace ConsoleEngine
{
    public class ConsoleManager<T> : ConsoleStorage<T>
        where T : INetworkClient
    {
        public static ConsoleManager<T> Instance { get; private set; }

        public ConsoleManager()
        {
            Instance = this;
        }

        public string InvokeCommand(T client, string text)
        {
            var cmd = ParseMultiSpacedArguments(text);
            var act = GetCommand(cmd[0].ToLower());

            if (act == null)
            {
                return "NotFound";
            }

            if (cmd.Length - 1 == 0)
                return act.Invoke(client,new string[0]);

            var args = new string[cmd.Length - 1];
            Array.Copy(cmd, 1, args, 0, cmd.Length - 1);

            return act.Invoke(client,args);
        }

        static string[] ParseMultiSpacedArguments(string commandLine)
        {
            var isLastCharSpace = false;
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ' && !isLastCharSpace)
                    parmChars[index] = '\n';

                isLastCharSpace = parmChars[index] == '\n' || parmChars[index] == ' ';
            }

            return (new string(parmChars)).Split('\n');
        }
    }
}
