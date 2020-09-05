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

namespace ZXing.Common
{
    /// <summary> This Binarizer implementation uses the old ZXing global histogram approach. </summary>
    /// <remarks>
    /// It is suitable for low-end mobile devices
    /// which don't have enough CPU or memory to use a local threshold algorithm.
    /// However, because it picks a global black point,
    /// it cannot handle difficult shadows and gradients.
    /// 
    /// Faster mobile devices and all desktop applications
    /// should probably use HybridBinarizer instead.
    /// 
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    /// <author>Sean Owen</author>
    /// </remarks>
    public class GlobalHistogramBinarizer : Binarizer {

        private readonly byte[] _Luminances;
        private readonly int[] _Buckets;

        private void ClearBuckets() {
            for (int x = 0; x < LUMINANCE_BUCKETS; x++) {
                _Buckets[x] = 0;
            }
        }

        public GlobalHistogramBinarizer(LuminanceSource source)
            : base(source) {
            _Luminances = new byte[source.Width];
            _Buckets = new int[LUMINANCE_BUCKETS];
            ClearBuckets();
            BlackPoint = GlobalBlackPoint();
        }

        /// <summary> Initialized from <see cref="GlobalBlackPoint"/>. </summary>
        public readonly int BlackPoint;

        /// <summary> Quickly calculates the histogram by sampling four middle rows from the image. </summary>
        /// <remarks>
        /// This proved to be more robust on the blackbox tests
        /// than sampling a diagonal as we used to do.
        /// </remarks>
        int GlobalBlackPoint() {
            var source = base.LuminanceSource;
            int width = source.Width;
            int[] localBuckets = _Buckets;
            for (int y = 1; y < 5; y++) {
                int row = source.Height * y / 5;
                var localLuminances = source.getRow(row, _Luminances);
                int right = (width << 2) / 5;
                for (int x = width / 5; x < right; x++) {
                    int pixel = localLuminances[x];
                    localBuckets[pixel >> LUMINANCE_SHIFT]++;
                }
            }
            var blackPoint = localBuckets.estimateBlackPoint();
            if (blackPoint < 0) {
                throw new ArgumentException("Could not determine BlackPoint!");
            }
            return blackPoint;
        }

        /// <summary> Applies simple sharpening to the row data to improve performance of 1D Readers. </summary>
        public override BitArray getBlackRow(int y, BitArray row = null)
        {
            LuminanceSource source = LuminanceSource;
            int width = source.Width;
            if (row == null || row.Size < width)
            {
                row = new BitArray(width);
            }
            else
            {
                row.clear();
            }

            byte[] localLuminances = source.getRow(y, _Luminances);
            int blackPoint = LocalBlackPoint(width, localLuminances);
            if (blackPoint < 0)
                return null;

            if (width < 3)
            {
                // Special case for very small images
                for (int x = 0; x < width; x++)
                {
                    if (localLuminances[x] < blackPoint)
                    {
                        row[x] = true;
                    }
                }
            }
            else
            {
                int left = localLuminances[0];
                int center = localLuminances[1];
                for (int x = 1; x < width - 1; x++)
                {
                    int right = localLuminances[x + 1];
                    // A simple -1 4 -1 box filter with a weight of 2.
                    // ((center << 2) - left - right) >> 1
                    if (((center * 4) - left - right) / 2 < blackPoint)
                    {
                        row[x] = true;
                    }
                    left = center;
                    center = right;
                }
            }

            return row;
        }

        private int LocalBlackPoint(int width, byte[] localLuminances)
        {
            ClearBuckets();
            int[] localBuckets = _Buckets;
            for (int x = 0; x < width; x++)
            {
                localBuckets[(localLuminances[x]) >> LUMINANCE_SHIFT]++;
            }
            int blackPoint = localBuckets.estimateBlackPoint();
            return blackPoint;
        }

        /// <summary> Sharpens the data, as this call is intended to only be used by 2D Readers. </summary>
        public override BitMatrix GetBlackMatrix()
        {
            LuminanceSource source = LuminanceSource;

            int width = source.Width;
            int height = source.Height;
            BitMatrix matrix = new BitMatrix(width, height);
            //var localLuminances = source.Matrix;
            for (int y = 0; y < height; y++)
            {
                byte[] localLuminances = source.getRow(y, _Luminances);
                int blackPoint = LocalBlackPoint(width, localLuminances);
                if (blackPoint < 0)
                {
                    blackPoint = 127;
                    //return null;
                }
                //int offset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int pixel = localLuminances[x]; // + offset];
                    matrix[x, y] = pixel < blackPoint;
                }
            }

