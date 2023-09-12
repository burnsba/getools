using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Reflection.Metadata.BlobBuilder;

namespace GzipSharpLib
{
    /// <summary>
    /// This context file roughly corresponds to gzip.c (GPL3 version)
    /// </summary>
    public class Context : IDisposable
    {
        private static byte[] GZIP_MAGIC = new byte[] { 0x1f, 0x8b };
        private static byte[] OLD_GZIP_MAGIC = new byte[] { 0x1f, 0x9e };
        private static byte[] RARE_1172_MAGIC = new byte[] { 0x11, 0x72 };

        private const int ASCII_FLAG = 0x01; /* bit 0 set: file probably ascii text */
        private const int CONTINUATION = 0x02; /* bit 1 set: continuation of multi-part gzip file */
        private const int EXTRA_FIELD = 0x04; /* bit 2 set: extra field present */
        private const int ORIG_NAME = 0x08; /* bit 3 set: original file name present */
        private const int COMMENT = 0x10;/* bit 4 set: file comment present */
        private const int ENCRYPTED = 0x20; /* bit 5 set: file is encrypted */
        private const int RESERVED = 0xC0; /* bit 6,7:   reserved */

        private const int WSIZE = 0x8000;

        private const int OUT_BUFFER_GROW = (1024 * 1024);

        /// <summary>
        /// Sliding window and suffix table.
        /// gzip.h
        /// </summary>
        private byte[] _window = new byte[2 * WSIZE];

        /// <summary>
        /// The entire contents of the deflate result will be stored in this array.
        /// It is initially set to null.
        /// Bytes will be appended one byte at a time, according to position
        /// <see cref="_out_buffer_pos"/>. If the next byte will not fit (or array is null),
        /// this array will be resized, adding an additional <see cref="OUT_BUFFER_GROW"/>
        /// bytes.
        /// The current allocted size is tracked in <see cref="_out_buffer_size"/>.
        /// </summary>
        private byte[]? _out_buffer = null;
        private int _out_buffer_size = 0;
        private int _out_buffer_pos = 0;

        private byte[]? _inbuf = null;

        /// <summary>
        /// Compression method.
        /// Strongly typed result from gzip source.
        /// </summary>
        private CompressionMethod _method = CompressionMethod.Deflated;

        /// <summary>
        /// Program exit code.
        /// Strongly typed result from gzip source.
        /// </summary>
        private ReturnCode _exitCode = ReturnCode.Ok;

        /// <summary>
        /// Number of output bytes.
        /// </summary>
        private long _bytesOut;

        /// <summary>
        /// Bytes in output buffer.
        /// gzhp.h
        /// </summary>
        private uint _outcnt;

        private ILogger _logger;

        private int _getByteCallCount = 0;

        private bool _finished = false;

        /// <summary>
        /// Debug/trace log.
        /// Generates json output to match custom modification of gzip c source.
        /// </summary>
        public bool Trace { get; set; } = false;

        public MemoryStream Source { get; set; }

        public MemoryStream? Destination { get; set; } = null;

        /// <summary>
        /// Index of next byte to be processed in inbuf.
        /// </summary>
        /// <remarks>
        /// This needs to be scoped to be available to <see cref="Inflate"/>.
        /// </remarks>
        internal uint Inptr { get; set; }

        /// <summary>
        /// Sliding window.
        /// </summary>
        /// <remarks>
        /// This needs to be scoped to be available to <see cref="Inflate"/>.
        /// </remarks>
        internal byte[] Window { get => _window; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Context(ILogger log)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            _logger = log;
        }

        public ReturnCode Execute()
        {
            if (_finished)
            {
                return ReturnCode.AlreadyRan;
            }

            if (object.ReferenceEquals(null, Source))
            {
                throw new NullReferenceException($"{nameof(Source)} not set");
            }
            
            _inbuf = Source.ToArray();

            Inptr = 0;
            _outcnt = 0;
            _bytesOut = 0L;

            ReadGzipMeta();

            DoUnzip();

            if (object.ReferenceEquals(null, _out_buffer))
            {
                throw new NullReferenceException($"{nameof(_out_buffer)} not set during inflate");
            }

            if (_out_buffer_pos > 0)
            {
                var content = new byte[_out_buffer_pos];
                Array.Copy(_out_buffer, 0, content, 0, _out_buffer_pos);
                Destination = new MemoryStream(content);
            }
            else
            {
                throw new Exception($"{_out_buffer_pos} unset, no output found");
            }

            _finished = true;

            return _exitCode;
        }

