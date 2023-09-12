using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    public interface IOptionsInputFile
    {
        /// <summary>
        /// Gets or sets input file name.
        /// </summary>
        string InputFilename { get; set; }
    }
}
