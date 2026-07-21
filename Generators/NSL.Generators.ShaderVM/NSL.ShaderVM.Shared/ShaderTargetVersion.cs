using System;

namespace NSL.ShaderVM
{
    public static class ShaderTargetVersion
    {
        public const string Default = "";

        public static bool Satisfies(string target, string minRequired)
        {
            if (string.IsNullOrEmpty(minRequired)) return true;

            if (string.IsNullOrEmpty(target)) return true;

            return string.Equals(target, minRequired, StringComparison.OrdinalIgnoreCase);
        }
    }
}
