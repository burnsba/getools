﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;
using Getools.Lib.Game;

namespace Getools.Lib
{
    /// <summary>
    /// Bit fiddling helper functions.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1119:Statement should not use unnecessary parenthesis", Justification = "Justification")]
    public static class BitUtility
    {
        /// <summary>
        /// Converts 32 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert. If null, converts to zero.</param>
        public static void InsertPointer32Little(byte[] arr, int index, int? value)
        {
            if (!value.HasValue)
            {
                arr[index + 0] = 0;
                arr[index + 1] = 0;
                arr[index + 2] = 0;
                arr[index + 3] = 0;
            }
            else
            {
                arr[index + 0] = (byte)(value & 0xff);
                arr[index + 1] = (byte)((value >> 8) & 0xff);
                arr[index + 2] = (byte)((value >> 16) & 0xff);
                arr[index + 3] = (byte)((value >> 24) & 0xff);
            }
        }

        /// <summary>
        /// Converts 32 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert. If null, converts to zero.</param>
        public static void InsertPointer32Big(byte[] arr, int index, int? value)
        {
            if (!value.HasValue)
            {
                arr[index + 0] = 0;
                arr[index + 1] = 0;
                arr[index + 2] = 0;
                arr[index + 3] = 0;
            }
            else
            {
                arr[index + 3] = (byte)(value & 0xff);
                arr[index + 2] = (byte)((value >> 8) & 0xff);
                arr[index + 1] = (byte)((value >> 16) & 0xff);
                arr[index + 0] = (byte)((value >> 24) & 0xff);
            }
        }

        /// <summary>
        /// Converts string to constant length byte array and inserts into array.
        /// If the value is longer than the specified length it is truncated.
        /// If the value is shorter than the specified length it is zero padded.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index of array to insert value at.</param>
        /// <param name="value">String to insert.</param>
        /// <param name="targetStringLength">Specified string length.</param>
        public static void InsertString(byte[] arr, int index, string value, int targetStringLength)
        {
            // not zero terminated!
            var strBytes = Encoding.ASCII.GetBytes(value);

            // created zero'd holding array
            var t = Enumerable.Repeat<byte>(0, targetStringLength).ToArray();

            // copy string into zero'd array
            var strCopyLen = System.Math.Min(value.Length, targetStringLength);
            Array.Copy(strBytes, t, strCopyLen);

            // copy result into destination
            Array.Copy(t, 0, arr, index, targetStringLength);
        }

        /// <summary>
        /// Returns ASCII byte encoding of string.
        /// </summary>
        /// <param name="s">String to convert. Must be non-null.</param>
        /// <param name="appendZero">
        /// Flag to indicate whether a '\0' character should be appended to the end of the string.
        /// By default, C# will not include a terminating zero when getting string as bytes.
        /// </param>
        /// <returns>String as byte array.</returns>
        public static byte[] StringToBytes(string s, bool appendZero)
        {
            return StringToBytesAlign(s, appendZero, -1, -1);
        }

        /// <summary>
        /// Returns ASCII byte encoding of string.
        /// </summary>
        /// <param name="s">String to convert. Must be non-null.</param>
        /// <param name="appendZero">
        /// Flag to indicate whether a '\0' character should be appended to the end of the string.
        /// By default, C# will not include a terminating zero when getting string as bytes.
        /// </param>
        /// <param name="prependBytesCount">Optional parameter, number of '\0' characters to prepend before string.</param>
        /// <param name="appendBytesCount">Optional parameter, number of '\0' characters to append after string.</param>
        /// <returns>String as byte array.</returns>
        public static byte[] StringToBytesPad(string s, bool appendZero, int prependBytesCount = 0, int appendBytesCount = 0)
        {
            if (object.ReferenceEquals(null, s))
            {
                throw new ArgumentException("String cannot be null");
            }

            if (prependBytesCount < 0)
            {
                throw new ArgumentException($"{nameof(prependBytesCount)} must be non-negative.");
            }

            if (appendBytesCount < 0)
            {
                throw new ArgumentException($"{nameof(appendBytesCount)} must be non-negative.");
            }

            var resultLength = prependBytesCount + s.Length + appendBytesCount;
            if (appendZero)
            {
                resultLength++;
            }

            var result = Enumerable.Repeat<byte>(0, resultLength).ToArray();

            Array.Copy(Encoding.ASCII.GetBytes(s), 0, result, prependBytesCount, s.Length);

            return result;
        }

