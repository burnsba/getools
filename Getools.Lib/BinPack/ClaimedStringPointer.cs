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
        private RodataString _pointsTo = null;

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetPointerAlignment;

        /// <summary>
        /// Gets or sets the .data section offset of the pointer.
        /// </summary>
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <summary>
        /// Gets the .data section size in bytes of the pointer.
        /// </summary>
        [JsonIgnore]
        public int BaseDataSize => Config.TargetPointerSize;

        /// <summary>
        /// Gets or sets the file offset being pointed to.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        [JsonIgnore]
        public int PointedToOffset { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes used in .data section for the data.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        [JsonIgnore]
        public int PointedToSize { get; set; }

        /// <summary>
        /// Not used for <see cref="ClaimedStringPointer"/>.
        /// </summary>
        [JsonIgnore]
        public string AddressOfVariableName
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

        /// <summary>
        /// Gets a value indicating whether the game file pointer is null or not.
        /// </summary>
        [JsonIgnore]
        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, _pointsTo);
            }
        }

        public ClaimedStringPointer()
        {
        }

        public ClaimedStringPointer(string value)
        {
            _pointsTo = new RodataString(value);
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

        /// <summary>
        /// Returns the object the pointer points to.
        /// </summary>
        /// <returns>Object or null.</returns>
        public IGetoolsLibObject Dereference()
        {
            return _pointsTo;
        }

        public RodataString GetLibString()
        {
            return _pointsTo;
        }

        public string GetString()
        {
            if (object.ReferenceEquals(null, _pointsTo))
            {
                return null;
            }

            return _pointsTo.Value;
        }

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
