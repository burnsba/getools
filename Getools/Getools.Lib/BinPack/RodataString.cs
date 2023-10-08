using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// MIPS string. If <see cref="Collect(IAssembleContext)"/> is called, this will be placed in .rodata.
    /// Otherwise its up to the parent object to place this in the correct file location.
    /// There are no pointers managed by this object.
    /// A terminating zero ('\0') is included in the binary string.
    /// </summary>
    public class RodataString : IGetoolsLibObject, IBinData
    {
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
        public RodataString(string? value)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RodataString"/> class.
        /// </summary>
        /// <param name="baseDataOffset">base data offset.</param>
        /// <param name="value">Value of string.</param>
        public RodataString(int baseDataOffset, string? value)
        {
            BaseDataOffset = baseDataOffset;
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
        public string? Value { get; set; }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        /// <param name="value">Zero terminated string.</param>
        public static implicit operator RodataString(string? value)
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
            if (object.ReferenceEquals(null, Value))
            {
                throw new NullReferenceException();
            }

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
            // Pointer will be updated when linked
            var bytes = ToByteArray();
            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value ?? string.Empty;
        }
    }
}
