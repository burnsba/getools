namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Interface to describe object to model pointer used in game file.
    /// </summary>
    public interface IPointerVariable : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Gets or sets the game file variable name of the pointer.
        /// </summary>
        string? AddressOfVariableName { get; set; }

        /// <summary>
        /// Gets a value indicating whether the game file pointer is null or not.
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// Gets or sets the file offset being pointed to.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        int PointedToOffset { get; set; }

        /// <summary>
        /// Gets or sets the size in bytes used in .data section for the data.
        /// This won't be known until after the .data section is assembled.
        /// </summary>
        int PointedToSize { get; set; }

        /// <summary>
        /// Sets the object that the pointer points to.
        /// </summary>
        /// <param name="pointsTo">Objet to point to.</param>
        void AssignPointer(IGetoolsLibObject pointsTo);

        /// <summary>
        /// Returns the object the pointer points to.
        /// </summary>
        /// <returns>Object or null.</returns>
        IGetoolsLibObject? Dereference();
    }
}