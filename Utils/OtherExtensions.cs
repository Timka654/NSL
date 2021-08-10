using System;
using System.Reflection;

namespace Utils
{
    public static class OtherExtensions
    {
        public static string GetAppVersion(this Assembly assembly)
        {
            Version version = assembly.GetName().Version;
            DateTime buildDate = new DateTime(2000, 1, 1)
                .AddDays(version.Build).AddSeconds(version.Revision * 2);
            return $"{version} ({buildDate})";
        }
    }
}
