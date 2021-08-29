using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Marker to help keep track of order of data in the setup file.
    /// </summary>
    public enum SetupSectionId
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Header struct.
        /// </summary>
        Header = 10,

        /// <summary>
        /// Main array for pad list.
        /// </summary>
        SectionPadList,

        /// <summary>
        /// Main array for pad3d list.
        /// </summary>
        SectionPad3dList,

        /// <summary>
        /// Optional data section.
        /// Used by <see cref="Intro.IntroCredits"/>.
        /// </summary>
        CreditsData,

        /// <summary>
        /// Main array propdef /object list.
        /// </summary>
        SectionObjects,

        /// <summary>
        /// Main array intro lsit.
        /// </summary>
        SectionIntro,

        /// <summary>
        /// Optional data, unreferenced path link (NULL pointer).
        /// </summary>
        UnreferencedPathLinkPointer,

        /// <summary>
        /// Optional data, unreferenced path link list ("not used" {-1}).
        /// </summary>
        UnreferencedPathLinkEntry,

        /// <summary>
        /// Data explicitly referenced from <see cref="SectionPathLink"/>.
        /// </summary>
        PathLinkEntries,

        /// <summary>
        /// Main array for path link list.
        /// </summary>
        SectionPathLink,

        /// <summary>
        /// Main array for pad3d names.
        /// </summary>
        SectionPad3dNames,

        /// <summary>
        /// Optional data, unreferenced path table entries.
        /// </summary>
        UnreferencedPathTableEntries,

        /// <summary>
        /// Data explicitly referenced from <see cref="SectionPathTable"/>.
        /// </summary>
        PathTableEntries,

        /// <summary>
        /// Main array for pad table list.
        /// </summary>
        SectionPathTable,

        /// <summary>
        /// Main array for pad names.
        /// </summary>
        SectionPadNames,

        /// <summary>
        /// Optional data, for path sets.
        /// </summary>
        UnreferencedPathSetEntries,

        /// <summary>
        /// Data explicitly referenced from <see cref="SectionPathSets"/>.
        /// </summary>
        PathSetEntries,

        /// <summary>
        /// Main array for path sets.
        /// </summary>
        SectionPathSets,

        /// <summary>
        /// Optional data, unreferenced AI functions.
        /// </summary>
        UnreferencedAiFunctions,

        /// <summary>
        /// Data explicitly referenced from <see cref="SectionAiList"/>.
        /// </summary>
        AiFunctionEntries,

        /// <summary>
        /// Main array for list of AI functions.
        /// </summary>
        SectionAiList,

        /// <summary>
        /// Unknown filler block.
        /// </summary>
        UnreferencedUnknown,

        /// <summary>
        /// Special file section, .rodata.
        /// </summary>
        Rodata,

        /// <summary>
        /// Special file section, marker to indicate not to print/process.
        /// </summary>
        Ignored,
    }
}
