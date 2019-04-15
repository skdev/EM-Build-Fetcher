using EM_Build_Fetcher.commands;
using EM_Build_Fetcher.commands.impl;
using EM_Build_Fetcher.exceptions;
using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.program;
using EM_Build_Fetcher.server;
using EM_Build_Fetcher.utils;
using EM_Build_Fetcher.watcher;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.IO;
using System.Linq;

namespace EM_Build_Fetcher
{
    public class Program
    {
        //This logger will get replaced with system logger once everything has started
        private static Logger Logger = LoggerFactory.GetSimpleLogger(typeof(Program).Name);
        private CommandRepo CommandRepo = new CommandRepo();
        private DirectoryWatcher _watcher;

        private bool Start()
        {
		    //Before we start the program - check if anything needs to be done (e.g. installing the product)
						
            AppConfig.ReadConfigFile();

            if(AppConfig.AutoHideEnabled())
            {
                SystemUtils.HideConsole();
            }

            if(AppConfig.Autologin())
            {
                Logger.Info("Autologin settings detected.");
                SystemUtils.SetWindowsAutoLogin(AppConfig.AutoLoginUsername(), AppConfig.AutoLoginPassword());
            }
            else
            {
                Logger.Info("No autologin set.");
                SystemUtils.ClearWindowsAutoLogin();
            }

            if (AppConfig.NeedInstall() && !HandleProductInstall(AppConfig.GetProductCode()))
            {
                Logger.Error("We had some issues installing the product., please install manually.");
            }

			//If no directory was previously specified - prompt the user for the location of the builds
						
            if (string.IsNullOrWhiteSpace(AppConfig.GetMonitoredDirectory()))
            {
                ManualSetup();

                /*  var config = RequestConfig();
                  if(string.IsNullOrWhiteSpace(config) || string.IsNullOrEmpty(config))
                  {
                      ManualSetup();
                  }
                  else
                  {
                      if(!FileUtils.CopyFile(new FileInfo(config), @"C:\Kitten", true))
                      {
                          Logger.Warning($"Error loading {config}, please setup manually.");
                          ManualSetup();
                      }
                  }*/

            }

            if((DirectoryUtils.IsDirectoryEmpty(Configuration.DropLocation) 
                && DirectoryUtils.IsDirectoryEmpty(Configuration.CacheLocation) 
                && !DirectoryUtils.IsDirectoryEmpty(AppConfig.GetMonitoredDirectory())) 
                || !IsLatestBuild())
            {
                var path = new DirectoryInfo(AppConfig.GetMonitoredDirectory())
                                            .GetDirectories()
                                            .OrderByDescending(d => d.LastWriteTimeUtc).First().FullName;

                if (AppConfig.GetProductCode() == Product.EM)
                {
                    var watcher = new EmBuildDropsWatchHandler(AppConfig.GetMonitoredDirectory());
                    if(watcher.DoWork(path) == State.Install)
                    {
                        if (!HandleProductInstall(AppConfig.GetProductCode()))
                        {
                            Logger.Error("We had some issues installing the product., please install manually.");
                        }
                        else
                        {
                            Logger.Info("Successfully install EM!");
                        }
                    }
                }
                else if (AppConfig.GetProductCode() == Product.FD)
                {
                    var watcher = new FDBuildDropsWatchHandler(AppConfig.GetMonitoredDirectory());
                    watcher.DoWork(path);
                }

                if (AppConfig.NeedInstall() && !HandleProductInstall(AppConfig.GetProductCode()))
                {
                    Logger.Error("We had some issues installing the product., please install manually.");
                }

            }

			//Start monitoring the build location for new builds
						
            if(!StartWatchService(AppConfig.GetMonitoredDirectory(), AppConfig.GetProductCode()))
            {
                Logger.Warning($"Could not start watch service, please see log file @ {Configuration.LogFile}");
                return false;
            }

            Logger.Info($"Watching {AppConfig.GetMonitoredDirectory()} for builds.");

			//Setup and start processing user input commands
						
            if(!BindCommands())
            {
                Logger.Warning("Commands failed to bind. See logs.");
                return false;
            }

            StartCommandProcessing();

            return true;
        }

        private void ManualSetup()
        {
            AppConfig.SetMonitoredDirectory(RequestUserForDirectory());
            AppConfig.SetProductCode(RequestUserForProductCode());
            AppConfig.SaveConfig();
        }

