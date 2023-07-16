using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiByteConvertable
    {
        /// <summary>
        /// Gets value of parameter in specified endieness.
        /// If the native <see cref="Endien"/> is not the same as parameter format,
        /// the result will be swapped.
        /// </summary>
        /// <param name="endien"></param>
        /// <returns></returns>
        byte[] ToByteArray(ByteOrder endien = ByteOrder.BigEndien);
    }
}
