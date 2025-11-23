using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Getools.Lib.Error
{
    /// <summary>
    /// Should be thrown when an object is not fully intiailized,
    /// or the current state is not setup, or similar conditions.
    /// </summary>
    public class InvalidStateException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        public InvalidStateException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public InvalidStateException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner Exception.</param>
        public InvalidStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
