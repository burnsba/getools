using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.MessageType;
using Gebug64.Unfloader.UsbPacket;
using Getools.Lib;

namespace Gebug64.Unfloader.Message
{
    public class RomAckMessage : RomMessage
    {
        // size in bytes
        public const int AckProtocolHeaderSize = 8;

        public const int AckProtocolHeaderPacketNumberOffset = 1;
        public const int AckProtocolHeaderTotalPacketsOffset = 3;
        public const int AckProtocolHeaderReplyCategoryOffset = 6;
        public const int AckProtocolHeaderReplyCommandOffset = 7;


        public RomMessage Reply { get; set; }

        public GebugMessageCategory AckCategory { get; set; }
        
        public int PacketNumber { get; set; }

        public int TotalNumberPackets { get; set; }

        public byte[] FragmentBytes { get; set; }

        public RomAckMessage()
            : base (GebugMessageCategory.Ack, 0)
        {
        }

        public static RomMessageParseResult Parse(byte[] data, out RomAckMessage? result)
        {
            result = null;

            var packetNumber = BitUtility.Read16Big(data, AckProtocolHeaderPacketNumberOffset);
            var totalPackets = BitUtility.Read16Big(data, AckProtocolHeaderTotalPacketsOffset);

            var ackCategory = (GebugMessageCategory)data[AckProtocolHeaderReplyCategoryOffset];

            if (data.Length < AckProtocolHeaderSize)
            {
                return RomMessageParseResult.Error;
            }

            result = new RomAckMessage()
            {
                Source = CommunicationSource.N64,
                AckCategory = ackCategory,
                PacketNumber = packetNumber,
                TotalNumberPackets = totalPackets,
            };

            if (totalPackets == 1)
            {
                result.Unwrap(data);
            }
            else
            {
                result.FragmentBytes = data;
            }

            return RomMessageParseResult.Success;
        }

        public void Unwrap(byte[] data)
        {
            AckCategory = (GebugMessageCategory)data[AckProtocolHeaderReplyCategoryOffset];
            int command = (int)data[AckProtocolHeaderReplyCommandOffset];

            switch (AckCategory)
            {
                case GebugMessageCategory.Meta:
                    {
                        Reply = new RomMetaMessage((GebugCmdMeta)command);
                        RomMetaMessage.ParseParameters((RomMetaMessage)Reply, (GebugCmdMeta)command, data, AckProtocolHeaderSize);
                    }
                    break;

                case GebugMessageCategory.Misc:
                    {
                        Reply = new RomMiscMessage((GebugCmdMisc)command);
                        RomMiscMessage.ParseParameters((RomMiscMessage)Reply, (GebugCmdMisc)command, data, AckProtocolHeaderSize);
                    }
                    break;

                case GebugMessageCategory.Vi:
                    {
                        Reply = new RomViMessage((GebugCmdVi)command);
                        RomViMessage.ParseParameters((RomViMessage)Reply, (GebugCmdVi)command, data, AckProtocolHeaderSize);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (!object.ReferenceEquals(null, Reply))
            {
                var replyString = Reply.ToString();

                sb.Append($"{Category} {replyString}");
            }
            else
            {
                sb.Append($"{Category} {AckCategory}");
            }

            return sb.ToString();
        }
    }
}
