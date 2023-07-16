using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json.Linq;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiParameter : IAiParameter
    {
        public string ParameterName { get; init; }
        public int ByteLength { get; init; }
        public int ByteValue { get; init; }

        public AiParameter()
        {
            //
        }

        public AiParameter(string name, int length, int val)
        {
            if (length < 1 || length > 4)
            {
                throw new NotSupportedException();
            }

            ParameterName = name;
            ByteLength = length;
            ByteValue = val;

            if (length == 1)
            {
                ByteValue = val & 0xff;
            }
            else if (length == 2)
            {
                ByteValue = val & 0xffff;
            }
            else if (length == 3)
            {
                ByteValue = val & 0xffffff;
            }
            else if (length == 4)
            {
                ByteValue = val;
            }
        }

        public void CMacroAppend(string prefix, StringBuilder sb)
        {
            var byteText = string.Join(string.Empty, ToByteArray().Select(x => x.ToString("x2")));

            sb.Append($"{prefix}0x{byteText}");
        }

        public override string ToString()
        {
            return $"{ParameterName}={GetValueText()}";
        }

        public string ToStringReverse()
        {
            return $"{ParameterName}={GetValueText(true)}";
        }

        public string ValueToString()
        {
            return GetValueText();
        }

        public string ValueToStringReverse()
        {
            return GetValueText(true);
        }

        public byte[] ToByteArray()
        {
            var results = new byte[ByteLength];

            if (ByteLength > 3)
            {
                results[3] = (byte)((ByteValue & 0xFF000000) >> 24);
            }

            if (ByteLength > 2)
            {
                results[2] = (byte)((ByteValue & 0x00FF0000) >> 16);
            }

            if (ByteLength > 1)
            {
                results[1] = (byte)((ByteValue & 0x0000FF00) >> 8);
            }

            if (ByteLength > 0)
            {
                results[0] = (byte)((ByteValue & 0x000000FF) >> 0);
            }

            return results;
        }

        private string GetValueText(bool reverse = false)
        {
            if (ParameterName == "chr_num")
            {
                var bb = (int)(sbyte)ByteValue;

                // chr_num has some reserved values:
                if (Enum.IsDefined(typeof(ChrNum), bb))
                {
                    ChrNum reserverdChr = (ChrNum)bb;
                    return reserverdChr.ToString();
                }
            }
            else if (ParameterName == "item_num")
            {
                if (Enum.IsDefined(typeof(ItemIds), ByteValue))
                {
                    ItemIds item = (ItemIds)ByteValue;
                    return item.ToString();
                }
            }
            else if (ParameterName == "prop_num")
            {
                if (Enum.IsDefined(typeof(PropId), ByteValue))
                {
                    PropId prop = (PropId)ByteValue;
                    return prop.ToString();
                }
            }

            if (reverse)
            {
                var byteText = string.Join(string.Empty, ToByteArray().Reverse().Select(x => x.ToString("x2")));

                return $"0x{byteText}";
            }
            else
            {
                var byteText = string.Join(string.Empty, ToByteArray().Select(x => x.ToString("x2")));

                return $"0x{byteText}";
            }
        }
    }
}
