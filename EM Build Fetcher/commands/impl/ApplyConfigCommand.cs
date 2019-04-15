using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class ApplyConfigCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(ApplyConfigCommand).FullName);

        public override bool Execute(string[] args)
        {
            if(args.Length < 5)
            {
                Logger.Warning("Invalid number of arguments specified, syntax: [machine] [directory] [product] [autohide] [autologin_username] [autologin_password]");
            }

            var machine = args[1];
            var needInstall = false;
            var directory = args[2];
            var product = args[3];

            bool.TryParse(args[4], out bool autohide);

            var previousDrop = "";
            var autologin_username = args[5];
            var autologin_password = args[6];

            var location = $"{machine}\\c$\\Kitten\\Configuration.txt";

            if (FileUtils.FileDoesNotExist(location) && !FileUtils.CreateFile(location))
            {
                Logger.Error("Error: Could not create status file: " + location);
                return false;
            }

            if (!FileUtils.WriteToFile(location, new string[] { $"{needInstall}", directory, $"{product}", $"{autohide}", $"{previousDrop}", $"{autologin_username}", $"{autologin_password}" }, false, true))
            {
                Logger.Error("Failed to write to configuration file.");
                return false;
            }

            return true;
        }
    }
}
