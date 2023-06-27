using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Options
{
    public interface IOptionsBase
    {
        /// <summary>
        /// Capture any remaining command line arguments here.
        /// </summary>
        IEnumerable<string> TypoCatch { get; set; }
    }
}
