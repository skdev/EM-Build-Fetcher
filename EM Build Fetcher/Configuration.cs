using EM_Build_Fetcher.config;

namespace EM_Build_Fetcher
{
    public class Configuration
    {
        public static readonly string RootFolder = Resource.RootFolder;
        public static readonly string StatusFile = $"{RootFolder}\\{Resource.ConfigurationFile}";
        public static readonly string LogDir = $"{RootFolder}\\Logs";
        public static readonly string LogFile = $"{LogDir}\\Logs.txt";
        public static readonly string CacheLocation = $"{RootFolder}\\Cache";
        public static readonly string DropLocation = $"{RootFolder}\\Build";
        public static readonly string MsiFolder = Resource.MsiFolder2;
        public static readonly string ExeLocation = Resource.ExeLocation2;
        public static readonly string WebPath = $"{RootFolder}\\www";
        public static readonly string PortalLocation = $"{RootFolder}\\www\\index.html";
    }
}