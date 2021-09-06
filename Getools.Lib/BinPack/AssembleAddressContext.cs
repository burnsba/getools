using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    public struct AssembleAddressContext
    {
        public int PriorCurrentAddress { get; private set; }
        public int DataStartAddress { get; private set; }
        public int FinalCurrentAdress { get; private set; }

        public AssembleAddressContext(int priorCurrentAddress, int dataStartAddress, int finalCurrentAdress)
        {
            PriorCurrentAddress = priorCurrentAddress;
            DataStartAddress = dataStartAddress;
            FinalCurrentAdress = finalCurrentAdress;
        }
    }
}
