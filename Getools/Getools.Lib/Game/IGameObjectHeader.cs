using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    /// <summary>
    /// Common interface for a game object that uses the standard
    /// 32-bit header.
    /// Cooresponds to struct ObjHeaderData.
    /// </summary>
    public interface IGameObjectHeader : IGameObject
    {
        /// <summary>
        /// Gets or sets object scale. Optional. Set to 0 if not used.
        /// </summary>
        UInt16 Scale { get; set; }

        /// <summary>
        /// Gets or sets byte value. Interpretation depends on type.
        /// Optional. Set to 0 if not used.
        /// </summary>
        byte Hidden2Raw { get; set; }

        /// <summary>
        /// Gets or sets propdef / type definition.
        /// </summary>
        byte TypeRaw { get; set; }

        /// <summary>
        /// Converts the object to a string to be used inside a c declaration
        /// of signed int array.
        /// Values are listed as signed 32 bit integers.
        /// </summary>
        /// <param name="prefix">Optional prefix to prepend before string.</param>
        /// <returns>Object as comma seperated list of signed 32 bit ints.</returns>
        string ToCInlineS32Array(string prefix = "");
    }
}
