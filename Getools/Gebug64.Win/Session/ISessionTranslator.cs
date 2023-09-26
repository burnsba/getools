using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.Win.Session
{
    /// <summary>
    /// Interface to define type conversion.
    /// </summary>
    /// <typeparam name="TRuntimeType">Type used by application at runtime.</typeparam>
    /// <typeparam name="TContainerType">DTO object to be used for serialization to/from disk.</typeparam>
    /// <remarks>
    /// Implementations must have a constructor that accepts <see cref="TranslateService"/>.
    /// </remarks>
    public interface ISessionTranslator<TRuntimeType, TContainerType>
    {
        /// <summary>
        /// Converts the runtime object to DTO.
        /// </summary>
        /// <param name="source">Values to convert.</param>
        /// <returns>Object.</returns>
        TContainerType ConvertFrom(TRuntimeType source);

        /// <summary>
        /// Restores values from a previously saved state.
        /// </summary>
        /// <param name="source">Values to convert</param>
        /// <returns>Object.</returns>
        TRuntimeType ConvertBack(TContainerType source);
    }
}
