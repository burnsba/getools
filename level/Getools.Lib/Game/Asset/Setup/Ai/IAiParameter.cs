using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiParameter : IAiCmacro, IAiByteConvertable
    {
        string ParameterName { get; init; }

        int ByteLength { get; init; }

        ByteOrder Endien { get; }

        /// <summary>
        /// Gets value of parameter in <see cref="ByteOrder.BigEndien"/> format.
        /// If the native <see cref="Endien"/> is not in <see cref="ByteOrder.BigEndien"/> format,
        /// the result will be swapped.
        /// </summary>
        /// <returns></returns>
        string ToStringBig();

        /// <summary>
        /// Gets value of parameter in <see cref="ByteOrder.LittleEndien"/> format.
        /// If the native <see cref="Endien"/> is not in <see cref="ByteOrder.LittleEndien"/> format,
        /// the result will be swapped.
        /// </summary>
        /// <returns></returns>
        string ToStringLittle();

        /// <summary>
        /// Converts parameter value to string.
        /// </summary>
        /// <param name="endien">Desired result endieness.</param>
        /// <param name="expandSpecial">Replace special values or ids with enum names.</param>
        /// <returns></returns>
        string ValueToString(ByteOrder endien = ByteOrder.BigEndien, bool expandSpecial = true);

        /// <summary>
        /// Gets value of parameter in specified endieness.
        /// If the native <see cref="Endien"/> is not the same as parameter format,
        /// the result will be swapped.
        /// </summary>
        /// <param name="endien"></param>
        /// <returns></returns>
        int GetIntValue(ByteOrder endien = ByteOrder.BigEndien);
    }
}
