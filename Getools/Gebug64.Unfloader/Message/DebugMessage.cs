using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class DebugMessage : GebugMessageBase
    {
        public DebugMessage()
        {
        }

        public DebugMessage(GebugMessageBase copy)
            : base(copy)
        {
        }
        
        public DebugMessage(IPacket usbPacket)
            : base (usbPacket)
        {
        }
    }
}
