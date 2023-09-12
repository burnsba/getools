using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GzipSharpLib
{
    /// <summary>
    /// Huffman code lookup table entry--this entry is four bytes for machines
    /// that have 16-bit pointers(e.g.PC's in the small or medium model).
    /// Valid extra bits are 0..13.  e == 15 is EOB (end of block), e == 16
    /// means that v is a literal, 16 < e< 32 means that v is a pointer to
    /// the next table, which codes e - 16 bits, and lastly e == 99 indicates
    /// an unused code.If a code with e == 99 is looked up, this implies an
    /// error in the data.
    /// </summary>
    public class Huft
    {
        /// <summary>
        /// Id used for trace/debug.
        /// </summary>
        internal int Id { get; set; }

        /// <summary>
        /// Number of extra bits or operation.
        /// </summary>
        /// <remarks>
        /// inflate.c: uch e
        /// </remarks>
        public byte ExtraBits { get; set; }

        /// <summary>
        /// Number of bits in this code or subcode.
        /// </summary>
        /// <remarks>
        /// inflate.c: uch b
        /// </remarks>
        public byte CodeBits { get; set; }

        /// <summary>
        /// Literal, length base, or distance base.
        /// </summary>
        /// <remarks>
        /// inflate.c: v.n from union { ush n; struct huft *t; } v
        /// </remarks>
        public UInt16 LengthBase { get; set; }

        /// <summary>
        /// Pointer to next level of table.
        /// </summary>
        /// <remarks>
        /// inflate.c: v.t from union { ush n; struct huft *t; } v
        /// </remarks>
        public HuftTable? Next { get; set; } = new HuftTable();

        /// <summary>
        /// Points to parent/previous struct.
        /// </summary>
        public HuftTable? Parent { get; set; }

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
            sb.Append($" 'id': {Id},");
            sb.Append($" 'e': {ExtraBits},");
            sb.Append($" 'b': {CodeBits},");
            sb.Append($" 'v_n': {LengthBase},");
            sb.Append($" 'v_t_id': ");
            if (Next != null)
            {
                sb.Append(Next.Id);
            }
            else
            {
                sb.Append(0);
            }
            sb.Append(',');
            sb.Append($" 'parent_id': ");
            if (Parent != null)
            {
                sb.Append(Parent.Id);
            }
            else
            {
                sb.Append(0);
            }
            sb.Append(" }");

            return sb.ToString();
        }

        /// <summary>
        /// Trace log output, in C# name format.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Matches custom logging added to inflate.c but uses C# property names, taking null to 0.
        /// </remarks>
        internal string ToJson(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            sb.Append("{ ");
            sb.Append($" '{nameof(Id)}': {Id},");
            sb.Append($" '{nameof(ExtraBits)}': {ExtraBits},");
            sb.Append($" '{nameof(CodeBits)}': {CodeBits},");
            sb.Append($" '{nameof(LengthBase)}': {LengthBase},");
            sb.Append($" '{nameof(Next)}.{nameof(Next.Id)}': ");
            if (Next != null)
            {
                sb.Append(Next.Id);
            }
            else
            {
                sb.Append(0);
            }
            sb.Append(',');
            sb.Append($" '{nameof(Parent)}.{nameof(Parent.Id)}': ");
            if (Parent != null)
            {
                sb.Append(Parent.Id);
            }
            else
            {
                sb.Append(0);
            }
            sb.Append(" }");

            return sb.ToString();
        }
    }
}
