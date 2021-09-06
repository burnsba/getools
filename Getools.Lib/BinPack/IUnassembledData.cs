//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Getools.Lib.BinPack
//{
//    /// <summary>
//    /// Interface to describe data that will end up in the .rodata section.
//    /// </summary>
//    public interface IUnassembledData
//    {
//        /// <summary>
//        /// Gets byte alignment of the data.
//        /// A value of 0 or 1 indicates no alignment, a value of 4 is word alignment, etc.
//        /// </summary>
//        int ByteAlignment { get; }

//        /// <summary>
//        /// Gets or sets data that will end up in .rodata section.
//        /// </summary>
//        object UncastData { get; set; }

//        /// <summary>
//        /// Assembles <see cref="UncastData"/> into bytes to be placed in .rodata.
//        /// </summary>
//        /// <param name="currentAddress">Current address of the file. May be necessary if <see cref="ByteAlignment"/> is used.</param>
//        /// <returns>Data.</returns>
//        byte[] GetAssembledData(int currentAddress);
//    }
//}
