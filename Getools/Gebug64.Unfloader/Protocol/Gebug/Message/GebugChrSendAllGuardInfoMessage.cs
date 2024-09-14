using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
          : base()
        {
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
        /// Processes the <see cref="Data"/> property intro strongly typed guard info.
        /// </summary>
        /// <returns>Guard info.</returns>
        public List<RmonGuardPosition> ParseGuardPositions()
        {
            if (object.ReferenceEquals(null, Data))
            {
                return new List<RmonGuardPosition>();
            }

            var results = new List<RmonGuardPosition>();

            int bodyOffset = 0;
            byte[] fullBody = Data!;

            int expectedBytes = Count * RmonGuardPosition.SizeOf;

            if (fullBody.Length < expectedBytes)
            {
                throw new EndOfStreamException($"Expected {RmonGuardPosition.SizeOf} x {Count} = {expectedBytes} bytes, but received {fullBody.Length}");
            }

            for (int i = 0; i < Count; i++)
            {
                var guard = RmonGuardPosition.TryParse(fullBody, bodyOffset);
                bodyOffset += RmonGuardPosition.SizeOf;

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
