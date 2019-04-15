using System;
using System.Runtime.Serialization;

namespace EM_Build_Fetcher.exceptions
{
    public class WatcherAlreadyRunningException : Exception
    {
        public WatcherAlreadyRunningException() { }

        public WatcherAlreadyRunningException(string message): base(message) { }

        public WatcherAlreadyRunningException(string message, Exception inner) : base(message, inner) { }

        public WatcherAlreadyRunningException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
