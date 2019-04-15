using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.program;
using EM_Build_Fetcher.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class RollbackCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(RollbackCommand).FullName);
        private static string Console;
        private static string Agent;
        private static string Tools;

        public RollbackCommand()
        {
            var is64Bit = Environment.Is64BitOperatingSystem;

            Console = is64Bit 
                ? $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerConsole64.msi" 
                : $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerConsole32.msi";

            Agent = is64Bit 
                ? $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerAgent64.msi" 
                : $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerAgent32.msi";

            Tools = is64Bit 
                ? $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerTools64.msi" 
                : $"{Configuration.DropLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerTools32.msi";
        }

        public override bool Execute(string[] args)
        {
            if (!UninstallEmTools() || !UninstallConsole() || !UninstallAgent())
            {
                Logger.Error("Failed to uninstall EM, stopping.");
                return false;
            }

            //reboot
            AppConfig.SetNeedInstall(true);
            AppConfig.SaveConfig();

            DirectoryUtils.CopyDirectory(Configuration.CacheLocation, Configuration.DropLocation, true);
            SystemUtils.RebootComputer(60);

            return true;
        }

        private bool UninstallEmTools()
        {
            var uninstallTools = Uninstaller.Uninstall(Tools);
            if (!(uninstallTools == ExitCode.Success || uninstallTools == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {uninstallTools}, Exit code: {uninstallTools}");
                return false;
            }
            return true;
        }

        private bool UninstallConsole()
        {
            var uninstallConsole = Uninstaller.Uninstall(Console);
            if (!(uninstallConsole == ExitCode.Success || uninstallConsole == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {Console}, Exit code: {uninstallConsole}");
                return false;
            }
            return true;
        }

        private bool UninstallAgent()
        {
            var uninstallAgent = Uninstaller.Uninstall(Agent);
            if (!(uninstallAgent == ExitCode.Success || uninstallAgent == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {Agent}, Exit code: {uninstallAgent}");
                return false;
            }
            return true;
        }

    }
}
