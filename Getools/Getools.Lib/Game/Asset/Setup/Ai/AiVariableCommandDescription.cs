using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Variable length AI Command.
    /// The length of a "variable" command depends on the parameters used.
    /// </summary>
    public class AiVariableCommandDescription : IAiVariableCommandDescription
    {
        /// <inheritdoc />
        public string? DecompName { get; set; }

        /// <inheritdoc />
        public byte CommandId { get; set; }

        /// <inheritdoc />
        public int CommandLengthBytes { get; set; }

        /// <summary>
        /// Variable length command data.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public byte[] CommandData { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
