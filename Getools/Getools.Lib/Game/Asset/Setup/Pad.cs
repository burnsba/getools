using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Pad. Each room in the game is composed of one or more pads.
    /// </summary>
    /// <remarks>
    /// TODO: see LookupPadLink() on <see cref="StandTile"/>.
    /// </remarks>
    public class Pad : IBinData, IGetoolsLibObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "struct pad";

        /// <summary>
        /// Size of the point struct in bytes.
        /// </summary>
        public const int SizeOf = 44;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pad"/> class.
        /// </summary>
        public Pad()
        {
        }

        /// <summary>
        /// Gets or sets position coordinate.
        /// Struct offset 0x0.
        /// </summary>
        public Coord3df? Position { get; set; }

        /// <summary>
        /// Gets or sets "up" coordinate.
        /// Struct offset 0xc.
        /// </summary>
        public Coord3df? Up { get; set; }

        /// <summary>
        /// Gets or sets "look" coordinate.
        /// Struct offset 0x18.
        /// </summary>
        public Coord3df? Look { get; set; }

        /// <summary>
        /// Gets or sets name string/pointer.
        /// Struct offset 0x24.
        /// </summary>
        public ClaimedStringPointer? Name { get; set; }

        /// <summary>
        /// TODO: Unknown fields.
        /// Struct offset 0x28.
        /// Seems to always be zero, to indicate end of pad in setup.
        /// </summary>
        public int Unknown { get; set; }

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
        public virtual void Collect(IAssembleContext context)
        {
            if (object.ReferenceEquals(null, Position))
            {
                throw new NullReferenceException($"{nameof(Position)}");
            }

            if (object.ReferenceEquals(null, Up))
            {
                throw new NullReferenceException($"{nameof(Up)}");
            }

            if (object.ReferenceEquals(null, Look))
            {
                throw new NullReferenceException($"{nameof(Look)}");
            }

            context.AppendToDataSection(Position);
            context.AppendToDataSection(Up);
            context.AppendToDataSection(Look);
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public virtual void Assemble(IAssembleContext context)
        {
            if (object.ReferenceEquals(null, Name))
            {
                throw new NullReferenceException($"{nameof(Name)}");
            }

            // the coord3d are handled in their own class, only need the native properties here.
            var size = Config.TargetWordSize * 2;
            var bytes = new byte[size];
            int pos = 0;
            int pointerOffset = 0;

            // need to save offset in struct to set the pointer baseaddress
            pointerOffset = pos;

            // pointer value will be resolved when linking
            BitUtility.Insert32Big(bytes, pos, 0);
            pos += Config.TargetPointerSize;

            BitUtility.Insert32Big(bytes, pos, (int)Unknown);
            pos += Config.TargetWordSize;

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;

            Name.BaseDataOffset = result.DataStartAddress;
            context.RegisterPointer(Name);
        }

        /// <summary>
        /// Common implementation, returns comma seperated string of struct contents.
        /// Does not include brackets or semi-colon.
        /// </summary>
        /// <param name="sb">String builder to append to.</param>
        /// <param name="prefix">Optional prefix to prepend.</param>
        protected virtual void ToCDeclarationCommon(StringBuilder sb, string prefix = "")
        {
            if (object.ReferenceEquals(null, Position))
            {
                throw new NullReferenceException($"{nameof(Position)}");
            }

            if (object.ReferenceEquals(null, Up))
            {
                throw new NullReferenceException($"{nameof(Up)}");
            }

            if (object.ReferenceEquals(null, Look))
            {
                throw new NullReferenceException($"{nameof(Look)}");
            }

            if (object.ReferenceEquals(null, Name))
            {
                throw new NullReferenceException($"{nameof(Name)}");
            }

            sb.Append(Position.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Up.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Look.ToCInlineDeclaration(string.Empty));
            sb.Append(", ");
            sb.Append(Formatters.Strings.ToCValueOrNullEmpty(Name.GetString()));
            sb.Append(", ");
            sb.Append(Unknown);
        }
    }
}