        public void Log(LogLevel level, string msg)
        {
            _logger.Log(level, msg);
        }

        public void Dispose()
        {
            if (!object.ReferenceEquals(null, Destination))
            {
                Destination.Dispose();
            }

            _out_buffer = null;
            _inbuf = null;
        }

        /// <summary>
        /// Copy a single byte into the local output buffer.
        /// This resizes the buffer on demand.
        /// </summary>
        /// <param name="c"></param>
        internal void MyPutByte(byte c)
        {
            if (_out_buffer == null)
            {
                _out_buffer = new byte[OUT_BUFFER_GROW];
                _out_buffer_size = OUT_BUFFER_GROW;
            }
            else if (_out_buffer_pos + 1 >= _out_buffer_size)
            {
                byte[] new_out_buffer;
                int old_size;

                old_size = _out_buffer_size;
                _out_buffer_size += OUT_BUFFER_GROW;

                new_out_buffer = new byte[_out_buffer_size];
                Array.Copy(_out_buffer, new_out_buffer, old_size);

                _out_buffer = new_out_buffer;
            }

            _out_buffer[_out_buffer_pos++] = c;
        }

        /// <summary>
        /// Sets number of bytes to flush, then calls <see cref="FlushWindow"/>.
        /// </summary>
        /// <param name="w">Number of bytes to copy.</param>
        /// <remarks>
        /// flush_outbuf() from util.c
        /// </remarks>
        internal void FlushOutput(int w)
        {
            _outcnt = ((uint)w);
            FlushWindow();
        }

        /// <summary>
        /// Flushes <see cref="_window"/> to the local output buffer.
        /// </summary>
        /// <remarks>
        /// flush_window() from util.c
        /// </remarks>
        internal void FlushWindow()
        {
            if (_outcnt == 0)
            {
                return;
            }

            long remain = _outcnt;
            long index = 0;
            while (remain > 0)
            {
                MyPutByte(_window[index]);
                index++;
                remain--;
                _bytesOut++;
            }

            _outcnt = 0;
        }

        /// <summary>
        /// Gets the next byte from the input buffer.
        /// </summary>
        /// <returns>Next byte.</returns>
        /// <remarks>
        /// Port from c macro to function.
        /// gzip.h (inflate.c) get_byte()
        /// </remarks>
        internal byte GetByte()
        {
            if (object.ReferenceEquals(null, _inbuf))
            {
                throw new NullReferenceException($"{nameof(_inbuf)} is null, source not set?");
            }

            _getByteCallCount++;
            byte readval = _inbuf[Inptr];

            if (Trace)
            {
                _logger.Log(LogLevel.Trace, $"{nameof(GetByte)}: call={_getByteCallCount}, inptr={Inptr}, read=0x{readval:x2}");
            }
            Inptr++;
            return readval;
        }

        /// <summary>
        /// Pass through to <see cref="GetByte"/>.
        /// </summary>
        /// <returns>Next byte.</returns>
        /// <remarks>
        /// Port from c macro to function.
        /// inflate.c NEXTBYTE()
        /// </remarks>
        internal byte NextByte()
        {
            return GetByte();
        }

        /// <summary>
        /// Gets the corresponding number of bits from the input buffer.
        /// </summary>
        /// <returns>Bits.</returns>
        /// <remarks>
        /// Port from c macro to function.
        /// inflate.c NEEDBITS(n)
        /// 
        /// ---------
        /// 
        /// Macros for inflate() bit peeking and grabbing.
        /// 
        /// The usage is:
        /// 
        /// NEEDBITS(j)
        /// x = b & mask_bits[j];
        /// DUMPBITS(j)
        /// 
        /// where NEEDBITS makes sure that b has at least j bits in it, and
        /// DUMPBITS removes the bits from b.The macros use the variable k
        /// for the number of bits in b.Normally, b and k are register
        /// variables for speed, and are initialized at the beginning of a
        /// routine that uses these macros from a global bit buffer and count.
        /// 
        /// If we assume that EOB will be the longest code, then we will never
        /// ask for bits with NEEDBITS that are beyond the end of the stream.
        /// So, NEEDBITS should not read any more bytes than are needed to
        /// meet the request.  Then no bytes need to be "returned" to the buffer
        /// at the end of the last block.
        /// 
        /// However, this assumption is not true for fixed blocks--the EOB code
        /// is 7 bits, but the other literal / length codes can be 8 or 9 bits.
        /// (The EOB code is shorter than other codes because fixed blocks are
        /// generally short.So, while a block always has an EOB, many other
        /// literal / length codes have a significantly lower probability of
        /// showing up at all.)  However, by making the first table have a
        /// lookup of seven bits, the EOB code will be found in that first
        /// lookup, and so will not require that too many bits be pulled from
        /// the stream.
        /// </remarks>
        internal void NeedBits(ref UInt64 b, ref UInt32 k, int n)
        {
            if (Trace)
            {
                _logger.Log(LogLevel.Trace, $"NEEDBITS: n={n}, k={k}, b={b}");
            }

            while (k < n)
            {
                b |= (UInt64)(((UInt32)NextByte()) << (int)k);
                k += 8;
            }
        }

