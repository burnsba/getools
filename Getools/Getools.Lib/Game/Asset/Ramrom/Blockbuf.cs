using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Ramrom
{
    /// <summary>
    /// Native struct ramrom_blockbuf.
    /// </summary>
    public class Blockbuf
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Blockbuf"/> class.
        /// </summary>
        public Blockbuf()
        {
        }

        /// <summary>
        /// Control stick x value.
        /// </summary>
        public sbyte StickX { get; set; }

        /// <summary>
        /// Control stick y value.
        /// </summary>
        public sbyte StickY { get; set; }

        /// <summary>
        /// Low byte of button press.
        /// </summary>
        public byte ButtonLow { get; set; }

        /// <summary>
        /// High byte of button press.
        /// </summary>
        public byte ButtonHigh { get; set; }
    }
}
