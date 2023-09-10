using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Converters
{
    /// <summary>
    /// Exception conversion methods.
    /// </summary>
    public static class ExceptionConverter
    {
        /// <summary>
        /// Extracts information from exception to print as text.
        /// </summary>
        /// <param name="ex">Exception to convert.</param>
        /// <returns>Contents of exception and all inner exceptions.</returns>
        public static string DefaultToString(Exception ex)
        {
            var sb = new StringBuilder();

            var currentEx = ex;
            var currentDepth = "Exception";

            do
            {
                sb.AppendLine("Current depth:");
                sb.AppendLine(currentDepth);
                sb.AppendLine();
                sb.AppendLine("Message:");
                sb.AppendLine(currentEx.Message);
                sb.AppendLine();
                sb.AppendLine("Type:");
                sb.AppendLine(currentEx.GetType().FullName);
                sb.AppendLine();
                sb.AppendLine("Stack trace:");
                sb.AppendLine(currentEx.StackTrace);
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine("==================================================");
                sb.AppendLine();

                currentEx = currentEx.InnerException;
                currentDepth += ".InnerException";
            }
            while (currentEx != null);

            return sb.ToString();
        }
    }
}
