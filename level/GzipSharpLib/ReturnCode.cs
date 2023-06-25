using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipSharpLib
{
    /// <remarks>
    /// gzip.h
    /// </remarks>
    public enum ReturnCode
    {
        Ok = 0,
        Error = 1,
        Warning = 2,

        AlreadyRan = 99,
    }
}
