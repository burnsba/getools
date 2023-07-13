using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Math
{
    /// <summary>
    /// Functions to convert one value to another.
    /// </summary>
    public static class Convert
    {
        /// <summary>
        /// Convert character literal to int.
        /// </summary>
        /// <param name="c">Character. Assumes max value of 'z'.</param>
        /// <returns>Int.</returns>
        public static int Base26ToInt(char c)
        {
            return (int)(c.ToString().ToLower()[0] - 'a');
        }
    }
}
