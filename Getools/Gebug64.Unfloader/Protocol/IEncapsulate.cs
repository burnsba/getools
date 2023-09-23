using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol
{
    public interface IEncapsulate
    {
        Type? InnerType { get; }
        object? InnerData { get; }
    }
}
