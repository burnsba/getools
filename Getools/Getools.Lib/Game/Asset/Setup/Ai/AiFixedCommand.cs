using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// An AI Command with a fixed size.
    /// </summary>
    public class AiFixedCommand : IAiFixedCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AiFixedCommand"/> class.
        /// </summary>
        public AiFixedCommand()
        {
            CommandParameters = new List<IAiParameter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiFixedCommand"/> class.
        /// </summary>
        /// <param name="description">Base command.</param>
        public AiFixedCommand(IAiCommandDescription description)
        {
            if (object.ReferenceEquals(null, description))
            {
                throw new NullReferenceException();
            }

            if (string.IsNullOrEmpty(description.DecompName))
            {
                throw new NullReferenceException();
            }

            DecompName = description.DecompName;
            CommandId = description.CommandId;
            CommandLengthBytes = description.CommandLengthBytes;

            CommandParameters = new List<IAiParameter>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AiFixedCommand"/> class.
        /// </summary>
        /// <param name="description">Base command.</param>
        /// <param name="parameters">Parameter values.</param>
        public AiFixedCommand(IAiCommandDescription description, List<IAiParameter> parameters)
            : this(description)
        {
            NumberParameters = parameters.Count;
            CommandParameters = parameters;
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

        /// <inheritdoc />
        public byte[] ToByteArray(ByteOrder endien = ByteOrder.BigEndien)
        {
            var results = new byte[CommandLengthBytes];
            results[0] = CommandId;
            int resultPosition = 1;
            for (var i = 0; i < NumberParameters; i++)
            {
                AppendParameter(results, resultPosition, CommandParameters[i], endien);
                resultPosition += CommandParameters[i].ByteLength;
            }

            return results;
        }

        /// <inheritdoc />
        public void CMacroAppend(string prefix, StringBuilder sb)
        {
            var indent = "    ";

            sb.AppendLine(prefix + $"// 0x{CommandId:x2}");
            sb.Append(prefix + DecompName);

            string argsText = string.Empty;
            string argsCommentText = string.Empty;
            if (NumberParameters > 0)
            {
                sb.AppendLine("(");
                argsText = string.Join(", ", CommandParameters.Select(x => x.ValueToString(expandSpecial: false)));
                argsCommentText = string.Join(", ", CommandParameters.Select(x => x.ParameterName));
                sb.AppendLine(prefix + indent + "// " + argsCommentText);
                sb.Append(prefix + indent + argsText + ")");
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (NumberParameters > 0)
            {
                var parameterText = string.Join(", ", CommandParameters.Select(x => x.ToString()));
                return $"0x{CommandId:x2}: {DecompName} {{ {parameterText} }}";
            }
            else
            {
                return $"0x{CommandId:x2}: {DecompName}";
            }
        }

        private void AppendParameter(byte[] arr, int position, IAiParameter parameter, ByteOrder endien = ByteOrder.BigEndien)
        {
            var bytes = parameter.ToByteArray(endien);

            Array.Copy(bytes, 0, arr, position, bytes.Length);
        }
    }
}
