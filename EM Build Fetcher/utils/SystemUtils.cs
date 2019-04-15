using EM_Build_Fetcher.logging;
using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace EM_Build_Fetcher.utils
{
    public class SystemUtils
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(SystemUtils).Name);
        private static readonly string ShutdownExe = @"C:\Windows\System32\shutdown.exe";
        private static readonly string TaskScheduler = @"C:\Windows\System32\schtasks.exe";
        private static readonly string TaskName = "EM_Build_Grab";

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        public static void HideConsole(int show = 0) {
            var hWnd = GetConsoleWindow();
            if (hWnd != IntPtr.Zero)
            {
                ShowWindow(hWnd, show);
            }
        }
       
        public static void PauseConsole() => Console.Read();

        public static string GetUserInput() => Console.ReadLine();

        public static void RebootComputer(int seconds)
        {
            var process = new Process()
            {
                StartInfo = {
                    FileName = ShutdownExe,
                    Arguments = $"-r -t {seconds}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                }
            };

            if (!process.Start())
            {
                Logger.Warning("Failed to start execute shutdown command. Please manually reboot.");
                return;
            }
        }

        private static bool ScheduledTaskExists()
        {
            var process = new Process()
            {
                StartInfo = {
                    FileName = TaskScheduler,
                    Arguments = @"/query",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };

            if (!process.Start())
            {
                Logger.Warning("Failed to start scheduled task.");
                return false;
            }

            var output = process.StandardOutput.ReadToEnd();

            if(output.Contains(TaskName))
            {
                return true;
            }

            return false;
        }

        public static bool CreateScheduledTask()
        {
            if(ScheduledTaskExists())
            {
                Logger.Info("Scheduled task already exists.");
                return true;
            }

            var process = new Process()
            {
                StartInfo = {
                    FileName = TaskScheduler,
                    Arguments = $"/create /SC ONLOGON /RL HIGHEST /TN \"{TaskName}\" /TR \"{Configuration.ExeLocation}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                }
            };

            if (!process.Start())
            {
                Logger.Warning("Failed to start scheduled task.");
                return false;
            }

            Logger.Info("Scheduled task has been created.");
            return true;
        }

        public static void SetWindowsAutoLogin(string username, string password)
        {
            try
            {
                var rekey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true);

                if (rekey == null)
                {
                    Logger.Error("Error setting autologin values.");
                    return;
                }

                rekey.SetValue("AutoAdminLogon", "1", RegistryValueKind.String);
                rekey.SetValue("DefaultUserName", username, RegistryValueKind.String);
                rekey.SetValue("DefaultPassword", password, RegistryValueKind.String);

                rekey.Close();
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to set autologin values: {e.Message}");
            }
        }

        public static void ClearWindowsAutoLogin()
        {
            try
            {
                var rekey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Winlogon", true);

                if (rekey == null)
                {
                    Logger.Error("Error removing autologin values.");
                    return;
                }

                rekey.DeleteValue("DefaultUserName", false);
                rekey.DeleteValue("DefaultPassword", false);
                rekey.DeleteValue("AutoAdminLogon", false);
                rekey.Close();
            }
            catch (Exception e)
            {
                Logger.Error($"Unable to clear autologin values: {e.Message}");
            }
        }
      
    }
}