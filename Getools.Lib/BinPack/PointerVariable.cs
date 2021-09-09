using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Lib object to model pointer used in game file.
    /// </summary>
    public class PointerVariable : IGetoolsLibObject, IBinData
    {
        private Guid _metaId = Guid.NewGuid();
        private IGetoolsLibObject _pointsTo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerVariable"/> class.
        /// </summary>
        public PointerVariable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerVariable"/> class.
        /// </summary>
        /// <param name="pointsTo">Object pointer points to.</param>
        public PointerVariable(IGetoolsLibObject pointsTo)
        {
            _pointsTo = pointsTo;
        }

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
        /// Gets or sets the game file variable name of the pointer.
        /// </summary>
        public string AddressOfVariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <summary>
        /// Gets a value indicating whether the game file pointer is null or not.
        /// </summary>
        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, _pointsTo);
            }
        }

        /// <summary>
        /// Sets the object that the pointer points to.
        /// </summary>
        /// <param name="pointsTo">Objet to point to.</param>
        public void AssignPointer(IGetoolsLibObject pointsTo)
        {
            _pointsTo = pointsTo;
        }

        /// <summary>
        /// Returns the object the pointer points to.
        /// </summary>
        /// <returns>Object or null.</returns>
        public IGetoolsLibObject Dereference()
        {
            return _pointsTo;
        }

        /// <summary>
        /// Gets value of the pointer as byte array.
        /// </summary>
        /// <param name="prependBytesCount">Optional parameter, number of '\0' characters to prepend before string.</param>
        /// <param name="appendBytesCount">Optional parameter, number of '\0' characters to append after string.</param>
        /// <returns>Pointer value.</returns>
        public byte[] ToByteArray(int? prependBytesCount = null, int? appendBytesCount = null)
        {
            int prepend = prependBytesCount ?? 0;
            var resultLength = prepend + Config.TargetPointerSize + (appendBytesCount ?? 0);

            var b = new byte[resultLength];

            // hmmm, shouldn't this be PointedToOffset? Pointer values get updated/set once
            // file is done with assembly phase, so BaseDataOffset probably isn't even used ...
            BitUtility.Insert32Big(b, prepend, BaseDataOffset);
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
