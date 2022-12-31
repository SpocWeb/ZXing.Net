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

using System.Collections.Generic;
using ZXing.Common;
using ZXing.QrCode.Internal;

namespace ZXing.Multi.QrCode.Internal
{

    public static class XMultiQrDetector {

        public static MultiQrDetector AsMultiQrDetector(this IGridSampler sampler) => new MultiQrDetector(sampler);
        public static MultiQrDetector AsMultiQrDetector(this BitMatrix image) => new MultiQrDetector(image);
        public static MultiQrDetector AsMultiQrDetector(this BinaryBitmap image) => new MultiQrDetector(image);

    }

    /// <summary>
    /// <p>Encapsulates logic that can detect one or more QR Codes in an image, even if the QR Code
    /// is rotated or skewed, or partially obscured.</p>
    ///
    /// <author>Sean Owen</author>
    /// <author>Hannes Erven</author>
    /// </summary>
    public sealed class MultiQrDetector : QrDetector
    {

        static readonly DetectorResult[] _emptyDetectorResults = new DetectorResult[0];

        public MultiQrDetector(IGridSampler sampler) : base(sampler) { }

        /// <summary> Initializes a new instance of the <see cref="QrDetector"/> class. </summary>
        public MultiQrDetector(BitMatrix image) : base(image) { }

        /// <summary> Initializes a new instance of the <see cref="QrDetector"/> class. </summary>
        public MultiQrDetector(BinaryBitmap image) : base(image) { }

        /// <summary> Detects multiple possible Locations. </summary>
        public QrFinderPatternInfo[] FindMulti(IDictionary<DecodeHintType, object> hints = null)
        {
            var resultPointCallback =
                hints?.ContainsKey(DecodeHintType.NEED_RESULT_POINT_CALLBACK) != true
                    ? null : (ResultPointCallback)hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK];
            return FindMulti(hints?.ContainsKey(DecodeHintType.TRY_HARDER) == true, resultPointCallback);
        }

        /// <summary> Detects multiple possible Locations. </summary>
        public QrFinderPatternInfo[] FindMulti(bool tryHarder, ResultPointCallback resultPointCallback = null)
        {
            var finder = new MultiQrPatternFinder(Image, resultPointCallback);
            var infos = finder.FindMulti(tryHarder);

            return infos;
        }

        /// <summary> Detects multiple possible Locations. </summary>
        public QrFinderPatternInfo[] FindMulti(int numSkip, ResultPointCallback resultPointCallback = null)
        {
            var finder = new MultiQrPatternFinder(Image, resultPointCallback);
            var infos = finder.FindMulti(numSkip);

            return infos;
        }

        /// <summary> Detects multiple possible Locations. </summary>
        public DetectorResult[] DetectMulti(IDictionary<DecodeHintType, object> hints = null)
            => DetectMulti(FindMulti(hints));

        public DetectorResult[] DetectMulti(QrFinderPatternInfo[] infos) {
            if (infos.Length == 0)
            {
                return _emptyDetectorResults;
            }

            var result = new List<DetectorResult>();
            foreach (QrFinderPatternInfo info in infos) {
                var detectorResult = ProcessFinderPatternInfo(info);
                if (detectorResult != null) {
                    result.Add(detectorResult);
                }
            }
            if (result.Count == 0) {
                return _emptyDetectorResults;
            }
            return result.ToArray();
        }

    }
}