        /// <summary>
        /// Returns ASCII byte encoding of string.
        /// </summary>
        /// <param name="s">String to convert. Must be non-null.</param>
        /// <param name="appendZero">
        /// Flag to indicate whether a '\0' character should be appended to the end of the string.
        /// If alignment adjusts the return length to include additional '\0' characters then
        /// this value is ignored.
        /// By default, C# will not include a terminating zero when getting string as bytes.
        /// </param>
        /// <param name="alignSize">Align size in bytes. 0 or 1 indicate no alignment, 4 is for (MIPS) word alignment, etc.</param>
        /// <param name="currentAddress">Current address used to determine alignment.</param>
        /// <returns>String as byte array.</returns>
        public static byte[] StringToBytesAlign(string s, bool appendZero, int alignSize, int currentAddress)
        {
            if (object.ReferenceEquals(null, s))
            {
                throw new ArgumentException("String cannot be null");
            }

            int adjust = 0;

            if (currentAddress > -1 && alignSize > 1)
            {
                var endAddress = BitUtility.AlignToWidth(currentAddress + alignSize, alignSize);
                adjust = endAddress - currentAddress;
            }

            var resultLength = adjust + s.Length;
            if (appendZero && adjust == 0)
            {
                resultLength++;
            }

            var result = Enumerable.Repeat<byte>(0, resultLength).ToArray();

            Array.Copy(Encoding.ASCII.GetBytes(s), result, s.Length);

            return result;
        }

        /// <summary>
        /// Converts 32 bit value to LSB using the lower 3 bytes and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void InsertLower24Little(byte[] arr, int index, int value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
        }

