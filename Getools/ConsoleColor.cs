using System;
using System.Collections.Generic;
using System.Text;

namespace Getools
{
    public static class ConsoleColor
    {
        public static void ConsoleWriteLineRed(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = System.ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
