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
using ZXing.Common;
using ZXing.Multi;
using ZXing.PDF417.Internal;

namespace ZXing.PDF417
{
    /// <summary> Can detect and decode PDF417 codes in an image. </summary>
    /// <remarks>
    /// PDF417 is a stacked linear barcode format. 
    /// <author>SITA Lab (kevin.osullivan@sita.aero)</author>
    /// <author>Guenther Grau</author>
    /// </remarks>
    public sealed class Pdf417Reader : IBarCodeDecoder, IMultipleBarcodeReader {

        /// <summary>
        /// Resets any internal state the implementation has after a decode, to prepare it
        /// for reuse.
        /// </summary>
        public void Reset() {
            // do nothing
        }

        /// <summary> Locates and decodes a PDF417 code in an image. </summary>
        /// <returns>a String representing the content encoded by the PDF417 code</returns>
        /// <exception cref="FormatException">if a PDF417 cannot be decoded</exception>
        public BarCodeText Decode(BinaryBitmap image,
            IDictionary<DecodeHintType, object> hints = null) {
            BarCodeText[] results = image.Decode(hints, false);
            if (results.Length == 0) {
                return null;
            }
            return results[0]; // First barcode discovered.
        }

        /// <summary> Locates and decodes Multiple PDF417 codes in an image. </summary>
        /// <returns>an array of Strings representing the content encoded by the PDF417 codes</returns>
        public BarCodeText[] DecodeMultiple(BinaryBitmap image
            , IDictionary<DecodeHintType, object> hints = null)
            => image.Decode(hints, true);

        public BarCodeText[] DecodeMultiple(LuminanceGridSampler image, IDictionary<DecodeHintType, object> hints) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public BarCodeText[] DecodeMultiple(IDictionary<DecodeHintType, object> hints, DetectorResult[] detectorResults) {
            throw new NotImplementedException();
        }

    }

    public static class XPdf417Reader {

        /// <summary>
        /// Decode the specified image, with the hints and optionally multiple barcodes.
        /// Based on Owen's Comments in <see cref="ZXing.ReaderException"/>, this method has been modified to continue silently
        /// if a barcode was not decoded where it was detected instead of throwing a new exception object.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="hints">Hints.</param>
        /// <param name="multiple">If set to <c>true</c> multiple.</param>
        public static BarCodeText[] Decode(this BinaryBitmap image
            , IDictionary<DecodeHintType, object> hints, bool multiple)
        {
            var results = new List<BarCodeText>();
            PDF417DetectorResult detectorResult = Detector.Detect(image, hints, multiple);
            if (detectorResult == null)
            {
                return results.ToArray();
            }

            foreach (var points in detectorResult.Points)
            {
                var decoderResult = PDF417ScanningDecoder.decode(detectorResult.Bits
                    , points[4], points[5],
                    points[6], points[7], GetMinCodewordWidth(points), GetMaxCodewordWidth(points));
                if (decoderResult == null)
                {
                    continue;
                }
                var result = new BarCodeText(decoderResult.Text, decoderResult.RawBytes, points, BarcodeFormat.PDF_417);
                result.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, decoderResult.ECLevel);
                var pdf417ResultMetadata = (PDF417ResultMetadata)decoderResult.Other;
                if (pdf417ResultMetadata != null)
                {
                    result.PutMetadata(ResultMetadataType.PDF417_EXTRA_METADATA, pdf417ResultMetadata);
                }
                results.Add(result);
            }
            return results.ToArray();
        }

        /// <summary>
        /// Gets the maximum width of the barcode
        /// </summary>
        /// <returns>The max width.</returns>
        /// <param name="p1">P1.</param>
        /// <param name="p2">P2.</param>
        private static int GetMaxWidth(this ResultPoint p1, ResultPoint p2)
        {
            if (p1 == null || p2 == null)
            {
                return 0;
            }
            return (int)Math.Abs(p1.X - p2.X);
        }

        /// <summary>
        /// Gets the minimum width of the barcode
        /// </summary>
        /// <returns>The minimum width.</returns>
        /// <param name="p1">P1.</param>
        /// <param name="p2">P2.</param>
        private static int GetMinWidth(this ResultPoint p1, ResultPoint p2)
        {
            if (p1 == null || p2 == null)
            {
                return int.MaxValue;
            }
            return (int)Math.Abs(p1.X - p2.X);
        }

        /// <summary>
        /// Gets the maximum width of the codeword.
        /// </summary>
        /// <returns>The max codeword width.</returns>
        /// <param name="p">P.</param>
        private static int GetMaxCodewordWidth(this IReadOnlyList<ResultPoint> p) => Math.Max(
            Math.Max(GetMaxWidth(p[0], p[4]), GetMaxWidth(p[6], p[2]) * PDF417Common.MODULES_IN_CODEWORD /
                PDF417Common.MODULES_IN_STOP_PATTERN),
            Math.Max(GetMaxWidth(p[1], p[5]), GetMaxWidth(p[7], p[3]) * PDF417Common.MODULES_IN_CODEWORD /
                PDF417Common.MODULES_IN_STOP_PATTERN));

        /// <summary>
        /// Gets the minimum width of the codeword.
        /// </summary>
        /// <returns>The minimum codeword width.</returns>
        /// <param name="p">P.</param>
        private static int GetMinCodewordWidth(this IReadOnlyList<ResultPoint> p) => Math.Min(
            Math.Min(GetMinWidth(p[0], p[4]), GetMinWidth(p[6], p[2]) * PDF417Common.MODULES_IN_CODEWORD /
                PDF417Common.MODULES_IN_STOP_PATTERN),
            Math.Min(GetMinWidth(p[1], p[5]), GetMinWidth(p[7], p[3]) * PDF417Common.MODULES_IN_CODEWORD /
                PDF417Common.MODULES_IN_STOP_PATTERN));

    }
}