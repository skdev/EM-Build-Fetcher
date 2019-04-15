using System.IO;
using System.Text;
using EM_Build_Fetcher.utils;
using Newtonsoft.Json;

namespace EM_Build_Fetcher.config
{
    /// <summary>
    /// This class contains method to read and write to json file
    /// </summary>
    public class JsonFile
    {
        /// <summary>
        /// This method reads the json file into ConfigJson object.
        /// </summary>
        /// <param name="pathToJsonFile"></param>
        /// <returns></returns>
        public ConfigJson ReadJsonFile(string pathToJsonFile)
        {
            var json = File.ReadAllText(pathToJsonFile, Encoding.Default);

            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }

        public bool WriteJsonFile(string pathToJsonFile, ConfigJson configJson)
        {
            var json = JsonConvert.SerializeObject(configJson);

            return FileUtils.WriteToFile(pathToJsonFile, new string[] { json }, false);

        }
    }
}
