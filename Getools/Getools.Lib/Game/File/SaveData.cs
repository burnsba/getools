using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.File
{
    /// <summary>
    /// Native struct save_data.
    /// </summary>
    public class SaveData
    {
        /// <summary>
        /// Size of the `struct save_data` in bytes.
        /// </summary>
        public const int SizeOf = 0x5e; // 94

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

        /// <summary>
        /// Converts this object to byte array as it would appear in MIPS .data section.
        /// Alignment is not considered.
        /// </summary>
        /// <returns>Byte array of this object.</returns>
        public byte[] ToByteArray()
        {
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;

            BitUtility.Insert32Big(bytes, pos, Checksum1);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, Checksum2);
            pos += Config.TargetWordSize;

            bytes[pos++] = CompletionBitflags;
            bytes[pos++] = Flag007;
            bytes[pos++] = MusicVolume;
            bytes[pos++] = SfxVolume;

            BitUtility.Insert16Big(bytes, pos, Options);
            pos += Config.TargetShortSize;

            bytes[pos++] = UnlockedCheats1;
            bytes[pos++] = UnlockedCheats2;
            bytes[pos++] = UnlockedCheats3;
            bytes[pos++] = Padding;

            for (int i = 0; i < Times.Length; i++)
            {
                bytes[pos++] = Times[i];
            }

            return bytes;
        }
    }
}
