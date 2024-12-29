using System;
using System.Collections.Generic;

namespace NSL.Utils.CommandLine
{
    public struct CommandLineArgsReader
    {
        private int index;

        public CommandLineArgs Args { get; }

        public int Index { get => index; private set => _setIndex(value); }

        public CommandLineArgsReader(CommandLineArgs args)
        {
            Args = args;
            index = -1;
            current = default;
        }

        public bool HaveNext()
            => Index < Args.Count - 1;

        public bool TryNext()
        {
            if (!HaveNext())
                return false;

            ++Index;
            return true;
        }

        public bool TryBack()
        {
            if (Index < 1)
                return false;

            --Index;

            return false;
        }

        private void _setIndex(int value)
        {
            if (value < 0 || value >= Args.Count)
                throw new IndexOutOfRangeException();

            index = value;
            current = null;
        }

        private KeyValuePair<string, CommandArgStruct>? current;

        private KeyValuePair<string, CommandArgStruct> getCurrent(int? index)
        {
            if (index.HasValue)
                return Args.At(index.Value);

            current = current ?? Args.At(Index);

            return current.Value;
        }

        /// <summary>
        /// Argument starts with character '/', '-', example "/key", "-key"
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsArgument(int? index = null)
            => getCurrent(index).Value.IsArgument();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsPath(int? index = null)
            => string.IsNullOrEmpty(getCurrent(index).Value.Prefix);


        /// <summary>
        /// Arg with value e.g - "/key:value", "key:value"
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool HaveValue(int? index = null)
            => getCurrent(index).Value.Value == default;

        public T GetValue<T>(T defaultValue = default, int? index = null)
            => Args.GetValue(getCurrent(index).Key, defaultValue);


        public string GetKey(int? index = null)
            => getCurrent(index).Key;

        public CommandLineArgsReader Clone()
            => new CommandLineArgsReader(Args) { Index = Index };

    }
}
