using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Collections.Internals
{
    internal enum PendingEventType
    {
        Add,
        AddRange,
        Insert,
        InsertRange,
        Remove,
        RemoveAt,
        Replace,
        Clear,
        Reset,
    }
}
