using System;
using System.Diagnostics;

namespace EM_Build_Fetcher.logging
{
    public class SimpleLogger : Logger
    {
        public SimpleLogger(string name, LogLevel level) : base(name, level) { }

        public override void Log(LogLevel level, string message)
        {
            Console.WriteLine($"{DateTime.Now} {level} [{name}] {new StackFrame(2).GetMethod().Name}: {message}");
        }
    }
}
