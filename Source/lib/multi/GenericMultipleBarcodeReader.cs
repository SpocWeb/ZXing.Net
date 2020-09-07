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

namespace ZXing.Multi
{
    /// <summary>Attempts to locate multiple barcodes in an image
    ///   by recursively decoding portion of the image.
    /// </summary>
    /// <remarks>
    /// After one barcode is found, the areas left, above, right and below
    /// the barcodes <see cref="ResultPoint"/>s are scanned, recursively.
    /// 
    /// <p>A caller may want to also employ <see cref="ByQuadrantReader"/>
    ///   when attempting to find multiple 2D barcodes, like QR Codes, in an image,
    ///   where the presence of multiple barcodes might prevent
    /// detecting any one of them.</p>
    /// 
    ///   <p>That is, instead of passing a <see cref="IBarCodeDecoder"/> a caller might pass
    ///   <code>new ByQuadrantReader(reader)</code>.</p>
    ///   <author>Sean Owen</author>
    /// </remarks>
    public sealed class GenericMultipleBarcodeReader : IMultipleBarcodeReader, IBarCodeDecoder
    {

        const int MIN_DIMENSION_TO_RECUR = 30;
        const int MAX_DEPTH = 4;

        public readonly IBarCodeDecoder Decoder;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMultipleBarcodeReader"/> class.
        /// </summary>
        /// <param name="decoder">The @delegate.</param>
        public GenericMultipleBarcodeReader(IBarCodeDecoder decoder)
        {
            Decoder = decoder;
        }

        public BarCodeText Decode(DetectorResult detectorResult, IDictionary<DecodeHintType, object> hints = null) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public BarCodeText[] DecodeMultiple(DetectorResult[] detectorResults
            , IDictionary<DecodeHintType, object> hints = null) {
            throw new NotImplementedException();
        }

        public BarCodeText[] DecodeMultiple(BinaryBitmap image
            , IDictionary<DecodeHintType, object> hints = null)
        {
            var results = new List<BarCodeText>();
            DoDecodeRecursively(image, hints, results, 0, 0, 0);
            if (results.Count == 0)
            {
                return null;
            }
            int numResults = results.Count;
            BarCodeText[] resultArray = new BarCodeText[numResults];
            for (int i = 0; i < numResults; i++)
            {
                resultArray[i] = results[i];
            }
            return resultArray;
        }

        void DoDecodeRecursively(BinaryBitmap image
            , IDictionary<DecodeHintType, object> hints
            , IList<BarCodeText> results
            , int xOffset, int yOffset, int currentDepth)
        {
            if (currentDepth > MAX_DEPTH)
            {
                return;
            }

            BarCodeText result = Decoder.Decode(image, hints);
            if (result == null) {
                return;
            }

            bool alreadyFound = false;
            for (int i = 0; i < results.Count; i++)
            {
                BarCodeText existingResult = results[i];
                if (existingResult.Text.Equals(result.Text))
                {
                    alreadyFound = true;
                    break;
                }
            }
            if (!alreadyFound)
            {
                results.Add(TranslateResultPoints(result, xOffset, yOffset));
            }

            ResultPoint[] resultPoints = result.ResultPoints;
            if (resultPoints == null || resultPoints.Length == 0)
            {
                return;
            }
            int width = image.Width;
            int height = image.Height;
            float minX = width;
            float minY = height;
            float maxX = 0.0f;
            float maxY = 0.0f;
            for (int i = 0; i < resultPoints.Length; i++)
            {
                ResultPoint point = resultPoints[i];
                if (point == null)
                {
                    continue;
                }
                float x = point.X;
                float y = point.Y;
                if (x < minX)
                {
                    minX = x;
                }
                if (y < minY)
                {
                    minY = y;
                }
                if (x > maxX)
                {
                    maxX = x;
                }
                if (y > maxY)
                {
                    maxY = y;
                }
            }

            // Decode left of barcode
            if (minX > MIN_DIMENSION_TO_RECUR)
            {
                DoDecodeRecursively(image.Crop(0, 0, (int)minX, height), hints, results, xOffset, yOffset, currentDepth + 1);
            }
            // Decode above barcode
            if (minY > MIN_DIMENSION_TO_RECUR)
            {
                DoDecodeRecursively(image.Crop(0, 0, width, (int)minY), hints, results, xOffset, yOffset, currentDepth + 1);
            }
            // Decode right of barcode
            if (maxX < width - MIN_DIMENSION_TO_RECUR)
            {
                DoDecodeRecursively(image.Crop((int)maxX, 0, width - (int)maxX, height), hints, results, xOffset + (int)maxX, yOffset, currentDepth + 1);
            }
            // Decode below barcode
            if (maxY < height - MIN_DIMENSION_TO_RECUR)
            {
                DoDecodeRecursively(image.Crop(0, (int)maxY, width, height - (int)maxY), hints, results, xOffset, yOffset + (int)maxY, currentDepth + 1);
            }
        }

        static BarCodeText TranslateResultPoints(BarCodeText result, int xOffset, int yOffset)
        {
            var oldResultPoints = result.ResultPoints;
            var newResultPoints = new ResultPoint[oldResultPoints.Length];
            for (int i = 0; i < oldResultPoints.Length; i++)
            {
                var oldPoint = oldResultPoints[i];
                if (oldPoint != null)
                {
                    newResultPoints[i] = new ResultPoint(oldPoint.X + xOffset, oldPoint.Y + yOffset);
                }
            }
            var newResult = new BarCodeText(result.Text, result.RawBytes, result.Bitmap, result.NumBits, newResultPoints, result.BarcodeFormat);
            newResult.PutAllMetadata(result.ResultMetadata);
            return newResult;
        }

        /// <summary> Locates and decodes a barcode in some format within an image. </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <returns> String which the barcode encodes </returns>
        public BarCodeText Decode(BinaryBitmap image) => Decoder.Decode(image);

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
        public BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints)
            => Decoder.Decode(image, hints);

        /// <summary>
        /// Resets any internal state the implementation has after a decode, to prepare it
        /// for reuse.
        /// </summary>
        public void Reset()
        {
            Decoder.Reset();
        }

        public BarCodeText[] DecodeMultiple(LuminanceGridSampler image, IDictionary<DecodeHintType, object> hints)
        {
            throw new NotImplementedException();
        }

    }
}
