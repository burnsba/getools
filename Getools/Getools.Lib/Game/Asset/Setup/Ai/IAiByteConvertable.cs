using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Interface to desribe something that can be converted to byte array.
    /// </summary>
    public interface IAiByteConvertable
    {
        /// <summary>
        /// Gets value of parameter in specified endieness.
        /// If the native <see cref="Endien"/> is not the same as parameter format,
        /// the result will be swapped.
        /// </summary>
        /// <param name="endien">Byte order.</param>
        /// <returns>Object as byte array.</returns>
        byte[] ToByteArray(ByteOrder endien = ByteOrder.BigEndien);
    }
}
