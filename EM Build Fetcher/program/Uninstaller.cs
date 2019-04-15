using EM_Build_Fetcher.logging;
using System;
using System.Diagnostics;
using System.Management;

namespace EM_Build_Fetcher.program
{
    public class Uninstaller
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(Uninstaller).Name);
        private static readonly string MsiExec = @"C:\Windows\System32\msiexec.exe";

        public static ExitCode Uninstall(string msi)
        {
            Logger.Info($"Uninstalling {msi}");

            var process = new Process()
            {
                StartInfo = {
                    FileName = MsiExec,
                    Arguments = $"/x {msi} /norestart /passive",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    Verb = "runas"
                }
            };

            if (!process.Start())
            {
                Logger.Warning("Failed to start 'msiexec.exe'");
                return ExitCode.FunctionFailed;
            }

            process.WaitForExit();

            var exitCode = process.ExitCode;

            Console.WriteLine("Exit: " + exitCode);

            if (exitCode == (int)ExitCode.Success)
            {
                return ExitCode.Success;
            }

            if (exitCode == (int)ExitCode.SucessNeedReboot)
            {
                return ExitCode.SucessNeedReboot;
            }

            return ExitCode.FunctionFailed;
        }
    }
}
