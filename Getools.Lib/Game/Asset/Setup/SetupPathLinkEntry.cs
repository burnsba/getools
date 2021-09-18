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
    /// <remarks>
    /// Record ends with (UInt32)0.
    /// </remarks>
    public class SetupPathLinkEntry : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// Each entry ends with this value.
        /// </summary>
        public const UInt32 RecordDelimiter = 0;

        /// <summary>
        /// The overall allocated size of this object in bytes, including child elements.
        /// </summary>
        public const int SizeOf = 16;

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct s_pathLink";

        /// <summary>
        /// Gets or sets pointer to <see cref="Neighbors"/>.
        /// </summary>
        public PointerVariable NeighborsPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="NeighborsPointer"/>.
        /// </summary>
        public PathListing Neighbors { get; set; }

        /// <summary>
        /// Gets or sets pointer to <see cref="Indeces"/>.
        /// </summary>
        public PointerVariable IndexPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="IndexPointer"/>.
        /// </summary>
        public PathListing Indeces { get; set; }

        /// <summary>
        /// Some setups have a single NULL entry before the path link section,
        /// instead of two "not used" arrays. Mark this as a NULL
        /// entry with this property.
        /// </summary>
        [JsonIgnore]
        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, Neighbors) && object.ReferenceEquals(null, Indeces);
            }
        }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string VariableName { get; set; }

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
        /// as a complete declaraction in c, using normal structs. Includes type, variable
        /// name and trailing semi-colon.
        /// </summary>
        /// <param name="prefix">Prefix or indentation.</param>
        /// <returns>String of object.</returns>
        public string ToCDeclaration(string prefix = "")
        {
            if (!IsNull)
            {
                throw new NotImplementedException($"{nameof(SetupPathLinkEntry)} {nameof(ToCDeclaration)} not implemented for non-null variable");
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{prefix}{CTypeName} {VariableName} = NULL;");

            return sb.ToString();
        }

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

            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Neighbors?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{Formatters.Strings.ToCPointerOrNull(Indeces?.VariableName)}");
            sb.Append($", ");
            sb.Append($"{RecordDelimiter}");

            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Should be called after deserializing from JSON or bin.
        /// Cleans up values/properties, sets pointers, variable names, etc.
        /// </summary>
        public void DeserializeFix()
        {
            if (object.ReferenceEquals(null, NeighborsPointer))
            {
                NeighborsPointer = new PointerVariable();

                if (!object.ReferenceEquals(null, Neighbors))
                {
                    NeighborsPointer.AssignPointer(Neighbors);
                }
            }

            if (object.ReferenceEquals(null, IndexPointer))
            {
                IndexPointer = new PointerVariable();

                if (!object.ReferenceEquals(null, Indeces))
                {
                    IndexPointer.AssignPointer(Indeces);
                }
            }
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            // Leaving this not implemented.
            // Collect should be called by the DataSectionPathList because the entries
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

            int neighborPointerOffset = 0;
            int indexPointerOffset = 0;

            // need to save offset in struct to set the pointer baseaddress
            neighborPointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            indexPointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, RecordDelimiter);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            NeighborsPointer.BaseDataOffset = BaseDataOffset + neighborPointerOffset;
            context.RegisterPointer(NeighborsPointer);

            IndexPointer.BaseDataOffset = BaseDataOffset + indexPointerOffset;
            context.RegisterPointer(IndexPointer);
        }
    }
}
