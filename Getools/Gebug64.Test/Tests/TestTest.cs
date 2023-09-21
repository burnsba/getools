using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Test.Framework;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Flashcart;
using Xunit;

namespace Gebug64.Test.Tests
{
    [Test]
    public class TestTest
    {
        public TestTest()
        {
        }

        [Fact]
        public void TestingTheTestFramework()
        {
            //Assert.True(false);
        }

        [Fact]
        public void TestingTheTestFrameworkFail()
        {
            Assert.True(true);
        }
    }
}
