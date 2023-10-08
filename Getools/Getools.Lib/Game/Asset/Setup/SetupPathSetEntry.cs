using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// PathLink.
    /// </summary>
    public class SetupPathSetEntry : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathSet";

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Gets or sets pointer to <see cref="Entry"/>.
        /// </summary>
        public PointerVariable? EntryPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="EntryPointer"/>.
        /// </summary>
        public PathSet? Entry { get; set; }

        /// <summary>
        /// Gets or sets path id.
        /// </summary>
        public byte PathId { get; set; }

        /// <summary>
        /// Gets or sets path flags.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        /// Gets or sets path length.
        /// </summary>
        public UInt16 PathLength { get; set; }

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

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Entry?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex2(PathId)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex2(Flags)}");
            sb.Append($", ");
            sb.Append($"{Formatters.IntegralTypes.ToHex4(PathLength)}");

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
            // Collect should be called by the DataSectionPathSet because the entries
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

            bytes[pos] = PathId;
            pos++;

            bytes[pos] = Flags;
            pos++;

            BitUtility.InsertShortBig(bytes, pos, PathLength);
            pos += 2;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            EntryPointer.BaseDataOffset = BaseDataOffset + pointerOffset;

            context.RegisterPointer(EntryPointer);
        }
    }
}
