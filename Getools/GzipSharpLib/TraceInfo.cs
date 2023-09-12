using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GzipSharpLib
{
    /// <summary>
    /// Helper class to print trace info.
    /// </summary>
    internal record class TraceInfo
    {
        /// <summary>
        /// Name of method currently executing.
        /// </summary>
        public string Method { get; init; }

        /// <summary>
        /// Number of times the method has been called.
        /// </summary>
        public int CallCount { get; init; }

        /// <summary>
        /// Name of variable being printed.
        /// </summary>
        public string VariableName { get; init; }

        /// <summary>
        /// Type (or pseudo type) of variable being printed.
        /// </summary>
        public string VariableType { get; init; }

        /// <summary>
        /// Whether the variable is an array or list.
        /// </summary>
        public bool IsCollection { get; init; }

        /// <summary>
        /// Object to print.
        /// </summary>
        public object? Value { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TraceInfo()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
        }

        /// <summary>
        /// Trace log output, in "plain" c format.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Matches custom logging added to inflate.c, taking null to 0.
        /// </remarks>
        internal string ToTraceJson(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            sb.Append("{ ");
            sb.Append($" 'method': '{Method}',");
            sb.Append($" 'call_count': {CallCount},");
            sb.Append($" 'name': '{VariableName}',");
            sb.Append($" 'type': '{VariableType}',");

            if (IsCollection)
            {
                sb.AppendLine($" 'values': [");
                sb.Append(prefix);

                if (!object.ReferenceEquals(null, Value))
                {
                    var typename = Value.GetType().Name;

                    if (Value.GetType().IsArray)
                    {
                        switch (typename)
                        {
                            case "Int16[]": // fallthrough
                            case "short[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int16>)Value)));
                                break;
                            }

                            case "int[]": // fallthrough
                            case "Int32[]": // fallthrough
                            case "Integer[]": // fallthrough
                            case "integer[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int32>)Value)));
                                break;
                            }

                            case "long[]": // fallthrough
                            case "Int64[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int64>)Value)));
                                break;
                            }

                            case "UInt16[]": // fallthrough
                            case "ushort[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt16>)Value)));
                                break;
                            }

                            case "uint[]": // fallthrough
                            case "UInt32[]": // fallthrough
                            case "UInteger[]": // fallthrough
                            case "unsigned[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt32>)Value)));
                                break;
                            }

                            case "Ulong[]": // fallthrough
                            case "UInt64[]": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt64>)Value)));
                                break;
                            }

                            default: throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        var listType = Value.GetType().GetGenericArguments()[0].Name;

                        switch (listType)
                        {
                            case "Int16": // fallthrough
                            case "short": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int16>)Value)));
                                break;
                            }

                            case "int": // fallthrough
                            case "Int32": // fallthrough
                            case "Integer": // fallthrough
                            case "integer": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int32>)Value)));
                                break;
                            }

                            case "long": // fallthrough
                            case "Int64": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<Int64>)Value)));
                                break;
                            }

                            case "UInt16": // fallthrough
                            case "ushort": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt16>)Value)));
                                break;
                            }

                            case "uint": // fallthrough
                            case "UInt32": // fallthrough
                            case "UInteger": // fallthrough
                            case "unsigned": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt32>)Value)));
                                break;
                            }

                            case "Ulong": // fallthrough
                            case "UInt64": // fallthrough
                            {
                                sb.Append(string.Join(",", ((IEnumerable<UInt64>)Value)));
                                break;
                            }

                            case nameof(HuftTable):
                            {
                                sb.AppendLine(string.Join(",\n", ((IEnumerable<HuftTable>)Value).Select(x => x.ToTraceJson(prefix + "    "))));
                                break;
                            }

                            default: throw new NotSupportedException();
                        }
                    }
                }

                sb.AppendLine();
                sb.Append(prefix);
                sb.Append($"]");
            }
            else
            {
                sb.Append($" 'value': ");

                if (object.ReferenceEquals(null, Value))
                {
                    sb.Append("null");
                }
                else
                {
                    var typename = Value.GetType().Name;
                    switch (typename)
                    {
                        case "Int16": // fallthrough
                        case "short": // fallthrough
                        {
                            sb.Append(((Int64)Value).ToString());
                            break;
                        }

                        case "int": // fallthrough
                        case "Int32": // fallthrough
                        case "Integer": // fallthrough
                        case "integer": // fallthrough
                        {
                            sb.Append(((Int32)Value).ToString());
                            break;
                        }

                        case "Int64": // fallthrough
                        case "long": // fallthrough
                        {
                            sb.Append(((Int64)Value).ToString());
                            break;
                        }

                        case "ushort": // fallthrough
                        case "UInt16": // fallthrough
                        {
                            sb.Append(((UInt16)Value).ToString());
                            break;
                        }

                        case "uint": // fallthrough
                        case "UInt32": // fallthrough
                        case "unsigned": // fallthrough
                        {
                            sb.Append(((UInt32)Value).ToString());
                            break;
                        }

                        case "UInt64": // fallthrough
                        case "ulong": // fallthrough
                        {
                            sb.Append(((UInt64)Value).ToString());
                            break;
                        }

                        default: throw new NotSupportedException();
                    }
                }
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
