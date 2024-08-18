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
    /// Sends notification from console to pc for all explosions created this tick.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Objects, Command = (byte)GebugCmdObjects.NotifyExplosionCreate)]
    public class GebugObjectsNotifyExplosionCreate : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugObjectsNotifyExplosionCreate"/> class.
        /// </summary>
        public GebugObjectsNotifyExplosionCreate()
          : base(GebugMessageCategory.Objects)
        {
            Command = (int)GebugCmdObjects.NotifyExplosionCreate;
        }

        /// <summary>
        /// Number of guard positions in <see cref="Data"/>.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 2, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public UInt16 Count { get; set; }

        /// <summary>
        /// Raw result from gebug message. Contains guard positions.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, IsVariableSize = true, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public byte[]? Data { get; set; }

        /// <summary>
        /// Processes the <see cref="Data"/> property intro strongly typed position data.
        /// </summary>
        /// <returns>Position info.</returns>
        public List<RmonExplosionCreatePosition> ParsePositionData()
        {
            if (object.ReferenceEquals(null, Data))
            {
                return new List<RmonExplosionCreatePosition>();
            }

            var results = new List<RmonExplosionCreatePosition>();

            int bodyOffset = 0;
            byte[] fullBody = Data!;

            for (int i = 0; i < Count; i++)
            {
                UInt16 explosionType = (UInt16)BitUtility.Read16Big(fullBody, bodyOffset);
                bodyOffset += 2;

                // Skip unused 16 bits.
                bodyOffset += 2;

                UInt32 packedStanId = (UInt32)BitUtility.Read32Big(fullBody, bodyOffset);
                bodyOffset += 4;

                double x = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                double y = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                double z = (double)BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                bodyOffset += 4;

                var position = new Getools.Lib.Game.Coord3dd(x, y, z);

                var item = new RmonExplosionCreatePosition()
                {
                    ExplosionType = explosionType,
                    PackedStanId = packedStanId,
                    Position = position,
                };

                results.Add(item);
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
                return $"{Category} {DebugCommand} - {Count} explosion(s)";
            }
        }
    }
}
