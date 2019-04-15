using System;

namespace EM_Build_Fetcher.logging
{
    public class AnonymousLogger : Logger
    {
        public AnonymousLogger(LogLevel level) : base("", level) { }

        public override void Log(LogLevel level, string message)
        {
            Console.WriteLine($"{DateTime.Now} {level}: {message}");
        }
    }
}
