using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;

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
        /// Size of the struct in bytes.
        /// </summary>
        public const int SizeOf = 8;

        /// <summary>
        /// Gets or sets the offset this entry was read from.
        /// </summary>
        public int Offset { get; set; }

        ///// <summary>
        ///// Gets or sets address of the <see cref="Entry"/> being pointed to.
        ///// Struct offset 0x0.
        ///// </summary>
        //public uint EntryPointer { get; set; }

        public PointerVariable EntryPointer { get; set; }

        /// <summary>
        /// List of ids pointed to from <see cref="EntryPointer"/>.
        /// </summary>
        public PathSet Entry { get; set; }

        /// <summary>
        /// TODO: Unknown.
        /// Struct offset 0x4.
        /// </summary>
        public uint Unknown_04 { get; set; }

        /// <inheritdoc />
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        public int BaseDataSize => SizeOf;

        /// <inheritdoc />
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
            sb.Append($"{Formatters.IntegralTypes.ToHex8(Unknown_04)}");

            sb.Append(" }");

            return sb.ToString();
        }

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
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;
            int pointerOffset = 0;

            // need to save offset in struct to set the pointer baseaddress
            pointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Unknown_04);
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            EntryPointer.BaseDataOffset = BaseDataOffset + pointerOffset;

            context.RegisterPointer(EntryPointer);
        }
    }
}
