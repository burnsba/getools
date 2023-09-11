using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public abstract class CommandParameterBase<T> : ICommandParameter, ICommandParameter<T>
    {
        public int Size { get; init; }
        public T Value { get; set; }
        public Type UnderlyingType { get; init; }

        public abstract byte[] GetBytes(Endianness endianness);
    }
}
