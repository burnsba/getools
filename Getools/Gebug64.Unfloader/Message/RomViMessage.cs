using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.CommandParameter;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;

namespace Gebug64.Unfloader.Message
{
    public class RomViMessage : RomMessage
    {
        private GebugCmdVi _command;

        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] FrameBuffer { get; set; }

        public GebugCmdVi Command
        {
            get
            {
                return _command;
            }

            set
            {
                _command = value;
                RawCommand = (int)value;
            }
        }

        public RomViMessage()
            : base(GebugMessageCategory.Vi, 0)
        {
        }

        public RomViMessage(GebugCmdVi command)
            : base(GebugMessageCategory.Vi, (int)command)
        {
            Command = command;
        }

        static internal void ParseParameters(RomViMessage self, GebugCmdVi command, byte[] data, int offset)
        {
            switch (command)
            {
                case GebugCmdVi.GrabFramebuffer:
                    {
                        short val;

                        val = BitUtility.Read16Big(data, offset);
                        self.Parameters.Add(new S16Parameter(val));
                        offset += 2;

                        self.Width = val;

                        val = BitUtility.Read16Big(data, offset);
                        self.Parameters.Add(new S16Parameter(val));
                        offset += 2;

                        self.Height = val;

                        var p3 = new U8ArrayParameter(data.Skip(offset).ToArray());
                        self.Parameters.Add(p3);
                        self.FrameBuffer = p3.Value;
                    }
                    break;
            }
        }

        public override string ToString()
        {
            return $"{Category} {Command} {Width} {Height}";
        }
    }
}
