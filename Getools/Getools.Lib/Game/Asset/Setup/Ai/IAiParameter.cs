using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Interface to define a parameter to an AI Command.
    /// </summary>
    public interface IAiParameter : IAiCmacro, IAiByteConvertable
    {
        /// <summary>
        /// Friendly parameter name.
        /// </summary>
        string? ParameterName { get; init; }

        /// <summary>
        /// Number of bytes in the parameter.
        /// </summary>
        int ByteLength { get; init; }

        /// <summary>
        /// Endieneness of parameter.
        /// </summary>
        ByteOrder Endien { get; }

        /// <summary>
        /// Gets value of parameter in <see cref="ByteOrder.BigEndien"/> format.
        /// If the native <see cref="Endien"/> is not in <see cref="ByteOrder.BigEndien"/> format,
        /// the result will be swapped.
        /// </summary>
        /// <returns>Value as string using big endien byte order.</returns>
        string ToStringBig();

        /// <summary>
        /// Gets value of parameter in <see cref="ByteOrder.LittleEndien"/> format.
        /// If the native <see cref="Endien"/> is not in <see cref="ByteOrder.LittleEndien"/> format,
        /// the result will be swapped.
        /// </summary>
        /// <returns>Value as string using little endien byte order.</returns>
        string ToStringLittle();

        /// <summary>
        /// Converts parameter value to string.
        /// </summary>
        /// <param name="endien">Desired result endieness.</param>
        /// <param name="expandSpecial">Replace special values or ids with enum names.</param>
        /// <returns>Value as string using specified byte order.</returns>
        string ValueToString(ByteOrder endien = ByteOrder.BigEndien, bool expandSpecial = true);

        /// <summary>
        /// Gets value of parameter in specified endieness.
        /// If the native <see cref="Endien"/> is not the same as parameter format,
        /// the result will be swapped.
        /// </summary>
        /// <param name="endien">Byte order.</param>
        /// <returns>Value as int.</returns>
        int GetIntValue(ByteOrder endien = ByteOrder.BigEndien);
    }
}
