using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Parameter
{
    /// <summary>
    /// Meta data to desribe parameter in code.
    /// </summary>
    public class GebugParameter : Attribute
    {
        /// <summary>
        /// Required.
        /// Zero based index of how parameter occurs in message.
        /// </summary>
        public int ParameterIndex { get; set; }

        /// <summary>
        /// When <see cref="IsVariableSize"/> is false, the number
        /// of bytes the parameter uses. Otherwise ignored.
        /// </summary>
        public int Size { get; set; } = 1;

        /// <summary>
        /// Whether or not this is a variable length parameter.
        /// </summary>
        public bool IsVariableSize { get; set; } = false;

        /// <summary>
        /// Parameter is a list of variables, and all of variable length.
        /// </summary>
        public bool IsVariableSizeList { get; set; } = false;

        /// <summary>
        /// Direction parameter should be used in.
        /// </summary>
        public ParameterUseDirection UseDirection { get; set; } = ParameterUseDirection.Never;
    }
}
