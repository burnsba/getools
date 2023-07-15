using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathSet, points to a list of ids. This is the definition for a patrol.
    /// </summary>
    public class PathSet : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "s32";

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSet"/> class.
        /// </summary>
        public PathSet()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathSet"/> class.
        /// </summary>
        /// <param name="ids">Collection of ids to initialize listing with.</param>
        public PathSet(IEnumerable<int> ids)
        {
            Ids = ids.ToList();
        }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets ids of this listing.
        /// </summary>
        public List<int> Ids { get; set; } = new List<int>();

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => Config.TargetWordSize * Ids.Count;

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append($"{prefix}{CTypeName} {VariableName}[] = {{ ");

            sb.Append(string.Join(", ", Ids));

            sb.AppendLine(" };");

            return sb.ToString();
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            // Leaving this not implemented.
            // Collect should be called by the DataSectionPathSet because the entries
            // and prequel entries need to be placed in the correct order as a complete
            // group.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var size = Ids.Count * Config.TargetWordSize;
            var bytes = new byte[size];
            int pos = 0;

            foreach (var id in Ids)
            {
                BitUtility.Insert32Big(bytes, pos, id);
                pos += Config.TargetWordSize;
            }

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }
    }
}
