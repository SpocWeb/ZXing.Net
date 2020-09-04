/*
* Copyright 2009 ZXing authors
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
using System.Text;
using ZXing.Common;

namespace ZXing
{
    /// <summary> Abstracts different bitmap implementations across platforms
    /// into standard greyscale luminance values. </summary>
    /// <remarks>
    /// Provides only immutable methods; crop and rotation create copies.
    /// This is to ensure that one Reader does not modify the original luminance source
    /// and leave it in an unknown state for other Readers in the chain.
    /// </remarks>
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    public abstract class LuminanceSource
    {
        private int width;
        private int height;

        protected LuminanceSource(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary> Fetches one row of luminance data from the underlying platform's bitmap. </summary>
        /// <remarks>
        /// Values range from 0 (black) to 255 (white).
        /// Because Java does not have an unsigned byte type,
        /// callers will have to bitwise AND with 0xff for each value.
        /// It is preferable for implementations of this method
        /// to only fetch this row rather than the whole image,
        /// since no 2D Readers may be installed and <see cref="Matrix"/> may never be called.
        /// </remarks>
        /// <param name="y">The row to fetch, which must be in [0, bitmap height)</param>
        /// <param name="row">An optional pre-allocated array. If null or too small, it will be ignored.
        /// Always use the returned object, and ignore the .length of the array.
        /// </param>
        /// <returns> An array containing the luminance data.</returns>
        public abstract byte[] getRow(int y, byte[] row);

        /// <summary> Fetches luminance data for the underlying bitmap. </summary>
        /// <returns>
        /// A row-major 2D array of luminance values.
        /// Do not use result.length as it may be larger than width * height.
        /// Do not modify the contents of the result.
        /// </returns>
        /// <remarks>
        /// Values should be fetched using: <code>int luminance = array[y * width + x] &amp; 0xff</code>
        /// </remarks>
        public abstract byte[] Matrix { get; }

        /// <returns> The width of the bitmap.</returns>
        public virtual int Width
        {
            get => width;
            protected set => width = value;
        }

        /// <returns> The height of the bitmap.</returns>
        public virtual int Height
        {
            get => height;
            protected set => height = value;
        }

        /// <returns> Whether this subclass supports cropping.</returns>
        public virtual bool CropSupported => false;

        /// <summary> 
        /// Returns a new object with cropped image data. Implementations may keep a reference to the
        /// original data rather than a copy. Only callable if CropSupported is true.
        /// </summary>
        /// <param name="left">The left coordinate, which must be in [0, Width)</param>
        /// <param name="top">The top coordinate, which must be in [0, Height)</param>
        /// <param name="width">The width of the rectangle to crop.</param>
        /// <param name="height">The height of the rectangle to crop.</param>
        /// <returns> A cropped version of this object.</returns>
        public virtual LuminanceSource crop(int left, int top, int width, int height)
        {
            throw new NotSupportedException("This luminance source does not support cropping.");
        }

        /// <returns> Whether this subclass supports counter-clockwise rotation.</returns>
        public virtual bool RotateSupported => false;

        /// <summary>
        /// Returns a new object with rotated image data by 90 degrees counterclockwise.
        /// Only callable if <see cref="RotateSupported"/> is true.
        /// </summary>
        /// <returns>A rotated version of this object.</returns>
        public virtual LuminanceSource rotateCounterClockwise()
        {
            throw new NotSupportedException("This luminance source does not support rotation.");
        }

        /// <summary>
        /// Returns a new object with rotated image data by 45 degrees counterclockwise.
        /// Only callable if <see cref="RotateSupported"/> is true.
        /// </summary>
        /// <returns>A rotated version of this object.</returns>
        public virtual LuminanceSource rotateCounterClockwise45()
        {
            throw new NotSupportedException("This luminance source does not support rotation by 45 degrees.");
        }

        /// <summary>
        /// </summary>
        /// <returns>Whether this subclass supports inversion.</returns>
        public virtual bool InversionSupported => false;

        /// <summary>
        /// inverts the luminance values, not supported here. has to implemented in sub classes
        /// </summary>
        /// <returns></returns>
        public virtual LuminanceSource invert()
        {
            throw new NotSupportedException("This luminance source does not support inversion.");
        }

        /// <summary> Readable 2D String Representation with 4 Levels of Brightness </summary>
        public override string ToString()
        {
            var row = new byte[width];
            var result = new StringBuilder(height * (width + 1));
            for (int y = 0; y < height; y++)
            {
                row = getRow(y, row);
                for (int x = 0; x < width; x++)
                {
                    int luminance = row[x];
                    char c;
                    if (luminance < 0x40)
                    {
                        c = '#';
                    }
                    else if (luminance < 0x80)
                    {
                        c = '+';
                    }
                    else if (luminance < 0xC0)
                    {
                        c = '.';
                    }
                    else
                    {
                        c = ' ';
                    }
                    result.Append(c);
                }
                result.Append('\n');
            }
            return result.ToString();
        }

        public bool SampleGridLine(IReadOnlyList<float> xyPairs
            , BitMatrix bits, int bitsRow
            , int blackThreshold, int range)
        {
            int max = xyPairs.Count;
            int[] sums = new int[max >> 1];
            byte[] row = new byte[Width];
            for (int dy = -range - 1; ++dy <= range;) {
                var imageY = (int)xyPairs[1];
                if (imageY < 0 || imageY >= Height)
                {
                    return false;
                }
                row = getRow(imageY + dy, row);
                for (int x = 0; x < max; x += 2)
                {
                    var imageX = (int)xyPairs[x];
                    if (imageX < 0 || imageX >= Width)
                    {
                        return false;
                    }

                    if (imageY != (int) xyPairs[x + 1]) {
                        //throw new ArgumentException();
                    }

                    for (int dx = -range - 1; ++dx <= range;) {
                        sums[x >> 1] += row[imageX + dx];
                    }
                }
            }
            for (int x = sums.Length; --x >= 0;) {
                bits[x, bitsRow] = sums[x] > blackThreshold;
            }
            return true;
        }

    }
}