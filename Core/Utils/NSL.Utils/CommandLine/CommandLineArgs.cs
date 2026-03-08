using System;
using System.Collections.Generic;
using System.Linq;

namespace NSL.Utils.CommandLine
{
    /// <summary>
    /// Sources : stackoverflow answers
    /// Very basic Command Line Args extracter
    /// <para>Parse command line args for args in the following format:</para>
    /// <para>/argname:argvalue /argname:argvalue /argname ...</para>
    /// </summary>
    public class CommandLineArgs
    {
        private readonly List<KeyValuePair<string, CommandArgStruct>> _args = new List<KeyValuePair<string, CommandArgStruct>>();
        private readonly Dictionary<string, CommandArgStruct> _argsDict = new Dictionary<string, CommandArgStruct>(StringComparer.OrdinalIgnoreCase);

        public CommandLineArgs()
        {
            BuildArgDictionary();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="haveExecutablePath">Must be setted <see langword="true"/> if first arg is executable path</param>
        public CommandLineArgs(string[] args, bool haveExecutablePath)
        {
            Parse(args, haveExecutablePath);
        }

        public string this[string key]
        {
            get
            {
                return _argsDict.ContainsKey(key) ? _argsDict[key].Value : null;
            }
        }

        public bool ContainsKey(string key)
        {
            return _argsDict.ContainsKey(key);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            if (_argsDict.TryGetValue(key, out var text))
                return text.Value;

            return defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_argsDict.TryGetValue(key, out var text))
                return (T)Convert.ChangeType(text.Value, typeof(T));

            return defaultValue;
        }

        public bool TryGetValue<T>(string key, ref T result)
        {
            if (_argsDict.TryGetValue(key, out var text))
            {
                try
                {
                    result = (T)Convert.ChangeType(text.Value, typeof(T));
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public bool TryGetOutValue<T>(string key, out T result)
        {
            result = default;
            return TryGetValue(key, ref result);
        }

        private void BuildArgDictionary()
        {
            Parse(Environment.GetCommandLineArgs(), true);
        }

        private void Parse(string[] args, bool haveExecutablePath)
        {
            IEnumerable<string> arguments = args;

            if (haveExecutablePath)
                arguments = arguments.Skip(1);

            foreach (var arg in arguments)
            {
                if (string.IsNullOrWhiteSpace(arg)) continue;

                string prefix = string.Empty;
                string key = arg;
                string value = null;

                if (key.StartsWith("--"))
                {
                    prefix = "--";
                    key = key.Substring(2);
                }
                else if (key.StartsWith("-"))
                {
                    prefix = "-";
                    key = key.Substring(1);
                }
                else if (key.StartsWith("/"))
                {
                    prefix = "/";
                    key = key.Substring(1);
                }

                int sepIdx = key.IndexOfAny(new[] { ':', '=' });
                if (sepIdx >= 0)
                {
                    value = key.Substring(sepIdx + 1);
                    if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
                        value = value.Substring(1, value.Length - 2);
                    key = key.Substring(0, sepIdx);
                }

                var argStruct = new CommandArgStruct(prefix, value);

                if (!_argsDict.ContainsKey(key))
                {
                    _argsDict.Add(key, argStruct);
                }

                _args.Add(new KeyValuePair<string, CommandArgStruct>(key, argStruct));
            }
        }

        public KeyValuePair<string, string>[] GetArgs()
        {
            return _args.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.Value)).ToArray();
        }

        public int Count => _args.Count;

        public bool IsEmpty => _args.Count == 0;

        public KeyValuePair<string, CommandArgStruct> At(int index)
        {
            if (index < 0 || index >= Count)
                throw new IndexOutOfRangeException();

            return _args[index];
        }

        public CommandLineArgsReader CreateReader()
            => new CommandLineArgsReader(this);
    }

    public struct CommandArgStruct
    {
        public string Prefix;
        public string Value;

        public CommandArgStruct(string prefix, string value)
        {
            Prefix = prefix;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is CommandArgStruct other &&
                   Prefix == other.Prefix &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            int hashCode = -493470533;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Prefix);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }

        public bool IsArgument() => Prefix == "/" || Prefix == "-" || Prefix == "--";

        public void Deconstruct(out string prefix, out string value)
        {
            prefix = Prefix;
            value = Value;
        }

        public static implicit operator (string Prefix, string Value)(CommandArgStruct value)
        {
            return (value.Prefix, value.Value);
        }

        public static implicit operator CommandArgStruct((string Prefix, string Value) value)
        {
            return new CommandArgStruct(value.Prefix, value.Value);
        }
    }
}
