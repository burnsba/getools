using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Bg
{
    /// <summary>
    /// Header object declare at top of bg.
    /// Subset of <see cref="BgFile"/>.
    /// </summary>
    public class BgFileHeader : IBinData
    {
        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="BgFileHeader"/> class.
        /// </summary>
        public BgFileHeader()
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <summary>
        /// Initial field, unknown. Appears to always be 4 zero'd bytes.
        /// </summary>
        public int Unknown1 { get; set; }

        /// <summary>
        /// Pointer to the room data array.
        /// </summary>
        public PointerVariable RoomDataTablePointer { get; set; }

        /// <summary>
        /// Pointer to the portal data array.
        /// </summary>
        public PointerVariable PortalDataTablePointer { get; set; }

        /// <summary>
        /// Pointer to the global visibility commands array.
        /// </summary>
        public PointerVariable GlobalVisibilityCommandsPointer { get; set; }

        /// <summary>
        /// Last field, unknown. Appears to always be 4 zero'd bytes.
        /// </summary>
        public int Unknown2 { get; set; }

        public int ByteAlignment => throw new NotImplementedException();

        public int BaseDataOffset { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int BaseDataSize => throw new NotImplementedException();

        public void Assemble(IAssembleContext context)
        {
            throw new NotImplementedException();
        }

        public void Collect(IAssembleContext context)
        {
            throw new NotImplementedException();
        }
    }
}
