
using EM_Build_Fetcher.utils;

namespace EM_Build_Fetcher.commands.impl
{
    public class HideConsoleCommand : Command
    {
        public override bool Execute(string[] args)
        {
            SystemUtils.HideConsole();
            return true;
        }
    }
}
