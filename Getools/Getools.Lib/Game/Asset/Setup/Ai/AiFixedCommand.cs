using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiFixedCommand : IAiFixedCommand
    {
        public AiFixedCommand()
        {
        }

        public AiFixedCommand(IAiCommandDescription description)
        {
            DecompName = description.DecompName;
            CommandId = description.CommandId;
            CommandLengthBytes = description.CommandLengthBytes;
        }

        public AiFixedCommand(IAiCommandDescription description, List<IAiParameter> parameters)
            : this(description)
        {
            NumberParameters = parameters.Count;
            CommandParameters = parameters;
        }

        public string DecompName { get; set; }

        public byte CommandId { get; set; }

        public int CommandLengthBytes { get; set; }

        public int NumberParameters { get; set; }

        public List<IAiParameter> CommandParameters { get; set; }

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
