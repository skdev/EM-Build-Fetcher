using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.utils;

namespace EM_Build_Fetcher.commands.impl
{
    public class AutoLoginCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(AutoLoginCommand).FullName);

        public override bool Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Info($"Invalid number of arguments specified, syntax: [username] [password] e.g. testing\admin password1");
                return false;
            }

            var username = args[1];
            var password = args[2];

            AppConfig.SetAutoLoginValues(username, password);
            AppConfig.SaveConfig();

            SystemUtils.SetWindowsAutoLogin(username, password);

            return true;
        }
    }
}