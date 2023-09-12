using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipSharpLib
{
    /// <summary>
    /// Return result for <see cref="Inflate.HuftBuild(uint[], uint, uint, ushort[]?, ushort[]?, ref HuftTable, ref int)"/>.
    /// </summary>
    internal enum HuftBuildResult
    {
        Success,
        Incomplete,
        Error,
    }
}
