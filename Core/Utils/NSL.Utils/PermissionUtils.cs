using System.IO;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Principal;
using NSL.Utils.CommandLine;

namespace NSL.Utils
{
    public class PermissionUtils
    {
        public static bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
        {
            string fullPath = Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    );
            try
            {
                using (FileStream fs = File.Create(
                    fullPath,
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch (Exception ex)
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        public static bool RequireRunningAsAdministrator()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        RestartAsAdministrator(Environment.GetCommandLineArgs());
                        return true;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Cannot access to object. Try run as administrator/root");
            }

            return false;
        }

        public static void RestartAsAdministrator(string[] args)
        {
            // Get the current process's executable path
            string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            // Build the arguments string
            var arguments = ArgumentJoinUtils.JoinArguments(args.Skip(1).ToArray());


            // Start the process with administrator privileges
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas" // Request administrator privileges
            };

            try
            {
                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to restart as administrator: {ex.Message}");
            }
        }
    }
}
