using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NSL.Utils
{
    /// <summary>
    /// Sources : stackoverflow answers
    /// Very basic Command Line Args extracter
    /// <para>Parse command line args for args in the following format:</para>
    /// <para>/argname:argvalue /argname:argvalue /argname ...</para>
    /// </summary>
    public class CommandLineArgs
    {
        private const string Pattern = "[\\/-]?((\\w+)(?:[=:](\"[^\"]+\"|[^\\s\"]+))?)(?:\\s+|$)";

        private readonly Regex _regex = new Regex(
            Pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly Dictionary<string, string> _args =
            new Dictionary<string, string>();

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
                return _args.ContainsKey(key) ? _args[key] : null;
            }
        }

        public bool ContainsKey(string key)
        {
            return _args.ContainsKey(key);
        }

        public bool TryGetValue<T>(string key, ref T result)
        {
            if (_args.TryGetValue(key, out var text))
            {
                result = (T)Convert.ChangeType(text, typeof(T));
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

            foreach (var match in args.Skip(1).Select(arg =>
                        _regex.Match(arg)).Where(m => m.Success))
            {
                try
                {
                    _args.Add(
                         match.Groups[2].Value,
                         match.Groups[3].Value);
                }
                // Ignore any duplicate args
                catch (Exception) { }
            }
        }

        public KeyValuePair<string, string>[] GetArgs()
        {
            return _args.ToArray();
        }
    }
}
