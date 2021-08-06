﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var strCopyLen = Math.Min(value.Length, targetStringLength);
            Array.Copy(strBytes, t, strCopyLen);

            // copy result into destination
            Array.Copy(t, 0, arr, index, targetStringLength);
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
    }
}
