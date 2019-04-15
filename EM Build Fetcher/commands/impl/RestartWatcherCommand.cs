using EM_Build_Fetcher.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class RestartWatcherCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(EmBuildDropsWatchHandler).FullName);

        public override bool Execute(string[] args)
        {
            if (args.Length < 2)
            {
                Logger.Warning("Usage: [-directory] [-target]");
                return false;
            }

            return false;

            //eturn Program.RestartEmWatchService(args[0], args[1]);
        }
    }
}
