using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    public interface IOptionsOutputFile
    {
        /// <summary>
        /// Gets or sets output file name.
        /// </summary>
        string OutputFilename { get; set; }
    }
}
