using System;

namespace ConfigurationEngine.Info
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
        public string Flags { get => flags; set { flags = value; compiledFlag = (compiledFlag = value.EndsWith("%") ? value : value + "%"); } }

        private string flags;

        private string compiledFlag;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="flags">Флаги указываются с разделителем % (прим. %c%d)</param>
        public ConfigurationInfo(string name, string value, string flags)
        {
            Path = name;
            Value = value;
            Flags = flags ?? "";
        }

    }
}
