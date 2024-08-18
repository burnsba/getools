using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace Gebug64.Unfloader.Protocol.Gebug.Dto
{
    /// <summary>
    /// Container to describe position when explosion is created.
    /// </summary>
    public class RmonExplosionCreatePosition : RmonBasicPosition
    {
        /// <summary>
        /// Type of explosion.
        /// </summary>
        public UInt16 ExplosionType { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var stanText = StanId.ToString("X6");
            var roomText = RoomId.ToString("X2");
            return $"{nameof(ExplosionType)}:{ExplosionType}, {nameof(StanId)}:0x{stanText}, {nameof(RoomId)}:0x{roomText}, {Position.X:0.0000}, {Position.Y:0.0000}, {Position.Z:0.0000}";
        }
    }
}
