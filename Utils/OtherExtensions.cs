using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
