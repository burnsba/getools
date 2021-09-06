using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    public interface IAssembleContext
    {
        void AppendToDataSection(IBinData data);
        void AppendToRodataSection(IBinData data);

        AssembleAddressContext AssembleAppendBytes(byte[] bytes, int align);
        void RegisterPointer(PointerVariable pointer);
        void RemovePointer(PointerVariable pointer);

        int GetCurrentAddress();
    }
}
