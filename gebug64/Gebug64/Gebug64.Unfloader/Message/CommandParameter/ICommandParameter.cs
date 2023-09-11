using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public interface ICommandParameter
    {
        int Size { get; }
        byte[] GetBytes(Endianness endianness);
        Type UnderlyingType { get; }
    }
}
