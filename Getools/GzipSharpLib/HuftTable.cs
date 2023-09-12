using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GzipSharpLib
{
    public class HuftTable
    {
        /// <summary>
        /// Id used for trace/debug.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// The c version uses an empty Huft structure as the base of a table,
        /// and then skips that for operations. Since <see cref="HuftTable"/>
        /// is split to a different class in C#, this <see cref="Root"/>
        /// property corresponds to the base c entry.
        /// </summary>
        public Huft? Root { get; set; } = null;

        /// <summary>
        /// List of entries in the table.
        /// </summary>
        public List<Huft> HuftList { get; set; } = new List<Huft>();

        public HuftTable()
        {
            // Don't instantiate a new Huft here, this will create stack overflow.
        }

        /// <summary>
        /// Trace log output, in "plain" c format.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Matches custom logging added to inflate.c, taking null to 0.
        /// </remarks>
        internal string ToTraceJson(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            sb.Append("{ ");
            sb.Append($" 'id': {Id},");
            sb.Append($" 'e': 0,");
            sb.Append($" 'e': 0,");
            sb.Append($" 'e': 0,");
            sb.Append($" 'v_t_id': 0,");
            sb.Append($" 'parent_id': 0, ");

            sb.Append($" 'table': [");

            if (HuftList.Any())
            {
                var last = HuftList.Last();
                sb.AppendLine();
                sb.Append(prefix);
                foreach (var h in HuftList)
                {
                    sb.Append(h.ToTraceJson(prefix + "    "));

                    if (!object.ReferenceEquals(last, h))
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                    sb.Append(prefix);
                }
            }

            sb.Append($"]");
            sb.Append(" }");

            return sb.ToString();
        }
    }
}
