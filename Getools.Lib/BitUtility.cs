using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib
{
    public static class BitUtility
    {
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

        public static void InsertLower24Little(byte[] arr, int index, int value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 2] = (byte)((value >> 16) & 0xff);
        }

        public static void InsertLower24Big(byte[] arr, int index, int value)
        {
            arr[index + 2] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
            arr[index + 0] = (byte)((value >> 16) & 0xff);
        }

        public static void InsertShortLittle(byte[] arr, int index, short value)
        {
            arr[index + 0] = (byte)(value & 0xff);
            arr[index + 1] = (byte)((value >> 8) & 0xff);
        }

        public static void InsertShortBig(byte[] arr, int index, short value)
        {
            arr[index + 1] = (byte)(value & 0xff);
            arr[index + 0] = (byte)((value >> 8) & 0xff);
        }
    }
}
