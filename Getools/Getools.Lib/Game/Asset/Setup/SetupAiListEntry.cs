using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupAiListEntry.
    /// </summary>
    public class SetupAiListEntry : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct ailist";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Gets or sets ai script id.
        /// Struct offset 0x4.
        /// </summary>
        public UInt32 Id { get; set; }

        /// <summary>
        /// AI Script associated with this entry.
        /// </summary>
        public AiFunction? Function { get; set; }

        /// <summary>
        /// Gets or sets pointer to <see cref="Function"/>.
        /// </summary>
        public PointerVariable? EntryPointer { get; set; }

        /// <summary>
        /// Gets or sets the index of how this entry is sorted
        /// in the <see cref="StageSetupFile"/> AI Script section.
        /// This should coorespond to sorting by <see cref="Id"/>.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => SizeOf;

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Builds a string to describe the current object
        /// as a complete declaraction in c, using normal structs.
        /// Does not include type, variable name, or trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCInlineDeclaration(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append($"{prefix}{{ ");

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Function?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Id)}");

            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Should be called after deserializing from JSON or bin.
        /// Cleans up values/properties, sets pointers, variable names, etc.
        /// </summary>
        public void DeserializeFix()
        {
            if (object.ReferenceEquals(null, EntryPointer))
            {
                EntryPointer = new PointerVariable();

                if (!object.ReferenceEquals(null, Function))
                {
                    EntryPointer.AssignPointer(Function);
                }
            }
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            // Leaving this not implemented.
            // Collect should be called by the DataSectionAiList because the entries
            // and prequel entries need to be placed in the correct order as a complete
            // group.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            if (object.ReferenceEquals(null, EntryPointer))
            {
                throw new NullReferenceException();
            }

            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;
            int pointerOffset = 0;

            // need to save offset in struct to set the pointer baseaddress
            pointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Id);
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            EntryPointer.BaseDataOffset = BaseDataOffset + pointerOffset;

            context.RegisterPointer(EntryPointer);
        }
    }
}
