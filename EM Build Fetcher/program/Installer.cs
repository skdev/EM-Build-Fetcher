using EM_Build_Fetcher.logging;
using System.Diagnostics;

namespace EM_Build_Fetcher.program
{
    public class Installer
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(Installer).Name);
        private static readonly string MsiExec = @"C:\Windows\System32\msiexec.exe";

        public static ExitCode Install(string msi, string logFile = "")
        {
            var args = $"/i \"{msi}\" /l \"{Configuration.RootFolder}\\{logFile}\" /passive /norestart";
            Logger.Info($"Installing {msi} using command: {args}");

            var process = new Process()
            {
                StartInfo = {
                    FileName = MsiExec,
                    Arguments = args,
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

            Logger.Trace($"Exit code for {msi} is {exitCode}");

            if (exitCode == (int)ExitCode.Success)
            {
                return ExitCode.Success;
            }

            if (exitCode == (int) ExitCode.SucessNeedReboot)
            {
                return ExitCode.SucessNeedReboot;
            }

            return ExitCode.FunctionFailed;
        }
    }
}
