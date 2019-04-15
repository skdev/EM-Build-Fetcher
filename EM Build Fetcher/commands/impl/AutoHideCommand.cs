namespace EM_Build_Fetcher.commands.impl
{
    public class AutoHideCommand : Command
    {
        public override bool Execute(string[] args)
        {
            AppConfig.SetAutoHide(true);
            AppConfig.SaveConfig();
            return true;
        }
    }
}