        /// <summary>
        /// Gets the corresponding number of bits from the input buffer.
        /// </summary>
        /// <returns>Bits.</returns>
        /// <remarks>
        /// Port from c macro to function.
        /// inflate.c NEEDBITS(n)
        /// 
        /// See remarks on <see cref="NeedBits(ref ulong, ref uint, int)"/>.
        /// </remarks>
        internal void DumpBits(ref UInt64 b, ref UInt32 k, int n)
        {
            b >>= (int)n;
            k -= (UInt32)n;
        }

        /// <summary>
        /// Checks the magic bytes of input and reads related metadata.
        /// Sets <see cref="_method"/>.
        /// </summary>
        /// <remarks>
        /// get_method(int) inflate.c
        /// </remarks>
        private void ReadGzipMeta()
        {
            byte flags;
            byte[] magic = new byte[2];
            ulong stamp;

            magic[0] = GetByte();
            magic[1] = GetByte();

            _method = CompressionMethod.DefaultUnknown;

            if (RARE_1172_MAGIC[0] == magic[0] && RARE_1172_MAGIC[1] == magic[1])
            {
                _method = CompressionMethod.Rare1172Deflated;
            }
            else if ((GZIP_MAGIC[0] == magic[0] && GZIP_MAGIC[1] == magic[1])
                || (OLD_GZIP_MAGIC[0] == magic[0] && OLD_GZIP_MAGIC[1] == magic[1]))
            {
                _method = (CompressionMethod)GetByte();
                flags = GetByte();

                stamp = (ulong)GetByte();
                stamp |= ((ulong)GetByte()) << 8;
                stamp |= ((ulong)GetByte()) << 16;
                stamp |= ((ulong)GetByte()) << 24;

                // Ignore extra flags for the moment
                GetByte();

                // Ignore OS type for the moment
                GetByte();

                if ((flags & ORIG_NAME) != 0)
                {
                    byte c;
                    do {
                        c = GetByte();
                    } while (c != 0);
                }
            }
            else
            {
                throw new Exception($"{nameof(ReadGzipMeta)}: could not match magic bytes: 0x{magic[1]:X2}{magic[0]:X2}");
            }
        }

        /// <summary>
        /// Entry point for deflate.
        /// Corresponds to `work = unzip`, unzip method in c source.
        /// </summary>
        /// <remarks>
        /// unzip(int, int) unzip.c
        /// </remarks>
        private void DoUnzip()
        {
            if (object.ReferenceEquals(null, _inbuf))
            {
                throw new NullReferenceException($"{nameof(_inbuf)} is null, source not set?");
            }

            ulong orig_len = 0;       /* original uncompressed length */
            int n;
            byte[] buf = new byte[8];        /* extended local header */

            if (_method == CompressionMethod.Deflated
                || _method == CompressionMethod.Rare1172Deflated)
            {
                var inflate = new Inflate(this);

                inflate.Go();
            }
            else
            {
                throw new Exception("internal error, invalid method");
            }

            for (n = 0; n < 8 && Inptr + 1 < _inbuf.Length; n++)
            {
                buf[n] = GetByte(); /* may cause an error if EOF */
            }

            orig_len = (ulong)BitConverter.ToInt32(buf, 4);

            /* Validate decompression */
            if (orig_len != (ulong)_bytesOut)
            {
                if (_method != CompressionMethod.Rare1172Deflated)
                {
                    throw new Exception("invalid compressed data--length error");
                }
            }
        }
    }
}
