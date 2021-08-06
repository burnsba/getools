using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Getools.Lib.Error
{
    public class BadFileFormatException : Exception
    {
        public BadFileFormatException()
        {
        }

        public BadFileFormatException(string message) : base(message)
        {
        }

        public BadFileFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
