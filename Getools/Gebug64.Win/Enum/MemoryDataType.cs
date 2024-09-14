using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Enum
{
    /// <summary>
    /// Data structure of memory watch data.
    /// </summary>
    public enum MemoryDataType
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Signed 8 bit value.
        /// </summary>
        S8,

        /// <summary>
        /// Unsigned 8 bit value.
        /// </summary>
        U8,

        /// <summary>
        /// Signed 16 bit value.
        /// </summary>
        S16,

        /// <summary>
        /// Unsigned 16 bit value.
        /// </summary>
        U16,

        /// <summary>
        /// Signed 32 bit value.
        /// </summary>
        S32,

        /// <summary>
        /// Unsigned 32 bit value.
        /// </summary>
        U32,

        /// <summary>
        /// 32 bit IEEE 754 floating point.
        /// </summary>
        F32,

        /// <summary>
        /// Unsigned 8 bit byte array.
        /// </summary>
        Array,

        /// <summary>
        /// Unsigned 32 bit value.
        /// </summary>
        Pointer,
    }
}
