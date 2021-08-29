using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pass through to create an unreferenced path sets section.
    /// </summary>
    public class UnrefSectionPathSets : DataSectionPathSets
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathSets"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        [JsonConstructor]
        private UnrefSectionPathSets(SetupSectionId typeId)
            : base(typeId)
        {
            IsUnreferenced = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathSets"/> class.
        /// </summary>
        /// <returns>New object.</returns>
        public static UnrefSectionPathSets NewUnreferencedSection()
        {
            return new UnrefSectionPathSets(SetupSectionId.UnreferencedPathSetEntries);
        }
    }
}
