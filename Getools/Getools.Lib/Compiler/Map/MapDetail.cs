using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Compiler.Map
{
    /// <summary>
    /// Information about an entry in compiler map file.
    /// </summary>
    public class MapDetail
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapDetail"/> class.
        /// </summary>
        public MapDetail()
        {
        }

        /// <summary>
        /// Segment/section name of item.
        /// </summary>
        public string SegmentName { get; set; } = string.Empty;

        /// <summary>
        /// Memory address of item.
        /// </summary>
        public UInt32 Address { get; set; }

        /// <summary>
        /// Variable name of item.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Filename item belongs to, without extension or directory.
        /// </summary>
        public string FileSource { get; set; } = string.Empty;

        /// <summary>
        /// Conver to string with more details.
        /// </summary>
        /// <returns>String.</returns>
        public string ToFullDetailString()
        {
            string hexAddress = Getools.Lib.Formatters.IntegralTypes.ToHex8(Address);

            return $".{SegmentName} {Name} {hexAddress} ({FileSource}.o)";
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string hexAddress = Getools.Lib.Formatters.IntegralTypes.ToHex8(Address);

            return $"{Name} {hexAddress}";
        }
    }
}
