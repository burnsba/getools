using System;
using System.IO;
using Getools.Lib.BinPack;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Base interface setup section.
    /// </summary>
    public interface ISetupSection : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Gets a value indicating whether this is one of the sections that can be
        /// referenced in the header (see <see cref="IsUnreferenced"/> for status).
        /// </summary>
        bool IsMainSection { get; }

        /// <summary>
        /// If this is a main section, this indicates whether the section is referenced in the header.
        /// If this is a filler section, indicates whether the data is referenced by the associated
        /// main section.
        /// </summary>
        bool IsUnreferenced { get; set; }

        /// <summary>
        /// Gets type of section.
        /// </summary>
        SetupSectionId TypeId { get; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        string VariableName { get; set; }

        /// <summary>
        /// Iterates over the collection after it has been deserialized
        /// and sets any remaining unset indeces or offsets.
        /// Updates variable names based on the indeces.
        /// </summary>
        /// <param name="startingIndex">Optional starting index value.</param>
        void DeserializeFix(int startingIndex = 0);

        /// <summary>
        /// Gets forward declaration text, without end semi colon.
        /// </summary>
        /// <returns>Type name and variable name, with associated array syntax if necessary.</returns>
        string GetDeclarationTypeName();

        /// <summary>
        /// Gets the count of the number of data (struct) entries in the section.
        /// </summary>
        /// <returns>Count.</returns>
        int GetEntriesCount();

        /// <summary>
        /// Gets the size in bytes of the amount of data (struct) that will
        /// be written prior to the main section.
        /// </summary>
        /// <returns>Count in bytes.</returns>
        int GetPrequelDataSize();

        /// <summary>
        /// If this is a main section, will write any associated prequel data.
        /// </summary>
        /// <param name="sw">Stream to write to.</param>
        void WritePrequelData(StreamWriter sw);

        /// <summary>
        /// Writes section contents. This can be main or filler section.
        /// </summary>
        /// <param name="sw">Stream to write to.</param>
        void WriteSectionData(StreamWriter sw);
    }
}