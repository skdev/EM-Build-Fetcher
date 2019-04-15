using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.utils;

namespace EM_Build_Fetcher.commands.impl
{
    public class RemoveAutoLogin : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(RemoveAutoLogin).FullName);

        public override bool Execute(string[] args)
        {
            AppConfig.SetAutoLoginValues("", "");
            AppConfig.SaveConfig();
            SystemUtils.ClearWindowsAutoLogin();
            return true;
        }
    }
}
