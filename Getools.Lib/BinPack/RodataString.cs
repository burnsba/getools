using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// MIPS string, will be stored in .rodata section.
    /// The string must be non-null (when placed into .rodata), but can be zero length (one byte, character '\0').
    /// </summary>
    public class RodataString : IGetoolsLibObject, IBinData
    {
        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="RodataString"/> class.
        /// </summary>
        public RodataString()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RodataString"/> class.
        /// </summary>
        /// <param name="value">Value of string.</param>
        public RodataString(string value)
        {
            Value = value;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize
        {
            get
            {
                if (object.ReferenceEquals(null, Value))
                {
                    return 1;
                }

                return Value.Length + 1;
            }
        }

        /// <summary>
        /// Gets or sets the value of the string.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        /// <param name="value">Zero terminated string.</param>
        public static implicit operator RodataString(string value)
        {
            return new RodataString(value);
        }

        /// <summary>
        /// Implicit convertion from int; offst value.
        /// </summary>
        /// <param name="offset">Offset being pointed to.</param>
        public static implicit operator RodataString(int offset)
        {
            return new RodataString() { BaseDataOffset = offset };
        }

        /// <summary>
        /// Gets the ASCII encoding of the string.
        /// </summary>
        /// <param name="prependBytesCount">Optional parameter, number of '\0' characters to prepend before string.</param>
        /// <param name="appendBytesCount">Optional parameter, number of '\0' characters to append after string.</param>
        /// <returns>String as byte array.</returns>
        public byte[] ToByteArray(int? prependBytesCount = null, int? appendBytesCount = null)
        {
            return BitUtility.StringToBytesPad(Value, true, prependBytesCount ?? 0, appendBytesCount ?? 0);
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToRodataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var aac = context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
            BaseDataOffset = aac.DataStartAddress;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null or empty, an
        /// empty string is returned.
        /// </summary>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public string ToCValue(string prefix = "")
        {
            if (string.IsNullOrEmpty(Value))
            {
                return prefix + Formatters.Strings.ToQuotedString(string.Empty);
            }

            return prefix + Formatters.Strings.ToQuotedString(Value);
        }

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null or empty, the c macro NULL
        /// is returned (without quotes).
        /// </summary>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public string ToCValueOrNull(string prefix = "")
        {
            if (string.IsNullOrEmpty(Value))
            {
                return $"{prefix}NULL";
            }

            return prefix + Formatters.Strings.ToQuotedString(Value);
        }

        /// <summary>
        /// Converts the string to a quoted c literal.
        /// If the <see cref="Value"/> is null, the c macro NULL
        /// is returned (without quotes). Otherwise, a quoted string is returned (this may be <see cref="string.Empty"/>).
        /// </summary>
        /// <param name="prefix">Optional prefix before string.</param>
        /// <returns>Quoted value.</returns>
        public string ToCValueOrNullEmpty(string prefix = "")
        {
            if (object.ReferenceEquals(null, Value))
            {
                return $"{prefix}NULL";
            }

            return prefix + Formatters.Strings.ToQuotedString(Value);
        }
    }
}
