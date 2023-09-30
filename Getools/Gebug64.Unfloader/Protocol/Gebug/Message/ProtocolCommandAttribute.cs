using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;

namespace Gebug64.Unfloader.Protocol.Gebug.Message
{
    /// <summary>
    /// Reflection level meta data to describe message.
    /// </summary>
    /// <remarks>
    /// Message also required <see cref="IActivatorGebugMessage"/>.
    /// </remarks>
    internal class ProtocolCommandAttribute : Attribute
    {
        /// <summary>
        /// Message category.
        /// 1 byte.
        /// </summary>
        public GebugMessageCategory Category { get; set; }

        /// <summary>
        /// Message command.
        /// </summary>
        public byte Command { get; set; }
    }
}
