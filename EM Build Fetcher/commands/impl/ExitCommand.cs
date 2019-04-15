
namespace EM_Build_Fetcher.commands.impl
{
    public class ExitCommand : Command
    {
        public override bool Execute(string[] args)
        {
            System.Environment.Exit(0);
            return true;
        }
    }
}
