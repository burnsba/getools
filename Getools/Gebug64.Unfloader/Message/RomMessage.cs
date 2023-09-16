using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Message.CommandParameter;
using Gebug64.Unfloader.Message.MessageType;

namespace Gebug64.Unfloader.Message
{
    public abstract class RomMessage : GebugMessageBase
    {
        public GebugMessageCategory Category { get; set; }

        public int RawCommand { get; set; }

        public List<ICommandParameter> Parameters { get; set; } = new List<ICommandParameter>();

        public RomMessage()
        {
        }

        public RomMessage(GebugMessageCategory category, int rawCommand)
        {
            Category = category;
            RawCommand = rawCommand;
        }

        public virtual byte[] ToSendData()
        {
            var results = new List<byte[]>();

            var header = new byte[] { (byte)Category, (byte)RawCommand };
            results.Add(header);

            foreach (var p in Parameters)
            {
                results.Add(p.GetBytes(Endianness.BigEndian));
            }

            return results.SelectMany(x => x).ToArray();
        }

        public static RomMessageParseResult Parse(byte[] data, out RomMessage? result)
        {
            result = null;

            if (data == null || data.Length < 2)
            {
                return RomMessageParseResult.Error;
            }

            switch ((GebugMessageCategory)data[0])
            {
                case GebugMessageCategory.Ack:
                    {
                        var ackCategory = (GebugMessageCategory)data[1];

                        if (data.Length < 3)
                        {
                            return RomMessageParseResult.Error;
                        }

                        result = new RomAckMessage()
                        {
                            Source = CommunicationSource.N64,
                            AckCategory = ackCategory,
                        };

                        ((RomAckMessage)result).Unwrap(data.Skip(1).ToArray());
                    }
                    break;

                default:
                    return RomMessageParseResult.Error;
            }

            return RomMessageParseResult.Success;
        }

        public override string ToString()
        {
            var commandName = ResolveCommand(Category, RawCommand);

            return $"{Category} {commandName}";
        }

        public static string ResolveCommand(GebugMessageCategory category, int command)
        {
            switch (category)
            {
                case GebugMessageCategory.Meta: return ((GebugCmdMeta)command).ToString();
                case GebugMessageCategory.Misc: return ((GebugCmdMisc)command).ToString();
                case GebugMessageCategory.Stage: return ((GebugCmdStage)command).ToString();
            }

            throw new NotImplementedException();
        }
    }
}
