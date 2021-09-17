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

        public PointerVariable PathTableSectionPointer { get; set; }
        public PointerVariable PathLinkSectionPointer { get; set; }
        public PointerVariable IntroSectionPointer { get; set; }
        public PointerVariable ObjectSectionPointer { get; set; }
        public PointerVariable PathSetSectionPointer { get; set; }
        public PointerVariable AiListSectionPointer { get; set; }
        public PointerVariable PadSectionPointer { get; set; }
        public PointerVariable Pad3dSectionPointer { get; set; }
        public PointerVariable PadNamesSectionPointer { get; set; }
        public PointerVariable Pad3dNamesSectionPointer { get; set; }

        [JsonIgnore]
        public int ByteAlignment => Config.TargetPointerAlignment;

        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        [JsonIgnore]
        public int BaseDataSize => 10 * Config.TargetPointerSize;

        [JsonIgnore]
        public Guid MetaId => _metaId;

        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

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
