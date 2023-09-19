﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib;
using Getools.Lib.Architecture;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public class U8Parameter : CommandParameterBase<byte>
    {
        public U8Parameter()
        {
            Size = 1;
            UnderlyingType = typeof(byte);
        }

        public U8Parameter(UInt32 value)
            : this()
        {
            Value = (byte)(value & 0xff);
        }

        public U8Parameter(Int32 value)
            : this()
        {
            Value = (byte)(value & 0xff);
        }

        public override byte[] GetBytes(ByteOrder endienness)
        {
            var result = new byte[Size];

            result[0] = Value;

            return result;
        }

        public override string ToString()
        {
            return "0x" + Value.ToString("X2");
        }
    }
}
