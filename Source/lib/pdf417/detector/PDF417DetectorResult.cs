/*
 * Copyright 2013 ZXing authors
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

using System.Collections.Generic;
using ZXing.Common;

namespace ZXing.PDF417.Internal
{
    /// <summary>
    /// PDF 417 Detector Result class.  Skipped private backing stores.
    /// <author>Guenther Grau</author> 
    /// </summary>
    public sealed class PDF417DetectorResult
    {
        public BitMatrix Bits { get; }

        /// <summary>
        /// points of the detected result in the image
        /// </summary>
        public IReadOnlyList<ResultPoint[]> Points { get; }

        public PDF417DetectorResult(BitMatrix bits, IReadOnlyList<ResultPoint[]> points)
        {
            Bits = bits;
            Points = points;
        }
    }
}