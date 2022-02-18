using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib
{
    /// <summary>
    /// Interface for objects.
    /// </summary>
    public interface IGetoolsLibObject
    {
        /// <summary>
        /// Gets Getools.Lib reference id for the object.
        /// </summary>
        Guid MetaId { get; }
    }
}
