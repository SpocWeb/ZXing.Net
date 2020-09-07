/*
 * Copyright 2010 ZXing authors
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

namespace ZXing.Common.Detector
{
    /// <summary> Detects a candidate barcode-like rectangular region within an image. </summary>
    /// <remarks>
    /// It starts around the center of the image, increases the size of the candidate
    /// region until it finds a white rectangular region.
    /// By keeping track of the last black points it encountered,
    /// it determines the corners of the barcode.
    /// </remarks>
    /// <author>David Olivier</author>
    public sealed class WhiteRectangleDetector
    {
        private const int INIT_SIZE = 10;
        private const int CORR = 1;

        private readonly IRoBitMatrix _Image;
        private readonly int _Height;
        private readonly int _Width;
        private readonly int _LeftInit;
        private readonly int _RightInit;
        private readonly int _DownInit;
        private readonly int _UpInit;

        /// <summary>
        /// Creates a WhiteRectangleDetector instance
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>null, if image is too small, otherwise a WhiteRectangleDetector instance</returns>
        public static WhiteRectangleDetector Create(IRoBitMatrix image)
        {
            if (image == null) {
                return null;
            }

            var instance = new WhiteRectangleDetector(image);

            if (instance._UpInit < 0 || instance._LeftInit < 0 || instance._DownInit >= instance._Height || instance._RightInit >= instance._Width)
            {
                return null;
            }

            return instance;
        }

        /// <summary>
        /// Creates a WhiteRectangleDetector instance
        /// </summary>
        /// <param name="image">barcode image to find a rectangle in</param>
        /// <param name="initSize">initial size of search area around center</param>
        /// <param name="x">x position of search center</param>
        /// <param name="y">y position of search center</param>
        /// <returns>
        /// null, if image is too small, otherwise a WhiteRectangleDetector instance
        /// </returns>
        public static WhiteRectangleDetector Create(IRoBitMatrix image, int initSize, int x, int y)
        {
            var instance = new WhiteRectangleDetector(image, initSize, x, y);

            if (instance._UpInit < 0 || instance._LeftInit < 0 || instance._DownInit >= instance._Height || instance._RightInit >= instance._Width)
            {
                return null;
            }

            return instance;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="WhiteRectangleDetector"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <exception cref="ArgumentException">if image is too small</exception>
        internal WhiteRectangleDetector(IRoBitMatrix image)
           : this(image, INIT_SIZE, image.Width / 2, image.Height / 2)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WhiteRectangleDetector"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="initSize">Size of the init.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        internal WhiteRectangleDetector(IRoBitMatrix image, int initSize, int x, int y)
        {
            _Image = image;
            _Height = image.Height;
            _Width = image.Width;
            int halfsize = initSize / 2;
            _LeftInit = x - halfsize;
            _RightInit = x + halfsize;
            _UpInit = y - halfsize;
            _DownInit = y + halfsize;
        }

        /// <summary>
        /// Detects a candidate barcode-like rectangular region within an image. It
        /// starts around the center of the image, increases the size of the candidate
        /// region until it finds a white rectangular region.
        /// </summary>
        /// <returns><see cref="ResultPoint" />[] describing the corners of the rectangular
        /// region. The first and last points are opposed on the diagonal, as
        /// are the second and third. The first point will be the topmost
        /// point and the last, the bottommost. The second point will be
        /// leftmost and the third, the rightmost</returns>
        public ResultPoint[] Detect()
        {
            int left = _LeftInit;
            int right = _RightInit;
            int up = _UpInit;
            int down = _DownInit;
            bool sizeExceeded = false;
            bool aBlackPointFoundOnBorder = true;

            bool atLeastOneBlackPointFoundOnRight = false;
            bool atLeastOneBlackPointFoundOnBottom = false;
            bool atLeastOneBlackPointFoundOnLeft = false;
            bool atLeastOneBlackPointFoundOnTop = false;

            while (aBlackPointFoundOnBorder)
            {

                aBlackPointFoundOnBorder = false;

                // .....
                // .   |
                // .....
                bool rightBorderNotWhite = true;
                while ((rightBorderNotWhite || !atLeastOneBlackPointFoundOnRight) && right < _Width)
                {
                    rightBorderNotWhite = ContainsBlackPoint(up, down, right, false);
                    if (rightBorderNotWhite)
                    {
                        right++;
                        aBlackPointFoundOnBorder = true;
                        atLeastOneBlackPointFoundOnRight = true;
                    }
                    else if (!atLeastOneBlackPointFoundOnRight)
                    {
                        right++;
                    }
                }

                if (right >= _Width)
                {
                    sizeExceeded = true;
                    break;
                }

                // .....
                // .   .
                // .___.
                bool bottomBorderNotWhite = true;
                while ((bottomBorderNotWhite || !atLeastOneBlackPointFoundOnBottom) && down < _Height)
                {
                    bottomBorderNotWhite = ContainsBlackPoint(left, right, down, true);
                    if (bottomBorderNotWhite)
                    {
                        down++;
                        aBlackPointFoundOnBorder = true;
                        atLeastOneBlackPointFoundOnBottom = true;
                    }
                    else if (!atLeastOneBlackPointFoundOnBottom)
                    {
                        down++;
                    }
                }

                if (down >= _Height)
                {
                    sizeExceeded = true;
                    break;
                }

                // .....
                // |   .
                // .....
                bool leftBorderNotWhite = true;
                while ((leftBorderNotWhite || !atLeastOneBlackPointFoundOnLeft) && left >= 0)
                {
                    leftBorderNotWhite = ContainsBlackPoint(up, down, left, false);
                    if (leftBorderNotWhite)
                    {
                        left--;
                        aBlackPointFoundOnBorder = true;
                        atLeastOneBlackPointFoundOnLeft = true;
                    }
                    else if (!atLeastOneBlackPointFoundOnLeft)
                    {
                        left--;
                    }
                }

                if (left < 0)
                {
                    sizeExceeded = true;
                    break;
                }

                // .___.
                // .   .
                // .....
                bool topBorderNotWhite = true;
                while ((topBorderNotWhite || !atLeastOneBlackPointFoundOnTop) && up >= 0)
                {
                    topBorderNotWhite = ContainsBlackPoint(left, right, up, true);
                    if (topBorderNotWhite)
                    {
                        up--;
                        aBlackPointFoundOnBorder = true;
                        atLeastOneBlackPointFoundOnTop = true;
                    }
                    else if (!atLeastOneBlackPointFoundOnTop)
                    {
                        up--;
                    }
                }

                if (up < 0)
                {
                    sizeExceeded = true;
                    break;
                }
            }

            if (!sizeExceeded)
            {

                int maxSize = right - left;

                ResultPoint z = null;
                for (int i = 1; z == null && i < maxSize; i++)
                {
                    z = getBlackPointOnSegment(left, down - i, left + i, down);
                }

                if (z == null)
                {
                    return null;
                }

                ResultPoint t = null;
                //go down right
                for (int i = 1; t == null && i < maxSize; i++)
                {
                    t = getBlackPointOnSegment(left, up + i, left + i, up);
                }

                if (t == null)
                {
                    return null;
                }

                ResultPoint x = null;
                //go down left
                for (int i = 1; x == null && i < maxSize; i++)
                {
                    x = getBlackPointOnSegment(right, up + i, right - i, up);
                }

                if (x == null)
                {
                    return null;
                }

                ResultPoint y = null;
                //go up left
                for (int i = 1; y == null && i < maxSize; i++)
                {
                    y = getBlackPointOnSegment(right, down - i, right - i, down);
                }

                if (y == null)
                {
                    return null;
                }

                return CenterEdges(y, z, x, t);
            }
            return null;
        }

        private ResultPoint getBlackPointOnSegment(float aX, float aY, float bX, float bY)
        {
            int dist = MathUtils.Round(MathUtils.Distance(aX, aY, bX, bY));
            float xStep = (bX - aX) / dist;
            float yStep = (bY - aY) / dist;

            for (int i = 0; i < dist; i++)
            {
                int x = MathUtils.Round(aX + i * xStep);
                int y = MathUtils.Round(aY + i * yStep);
                if (_Image[x, y])
                {
                    return new ResultPoint(x, y);
                }
            }
            return null;
        }

        /// <summary>
        /// recenters the points of a constant distance towards the center
        /// </summary>
        /// <param name="y">bottom most point</param>
        /// <param name="z">left most point</param>
        /// <param name="x">right most point</param>
        /// <param name="t">top most point</param>
        /// <returns><see cref="ResultPoint"/>[] describing the corners of the rectangular
        /// region. The first and last points are opposed on the diagonal, as
        /// are the second and third. The first point will be the topmost
        /// point and the last, the bottommost. The second point will be
        /// leftmost and the third, the rightmost</returns>
        private ResultPoint[] CenterEdges(ResultPoint y, ResultPoint z,
                                          ResultPoint x, ResultPoint t)
        {
            //
            //       t            t
            //  z                      x
            //        x    OR    z
            //   y                    y
            //

            float yi = y.X;
            float yj = y.Y;
            float zi = z.X;
            float zj = z.Y;
            float xi = x.X;
            float xj = x.Y;
            float ti = t.X;
            float tj = t.Y;

            if (yi < _Width / 2.0f)
            {
                return new[]
                          {
                         new ResultPoint(ti - CORR, tj + CORR),
                         new ResultPoint(zi + CORR, zj + CORR),
                         new ResultPoint(xi - CORR, xj - CORR),
                         new ResultPoint(yi + CORR, yj - CORR)
                      };
            }
            return new[]
            {
                new ResultPoint(ti + CORR, tj + CORR),
                new ResultPoint(zi + CORR, zj - CORR),
                new ResultPoint(xi - CORR, xj + CORR),
                new ResultPoint(yi - CORR, yj - CORR)
            };
        }

        /// <summary>
        /// Determines whether a segment contains a black point
        /// </summary>
        /// <param name="a">min value of the scanned coordinate</param>
        /// <param name="b">max value of the scanned coordinate</param>
        /// <param name="fixed">value of fixed coordinate</param>
        /// <param name="horizontal">set to true if scan must be horizontal, false if vertical</param>
        /// <returns>
        ///   true if a black point has been found, else false.
        /// </returns>
        private bool ContainsBlackPoint(int a, int b, int @fixed, bool horizontal)
        {
            if (horizontal)
            {
                for (int x = a; x <= b; x++)
                {
                    if (_Image[x, @fixed])
                    {
                        return true;
                    }
                }
            }
            else
            {
                for (int y = a; y <= b; y++)
                {
                    if (_Image[@fixed, y])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}