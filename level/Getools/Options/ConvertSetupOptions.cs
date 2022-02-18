using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace Getools.Options
{
    /// <summary>
    /// Verb to convert setup file from one format to another.
    /// </summary>
    [Verb("convert_setup", HelpText = "Convert setup file/object from one format to another.")]
    public class ConvertSetupOptions : ConvertOptionsBase
    {
    }
}
