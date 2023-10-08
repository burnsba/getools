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
    /// <summary>
    /// AI Command parameter.
    /// Can be used in the concrete implementation, in which case the value needs to be set,
    /// or can be used in the command desription.
    /// </summary>
    public class AiParameter : IAiParameter
    {
        private readonly byte[] _value;
        private readonly ByteOrder _endien;
        private readonly int _valueBig;
        private readonly int _valueLittle;

        /// <summary>
        /// Initializes a new instance of the <see cref="AiParameter"/> class.
        /// </summary>
        /// <remarks>
        /// Should be used with AI Command descriptions (i.e., not concrete).
        /// </remarks>
        public AiParameter()
        {
            _value = new byte[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiParameter"/> class.
        /// </summary>
        /// <param name="name">Parameter name.</param>
        /// <param name="length">Set <see cref="ByteLength"/>. Must be between 1 and 4 bytes inclusive.</param>
        /// <param name="val">Value of parameter. This parameter should use <paramref name="endien"/>.</param>
        /// <param name="endien">Specifies the internal byte order of the parameter.</param>
        /// <exception cref="NotSupportedException">If length is not between 1 and 4 bytes inclusive.</exception>
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

                _valueLittle >>= 8 * (4 - ByteLength);

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

                _valueBig <<= 8 * (4 - ByteLength);

                _valueLittle = BinaryPrimitives.ReverseEndianness(_valueBig);
            }
        }

        /// <inheritdoc />
        public string? ParameterName { get; init; }

        /// <inheritdoc />
        public int ByteLength { get; init; }

        /// <inheritdoc />
        public ByteOrder Endien => _endien;

        /// <inheritdoc />
        public void CMacroAppend(string prefix, StringBuilder sb)
        {
            var byteText = string.Join(string.Empty, ToByteArray().Select(x => x.ToString("x2")));

            sb.Append($"{prefix}0x{byteText}");
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ParameterName}={GetValueText()}";
        }

        /// <inheritdoc />
        public string ToStringBig()
        {
            return $"{ParameterName}={GetValueText(ByteOrder.BigEndien)}";
        }

        /// <inheritdoc />
        public string ToStringLittle()
        {
            return $"{ParameterName}={GetValueText(ByteOrder.LittleEndien)}";
        }

        /// <inheritdoc />
        public string ValueToString(ByteOrder endien = ByteOrder.BigEndien, bool expandSpecial = true)
        {
            return GetValueText(endien, expandSpecial: expandSpecial);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public int GetIntValue(ByteOrder endien = ByteOrder.BigEndien)
        {
            if (endien == ByteOrder.BigEndien)
            {
                return _valueBig;
            }

            return _valueLittle;
        }

        /// <summary>
        /// Gets value of parameter in specified endieness.
        /// Note that the value is stored internally as an <see cref="Int32"/>,
        /// so endieness applies.
        /// </summary>
        /// <param name="endien">Which end of the internal value to read.</param>
        /// <returns>Value as byte.</returns>
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
            // x86 native format
            int workingValue = GetIntValue(ByteOrder.LittleEndien);

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
            else if (expandSpecial && ParameterName == "pad")
            {
                // 9000 is Special ID for selecting PadPreset in AI list.
                // stored as chr->padpreset1
                if (workingValue == 9000)
                {
                    return "PAD_PRESET1";
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
