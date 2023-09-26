using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    public class GebugParameter : Attribute
    {
        public int ParameterIndex { get; set; }
        public int Size { get; set; } = 1;
        public bool IsVariableSize { get; set; } = false;
        public ParameterUseDirection UseDirection { get; set; } = ParameterUseDirection.Never;
    }
}
