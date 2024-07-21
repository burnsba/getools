using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Dto;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;
using Getools.Lib;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json.Linq;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Sends all "active" character positions from console to pc (active means the model is not null).
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Chr, Command = (byte)GebugCmdChr.SendAllGuardInfo)]
    public class GebugChrSendAllGuardInfoMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugChrSendAllGuardInfoMessage"/> class.
        /// </summary>
        public GebugChrSendAllGuardInfoMessage()
          : base(GebugMessageCategory.Chr)
        {
            Command = (int)GebugCmdChr.SendAllGuardInfo;
        }

        /// <summary>
        /// Number of guard positions in <see cref="Data"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public UInt16 Count { get; set; }

        /// <summary>
        /// Raw result from gebug message. Contains guard positions.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[]? Data { get; set; }

        public List<RmonGuardPosition> ParseGuardPositions()
        {
            if (object.ReferenceEquals(null, Data))
            {
                return new List<RmonGuardPosition>();
            }

            var results = new List<RmonGuardPosition>();

            int bodyOffset = 0;
            byte[] fullBody = Data!;

            for (int i = 0; i < Count; i++)
            {
                ushort chrnum = (ushort)BitUtility.Read16Big(fullBody, bodyOffset);
                bodyOffset += 2;

                byte chrSlotIndex = fullBody[bodyOffset++];

                GuardActType action = (GuardActType)fullBody[bodyOffset++];
                if (!Enum.IsDefined(typeof(GuardActType), action))
                {
                    action = GuardActType.ActInvalidData;
                }

                double x = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                double y = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                double z = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                var propPos = new Getools.Lib.Game.Coord3dd(x, y, z);

                x = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                y = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                z = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                var targetPos = new Getools.Lib.Game.Coord3dd(x, y, z);

                Single rot = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                Single damage = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                Single maxdamage = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                Single intolerance = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                var guard = new RmonGuardPosition()
                {
                    Chrnum = chrnum,
                    ChrSlotIndex = chrSlotIndex,
                    ActionType = action,
                    PropPos = propPos,
                    TargetPos = targetPos,
                    Subroty = rot,
                    Damage = damage,
                    MaxDamage = maxdamage,
                    Intolerance = intolerance,
                };

                results.Add(guard);
            }

            return results;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand} - {Count} chr(s)";
            }
        }
    }
}
