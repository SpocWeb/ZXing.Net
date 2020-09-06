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

namespace ZXing.Common
{
    /// <summary> Local 2D threshold-algorithm;
    /// slower than <see cref="GlobalHistogramBinarizer"/>,
    /// is fairly efficient for what it does.
    /// </summary>
    /// <remarks>
    /// It is designed for
    /// high frequency images of barcodes with black data on white backgrounds.
    /// For this application, it does a much better job than a global black-point
    /// with severe shadows and gradients.
    /// However it tends to produce artifacts on lower frequency images
    /// and is therefore not a good general purpose binarizer for uses outside ZXing.
    /// 
    /// This class extends <see cref="GlobalHistogramBinarizer"/>,
    /// using the older histogram approach for 1D readers,
    /// and the newer local approach for 2D readers.
    /// 
    /// 1D decoding using a per-row histogram is already inherently local,
    /// but fails for horizontal gradients.
    ///
    /// We can revisit that problem later,
    /// but for now it was not a win to use local blocks for 1D.
    /// 
    /// This Binarizer is the default for the unit tests
    /// and the recommended class for library users.
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    /// </remarks>
    public sealed class TwoDBinarizer : GlobalHistogramBinarizer
    {
        /// <summary> Calculates the complete black matrix </summary>
        public override BitMatrix GetBlackMatrix()
        {
            CalculateEntireImage();

            return _Matrix;
        }

        private BitMatrix _Matrix;

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="source"></param>
        public TwoDBinarizer(LuminanceSource source)
            : base(source) { }

        /// <summary>
        /// creates a new instance
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Binarizer createBinarizer(LuminanceSource source) => new TwoDBinarizer(source);

        /// <summary> Eagerly calculates the final BitMatrix once for all requests. </summary>
        /// <remarks>
        /// This could be called once from the constructor instead,
        /// but there are some advantages to doing it lazily,
        /// such as making profiling easier,
        /// and not doing heavy lifting when callers don't expect it.
        /// </remarks>
        private void CalculateEntireImage()
        {
            if (_Matrix != null)
            {
                return;
            }

            LuminanceSource source = LuminanceSource;
            int width = source.Width;
            int height = source.Height;
            if (width < XHybridBinarizer.MINIMUM_DIMENSION || height < XHybridBinarizer.MINIMUM_DIMENSION)
            {
                // If the image is too small, fall back to the global histogram approach.
                _Matrix = base.GetBlackMatrix();
            }
            else
            {
                byte[] luminances = source.Matrix;

                int subWidth = width >> XHybridBinarizer.BLOCK_SIZE_POWER;
                if ((width & XHybridBinarizer.BLOCK_SIZE_MASK) != 0)
                {
                    subWidth++;
                }

                int subHeight = height >> XHybridBinarizer.BLOCK_SIZE_POWER;
                if ((height & XHybridBinarizer.BLOCK_SIZE_MASK) != 0)
                {
                    subHeight++;
                }

                int[][] blackPoints = luminances.CalculateBlackPoints(subWidth, subHeight, width, height);

                var newMatrix = new BitMatrix(width, height);
                blackPoints.CalculateThresholdForBlock(luminances, subWidth, subHeight, width, height, newMatrix);
                _Matrix = newMatrix;
            }
        }

    }

    static class XHybridBinarizer {

        public const int MIN_DYNAMIC_RANGE = 24;

        // This class uses 5x5 blocks to compute local luminance, where each block is 8x8 pixels.
        // So this is the smallest dimension in each axis we can accept.
        public const int BLOCK_SIZE_POWER = 3;
        public const int BLOCK_SIZE = 1 << BLOCK_SIZE_POWER; // ...0100...00
        public const int BLOCK_SIZE_MASK = BLOCK_SIZE - 1; // ...0011...11
        public const int MINIMUM_DIMENSION = 40;

        /// <summary>
        /// For each 8x8 block in the image, calculate the average black point using a 5x5 grid
        /// of the blocks around it. Also handles the corner cases (fractional blocks are computed based
        /// on the last 8 pixels in the row/column which are also used in the previous block).
        /// </summary>
        public static void CalculateThresholdForBlock(this int[][] blackPoints
            , byte[] luminances, int subWidth, int subHeight, int width, int height
            , BitMatrix matrix)
        {
            int maxYOffset = height - BLOCK_SIZE;
            int maxXOffset = width - BLOCK_SIZE;

            for (int y = 0; y < subHeight; y++)
            {
                int yoffset = y << BLOCK_SIZE_POWER;
                if (yoffset > maxYOffset)
                {
                    yoffset = maxYOffset;
                }
                int top = Cap(y, subHeight - 3);
                for (int x = 0; x < subWidth; x++)
                {
                    int xoffset = x << BLOCK_SIZE_POWER;
                    if (xoffset > maxXOffset)
                    {
                        xoffset = maxXOffset;
                    }
                    int left = Cap(x, subWidth - 3);
                    int sum = 0;
                    for (int z = -2; z <= 2; z++)
                    {
                        int[] blackRow = blackPoints[top + z];
                        sum += blackRow[left - 2];
                        sum += blackRow[left - 1];
                        sum += blackRow[left];
                        sum += blackRow[left + 1];
                        sum += blackRow[left + 2];
                    }
                    int average = sum / 25;
                    ThresholdBlock(luminances, xoffset, yoffset, average, width, matrix);
                }
            }
        }

