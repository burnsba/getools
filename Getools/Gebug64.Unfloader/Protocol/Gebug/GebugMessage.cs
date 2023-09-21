using System;
using System.Collections.Generic;
using System.Linq;
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
        public GebugMessageCategory Category { get; init; }

        public int Command { get; set; }

        protected GebugMessage(GebugMessageCategory category)
        {
            Category = category;
        }

        public static GebugMessage FromPacket(GebugPacket packet)
        {
            return new GebugMetaPingMessage();
        }

        public static GebugMessage FromPackets(IEnumerable<GebugPacket> packets)
        {
            return new GebugMetaPingMessage();
        }

        public List<GebugPacket> ToSendPackets()
        {
            var propAttributes = new List<PropAttribute>();

            var props = GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(GebugParameter)));
            foreach (var prop in props)
            {
                var customAttributes = (GebugParameter[])GetType().GetCustomAttributes(typeof(GebugParameter), true);
                if (customAttributes.Length > 0)
                {
                    var attr = customAttributes[0];

                    propAttributes.Add(new PropAttribute() { Property = prop, Attribute = attr });
                }
            }

            var bodyBytes = new List<IEnumerable<byte>>();

            foreach (var pa in propAttributes.OrderBy(x => x.Attribute.ParameterIndex))
            {
                bodyBytes.Add(GetPropertySendBytes(pa));
            }

            return new List<GebugPacket>();
        }

        private byte[] GetPropertySendBytes(PropAttribute pa)
        {
            var result = new List<byte>();

            if (pa.Attribute.UseDirection == ParameterUseDirection.PcToConsole
              || pa.Attribute.UseDirection == ParameterUseDirection.Both)
            {
                var value = pa.Property.GetValue(this);

                if (pa.Attribute.IsVariableSize)
                {
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
    }
}