        private bool IsLatestBuild()
        {
            if(string.IsNullOrWhiteSpace(AppConfig.GetMonitoredDirectory()) || string.IsNullOrWhiteSpace(AppConfig.CurrentDropLocation()))
            {
                return true;
            }

            if(DirectoryUtils.IsDirectoryEmpty(AppConfig.GetMonitoredDirectory()))
            {
                return true;
            }

            var latestBuild = new DirectoryInfo(AppConfig.GetMonitoredDirectory())
                        .GetDirectories()
                        .OrderByDescending(d => d.LastWriteTimeUtc).First();

            var previousDrop = new DirectoryInfo(AppConfig.CurrentDropLocation());

            if(latestBuild.LastWriteTime > previousDrop.LastWriteTime)
            {
                Logger.Info("A new build is available");
                return false;
            }

            Logger.Info("Product is up-to-date."); 
            return true;
        }

        private bool HandleProductInstall(Product product)
        {
            switch (product)
            {
                case Product.EM:
                    return InstallEM();
                case Product.FD:
                    return InstallFD();
                case Product.Unspecified:
                    return false;
                default:
                    return false;
            }
        }

        private string RequestConfig()
        {
            Logger.Info("If you have a config, please enter the location now otherwise leave blank to run through setup...");

            var input = string.Empty;
            var valid = false;

            while(!valid)
            {
                var path = SystemUtils.GetUserInput();
                if(string.IsNullOrWhiteSpace(path))
                {
                    valid = true;
                    break;
                }
                
                if(FileUtils.FileDoesNotExist(path))
                {
                    Logger.Warning($"Cannot find config file: '{path}', please try again.");
                }
                else
                {
                    input = path;
                    valid = true;
                }

            }

            return input;
        }

        private string RequestUserForDirectory()
        {
            var verifiedInput = false;
            var directory = "";

            while (!verifiedInput)
            {
                Logger.Info("Enter directory to monitor: ");
                directory = SystemUtils.GetUserInput();
                if (DirectoryUtils.DirectoryDoesNotExist(directory))
                {
                    Logger.Warning($"Build location {directory} cannot be found, please try again.");
                }
                else
                {
                    verifiedInput = true;
                }
            }

            return directory;
        }

        private Product RequestUserForProductCode()
        {
            Product p = Product.Unspecified;
            var validProductCode = false;
            while (!validProductCode)
            {
                Logger.Info("Enter product code (EM or FD): ");
                var input = SystemUtils.GetUserInput();
                if (input.ToLower().Contains("FD".ToLower()))
                {
                    p = Product.FD;
                    validProductCode = true;
                }
                else if (input.ToLower().Contains("EM".ToLower()))
                {
                    p = Product.EM;
                    validProductCode = true;
                }
                else
                {
                    Logger.Warning($"Unknown product code: {input}");
                }
            }
            return p;
        }

        public bool StartWatchService(string directory, Product product)
        {
            Logger.Info($"Starting watcher on {directory}");

            try
            {
                switch (product)
                {
                    case Product.EM:
                        _watcher = DirectoryWatcher.CreateDirectoryWatcherStarted(directory, new EmBuildDropsWatchHandler(directory));
                        break;
                    case Product.FD:
                        _watcher = DirectoryWatcher.CreateDirectoryWatcherStarted(directory, new FDBuildDropsWatchHandler(directory));
                        break;
                    case Product.Unspecified:
                        Logger.Warning("Error: Could not start watch service as no product was specified.");
                        break;
                }
                
                return _watcher.Running;
            }
            catch (DirectoryNotFoundException)
            {
                Logger.Warning($"Could not find directory: {directory}");
                return false;
            }
            catch (NullReferenceException)
            {
                Logger.Error("EMBuildDropsWatchHandler is null");
                return false;
            }
            catch (WatcherAlreadyRunningException)
            {
                Logger.Warning("Watch service has already started.");
                return false;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }
        }

        public bool RestartEmWatchService(string directory, string target, Product product)
        {
            var watcherRunning = false;

            if (_watcher != null && _watcher.Running)
            {
                try
                {
                   watcherRunning = _watcher.Stop();
                }
                catch (Exception e)
                {
                    Logger.Error($"Unable to stop the previous watch service: {e.Message}");
                    return false;
                }
            }

            return !watcherRunning && StartWatchService(directory, product);
        }

