
using System.Collections.Generic;
using System.IO;

namespace EM_Build_Fetcher.utils
{
    public class DirectoryUtils
    {
        /// <exception cref="System.Exception"></exception>
        public static bool CreateDirectory(string destination)
        {
            if (DirectoryExists(destination))
            {
                return false;
            }
            Directory.CreateDirectory(destination);
            return DirectoryExists(destination);
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool CopyDirectory(string source, string destination, bool overrideFiles = false)
        {
            if (!CreateDirectoryIfNotExist(destination))
            {
                throw new DirectoryNotFoundException();
            }

            GetFilesInDirectory(source).ForEach(file => FileUtils.CopyFile(file, destination, overrideFiles));
            GetDirectories(source).ForEach(directory => CopyDirectory(directory.FullName, Path.Combine(destination, directory.Name)));

            return DirectoryExists(destination);
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static List<FileInfo> GetFilesInDirectory(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                throw new DirectoryNotFoundException();
            }
            return new List<FileInfo>(new DirectoryInfo(directory).GetFiles());
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static List<DirectoryInfo> GetDirectories(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                throw new DirectoryNotFoundException();
            }
            return new List<DirectoryInfo>(new DirectoryInfo(directory).GetDirectories());
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool CleanDirectory(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                throw new DirectoryNotFoundException();
            }
            return DeleteDirectory(directory) && CreateDirectory(directory);
        }

        /// <exception cref="System.Exception"></exception>
        public static bool CreateDirectoryIfNotExist(string directory)
        {
            return DirectoryExists(directory) || CreateDirectory(directory);
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool IsDirectoryEmpty(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                throw new DirectoryNotFoundException();
            }
            return GetFilesInDirectory(directory).Count <= 0 && GetDirectories(directory).Count <= 0;
        }

        /// <exception cref="System.Exception"></exception>
        /// <exception cref="System.Exception"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool DeleteDirectory(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                return true;
            }

            if (IsDirectoryEmpty(directory))
            {
                Directory.Delete(directory);
                return DirectoryDoesNotExist(directory);
            }

            if (!DeleteFilesInDirectory(directory))
            {
                return false;
            }

            GetDirectories(directory).ForEach(dir => DeleteDirectory(dir.FullName));

            return DirectoryDoesNotExist(directory);
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.Exception"></exception>
        public static bool DeleteFilesInDirectory(string directory)
        {
            if (DirectoryDoesNotExist(directory))
            {
                throw new DirectoryNotFoundException();
            }

            GetFilesInDirectory(directory).ForEach(file => FileUtils.DeleteFile(file.FullName));

            return IsDirectoryEmpty(directory);
        }

        public static bool DirectoryExists(string directory) => Directory.Exists(directory); //!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory) && File.GetAttributes(directory).HasFlag(FileAttributes.Directory);
        public static bool DirectoryDoesNotExist(string directory) => !DirectoryExists(directory);
       
    }
}
