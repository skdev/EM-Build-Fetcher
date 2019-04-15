using EM_Build_Fetcher.logging;

namespace EM_Build_Fetcher.commands.impl
{
    public class HelpCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetAnonymousLogger();

        public override bool Execute(string[] args)
        {
            Logger.Info("");
            Logger.Info("=== List of commands === ");
            Logger.Info("help");
            Logger.Info("hide");
            Logger.Info("deploy [machine] [username] [password]");
            Logger.Info("autohide");
            Logger.Info("autologin [username] [password]");
            Logger.Info("removeautologin");
            Logger.Info("rollback");
            Logger.Info("query [domain] [prefix]");
            Logger.Info("install [msi]");
            Logger.Info("exit [-r]");
            Logger.Info("==========================");
            Logger.Info($"Logs are stored here: {Configuration.LogFile}");
            Logger.Info("");
            return true;
        }
    }
}
