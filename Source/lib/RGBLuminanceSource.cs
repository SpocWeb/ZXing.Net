/*
* Copyright 2012 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace ZXing
{
    /// <summary>
    /// Luminance source class which support different formats of images.
    /// </summary>
    public class RgbLuminanceSource : BaseLuminanceSource
    {
        /// <summary>
        /// enumeration of supported bitmap format which the RGBLuminanceSource can process
        /// </summary>
        public enum BitmapFormat
        {
            /// <summary>
            /// format of the byte[] isn't known. RGBLuminanceSource tries to determine the best possible value
            /// </summary>
            UNKNOWN,
            /// <summary>
            /// grayscale array, the byte array is a luminance array with 1 byte per pixel
            /// </summary>
            GRAY8,
            /// <summary>
            /// grayscale array, the byte array is a luminance array with 2 bytes per pixel
            /// </summary>
            GRAY16,
            /// <summary>
            /// 3 bytes per pixel with the channels red, green and blue
            /// </summary>
            RGB24,
            /// <summary>
            /// 4 bytes per pixel with the channels red, green and blue
            /// </summary>
            RGB32,
            /// <summary>
            /// 4 bytes per pixel with the channels alpha, red, green and blue
            /// </summary>
            ARGB32,
            /// <summary>
            /// 3 bytes per pixel with the channels blue, green and red
            /// </summary>
            BGR24,
            /// <summary>
            /// 4 bytes per pixel with the channels blue, green and red
            /// </summary>
            BGR32,
            /// <summary>
            /// 4 bytes per pixel with the channels blue, green, red and alpha
            /// </summary>
            BGRA32,
            /// <summary>
            /// 2 bytes per pixel, 5 bit red, 6 bits green and 5 bits blue
            /// </summary>
            RGB565,
            /// <summary>
            /// 4 bytes per pixel with the channels red, green, blue and alpha
            /// </summary>
            RGBA32,
            /// <summary>
            /// 4 bytes for two pixels, UYVY formatted
            /// </summary>
            UYVY,
            /// <summary>
            /// 4 bytes for two pixels, YUYV formatted
            /// </summary>
            YUYV
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected RgbLuminanceSource(int width, int height)
           : base(width, height)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbLuminanceSource"/> class.
        /// It supports a byte array with 3 bytes per pixel (RGB24).
        /// </summary>
        /// <param name="rgbRawBytes">The RGB raw bytes.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RgbLuminanceSource(byte[] rgbRawBytes, int width, int height)
           : this(rgbRawBytes, width, height, BitmapFormat.RGB24)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbLuminanceSource"/> class.
        /// It supports a byte array with 3 bytes per pixel (RGB24).
        /// </summary>
        /// <param name="rgbRawBytes">The RGB raw bytes.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="bitmapFormat">The bitmap format.</param>
        public RgbLuminanceSource(byte[] rgbRawBytes, int width, int height, BitmapFormat bitmapFormat)
           : base(width, height)
        {
            CalculateLuminance(rgbRawBytes, bitmapFormat);
        }

        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new RgbLuminanceSource(width, height) { luminances = newLuminances };
        }

        private static BitmapFormat DetermineBitmapFormat(IReadOnlyCollection<byte> rgbRawBytes, int width, int height)
        {
            var square = width * height;
            var bytePerPixel = rgbRawBytes.Count / square;

            switch (bytePerPixel)
            {
                case 1:
                    return BitmapFormat.GRAY8;
                case 2:
                    return BitmapFormat.RGB565;
                case 3:
                    return BitmapFormat.RGB24;
                case 4:
                    return BitmapFormat.RGB32;
                default:
                    throw new ArgumentException("The bitmap format could not be determined. Please specify the correct value.");
            }
        }

        /// <summary>
        /// calculates the luminance values for the given byte array and bitmap format
        /// </summary>
        /// <param name="rgbRawBytes"></param>
        /// <param name="bitmapFormat"></param>
        protected void CalculateLuminance(byte[] rgbRawBytes, BitmapFormat bitmapFormat)
        {
            if (bitmapFormat == BitmapFormat.UNKNOWN)
            {
                bitmapFormat = DetermineBitmapFormat(rgbRawBytes, Width, Height);
            }
            switch (bitmapFormat)
            {
                case BitmapFormat.GRAY8:
                    Buffer.BlockCopy(rgbRawBytes, 0, luminances, 0, rgbRawBytes.Length < luminances.Length ? rgbRawBytes.Length : luminances.Length);
                    break;
                case BitmapFormat.GRAY16:
                    CalculateLuminanceGray16(rgbRawBytes);
                    break;
                case BitmapFormat.RGB24:
                    CalculateLuminanceRgb24(rgbRawBytes);
                    break;
                case BitmapFormat.BGR24:
                    CalculateLuminanceBgr24(rgbRawBytes);
                    break;
                case BitmapFormat.RGB32:
                    CalculateLuminanceRgb32(rgbRawBytes);
                    break;
                case BitmapFormat.BGR32:
                    CalculateLuminanceBgr32(rgbRawBytes);
                    break;
                case BitmapFormat.RGBA32:
                    CalculateLuminanceRgba32(rgbRawBytes);
                    break;
                case BitmapFormat.ARGB32:
                    CalculateLuminanceArgb32(rgbRawBytes);
                    break;
                case BitmapFormat.BGRA32:
                    CalculateLuminanceBgra32(rgbRawBytes);
                    break;
                case BitmapFormat.RGB565:
                    CalculateLuminanceRgb565(rgbRawBytes);
                    break;
                case BitmapFormat.UYVY:
                    CalculateLuminanceUyVy(rgbRawBytes);
                    break;
                case BitmapFormat.YUYV:
                    CalculateLuminanceYuYv(rgbRawBytes);
                    break;
                default:
                    throw new ArgumentException("The bitmap format isn't supported.", bitmapFormat.ToString());
            }
        }

        private void CalculateLuminanceRgb565(IReadOnlyList<byte> rgb565RawData)
        {
            var luminanceIndex = 0;
            for (var index = 0; index < rgb565RawData.Count && luminanceIndex < luminances.Length; index += 2, luminanceIndex++)
            {
                var byte1 = rgb565RawData[index];
                var byte2 = rgb565RawData[index + 1];

                var b5 = byte1 & 0x1F;
                var g5 = (((byte1 & 0xE0) >> 5) | ((byte2 & 0x03) << 3)) & 0x1F;
                var r5 = (byte2 >> 2) & 0x1F;
                var r8 = (r5 * 527 + 23) >> 6;
                var g8 = (g5 * 527 + 23) >> 6;
                var b8 = (b5 * 527 + 23) >> 6;

                // cheap, not fully accurate conversion
                //var pixel = (byte2 << 8) | byte1;
                //b8 = (((pixel) & 0x001F) << 3);
                //g8 = (((pixel) & 0x07E0) >> 2);
                //r8 = (((pixel) & 0xF800) >> 8);

                luminances[luminanceIndex] = (byte)((RChannelWeight * r8 + GChannelWeight * g8 + BChannelWeight * b8) >> ChannelWeight);
            }
        }

        private void CalculateLuminanceRgb24(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                int r = rgbRawBytes[rgbIndex++];
                int g = rgbRawBytes[rgbIndex++];
                int b = rgbRawBytes[rgbIndex++];
                luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
            }
        }

        private void CalculateLuminanceBgr24(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                int b = rgbRawBytes[rgbIndex++];
                int g = rgbRawBytes[rgbIndex++];
                int r = rgbRawBytes[rgbIndex++];
                luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
            }
        }

        private void CalculateLuminanceRgb32(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                int r = rgbRawBytes[rgbIndex++];
                int g = rgbRawBytes[rgbIndex++];
                int b = rgbRawBytes[rgbIndex++];
                rgbIndex++;
                luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
            }
        }

        private void CalculateLuminanceBgr32(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                int b = rgbRawBytes[rgbIndex++];
                int g = rgbRawBytes[rgbIndex++];
                int r = rgbRawBytes[rgbIndex++];
                rgbIndex++;
                luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
            }
        }

        private void CalculateLuminanceBgra32(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                var b = rgbRawBytes[rgbIndex++];
                var g = rgbRawBytes[rgbIndex++];
                var r = rgbRawBytes[rgbIndex++];
                var alpha = rgbRawBytes[rgbIndex++];
                var luminance = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
            }
        }

        private void CalculateLuminanceRgba32(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                var r = rgbRawBytes[rgbIndex++];
                var g = rgbRawBytes[rgbIndex++];
                var b = rgbRawBytes[rgbIndex++];
                var alpha = rgbRawBytes[rgbIndex++];
                var luminance = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
            }
        }

        private void CalculateLuminanceArgb32(IReadOnlyList<byte> rgbRawBytes)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Count && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                var alpha = rgbRawBytes[rgbIndex++];
                var r = rgbRawBytes[rgbIndex++];
                var g = rgbRawBytes[rgbIndex++];
                var b = rgbRawBytes[rgbIndex++];
                var luminance = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
            }
        }

        private void CalculateLuminanceUyVy(IReadOnlyList<byte> uyvyRawBytes)
        {
            // start by 1, jump over first U byte
            for (int uyvyIndex = 1, luminanceIndex = 0; uyvyIndex < uyvyRawBytes.Count - 3 && luminanceIndex < luminances.Length;)
            {
                byte y1 = uyvyRawBytes[uyvyIndex];
                uyvyIndex += 2; // jump from 1 to 3 (from Y1 over to Y2)
                byte y2 = uyvyRawBytes[uyvyIndex];
                uyvyIndex += 2; // jump from 3 to 5

                luminances[luminanceIndex++] = y1;
                luminances[luminanceIndex++] = y2;
            }
        }

        private void CalculateLuminanceYuYv(IReadOnlyList<byte> yuyvRawBytes)
        {
            // start by 0 not by 1 like UYUV
            for (int yuyvIndex = 0, luminanceIndex = 0; yuyvIndex < yuyvRawBytes.Count - 3 && luminanceIndex < luminances.Length;)
            {
                byte y1 = yuyvRawBytes[yuyvIndex];
                yuyvIndex += 2; // jump from 0 to 2 (from Y1 over over to Y2)
                byte y2 = yuyvRawBytes[yuyvIndex];
                yuyvIndex += 2; // jump from 2 to 4

                luminances[luminanceIndex++] = y1;
                luminances[luminanceIndex++] = y2;
            }
        }

        private void CalculateLuminanceGray16(IReadOnlyList<byte> gray16RawBytes)
        {
            for (int grayIndex = 0, luminanceIndex = 0; grayIndex < gray16RawBytes.Count && luminanceIndex < luminances.Length; grayIndex += 2, luminanceIndex++)
            {
                byte gray8 = gray16RawBytes[grayIndex];

                luminances[luminanceIndex] = gray8;
            }
        }
    }
}