using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Config
{
    /// <summary>
    /// Configuration settings related to the memory window.
    /// </summary>
    public class MemorySettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemorySettings"/> class.
        /// </summary>
        public MemorySettings()
        {
        }

        /// <summary>
        /// Path to build output file giving memory locations of every ELF component.
        /// </summary>
        public string? MapBuildFile { get; set; }
    }
}
