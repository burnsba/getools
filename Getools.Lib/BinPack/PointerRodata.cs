using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Container for value that points to rodata section.
    /// </summary>
    public class PointerRodata
    {
        /// <summary>
        /// Size in bytes of pointer from .data section to .rodata section.
        /// </summary>
        /// <remarks>
        /// N64 MIPS architecture.
        /// </remarks>
        public const int SizeOfRodataPointer = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerRodata"/> class.
        /// </summary>
        /// <param name="baseDataSize">Pointer size.</param>
        public PointerRodata(int baseDataSize)
        {
            BaseDataSize = baseDataSize;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerRodata"/> class.
        /// </summary>
        /// <param name="value">Value for <see cref="UnassembledData"/>.</param>
        /// <param name="baseDataSize">Pointer size in bytes in .data section.</param>
        public PointerRodata(StringPointer value, int baseDataSize)
        {
            UnassembledData = new UnassembledStringData(value.Value);
            BaseDataOffset = (int)value.Offset;
            BaseDataSize = baseDataSize;
        }

        /// <summary>
        /// Gets or sets the .data section offset of the pointer.
        /// </summary>
        public int BaseDataOffset { get; set; }

        /// <summary>
        /// Gets the .data section size in bytes of the pointer.
        /// </summary>
        public int BaseDataSize { get; private set; }

        /// <summary>
        /// Gets or sets the .rodata section offset this data should reside at.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        public int RodataOffset { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes used in .rodata section for the data.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        public int RodataSize { get; set; }

        /// <summary>
        /// Gets the .rodata section data. This will only be set after
        /// calling <see cref="Assemble"/>.
        /// </summary>
        public byte[] AssembledData { get; private set; }

        /// <summary>
        /// Gets or sets the data that will reside in .rodata section.
        /// </summary>
        public IUnassembledData UnassembledData { get; set; }

        /// <summary>
        /// Once the .data section is assembled, the .rodata section
        /// can be assembled. This will take the current address of the file up to this point
        /// in .rodata and assemble the pointed-to data, taking into account alignment.
        /// This sets <see cref="AssembledData"/>.
        /// </summary>
        /// <param name="currentAddress">Current address of the file, within .rodata section.</param>
        public void Assemble(int currentAddress)
        {
            RodataOffset = currentAddress;
            AssembledData = UnassembledData.GetAssembledData(currentAddress);
            RodataSize = AssembledData.Length;
        }

        /// <summary>
        /// Once the .rodata section has been assembled with correct alignment
        /// (i.e., <see cref="Assemble"/> was called), this will update the pointer
        /// in the .data section that points to this .rodata.
        /// </summary>
        /// <param name="dataSection">Bytes of file assembled up to this point. Probably just the .data section.</param>
        public void SetPointerAddress(byte[] dataSection)
        {
            if (BaseDataSize == 1)
            {
                dataSection[BaseDataOffset] = (byte)RodataOffset;
            }
            else if (BaseDataSize == 2)
            {
                BitUtility.InsertShortBig(dataSection, BaseDataOffset, (short)RodataOffset);
            }
            else if (BaseDataSize == 4)
            {
                BitUtility.Insert32Big(dataSection, BaseDataOffset, RodataOffset);
            }
            else
            {
                throw new NotSupportedException($"Can't insert rodata pointer into array for unsupported datasize={BaseDataSize}");
            }
        }
    }
}