        /// <summary>
        /// Converts 32 bit value to MSB using the lower 3 bytes and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void InsertLower24Big(byte[] arr, int index, int value)
        {
            arr[index + 2] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 0] = (byte)((value >> 16) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void InsertShortLittle(byte[] arr, int index, short value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void InsertShortBig(byte[] arr, int index, short value)
        {
            arr[index + 1] = (byte)(value & 0xff);
            arr[index + 0] = (byte)((value >> 8) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void InsertShortBig(byte[] arr, int index, ushort value)
        {
            arr[index + 1] = (byte)(value & 0xff);
            arr[index + 0] = (byte)((value >> 8) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert16Little(byte[] arr, int index, Int16 value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert16Little(byte[] arr, int index, UInt16 value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert16Big(byte[] arr, int index, Int16 value)
        {
            arr[index + 0] = (byte)((value >> 8) & 0xff);
            arr[index + 1] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Converts 16 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert16Big(byte[] arr, int index, UInt16 value)
        {
            arr[index + 0] = (byte)((value >> 8) & 0xff);
            arr[index + 1] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Converts 32 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert32Little(byte[] arr, int index, int value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
            arr[index + 3] = (byte)((value >> 24) & 0xff);
        }

        /// <summary>
        /// Converts 32 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert32Little(byte[] arr, int index, uint value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
            arr[index + 3] = (byte)((value >> 24) & 0xff);
        }

        /// <summary>
        /// Converts 32 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert32Big(byte[] arr, int index, int value)
        {
            arr[index + 0] = (byte)((value >> 24) & 0xff);
            arr[index + 1] = (byte)((value >> 16) & 0xff);
            arr[index + 2] = (byte)((value >> 8) & 0xff);
            arr[index + 3] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Converts 32 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert32Big(byte[] arr, int index, uint value)
        {
            arr[index + 0] = (byte)((value >> 24) & 0xff);
            arr[index + 1] = (byte)((value >> 16) & 0xff);
            arr[index + 2] = (byte)((value >> 8) & 0xff);
            arr[index + 3] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Converts 64 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert64Little(byte[] arr, int index, long value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
            arr[index + 3] = (byte)((value >> 24) & 0xff);
            arr[index + 4] = (byte)((value >> 32) & 0xff);
            arr[index + 5] = (byte)((value >> 40) & 0xff);
            arr[index + 6] = (byte)((value >> 48) & 0xff);
            arr[index + 7] = (byte)((value >> 56) & 0xff);
        }

        /// <summary>
        /// Converts 64 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert64Little(byte[] arr, int index, ulong value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
            arr[index + 3] = (byte)((value >> 24) & 0xff);
            arr[index + 4] = (byte)((value >> 32) & 0xff);
            arr[index + 5] = (byte)((value >> 40) & 0xff);
            arr[index + 6] = (byte)((value >> 48) & 0xff);
            arr[index + 7] = (byte)((value >> 56) & 0xff);
        }

        /// <summary>
        /// Converts 64 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert64Big(byte[] arr, int index, long value)
        {
            arr[index + 0] = (byte)((value >> 56) & 0xff);
            arr[index + 1] = (byte)((value >> 48) & 0xff);
            arr[index + 2] = (byte)((value >> 40) & 0xff);
            arr[index + 3] = (byte)((value >> 32) & 0xff);
            arr[index + 4] = (byte)((value >> 24) & 0xff);
            arr[index + 5] = (byte)((value >> 16) & 0xff);
            arr[index + 6] = (byte)((value >> 8) & 0xff);
            arr[index + 7] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Converts 64 bit value to MSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert64Big(byte[] arr, int index, ulong value)
        {
            arr[index + 0] = (byte)((value >> 56) & 0xff);
            arr[index + 1] = (byte)((value >> 48) & 0xff);
            arr[index + 2] = (byte)((value >> 40) & 0xff);
            arr[index + 3] = (byte)((value >> 32) & 0xff);
            arr[index + 4] = (byte)((value >> 24) & 0xff);
            arr[index + 5] = (byte)((value >> 16) & 0xff);
            arr[index + 6] = (byte)((value >> 8) & 0xff);
            arr[index + 7] = (byte)(value & 0xff);
        }

        /// <summary>
        /// Reads two bytes from the stream as MSB.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Value.</returns>
        public static short Read16Big(BinaryReader stream)
        {
            short s = stream.ReadByte();
            s <<= 8;
            s |= (short)stream.ReadByte();

            return s;
        }

        /// <summary>
        /// Reads 2 bytes from a byte array as big endian 16-bit int.
        /// </summary>
        /// <param name="arr">Array to read.</param>
        /// <param name="index">Starting index in array.</param>
        /// <returns>Int.</returns>
        public static Int16 Read16Big(byte[] arr, int index)
        {
            if (index < 0)
            {
                throw new ArgumentException("Array index must be non-negative integer");
            }

            if (index + 2 > arr.Length)
            {
                throw new EndOfStreamException("Reading 2 bytes from array exceeds array length");
            }

            Int16 i = arr[index];
            i <<= 8;
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
            i |= arr[index + 1];
#pragma warning restore CS0675 // Bitwise-or operator used on a sign-extended operand

            return i;
        }

        /// <summary>
        /// Reads two bytes from the stream as LSB.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Value.</returns>
        public static short Read16Little(BinaryReader stream)
        {
            short s = stream.ReadByte();
            s |= (short)((short)(stream.ReadByte()) << 8);

            return s;
        }

        /// <summary>
        /// Reads four bytes from the stream as MSB.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Value.</returns>
        public static Int32 Read32Big(BinaryReader stream)
        {
            Int32 i = stream.ReadByte();
            i <<= 8;
            i |= (Int32)stream.ReadByte();
            i <<= 8;
            i |= (Int32)stream.ReadByte();
            i <<= 8;
            i |= (Int32)stream.ReadByte();

            return i;
        }

        /// <summary>
        /// Reads 4 bytes from a byte array as big endian 32-bit int.
        /// </summary>
        /// <param name="arr">Array to read.</param>
        /// <param name="index">Starting index in array.</param>
        /// <returns>Int.</returns>
        public static Int32 Read32Big(byte[] arr, int index)
        {
            if (index < 0)
            {
                throw new ArgumentException("Array index must be non-negative integer");
            }

            if (index + 4 > arr.Length)
            {
                throw new EndOfStreamException("Reading 4 bytes from array exceeds array length");
            }

            Int32 i = arr[index];
            i <<= 8;
            i |= arr[index + 1];
            i <<= 8;
            i |= arr[index + 2];
            i <<= 8;
            i |= arr[index + 3];

            return i;
        }

        /// <summary>
        /// Reads four bytes from the stream as LSB.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Value.</returns>
        public static Int32 Read32Little(BinaryReader stream)
        {
            Int32 i = stream.ReadByte();
            i |= (Int32)((Int32)(stream.ReadByte()) << 8);
            i |= (Int32)((Int32)(stream.ReadByte()) << 16);
            i |= (Int32)((Int32)(stream.ReadByte()) << 24);

            return i;
        }

        /// <summary>
        /// Swap endianess on 16 bit value.
        /// </summary>
        /// <param name="s">Value.</param>
        /// <returns>Swapped value.</returns>
        public static short Swap(short s)
        {
            return (short)(((s & 0xff) << 8) | ((s & 0xff00) >> 8));
        }

        /// <summary>
        /// Swap endianess on 32 bit value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>Swapped value.</returns>
        public static UInt32 Swap(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// Swap endianess on 32 bit value.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <returns>Swapped value.</returns>
        public static Int32 Swap(Int32 value)
        {
            var v = (UInt32)value;

            return (Int32)(
                (v & 0x000000FFU) << 24 | (v & 0x0000FF00U) << 8 |
                (v & 0x00FF0000U) >> 8 | (v & 0xFF000000U) >> 24);
        }

        /// <summary>
        /// Treats value as if it were internal representation of float and returns the result.
        /// </summary>
        /// <param name="i">Value.</param>
        /// <returns>Cast value.</returns>
        public static Single CastToFloat(int i)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
        }

        /// <summary>
        /// Returns the internal representation of float value.
        /// </summary>
        /// <param name="f">Value.</param>
        /// <returns>Cast value.</returns>
        public static Int32 CastToInt32(Single f)
        {
            return BitConverter.ToInt32(BitConverter.GetBytes((Single)f), 0);
        }

        /// <summary>
        /// Returns the value incremented to the next closest multiple of 16.
        /// If the current value is a multiple of 16 then that is returned.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Address, or next largest multiple of 16.</returns>
        public static int Align16(int address)
        {
            int next16 = 0;
            if ((address % 16) != 0)
            {
                next16 = ((int)(address / 16) + 1) * 16;
            }
            else
            {
                next16 = address;
            }

            return next16;
        }

        /// <summary>
        /// Returns the value incremented to the next closest multiple of 8.
        /// If the current value is a multiple of 8 then that is returned.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Address, or next largest multiple of 8.</returns>
        /// <remarks>
        /// AKA AlignDWord
        /// </remarks>
        public static int Align8(int address)
        {
            int next8 = 0;
            if ((address % 8) != 0)
            {
                next8 = ((int)(address / 8) + 1) * 8;
            }
            else
            {
                next8 = address;
            }

            return next8;
        }

        /// <summary>
        /// Returns the value incremented to the next closest multiple of 4.
        /// If the current value is a multiple of 4 then that is returned.
        /// </summary>
        /// <param name="address">Address to check.</param>
        /// <returns>Address, or next largest multiple of 4.</returns>
        /// <remarks>
        /// AKA AlignWord
        /// </remarks>
        public static int Align4(int address)
        {
            int next4 = 0;
            if ((address % 4) != 0)
            {
                next4 = ((int)(address / 4) + 1) * 4;
            }
            else
            {
                next4 = address;
            }

            return next4;
        }

        /// <summary>
        /// Parameterized byte alignment.
        /// </summary>
        /// <param name="address">Address to align to the nearest width.</param>
        /// <param name="width">Size in bytes of alignment. Size of 0 or 1 will both return the <paramref name="address"/>.</param>
        /// <returns>Address, or next nearest multiple, or throws <see cref="NotSupportedException"/>.</returns>
        public static int AlignToWidth(int address, int width)
        {
            switch (width)
            {
                case 0:
                case 1:
                    return address;

                case 4:
                    return Align4(address);

                case 8:
                    return Align8(address);

                case 16:
                    return Align16(address);

                default:
                    throw new NotSupportedException($"Cannot align address={address} to width={width} bytes");
            }
        }

        /// <summary>
        /// Reads from array one character at a time until reading '\0'.
        /// Returns the string, without terminating zero.
        /// Reads up to array length or <paramref name="maxStringLength"/>.
        /// If the first character read is '\0' then <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="arr">Array to read.</param>
        /// <param name="index">Starting index to read from.</param>
        /// <param name="maxStringLength">Max string length to read.</param>
        /// <returns>String or string.empty.</returns>
        public static string ReadString(byte[] arr, int index, int maxStringLength)
        {
            if (maxStringLength <= 0)
            {
                throw new ArgumentException(nameof(maxStringLength));
            }

            if (index < 0)
            {
                throw new ArgumentException(nameof(index));
            }

            if (index > arr.Length)
            {
                throw new ArgumentException($"{nameof(index)} exceeds array length");
            }

            int endPos = System.Math.Min(index + maxStringLength, arr.Length - 1);

            var sb = new StringBuilder();

            while (index < endPos)
            {
                if (arr[index] > 0)
                {
                    sb.Append((char)arr[index]);
                }
                else
                {
                    break;
                }

                index++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// UNFLoader calc_padsize. Used with everdrive.
        /// </summary>
        /// <param name="size">Input value.</param>
        /// <returns>UNFLoader adjusted size.</returns>
        public static UInt32 CalculatePadsize(UInt32 size)
        {
            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            size++;

            return size;
        }
    }
}
