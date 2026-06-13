using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Thor404Controller
{
    partial class Helpers
    {
        [DllImport("libc")]
        private static extern uint geteuid();

        public static bool IsCurrentProcessElevated()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                    .IsInRole(WindowsBuiltInRole.Administrator);
            }

            return geteuid() == 0;
        }

        public static (bool, Process) TryStartElevated()
        {
            if (IsCurrentProcessElevated())
            {
                return (false, null!);
            }

            Console.WriteLine("Missing permissions, app will ask for higher priviledges");

            var currentProcessPath = Environment.ProcessPath ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.ChangeExtension(typeof(Program).Assembly.Location, "exe")
                : Path.ChangeExtension(typeof(Program).Assembly.Location, null));

            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
            };
            ConfigureProcessStartInfo(processStartInfo, currentProcessPath);

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Could not start process");
            }

            return (true, process);
        }

        private static void ConfigureProcessStartInfo(
            ProcessStartInfo startInfo,
            string processPath)
        {
            if (File.Exists("/usr/bin/pkexec"))
            {
                startInfo.FileName = "pkexec";

                // launching the gui with pkexec makes gtk unable to open display
                // fix that by saving the env variables of this current non-elevated process
                // and pass that into pkexec
                var vars = new[]
                {
                    "DISPLAY",
                    "XAUTHORITY",
                    "WAYLAND_DISPLAY",
                    "DBUS_SESSION_BUS_ADDRESS"
                };

                startInfo.ArgumentList.Add("env");
                foreach (var name in vars)
                {
                    var value = Environment.GetEnvironmentVariable(name);
                    if (!string.IsNullOrEmpty(value))
                    {
                        startInfo.ArgumentList.Add($"{name}={value}");
                    }
                }
            }
            else if (File.Exists("/usr/bin/sudo"))
            {
                startInfo.FileName = "sudo";
            }
            else if (File.Exists("/usr/bin/doas"))
            {
                startInfo.FileName = "doas";
            }
            else
            {
                throw new PlatformNotSupportedException("Could not elevate priviledges. pkexec, sudo, doas weren't found on the system");
            }

            startInfo.ArgumentList.Add(processPath);
        }

        public static string RgbaToHex(Gdk.RGBA rgba)
        {
            var r = (byte)Math.Round(rgba.Red * 255);
            var g = (byte)Math.Round(rgba.Green * 255);
            var b = (byte)Math.Round(rgba.Blue * 255);

            return $"{r:X2}{g:X2}{b:X2}";
        }
    }
}