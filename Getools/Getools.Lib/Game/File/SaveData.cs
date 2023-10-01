using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.File
{
    /// <summary>
    /// Native struct save_data.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveData"/> class.
        /// </summary>
        public SaveData()
        {
        }

        /// <summary>
        /// Gets or sets checksum 1.
        /// </summary>
        public UInt32 Checksum1 { get; set; }

        /// <summary>
        /// Gets or sets checksum 2.
        /// </summary>
        public UInt32 Checksum2 { get; set; }

        /// <summary>
        /// Completion bit flags.
        /// </summary>
        public byte CompletionBitflags { get; set; }

        /// <summary>
        /// 007 flag.
        /// </summary>
        public byte Flag007 { get; set; }

        /// <summary>
        /// Music volume.
        /// </summary>
        public byte MusicVolume { get; set; }

        /// <summary>
        /// Sound effects volume.
        /// </summary>
        public byte SfxVolume { get; set; }

        /// <summary>
        /// Save file options.
        /// </summary>
        public ushort Options { get; set; }

        /// <summary>
        /// Unlocked cheats, part 1.
        /// </summary>
        public byte UnlockedCheats1 { get; set; }

        /// <summary>
        /// Unlocked cheats, part 2.
        /// </summary>
        public byte UnlockedCheats2 { get; set; }

        /// <summary>
        /// Unlocked cheats, part 3.
        /// </summary>
        public byte UnlockedCheats3 { get; set; }

        /// <summary>
        /// Unused.
        /// </summary>
        public byte Padding { get; set; }

        /// <summary>
        /// Completion times in seconds.
        /// </summary>
        /// <remarks>
        /// 4 is ... difficulty? File folder?
        /// </remarks>
        public byte[] Times { get; set; } = new byte[(BondConstants.SinglePlayerLevelCount - 1) * 4];
    }
}
