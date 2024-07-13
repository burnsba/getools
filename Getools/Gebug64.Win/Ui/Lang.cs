using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Windows.Mdi;

namespace Gebug64.Win.Ui
{
    /// <summary>
    /// Constant text strings to display in the app.
    /// </summary>
    public static class Lang
    {
        /// <summary>
        /// Main application title.
        /// </summary>
        public const string AppTitle = "Gebug64";

        /// <summary>
        /// MDI Child "main window" title.
        /// </summary>
        public const string Window_MessageCenterTitle = "Message Center";

        /// <summary>
        /// MDI Child "map" title.
        /// </summary>
        public const string Window_MapTitle = "Map";

        /// <summary>
        /// MDI Child "log" window title.
        /// </summary>
        public const string Window_LogTitle = "Log";

        /// <summary>
        /// MDI Child "query task" window title.
        /// </summary>
        public const string Window_QueryTaskTitle = "Query Tasks";

        /// <summary>
        /// Gets the default window title per the given type.
        /// </summary>
        /// <param name="windowType">Type of window or control.</param>
        /// <returns>Default window title.</returns>
        /// <exception cref="NotSupportedException">Throws if the given type does not have a default title.</exception>
        public static string GetDefaultWindowTitle(Type windowType)
        {
            if (windowType == typeof(MainControl))
            {
                return Window_MessageCenterTitle;
            }
            else if (windowType == typeof(MapControl))
            {
                return Window_MapTitle;
            }
            else if (windowType == typeof(LogControl))
            {
                return Window_LogTitle;
            }
            else if (windowType == typeof(QueryTasksControl))
            {
                return Window_QueryTaskTitle;
            }

            throw new NotSupportedException();
        }
    }
}
