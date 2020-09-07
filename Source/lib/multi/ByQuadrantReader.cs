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

namespace ZXing.Multi
{
    /// <summary> Attempts to decode a barcode from an image by scanning subsets of the image.
    ///
    /// It is important not by scanning the whole image when there may be multiple barcodes in
    /// an image, and detecting a barcode may find parts of multiple barcode and fail to decode
    /// (e.g. QR Codes). Instead this scans the four quadrants of the image -- and also the center
    /// 'quadrant' to cover the case where a barcode is found in the center.
    /// </summary>
    /// <seealso cref="GenericMultipleBarcodeReader" />
    public sealed class ByQuadrantReader : IBarCodeDecoder {

        readonly IBarCodeDecoder Decoder;

        public ByQuadrantReader(IBarCodeDecoder decoder) {
            Decoder = decoder;
        }

        /// <summary>
        /// Locates and decodes a barcode in some format within an image.
        /// </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <returns>
        /// String which the barcode encodes
        /// </returns>
        public BarCodeText Decode(BinaryBitmap image) {
            return Decode(image, null);
        }

        /// <summary>
        /// Locates and decodes a barcode in some format within an image. This method also accepts
        /// hints, each possibly associated to some data, which may help the implementation decode.
        /// </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <param name="hints">passed as a <see cref="IDictionary{TKey, TValue}"/> from <see cref="DecodeHintType"/>
        /// to arbitrary data. The
        /// meaning of the data depends upon the hint type. The implementation may or may not do
        /// anything with these hints.</param>
        /// <returns>
        /// String which the barcode encodes
        /// </returns>
        public BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints) {
            int width = image.Width;
            int height = image.Height;
            int halfWidth = width / 2;
            int halfHeight = height / 2;

            // No need to call makeAbsolute as results will be relative to original top left here
            var result = Decoder.Decode(image.Crop(0, 0, halfWidth, halfHeight), hints);
            if (result != null) {
                return result;
            }

            result = Decoder.Decode(image.Crop(halfWidth, 0, halfWidth, halfHeight), hints);
            if (result != null) {
                result.ResultPoints.MakeAbsolute(halfWidth, 0);
                return result;
            }

            result = Decoder.Decode(image.Crop(0, halfHeight, halfWidth, halfHeight), hints);
            if (result != null) {
                result.ResultPoints.MakeAbsolute(0, halfHeight);
                return result;
            }

            result = Decoder.Decode(image.Crop(halfWidth, halfHeight, halfWidth, halfHeight), hints);
            if (result != null) {
                result.ResultPoints.MakeAbsolute(halfWidth, halfHeight);
                return result;
            }

            int quarterWidth = halfWidth / 2;
            int quarterHeight = halfHeight / 2;
            var center = image.Crop(quarterWidth, quarterHeight, halfWidth, halfHeight);
            result = Decoder.Decode(center, hints);
            result?.ResultPoints.MakeAbsolute(quarterWidth, quarterHeight);
            return result;
        }

        /// <inheritdoc />
        public BarCodeText Decode(DetectorResult detectorResult, IDictionary<DecodeHintType, object> hints = null) {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Resets any internal state the implementation has after a decode, to prepare it
        /// for reuse.
        /// </summary>
        public void Reset() {
            Decoder.Reset();
        }

    }

    public static class X {

        public static void MakeAbsolute(this IList<ResultPoint> points, int leftOffset, int topOffset) {
            if (points == null) {
                return;
            }
            for (int i = 0; i < points.Count; i++)
            {
                ResultPoint relative = points[i];
                if (relative != null)
                {
                    points[i] = new ResultPoint(relative.X + leftOffset, relative.Y + topOffset);
                }
            }
        }
    }
}
