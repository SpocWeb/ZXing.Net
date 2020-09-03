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

namespace ZXing.Common
{

    /// <author>  Sean Owen
    /// </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    public sealed class DefaultGridSampler : GridSampler
    {
        /// <summary>
        /// </summary>
        public override BitMatrix sampleGrid(BitMatrix image, int dimensionX, int dimensionY, float p1ToX, float p1ToY, float p2ToX, float p2ToY, float p3ToX, float p3ToY, float p4ToX, float p4ToY, float p1FromX, float p1FromY, float p2FromX, float p2FromY, float p3FromX, float p3FromY, float p4FromX, float p4FromY)
        {
            PerspectiveTransform transform = XTrafo.quadrilateralToQuadrilateral(
               p1ToX, p1ToY, p2ToX, p2ToY, p3ToX, p3ToY, p4ToX, p4ToY,
               p1FromX, p1FromY, p2FromX, p2FromY, p3FromX, p3FromY, p4FromX, p4FromY);
            return sampleGrid(image, dimensionX, dimensionY, transform);
        }

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public override BitMatrix sampleGrid(BitMatrix image
            , int dimensionX, int dimensionY, PerspectiveTransform transform)
            => sampleGrid(image, dimensionX, dimensionY, transform, 1);

        /// <summary> Samples the <paramref name="image"/> to a lower Resolution </summary>
        public BitMatrix sampleGrid(BitMatrix image
            , int dimensionX, int dimensionY, PerspectiveTransform transform, int range)
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
                float iValue = y + 0.5f;
                for (int x = 0; x < max; x += 2)
                {
                    xyPairs[x] = (x >> 1) + 0.5f; //x * 0.5f;
                    xyPairs[x + 1] = iValue;
                }
                transform.transformPoints(xyPairs);
                // Quick check to see if points transformed to something inside the image;
                // sufficient to check the endpoints
                if (!checkAndNudgePoints(image, xyPairs))
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
    }
}