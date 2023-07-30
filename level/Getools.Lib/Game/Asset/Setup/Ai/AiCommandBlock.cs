using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public class AiCommandBlock
    {
        public AiScriptType ScriptType { get; set; } = AiScriptType.DefaultUnknown;

        public int Id { get; set; }

        public List<IAiConcreteCommand> Commands { get; set; } = new List<IAiConcreteCommand>();

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
