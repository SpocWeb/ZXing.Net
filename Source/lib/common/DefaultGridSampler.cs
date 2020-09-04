/*
* Copyright 2007 ZXing authors
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

namespace ZXing.Common
{

    /// <author> Sean Owen </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    public sealed class DefaultGridSampler : GridSampler {

        public DefaultGridSampler(BitMatrix image) {
            _Image = image;
        }

        readonly BitMatrix _Image;

        /// <inheritdoc />
        public override BitMatrix GetImage() => _Image;

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public override BitMatrix sampleGrid(int dimensionX, int dimensionY, PerspectiveTransform transform)
            => XGridSampler.sampleGrid(_Image, dimensionX, dimensionY, transform, 1);

    }

    public sealed class LuminanceGridSampler : GridSampler {

        public LuminanceGridSampler(LuminanceSource image) {
            Image = image;
            Binarizer = new GlobalHistogramBinarizer(image);
        }

        readonly GlobalHistogramBinarizer Binarizer;

        readonly LuminanceSource Image;

        /// <inheritdoc />
        public override BitMatrix GetImage() => Binarizer.BlackMatrix;

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public override BitMatrix sampleGrid(int dimensionX, int dimensionY, PerspectiveTransform transform)
            => XGridSampler.sampleGrid(Image, dimensionX, dimensionY, transform, Binarizer.BlackPoint, 1);

    }

    public static class XGridSampler {

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public static BitMatrix sampleGrid(LuminanceSource image
            , int dimensionX, int dimensionY, PerspectiveTransform transform
            , int blackPoint, int range)
        {
            if (dimensionX <= 0 || dimensionY <= 0)
            {
                return null;
            }
            int half = (((range << 1) + 1)*((range << 1) + 1)) >> 1;
            BitMatrix bits = new BitMatrix(dimensionX, dimensionY);
            float[] xyPairs = new float[dimensionX << 1];
            for (int y = 0; y < dimensionY; y++)
            {
                int max = xyPairs.Length;
                float yValue = y + 0.5f;
                for (int x = 0; x < max; x += 2)
                {
                    xyPairs[x] = (x >> 1) + 0.5f; //x * 0.5f;
                    xyPairs[x + 1] = yValue;
                }
                transform.transformPoints(xyPairs);
                // Quick check to see if points transformed to something inside the image;
                // sufficient to check the endpoints
                if (!checkAndNudgePoints(xyPairs, image.Width, image.Height))
                {
                    return null;
                }

                try
                {
                    if (image.SampleGridLine(xyPairs, bits, y, half * blackPoint, range))
                    {
                        return null;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // This feels wrong, but, sometimes if the finder patterns are misidentified,
                    // the resulting transform gets "twisted"
                    // such that it maps a straight line of points
                    // to a set of points whose endpoints are in bounds, but others are not.
                    //
                    // There is probably some mathematical way to detect this.
                    // This results in an ugly runtime exception despite our clever checks above --
                    // We could check each point's coordinates but that feels duplicate.
                    // We settle for catching and wrapping ArrayIndexOutOfBoundsException.
                    return null;
                }
            }
            return bits;
        }

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public static BitMatrix sampleGrid(this BitMatrix image
            , int dimensionX, int dimensionY, PerspectiveTransform transform
            , int range)
        {
            if (dimensionX <= 0 || dimensionY <= 0)
            {
                return null;
            }

            int half = (((range << 1) + 1) * ((range << 1) + 1)) >> 1;
            BitMatrix bits = new BitMatrix(dimensionX, dimensionY);
            float[] xyPairs = new float[dimensionX << 1];
            for (int y = 0; y < dimensionY; y++)
            {
                int max = xyPairs.Length;
                float iValue = y + 0.5f;
                for (int x = 0; x < max; x += 2)
                {
                    xyPairs[x] = (x >> 1) + 0.5f; //x * 0.5f;
                    xyPairs[x + 1] = iValue;
                }
                transform.transformPoints(xyPairs);
                // Quick check to see if points transformed to something inside the image;
                // sufficient to check the endpoints
                if (!checkAndNudgePoints(xyPairs, image.Width, image.Height))
                {
                    return null;
                }

                try
                {
                    var imageWidth = image.Width;
                    var imageHeight = image.Height;

                    for (int x = 0; x < max; x += 2)
                    {
                        var imageX = (int)xyPairs[x];
                        var imageY = (int)xyPairs[x + 1];

                        if (imageX < 0 || imageX >= imageWidth ||
                            imageY < 0 || imageY >= imageHeight)
                        {
                            return null;
                        }

                        bool bit;
                        if (range <= 0)
                        {
                            bit = image[imageX, imageY];
                        }
                        else
                        {
                            var sum = 0;
                            for (int dx = -range - 1; ++dx <= range; )
                            {
                                for (int dy = -range - 1; ++dy <= range; )
                                {
                                    if (image[imageX + dx, imageY + dy])
                                    {
                                        ++sum;
                                    }
                                }
                            }
                            bit = sum > half;
                        }

                        bits[x >> 1, y] = bit;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // java version:
                    // 
                    // This feels wrong, but, sometimes if the finder patterns are misidentified, the resulting
                    // transform gets "twisted" such that it maps a straight line of points to a set of points
                    // whose endpoints are in bounds, but others are not. There is probably some mathematical
                    // way to detect this about the transformation that I don't know yet.
                    // This results in an ugly runtime exception despite our clever checks above -- can't have
                    // that. We could check each point's coordinates but that feels duplicative. We settle for
                    // catching and wrapping ArrayIndexOutOfBoundsException.
                    return null;
                }
            }
            return bits;
        }

        /// <summary> <p>Checks a set of points that have been transformed to sample points on an image against
        /// the image's dimensions to see if the point are even within the image.</p>
        /// 
        /// <p>This method will actually "nudge" the endpoints back onto the image if they are found to be
        /// barely (less than 1 pixel) off the image. This accounts for imperfect detection of finder
        /// patterns in an image where the QR Code runs all the way to the image border.</p>
        /// 
        /// <p>For efficiency, the method will check points from either end of the line until one is found
        /// to be within the image. Because the set of points are assumed to be linear, this is valid.</p>
        /// 
        /// </summary>
        /// <param name="image">image into which the points should map
        /// </param>
        /// <param name="points">actual points in x1,y1,...,xn,yn form
        /// </param>
        static bool checkAndNudgePoints(float[] points, int width, int height)
        {
            // Check and nudge points from start until we see some that are OK:
            bool nudged = true;
            int maxOffset = points.Length - 1; // points.length must be even
            for (int offset = 0; offset < maxOffset && nudged; offset += 2)
            {
                int x = (int)points[offset];
                int y = (int)points[offset + 1];
                if (x < -1 || x > width || y < -1 || y > height)
                {
                    return false;
                }
                nudged = false;
                if (x == -1)
                {
                    points[offset] = 0.0f;
                    nudged = true;
                }
                else if (x == width)
                {
                    points[offset] = width - 1;
                    nudged = true;
                }
                if (y == -1)
                {
                    points[offset + 1] = 0.0f;
                    nudged = true;
                }
                else if (y == height)
                {
                    points[offset + 1] = height - 1;
                    nudged = true;
                }
            }
            // Check and nudge points from end:
            nudged = true;
            for (int offset = points.Length - 2; offset >= 0 && nudged; offset -= 2)
            {
                int x = (int)points[offset];
                int y = (int)points[offset + 1];
                if (x < -1 || x > width || y < -1 || y > height)
                {
                    return false;
                }
                nudged = false;
                if (x == -1)
                {
                    points[offset] = 0.0f;
                    nudged = true;
                }
                else if (x == width)
                {
                    points[offset] = width - 1;
                    nudged = true;
                }
                if (y == -1)
                {
                    points[offset + 1] = 0.0f;
                    nudged = true;
                }
                else if (y == height)
                {
                    points[offset + 1] = height - 1;
                    nudged = true;
                }
            }

            return true;
        }

    }
}