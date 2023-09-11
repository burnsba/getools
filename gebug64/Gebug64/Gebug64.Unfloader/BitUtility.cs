using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    public static class BitUtility
    {
        /// <summary>
        /// Converts 32 bit value to LSB and inserts into array at index.
        /// </summary>
        /// <param name="arr">Array to insert value into.</param>
        /// <param name="index">Index to insert value at.</param>
        /// <param name="value">Value to insert.</param>
        public static void Insert32Little(byte[] arr, Int32 index, Int32 value)
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
        public static void Insert32Little(byte[] arr, Int32 index, UInt32 value)
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
        public static void Insert32Big(byte[] arr, Int32 index, Int32 value)
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
        public static void Insert32Big(byte[] arr, Int32 index, UInt32 value)
        {
            arr[index + 0] = (byte)((value >> 24) & 0xff);
            arr[index + 1] = (byte)((value >> 16) & 0xff);
            arr[index + 2] = (byte)((value >> 8) & 0xff);
            arr[index + 3] = (byte)(value & 0xff);
        }

        /// <summary>
        /// UNFLoader calc_padsize.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
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
    }
}
