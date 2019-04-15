using EM_Build_Fetcher.logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.commands.impl
{
    public class CommandPromptCommand : Command
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(CommandPromptCommand).FullName);
        private static readonly string CMD = @"C:\Windows\System32\cmd.exe";

        public override bool Execute(string[] args)
        {
            var argument = "";
            for(var i =1; i < args.Length; i++)
            {
                argument += args[i] + " ";
            }

            Console.WriteLine(ExecuteCmd(argument));
            return true;
        }

        private string ExecuteCmd(string args)
        {
            Console.WriteLine("Executing CMD with args: " + args);

            var process = new Process()
            {
                StartInfo = {
                    FileName = CMD,
                    Arguments = $"{args}",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                }
            };

            if (!process.Start())
            {
                Logger.Warning("Failed to sexecute command in command prompt.");
                return string.Empty;
            }

           
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;

        }
    }
}
