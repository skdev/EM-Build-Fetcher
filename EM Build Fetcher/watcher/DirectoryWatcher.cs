using EM_Build_Fetcher.exceptions;
using System;
using System.IO;

namespace EM_Build_Fetcher
{
    public class DirectoryWatcher
    {
        private FileSystemWatcher       _watcher;
        private IDirectoryWatchHandler   _handler;
        private readonly string         _directory;
        public bool Running { get; private set; }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        private DirectoryWatcher(string directory, IDirectoryWatchHandler handler)
        {
            if (!Directory.Exists(directory) || string.IsNullOrWhiteSpace(directory))
            {
                throw new DirectoryNotFoundException();
            }
            _handler = handler ?? throw new NullReferenceException();
            _directory = directory;
        }

        /// <exception cref="System.IO.DirectoryNotFoundException"></exception>
        /// <exception cref="System.NullReferenceException"></exception>
        /// <exception cref="System.IO.IOException"></exception>
        /// <exception cref="System.Exception"></exception>
        public bool Start()
        {
            if (!Directory.Exists(_directory))
            {
                throw new DirectoryNotFoundException();
            }
            if (_handler is null)
            {
                throw new NullReferenceException();
            }
            if (Running)
            {
                throw new WatcherAlreadyRunningException();
            }

            if (_watcher is null)
            {
                _watcher = new FileSystemWatcher
                {
                    Path = _directory,
                    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    Filter = "*.*",     
                };

                //Can't bang these in the above block because I'm not going to enable experimental c# features
                _watcher.Created += _handler.Created;
                _watcher.Changed += _handler.Changed;
                _watcher.Renamed += _handler.Renamed;
                _watcher.Deleted += _handler.Deleted;
                _watcher.Error   += _handler.Error;
            }

            try
            {
                //TODO: Check if execution if execution stops if an exception is raised. If so, we don't need the try block.
                _watcher.EnableRaisingEvents = true;
            }
            catch (Exception)
            {
                Running = false;
                throw;
            }

            Running = true;
            return true;
        }

        /// <exception cref="System.Exception"></exception>
        public bool Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();

            if (_watcher.EnableRaisingEvents)
            {
                return false;
            }

            Running = false;
            return true;
        }

        /// <exception cref="System.ArgumentNullException"></exception>
        public DirectoryWatcher SetHandler(IDirectoryWatchHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException();   
            return this;
        }

        public static DirectoryWatcher CreateDirectoryWatcher(string directory, IDirectoryWatchHandler handler)
        {
            return new DirectoryWatcher(directory, handler);
        }

        public static DirectoryWatcher CreateDirectoryWatcherStarted(string directory, IDirectoryWatchHandler handler)
        {
            var watcher = new DirectoryWatcher(directory, handler);
            watcher.Start();
            return watcher;
        }
    }
}
