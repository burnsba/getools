using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    public class PropAttribute
    {
        public PropertyInfo Property { get; set; }
        public GebugParameter Attribute { get; set; }
    }
}
