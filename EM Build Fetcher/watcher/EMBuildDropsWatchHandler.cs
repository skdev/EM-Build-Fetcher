using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.program;
using EM_Build_Fetcher.utils;
using EM_Build_Fetcher.watcher;
using System;
using System.IO;
using System.Threading;

namespace EM_Build_Fetcher
{
    public class EmBuildDropsWatchHandler : IDirectoryWatchHandler
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(EmBuildDropsWatchHandler).FullName);
        private static string Console;
        private static string Agent;
        private static string Tools;

        private readonly string _target;
        private readonly string _monitoredDir;

        /// <param name="target">The location the latest build will save to</param>
        /// <exception cref="System.NullReferenceException"></exception>
        public EmBuildDropsWatchHandler(string monitoredDir)
        {
            _monitoredDir = monitoredDir ?? throw new NullReferenceException();
            _target = Configuration.DropLocation;

            var is64Bit = Environment.Is64BitOperatingSystem;

            Console = is64Bit 
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerConsole64.msi" 
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerConsole32.msi";

            Agent = is64Bit 
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerAgent64.msi" 
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerAgent32.msi";

            Tools = is64Bit 
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerTools64.msi" 
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\EnvironmentManagerTools32.msi";
        }

        public void Created(object sender, FileSystemEventArgs args)
        {
            var path = args.FullPath;

            Logger.Trace($"Notified that {path} was created");

            //This class is Em BuildDrops specific, we just want to work on the root directory of the builds
            //i.e. we want to copy the whole of 10.1.0_650.0 and not individual files that come in.
            if (DirectoryUtils.DirectoryDoesNotExist(path) || !path.Contains("10"))
            {
                Logger.Trace($"{path} is not an EM build.");
                return;
            }

            Logger.Info($"Found a new build: {path}");

            //sleep gives any files being copied into the directory time to be added.
            //Without this wait, files may not be fully copied or missed.
            //This seems like the best/easist way for now because, even if we check if the files we want were added,
            //the build will still be in progress after a file has been placed in dir (you can see the size of the file 
            //increasing if you montior manually).
            //

            Logger.Info("Waiting for build to complete - this may take a while");

            Thread.Sleep(TimeSpan.FromHours(3));

            //  Thread.Sleep(TimeSpan.FromSeconds(10));

            //If the directory is still empty, well... the build probably failed.
            //There is still the chance files may be taking longer than 2 hours to copy,
            //in that case, we missed the build.
            if (DirectoryUtils.IsDirectoryEmpty(path) && !FileUtils.IsFile(path))
            {
                Logger.Warning("EM build has failed.");
                return;
            }

            DoWork(path);
        }

        public State DoWork(string path)
        {
            var rebootRequired = false;
            if (!DirectoryUtils.IsDirectoryEmpty(Configuration.CacheLocation))
            {
                Logger.Info("Uninstalling old EM");

                if (!UninstallEmTools() || !UninstallConsole() || !UninstallAgent())
                {
                    Logger.Error("Failed to uninstall EM, stopping.");
                    return State.Idle;
                }
                else
                {
                    rebootRequired = true;
                }

                Logger.Trace($"Cleaning directory: {Configuration.CacheLocation}");

                try
                {
                    var cleaned = DirectoryUtils.CleanDirectory($"{Configuration.CacheLocation}");
                    if (!cleaned)
                    {
                        Logger.Error($"Failed to clean download directory for updates: {Configuration.CacheLocation}");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to clean download directory for updates: {Configuration.CacheLocation} : {e.Message}");
                    return State.Idle;
                }
            }

            Logger.Info("Backing up old EM installation");

            try
            {
                DirectoryUtils.CopyDirectory(path, Configuration.CacheLocation, true);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to fetch new EM update: {e.Message}");
                return State.Idle;
            }

            Logger.Trace($"Cleaning directory: {_target}");

            try
            {
                DirectoryUtils.CleanDirectory(_target);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to clean download directory for updates: {_target}: {e.Message}");
                return State.Idle;
            }

            Logger.Trace($"Copying {path} to {_target}");

            //_target will look like: \\Directory\Latest_Build\blah\blah\blah\*.msi
            //Optionally we could add some code so the directory is like: \\Directory\Latest_Build\10.1.650_0\blah\blah.msi
            try
            {
                DirectoryUtils.CopyDirectory(path, _target, true);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to fetch new EM update: {e.Message}");
                return State.Idle;
            }

            AppConfig.SetCurrent($"{path}");
            AppConfig.SetNeedInstall(true);
            AppConfig.SaveConfig();

            Logger.Info("Successfully fetched latest EM build!");

            if(rebootRequired)
            {
                Logger.Info("New updates will install after reboot. Computer requires reboot, will reboot in under 1 minute.");
                SystemUtils.RebootComputer(60);
                return State.Idle;
                
            }
            return State.Install;
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

        public void Changed(object sender, FileSystemEventArgs args)
        {
            //Ignoring changed
        }

        public void Renamed(object sender, FileSystemEventArgs args)
        {
            //Ignoring renamed
        }

        public void Deleted(object sender, FileSystemEventArgs args)
        {
            var path = args.FullPath;

            Logger.Warning($"{path} was deleted - This may be due to a failed build.");
        }

        public void Error(object sender, ErrorEventArgs args)
        {
            Logger.Error($"Error: {args}");
        }
    }
}
