using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiVariableCommand : IAiVariableCommand
    {
        public AiVariableCommand()
        {
        }

        public AiVariableCommand(IAiCommandDescription description)
        {
            DecompName = description.DecompName;
            CommandId = description.CommandId;
            CommandLengthBytes = description.CommandLengthBytes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="commandData">Includes leading commandid byte.</param>
        public AiVariableCommand(IAiCommandDescription description, byte[] commandData)
        {
            DecompName = description.DecompName;
            CommandId = description.CommandId;

            CommandLengthBytes = commandData.Length + 1;
            CommandData = commandData;
        }

        public string DecompName { get; set; }

        public byte CommandId { get; set; }

        /// <summary>
        /// Gets or sets length of all byte data, including leading commandid byte.
        /// </summary>
        public int CommandLengthBytes { get; set; }

        public byte[] CommandData { get; set; }

        public void CMacroAppend(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + $"// 0x{CommandId:x2}");
            sb.Append(prefix + $"// {DecompName}");

            var totalBytes = ToByteArray();

            // try to log as ASCII. Skip commandid and final '\0'.
            if (totalBytes.Length > 2)
            {
                var maybe = totalBytes.Skip(1).Take(totalBytes.Length - 2);
                var chars = new List<Byte>();
                foreach (var c in maybe)
                {
                    if (c >= 0x20 && c <= 0x7e)
                    {
                        chars.Add(c);
                    }
                }

                if (chars.Count > 0)
                {
                    sb.Append(": ");
                    sb.Append(System.Text.Encoding.ASCII.GetString(chars.ToArray()));
                }
            }

            sb.AppendLine();
            sb.Append(prefix);

            var joined = string.Join(", ", totalBytes.Select(x => "0x" + x.ToString("x2")));

            sb.Append(joined);

            // The macros will automatically append trailing comma, but this is echoing the raw bytes,
            // so manually add a comma.
            sb.Append(",");
        }

        public void CMacroAppendInt(string prefix, StringBuilder sb)
        {
            sb.AppendLine(prefix + $"// 0x{CommandId:x2}");
            sb.AppendLine(prefix + $"// {DecompName}");

            var totalBytes = ToByteArray();
            var words = (totalBytes.Length - 1) >> 2;
            var rem = totalBytes.Length % 4;
            if (rem == 0)
            {
                rem = 4;
            }

            int pos = 0;

            sb.Append(prefix);

            for (int i = 0; i < words; i++)
            {
                sb.Append("0x");
                sb.Append(totalBytes[pos++].ToString("x2"));
                sb.Append(totalBytes[pos++].ToString("x2"));
                sb.Append(totalBytes[pos++].ToString("x2"));
                sb.Append(totalBytes[pos++].ToString("x2"));

                if (i + 1 < words || rem > 0)
                {
                    sb.Append(", ");
                }
            }

            if (rem > 0)
            {
                sb.Append("0x");
                while (rem-- > 0)
                {
                    sb.Append(totalBytes[pos++].ToString("x2"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="endien">Ignored.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] ToByteArray(ByteOrder endien = ByteOrder.BigEndien)
        {
            if (CommandLengthBytes < 1)
            {
                throw new InvalidOperationException();
            }

            var results = new byte[CommandLengthBytes];
            results[0] = CommandId;
            Array.Copy(CommandData, 0, results, 1, CommandData.Length);
            return results;
        }
    }
}
