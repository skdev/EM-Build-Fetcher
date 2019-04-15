
using EM_Build_Fetcher.logging.loggers;

namespace EM_Build_Fetcher.logging
{
    public static class LoggerFactory
    {
        private static LogLevel _level;

        public static void SetLevel(LogLevel level) => _level = level;

        public static LogLevel GetLevel() => _level;

        public static Logger GetSimpleLogger(string name) => new SimpleLogger(name, _level);

        public static Logger GetAnonymousLogger() => new AnonymousLogger(_level);

        public static Logger GetTrayLogger(string name) => new TrayLogger(name, _level);

        public static Logger GetFileLogger(string logFile, string name) => new FileLogger(logFile, name, _level);

        public static Logger GetSystemLogger(string logFile, string name) => new SystemLogger(logFile, name, _level);
    }
}
