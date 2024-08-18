using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Send Bond position from console to PC.
    /// </summary>
    [ProtocolCommand(Category = GebugMessageCategory.Bond, Command = (byte)GebugCmdBond.SendPosition)]
    public class GebugBondSendPositionMessage : GebugMessage, IActivatorGebugMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GebugBondSendPositionMessage"/> class.
        /// </summary>
        public GebugBondSendPositionMessage()
          : base(GebugMessageCategory.Bond)
        {
            Command = (int)GebugCmdBond.SendPosition;
        }

        /// <summary>
        /// Bond's current stan.
        /// </summary>
        [GebugParameter(ParameterIndex = 0, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public UInt32 PackedStanId { get; set; }

        /// <summary>
        /// Bond's global scaled coordinate x position.
        /// </summary>
        [GebugParameter(ParameterIndex = 1, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public float PosX { get; set; }

        /// <summary>
        /// Bond's global scaled coordinate y (vertical) position.
        /// </summary>
        [GebugParameter(ParameterIndex = 2, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public float PosY { get; set; }

        /// <summary>
        /// Bond's global scaled coordinate z position.
        /// </summary>
        [GebugParameter(ParameterIndex = 3, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public float PosZ { get; set; }

        /// <summary>
        /// g_CurrentPlayer->vv_theta .
        /// </summary>
        [GebugParameter(ParameterIndex = 4, Size = 4, UseDirection = ParameterUseDirection.ConsoleToPc)]
        public float VVTheta { get; set; }

        /// <summary>
        /// Bond's current room.
        /// </summary>
        /// <remarks>
        /// Not a message parameter.
        /// </remarks>
        public byte RoomId => (byte)((PackedStanId & 0xFF000000) >> 24);

        /// <summary>
        /// Bond's current stan.
        /// </summary>
        /// <remarks>
        /// Not a message parameter.
        /// </remarks>
        public UInt32 StanId => PackedStanId & 0x00FFFFFF;

        /// <inheritdoc />
        public override string ToString()
        {
            if (Source == CommunicationSource.Pc)
            {
                return $"{Category} {DebugCommand}";
            }
            else
            {
                return $"{Category} {DebugCommand} {PosX:0.0000}, {PosY:0.0000}, {PosZ:0.0000}";
            }
        }
    }
}
