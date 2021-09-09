using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Interface to define object that can be compiled into file used by the game.
    /// </summary>
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

        /// <summary>
        /// Called by <see cref="IAssembleContext"/>, this is used to append lib objects
        /// into the correct order they should appear in the file.
        /// </summary>
        /// <param name="context">Context to add object to.</param>
        void Collect(IAssembleContext context);

        /// <summary>
        /// Called by <see cref="IAssembleContext"/>, this is used to convert the object
        /// to a byte array to be assembled into the file.
        /// </summary>
        /// <param name="context">Context to add byte array to.</param>
        void Assemble(IAssembleContext context);
    }
}
