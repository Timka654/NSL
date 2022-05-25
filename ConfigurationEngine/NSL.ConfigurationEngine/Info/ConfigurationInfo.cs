using NSL.ConfigurationEngine.Providers;
using System;

namespace NSL.ConfigurationEngine.Info
{
    public class ConfigurationInfo : IEquatable<ConfigurationInfo>
    {
        /// <summary>
        /// Полный путь(название)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Необходимо отправить клиенту
        /// </summary>
        public string Flags { get => flags; set { flags = value; compiledFlag = (compiledFlag = value.TrimEnd('%') + "%"); } }

        private string flags;

        private string compiledFlag;
        internal IConfigurationProvider Provider { get; set; }

        public bool ExistFlag(string flag)
        {
            return compiledFlag.Contains(flag + "%");
        }

        public bool Equals(ConfigurationInfo other)
        {
            return other.Path.Equals(Path) && other.Value.Equals(Value) && other.Flags.Equals(Flags);
        }

        protected ConfigurationInfo()
        { }

        public ConfigurationInfo(string name, string value, string flags) : this(name, value, BaseConfigurationManager.NoProvider, flags)
        { 
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="flags">Флаги указываются с разделителем % (прим. %c%d)</param>
        public ConfigurationInfo(string name, string value, IConfigurationProvider provider, string flags)
        {
            Path = name.ToLower();
            Value = value;
            this.Provider = provider;
            Flags = flags ?? "";
        }
    }
}
