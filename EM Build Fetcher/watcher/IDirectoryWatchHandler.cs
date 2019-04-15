using System.IO;

namespace EM_Build_Fetcher
{
    public interface IDirectoryWatchHandler
    {
        void Created(object sender, FileSystemEventArgs args);
        void Changed(object sender, FileSystemEventArgs args);
        void Renamed(object sender, FileSystemEventArgs args);
        void Deleted(object sender, FileSystemEventArgs args);
        void Error(object sender, ErrorEventArgs args);
    }
}
