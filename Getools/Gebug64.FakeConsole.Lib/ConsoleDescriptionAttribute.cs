using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.FakeConsole.Lib
{
    public class ConsoleDescriptionAttribute : Attribute
    {
        public ConsoleDescriptionAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }

        public ConsoleDescriptionAttribute(string friendlyName, int displayOrder)
        {
            FriendlyName = friendlyName;
            DisplayOrder = displayOrder;
        }

        public string FriendlyName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 0;
    }
}
