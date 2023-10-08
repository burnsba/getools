using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// A command block is a collection (sequence) of commands to be run.
    /// </summary>
    public class AiCommandBlock
    {
        /// <summary>
        /// Type of the script.
        /// </summary>
        public AiScriptType ScriptType { get; set; } = AiScriptType.DefaultUnknown;

        /// <summary>
        /// Id or index of the script.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Commands in the script.
        /// </summary>
        public List<IAiConcreteCommand> Commands { get; set; } = new List<IAiConcreteCommand>();

        /// <summary>
        /// Converts script into newline separate list of commands.
        /// </summary>
        /// <param name="prefix">Whitespace indentation.</param>
        /// <returns>String.</returns>
        public string ToFriendlyLines(string prefix = "")
        {
            var sb = new StringBuilder();

            for (int i = 0; i < Commands.Count; i++)
            {
                var c = Commands[i];
                sb.Append(c.ToString());

                if (i < Commands.Count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Complete byte array of all commands in the script.
        /// </summary>
        /// <returns>Data.</returns>
        public byte[] ToBytes()
        {
            var results = new List<byte>();
            foreach (var c in Commands)
            {
                results.AddRange(c.ToByteArray());
            }

            while (results.Count % 4 != 0)
            {
                results.Add(0);
            }

            return results.ToArray();
        }

        /// <summary>
        /// Writes object as C macro definition.
        /// </summary>
        /// <param name="prefix">Whitespace indentation.</param>
        /// <returns>Macro text.</returns>
        public string ToCMacro(string prefix = "")
        {
            var sb = new StringBuilder();

            var byteCount = 0;

            for (int i = 0; i < Commands.Count; i++)
            {
                var c = Commands[i];

                byteCount += c.CommandLengthBytes;

                c.CMacroAppend(prefix, sb);

                //// the c macros are defined with trailing commas, so don't add one here.

                sb.AppendLine();
            }

            var rem = 4 - (byteCount % 4);
            if (rem == 4)
            {
                rem = 0;
            }

            if (rem > 0)
            {
                sb.Append(prefix);
                while (rem-- > 0)
                {
                    sb.Append(",0x00");
                }
            }

            return sb.ToString();
        }
    }
}
