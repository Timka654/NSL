using DBEngine;
using SocketServer.Utils.Buffer;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Utils.Helper.Configuration.Info
{
    public class ConfigurationInfo
    {
        /// <summary>
        /// Полный путь(название)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Необходимо отправить клиенту
        /// </summary>
        public string Flags { get => flags; set { flags = value; compiledFlag = null; } }

        private string compiledFlag;
        private string flags;

        private string CompiledFlag => compiledFlag ?? (compiledFlag = Flags.EndsWith("%") ? Flags : Flags + "%");

        public bool ExistFlag(string flag)
        {
            return CompiledFlag.Contains(flag + "%");
        }

        public ConfigurationInfo()
        { }

        public ConfigurationInfo(string name, string value, string flags)
        {
            Name = name;
            Value = value;
            Flags = flags;
        }
    }
}
