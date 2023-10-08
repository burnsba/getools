using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// String pointer with associated string.
    /// The string is <see cref="RodataString"/>.
    /// This object only manages it's own pointer value during .bin building.
    /// It is up to the parent object to dereference the pointer and <see cref="MipsFile.AppendToDataSection(IBinData)"/>
    /// or <see cref="MipsFile.AppendToRodataSection(IBinData)"/> the referenced string in the correct location.
    /// The parent object will also need to <see cref="MipsFile.RegisterPointer(IPointerVariable)"/>.
    /// </summary>
    public class ClaimedStringPointer : IPointerVariable
    {
        private RodataString? _pointsTo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimedStringPointer"/> class.
        /// </summary>
        public ClaimedStringPointer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimedStringPointer"/> class.
        /// </summary>
        /// <param name="value">Initial string value.</param>
        public ClaimedStringPointer(string value)
        {
            _pointsTo = new RodataString(value);
        }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetPointerAlignment;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => Config.TargetPointerSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int PointedToOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int PointedToSize { get; set; }

        /// <summary>
        /// Not used for <see cref="ClaimedStringPointer"/>.
        /// </summary>
        [JsonIgnore]
        public string? AddressOfVariableName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, _pointsTo);
            }
        }

        /// <summary>
        /// Implicit conversion from string.
        /// </summary>
        /// <param name="value">Zero terminated string.</param>
        public static implicit operator ClaimedStringPointer(string value)
        {
            return new ClaimedStringPointer(value);
        }

        /// <summary>
        /// Sets the object that the pointer points to.
        /// </summary>
        /// <param name="pointsTo">Objet to point to.</param>
        public void AssignPointer(IGetoolsLibObject pointsTo)
        {
            if (object.ReferenceEquals(null, pointsTo))
            {
                _pointsTo = null;
            }
            else
            {
                if (pointsTo is RodataString rs)
                {
                    _pointsTo = rs;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(pointsTo)} must be of type {nameof(RodataString)} or null");
                }
            }
        }

        /// <inheritdoc />
        public IGetoolsLibObject? Dereference()
        {
            return _pointsTo;
        }

        /// <summary>
        /// Strongly typed version of <see cref="Dereference"/>.
        /// </summary>
        /// <returns>Underlying <see cref="RodataString"/>.</returns>
        public RodataString? GetLibString()
        {
            return _pointsTo;
        }

        /// <summary>
        /// Gets the underlying string being pointed to or null.
        /// </summary>
        /// <returns>String being pointed to or null.</returns>
        public string? GetString()
        {
            if (object.ReferenceEquals(null, _pointsTo))
            {
                return null;
            }

            return _pointsTo.Value;
        }

        /// <summary>
        /// Sets the underlying string being pointed to.
        /// If the parameter is null, this sets the private <see cref="RodataString"/> to null
        /// instead of making the private <see cref="RodataString"/> be a null pointer.
        /// </summary>
        /// <param name="s">String value.</param>
        public void SetString(string s)
        {
            if (object.ReferenceEquals(null, s))
            {
                _pointsTo = null;
            }
            else
            {
                _pointsTo = new RodataString(s);
            }
        }

        /// <summary>
        /// Converts this object to byte array as it would appear in MIPS .data section.
        /// Alignment is not considered.
        /// </summary>
        /// <returns>Byte array of this pointer value.</returns>
        public byte[] ToByteArray()
        {
            var b = new byte[Config.TargetPointerSize];

            if (!object.ReferenceEquals(null, _pointsTo))
            {
                BitUtility.Insert32Big(b, 0, _pointsTo.BaseDataOffset);
            }

            return b;
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
        }
    }
}
