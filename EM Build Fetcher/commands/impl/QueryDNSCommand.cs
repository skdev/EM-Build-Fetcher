using EM_Build_Fetcher.logging;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class QueryDNSCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(QueryDNSCommand).FullName);

        public override bool Execute(string[] args)
        {

            if(args.Count() < 2)
            {
                Logger.Info("Invalid number of arguments specfied, syntax: [domain] [prefix] e.g. query testing.local sk-win");
                return false;
            }

            var domain = args[1];
            var prefix = args[2];

            PrintCN(domain, prefix);

            return true;
        }

        private static void PrintCN(string domain, string prefix)
        {
            Logger.Info($"Searching {domain} for CN that includes {prefix} - this may take a while...");

            var entry = new DirectoryEntry($"LDAP://{domain}");
            var mySearcher = new DirectorySearcher(entry)
            {
                Filter = ("(objectClass=computer)"),
                SizeLimit = 10000,
                PageSize = int.MaxValue
            };

            var counter = 0;

            foreach (SearchResult resEnt in mySearcher.FindAll())
            {
                var CN = resEnt.GetDirectoryEntry().Name;

                if (CN.StartsWith("CN=") && CN.ToLower().Contains($"{prefix}"))
                {
                    CN = CN.Remove(0, "CN=".Length);
                    Logger.Info(CN);
                    counter++;
                }
            }

            Logger.Info("Finished querying DNS");
            Logger.Info($"Total number of machines found: {counter}");
            mySearcher.Dispose();
            entry.Dispose();
        }
    }
}
