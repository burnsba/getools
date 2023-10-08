using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Interface to define context used to assemble data into game object file / .bin file.
    /// Compiling is handled in multiple phases.
    /// The first phase is to collect objects in the correct order. The <see cref="IBinData"/>
    /// should call back to <see cref="AppendToDataSection"/> or <see cref="AppendToRodataSection"/>.
    /// After all objects are added to the context, <see cref="Assemble"/> should be called.
    /// This should iterate the items and call <see cref="IBinData.Assemble(IAssembleContext)"/>.
    /// Each item should call back to <see cref="this"/> to <see cref="AssembleAppendBytes"/>.
    /// Each item should also register any used pointers via <see cref="RegisterPointer(PointerVariable)"/>.
    /// After assembly is complete, call <see cref="GetLinkedFile"/> to set pointer values
    /// and get the compiled file contents.
    /// </summary>
    public interface IAssembleContext
    {
        /// <summary>
        /// Used during the collect phase of compiling, appends the object to the .data section.
        /// </summary>
        /// <param name="data">Object to add.</param>
        void AppendToDataSection(IBinData data);

        /// <summary>
        /// Used during the collect phase of compiling, appends the object to the .rodata section.
        /// </summary>
        /// <param name="data">Object to add.</param>
        void AppendToRodataSection(IBinData data);

        /// <summary>
        /// This iterates all of the data collected from <see cref="AppendToDataSection"/>
        /// and <see cref="AppendToRodataSection"/> calling <see cref="IBinData.Assemble(IAssembleContext)"/>
        /// to convert each object to a byte array.
        /// Each <see cref="IBinData"/> should make a call back to <see cref="AssembleAppendBytes"/> to
        /// add the object as byte array to this file.
        /// </summary>
        void Assemble();

        /// <summary>
        /// Used during the assembling phase of compiling. Registers a pointer that will
        /// be resolved once assembling is complete.
        /// </summary>
        /// <param name="pointer">Pointer to resolve later.</param>
        void RegisterPointer(IPointerVariable pointer);

        /// <summary>
        /// Stops tracking pointer.
        /// </summary>
        /// <param name="pointer">Pointer to no longer resolve.</param>
        void RemovePointer(IPointerVariable pointer);

        /// <summary>
        /// Removes any pointers to this object. If there are no pointers, nothing happens.
        /// </summary>
        /// <param name="pointsTo">Object to remove references to.</param>
        void UnreferenceObject(IGetoolsLibObject pointsTo);

        /// <summary>
        /// Used during the assembling phase of compiling. Appends byte array to the current
        /// file contents. Will prepend any necessary zero bytes to account for alignment.
        /// </summary>
        /// <param name="bytes">Bytes to add to file.</param>
        /// <param name="align">Alignment of bytes. 0 or 1 is unused, 4 is (MIPS) word aligned, etc.</param>
        /// <returns>Information about where the bytes were added. Used to register pointers once the start address of the data is known.</returns>
        AssembleAddressContext AssembleAppendBytes(byte[] bytes, int align);

        /// <summary>
        /// Returns current address of the file being assembled. Does not adjust for alignment.
        /// </summary>
        /// <returns>Current address.</returns>
        int GetCurrentAddress();

        /// <summary>
        /// Returns current section being assembled.
        /// </summary>
        /// <returns>Current section.</returns>
        MipsElfSection GetCurrentSection();

        /// <summary>
        /// Builds .data section and .rodata section.
        /// Resulting file is saved in an internal variable which can be retrieved by calling this again
        /// (i.e., the return value can be ignored one or more times).
        /// All pointers from .data to .rodata are resolved.
        /// The .rodata section and end of file are aligned to 16 bytes.
        /// Call <see cref="Assemble"/> to build byte arrays first.
        /// </summary>
        /// <returns>Full linked and assembled file as byte array.</returns>
        byte[]? GetLinkedFile();
    }
}
