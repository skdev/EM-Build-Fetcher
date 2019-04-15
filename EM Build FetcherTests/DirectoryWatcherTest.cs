using Microsoft.VisualStudio.TestTools.UnitTesting;
using EM_Build_Fetcher;

namespace EM_Build_FetcherTests.watcher
{
    /// <summary>
    /// Summary description for DirectoryWatcherTest
    /// </summary>
    [TestClass]
    public class DirectoryWatcherTest
    {
        private readonly string _directory = "";
        private readonly string _target = "";

        [TestMethod()]
        public void StartWatcherTest()
        {
            var watcher1 = DirectoryWatcher.CreateDirectoryWatcher(_directory, new EmBuildDropsWatchHandler(_directory, _target));
            watcher1.Start();

            var watcher2 = DirectoryWatcher.CreateDirectoryWatcherStarted(_directory, new EmBuildDropsWatchHandler(_directory, _target));

            Assert.IsTrue(watcher1.Running);
            Assert.IsTrue(watcher2.Running);
        }

        [TestMethod()]
        public void StopWatcherTest()
        {
            var watcher1 = DirectoryWatcher.CreateDirectoryWatcher(_directory, new EmBuildDropsWatchHandler(_directory, _target));
            watcher1.Start();
            watcher1.Stop();

            var watcher2 = DirectoryWatcher.CreateDirectoryWatcherStarted(_directory, new EmBuildDropsWatchHandler(_directory, _target));
            watcher2.Stop();

            Assert.IsFalse(watcher1.Running);
            Assert.IsFalse(watcher2.Running);
        }

        [TestMethod()]
        public void CreateTest()
        {
            var watcher = DirectoryWatcher.CreateDirectoryWatcher(_directory, new EmBuildDropsWatchHandler(_directory, _target));
            Assert.IsNotNull(watcher);
        }

        [TestMethod()]
        public void CreateStartedTest()
        {
            var watcher = DirectoryWatcher.CreateDirectoryWatcherStarted(_directory, new EmBuildDropsWatchHandler(_directory, _target));
            Assert.IsNotNull(watcher);
            Assert.IsTrue(watcher.Running);
        }
    }
}
