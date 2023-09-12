using GzipSharpLib;
using Newtonsoft.Json.Linq;
using NuGet.Frameworks;
using System.IO;
using System.Linq;

namespace GzipSharpTest
{
    public class UnitTestInflate
    {
        [Fact]
        public void InflateTest1()
        {
            var logger = new GzipSharpLib.Logging.Logger();

            var gzipContext = new Context(logger);

            var source = new byte[]
            {
                0x1f, 0x8b, 0x08, 0x08, 0x54, 0x6e, 0x76, 0x64, 0x00, 0x03, 0x66, 0x69, 0x6c, 0x65, 0x2e,
                0x74, 0x78, 0x74, 0x00, 0x2b, 0x49, 0x2d, 0x2e, 0x51, 0x28, 0x21, 0x81, 0xe0, 0x22, 0x45,
                0xf1, 0xa8, 0x8e, 0x51, 0x1d, 0xa3, 0x3a, 0x46, 0x75, 0x8c, 0xea, 0x18, 0xd5, 0x41, 0xbe,
                0x0e, 0x00, 0x10, 0xc6, 0x76, 0xce, 
                /* destination length: 0x000006a4 = 1700 */
                0xa4, 0x06, 0x00, 0x00
            };

            gzipContext.Source = new MemoryStream(source);

            var result = gzipContext.Execute();

            Assert.Equal(ReturnCode.Ok, result);

            Assert.NotNull(gzipContext.Destination);

            var destinationBytes = gzipContext.Destination!.ToArray();

            var expected = string.Join("\n", Enumerable.Repeat("test test test test test test test test test test", 34)) + "\n";
            var expectedBytes = System.Text.Encoding.ASCII.GetBytes(expected);

            Assert.Equal(expectedBytes.Length, destinationBytes.Length);

            for (int i=0; i<destinationBytes.Length; i++)
            {
                Assert.Equal(expectedBytes[i], destinationBytes[i]);
            }
        }
    }
}