using Microsoft.VisualStudio.TestTools.UnitTesting;
using EM_Build_Fetcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_Build_Fetcher.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        [TestMethod()]
        public void StartEmWatchServiceTest()
        {
            var toWatch = "";
            var toSave = "";
           // Assert.IsTrue(Program.StartEmWatchService(toWatch, toSave));
        }

        [TestMethod()]
        public void RestartEmWatchServiceTest()
        {
            var toWatch = "";
            var toSave = "";
            //var started = Program.StartEmWatchService(toWatch, toSave);
          //  Assert.IsTrue(started);

            //var restarted = Program.RestartEmWatchService(toWatch, toSave);
            //Assert.IsTrue(restarted);
        }
    }
}