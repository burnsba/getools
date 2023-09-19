﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib;
using Getools.Lib.Architecture;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public class S32Parameter : CommandParameterBase<Int32>
    {
        public S32Parameter()
        {
            Size = 4;
            UnderlyingType = typeof(Int32);
        }

        public S32Parameter(UInt32 value)
            : this()
        {
            Value = (Int32)value;
        }

        public S32Parameter(Int32 value)
            : this()
        {
            Value = value;
        }

        public override byte[] GetBytes(ByteOrder endienness)
        {
            var result = new byte[Size];

            if (endienness == ByteOrder.LittleEndien)
            {
                BitUtility.Insert32Little(result, 0, Value);
            }
            else
            {
                BitUtility.Insert32Big(result, 0, Value);
            }

            return result;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
