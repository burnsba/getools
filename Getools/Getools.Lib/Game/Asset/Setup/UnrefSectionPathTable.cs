using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pass through to create an unreferenced path table section.
    /// </summary>
    public class UnrefSectionPathTable : DataSectionPathTable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathTable"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        [JsonConstructor]
        private UnrefSectionPathTable(SetupSectionId typeId)
            : base(typeId)
        {
            IsUnreferenced = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionPathTable"/> class.
        /// </summary>
        /// <returns>New object.</returns>
        public static UnrefSectionPathTable NewUnreferencedSection()
        {
            return new UnrefSectionPathTable(SetupSectionId.UnreferencedPathTableEntries);
        }
    }
}
