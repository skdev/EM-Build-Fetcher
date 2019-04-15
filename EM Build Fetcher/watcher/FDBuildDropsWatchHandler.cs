using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.program;
using EM_Build_Fetcher.utils;
using System;
using System.IO;
using System.Threading;


namespace EM_Build_Fetcher.watcher
{
    public class FDBuildDropsWatchHandler : IDirectoryWatchHandler
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(EmBuildDropsWatchHandler).FullName);
        private static string DataNow;
        private static string TestHarness;
        private static string ToolBox;

        private readonly string _target;
        private readonly string _monitoredDir;

        /// <param name="target">The location the latest build will save to</param>
        /// <exception cref="System.NullReferenceException"></exception>
        public FDBuildDropsWatchHandler(string monitoredDir)
        {
            _monitoredDir = monitoredDir ?? throw new NullReferenceException();
            _target = Configuration.DropLocation;

            var is64Bit = Environment.Is64BitOperatingSystem;

            DataNow = is64Bit
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNow_x64.msi"
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNow_x86.msi";

            TestHarness = is64Bit
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNowTestHarness_x64.msi"
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNowTestHarness_x86.msi";

            ToolBox = is64Bit
                ? $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNowToolbox64.msi"
                : $"{Configuration.CacheLocation}\\{Configuration.MsiFolder}\\DataNowToolbox32.msi";
        }

        public void Created(object sender, FileSystemEventArgs args)
        {
            var path = args.FullPath;

            Logger.Trace($"Notified that {path} was created");

            //This class is Em BuildDrops specific, we just want to work on the root directory of the builds
            //i.e. we want to copy the whole of 10.1.0_650.0 and not individual files that come in.
            if (DirectoryUtils.DirectoryDoesNotExist(path) || !path.Contains("9"))
            {
                Logger.Trace($"{path} is not an FD build.");
                return;
            }

            Logger.Info($"Found a new build: {path}");

            Logger.Info("Waiting for build to complete - this may take a while");

             Thread.Sleep(TimeSpan.FromHours(1));

            //Thread.Sleep(TimeSpan.FromSeconds(10));

            //If the directory is still empty, well... the build probably failed.
            //There is still the chance files may be taking longer than 2 hours to copy,
            //in that case, we missed the build.
            if (DirectoryUtils.IsDirectoryEmpty(path) && !FileUtils.IsFile(path))
            {
                Logger.Warning("FD build has failed.");
                return;
            }

            DoWork(path);
        }

        public void DoWork(string path)
        {
            if (!DirectoryUtils.IsDirectoryEmpty(Configuration.CacheLocation))
            {
                Logger.Info("Uninstalling old FD");

                if (!UninstallDataNow() || !UninstallTestHarness() || !UninstallToolBox())
                {
                    return;
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
                    return;
                }
            }

            Logger.Info("Backing up old FD installation");

            try
            {
                DirectoryUtils.CopyDirectory(path, Configuration.CacheLocation, true);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to fetch new EM update: {e.Message}");
                return;
            }

            Logger.Trace($"Cleaning directory: {_target}");

            try
            {
                DirectoryUtils.CleanDirectory(_target);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to clean download directory for updates: {_target}: {e.Message}");
                return;
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
                return;
            }

            AppConfig.SetNeedInstall(true);
            AppConfig.SaveConfig();

            Logger.Info("Successfully fetched latest EM build! New updates will install after reboot.");
            Logger.Info("Computer requires reboot, will reboot in under 1 minute.");

            SystemUtils.RebootComputer(60);
        }

        private bool UninstallDataNow()
        {
            var uninstallTools = Uninstaller.Uninstall(DataNow);
            if (!(uninstallTools == ExitCode.Success || uninstallTools == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {uninstallTools}, Exit code: {uninstallTools}");
                return false;
            }
            return true;
        }

        private bool UninstallTestHarness()
        {
            var uninstallTools = Uninstaller.Uninstall(TestHarness);
            if (!(uninstallTools == ExitCode.Success || uninstallTools == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {uninstallTools}, Exit code: {uninstallTools}");
                return false;
            }
            return true;
        }

        private bool UninstallToolBox()
        {
            var uninstallTools = Uninstaller.Uninstall(ToolBox);
            if (!(uninstallTools == ExitCode.Success || uninstallTools == ExitCode.SucessNeedReboot))
            {
                Logger.Error($"Failed to uninstall: {uninstallTools}, Exit code: {uninstallTools}");
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
