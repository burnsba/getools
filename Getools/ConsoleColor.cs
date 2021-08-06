using System;
using System.Collections.Generic;
using System.Text;

namespace Getools
{
    /// <summary>
    /// Console helper methods, to write text with foreground or background color set.
    /// </summary>
    public static class ConsoleColor
    {
        /// <summary>
        /// Writes message to console, with foreground color set to red.
        /// </summary>
        /// <param name="message">Message to write.</param>
        public static void ConsoleWriteLineRed(string message)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = System.ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = oldColor;
        }
    }
}
