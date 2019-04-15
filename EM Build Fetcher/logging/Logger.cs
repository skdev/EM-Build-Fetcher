using EM_Build_Fetcher.logging;
using System;

namespace EM_Build_Fetcher
{
    public abstract class Logger
    {
        protected string name;
        private LogLevel _level;
        private bool _disabled;

        public Logger(string name, LogLevel level = LogLevel.Info, bool disabled = false)
        {
            this.name = name;
            _level = level;
            _disabled = disabled;
        }

        public abstract void Log(LogLevel level, string message);

        private void DoLog(LogLevel level, string message)
        {
            Log(level, message);
        }

        public void Trace(string message) => DoLog(LogLevel.Trace, message);

        public void Info(string message) => DoLog(LogLevel.Info, message);

        public void Warning(string message) => DoLog(LogLevel.Warning, message);

        public void Error(string message) => DoLog(LogLevel.Error, message);

        public void Debug(string message) => DoLog(LogLevel.Debug, message);

        public void Disable() => _disabled = true;

        public void Enable() => _disabled = false;
    }
}
