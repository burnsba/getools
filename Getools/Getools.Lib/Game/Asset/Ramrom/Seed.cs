using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Ramrom
{
    /// <summary>
    /// Native struct ramrom_seed.
    /// </summary>
    public class Seed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Seed"/> class.
        /// </summary>
        public Seed()
        {
        }

        /// <summary>
        /// Number of frames specified for this <see cref="Blockbuf"/> capture iteration.
        /// </summary>
        public byte SpeedFrames { get; set; }

        /// <summary>
        /// Number of sets of <see cref="Blockbuf"/> in this capture iteration.
        /// </summary>
        public byte Count { get; set; }

        /// <summary>
        /// Random seed.
        /// </summary>
        public byte RandomSeed { get; set; }

        /// <summary>
        /// Checksum byte.
        /// </summary>
        public byte Check { get; set; }

        /// <summary>
        /// Converts seed into byte array.
        /// </summary>
        /// <returns>Data.</returns>
        public byte[] ToByteArray()
        {
            return new byte[] { SpeedFrames, Count, RandomSeed, Check };
        }
    }
}
