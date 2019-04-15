using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class RemoteDeployCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(RemoteDeployCommand).FullName);

        public override bool Execute(string[] args)
        {
            if(args.Length < 3)
            {
                Logger.Info($"Invalid number of arguments specified, syntax: [machine] [username] [password] e.g. \\tes1.testing.local testing\admin password1");
                return false;
            }

            var machine = args[1];
            var username = args[2];
            var password = args[3];

            return RemoteSystemUtils.DeployAgent(machine, username, password);
        }
    }
}
