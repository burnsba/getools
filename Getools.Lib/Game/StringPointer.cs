using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game
{
    public class StringPointer
    {
        public StringPointer()
        {
        }

        public StringPointer(string value)
        {
            Value = value;
        }

        public StringPointer(int? offset, string value)
        {
            Offset = offset;
            Value = value;
        }

        public int? Offset { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
