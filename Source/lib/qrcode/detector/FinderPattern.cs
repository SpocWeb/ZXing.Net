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

namespace ZXing.QrCode.Internal
{
    /// <summary> Center of a finder pattern,
    /// which are the square patterns found in 3 of the 4 corners of QR Codes. </summary>
    /// <remarks>
    /// It also encapsulates a count of similar finder patterns,
    /// as a convenience to the finder's bookkeeping.</p>
    /// </remarks>
    /// <author>Sean Owen</author>
    public sealed class FinderPattern : ResultPoint
    {
        internal FinderPattern(float posX, float posY, float estimatedModuleSize)
           : this(posX, posY, estimatedModuleSize, 1)
        {
            EstimatedModuleSize = estimatedModuleSize;
            Count = 1;
        }

        internal FinderPattern(float posX, float posY, float estimatedModuleSize, int count)
           : base(posX, posY)
        {
            EstimatedModuleSize = estimatedModuleSize;
            Count = count;
        }

        public float EstimatedModuleSize { get; }

        /// <summary> #of Evidence for this Pattern </summary>
        internal int Count { get; }

        /// <summary> <p>Determines if this finder pattern "about equals" a finder pattern at the stated
        /// position and size --
        /// meaning, it is at nearly the same center with nearly the same size.</p>
        /// </summary>
        internal bool aboutEquals(float moduleSize, float i, float j)
        {
            if (Math.Abs(i - Y) <= moduleSize && Math.Abs(j - X) <= moduleSize)
            {
                float moduleSizeDiff = Math.Abs(moduleSize - EstimatedModuleSize);
                return moduleSizeDiff <= 1.0f || moduleSizeDiff <= EstimatedModuleSize;

            }
            return false;
        }

        /// <summary> Arith. Mean of current with new estimate in position and module size. </summary>
        /// <returns> a new <see cref="FinderPattern"/> containing a weighted average based on <see cref="Count"/>. </returns>
        internal FinderPattern combineEstimate(float x, float y, float newModuleSize)
        {
            int combinedCount = Count + 1;
            float combinedX = (Count * X + y) / combinedCount;
            float combinedY = (Count * Y + x) / combinedCount;
            float combinedModuleSize = (Count * EstimatedModuleSize + newModuleSize) / combinedCount;
            return new FinderPattern(combinedX, combinedY, combinedModuleSize, combinedCount);
        }
    }
}