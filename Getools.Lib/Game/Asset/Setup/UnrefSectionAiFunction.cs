using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pass through to create an unreferenced AI function section.
    /// </summary>
    public class UnrefSectionAiFunction : DataSectionAiList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionAiFunction"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        [JsonConstructor]
        private UnrefSectionAiFunction(SetupSectionId typeId)
            : base(typeId)
        {
            IsUnreferenced = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionAiFunction"/> class.
        /// </summary>
        /// <returns>New object.</returns>
        public static UnrefSectionAiFunction NewUnreferencedSection()
        {
            return new UnrefSectionAiFunction(SetupSectionId.UnreferencedAiFunctions);
        }
    }
}
