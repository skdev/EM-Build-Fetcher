using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.program;
using EM_Build_Fetcher.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class InstallMSICommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(InstallMSICommand).FullName);

        public override bool Execute(string[] args)
        {
            if(args.Count() < 1)
            {
                Logger.Info($"Invalid number of arguments specified, syntax: [PathToMsi] e.g. \\\\infrastore\\Test.msi");
                return false;
            }

            var msi = args[1];
            var temp = @"C:\Kitten\Temp\test.msi";

            if(FileUtils.FileDoesNotExist("File"))
            {
                Logger.Info($"{msi} could not be found.");
                return false;
            }

            if(!DirectoryUtils.CreateDirectoryIfNotExist("C:\\Kitten\\Temp"))
            {
                Logger.Info($"Failed to create temp directory for MSI");
                return false;
            }

            File.Copy(msi, temp);

            var install = Installer.Install(msi, $"C:\\Kitten\\Temp\\{msi}.log");

            if(install == ExitCode.SucessNeedReboot)
            {
                Logger.Info("Successfully install {msi} - reboot required. Restarting.");
                SystemUtils.RebootComputer(60);
                return true;
            }

            return install == ExitCode.Success;
        }
    }
}
