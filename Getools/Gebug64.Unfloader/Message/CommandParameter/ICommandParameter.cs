using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public interface ICommandParameter
    {
        int Size { get; }
        byte[] GetBytes(ByteOrder endienness);
        Type UnderlyingType { get; }

        bool TryGetValue<TValue>(out TValue value);
        int GetValueIntOrDefault();
    }
}
