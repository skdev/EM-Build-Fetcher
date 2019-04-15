
using EM_Build_Fetcher.logging;
using System;
using System.IO;

namespace EM_Build_Fetcher.utils
{
    public static class FileUtils
    {
        private static readonly Logger Logger = LoggerFactory.GetSystemLogger(Configuration.LogFile, typeof(FileUtils).Name);

        /// <exception cref="System.Exception"></exception>
        public static bool CreateFile(string destination)
        {
            if (FileExists(destination))
            {
                return false;
            }
            File.Create(destination).Close();
            return FileExists(destination);
        }

        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool CopyFile(FileInfo file, string destination, bool overrideFile = false)
        {
            if (file is null)
            {
                throw new ArgumentNullException();
            }

            if (!file.Exists)
            {
                throw new FileNotFoundException();
            }

            if (DirectoryUtils.DirectoryDoesNotExist(destination))
            {
                throw new DirectoryNotFoundException();
            }

            file.CopyTo(Path.Combine(destination, file.Name), overrideFile);

            return FileExists(destination);
        }

        public static bool WriteToFile(string file, string[] args, bool append = true, bool verify = true)
        {
            if(!IsFile(file))
            {
                return false;
            }

            using (var writer = new StreamWriter(file, append))
            {
                foreach(string s in args) {
                    writer.WriteLine(s);
                }
                writer.Close();
            }

            if(!verify)
            {
                return true;
            }

            return VerifyFileContents(file, args);
        }

        private static bool VerifyFileContents(string file, string[] contents)
        {
            if (!IsFile(file))
            {
                return false;
            }

            using (var reader = new StreamReader(file))
            {
                foreach(string s in contents)
                {
                    if(!s.Contains(reader.ReadLine()))
                    {
                        reader.Close();
                        Logger.Warning($"File {file} contents does not match the data specified.");
                        return false;
                    }
                }

                reader.Close();
            }

            return true;
        }

        public static bool DeleteFile(string file)
        {
            File.Delete(file);
            return FileDoesNotExist(file);
        }

        /// <exception cref="System.Exception"></exception>
        public static bool IsFile(string file) => FileExists(file) && DirectoryUtils.DirectoryDoesNotExist(file);

        public static bool FileExists(string file) => File.Exists(file);//!string.IsNullOrWhiteSpace(file) && (Directory.Exists(file) && !(File.GetAttributes(file).HasFlag(FileAttributes.Directory)));

        public static bool FileDoesNotExist(string file) => !FileExists(file);
    }
}
