using System.ComponentModel.Design;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    internal class Program
    {

        private enum ParseState
        {
            Initial,
            StartLine,
            Name,
            CommandId,
            InfoStart,
            InfoLines,
            NoteStart,
            NoteLines,
            DefineId,
            DefineLength,
            DefineCommand,
            DefineParameter,
        }

        static void Main(string[] args)
        {
            var reName = new Regex(@"^//\s*name:\s*(.*)$");
            var reCommandId = new Regex(@"^//\s*command id:\s*(.*)$");
            var reInfo = new Regex(@"^//\s*info:\s*(.*)$");
            var reArguments = new Regex(@"^//\s*arguments:\s*(.*)$");
            var reNote = new Regex(@"^//\s*note:\s*(.*)$");
            var reComment = new Regex(@"^//\s*([^=][^=].*)$");
            var reDefine = new Regex(@"^#define(.*)$");
            var reParen = new Regex(@"\(([^)]+)\)");
            // ai_list_end_ID is the only command that doesn't end in a comma
            // objective_bitfield_set_on
            var reCommand = new Regex(@"^\s+([^,]+),?.*$");
            var reCommand16 = new Regex(@"^\s+cha?rarray16\(([^,]+)\),?.*$");
            var reCommand24 = new Regex(@"^\s+cha?rarray24\(([^,]+)\),?.*$");
            var reCommand32 = new Regex(@"^\s+cha?rarray32\(([^,]+)\),?.*$");
            var reEmpty = new Regex(@"^\s*$");

            var inputFilePath = "C:\\Users\\bburns\\code\\scratch\\aajk\\ai.h";

            var lines = File.ReadAllLines(inputFilePath);

            ParseState state = ParseState.Initial;
            int currentCommandParameterIndex = 0;
            AiCommandParsed currentCommand = null;

            var commands = new List<AiCommandParsed>();

            foreach (var line in lines)
            {
                if (state == ParseState.Initial)
                {
                    if (line.Contains("ai commands macros and information"))
                    {
                        state = ParseState.StartLine;
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.StartLine)
                {
                    if (line.StartsWith("/*========="))
                    {
                        state = ParseState.Name;

                        currentCommand = new AiCommandParsed();
                        currentCommandParameterIndex = 0;

                        continue;
                    }

                    continue;
                }

                if (state == ParseState.Name)
                {
                    var re = reName.Match(line);
                    if (re.Success)
                    {
                        currentCommand.DecompName = re.Groups[1].Value;

                        state = ParseState.CommandId;
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.CommandId)
                {
                    var re = reCommandId.Match(line);
                    if (re.Success)
                    {
                        currentCommand.CommandId = (byte)GetFromHex(re.Groups[1].Value.Trim());

                        state = ParseState.InfoStart;
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.InfoStart)
                {
                    var re = reInfo.Match(line);
                    if (re.Success)
                    {
                        currentCommand.Description = re.Groups[1].Value.Trim();

                        state = ParseState.InfoLines;
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.InfoLines)
                {
                    var re = reComment.Match(line);
                    if (re.Success)
                    {
                        currentCommand.Description += " " + re.Groups[1].Value;

                        continue;
                    }

                    if (line.StartsWith("//==========="))
                    {
                        state = ParseState.NoteStart;
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.NoteStart)
                {
                    var re = reNote.Match(line);
                    if (re.Success)
                    {
                        currentCommand.Note = re.Groups[1].Value.Trim();

                        state = ParseState.NoteLines;
                        continue;
                    }

                    re = reArguments.Match(line);
                    if (re.Success)
                    {
                        currentCommand.Note = re.Groups[1].Value.Trim();

                        state = ParseState.NoteLines;
                        continue;
                    }

                    re = reDefine.Match(line);
                    if (re.Success)
                    {
                        // no need to set id, parsed from comment above

                        state = ParseState.DefineLength;
                        continue;
                    }

                    continue;
                }

                // reading multi-line note/arguments.
                if (state == ParseState.NoteLines)
                {
                    if (line.StartsWith("//============"))
                    {
                        state = ParseState.DefineId;
                        continue;
                    }

                    var re = reComment.Match(line);
                    if (re.Success)
                    {
                        currentCommand.Note += " " + re.Groups[1].Value.Trim();
                        continue;
                    }

                    continue;
                }

                if (state == ParseState.DefineId)
                {
                    var re = reDefine.Match(line);
                    if (re.Success)
                    {
                        // id was parsed from comment above, just continue
                        state = ParseState.DefineLength;
                        continue;
                    }

                    continue;
                }

                // #define ..._LENGTH
                if (state == ParseState.DefineLength)
                {
                    var re = reDefine.Match(line);
                    if (re.Success)
                    {
                        if (re.Groups[1].Value.Contains("_LENGTH"))
                        {
                            var parts = re.Groups[1].Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length > 1)
                            {
                                currentCommand.CommandLengthBytes = (byte)GetFromHex(parts[1].Trim());
                            }
                            else
                            {
                                throw new InvalidOperationException("Cannot parse command length from line: " + line);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("State machine is lost. Trying to find command length, but got line: " + line);
                        }

                        state = ParseState.DefineCommand;
                        continue;
                    }
                }

                // first line of #define command definition
                if (state == ParseState.DefineCommand)
                {
                    var re = reDefine.Match(line);
                    if (re.Success)
                    {
                        var reparameters = reParen.Match(re.Groups[1].Value);
                        if (reparameters.Success)
                        {
                            var parameterNames = reparameters.Groups[1].Value.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                            currentCommand.NumberParameters = parameterNames.Count;
                        }
                        else
                        {
                            currentCommand.NumberParameters = 0;
                        }

                        state = ParseState.DefineParameter;
                    }

                    continue;
                }

                if (state == ParseState.DefineParameter)
                {
                    int length = 1;
                    string name = string.Empty;

                    if (reEmpty.Match(line).Success)
                    {
                        commands.Add(currentCommand);

                        state = ParseState.StartLine;
                        continue;
                    }

                    // need to test most permissive lines last
                    var re32 = reCommand32.Match(line);
                    var re24 = reCommand24.Match(line);
                    var re16 = reCommand16.Match(line);
                    var re = reCommand.Match(line);
                    if (re32.Success)
                    {
                        length = 4;
                        name = re32.Groups[1].Value.Trim();
                    }
                    else if (re24.Success)
                    {
                        length = 3;
                        name = re24.Groups[1].Value.Trim();
                    }
                    else if (re16.Success)
                    {
                        length = 2;
                        name = re16.Groups[1].Value.Trim();
                    }
                    else if (re.Success)
                    {
                        if (currentCommandParameterIndex == 0)
                        {
                            // skip id parameter
                            currentCommandParameterIndex++;
                            continue;
                        }

                        length = 1;
                        name = re.Groups[1].Value.Trim();
                    }
                    else
                    {
                        throw new InvalidOperationException("State machine is lost. Trying to find parameter, but got line: " + line);
                    }

                    var parameter = new AiCommandParameter(SanitizeCSharpName(name), length, 0);
                    currentCommand.CommandParameters.Add(parameter);

                    currentCommandParameterIndex++;

                    continue;
                }
            }

            WriteOutputFile(commands);

            Console.WriteLine("done");
        }

        private static System.ComponentModel.Int32Converter _converter = new System.ComponentModel.Int32Converter();
        private static int GetFromHex(string s)
        {
            if (s.Contains("0x"))
            {
                return (int)_converter.ConvertFromString(s);
            }
            else
            {
                return int.Parse(s, System.Globalization.NumberStyles.HexNumber);
            }
        }

        private static void WriteOutputFile(List<AiCommandParsed> commands)
        {
            var outputFilePath = "C:\\Users\\bburns\\code\\scratch\\aajk\\AiCommandBuilder.cs";
            var lines = new List<string>();
            var indent = "    ";

            var dictionaryLines = new List<string>();

            lines.Add("namespace bbbb");
            lines.Add("{");
            lines.Add(indent + "public class AiCommandBuilder");
            lines.Add(indent + "{");

            foreach (var command in commands)
            {
                var propertyName = SnakeToCamel(command.DecompName);

                dictionaryLines.Add($"{{ {propertyName}.{nameof(AiCommandDescription.CommandId)}, {propertyName} }},");

                string line;
                //string argsText = string.Empty;
                //if (command.NumberParameters > 0)
                //{
                //    argsText = string.Join(", ", command.CommandParameters.Select(x => "int " + x.ParameterName));
                //}

                lines.Add(indent + indent + "/// <summary>");
                lines.Add(indent + indent + "/// " + command.Description);
                lines.Add(indent + indent + "/// </summary>");
                if (!string.IsNullOrEmpty(command.Note))
                {
                    lines.Add(indent + indent + "/// <remarks>");
                    lines.Add(indent + indent + "/// " + command.Note);
                    lines.Add(indent + indent + "/// </remarks>");
                }
                line = $"public static {nameof(AiCommandDescription)} {propertyName}";
                lines.Add(indent + indent + line);
                lines.Add(indent + indent + "{");
                lines.Add(indent + indent + indent + "get");
                lines.Add(indent + indent + indent + "{");
                lines.Add(indent + indent + indent + indent + $"return new {nameof(AiCommandDescription)}()");
                lines.Add(indent + indent + indent + indent + "{");
                lines.Add(indent + indent + indent + indent + indent + $"{nameof(AiCommandDescription.DecompName)} = \"{command.DecompName}\",");
                lines.Add(indent + indent + indent + indent + indent + $"{nameof(AiCommandDescription.CommandId)} = {command.CommandId},");
                lines.Add(indent + indent + indent + indent + indent + $"{nameof(AiCommandDescription.CommandLengthBytes)} = {command.CommandLengthBytes},");
                lines.Add(indent + indent + indent + indent + indent + $"{nameof(AiCommandDescription.NumberParameters)} = {command.NumberParameters},");
                lines.Add(indent + indent + indent + indent + indent + $"{nameof(AiCommandDescription.CommandParameters)} = new List<{nameof(AiCommandParameterDescription)}>()");
                if (command.NumberParameters > 0 )
                {
                    lines.Add(indent + indent + indent + indent + indent + "{");
                    foreach (var commandParameter in command.CommandParameters)
                    {
                        lines.Add(indent + indent + indent + indent + indent + indent + 
                            $"new {nameof(AiCommandParameterDescription)}(){{ {nameof(commandParameter.ParameterName)} = \"{commandParameter.ParameterName}\", {nameof(commandParameter.ByteLength)} = {commandParameter.ByteLength} }},");
                    }
                    lines.Add(indent + indent + indent + indent + indent + "}");
                }
                lines.Add(indent + indent + indent + indent + "};");
                lines.Add(indent + indent + indent + "}");
                lines.Add(indent + indent + "}");
                lines.Add(string.Empty);
            }

            lines.Add(string.Empty);
            lines.Add(indent + indent + $"public static Dictionary<int, {nameof(AiCommandDescription)}> AiCommandById = new Dictionary<int, {nameof(AiCommandDescription)}>()");
            lines.Add(indent + indent + "{");
            foreach (var line in dictionaryLines)
            {
                lines.Add(indent + indent + indent + line);
            }
            lines.Add(indent + indent + "};");

            lines.Add(string.Empty);
            lines.Add(indent + "}");
            
            lines.Add("}");
            lines.Add(string.Empty);

            System.IO.File.WriteAllLines(outputFilePath, lines);
        }

        private static string SnakeToCamel(string s)
        {
            return s.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);
        }

        private static string SanitizeCSharpName(string name)
        {
            if (name == "byte")
            {
                return "cbyte";
            }

            return name;
        }

        //public static AiCommandBlock ParseBytes(byte[] bytes)
        //{
        //    int position = 0;
        //    byte b;
        //    var results = new AiCommandBlock();

        //    while (true)
        //    {
        //        b = bytes[position++];
        //        var commandDescription = AiCommandBuilder.AiCommandById[b];
        //        var commandParameters = new List<AiCommandParameter>();
        //        for (int i = 0; i < commandDescription.NumberParameters; i++)
        //        {
        //            var len = commandDescription.CommandParameters[i].ByteLength;
        //            int val = 0;
        //            for (var j = 0; j < len; j++)
        //            {
        //                val |= bytes[position + j] << (8 * j);
        //            }
        //            commandParameters.Add(new AiCommandParameter(commandDescription.CommandParameters[i].ParameterName, len, val));
        //            position += len;
        //        }

        //        var aic = new AiCommand(commandDescription)
        //        {
        //            CommandParameters = commandParameters,
        //        };

        //        results.Commands.Add(aic);

        //        if (commandDescription.CommandId == AiCommandBuilder.AiListEnd.CommandId
        //            || position >= bytes.Length)
        //        {
        //            break;
        //        }
        //    }

        //    return results;
        //}
    }

    public class AiCommandBlock
    {
        public List<AiCommand> Commands { get; set; } = new List<AiCommand>();

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
            string indent = "    ";
            
            for (int i = 0; i < Commands.Count; i++)
            {
                var c = Commands[i];

                sb.AppendLine(prefix + $"// 0x{c.CommandId}");
                sb.Append(prefix + c.DecompName);

                string argsText = string.Empty;
                string argsCommentText = string.Empty;
                if (c.NumberParameters > 0)
                {
                    sb.AppendLine("(");
                    argsText = string.Join(", ", c.CommandParameters.Select(x => "0x" + x.ByteValue.ToString("x")));
                    argsCommentText = string.Join(", ", c.CommandParameters.Select(x => x.ParameterName));
                    sb.AppendLine(prefix + indent + "// " + argsCommentText);
                    sb.Append(prefix + indent + argsText + ")");
                }

                if (i < Commands.Count - 1)
                {
                    sb.Append(",");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }

    public class AiCommandDescription
    {
        public string DecompName { get; set; }
        public byte CommandId { get; set; }
        public int CommandLengthBytes { get; set; }
        public int NumberParameters { get; set; }
        public List<AiCommandParameterDescription> CommandParameters { get; set; } = new List<AiCommandParameterDescription>();
    }

    public class AiCommand
    {
        public AiCommand()
        {
        }

        public AiCommand(AiCommandDescription description)
        {
            DecompName = description.DecompName;
            CommandId = description.CommandId;
            CommandLengthBytes = description.CommandLengthBytes;
            NumberParameters = description.NumberParameters;
        }

        public string DecompName { get; set; }
        public byte CommandId { get; set; }
        public int CommandLengthBytes { get; set; }
        public int NumberParameters { get; set; }
        public List<AiCommandParameter> CommandParameters { get; set; } = new List<AiCommandParameter>();

        public byte[] ToByteArray()
        {
            var results = new byte[CommandLengthBytes];
            results[0] = CommandId;
            int resultPosition = 1;
            for (var i = 0; i < NumberParameters; i++)
            {
                AppendParameter(results, resultPosition, CommandParameters[i]);
                resultPosition += CommandParameters[i].ByteLength;
            }
            return results;
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

        private void AppendParameter(byte[] arr, int position, AiCommandParameter parameter)
        {
            if (parameter.ByteLength < 1 || parameter.ByteLength > 4)
            {
                throw new NotSupportedException();
            }

            if (parameter.ByteLength > 3)
            {
                arr[position + 3] = (byte)((parameter.ByteValue & 0xFF000000) >> 24);
            }

            if (parameter.ByteLength > 2)
            {
                arr[position + 2] = (byte)((parameter.ByteValue & 0x00FF0000) >> 16);
            }

            if (parameter.ByteLength > 1)
            {
                arr[position + 1] = (byte)((parameter.ByteValue & 0x0000FF00) >> 8);
            }

            if (parameter.ByteLength > 0)
            {
                arr[position + 0] = (byte)((parameter.ByteValue & 0x000000FF) >> 0);
            }
        }
    }

    public class AiCommandParsed : AiCommand
    {
        public AiCommandParsed()
        {
        }

        public string Description { get; set; } // info
        public string Note { get; set; } // note, arguments
    }

    public record AiCommandParameterDescription
    {
        public string ParameterName { get; init; }
        public int ByteLength { get; init; }
    }

    public record AiCommandParameter
    {
        public AiCommandParameter(string name, int length, int val)
        {
            if (length < 1 || length > 4)
            {
                throw new NotSupportedException();
            }

            ParameterName = name;
            ByteLength = length;
            ByteValue = val;

            if (length == 1)
            {
                ByteValue = val & 0xff;
            }
            else if (length == 2)
            {
                ByteValue = val & 0xffff;
            }
            else if (length == 3)
            {
                ByteValue = val & 0xffffff;
            }
            else if (length == 4)
            {
                ByteValue = val;
            }
        }

        public string ParameterName { get; init; }
        public int ByteLength { get; init; }
        public int ByteValue { get; init; }

        public override string ToString()
        {
            return $"{ParameterName}=0x{ByteValue:x}";
        }
    }
}