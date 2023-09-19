using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public abstract class CommandParameterBase<T> : ICommandParameter, ICommandParameter<T>
    {
        public int Size { get; init; }
        public T Value { get; set; }
        public Type UnderlyingType { get; init; }

        public abstract byte[] GetBytes(ByteOrder endienness);

        public bool TryGetValue<TValue>(out TValue value)
        {
            if (typeof(TValue) == typeof(T))
            {
                value = (TValue)(object)(T)Value;
                return true;
            }

            var converter = TypeDescriptor.GetConverter(typeof(TValue));
            if (converter.CanConvertFrom(typeof(T)))
            {
                value = (TValue)converter.ConvertFrom((T)Value);
                return true;
            }

            value = default(TValue);
            return false;
        }

        public int GetValueIntOrDefault()
        {
            int i;
            if (TryGetValue<int> (out i))
            {
                return i;
            }

            return 0;
        }
    }
}
