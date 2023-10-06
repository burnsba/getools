using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Getools.Lib.Kaitai.Gen.RamromReplay;

namespace Getools.Lib.Game.Asset.Ramrom
{
    /// <summary>
    /// Ties together <see cref="Blockbuf"/> and <see cref="Seed"/>
    /// in each iteration.
    /// </summary>
    public class CaptureIteration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CaptureIteration"/> class.
        /// </summary>
        /// <param name="head">Iteration info.</param>
        public CaptureIteration(Seed head)
        {
            Head = head;
        }

        /// <summary>
        /// Offset count since beginning of <see cref="RamromFile.Iterations"/>.
        /// Not included in byte array.
        /// </summary>
        public int FrameIndex { get; set; }

        /// <summary>
        /// Header word for this capture iteration.
        /// </summary>
        public Seed Head { get; set; }

        /// <summary>
        /// Input capture for this iteration.
        /// Length should be <see cref="RamromFile.SizeCommands"/> * <see cref="Seed.Count"/>.
        /// </summary>
        public List<Blockbuf> Blocks { get; set; } = new List<Blockbuf>();

        /// <summary>
        /// Converts <see cref="Head> and all <see cref="Blocks"/> into byte array.
        /// </summary>
        /// <returns>Data.</returns>
        public byte[] ToByteArray()
        {
            // (Head + blocks) times 4 bytes per each.
            var result = new byte[(Blocks.Count + 1) * 4];

            Head.ToByteArray().CopyTo(result, 0);

            int offset = 4;
            foreach (var block in Blocks)
            {
                block.ToByteArray().CopyTo(result, offset);
                offset += 4;
            }

            return result;
        }
    }
}
