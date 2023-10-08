using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// Describes an AI Command.
    /// </summary>
    public class AiFixedCommandDescription : IAiFixedCommandDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AiFixedCommandDescription"/> class.
        /// </summary>
        public AiFixedCommandDescription()
        {
            CommandParameters = new List<IAiParameter>();
        }

        /// <inheritdoc />
        public string? DecompName { get; set; }

        /// <inheritdoc />
        public byte CommandId { get; set; }

        /// <inheritdoc />
        public int CommandLengthBytes { get; set; }

        /// <inheritdoc />
        public int NumberParameters { get; set; }

        /// <inheritdoc />
        public List<IAiParameter> CommandParameters { get; set; }
    }
}
