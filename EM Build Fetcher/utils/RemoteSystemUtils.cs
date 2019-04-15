using EM_Build_Fetcher.logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace EM_Build_Fetcher.utils
{
    public class RemoteSystemUtils
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(RemoteSystemUtils).Name);
        private static readonly string TaskScheduler = @"C:\Windows\System32\schtasks.exe";
        private static readonly string ShutdownExe = @"C:\Windows\System32\cmd.exe";
        private static readonly string TaskName = "EM_Build_Grab";

        [DllImport("advapi32.DLL", SetLastError = true)]
        public static extern int LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, ref IntPtr phToken);

        public static bool DeployAgent(string machine, string username, string password)
        {
            var target = $"\\\\{machine}\\c$";
            var agent = Configuration.ExeLocation;
            var statusFile = $"{target}\\Kitten\\Configuration.txt";
            
            //Copy agent

            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                IntPtr token = default(IntPtr);

                var domain = username.Split('\\')[0];
                var user = username.Split('\\')[1];

                Logger.Info($"Creds: {domain} {user} {password}");

                if (LogonUser(user, domain, password, 9, 0, ref token) != 0)
                {

                    WindowsIdentity identity = new WindowsIdentity(token);
                    WindowsImpersonationContext context = identity.Impersonate();

                    try
                    {
                        File.Copy(agent, $"{target}\\Build Fetcher.exe", true);
                        //Create dir
                        Directory.CreateDirectory($"{target}\\Kitten");
                        //Copy current config to endpoint
                        File.Copy(Configuration.StatusFile, $"{target}\\Kitten\\Configuration.txt", true);
                    }
                    finally
                    {
                        context.Undo();
                    }
                } 
                else
                {
                    Logger.Info("Failed to auth user.");
                    return false;
                }

            }
            catch (Exception e)
            {
                Logger.Error($"Failed to copy tool to {target}, reason: {e.Message}");
                return false;
            }
            
            var schedule = new Process()
            {
                StartInfo = {
                    FileName = TaskScheduler,
                    Arguments = $"/create /S \"{machine}\" /U {username} /P {password} /SC ONLOGON /RL HIGHEST /TN \"{TaskName}\" /TR \"{Configuration.ExeLocation}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                }
            };

            if (!schedule.Start())
            {
                Logger.Warning("Failed to start scheduled task on remote machine.");
                return false;
            }

            var authCmd = $"NET USE {target}\\IPC$ {password} /USER:{username}";
            var rebootCmd = $"shutdown -r -m \"{machine}\" -t 60";

            //Reboot remote machine
            var shutdown = new Process()
            {
                StartInfo = {
                    FileName = ShutdownExe,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas"
                }
            };

            if (!shutdown.Start())
            {
                Logger.Warning($"Failed to restart {target} - please manually reboot.");
            }

            Logger.Info(authCmd);
            Logger.Info(rebootCmd);

            shutdown.StandardInput.WriteLine(authCmd);
            shutdown.StandardInput.WriteLine(rebootCmd);

            return true;
        }
    }
}
