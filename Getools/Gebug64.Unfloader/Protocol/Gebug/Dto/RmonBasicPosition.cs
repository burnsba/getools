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
    /// Container to describe simple position data.
    /// </summary>
    public class RmonBasicPosition
    {
        /// <summary>
        /// Stan with room id.
        /// </summary>
        /// <remarks>
        /// 24 bit stan id, with 8 bit room id
        /// on console: (stanid & 0x00FFFFFF) | ((u32)roomid << 24)
        /// </remarks>
        public UInt32 PackedStanId { get; set; }

        /// <summary>
        /// Room.
        /// </summary>
        public byte RoomId => (byte)((PackedStanId & 0xFF000000) >> 24);

        /// <summary>
        /// Stan <see cref="Getools.Lib.Game.Asset.Stan.StandTile.InternalName"/>.
        /// </summary>
        public UInt32 StanId => PackedStanId & 0x00FFFFFF;

        /// <summary>
        /// 3d coord.
        /// </summary>
        public Coord3dd Position { get; set; } = Coord3dd.Zero.Clone();

        /// <inheritdoc />
        public override string ToString()
        {
            var stanText = StanId.ToString("X6");
            var roomText = RoomId.ToString("X2");
            return $"{nameof(StanId)}:0x{stanText}, {nameof(RoomId)}:0x{roomText}, {Position.X:0.0000}, {Position.Y:0.0000}, {Position.Z:0.0000}";
        }
    }
}
