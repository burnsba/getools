using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Object to capture state of the file being assembled when
    /// adding a data block. This details address information.
    /// </summary>
    public struct AssembleAddressContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssembleAddressContext"/> struct.
        /// </summary>
        /// <param name="priorCurrentAddress"><see cref="PriorCurrentAddress"/>.</param>
        /// <param name="dataStartAddress"><see cref="DataStartAddress"/>.</param>
        /// <param name="finalCurrentAdress"><see cref="FinalCurrentAdress"/>.</param>
        public AssembleAddressContext(int priorCurrentAddress, int dataStartAddress, int finalCurrentAdress)
        {
            PriorCurrentAddress = priorCurrentAddress;
            DataStartAddress = dataStartAddress;
            FinalCurrentAdress = finalCurrentAdress;
        }

        /// <summary>
        /// Gets the address of the file being assembled, before the function call.
        /// This may not be word aligned.
        /// </summary>
        public int PriorCurrentAddress { get; private set; }

        /// <summary>
        /// Gets the address of the start of data added in the file being assembled.
        /// This will have been adjusted for any alignment.
        /// </summary>
        public int DataStartAddress { get; private set; }

        /// <summary>
        /// Gets the current address of the file being assembled after adding the data.
        /// This may not be word aligned.
        /// </summary>
        public int FinalCurrentAdress { get; private set; }
    }
}
