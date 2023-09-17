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
        public const int ProtocolHeaderCategoryOffset = 0;

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

        public override byte[] ToSendData()
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
            RomMessageParseResult parseResult = RomMessageParseResult.Error;

            if (data == null || data.Length < 2)
            {
                return RomMessageParseResult.Error;
            }

            switch ((GebugMessageCategory)data[0])
            {
                case GebugMessageCategory.Ack:
                    {
                        RomAckMessage ackMessage;
                        parseResult = RomAckMessage.Parse(data, out ackMessage);
                        if (parseResult == RomMessageParseResult.Success)
                        {
                            result = ackMessage;
                        }
                        return parseResult;
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
                case GebugMessageCategory.Vi: return ((GebugCmdVi)command).ToString();
            }

            throw new NotImplementedException();
        }
    }
}
