using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Getools.Lib.Error
{
    internal class ExpectedStreamEndException : Exception
    {
        public ExpectedStreamEndException()
        {
        }

        public ExpectedStreamEndException(string message) : base(message)
        {
        }

        public ExpectedStreamEndException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExpectedStreamEndException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
