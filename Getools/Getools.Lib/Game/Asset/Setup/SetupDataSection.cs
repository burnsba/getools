using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Base class for setup data section.
    /// </summary>
    public abstract class SetupDataSection : ISetupSection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupDataSection"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        /// <param name="variableName">Variable name.</param>
        public SetupDataSection(SetupSectionId typeId, string variableName)
        {
            VariableName = variableName;
            TypeId = typeId;

            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupDataSection"/> class.
        /// </summary>
        /// <param name="typeId">Section type.</param>
        protected SetupDataSection(SetupSectionId typeId)
        {
            TypeId = typeId;

            VariableName = "noname";

            Init();
        }

        /// <inheritdoc />
        public SetupSectionId TypeId { get; private set; }

        /// <inheritdoc />
        public string VariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsMainSection { get; private set; }

        /// <inheritdoc />
        public bool IsUnreferenced { get; set; } = false;

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
        public abstract int BaseDataSize { get; set; }

        /// <inheritdoc />
        public abstract string GetDeclarationTypeName();

        /// <inheritdoc />
        public abstract void WritePrequelData(StreamWriter sw);

        /// <inheritdoc />
        public abstract void WriteSectionData(StreamWriter sw);

        /// <inheritdoc />
        public abstract void DeserializeFix(int startingIndex = 0);

        /// <inheritdoc />
        public abstract int GetEntriesCount();

        /// <inheritdoc />
        public abstract int GetPrequelDataSize();

        /// <inheritdoc />
        public abstract void Collect(IAssembleContext context);

        /// <inheritdoc />
        public abstract void Assemble(IAssembleContext context);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return MetaId.GetHashCode();
        }

        private void Init()
        {
            switch (TypeId)
            {
                case SetupSectionId.SectionPathTable:
                case SetupSectionId.SectionPathLink:
                case SetupSectionId.SectionIntro:
                case SetupSectionId.SectionObjects:
                case SetupSectionId.SectionPathSets:
                case SetupSectionId.SectionAiList:
                case SetupSectionId.SectionPadList:
                case SetupSectionId.SectionPad3dList:
                case SetupSectionId.SectionPadNames:
                case SetupSectionId.SectionPad3dNames:
                    IsMainSection = true;
                    break;

                default:
                    IsMainSection = false;
                    break;
            }
        }
    }
}
