using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Unfloader.Protocol.Gebug.Parameter;
using Getools.Lib;

namespace Gebug64.Unfloader.Protocol.Gebug
{
    /// <summary>
    /// General gebug message.
    /// </summary>
    /// <remarks>
    /// All message logic should be contained in this class. The concrete implementations will have
    /// attributes to define the message parameters, which will be resolved at runtime.
    /// </remarks>
    public abstract class GebugMessage : IGebugMessage
    {
        /// <summary>
        /// This collection will contain the available message types that can be instantiated through reflection.
        /// This goes through a two step filter process, looking for classes that implement <see cref="IActivatorGebugMessage"/>, with
        /// an attribute of <see cref="Gebug64.Unfloader.Protocol.Gebug.Message.ProtocolCommand"/>.
        /// The key is defined as the two-byte tuple of <see cref="Category"/> and <see cref="Command"/>.
        /// </summary>
        private static Dictionary<ushort, Type>? _messageTypes = null;

        private ushort _messageId;
        private ushort _ackId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMessage"/> class.
        /// </summary>
        /// <param name="category">Category of message.</param>
        protected GebugMessage(GebugMessageCategory category)
        {
            Category = category;

            MessageId = GebugPacket.GetRandomMessageId();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GebugMessage"/> class.
        /// </summary>
        protected GebugMessage()
        {
            var customAttributes = (ProtocolCommand[])this.GetType().GetCustomAttributes(typeof(ProtocolCommand), true);

            if (!customAttributes.Any())
            {
                throw new InvalidOperationException();
            }

            var attr = customAttributes[0];

            Category = attr.Category;
            Command = attr.Command;

            MessageId = GebugPacket.GetRandomMessageId();
        }

        /// <inheritdoc />
        public GebugMessageCategory Category { get; init; }

        /// <inheritdoc />
        public ushort MessageId
        {
            get
            {
                if (object.ReferenceEquals(null, FirstPacket))
                {
                    return _messageId;
                }

                return FirstPacket.MessageId;
            }

            private set
            {
                _messageId = value;
            }
        }

        /// <inheritdoc />
        public ushort AckId
        {
            get
            {
                if (object.ReferenceEquals(null, FirstPacket))
                {
                    return _ackId;
                }

                return FirstPacket.AckId;
            }

            private set
            {
                _ackId = value;
            }
        }

        /// <inheritdoc />
        public int Command { get; set; }

        /// <inheritdoc />
        public DateTime InstantiateTime { get; set; } = DateTime.Now;

        /// <inheritdoc />
        public CommunicationSource Source { get; set; } = CommunicationSource.Pc;

        /// <inheritdoc />
        public GebugPacket? FirstPacket { get; set; }

        /// <summary>
        /// Gets the friendly name of the command.
        /// </summary>
        protected string DebugCommand => Gebug64.Unfloader.Protocol.Gebug.Message.MessageType.CommandResolver.ResolveCommand(Category, Command);

        private string DebugFlags => ((GebugMessageFlags)Flags).ToString();

        /// <summary>
        /// Gets the flags of the first packet, or zero.
        /// </summary>
        private ushort Flags
        {
            get
            {
                if (object.ReferenceEquals(null, FirstPacket))
                {
                    return 0;
                }

                return FirstPacket.Flags;
            }
        }

        /// <summary>
        /// Converts packet into single message.
        /// Assumes this is not a multi-packet message.
        /// </summary>
        /// <param name="packet">Packet to convert into message.</param>
        /// <param name="packetFromDirection">Whether this is source (PC) or reply (console).</param>
        /// <returns>Message.</returns>
        /// <exception cref="NotSupportedException">Throw if key not found (category+command).</exception>
        public static GebugMessage FromPacket(GebugPacket packet, ParameterUseDirection packetFromDirection)
        {
            PopulateTypeResolver();

            var key = LookupKey(packet);

            if (!_messageTypes!.ContainsKey(key))
            {
                throw new NotSupportedException("Could not find message type to instantiate for packet. Is Interface or Attribute missing?");
            }

            var type = _messageTypes[key];

            GebugMessage message = (GebugMessage)Activator.CreateInstance(type)!;
            message.FirstPacket = packet;
            message.MessageId = packet.MessageId;
            message.AckId = packet.AckId;

            // If the packet is coming from the console, property values are unknown, so set them.
            // Otherwise this is from the PC, so assume the values are already set.
            if (packetFromDirection == ParameterUseDirection.ConsoleToPc)
            {
                SetProperties(message, type, packet, packet.Body);
            }

            message.Source = packetFromDirection == ParameterUseDirection.PcToConsole
                ? CommunicationSource.Pc
                : CommunicationSource.N64;

            return message;
        }

        /// <summary>
        /// Combines multiple packets into single messages.
        /// Assumes the collection contains all packets in the message.
        /// Assumes packets are in logical order (by <see cref="GebugPacket.PacketNumber"/>).
        /// </summary>
        /// <param name="packets">Packets to combine into message.</param>
        /// <param name="packetFromDirection">Whether this is source (PC) or reply (console).</param>
        /// <returns>Message.</returns>
        /// <exception cref="NotSupportedException">Throw if key not found (category+command).</exception>
        public static GebugMessage FromPackets(IEnumerable<GebugPacket> packets, ParameterUseDirection packetFromDirection)
        {
            PopulateTypeResolver();

            var key = LookupKey(packets);

            if (!_messageTypes!.ContainsKey(key))
            {
                throw new NotSupportedException("Could not find message type to instantiate for packet. Is Interface or Attribute missing?");
            }

            var type = _messageTypes[key];

            GebugMessage message = (GebugMessage)Activator.CreateInstance(type)!;
            message.FirstPacket = packets.First();
            message.MessageId = message.FirstPacket.MessageId;
            message.AckId = message.FirstPacket.AckId;

            // If the packet is coming from the console, property values are unknown, so set them.
            // Otherwise this is from the PC, so assume the values are already set.
            if (packetFromDirection == ParameterUseDirection.ConsoleToPc)
            {
                SetProperties(message, type, packets.First(), packets.SelectMany(x => x.Body).ToArray());
            }

            message.Source = packetFromDirection == ParameterUseDirection.PcToConsole
                ? CommunicationSource.Pc
                : CommunicationSource.N64;

            return message;
        }

        /// <summary>
        /// Gets standard key (category+command) from packet.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <returns>Key.</returns>
        private static ushort LookupKey(GebugPacket packet)
        {
            ushort key = 0;

            key |= (ushort)(((int)packet.Category) << 8);
            key |= (ushort)packet.Command;

            return key;
        }

        /// <summary>
        /// Gets standard key (category+command) from first packet in collection.
        /// </summary>
        /// <param name="packets">One or more packets.</param>
        /// <returns>Key.</returns>
        private static ushort LookupKey(IEnumerable<GebugPacket> packets)
        {
            ushort key = 0;

            key |= (ushort)(((int)packets.First().Category) << 8);
            key |= (ushort)packets.First().Command;

            return key;
        }

        /// <summary>
        /// Searches the assembly for types that implement <see cref="IActivatorGebugMessage"/>, and
        /// that contains attribute <see cref="Gebug64.Unfloader.Protocol.Gebug.Message.ProtocolCommand"/>, then
        /// saves them into <see cref="_messageTypes"/>.
        /// This method can only be run once.
        /// </summary>
        private static void PopulateTypeResolver()
        {
            if (object.ReferenceEquals(null, _messageTypes))
            {
                var types = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => typeof(IActivatorGebugMessage).IsAssignableFrom(x))
                    .ToList();

                _messageTypes = new Dictionary<ushort, Type>();

                foreach (var type in types)
                {
                    if (Attribute.IsDefined(type, typeof(ProtocolCommand)))
                    {
                        var customAttributes = (ProtocolCommand[])type.GetCustomAttributes(typeof(ProtocolCommand), true);
                        if (customAttributes.Length > 0)
                        {
                            var attr = customAttributes[0];

                            ushort key = 0;

                            key |= (ushort)(((int)attr.Category) << 8);
                            key |= (ushort)attr.Command;

                            _messageTypes.Add(key, type);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when parsing message content.
        /// Reads the incoming body bytes and sets all incoming (console to pc) properties.
        /// Properties are resolved from the <see cref="GebugParameter"/> attribute.
        /// </summary>
        /// <param name="instance">Current message being processed.</param>
        /// <param name="messageType">Concrete implementation type of message being processed.</param>
        /// <param name="firstPacket">First packet of message; used to access header protocol data.</param>
        /// <param name="fullBody">All body byte data for message.</param>
        /// <exception cref="InvalidOperationException">Throw when number parameter mismatch, or error setting property ("native" type)</exception>
        /// <exception cref="NotSupportedException">Throw if trying to set variable length property that isn't byte[].</exception>
        private static void SetProperties(GebugMessage instance, Type messageType, GebugPacket firstPacket, byte[] fullBody)
        {
            var propAttributes = new List<PropAttribute>();

            ushort numberConsoleParameters = 0;

            // Find property definitions on the concrete type.
            var props = messageType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
            foreach (var prop in props)
            {
                var customAttributes = (GebugParameter[])prop.GetCustomAttributes(typeof(GebugParameter), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    // Filter to only incoming properties.
                    if (attr.UseDirection == ParameterUseDirection.ConsoleToPc
                        || attr.UseDirection == ParameterUseDirection.Both)
                    {
                        propAttributes.Add(new PropAttribute(prop, attr));

                        numberConsoleParameters++;
                    }
                }
            }

            bool verifyParameterCount = true;

            // Iterate the incoming properties and set the values according to the
            // info defined in the property attribute.
            int bodyOffset = 0;
            foreach (var pa in propAttributes.OrderBy(x => x.Attribute.ParameterIndex))
            {
                if (pa.Attribute.IsVariableSize == false && pa.Attribute.IsVariableSizeList == true)
                {
                    verifyParameterCount = false;

                    var type = pa.Property.PropertyType;

                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type itemType = type.GetGenericArguments()[0];

                        var listType = typeof(List<>);
                        var constructedListType = listType.MakeGenericType(itemType);

                        IList propertyListInstance = (IList)Activator.CreateInstance(constructedListType)!;

                        var itemTypePropAttributes = new List<PropAttribute>();

                        var itemTypeProps = itemType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
                        foreach (var prop in itemTypeProps)
                        {
                            var customAttributes = (GebugParameter[])prop.GetCustomAttributes(typeof(GebugParameter), true);
                            if (customAttributes.Length > 0)
                            {
                                var attr = customAttributes[0];

                                // Filter to only incoming properties.
                                if (attr.UseDirection == ParameterUseDirection.ConsoleToPc
                                    || attr.UseDirection == ParameterUseDirection.Both)
                                {
                                    itemTypePropAttributes.Add(new PropAttribute(prop, attr));
                                }
                            }
                        }

                        int max = instance.GetVariableSizeListCount();

                        for (int i = 0; i < max; i++)
                        {
                            var subInstance = Activator.CreateInstance(itemType);

                            foreach (var itempa in itemTypePropAttributes.OrderBy(x => x.Attribute.ParameterIndex))
                            {
                                ReadSetPropAttribute(subInstance!, fullBody, itempa, ref bodyOffset);
                            }

                            propertyListInstance.Add(subInstance);
                        }

                        pa.Property.SetValue(instance, propertyListInstance);
                    }
                }
                else
                {
                    ReadSetPropAttribute(instance, fullBody, pa, ref bodyOffset);
                }
            }

            if (verifyParameterCount)
            {
                // sanity check
                if (firstPacket.NumberParameters != numberConsoleParameters)
                {
                    throw new InvalidOperationException($"Error setting properties for message. Expected {numberConsoleParameters} parameters, but message contains {firstPacket.NumberParameters} parameters.");
                }
            }
        }

        /// <inheritdoc />
        public List<GebugPacket> ToSendPackets(ParameterUseDirection sendDirection)
        {
            var results = new List<GebugPacket>();

            var propAttributes = new List<PropAttribute>();

            ushort numberParameters = 0;

            // Start by getting all property definitions on the concrete type.
            // Properties will be filtered out below based on send direction.
            var props = GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
            foreach (var prop in props)
            {
                var customAttributes = (GebugParameter[])prop.GetCustomAttributes(typeof(GebugParameter), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    propAttributes.Add(new PropAttribute(prop, attr));

                    numberParameters++;
                }
            }

            // Collect the entire message body, according to property definitions and send direction.
            var bodyBytesMany = new List<IEnumerable<byte>>();

            foreach (var pa in propAttributes.OrderBy(x => x.Attribute.ParameterIndex))
            {
                bodyBytesMany.Add(GetPropertySendBytes(pa, sendDirection));
            }

            var bodyBytes = bodyBytesMany.SelectMany(x => x).ToArray();

            // The number of bytes available in the body will vary for single
            // and multi-packet messages.
            // First check if this fits in a single packet.
            int dividePacketSize = GebugPacket.ProtocolMaxBodySizeSingle;

            if (bodyBytes.Length > GebugPacket.ProtocolMaxBodySizeSingle)
            {
                dividePacketSize = GebugPacket.ProtocolMaxBodySizeMulti;
            }

            // Determine the total number of packets that will be needed.
            var totalNumPackets = (bodyBytes.Length / dividePacketSize) + 1;

            // An even multiple of the buffer size will be off by one due to the addition above.
            if ((bodyBytes.Length % dividePacketSize) == 0)
            {
                totalNumPackets--;
            }

            // However, want to count a size of zero as exactly one packet.
            if (totalNumPackets == 0)
            {
                totalNumPackets = 1;
            }

            int packetNumber = 1;
            int bodySize = bodyBytes.Length;
            int bodySkip = 0;

            // Make sure message id is set.
            ushort messageId = MessageId;
            if (messageId == 0)
            {
                messageId = GebugPacket.GetRandomMessageId();
            }

            ushort ackId = AckId;

            ushort flags = Flags;

            if (totalNumPackets > ushort.MaxValue)
            {
                throw new NotSupportedException($"Too many packets: {totalNumPackets}");
            }

            // set flag if multi packet
            if (totalNumPackets > 1)
            {
                flags |= (ushort)GebugMessageFlags.IsMultiMessage;
            }

            // Split the body into the appropriate number of packets.
            while (bodySize > 0)
            {
                // Start by taking all available remaining bytes.
                int packetSize = bodySize;

                // But if that's more than can fit in a single packet,
                // only take that amount.
                if (packetSize > dividePacketSize)
                {
                    packetSize = dividePacketSize;
                }

                // optional header values should be null if not needed.
                ushort? packetNumberParameter = null;
                ushort? totalPacketsParameter = null;

                if (totalNumPackets > 1)
                {
                    packetNumberParameter = (ushort)packetNumber;
                    totalPacketsParameter = (ushort)totalNumPackets;
                }

                var splitPacket = new GebugPacket(
                    Category,
                    (byte)Command,
                    flags,
                    numberParameters,
                    messageId,
                    ackId,
                    packetNumberParameter,
                    totalPacketsParameter,
                    bodyBytes.Skip(bodySkip).Take(packetSize).ToArray());
                results.Add(splitPacket);
                bodySkip += packetSize;

                packetNumber++;
                bodySize -= packetSize;
            }

            // If the body is empty then the above `while` loop didn't
            // generate a message, so create one here.
            if (!results.Any())
            {
                var singlePacket = new GebugPacket(
                    Category,
                    (byte)Command,
                    flags,
                    numberParameters,
                    messageId,
                    ackId,
                    null,
                    null,
                    new byte[0]);
                results.Add(singlePacket);
            }

            return results;
        }

        /// <inheritdoc />
        public void ReplyTo(IGebugMessage source)
        {
            AckId = source.MessageId;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Category} {DebugCommand}";
        }

        /// <summary>
        /// When parsing a <see cref="GebugParameter.IsVariableSizeList"/> parameter, this method
        /// will resolve the number of elements in the list.
        /// </summary>
        /// <returns>Returns zero by default.</returns>
        public virtual int GetVariableSizeListCount()
        {
            return 0;
        }

        /// <summary>
        /// Helper method.
        /// Reads value from byte array and sets property to value.
        /// </summary>
        /// <param name="propertyOwner">Object instance that contains the property.</param>
        /// <param name="fullBody">Byte array that will be read.</param>
        /// <param name="pa">Descriptiong of property.</param>
        /// <param name="bodyOffset">Starts at current byte offset into <paramref name="fullBody"/> that
        /// will be read. Gets updated to the next byte-to-read.</param>
        /// <exception cref="NotSupportedException">Errors on not supported variable sized array prefix, or not supported size.</exception>
        private static void ReadSetPropAttribute(object propertyOwner, byte[] fullBody, PropAttribute pa, ref int bodyOffset)
        {
            if (pa.Attribute.IsVariableSize == true)
            {
                byte escape = fullBody[bodyOffset++];
                int parameterLength = 0;

                if (escape == 0xff)
                {
                    parameterLength = fullBody[bodyOffset];
                    bodyOffset += 1;
                }
                else if (escape == 0xfe)
                {
                    parameterLength = BitUtility.Read16Big(fullBody, bodyOffset);
                    bodyOffset += 2;
                }
                else if (escape == 0xfd)
                {
                    parameterLength = BitUtility.Read32Big(fullBody, bodyOffset);
                    bodyOffset += 4;
                }
                else
                {
                    throw new NotSupportedException($"Error setting property \"{pa.Property.Name}\" value. Received length prefix escape value of 0x{escape:x2}");
                }

                if (pa.Property.PropertyType == typeof(byte[]))
                {
                    var parameterValue = new byte[parameterLength];
                    Array.Copy(fullBody, bodyOffset, parameterValue, 0, parameterLength);
                    bodyOffset += parameterLength;

                    pa.Property.SetValue(propertyOwner, parameterValue);
                }
                else
                {
                    throw new NotSupportedException($"Error setting property \"{pa.Property.Name}\" value. Expected typeof(byte[]) but type is \"{pa.Property.PropertyType.Name}\"");
                }
            }
            else
            {
                // else assume a regular parameter
                if (pa.Attribute.Size == 1)
                {
                    byte val = fullBody[bodyOffset++];
                    pa.Property.SetValue(propertyOwner, val);
                }
                else if (pa.Attribute.Size == 2)
                {
                    if (pa.Property.PropertyType == typeof(short))
                    {
                        short val16 = (short)BitUtility.Read16Big(fullBody, bodyOffset);
                        bodyOffset += 2;
                        pa.Property.SetValue(propertyOwner, val16);
                    }
                    else
                    {
                        ushort val16 = (ushort)BitUtility.Read16Big(fullBody, bodyOffset);
                        bodyOffset += 2;
                        pa.Property.SetValue(propertyOwner, val16);
                    }
                }
                else if (pa.Attribute.Size == 4)
                {
                    if (pa.Property.PropertyType == typeof(int))
                    {
                        int val32 = (int)BitUtility.Read32Big(fullBody, bodyOffset);
                        bodyOffset += 4;
                        pa.Property.SetValue(propertyOwner, val32);
                    }
                    else if (pa.Property.PropertyType == typeof(float))
                    {
                        Single val32 = BitUtility.CastToFloat((int)BitUtility.Read32Big(fullBody, bodyOffset));
                        bodyOffset += 4;
                        pa.Property.SetValue(propertyOwner, val32);
                    }
                    else
                    {
                        uint val32 = (uint)BitUtility.Read32Big(fullBody, bodyOffset);
                        bodyOffset += 4;
                        pa.Property.SetValue(propertyOwner, val32);
                    }
                }
                else
                {
                    throw new NotSupportedException($"Error setting property \"{pa.Property.Name}\" value. Size not supported: {pa.Attribute.Size}");
                }
            }
        }

        /// <summary>
        /// Uses the reflection information about a property and converts it into byte
        /// data that will be included in the message body.
        /// Byte values will be in big endien format, for values of length 4 bytes or less.
        /// Variable length values will include the length prefix.
        /// </summary>
        /// <param name="pa">Reflection info.</param>
        /// <param name="sendDirection">Whether this is source (PC) or reply (console).</param>
        /// <returns>Byute data.</returns>
        /// <exception cref="NullReferenceException">Throw when property value is null.</exception>
        private byte[] GetPropertySendBytes(PropAttribute pa, ParameterUseDirection sendDirection)
        {
            var result = new List<byte>();

            if (pa.Attribute.UseDirection == sendDirection
              || pa.Attribute.UseDirection == ParameterUseDirection.Both)
            {
                var value = pa.Property.GetValue(this);

                if (object.ReferenceEquals(null, value))
                {
                    throw new NullReferenceException("Can't insert null property into packet");
                }

                if (pa.Attribute.IsVariableSize)
                {
                    result.AddRange(Parameter.ParameterInfo.GetParameterPrefix(value));
                    result.AddRange(ByteExtractor.GetBytesEnumerable(value));
                }
                else
                {
                    var size = pa.Attribute.Size;
                    result.AddRange(ByteExtractor.GetBytes(value, size, Getools.Lib.Architecture.ByteOrder.BigEndien));
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Helper container to pair property instance with meta data.
        /// </summary>
        private record PropAttribute
        {
            public PropAttribute(PropertyInfo property, GebugParameter attribute)
            {
                Property = property;
                Attribute = attribute;
            }

            /// <summary>
            /// Property.
            /// </summary>
            public PropertyInfo Property { get; init; }

            /// <summary>
            /// Attribute.
            /// </summary>
            public GebugParameter Attribute { get; init; }
        }
    }
}
