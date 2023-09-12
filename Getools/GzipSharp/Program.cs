using System;
using System.IO;
using System.Reflection.Metadata;
using GzipSharpLib;
using Microsoft.Extensions.Logging;

namespace GzipSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var logger = new GzipSharpLib.Logging.Logger();

            var gzipContext = new Context(logger);

            gzipContext.Trace = true;

            var sourcePath = "C:\\Users\\benja\\code\\getools\\level\\asset\\music\\Mfrigate_outro.bin.1172";
            var destPath = "C:\\Users\\benja\\code\\getools\\level\\asset\\music\\Mfrigate_outro-cs.bin";

            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException(sourcePath);
            }

            ReturnCode result;

            using (var ms = new MemoryStream(System.IO.File.ReadAllBytes(sourcePath)))
            {
                gzipContext.Source = ms;
                result = gzipContext.Execute();
            }

            if (result == ReturnCode.Ok)
            {
                if (object.ReferenceEquals(null, gzipContext.Destination))
                {
                    throw new NullReferenceException("Destination not set.");
                }

                var contentResult = gzipContext.Destination.ToArray();

                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }

                System.IO.File.WriteAllBytes(destPath, contentResult);
            }

            Environment.Exit((int)result);
        }
    }
}