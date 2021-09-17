using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Non main section. Pseudo section, used to claim bytes.
    /// Shouldn't print any data.
    /// </summary>
    public class RefSectionIgnore : SetupDataSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefSectionIgnore"/> class.
        /// </summary>
        public RefSectionIgnore()
            : base(SetupSectionId.Ignored)
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public override int BaseDataSize { get; set; }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <summary>
        /// This is a "fake" setion only used to put the data in the right place, should refer to parent Intros section.
        /// </summary>
        /// <returns>Zero.</returns>
        public override int GetEntriesCount()
        {
            return 0;
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return 0;
        }

        /// <inheritdoc />
        public override void Collect(IAssembleContext context)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void Assemble(IAssembleContext context)
        {
            // nothing to do
        }
    }
}
