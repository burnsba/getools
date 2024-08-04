using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game.Enums;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.SetupObject
{
    /// <summary>
    /// Base class for setup object definitions.
    /// </summary>
    public abstract class SetupObjectBase : GameObjectHeaderBase, ISetupObject
    {
        /// <summary>
        /// C file, type name. Should match known struct type.
        /// </summary>
        public const string CTypeName = "s32";

        /// <summary>
        /// Number of bytes for object.
        /// </summary>
        public const int BaseSizeOf = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectBase"/> class.
        /// </summary>
        /// <param name="type">Type of object.</param>
        public SetupObjectBase(PropDef type)
        {
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupObjectBase"/> class.
        /// Should only be used for serialization purposes.
        /// </summary>
        internal SetupObjectBase()
        {
        }

        /// <inheritdoc/>
        public PropDef Type
        {
            get
            {
                return (PropDef)TypeRaw;
            }

            set
            {
                TypeRaw = (byte)value;
            }
        }

        /// <inheritdoc/>
        public int SetupIndex { get; set; }

        /// <inheritdoc/>
        public int SetupSectionTypeIndex { get; set; }

        /// <summary>
        /// Gets Getools.Lib reference id for the section/filler section.
        /// </summary>
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <inheritdoc />
        [JsonIgnore]
        public virtual int ByteAlignment => Config.TargetWordSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public virtual int BaseDataSize { get; set; }

        /// <inheritdoc />
        public virtual void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public virtual void Assemble(IAssembleContext context)
        {
            var bytes = new byte[GameObjectHeaderBase.SizeOf];

            var headerBytes = ((GameObjectHeaderBase)this).ToByteArray();
            Array.Copy(headerBytes, bytes, headerBytes.Length);

            var result = context.AssembleAppendBytes(bytes, Config.TargetWordSize);
            BaseDataOffset = result.DataStartAddress;
        }
    }
}
