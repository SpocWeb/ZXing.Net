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

using System.Collections.Generic;
using System.Linq;
using ZXing.Aztec.Internal;
using ZXing.Common;

namespace ZXing.Aztec
{
    /// <summary> detect and decode Aztec codes in an image. </summary>
    /// <author>David Olivier</author>
    public class AztecReader : IBarCodeDecoder
    {
        /// <summary>
        /// Locates and decodes a barcode in some format within an image.
        /// </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <returns>
        /// a String representing the content encoded by the Data Matrix code
        /// </returns>
        public BarCodeText decode(BinaryBitmap image)
        {
            return Decode(image, null);
        }

        /// <summary>
        ///  Locates and decodes a Data Matrix code in an image.
        /// </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <param name="hints">passed as a {@link java.util.Hashtable} from {@link com.google.zxing.DecodeHintType}
        /// to arbitrary data. The
        /// meaning of the data depends upon the hint type. The implementation may or may not do
        /// anything with these hints.</param>
        /// <returns>
        /// String which the barcode encodes
        /// </returns>
        public BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints)
        {
            var blackmatrix = image.GetBlackMatrix();
            if (blackmatrix == null) {
                return null;
            }

            IGridSampler gridSampler = new DefaultGridSampler(blackmatrix);
            Detector detector = new Detector(gridSampler);
            ResultPoint[] points = null;
            DecoderResult decoderResult = null;

            var detectorResult = detector.detect();
            if (detectorResult != null)
            {
                points = detectorResult.Points.Single();

                decoderResult = new Decoder().decode(detectorResult);
            }
            if (decoderResult == null)
            {
                detectorResult = detector.detect(true);
                if (detectorResult == null) {
                    return null;
                }

                points = detectorResult.Points.Single();
                decoderResult = new Decoder().decode(detectorResult);
                if (decoderResult == null) {
                    return null;
                }
            }

            if (hints != null &&
                hints.ContainsKey(DecodeHintType.NEED_RESULT_POINT_CALLBACK))
            {
                var rpcb = (ResultPointCallback)hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK];
                if (rpcb != null)
                {
                    foreach (var point in points)
                    {
                        rpcb(point);
                    }
                }
            }

            var result = new BarCodeText(decoderResult.Text, decoderResult.RawBytes, decoderResult.NumBits, points, BarcodeFormat.AZTEC);

            IList<byte[]> byteSegments = decoderResult.ByteSegments;
            if (byteSegments != null)
            {
                result.PutMetadata(ResultMetadataType.BYTE_SEGMENTS, byteSegments);
            }
            var ecLevel = decoderResult.ECLevel;
            if (ecLevel != null)
            {
                result.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, ecLevel);
            }

            result.PutMetadata(ResultMetadataType.AZTEC_EXTRA_METADATA,
                               new AztecResultMetadata(detectorResult.Compact, detectorResult.NbDatablocks, detectorResult.NbLayers));

            return result;
        }

        public BarCodeText Decode(DetectorResult detectorResult, IDictionary<DecodeHintType, object> hints = null) {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Resets any internal state the implementation has after a decode, to prepare it
        /// for reuse.
        /// </summary>
        public void Reset()
        {
            // do nothing
        }
    }
}