using EM_Build_Fetcher.logging;
using EM_Build_Fetcher.utils;

using EM_Build_Fetcher.config;

namespace EM_Build_Fetcher
{
    public class AppConfig
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(AppConfig).FullName);
        private static AppConfig Instance = new AppConfig();

        private static JsonFile jsonfile = new JsonFile();
        private static ConfigJson configjson = new ConfigJson();

        private AppConfig() { }

        public static void SetNeedInstall(bool needToInstall) => configjson.NeedInstall = needToInstall;

        public static void SetMonitoredDirectory(string dir) => configjson.Directory = dir;

        public static void SetCurrent(string dir) => configjson.PreviousDrop = dir;

        public static void SetProductCode(Product product) => configjson.Product = product;

        public static void SetAutoHide(bool hide) => configjson.AutoHide = hide;

        public static void SetAutoLoginValues(string username, string password)
        {
            configjson.AutoLogonUserName = username;
            configjson.AutoLogonPassword = password;
        }

        public static bool NeedInstall() => configjson.NeedInstall;

        public static string GetMonitoredDirectory() => configjson.Directory;

        public static Product GetProductCode() => configjson.Product;

        public static bool AutoHideEnabled() => configjson.AutoHide;

        public static string CurrentDropLocation() => configjson.PreviousDrop;

        public static string AutoLoginUsername() => configjson.AutoLogonUserName;

        public static string AutoLoginPassword() => configjson.AutoLogonPassword;
        public static bool Autologin()
        {
            return !string.IsNullOrWhiteSpace(configjson.AutoLogonUserName);
        }

        public static AppConfig Reset()
        {
            Instance = new AppConfig();
            return Instance;
        }

        public static bool SaveConfig()
        {
            return jsonfile.WriteJsonFile(Resource.RootFolder + @"\" + Resource.ConfigurationFile, configjson);
        }

        public static void ReadConfigFile()
        {
            if (FileUtils.FileDoesNotExist(Configuration.StatusFile))
            {
                return;
            }

            configjson = jsonfile.ReadJsonFile(Resource.RootFolder + @"\" + Resource.ConfigurationFile);
        }
    }
}
