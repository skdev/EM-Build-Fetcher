using EM_Build_Fetcher.utils;
using System;
using System.Diagnostics;
using System.IO;

namespace EM_Build_Fetcher.logging.loggers
{
    public class FileLogger : Logger
    {
        private readonly string _logFile;

        public FileLogger(string logFile, string name, LogLevel level) : base(name, level) {
            _logFile = logFile;
        }

        public override void Log(LogLevel level, string message)
        {
            FileUtils.CreateFile(_logFile);
            FileUtils.WriteToFile(_logFile, new string[] { $"{DateTime.Now} {level} [{name}] {new StackFrame(2).GetMethod().Name}: {message}"  }, true, false);
        }
    }
}