            return matrix;
        }

        /// <summary> Does NOT sharpen the data, as this call is intended to only be used by 2D Readers. </summary>
        public BitMatrix GetBlackMatrix2()
        {
            LuminanceSource source = LuminanceSource;

            int width = source.Width;
            int height = source.Height;
            BitMatrix matrix = new BitMatrix(width, height);
            var blackPoint = GlobalBlackPoint();
            // Delay reading the entire image luminance until the black point estimation succeeds.
            // Although we end up reading four rows twice,
            // it is consistent with our motto of "fail quickly"
            // which is necessary for continuous scanning.
            if (blackPoint < 0)
                return new BitMatrix(1, 1);

            var localLuminances = source.Matrix;
            for (int y = 0; y < height; y++)
            {
                int offset = y * width;
                for (int x = 0; x < width; x++)
                {
                    int pixel = localLuminances[offset + x];
                    matrix[x, y] = (pixel < blackPoint);
                }
            }

            return matrix;
        }

        /// <summary> Creates a new, clean Binarizer initialized with <paramref name="source"/>. </summary>
        /// <remarks>
        /// This is needed because Binarizer implementations may be stateful,
        /// e.g. keeping a cache of 1 bit data.
        /// </remarks>
        public override Binarizer createBinarizer(LuminanceSource source)
            => new GlobalHistogramBinarizer(source);

    }

    public static class X {

        public static int estimateBlackPoint(this IReadOnlyList<int> buckets)
        {
            var firstPeak = FindTallestPeak(buckets);
            var maxBucketCount = buckets[firstPeak];

            var secondPeak = FindSecondTallestPeak(buckets, firstPeak);

            int numBuckets = buckets.Count;

            // Make sure firstPeak corresponds to the black peak.
            if (firstPeak > secondPeak)
            {
                int temp = firstPeak;
                firstPeak = secondPeak;
                secondPeak = temp;
            }

            // Too little contrast in the image to pick a meaningful black point,
            // throw rather than waste time trying to decode the image, and risk false positives.
            // TODO: It might be worth comparing the brightest and darkest pixels seen,
            // rather than the two peaks, to determine the contrast.
            if (secondPeak - firstPeak <= numBuckets >> 4)
            {
                return int.MinValue;
            }

            // Find a valley between them that is low and closer to the white peak.
            int bestValley = secondPeak - 1;
            int bestValleyScore = -1;
            for (int x = secondPeak - 1; x > firstPeak; x--)
            {
                int fromFirst = x - firstPeak;
                int score = fromFirst * fromFirst * (secondPeak - x) * (maxBucketCount - buckets[x]);
                if (score > bestValleyScore)
                {
                    bestValley = x;
                    bestValleyScore = score;
                }
            }

            return bestValley << Binarizer.LUMINANCE_SHIFT;
        }

        /// <summary> Finds the second tallest Peak that is somewhat far from the tallest peak. </summary>
        public static int FindSecondTallestPeak(this IReadOnlyList<int> buckets, int firstPeak) {
            int numBuckets = buckets.Count;
            var secondPeak = 0;
            int secondPeakScore = 0;
            for (int x = 0; x < numBuckets; x++) {
                int distanceToBiggest = x - firstPeak;
                // Encourage more distant second peaks by multiplying by square of distance.
                int score = buckets[x] * distanceToBiggest * distanceToBiggest;
                if (score > secondPeakScore) {
                    secondPeak = x;
                    secondPeakScore = score;
                }
            }
            return secondPeak;
        }

        /// <summary> Find the tallest peak in the histogram. </summary>
        public static int FindTallestPeak(this IReadOnlyList<int> buckets) {
            int numBuckets = buckets.Count;
            var firstPeak = 0;
            int firstPeakSize = 0;
            for (int x = 0; x < numBuckets; x++) {
                if (firstPeakSize < buckets[x]) {
                    firstPeakSize = buckets[x];
                    firstPeak = x;
                }
            }
            return firstPeak;
        }

    }
}