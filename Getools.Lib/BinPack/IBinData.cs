using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    public interface IBinData : IGetoolsLibObject
    {
        /// <summary>
        /// Gets byte alignment of the data.
        /// A value of 0 or 1 indicates no alignment, a value of 4 is word alignment, etc.
        /// </summary>
        int ByteAlignment { get; }

        /// <summary>
        /// Gets or sets the ELF section offset of the object.
        /// </summary>
        int BaseDataOffset { get; set; }

        /// <summary>
        /// Gets the ELF section size in bytes of the object.
        /// This does not account for alignment adjustments.
        /// </summary>
        int BaseDataSize { get; }

        void Collect(IAssembleContext context);

        void Assemble(IAssembleContext context);
    }
}