        private bool InstallFD()
        {
            var directory = $"{Configuration.DropLocation}\\{Configuration.MsiFolder}";
            var is64Bit = Environment.Is64BitOperatingSystem;

            var dataNow = is64Bit
                ? $"{directory}\\DataNow_x64.msi"
                : $"{directory}\\DataNow_x86.msi";

            var testHarness = is64Bit
                ? $"{directory}\\DataNowTestHarness_x64.msi"
                : $"{directory}\\DataNowTestHarness_x86.msi";

            var toolbox = is64Bit
                ? $"{directory}\\DataNowToolbox64.msi"
                : $"{directory}\\DataNowToolbox32.msi";


            Logger.Info($"Installing FD from location: {directory}");

            var dataNowExit = Installer.Install(dataNow, "FD_DataNow.log");

            Logger.Info($"Exit Code DataNow {dataNowExit}");

            if (!(dataNowExit == ExitCode.Success || dataNowExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing DataNow, you will need to manually install this.");
            }

            var testHarnessExit = Installer.Install(testHarness, "FD_TestHarness.log");
            if (!(testHarnessExit == ExitCode.Success || testHarnessExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing TestHarness, you will need to manually install this.");
            }

            var toolBoxExit = Installer.Install(toolbox, "FD_ToolBox.log");
            if (!(toolBoxExit == ExitCode.Success || toolBoxExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing Toolboxt, you will need to manually install this.");
            }

            AppConfig.SetNeedInstall(false);
            AppConfig.SetProductCode(Product.FD);
            AppConfig.SaveConfig();

            Logger.Info("Computer requires reboot, will reboot in under 1 minute.");
            SystemUtils.RebootComputer(60);

            return true;
        }

        private bool InstallEM()
        {
            var directory = $"{Configuration.DropLocation}\\{Configuration.MsiFolder}";
            var is64Bit = Environment.Is64BitOperatingSystem;

            var console = is64Bit
                ? $"{directory}\\EnvironmentManagerConsole64.msi"
                : $"{directory}\\EnvironmentManagerConsole32.msi";

            var agent = is64Bit
                ? $"{directory}\\EnvironmentManagerAgent64.msi"
                : $"{directory}\\EnvironmentManagerAgent32.msi";

            var tools = is64Bit
                ? $"{directory}\\EnvironmentManagerTools64.msi"
                : $"{directory}\\EnvironmentManagerTools32.msi";

            Logger.Info($"Installing EM from location: {directory}");

            var toolsExit = Installer.Install(tools, "Em_Tools.log");

            Logger.Info($"Exit Code Tools {toolsExit}");

            if (!(toolsExit == ExitCode.Success || toolsExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing EM Tools, you will need to manually install this.");
            }

            var consoleExit = Installer.Install(console, "Em_Console.log");
            if (!(consoleExit == ExitCode.Success || consoleExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing EM Console, you will need to manually install this.");
            }

            var agentExit = Installer.Install(agent, "Em_Agent.log");
            if (!(agentExit == ExitCode.Success || agentExit == ExitCode.SucessNeedReboot))
            {
                Logger.Error("There was an error installing EM Agent, you will need to manually install this.");
            }

            AppConfig.SetNeedInstall(false);
            AppConfig.SetProductCode(Product.EM);
            AppConfig.SaveConfig();

            Logger.Info("Computer requires reboot, will reboot in under 1 minute.");
            SystemUtils.RebootComputer(60);
            return true;
        }

        private bool BindCommands()
        {
            var exit = CommandRepo.Add("exit", new ExitCommand());
            if (!exit)
            {
                Logger.Warning("Failed to bind exit command.");
            }

            var help = CommandRepo.Add("help", new HelpCommand());
            if (!help)
            {
                Logger.Warning("Failed to bind help command.");
            }

            var restart = CommandRepo.Add("restart", new RestartWatcherCommand());
            if (!restart)
            {
                Logger.Warning("Failed to bind restart command.");
            }

            var hide = CommandRepo.Add("hide", new HideConsoleCommand());
            if (!hide)
            {
                Logger.Warning("Failed to bind hide command.");
            }

            var autohide = CommandRepo.Add("autohide", new AutoHideCommand());
            if (!hide)
            {
                Logger.Warning("Failed to bind autohide command.");
            }

            var remote = CommandRepo.Add("deploy", new RemoteDeployCommand());
            if (!remote)
            {
                Logger.Warning("Failed to bind deploy command.");
            }

            var autologin = CommandRepo.Add("autologin", new AutoLoginCommand());
            if(!autologin)
            {
                Logger.Warning("Failed to bind autologin command.");
            }

            var removeAutoLogin = CommandRepo.Add("removeautologin", new RemoveAutoLogin());
            if (!autologin)
            {
                Logger.Warning("Failed to bind removeautologin command.");
            }

            var rollback = CommandRepo.Add("rollback", new RollbackCommand());
            if (!autologin)
            {
                Logger.Warning("Failed to bind rollback command.");
            }

            var query = CommandRepo.Add("query", new QueryDNSCommand());
            if (!query)
            {
                Logger.Warning("Failed to bind query command.");
            }

            var cmd = CommandRepo.Add("cmd", new CommandPromptCommand());
            if (!cmd)
            {
                Logger.Warning("Failed to bind cmd command.");
            }

            var installMSI = CommandRepo.Add("install", new InstallMSICommand());
            if (!cmd)
            {
                Logger.Warning("Failed to bind install command.");
            }

            var applyConfig = CommandRepo.Add("applyconfig", new ApplyConfigCommand());
            if (!applyConfig)
            {
                Logger.Warning("Failed to bind applyconfig command.");
            }

            var binded = exit && help && restart && hide && remote && autologin && removeAutoLogin && rollback && query && applyConfig;

            return binded;
        }

        private bool ParseInputAndRunCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            var arguments = input.Split(' ');
            var cmd = arguments[0];

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return false;
            }

            if (!CommandRepo.Contains(cmd))
            {
                Logger.Info($"No internal command: {cmd}");
                return false;
            }

            return CommandRepo.Get(cmd).Execute(arguments);
        }

        private void StartCommandProcessing()
        {
            Logger.Info("If you need some advice, enter 'help'");

            var input = "";

            while ((input = SystemUtils.GetUserInput()) != null)
            {
                Logger.Info(ParseInputAndRunCommand(input) ? $"Successfully executed action {input}" : $"Failed to execute action: {input}");
            }
        }

        private bool Setup()
        {
            return CreateLocation(Configuration.RootFolder) && CreateLocation(Configuration.LogDir) && CreateLocation(Configuration.WebPath) &&  CreateLocation(Configuration.CacheLocation) && CreateLocation(Configuration.DropLocation) && CreateLocation(Configuration.LogDir) && SystemUtils.CreateScheduledTask();
        }

        private bool CreateLocation(string location)
        {
            Logger.Trace($"Creating {location}");

            var created = DirectoryUtils.CreateDirectoryIfNotExist(location);

            if(!created)
            {
                Logger.Warning($"Failed to create {location}");
            }

            return created;
        }

        private bool EnsureInCDrive()
        {
            if(FileUtils.FileDoesNotExist(Configuration.ExeLocation))
            {
                File.Copy(@"\\infrastore.testing.local\Scratch\Suraj_Kumar\Build Fetcher.exe", Configuration.ExeLocation);
            }
            return FileUtils.FileExists(Configuration.ExeLocation);
        }

        public static void Main(string[] args)
        {
            var program = new Program();

            //TODO: Uncomment
            if (!program.EnsureInCDrive())
            {
                Console.WriteLine(@"Please place 'Build Fetcher.exe' in 'C:\' and restart.");
                SystemUtils.PauseConsole();
                return;
            } 

            if (!program.Setup())
            {
                Console.WriteLine("Failed to setup application.");
                SystemUtils.PauseConsole();
                return;
            }

            Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(Program).Name);

            try
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (FileUtils.FileDoesNotExist($"{desktop}\\Web Portal.html"))
                {
                    Logger.Info("Creating shortcut on desktop to web portal...");
                    File.Copy(@"\\infrastore.testing.local\Scratch\Suraj_Kumar\EM Build Fetcher\Web Portal.html", $"{desktop}\\Web Portal.html", true);
                }

                if (FileUtils.FileDoesNotExist($"{Configuration.PortalLocation}"))
                {
                    File.Copy(@"\\infrastore.testing.local\Scratch\Suraj_Kumar\EM Build Fetcher\Web Portal.html", $"{Configuration.PortalLocation}", true);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to copy web portal onto desktop: " + e.Message);
            }

            Logger.Info("Configuring HTTP server");
            try
            {
                new Server(4444, program.CommandRepo);
                Logger.Info("Finished configuring HTTP server: http://localhost:4444");
            }
            catch (Exception e)
            {
                Logger.Warning("Failed to setup HTTP server: " + e.Message);
            }

            if (!program.Start())
            {
                Logger.Error("Failed to start application.");
            }
            
        }
    }
}