using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// MIPS string. A pointer will be placed in .data.
    /// If the string is non-NULL, the string value will be placed in .rodata.
    /// A zero length string (char '\0') is non-NULL.
    /// </summary>
    public class RodataString : IGetoolsLibObject, IBinData
    {
        private PointedToString _rodataString;

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
        public Guid MetaId { get; private set; } = Guid.NewGuid();

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
            context.AppendToDataSection(this);

            if (!object.ReferenceEquals(null, Value))
            {
                _rodataString = new PointedToString()
                {
                    Value = Value,
                };

                _rodataString.Collect(context);
            }
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            // Pointer will be updated when linked
            var bytes = Enumerable.Repeat<byte>(0, Config.TargetWordSize).ToArray();
            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            // unless this is a NULL pointer, then we're done.
            if (!object.ReferenceEquals(null, Value) && !object.ReferenceEquals(null, _rodataString))
            {
                var p = new PointerVariable(_rodataString);
                p.BaseDataOffset = result.DataStartAddress;
                context.RegisterPointer(p);
            }
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

        private class PointedToString : IBinData, IGetoolsLibObject
        {
            public string Value { get; set; }

            public int ByteAlignment => Config.TargetWordSize;

            public int BaseDataOffset { get; set; }

            public int BaseDataSize => Value?.Length ?? 0;

            public Guid MetaId { get; private set; } = Guid.NewGuid();

            public void Collect(IAssembleContext context)
            {
                context.AppendToRodataSection(this);
            }

            public void Assemble(IAssembleContext context)
            {
                var bytes = BitUtility.StringToBytesPad(Value, true, 0, 0);
                var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
                BaseDataOffset = result.DataStartAddress;
            }
        }
    }
}
