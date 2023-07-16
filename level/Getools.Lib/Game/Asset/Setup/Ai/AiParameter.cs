using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json.Linq;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiParameter : IAiParameter
    {
        private readonly byte[] _value;
        private readonly ByteOrder _endien;
        private readonly int _valueBig;
        private readonly int _valueLittle;

        public string ParameterName { get; init; }
        public int ByteLength { get; init; }
        public ByteOrder Endien => _endien;

        public AiParameter()
        {
            //
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="length"></param>
        /// <param name="val">Value of parameter. This parameter should use <paramref name="endien"/>.</param>
        /// <param name="endien">Specifies the internal byte order of the parameter.</param>
        /// <exception cref="NotSupportedException"></exception>
        public AiParameter(string name, int length, byte[] val, ByteOrder endien = ByteOrder.BigEndien)
        {
            if (length < 1 || length > 4)
            {
                throw new NotSupportedException();
            }

            ParameterName = name;
            ByteLength = length;
            _endien = endien;

            _value = new byte[ByteLength];
            Array.Copy(val, _value, ByteLength);

            if (endien == ByteOrder.BigEndien)
            {
                _valueLittle = val[0] << 24;

                if (ByteLength > 1)
                {
                    _valueLittle |= val[1] << 16;
                }

                if (ByteLength > 2)
                {
                    _valueLittle |= val[2] << 8;
                }

                if (ByteLength > 3)
                {
                    _valueLittle |= val[3] << 0;
                }

                _valueLittle >>= (8 * (4 - ByteLength));

                _valueBig = BinaryPrimitives.ReverseEndianness(_valueLittle);
            }
            else
            {
                _valueBig = val[0] << 24;

                if (ByteLength > 1)
                {
                    _valueBig |= val[1] << 16;
                }

                if (ByteLength > 2)
                {
                    _valueBig |= val[2] << 8;
                }

                if (ByteLength > 3)
                {
                    _valueBig |= val[3] << 0;
                }

                _valueBig <<= (8 * (4 - ByteLength));

                _valueLittle = BinaryPrimitives.ReverseEndianness(_valueBig);
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

        public string ToStringBig()
        {
            return $"{ParameterName}={GetValueText(ByteOrder.BigEndien)}";
        }

        public string ToStringLittle()
        {
            return $"{ParameterName}={GetValueText(ByteOrder.LittleEndien)}";
        }

        public string ValueToString(ByteOrder endien = ByteOrder.BigEndien, bool expandSpecial = true)
        {
            return GetValueText(endien, expandSpecial: expandSpecial);
        }

        public byte[] ToByteArray(ByteOrder endien = ByteOrder.BigEndien)
        {
            var results = new byte[ByteLength];

            Array.Copy(_value, results, ByteLength);

            if (endien != _endien)
            {
                results = results.Reverse().ToArray();
            }

            return results;
        }

        public int GetIntValue(ByteOrder endien = ByteOrder.BigEndien)
        {
            if (endien == ByteOrder.BigEndien)
            {
                return _valueBig;
            }

            return _valueLittle;
        }

        public byte GetByteValue(ByteOrder endien = ByteOrder.BigEndien)
        {
            if (endien == ByteOrder.BigEndien)
            {
                return (byte)((_valueBig & 0xff000000) >> 24);
            }

            return (byte)((_valueLittle & 0xff) >> 0);
        }

        private string GetValueText(ByteOrder endien = ByteOrder.BigEndien, bool expandSpecial = true)
        {
            int workingValue = GetIntValue(endien);

            if (expandSpecial && ParameterName == "chr_num")
            {
                // careful with sign extension here!
                var bb = (int)(sbyte)GetByteValue(endien);

                // chr_num has some reserved values, but these are negative.
                if (Enum.IsDefined(typeof(ChrNum), bb))
                {
                    ChrNum reserverdChr = (ChrNum)bb;
                    return reserverdChr.ToString();
                }
            }
            else if (expandSpecial && ParameterName == "item_num")
            {
                if (Enum.IsDefined(typeof(ItemIds), workingValue))
                {
                    ItemIds item = (ItemIds)workingValue;
                    return item.ToString();
                }
            }
            else if (expandSpecial && ParameterName == "prop_num")
            {
                if (Enum.IsDefined(typeof(PropId), workingValue))
                {
                    PropId prop = (PropId)workingValue;
                    return prop.ToString();
                }
            }

            if (endien != _endien)
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