        public static int Cap(int value, int max) => value < 2 ? 2 : value > max ? max : value;

        /// <summary> Applies individual threshold to an 8x8 block of pixels. </summary>
        public static void ThresholdBlock(this byte[] luminances
            , int xOffset, int yOffset, int threshold, int stride, BitMatrix matrix)
        {
            int offset = yOffset * stride + xOffset;
            for (int y = 0; y < BLOCK_SIZE; y++, offset += stride)
            {
                for (int x = 0; x < BLOCK_SIZE; x++)
                {
                    int pixel = luminances[offset + x];
                    // Comparison needs to be <= so that black == 0 pixels are black even if the threshold is 0.
                    matrix[xOffset + x, yOffset + y] = pixel <= threshold;
                }
            }
        }

        /// <summary> Calculates a black point for each 8x8 block of pixels and saves it away.
        /// See the following thread for a discussion of this algorithm:
        /// http://groups.google.com/group/zxing/browse_thread/thread/d06efa2c35a7ddc0
        /// </summary>
        /// <param name="luminances">The luminances.</param>
        /// <param name="subWidth">Width of the sub.</param>
        /// <param name="subHeight">Height of the sub.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        public static int[][] CalculateBlackPoints(this byte[] luminances
            , int subWidth, int subHeight, int width, int height)
        {
            int maxYOffset = height - BLOCK_SIZE;
            int maxXOffset = width - BLOCK_SIZE;
            int[][] blackPoints = new int[subHeight][];
            for (int i = 0; i < subHeight; i++)
            {
                blackPoints[i] = new int[subWidth];
            }

            for (int y = 0; y < subHeight; y++)
            {
                int yoffset = y << BLOCK_SIZE_POWER;
                if (yoffset > maxYOffset)
                {
                    yoffset = maxYOffset;
                }
                var blackPointsY = blackPoints[y];
                var blackPointsY1 = y > 0 ? blackPoints[y - 1] : null;
                for (int x = 0; x < subWidth; x++)
                {
                    int xoffset = x << BLOCK_SIZE_POWER;
                    if (xoffset > maxXOffset)
                    {
                        xoffset = maxXOffset;
                    }
                    int sum = 0;
                    int min = 0xFF;
                    int max = 0;
                    for (int yy = 0, offset = yoffset * width + xoffset; yy < BLOCK_SIZE; yy++, offset += width)
                    {
                        for (int xx = 0; xx < BLOCK_SIZE; xx++)
                        {
                            int pixel = luminances[offset + xx];
                            // still looking for good contrast
                            sum += pixel;
                            if (pixel < min)
                            {
                                min = pixel;
                            }
                            if (pixel > max)
                            {
                                max = pixel;
                            }
                        }
                        // short-circuit min/max tests once dynamic range is met
                        if (max - min > MIN_DYNAMIC_RANGE)
                        {
                            // finish the rest of the rows quickly
                            for (yy++, offset += width; yy < BLOCK_SIZE; yy++, offset += width)
                            {
                                for (int xx = 0; xx < BLOCK_SIZE; xx++)
                                {
                                    sum += luminances[offset + xx];
                                }
                            }
                        }
                    }

                    // The default estimate is the average of the values in the block.
                    int average = sum >> (BLOCK_SIZE_POWER * 2);
                    if (max - min <= MIN_DYNAMIC_RANGE)
                    {
                        // If variation within the block is low, assume this is a block with only light or only
                        // dark pixels. In that case we do not want to use the average, as it would divide this
                        // low contrast area into black and white pixels, essentially creating data out of noise.
                        //
                        // The default assumption is that the block is light/background. Since no estimate for
                        // the level of dark pixels exists locally, use half the min for the block.
                        average = min >> 1;

                        if (blackPointsY1 != null && x > 0)
                        {
                            // Correct the "white background" assumption for blocks that have neighbors by comparing
                            // the pixels in this block to the previously calculated black points. This is based on
                            // the fact that dark barcode symbology is always surrounded by some amount of light
                            // background for which reasonable black point estimates were made. The bp estimated at
                            // the boundaries is used for the interior.

                            // The (min < bp) is arbitrary but works better than other heuristics that were tried.
                            int averageNeighborBlackPoint = (blackPointsY1[x] + 2 * blackPointsY[x - 1] + blackPointsY1[x - 1]) >> 2;
                            if (min < averageNeighborBlackPoint)
                            {
                                average = averageNeighborBlackPoint;
                            }
                        }
                    }
                    blackPointsY[x] = average;
                }
            }
            return blackPoints;
        }
    }
}