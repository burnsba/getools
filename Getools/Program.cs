using System;
using Getools.Lib.Converters;

namespace Getools
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = "../../../test.c";

            var stan = StanConverters.ParseFromC(path);

            StanConverters.WriteToC(stan, "test_write.c");
            StanConverters.WriteToBin(stan, "test_write.bin");
        }
    }
}
