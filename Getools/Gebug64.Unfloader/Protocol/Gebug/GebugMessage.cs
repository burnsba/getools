using System;
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
    public abstract class GebugMessage : IGebugMessage
    {
        private ushort _messageId;
        private ushort _ackId;

        private static Dictionary<ushort, Type> _messageTypes = null;

        public GebugMessageCategory Category { get; init; }

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

        public ushort Flags
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

        public int Command { get; set; }

        public GebugPacket? FirstPacket { get; set; }

        protected GebugMessage(GebugMessageCategory category)
        {
            Category = category;

            MessageId = GebugPacket.GetRandomMessageId();
        }

        public static GebugMessage FromPacket(GebugPacket packet, ParameterUseDirection packetFromDirection)
        {
            PopulateTypeResolver();

            var key = LookupKey(packet);

            if (!_messageTypes.ContainsKey(key))
            {
                throw new NotSupportedException("Could not find message type to instantiate for packet. Is Interface or Attribute missing?");
            }

            var type = _messageTypes[key];

            GebugMessage message = (GebugMessage)Activator.CreateInstance(type)!;
            message.FirstPacket = packet;
            message.MessageId = packet.MessageId;
            message.AckId = packet.AckId;

            if (packetFromDirection == ParameterUseDirection.ConsoleToPc)
            {
                SetProperties(message, type, packet, packet.Body);
            }

            return message;
        }

        public static GebugMessage FromPackets(IEnumerable<GebugPacket> packets, ParameterUseDirection packetFromDirection)
        {
            PopulateTypeResolver();

            var key = LookupKey(packets);

            if (!_messageTypes.ContainsKey(key))
            {
                throw new NotSupportedException("Could not find message type to instantiate for packet. Is Interface or Attribute missing?");
            }

            var type = _messageTypes[key];

            GebugMessage message = (GebugMessage)Activator.CreateInstance(type)!;
            message.FirstPacket = packets.First();
            message.MessageId = message.FirstPacket.MessageId;
            message.AckId = message.FirstPacket.AckId;

            if (packetFromDirection == ParameterUseDirection.ConsoleToPc)
            {
                SetProperties(message, type, packets.First(), packets.SelectMany(x => x.Body).ToArray());
            }

            return message;
        }

        public List<GebugPacket> ToSendPackets(ParameterUseDirection sendDirection)
        {
            var results = new List<GebugPacket>();

            var propAttributes = new List<PropAttribute>();

            ushort numberParameters = 0;

            var props = GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
            foreach (var prop in props)
            {
                var customAttributes = (GebugParameter[])prop.GetCustomAttributes(typeof(GebugParameter), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    propAttributes.Add(new PropAttribute() { Property = prop, Attribute = attr });

                    numberParameters++;
                }
            }

            var bodyBytesMany = new List<IEnumerable<byte>>();

            foreach (var pa in propAttributes.OrderBy(x => x.Attribute.ParameterIndex))
            {
                bodyBytesMany.Add(GetPropertySendBytes(pa, sendDirection));
            }

            var bodyBytes = bodyBytesMany.SelectMany(x => x).ToArray();

            int dividePacketSize = GebugPacket.ProtocolMaxBodySizeSingle;

            if (bodyBytes.Length > GebugPacket.ProtocolMaxBodySizeSingle)
            {
                dividePacketSize = GebugPacket.ProtocolMaxBodySizeMulti;
            }

            var totalNumPackets = (bodyBytes.Length / (dividePacketSize)) + 1;

            // An even multiple of the buffer size will be off by one due to the addition above.
            if ((bodyBytes.Length % (dividePacketSize)) == 0)
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

            if (totalNumPackets > 1)
            {
                flags |= (ushort)GebugMessageFlags.IsMultiMessage;
            }

            while (bodySize > 0)
            {
                int packetSize = bodySize;

                if (packetSize > dividePacketSize)
                {
                    packetSize = dividePacketSize;
                }

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
                    bodyBytes.Skip(bodySkip).Take(packetSize).ToArray()
                    );
                results.Add(splitPacket);
                bodySkip += packetSize;

                packetNumber++;
                bodySize -= packetSize;
            }

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
                    new byte[0]
                    );
                results.Add(singlePacket);
            }

            return results;
        }

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
                    if (sendDirection == ParameterUseDirection.PcToConsole)
                    {
                        // If this is on the PC, any existing values will be in little endien format.
                        result.AddRange(ByteExtractor.GetBytes(value, size, Getools.Lib.Architecture.ByteOrder.LittleEndien));
                    }
                    else
                    {
                        result.AddRange(ByteExtractor.GetBytes(value, size, Getools.Lib.Architecture.ByteOrder.BigEndien));
                    }
                }
            }

            return result.ToArray();
        }

        private static ushort LookupKey(GebugPacket packet)
        {
            ushort key = 0;

            key |= (ushort)(((int)packet.Category) << 8);
            key |= (ushort)packet.Command;

            return key;
        }

        private static ushort LookupKey(IEnumerable<GebugPacket> packets)
        {
            ushort key = 0;

            key |= (ushort)(((int)packets.First().Category) << 8);
            key |= (ushort)packets.First().Command;

            return key;
        }

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
                    if (Attribute.IsDefined(type, typeof(ProtocolCommandAttribute)))
                    {
                        var customAttributes = (ProtocolCommandAttribute[])type.GetCustomAttributes(typeof(ProtocolCommandAttribute), true);
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

        private static void SetProperties(GebugMessage instance, Type messageType, GebugPacket firstPacket, byte[] fullBody)
        {
            var propAttributes = new List<PropAttribute>();

            ushort numberConsoleParameters = 0;

            var props = messageType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
            foreach (var prop in props)
            {
                var customAttributes = (GebugParameter[])prop.GetCustomAttributes(typeof(GebugParameter), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    if (attr.UseDirection == ParameterUseDirection.ConsoleToPc
                        || attr.UseDirection == ParameterUseDirection.Both)
                    {
                        propAttributes.Add(new PropAttribute() { Property = prop, Attribute = attr });

                        numberConsoleParameters++;
                    }
                }
            }

            // sanity check
            if (firstPacket.NumberParameters != numberConsoleParameters)
            {
                throw new InvalidOperationException($"Error setting properties for message. Expected {numberConsoleParameters} parameters, but message contains {firstPacket.NumberParameters} parameters.");
            }

            int bodyOffset = 0;
            foreach (var pa in propAttributes.OrderBy(x => x.Attribute.ParameterIndex))
            {
                if (pa.Attribute.IsVariableSize == false)
                {
                    if (pa.Attribute.Size == 1)
                    {
                        byte val = fullBody[bodyOffset++];
                        pa.Property.SetValue(instance, val);
                    }
                    else if (pa.Attribute.Size == 2)
                    {
                        if (pa.Property.PropertyType == typeof(short))
                        {
                            short val16 = (short)BitUtility.Read16Big(fullBody, bodyOffset);
                            bodyOffset += 2;
                            pa.Property.SetValue(instance, val16);
                        }
                        else
                        {
                            ushort val16 = (ushort)BitUtility.Read16Big(fullBody, bodyOffset);
                            bodyOffset += 2;
                            pa.Property.SetValue(instance, val16);
                        }
                    }
                    else if (pa.Attribute.Size == 4)
                    {
                        if (pa.Property.PropertyType == typeof(int))
                        {
                            int val32 = (int)BitUtility.Read32Big(fullBody, bodyOffset);
                            bodyOffset += 4;
                            pa.Property.SetValue(instance, val32);
                        }
                        else
                        {
                            uint val32 = (uint)BitUtility.Read32Big(fullBody, bodyOffset);
                            bodyOffset += 4;
                            pa.Property.SetValue(instance, val32);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error setting property \"{pa.Property.Name}\" value. Size not supported: {pa.Attribute.Size}");
                    }
                }
                else
                {
                    byte size = fullBody[bodyOffset++];
                    int parameterLength = 0;

                    if (size == 0xfe)
                    {
                        parameterLength = BitUtility.Read16Big(fullBody, bodyOffset);
                        bodyOffset += 2;
                    }
                    else if (size == 0xfd)
                    {
                        parameterLength = BitUtility.Read32Big(fullBody, bodyOffset);
                        bodyOffset += 4;
                    }

                    if (pa.Property.PropertyType == typeof(byte[]))
                    {
                        var parameterValue = new byte[parameterLength];
                        Array.Copy(fullBody, bodyOffset, parameterValue, 0, parameterLength);
                        bodyOffset += parameterLength;

                        pa.Property.SetValue(instance, parameterValue);
                    }
                    else
                    {
                        throw new NotSupportedException($"Error setting property \"{pa.Property.Name}\" value. Expected typeof(byte[]) but type is \"{pa.Property.PropertyType.Name}\"");
                    }
                }
            }
        }
    }
}
