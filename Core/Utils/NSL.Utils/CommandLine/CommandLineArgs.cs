using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        private const string Pattern = @"([\/-]?)((\w+)(?:[=:](""[^""]+""|[^\s""]+))?)(?:\s+|$)";

        private readonly Regex _regex = new Regex(
            Pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Dictionary<string, CommandArgStruct> _args =
            new Dictionary<string, CommandArgStruct>();

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
                return _args.ContainsKey(key) ? _args[key].Value : null;
            }
        }

        public bool ContainsKey(string key)
        {
            return _args.ContainsKey(key);
        }

        public string GetValue(string key, string defaultValue = null)
        {
            if (_args.TryGetValue(key, out var text))
                return text.Value;

            return defaultValue;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_args.TryGetValue(key, out var text))
                return (T)Convert.ChangeType(text.Value, typeof(T));

            return defaultValue;
        }

        public bool TryGetValue<T>(string key, ref T result)
        {
            if (_args.TryGetValue(key, out var text))
            {
                    result = (T)Convert.ChangeType(text.Value, typeof(T));
                return true;
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

            var regexArgs = arguments.Select(arg =>
                        _regex.Match(arg))
                .Where(m => m.Success)
                .ToArray();

            foreach (var match in regexArgs)
            {
                try
                {
                    _args.Add(
                         match.Groups[3].Value,
                         (match.Groups[1].Value,
                         match.Groups[4].Value));
                }
                // Ignore any duplicate args
                catch (Exception) { }
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

            return _args.ElementAt(index);
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

        public bool IsArgument() => Prefix == "/" || Prefix == "-";

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
