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

using System.Collections.Generic;

namespace ZXing.Common
{
    /// <summary> result of detecting a barcode in an image. </summary>
    /// <remarks>
    /// This includes the raw matrix of black/white pixels corresponding to the barcode,
    /// and possibly points of interest in the image,
    /// like the location of finder patterns or corners of the barcode in the image.</p>
    /// </remarks>
    /// <author>Sean Owen</author>
    public class DetectorResult
    {
        public IBitMatrix Bits { get; }

        /// <summary> pixel points where the result is found </summary>
        public ResultPoint[] Points { get; }

        public DetectorResult(IBitMatrix bits, ResultPoint[] points)
        {
            Bits = bits;
            Points = points;
        }
    }
}