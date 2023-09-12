using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Helper class used by <see cref="StageSetupFile"/> during compiling
    /// into .bin file.
    /// </summary>
    internal class StageSetupFileHeader : IBinData
    {
        private Guid _metaId = Guid.NewGuid();

        /// <summary>
        /// Initializes a new instance of the <see cref="StageSetupFileHeader"/> class.
        /// </summary>
        /// <param name="pathTableSection">Path table section or null.</param>
        /// <param name="pathLinkSection">Path link/list section or null.</param>
        /// <param name="introSection">Intro section or null.</param>
        /// <param name="objectSection">Object section or null.</param>
        /// <param name="pathSetSection">Path set section or null.</param>
        /// <param name="aiListSection">AI script section or null.</param>
        /// <param name="padSection">Pad section or null.</param>
        /// <param name="pad3dSection">Pad3d section or null.</param>
        /// <param name="padNamesSection">Pad names section or null.</param>
        /// <param name="pad3dNamesSection">Pad3d names section or null.</param>
        public StageSetupFileHeader(
            IGetoolsLibObject pathTableSection,
            IGetoolsLibObject pathLinkSection,
            IGetoolsLibObject introSection,
            IGetoolsLibObject objectSection,
            IGetoolsLibObject pathSetSection,
            IGetoolsLibObject aiListSection,
            IGetoolsLibObject padSection,
            IGetoolsLibObject pad3dSection,
            IGetoolsLibObject padNamesSection,
            IGetoolsLibObject pad3dNamesSection)
        {
            PathTableSectionPointer = new PointerVariable(pathTableSection);
            PathLinkSectionPointer = new PointerVariable(pathLinkSection);
            IntroSectionPointer = new PointerVariable(introSection);
            ObjectSectionPointer = new PointerVariable(objectSection);
            PathSetSectionPointer = new PointerVariable(pathSetSection);
            AiListSectionPointer = new PointerVariable(aiListSection);
            PadSectionPointer = new PointerVariable(padSection);
            Pad3dSectionPointer = new PointerVariable(pad3dSection);
            PadNamesSectionPointer = new PointerVariable(padNamesSection);
            Pad3dNamesSectionPointer = new PointerVariable(pad3dNamesSection);
        }

        /// <summary>
        /// Gets or sets pointer to path table section.
        /// </summary>
        public PointerVariable PathTableSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to path link/list section.
        /// </summary>
        public PointerVariable PathLinkSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to intro section.
        /// </summary>
        public PointerVariable IntroSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to object section.
        /// </summary>
        public PointerVariable ObjectSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to path set section.
        /// </summary>
        public PointerVariable PathSetSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to AI script section.
        /// </summary>
        public PointerVariable AiListSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to pad section.
        /// </summary>
        public PointerVariable PadSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to pad3d section.
        /// </summary>
        public PointerVariable Pad3dSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to pad names section.
        /// </summary>
        public PointerVariable PadNamesSectionPointer { get; set; }

        /// <summary>
        /// Gets or sets pointer to pad3d names section.
        /// </summary>
        public PointerVariable Pad3dNamesSectionPointer { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetPointerAlignment;

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int BaseDataSize => 10 * Config.TargetPointerSize;

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId => _metaId;

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            var bytes = new byte[BaseDataSize];

            var result = context.AssembleAppendBytes(bytes, ByteAlignment);

            int filePosition = result.DataStartAddress;

            PathTableSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(PathTableSectionPointer);
            filePosition += Config.TargetPointerSize;

            PathLinkSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(PathLinkSectionPointer);
            filePosition += Config.TargetPointerSize;

            IntroSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(IntroSectionPointer);
            filePosition += Config.TargetPointerSize;

            ObjectSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(ObjectSectionPointer);
            filePosition += Config.TargetPointerSize;

            PathSetSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(PathSetSectionPointer);
            filePosition += Config.TargetPointerSize;

            AiListSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(AiListSectionPointer);
            filePosition += Config.TargetPointerSize;

            PadSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(PadSectionPointer);
            filePosition += Config.TargetPointerSize;

            Pad3dSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(Pad3dSectionPointer);
            filePosition += Config.TargetPointerSize;

            PadNamesSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(PadNamesSectionPointer);
            filePosition += Config.TargetPointerSize;

            Pad3dNamesSectionPointer.BaseDataOffset = filePosition;
            context.RegisterPointer(Pad3dNamesSectionPointer);
            filePosition += Config.TargetPointerSize;
        }
    }
}
