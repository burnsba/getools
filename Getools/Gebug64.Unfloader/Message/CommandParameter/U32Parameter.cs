﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public class U32Parameter : CommandParameterBase<UInt32>
    {
        public U32Parameter()
        {
            Size = 4;
            UnderlyingType = typeof(UInt32);
        }

        public U32Parameter(UInt32 value)
            : this()
        {
            Value = value;
        }

        public U32Parameter(Int32 value)
            : this()
        {
            Value = (UInt32)value;
        }

        public override byte[] GetBytes(Endianness endianness)
        {
            var result = new byte[Size];

            if (endianness == Endianness.LittleEndian)
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
            return "0x" + Value.ToString("X8");
        }
    }
}