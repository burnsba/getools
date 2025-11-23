using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Getools.Lib.Error
{
    /// <summary>
    /// Internal error. This exception should be thrown when a file is currently
    /// being parsed, but it ends sooner than expected.
    /// </summary>
    internal class ExpectedStreamEndException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedStreamEndException"/> class.
        /// </summary>
        public ExpectedStreamEndException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedStreamEndException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public ExpectedStreamEndException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedStreamEndException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner Exception.</param>
        public ExpectedStreamEndException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
