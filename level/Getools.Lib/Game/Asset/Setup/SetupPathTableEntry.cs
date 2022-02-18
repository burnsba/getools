using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// SetupPathTableEntry.
    /// </summary>
    public class SetupPathTableEntry : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathTbl";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 16;

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets pad id.
        /// Struct offset 0x0.
        /// </summary>
        public UInt32 PadId { get; set; }

        /// <summary>
        /// Value pointed to from <see cref="EntryPointer"/>.
        /// </summary>
        public PathTable Entry { get; set; }

        /// <summary>
        /// Gets or sets pointer to <see cref="Entry"/>.
        /// Struct offset 0x4.
        /// </summary>
        public PointerVariable EntryPointer { get; set; }

        /// <summary>
        /// Gets or sets group num.
        /// Struct offset 0x8.
        /// </summary>
        public UInt32 GroupNum { get; set; }

        /// <summary>
        /// Gets or sets distance
        /// Struct offset 0xc.
        /// </summary>
        public UInt32 Distance { get; set; }

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

            sb.Append($"{Formatters.IntegralTypes.ToHex8(PadId)}");
            sb.Append($", ");
            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Entry?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(GroupNum)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Distance)}");

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

                if (!object.ReferenceEquals(null, Entry))
                {
                    EntryPointer.AssignPointer(Entry);
                }
            }
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            // Leaving this not implemented.
            // Collect should be called by the DataSectionPathTable because the entries
            // and prequel entries need to be placed in the correct order as a complete
            // group.
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;
            int pointerOffset = 0;

            BitUtility.Insert32Big(bytes, pos, (int)PadId);
            pos += Config.TargetWordSize;

            // need to save offset in struct to set the pointer baseaddress
            pointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)GroupNum);
            pos += Config.TargetWordSize;

            BitUtility.Insert32Big(bytes, pos, (int)Distance);
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            EntryPointer.BaseDataOffset = BaseDataOffset + pointerOffset;

            context.RegisterPointer(EntryPointer);
        }
    }
}
