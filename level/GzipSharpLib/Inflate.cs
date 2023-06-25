/* inflate.c -- Not copyrighted 1992 by Mark Adler
   version c10p1, 10 January 1993 */

using GzipSharpLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GzipSharpLib
{
    /// <summary>
    /// Corresponds to gzip inflate.c
    /// </summary>
    /// <remarks>
    /// Inflate deflated (PKZIP's method 8 compressed) data.  The compression
    /// method searches for as much of the current string of bytes (up to a
    /// length of 258) in the previous 32K bytes.  If it doesn't find any
    /// matches (of at least length 3), it codes the next byte.  Otherwise, it
    /// codes the length of the matched string and its distance backwards from
    /// the current position.  There is a single Huffman code that codes both
    /// single bytes (called "literals") and match lengths.  A second Huffman
    /// code codes the distance information, which follows a length code.  Each
    /// length or distance code actually represents a base value and a number
    /// of "extra" (sometimes zero) bits to get to add to the base value.  At
    /// the end of each deflated block is a special end-of-block (EOB) literal/
    /// length code.  The decoding process is basically: get a literal/length
    /// code; if EOB then done; if a literal, emit the decoded byte; if a
    /// length then get the distance and emit the referred-to bytes from the
    /// sliding window of previously emitted data.
    /// 
    /// There are (currently) three kinds of inflate blocks: stored, fixed, and
    /// dynamic.  The compressor deals with some chunk of data at a time, and
    /// decides which method to use on a chunk-by-chunk basis.  A chunk might
    /// typically be 32K or 64K.  If the chunk is uncompressible, then the
    /// "stored" method is used.  In this case, the bytes are simply stored as
    /// is, eight bits per byte, with none of the above coding.  The bytes are
    /// preceded by a count, since there is no longer an EOB code.
    /// 
    /// If the data is compressible, then either the fixed or dynamic methods
    /// are used.  In the dynamic method, the compressed data is preceded by
    /// an encoding of the literal/length and distance Huffman codes that are
    /// to be used to decode this block.  The representation is itself Huffman
    /// coded, and so is preceded by a description of that code.  These code
    /// descriptions take up a little space, and so for small blocks, there is
    /// a predefined set of codes, called the fixed codes.  The fixed method is
    /// used if the block codes up smaller that way (usually for quite small
    /// chunks), otherwise the dynamic method is used.  In the latter case, the
    /// codes are customized to the probabilities in the current block, and so
    /// can code it much better than the pre-determined fixed codes.
    ///  
    /// The Huffman codes themselves are decoded using a mutli-level table
    /// lookup, in order to maximize the speed of decoding plus the speed of
    /// building the decoding tables.  See the comments below that precede the
    /// lbits and dbits tuning parameters.
    /// </remarks>
    internal class Inflate
    {
        /// <summary>
        /// Maximum bit length of any code (16 for explode).
        /// </summary>
        public const int BMAX = 16;
        
        /// <summary>
        /// Maximum number of codes in any set.
        /// </summary>
        public const int N_MAX = 288;

        public const int WSIZE = 0x8000;

        /* Tables for deflate from PKZIP's appnote.txt. */

        /// <summary>
        /// Order of the bit length code lengths.
        /// </summary>
        /// <remarks>
        /// inflate.c: border
        /// </remarks>
        public UInt32[] Border = new List<int>() {
            16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15
        }.Select(x => (UInt32)x).ToArray();

        /// <summary>
        /// Copy lengths for literal codes 257..285.
        /// </summary>
        /// <remarks>
        /// inflate.c: cplens
        /// </remarks>
        public UInt16[] LiteralCodesLengths = new List<int>() {
            3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
            /* note: see note #13 above about the 258 in this list. */
            35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258, 0, 0
        }.Select(x => (UInt16)x).ToArray();

        /// <summary>
        /// Extra bits for literal codes 257..285.
        /// </summary>
        /// <remarks>
        /// inflate.c: cplext
        /// </remarks>
        public UInt16[] LiteralCodesExtraBits = new List<int>() {
            0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
            3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0,
            /* 99==invalid */
            99, 99
        }.Select(x => (UInt16)x).ToArray();

        /// <summary>
        /// Copy offsets for distance codes 0..29.
        /// </summary>
        /// <remarks>
        /// inflate.c: cpdist
        /// </remarks>
        public UInt16[] DistanceCodesCopyOffsets = new List<int>() {
            1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
            257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
            8193, 12289, 16385, 24577
        }.Select(x => (UInt16)x).ToArray();

        /// <summary>
        /// Extra bits for distance codes.
        /// </summary>
        /// <remarks>
        /// inflate.c: cpdext
        /// </remarks>
        public UInt16[] DistanceCodesExtraBits = new List<int>() {
            0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
            7, 7, 8, 8, 9, 9, 10, 10, 11, 11,
            12, 12, 13, 13
        }.Select(x => (UInt16)x).ToArray();

        public UInt16[] MaskBits = new List<int>() {
            0x0000,
            0x0001, 0x0003, 0x0007, 0x000f, 0x001f, 0x003f, 0x007f, 0x00ff,
            0x01ff, 0x03ff, 0x07ff, 0x0fff, 0x1fff, 0x3fff, 0x7fff, 0xffff
        }.Select(x => (UInt16)x).ToArray();

        /// <summary>
        /// bits in base literal/length lookup table
        /// </summary>
        private int _lbits = 9;

        /// <summary>
        /// bits in base distance lookup table
        /// </summary>
        private int _dbits = 6;

        /// <summary>
        /// Bit buffer;
        /// </summary>
        private UInt64 _bitBuffer;

        /// <summary>
        /// Bits in bit buffer.
        /// </summary>
        private uint _bk;

        /// <summary>
        /// Track memory usage.
        /// </summary>
        private int _hufts;

        private int _wp;

        private Context _context;

        /// <summary>
        /// "unique" id to assign to each <see cref="Huft"/> / <see cref="HuftTable"/>.
        /// This is for debugging to match c implementation.
        /// </summary>
        private int _huft_id = 1;
        private int _trace_huft_build_count = 0;

        public Inflate(Context context)
        {
            _context = context;
        }

        public void Go()
        {
            InflateEntryMain();
        }

        /// <summary>
        /// Given a list of code lengths and a maximum table size, make a set of
        /// tables to decode that set of codes.
        /// </summary>
        /// <param name="b">Code lengths in bits (all assumed <= BMAX).</param>
        /// <param name="n">Number of codes (assumed <= N_MAX).</param>
        /// <param name="s">Number of simple-valued codes (0..s-1).</param>
        /// <param name="d">List of base values for non-simple codes.</param>
        /// <param name="e">List of extra bits for non-simple codes.</param>
        /// <param name="t">Result: starting table.</param>
        /// <param name="m">Maximum lookup bits, returns actual.</param>
        /// <returns>
        /// Return zero on success, one if the given code set is incomplete (the
        /// tables are still built in this case), two if the input is invalid (all
        /// zero length codes or an oversubscribed set of lengths), and three if
        /// not enough memory.
        /// </returns>
        private HuftBuildResult HuftBuild(UInt32[] b, UInt32 n, UInt32 s, UInt16[]? d, UInt16[]? e, ref HuftTable t, ref int m)
        {
            string traceName = System.Reflection.MethodBase.GetCurrentMethod()!.Name;
            _trace_huft_build_count++;

            if (_context.Trace)
            {
                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"Enter {nameof(HuftBuild)}");
            }

            // counter for codes of length k
            UInt32 a;

            // bit length count table
            UInt32[] c = new UInt32[BMAX + 1];

            UInt32 f;              /* i repeats in table every f entries */
            int g;                 /* maximum code length */
            int h;                 /* table level */
            UInt32 i;              /* counter, current code */
            UInt32 j;              /* counter */
            int k;                 /* number of bits in current code */
            int l;                 /* bits per table (returned in m) */
            UInt32 p;              /* pointer into c[], b[], or v[] */

            HuftTable q;           /* points to current table */
            Huft r = new Huft();   /* table entry for structure assignment */
            r.Id = _huft_id;
            _huft_id++;

            // table stack.
            // Declare as array in c:
            // HuftTable[BMAX]
            List<HuftTable> u = new List<HuftTable>();

            UInt32[] v = new UInt32[N_MAX];     /* values in order of bit length */
            int w;                              /* bits before this table == (l * h) */
            UInt32[] x = new UInt32[BMAX + 1];  /* bit offsets, then code stack */
            UInt32 xp;                          /* pointer into x */
            int y;                              /* number of dummy codes added */
            UInt32 z;                           /* number of entries in current table */

            // t_set and prev_t are used for book keeping in C#.
            // This is to keep the huft->next pointers assigned correctly,
            // similar to how the c implementation does.
            bool t_set = false;
            Huft? prev_t = null;

            // Debug/trace variable to keep track of what is allocated in this method call.
            List<HuftTable> qaloc = new List<HuftTable>();

            /* Generate counts for each bit length */
            Array.Clear(c, 0, c.Length);

            if (_context.Trace)
            {
                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"BEGIN start {traceName} variables");

                // n is the number of codes in b/
                // the c (language) debug output only prints [0-n] items.
                uint[] printb = b.Take((int)n).ToArray();

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(b),
                    VariableType = b.GetType().Name,
                    IsCollection = true,
                    Value = printb,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(n),
                    VariableType = n.GetType().Name,
                    IsCollection = false,
                    Value = n,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(s),
                    VariableType = s.GetType().Name,
                    IsCollection = false,
                    Value = s,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(d),
                    VariableType = object.ReferenceEquals(null, d) ? "UInt16[]" : d.GetType().Name,
                    IsCollection = true,
                    Value = d,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(e),
                    VariableType = object.ReferenceEquals(null, e) ? "UInt16[]" : e.GetType().Name,
                    IsCollection = true,
                    Value = e,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"END start {traceName} variables");
            }

            // p was a pointer, now it is an index into array.
            // c code:
            // p = b;

            p = 0;
            i = n;
            do
            {
                //Tracecv(*p, (stderr, (n - i >= ' ' && n - i <= '~' ? "%c %d\n" : "0x%x %d\n"),n - i, *p));
                c[b[p]]++;                    /* assume all entries <= BMAX */
                p++;                      /* Can't combine with above line (Solaris bug) */
            } while (--i > 0);

            if (c[0] == n)                /* null input--all zero length codes */
            {
                m = 0;
                //t = null;
                //return HuftBuildResult.Success;
                throw new Exception("null input--all zero length codes");
            }

            /* Find minimum and maximum length, bound *m by those */
            l = m;
            for (j = 1; j <= BMAX; j++)
            {
                if (c[j] > 0)
                {
                    break;
                }
            }

            k = (int)j;                        /* minimum code length */
            if ((UInt32)l < j)
            {
                l = (int)j;
            }

            for (i = BMAX; i > 0; i--)
            {
                if (c[i] > 0)
                {
                    break;
                }
            }

            g = (int)i;                        /* maximum code length */
            if ((UInt32)l > i)
            {
                l = (int)i;
            }

            m = l;

            /* Adjust last length count to fill out codes, if needed */
            for (y = 1 << (int)j; j < i; j++, y <<= 1)
            {
                y -= (int)c[j];
                if (y < 0)
                {
                    throw new Exception("bad input: more codes than bits");
                }
            }

            y -= (int)c[i];
            if (y < 0)
            {
                throw new Exception($"Unspecified error in {nameof(HuftBuild)}");
            }

            c[i] += (UInt32)y;

            /* Generate starting offsets into the value table for each length */
            x[1] = j = 0;

            // p was a pointer, now it is an index into array.
            // c code:
            // p = c + 1;
            p = 1;

            // xp was a pointer, now it is an index into array.
            // c code:
            // xp = x + 2;
            xp = 2;
            while (--i > 0)
            {                 /* note that i == g from above */
                j += c[p];
                p++;

                x[xp] = j;
                xp++;
            }

            /* Make a table of values in order of bit lengths */

            // p was a pointer, now it is an index into array.
            // c code:
            // p = b;

            p = 0;
            i = 0;
            do
            {
                j = b[p];
                p++;

                if (j != 0)
                {
                    v[x[j]++] = i;
                }
            } while (++i < n);

            /* Generate the Huffman codes and for each, make the table entries */
            x[0] = i = 0;                 /* first Huffman code is zero */
            // p = v;
            p = 0;                        /* grab values in bit order */
            h = -1;                       /* no tables yet--level -1 */
            w = -l;                       /* bits decoded == (l * h) */

            //u[0] = null;   /* just to keep compilers happy */
            q = null!;        /* ditto */
            z = 0;           /* ditto */

            if (_context.Trace)
            {
                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"BEGIN mid {traceName} variables");

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(c),
                    VariableType = c.GetType().Name,
                    IsCollection = true,
                    Value = c,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(g),
                    VariableType = g.GetType().Name,
                    IsCollection = false,
                    Value = g,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(h),
                    VariableType = h.GetType().Name,
                    IsCollection = false,
                    Value = h,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(i),
                    VariableType = i.GetType().Name,
                    IsCollection = false,
                    Value = i,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(j),
                    VariableType = j.GetType().Name,
                    IsCollection = false,
                    Value = j,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(k),
                    VariableType = k.GetType().Name,
                    IsCollection = false,
                    Value = k,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(l),
                    VariableType = l.GetType().Name,
                    IsCollection = false,
                    Value = l,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(p),
                    VariableType = (v[p]).GetType().Name,
                    IsCollection = false,
                    Value = v[p],
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(v),
                    VariableType = v.GetType().Name,
                    IsCollection = true,
                    Value = v,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(w),
                    VariableType = w.GetType().Name,
                    IsCollection = false,
                    Value = w,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(x),
                    VariableType = x.GetType().Name,
                    IsCollection = true,
                    Value = x,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(xp),
                    VariableType = (x[xp]).GetType().Name,
                    IsCollection = false,
                    Value = x[xp],
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(y),
                    VariableType = y.GetType().Name,
                    IsCollection = false,
                    Value = y,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(z),
                    VariableType = z.GetType().Name,
                    IsCollection = false,
                    Value = z,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"END mid {traceName} variables");
            }

            /* go through the bit lengths (k already is bits in shortest code) */
            for (; k <= g; k++)
            {
                a = c[k];
                while (a-- > 0)
                {
                    /* here i is the Huffman code of length k bits for value *p */
                    /* make tables up to required level */
                    while (k > w + l)
                    {
                        h++;
                        w += l;                 /* previous table always l bits */

                        /* compute minimum size table less than or equal to l bits */
                        z = (UInt32)g - (UInt32)w;
                        z = z > (UInt32)l ? (UInt32)l : z;  /* upper limit on table size */

                        j = (UInt32)k - (UInt32)w;
                        f = (UInt32)(1 << (int)j);

                        if ((f) > a + 1)     /* try a k-w bit table */
                        {                       /* too few codes for k-w bit table */
                            f -= a + 1;           /* deduct codes from patterns left */

                            // xp was a pointer, now it is an index into array.
                            // c code:
                            // xp = c + k;
                            xp = (UInt32)k;

                            while (++j < z)       /* try smaller tables up to z bits */
                            {
                                ++xp;

                                if ((f <<= 1) <= c[xp])
                                {
                                    break;            /* enough codes to use up j bits */
                                }

                                f -= c[xp];           /* else deduct codes from patterns */
                            }
                        }

                        z = (UInt32)(1 << (int)j);             /* table entries for j-bit table */

                        /* allocate and link in new table */
                        q = new HuftTable();

                        // c version skips the first entry in the allocated table, but still uses it when
                        // performing address arithmetic.
                        // Store a reference in the `Root` property here.
                        q.Root = new Huft();
                        q.Root.Id = _huft_id;
                        _huft_id++;

                        // HuftTable and Huft are different in C#, but the root entry corresponds to the start
                        // of the table allocation in c. So the ids should be the same.
                        q.Id = q.Root.Id;

                        // Allocated all required Huft objects.
                        q.HuftList = Enumerable.Repeat(0, (int)z)
                            .Select(x => new Huft())
                            .ToList();

                        // unique id is only tracked in this file, so set those here.
                        foreach (var hufty in q.HuftList)
                        {
                            hufty.Id = _huft_id;
                            _huft_id++;
                        }

                        qaloc.Add(q);

                        _hufts += (int)z + 1;         /* track memory usage */

                        // The next two `if/else` statements are unravelling the arcane c pointer assignments at this spot.

                        if (!t_set)
                        {
                            //*t = q + 1;             /* link to list for huft_free() */
                            //prev_t = *q;
                            //t_set = true;

                            t = q;
                            prev_t = q.Root;
                            t_set = true;
                        }
                        else
                        {
                            //prev_t.Next = q + 1;
                            //prev_t = *q;

                            prev_t!.Next = q;
                            prev_t = q.Root;
                        }

                        //u[h] = q;             /* table starts after link */
                        if (h+1>u.Count)
                        {
                            u.Add(q);
                            u.Last().HuftList[0].Parent = q;
                        }
                        else
                        {
                            u[h] = q;
                            u[h].HuftList[0].Parent = q;
                        }

                        /* connect to last table, if there is one */
                        if (h > 0)
                        {
                            x[h] = i;             /* save pattern for backing up */
                            r.CodeBits = (byte) l;         /* bits to dump before this table */
                            r.ExtraBits = (byte) (16 + j);  /* bits in this table */
                            r.Next = q;            /* pointer to this table */
                            j = i >> (w - l);     /* (get around Turbo C bug) */

                            // the c code does a bulk struct copy as follows:
                            //u[h - 1].HuftList[(int)j + 1] = r;        /* connect to last table */
                            //
                            // Instead, we want the individual properties copied, without
                            // overwriting the id.

                            u[h - 1].HuftList[(int)j].CodeBits = r.CodeBits;
                            u[h - 1].HuftList[(int)j].ExtraBits = r.ExtraBits;
                            u[h - 1].HuftList[(int)j].Next = r.Next;
                            u[h - 1].HuftList[(int)j].LengthBase = r.LengthBase;
                            r.Next = null;
                            r.Parent = null;
                        }
                    }
                    
                    /* set up table entry in r */
                    r.CodeBits = (byte)(k - w);

                    // p was a pointer, now it is an index into array.
                    // c code:
                    // if (p >= v + n)

                    if (p >= n)
                    {
                        r.ExtraBits = 99;               /* out of values--invalid code */
                    }
                    else if (v[p] < s)
                    {
                        r.ExtraBits = (byte)(v[p] < 256 ? 16 : 15);    /* 256 is end-of-block code */
                        r.LengthBase = (UInt16)(v[p]);             /* simple code is just the value */
                        p++;                           /* one compiler does not like *p++ */
                    }
                    else
                    {
                        r.ExtraBits = (byte)e![v[p] - s];   /* non-simple--look up in lists */
                        r.LengthBase = d![v[p] - s];
                        p++;
                    }
                    
                    /* fill code-like entries with r */
                    f = (UInt32)(1 << (int)(k - w));
                    for (j = i >> w; j < z; j += f)
                    {
                        q.HuftList[(int)j].CodeBits = r.CodeBits;
                        q.HuftList[(int)j].ExtraBits = r.ExtraBits;
                        q.HuftList[(int)j].Next = r.Next;
                        q.HuftList[(int)j].LengthBase = r.LengthBase;
                    }
                    r.Next = null;
                    r.Parent = null;

                    /* backwards increment the k-bit code i */
                    for (j = (UInt32)(1 << (k - 1)); (i & j) != 0; j >>= 1)
                    {
                        i ^= j;
                    }
                    
                    i ^= j;
                    
                    /* backup over finished tables */
                    while ((i & ((1 << w) - 1)) != x[h])
                    {
                        h--;                    /* don't need to update q */
                        w -= l;
                    }
                }
            }

            if (_context.Trace)
            {
                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"BEGIN finish {traceName} variables");

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(c),
                    VariableType = c.GetType().Name,
                    IsCollection = true,
                    Value = c,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(g),
                    VariableType = g.GetType().Name,
                    IsCollection = false,
                    Value = g,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(h),
                    VariableType = h.GetType().Name,
                    IsCollection = false,
                    Value = h,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(k),
                    VariableType = k.GetType().Name,
                    IsCollection = false,
                    Value = k,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(l),
                    VariableType = l.GetType().Name,
                    IsCollection = false,
                    Value = l,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(v),
                    VariableType = v.GetType().Name,
                    IsCollection = true,
                    Value = v,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(x),
                    VariableType = x.GetType().Name,
                    IsCollection = true,
                    Value = x,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(w),
                    VariableType = w.GetType().Name,
                    IsCollection = false,
                    Value = w,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(y),
                    VariableType = y.GetType().Name,
                    IsCollection = false,
                    Value = y,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(z),
                    VariableType = z.GetType().Name,
                    IsCollection = false,
                    Value = z,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(m),
                    VariableType = m.GetType().Name,
                    IsCollection = false,
                    Value = m,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(u),
                    VariableType = "list",
                    IsCollection = true,
                    Value = u,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, new TraceInfo()
                {
                    Method = traceName,
                    CallCount = _trace_huft_build_count,
                    VariableName = nameof(qaloc),
                    VariableType = "list",
                    IsCollection = true,
                    Value = qaloc,
                }.ToTraceJson());

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"END finish {traceName} variables");

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"EXIT {traceName}");
            }

            /* Return true (1) if we were given an incomplete table */
            if (y != 0 && g != 1)
            {
                return HuftBuildResult.Incomplete;
            }

            return HuftBuildResult.Success;
        }

        /// <summary>
        /// inflate (decompress) the codes in a deflated (compressed) block.
        /// Return an error code or zero if it all goes ok.
        /// </summary>
        /// <returns></returns>
        private void InflateCodes(HuftTable tl, HuftTable td, int bl, int bd)
        {
            UInt32 e;  /* table entry flag/number of extra bits */
            UInt32 n, d;        /* length and index for copy */
            UInt32 w;           /* current window position */
            Huft t;       /* pointer to table entry */
            UInt32 ml, md;      /* masks for bl and bd bits */
            UInt64 b;       /* bit buffer */
            UInt32 k;  /* number of bits in bit buffer */

            int _inflate_codes_iteration = 0;
            int _inflate_codes_i2 = 0;

            /* make local copies of globals */
            b = _bitBuffer;                       /* initialize bit buffer */
            k = _bk;
            w = (UInt32)_wp;                       /* initialize window position */

            /* inflate the coded data */
            ml = MaskBits[bl];           /* precompute masks for speed */
            md = MaskBits[bd];

            for (;;)                      /* do until end of block */
            {
                _inflate_codes_iteration++;

                _context.NeedBits(ref b, ref k, (int)bl);
                //NEEDBITS((int)bl);
                t = tl.HuftList[(int)((UInt32)b & ml)];
                e = t.ExtraBits;

                _inflate_codes_i2 = 0;
                if (e > 16)
                {
                    do {
                        if (e == 99)
                        {
                            throw new Exception($"{nameof(InflateCodes)}: unused code");
                        }

                        _context.DumpBits(ref b, ref k, (int)t.CodeBits);
                        //DUMPBITS(t->CodeBits);
                        e -= 16;
                        _context.NeedBits(ref b, ref k, (int)e);
                        //NEEDBITS((int)e);

                        // end of body
                        //t = t.Next + ((UInt32)b & MaskBits[e]);
                        t = t!.Next!.HuftList[(int)((UInt32)b & MaskBits[e])];
                        e = t.ExtraBits;
                        _inflate_codes_i2++;
                    } while (e > 16);

                }

                _context.DumpBits(ref b, ref k, (int)t.CodeBits);
                //DUMPBITS(t->CodeBits);
                if (e == 16)                /* then it's a literal */
                {
                    _context.Window[w++] = (byte) t.LengthBase;
                    //Tracevv
                    if (w == WSIZE)
                    {
                        FlushOutput((int)w);
                        w = 0;
                    }
                }
                else                        /* it's an EOB or a length */
                {
                    /* exit if end of block */
                    if (e == 15)
                    {
                        break;
                    }

                    /* get length of block to copy */

                    _context.NeedBits(ref b, ref k, (int)e);
                    //NEEDBITS((int)e);
                    n = t.LengthBase + ((UInt32)b & MaskBits[e]);
                    _context.DumpBits(ref b, ref k, (int)e);
                    //DUMPBITS((int)e);

                    /* decode distance of block to copy */
                    _context.NeedBits(ref b, ref k, (int)bd);
                    //NEEDBITS((int)bd);

                    t = td.HuftList[(int)((UInt32)b & md)];
                    e = t.ExtraBits;
                    _inflate_codes_i2 = 0;

                    if (e > 16)
                    {
                        do
                        {
                            if (e == 99)
                            {
                                throw new Exception($"{nameof(InflateCodes)}: unused code");
                            }

                            _context.DumpBits(ref b, ref k, (int)t.CodeBits);
                            //DUMPBITS(t->CodeBits);
                            e -= 16;
                            _context.NeedBits(ref b, ref k, (int)e);
                            //NEEDBITS((int)e);

                            // end body
                            //t = t->Next + ((UInt32)b & MaskBits[e]);
                            t = t!.Next!.HuftList[(int)((UInt32)b & MaskBits[e])];
                            e = t.ExtraBits;
                            _inflate_codes_i2++;
                        } while (e > 16);
                    }

                    _context.DumpBits(ref b, ref k, (int)t.CodeBits);
                    //DUMPBITS(t->CodeBits);
                    _context.NeedBits(ref b, ref k, (int)e);
                    //NEEDBITS((int)e);
                    d = w - t.LengthBase - ((UInt32)b & MaskBits[e]);
                    _context.DumpBits(ref b, ref k, (int)e);
                    //DUMPBITS((int)e);
                    // Tracevv

                    /* do the copy */
                    do
                    {
                        d &= WSIZE - 1;
                        e = WSIZE - (d > w ? d : w);
                        e = e > n ? n : e;
                        n -= e;

                        if (w - d >= e)         /* (this test assumes unsigned comparison) */
                        {
                            Array.Copy(_context.Window, d, _context.Window, w, e);
                            w += e;
                            d += e;

                        }
                        else                      /* do it slow to avoid memcpy() overlap */
                        {
                            do
                            {
                                _context.Window[w++] = _context.Window[d++];
                                //Tracevv((stderr, "%c", slide[w - 1]));
                            } while (--e > 0);
                        }

                        if (w == WSIZE)
                        {
                            FlushOutput((int)w);
                            w = 0;
                        }
                    } while (n > 0);
                }
            }

            /* restore the globals from the locals */
            _wp = (int)w;                       /* restore global window pointer */
            _bitBuffer = b;                       /* restore global bit buffer */
            _bk = k;

            /* done */
            //success
        }

        /// <summary>
        /// "decompress" an inflated type 0 (stored) block.
        /// </summary>
        /// <returns></returns>
        private int InflateStored()
        {
            UInt32 n;           /* number of bytes in block */
            UInt32 w;           /* current window position */
            UInt64 b;       /* bit buffer */
            UInt32 k;  /* number of bits in bit buffer */

            /* make local copies of globals */
            b = _bitBuffer;                       /* initialize bit buffer */
            k = _bk;
            w = (UInt32)_wp;                       /* initialize window position */

            /* go to byte boundary */
            n = k & 7;
            _context.DumpBits(ref b, ref k, (int)n);
            //DUMPBITS((int)n);

            /* get the length and its complement */
            //NEEDBITS(16);
            _context.NeedBits(ref b, ref k, 16);
            n = ((UInt32)b & 0xffff);
            _context.DumpBits(ref b, ref k, 16);
            //DUMPBITS(16);
            _context.NeedBits(ref b, ref k, 16);
            //NEEDBITS(16);
            if (n != (UInt32)((~b) & 0xffff))
            {
                return 1;                   /* error in compressed data */
            }
            _context.DumpBits(ref b, ref k, 16);
            //DUMPBITS(16);

            /* read and output the compressed data */
            while (n-- > 0)
            {
                //NEEDBITS(8);
                _context.NeedBits(ref b, ref k, 8);
                _context.Window[w++] = (byte)b;
                if (w == WSIZE)
                {
                    FlushOutput((int)w);
                    w = 0;
                }
                _context.DumpBits(ref b, ref k, 8);
                //DUMPBITS(8);
            }

            /* restore the globals from the locals */
            _wp = (int)w;                       /* restore global window pointer */
            _bitBuffer = b;                       /* restore global bit buffer */
            _bk = k;

            return 0;
        }

        /// <summary>
        /// decompress an inflated type 1 (fixed Huffman codes) block.  We should
        /// either replace this with a custom decoder, or at least precompute the
        /// Huffman tables.
        /// </summary>
        /// <returns></returns>
        private void InflateFixed()
        {
            int i;                /* temporary variable */
            HuftTable tl = new HuftTable();      /* literal/length code table */
            HuftTable td = new HuftTable();      /* distance code table */
            int bl;               /* lookup bits for tl */
            int bd;               /* lookup bits for td */
            UInt32[] l = new UInt32[288];      /* length list for HuftBuild */
            HuftBuildResult buildResult;

            /* set up literal table */
            for (i = 0; i< 144; i++)
            {
                l[i] = 8;
            }

            for (; i< 256; i++)
            {
                l[i] = 9;
            }

            for (; i< 280; i++)
            {
                l[i] = 7;
            }

            for (; i< 288; i++)          /* make a complete, but wrong code set */
            {
                l[i] = 8;
            }

            bl = 7;

            buildResult = HuftBuild(l, 288, 257, LiteralCodesLengths, LiteralCodesExtraBits, ref tl, ref bl);
            if (buildResult != HuftBuildResult.Success)
            {
                throw new Exception($"{nameof(InflateFixed)}: incomplete table");
            }

            /* set up distance table */
            for (i = 0; i< 30; i++)      /* make an incomplete code set */
            { 
                l[i] = 5;
            }

            bd = 5;
            buildResult = HuftBuild(l, 30, 0, DistanceCodesCopyOffsets, DistanceCodesExtraBits, ref td, ref bd);
            
            // This may not return success, but an incomplete table is allowed.
            if (buildResult == HuftBuildResult.Error)
            {
                throw new Exception($"{nameof(InflateFixed)}: error via {nameof(HuftBuild)}");
            }

            /* decompress until an end-of-block code */
            InflateCodes(tl, td, bl, bd);

            /* free the decoding tables, return */
            // success.
        }

        /// <summary>
        /// Decompress an inflated type 2 (dynamic Huffman codes) block.
        /// </summary>
        /// <returns></returns>
        private void InflateDynamic()
        {
            int i;                /* temporary variables */
            UInt32 j;
            UInt32 l;           /* last length */
            UInt32 m;           /* mask for bit lengths table */
            UInt32 n;           /* number of lengths to get */
            HuftTable tl = new HuftTable();      /* literal/length code table */
            HuftTable td = new HuftTable();      /* distance code table */
            int bl;               /* lookup bits for tl */
            int bd;               /* lookup bits for td */
            UInt32 nb;          /* number of bit length codes */
            UInt32 nl;          /* number of literal/length codes */
            UInt32 nd;          /* number of distance codes */
            UInt32[] ll = new UInt32[286 + 30];  /* literal/length and distance code lengths */
            UInt64 b;       /* bit buffer */
            UInt32 k;  /* number of bits in bit buffer */
            HuftBuildResult buildResult;

            _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"ENTER {nameof(InflateDynamic)}");

            /* make local bit buffer */
            b = _bitBuffer;
            k = _bk;

            /* read in table lengths */
            _context.NeedBits(ref b, ref k, 5);
            //NEEDBITS(5);
            nl = 257 + ((UInt32) b & 0x1f);      /* number of literal/length codes */
            _context.DumpBits(ref b, ref k, 5);
            //DUMPBITS(5);
            _context.NeedBits(ref b, ref k, 5);
            //NEEDBITS(5);
            nd = 1 + ((UInt32) b & 0x1f);        /* number of distance codes */
            _context.DumpBits(ref b, ref k, 5);
            //DUMPBITS(5);
            _context.NeedBits(ref b, ref k, 4);
            //NEEDBITS(4);
            nb = 4 + ((UInt32) b & 0xf);         /* number of bit length codes */
            _context.DumpBits(ref b, ref k, 4);
            //DUMPBITS(4);

            if (nl > 286 || nd > 30)
            {
                // bad lengths
                throw new Exception($"{nameof(InflateDynamic)}: bad lengths");
            }

            /* read in bit-length-code lengths */
            for (j = 0; j<nb; j++)
            {
                _context.NeedBits(ref b, ref k, 3);
                //NEEDBITS(3);
                ll[Border[j]] = (UInt32) b & 7;
                _context.DumpBits(ref b, ref k, 3);
                //DUMPBITS(3);
            }

            for (; j< 19; j++)
            {
                ll[Border[j]] = 0;
            }

            /* build decoding table for trees--single level, 7 bit lookup */
            bl = 7;
            buildResult = HuftBuild(ll, 19, 19, null, null, ref tl, ref bl);
            if (buildResult != HuftBuildResult.Success)
            {
                throw new Exception($"{nameof(InflateDynamic)}: error via {nameof(HuftBuild)}");
            }

            /* read in literal and distance code lengths */
            n = nl + nd;
            m = MaskBits[bl];
            i = 0;
            l = 0;

            _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : n = {n}");
            _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : m = {m}");

            while ((UInt32)i < n)
            {
                Huft td_while = null;

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : while loop. i = {i}, n = {n}");

                _context.NeedBits(ref b, ref k, (int)bl);
                //NEEDBITS(bl);
                //td = tl.HuftList[(int)((UInt32)b & m)];
                td_while = tl.HuftList[(int)((UInt32)b & m)];
                j = td_while.CodeBits;

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : td.id = {td_while.Id}");
                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : j (1) = {j}");

                _context.DumpBits(ref b, ref k, (int)j);
                //DUMPBITS((int)j);
                j = td_while.LengthBase;

                _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"{nameof(InflateDynamic)} : j (2) = {j}");

                if (j < 16)                 /* length of code in bits (0..15) */
                {
                    ll[i++] = l = j;          /* save last length in l */
                }
                else if (j == 16)           /* repeat last length 3 to 6 times */
                {
                    _context.NeedBits(ref b, ref k, 2);
                    //NEEDBITS(2);
                    j = 3 + ((UInt32)b & 3);
                    _context.DumpBits(ref b, ref k, (int)2);
                    //DUMPBITS(2);

                    if ((UInt32)i + j > n)
                    {
                        throw new Exception($"{nameof(InflateDynamic)}: unspecified error");
                    }

                    while (j-- > 0)
                    {
                        ll[i++] = l;
                    }
                }
                else if (j == 17)           /* 3 to 10 zero length codes */
                {
                    _context.NeedBits(ref b, ref k, 3);
                    //NEEDBITS(3);
                    j = 3 + ((UInt32)b & 7);
                    _context.DumpBits(ref b, ref k, (int)3);
                    //DUMPBITS(3);

                    if ((UInt32)i + j > n)
                    {
                        throw new Exception($"{nameof(InflateDynamic)}: unspecified error");
                    }

                    while (j-- > 0)
                    {
                        ll[i++] = 0;
                    }

                    l = 0;
                }
                else                        /* j == 18: 11 to 138 zero length codes */
                {
                    _context.NeedBits(ref b, ref k, 7);
                    //NEEDBITS(7);
                    j = 11 + ((UInt32)b & 0x7f);
                    _context.DumpBits(ref b, ref k, (int)7);
                    //DUMPBITS(7);

                    if ((UInt32)i + j > n)
                    {
                        throw new Exception($"{nameof(InflateDynamic)}: unspecified error");
                    }

                    while (j-- > 0)
                    {
                        ll[i++] = 0;
                    }

                    l = 0;
                }
            }

            /* free decoding table for trees */

            /* restore the global bit buffer */
            _bitBuffer = b;
            _bk = k;

            /* build the decoding tables for literal/length and distance codes */
            bl = _lbits;
            try
            {
                HuftBuild(ll, nl, 257, LiteralCodesLengths, LiteralCodesExtraBits, ref tl, ref bl);
            }
            catch (Exception e)
            {
                throw new Exception("Incomplete literal tree", e);
            }

            bd = _dbits;

            var templl = ll.Skip((int)nl).ToArray();
            try
            {
                HuftBuild(templl, nd, 0, DistanceCodesCopyOffsets, DistanceCodesExtraBits, ref td, ref bd);
            }
            catch (Exception e)
            {
                throw new Exception("Incomplete literal tree", e);
            }

            /* decompress until an end-of-block code */
            InflateCodes(tl, td, bl, bd);

            /* free the decoding tables, return */
            // success

            _context.Log(Microsoft.Extensions.Logging.LogLevel.Trace, $"EXIT {nameof(InflateDynamic)}");
        }

        private void InflateBlock(ref int e)
        {
            UInt32 t;           /* block type */
            UInt64 b;       /* bit buffer */
            UInt32 k;  /* number of bits in bit buffer */

            /* make local bit buffer */
            b = _bitBuffer;
            k = _bk;

            /* read in last block bit */
            _context.NeedBits(ref b, ref k, 1);
            //NEEDBITS(1);
            e = (int)b & 1;
            _context.DumpBits(ref b, ref k, (int)1);
            //DUMPBITS(1);

            /* read in block type */
            _context.NeedBits(ref b, ref k, 2);
            //NEEDBITS(2);
            t = (UInt32)b & 3;
            _context.DumpBits(ref b, ref k, (int)2);
            //DUMPBITS(2);

            /* restore the global bit buffer */
            _bitBuffer = b;
            _bk = k;

            /* inflate that block type */
            if (t == 2)
            {
                InflateDynamic();
                return;
            }
            else if (t == 0)
            {
                InflateStored();
                return;
            }
            else if (t == 1)
            {
                InflateFixed();
                return;
            }

            /* bad block type */
            throw new Exception("invalid compressed data--format violated");
        }

        private int InflateEntryMain()
        {
            int e = 0;                /* last block flag */
            int h;           /* maximum struct huft's malloc'ed */

            /* initialize window, bit buffer */
            _wp = 0;
            _bk = 0;
            _bitBuffer = 0;

            /* decompress until the last block */
            h = 0;
            do
            {
                _hufts = 0;

                InflateBlock(ref e);

                if (_hufts > h)
                {
                    h = _hufts;
                }
            } while (e == 0);

            /* Undo too much lookahead. The next read will be byte aligned so we
             * can discard unused bits in the last meaningful byte.
             */
            while (_bk >= 8)
            {
                _bk -= 8;
                _context.Inptr--;
            }

            /* flush out slide */
            FlushOutput(_wp);

            /* return success */
            return 0;
        }

        private void FlushOutput(int w)
        {
            _context.FlushOutput(w);
        }
    }
}
