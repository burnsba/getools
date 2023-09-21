using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Test.Framework
{
    public class TestResults
    {
        public int TotalTestClasses { get; set; }
        public int TotalTestMethods { get; set; }

        public int PassCount { get; set; }
        public int FailCount { get; set; }

        public List<TestCaseInfo> TestFailures { get; set; } = new List<TestCaseInfo>();
    }
}
