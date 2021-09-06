using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    public class PointerVariable : IGetoolsLibObject, IBinData
    {
        private Guid _metaId = Guid.NewGuid();
        private IGetoolsLibObject _pointsTo = null;

        public PointerVariable()
        {
        }

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

        //public string VariableName { get; set; }

        public string AddressOfVariableName { get; set; }

        [JsonIgnore]
        public Guid MetaId => _metaId;

        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, _pointsTo);
            }
        }

        public void AssignPointer(IGetoolsLibObject pointsTo)
        {
            _pointsTo = pointsTo;
        }

        public IGetoolsLibObject Dereference()
        {
            return _pointsTo;
        }

        public byte[] ToByteArray(int? prependBytesCount = null, int? appendBytesCount = null)
        {
            int prepend = prependBytesCount ?? 0;
            var resultLength = prepend + Config.TargetPointerSize + (appendBytesCount ?? 0);

            var b = new byte[resultLength];
            BitUtility.Insert32Big(b, prepend, BaseDataOffset);
            return b;
        }

        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        public void Assemble(IAssembleContext context)
        {
            context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
        }
    }
}
