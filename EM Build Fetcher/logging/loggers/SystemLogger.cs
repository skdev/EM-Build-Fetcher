using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.logging.loggers
{
    public class SystemLogger : Logger
    {
        private Logger simple;
        private Logger file;

        public SystemLogger(string logFile, string name, LogLevel level) : base(name, level) {
            simple = LoggerFactory.GetAnonymousLogger();
            file = LoggerFactory.GetFileLogger(logFile, name);
        }

        public override void Log(LogLevel level, string message)
        {
            simple.Log(level, message);
            file.Log(level, message);
        }
    }
}
