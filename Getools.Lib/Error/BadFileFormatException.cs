using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Getools.Lib.Error
{
    /// <summary>
    /// General convert exception. Should be thrown when invalid convert parameters are supplied,
    /// not enough information is given to perform a conversion,
    /// required conversion parameters are not set,
    /// and other similar cases.
    /// </summary>
    public class BadFileFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BadFileFormatException"/> class.
        /// </summary>
        public BadFileFormatException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadFileFormatException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public BadFileFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadFileFormatException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner Exception.</param>
        public BadFileFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BadFileFormatException"/> class.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected BadFileFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
