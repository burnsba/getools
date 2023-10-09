using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gebug64.Win.Image
{
    /// <summary>
    /// Helper class to manage image data.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Convert a byte array of the N64 native framebuffer data into a friendly windows format.
        /// </summary>
        /// <param name="inData">Console framebuffer data in RGBA 5551 format.</param>
        /// <param name="width">Pixel width of framebuffer.</param>
        /// <param name="height">Pixel height of framebuffer.</param>
        /// <returns>Pixel data translated to windows ARGB 1555 format.</returns>
        public static byte[] GeRgba5551ToWindowsArgb1555(byte[] inData, int width, int height)
        {
            int outSize = width * height * sizeof(Int16);

            var outData = new byte[outSize];

            int readPos = 0;
            int destPos = 0;
            while (readPos < inData.Length && destPos < outSize)
            {
                var b1 = inData[readPos++];
                var b2 = inData[readPos++];

                int s16 = (int)(((int)b1 << 8) | (int)b2);

                int r = (int)((s16 >> 11) & 0x1f);
                int g = (int)((s16 >> 6) & 0x1f);
                int b = (int)((s16 >> 1) & 0x1f);
                int a = (int)((s16 >> 0) & 0x1);

                int destWord = (a & 0x1) << 15;
                destWord |= r << 10;
                destWord |= g << 5;
                destWord |= b << 0;

                outData[destPos++] = (byte)(destWord & 0xff);
                outData[destPos++] = (byte)((destWord >> 8) & 0xff);
            }

            return outData;
        }

        /// <summary>
        /// Converts a raw byte array into a managed object.
        /// </summary>
        /// <param name="data">Image pixel data.</param>
        /// <param name="dataPixelFormat">Data format of <paramref name="data"/>.</param>
        /// <param name="width">Pixel width of <paramref name="data"/>.</param>
        /// <param name="height">Pixel height of <paramref name="data"/>.</param>
        /// <returns>Managed <see cref="Bitmap"/>.</returns>
        public static Bitmap BitmapFromRaw(byte[] data, System.Drawing.Imaging.PixelFormat dataPixelFormat, int width, int height)
        {
            // Here create the Bitmap to the know height, width and format
            Bitmap bmp = new Bitmap(width, height, dataPixelFormat);

            // Create a BitmapData and Lock all pixels to be written.
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);

            // Copy the data from the byte array into BitmapData.Scan0
            Marshal.Copy(data, 0, bmpData.Scan0, data.Length);

            // Unlock the pixels
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        /// <summary>
        /// Converts a raw byte array into an image and saves to disk.
        /// </summary>
        /// <param name="data">Image pixel data.</param>
        /// <param name="dataPixelFormat">Data format of <paramref name="data"/>.</param>
        /// <param name="width">Pixel width of <paramref name="data"/>.</param>
        /// <param name="height">Pixel height of <paramref name="data"/>.</param>
        /// <param name="path">Path on disk to save file.</param>
        /// <param name="saveImageFormat">Format of image to save to.</param>
        public static void SaveRawToFile(byte[] data, System.Drawing.Imaging.PixelFormat dataPixelFormat, int width, int height, string path, ImageFormat saveImageFormat)
        {
            var bmp = BitmapFromRaw(data, dataPixelFormat, width, height);

            bmp.Save(path, saveImageFormat);
        }

        /// <summary>
        /// Converts a <see cref="System.Drawing.Bitmap"/> into an <see cref="BitmapImage"/> to be used by WPF.
        /// </summary>
        /// <param name="bitmap">Bitmap.</param>
        /// <returns>Bitmap.</returns>
        /// <remarks>
        /// https://stackoverflow.com/a/66957361/1462295 .
        /// </remarks>
        public static BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }
    }
}
