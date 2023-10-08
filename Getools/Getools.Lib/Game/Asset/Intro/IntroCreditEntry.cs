using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Credits screen entry.
    /// </summary>
    public class IntroCreditEntry : IBinData, IGetoolsLibObject
    {
        /*
         * Struct layout:
         *     id1
         *     id2
         *     position1
         *     alignment1
         *     position2
         *     alignemtn2
         */

        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "CreditsEntry";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntroCreditEntry"/> class.
        /// </summary>
        public IntroCreditEntry()
        {
        }

        /// <summary>
        /// Overall game text id, for text 1.
        /// </summary>
        public ushort TextId1 { get; set; }

        /// <summary>
        /// Overall game text id, for text 2.
        /// </summary>
        public ushort TextId2 { get; set; }

        /// <summary>
        /// Text 1 position.
        /// TODO: Unknown (vertical? horizontal?)
        /// </summary>
        public short Position1 { get; set; }

        /// <summary>
        /// Text 1 alignment.
        /// </summary>
        public CreditTextAlignment Alignment1 { get; set; }

        /// <summary>
        /// Text 1 position.
        /// TODO: Unknown (vertical? horizontal?)
        /// </summary>
        public short Position2 { get; set; }

        /// <summary>
        /// Text 2 alignment.
        /// </summary>
        public CreditTextAlignment Alignment2 { get; set; }

        /// <summary>
        /// Gets or sets the variable name used in source file.
        /// </summary>
        public string? VariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public virtual int BaseDataSize => SizeOf;

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

            sb.AppendLine($"{prefix}{CTypeName} {VariableName} = {{");

            ToCDeclarationCommon(sb, prefix);

            sb.AppendLine($"{prefix}}};");

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

            ToCDeclarationCommon(sb, prefix);

            sb.Append(" }");

            return sb.ToString();
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var size = SizeOf;
            var bytes = new byte[size];
            int pos = 0;

            BitUtility.InsertShortBig(bytes, pos, TextId1);
            pos += Config.TargetPointerSize;

            BitUtility.InsertShortBig(bytes, pos, TextId2);
            pos += Config.TargetPointerSize;

            BitUtility.InsertShortBig(bytes, pos, Position1);
            pos += Config.TargetPointerSize;

            BitUtility.InsertShortBig(bytes, pos, (short)Alignment1);
            pos += Config.TargetPointerSize;

            BitUtility.InsertShortBig(bytes, pos, Position2);
            pos += Config.TargetPointerSize;

            BitUtility.InsertShortBig(bytes, pos, (short)Alignment2);
            pos += Config.TargetPointerSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }

        /// <summary>
        /// Common implementation, returns comma seperated string of struct contents.
        /// Does not include brackets or semi-colon.
        /// </summary>
        /// <param name="sb">String builder to append to.</param>
        /// <param name="prefix">Optional prefix to prepend.</param>
        protected virtual void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            sb.Append(Formatters.IntegralTypes.ToHex4(TextId1));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex4(TextId2));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex4(Position1));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex4((short)Alignment1));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex4(Position2));
            sb.Append(", ");
            sb.Append(Formatters.IntegralTypes.ToHex4((short)Alignment2));
        }
    }
}
