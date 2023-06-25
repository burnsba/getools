using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.Packet
{
    public interface IExecuteFunctionArg
    {
        int Size { get; set; }

        int Value { get; set; }
    }
}
