using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pass through to create an unreferenced path list section.
    /// </summary>
    public class UnrefSectionPathList : DataSectionPathList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathList"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        [JsonConstructor]
        private UnrefSectionPathList(SetupSectionId typeId)
            : base(typeId)
        {
            IsUnreferenced = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathList"/> class.
        /// </summary>
        /// <returns>New object.</returns>
        public static UnrefSectionPathList NewUnreferencedPathLinkEntry()
        {
            return new UnrefSectionPathList(SetupSectionId.UnreferencedPathLinkEntry);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathList"/> class.
        /// </summary>
        /// <returns>New object.</returns>
        public static UnrefSectionPathList NewUnreferencedPathLinkPointer()
        {
            return new UnrefSectionPathList(SetupSectionId.UnreferencedPathLinkPointer);
        }
    }
}
