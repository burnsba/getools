using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Architecture;

namespace Getools.Lib
{
    public static class ByteExtractor
    {
        public static byte[] GetBytesEnumerable(object source)
        {
            var type = source.GetType();

            if (type == typeof(byte[]))
            {
                return (byte[])source;
            }
            else if (type == typeof(List<byte>))
            {
                return ((List<byte>)source).ToArray();
            }
            else if (type == typeof(IList<byte>))
            {
                return ((IList<byte>)source).ToArray();
            }
            else if (type == typeof(IEnumerable<byte>))
            {
                return ((IEnumerable<byte>)source).ToArray();
            }

            throw new NotSupportedException();
        }

        public static byte[] GetBytes(object source, int getByteCount, ByteOrder order)
        {
            var type = source.GetType();

            if (order == ByteOrder.LittleEndien)
            {
                if (type == typeof(int)
                  || type == typeof(Int32))
                {
                    return GetBytesExplicitLittle((int)source, getByteCount);
                }
                else if (type == typeof(uint)
                  || type == typeof(UInt32))
                {
                    return GetBytesExplicitLittle((uint)source, getByteCount);
                }
                else if (type == typeof(short)
                  || type == typeof(Int16))
                {
                    return GetBytesExplicitLittle((short)source, getByteCount);
                }
                else if (type == typeof(ushort)
                  || type == typeof(UInt16))
                {
                    return GetBytesExplicitLittle((ushort)source, getByteCount);
                }
                else if (type == typeof(byte))
                {
                    return GetBytesExplicitLittle((byte)source, getByteCount);
                }
                else if (type == typeof(sbyte))
                {
                    return GetBytesExplicitLittle((sbyte)source, getByteCount);
                }
            }
            else if (order == ByteOrder.LittleEndien)
            {
                if (type == typeof(int)
                  || type == typeof(Int32))
                {
                    return GetBytesExplicitBig((int)source, getByteCount);
                }
                else if (type == typeof(uint)
                  || type == typeof(UInt32))
                {
                    return GetBytesExplicitBig((uint)source, getByteCount);
                }
                else if (type == typeof(short)
                  || type == typeof(Int16))
                {
                    return GetBytesExplicitBig((short)source, getByteCount);
                }
                else if (type == typeof(ushort)
                  || type == typeof(UInt16))
                {
                    return GetBytesExplicitBig((ushort)source, getByteCount);
                }
                else if (type == typeof(byte))
                {
                    return GetBytesExplicitBig((byte)source, getByteCount);
                }
                else if (type == typeof(sbyte))
                {
                    return GetBytesExplicitBig((sbyte)source, getByteCount);
                }
            }

            throw new NotSupportedException();
        }

        private static byte[] GetBytesExplicitLittle(byte source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(byte source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitLittle(sbyte source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(sbyte source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitLittle(short source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                case 2: return new byte[] { (byte)(source >> 8), (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(short source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 8) };
                case 2: return new byte[] { (byte)(source >> 0), (byte)(source >> 8) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitLittle(ushort source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                case 2: return new byte[] { (byte)(source >> 8), (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(ushort source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 8) };
                case 2: return new byte[] { (byte)(source >> 0), (byte)(source >> 8) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitLittle(int source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                case 2: return new byte[] { (byte)(source >> 8), (byte)(source >> 0) };
                case 3: return new byte[] { (byte)(source >> 16), (byte)(source >> 8), (byte)(source >> 0) };
                case 4: return new byte[] { (byte)(source >> 24), (byte)(source >> 16), (byte)(source >> 8), (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(int source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 24) };
                case 2: return new byte[] { (byte)(source >> 16), (byte)(source >> 24) };
                case 3: return new byte[] { (byte)(source >> 8), (byte)(source >> 16), (byte)(source >> 24) };
                case 4: return new byte[] { (byte)(source >> 0), (byte)(source >> 8), (byte)(source >> 16), (byte)(source >> 24) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitLittle(uint source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 0) };
                case 2: return new byte[] { (byte)(source >> 8), (byte)(source >> 0) };
                case 3: return new byte[] { (byte)(source >> 16), (byte)(source >> 8), (byte)(source >> 0) };
                case 4: return new byte[] { (byte)(source >> 24), (byte)(source >> 16), (byte)(source >> 8), (byte)(source >> 0) };
                default: throw new NotSupportedException();
            }
        }

        private static byte[] GetBytesExplicitBig(uint source, int getByteCount)
        {
            switch (getByteCount)
            {
                case 1: return new byte[] { (byte)(source >> 24) };
                case 2: return new byte[] { (byte)(source >> 16), (byte)(source >> 24) };
                case 3: return new byte[] { (byte)(source >> 8), (byte)(source >> 16), (byte)(source >> 24) };
                case 4: return new byte[] { (byte)(source >> 0), (byte)(source >> 8), (byte)(source >> 16), (byte)(source >> 24) };
                default: throw new NotSupportedException();
            }
        }
    }
}
