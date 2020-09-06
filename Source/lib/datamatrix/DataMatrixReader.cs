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
using System.Linq;
using ZXing.Common;
using ZXing.Datamatrix.Internal;

namespace ZXing.Datamatrix
{
    /// <summary> detect and decode Data Matrix codes in an image. </summary>
    /// <author>bbrown@google.com (Brian Brown)</author>
    public sealed class DataMatrixReader : IBarCodeDecoder {

        static readonly ResultPoint[] NO_POINTS = new ResultPoint[0];

        readonly Decoder _Decoder = new Decoder();

        /// <summary> Locates and decodes a Data Matrix code in an <paramref name="image"/>. </summary>
        /// <returns>a String representing the content encoded by the Data Matrix code</returns>
        public BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints) {
            DecoderResult decoderResult;
            ResultPoint[] points;
            if (hints?.ContainsKey(DecodeHintType.PURE_BARCODE) == true) {
                BitMatrix bits = ExtractPureBits(image.GetBlackMatrix());
                if (bits == null) {
                    return null;
                }
                decoderResult = _Decoder.decode(bits);
                points = NO_POINTS;
            } else {
                IGridSampler sampler = new DefaultGridSampler(image.GetBlackMatrix());
                DetectorResult detectorResult = new Detector(sampler).detect();
                if (detectorResult?.Bits == null) {
                    return null;
                }
                decoderResult = _Decoder.decode(detectorResult.Bits);
                points = detectorResult.Points.Single();
            }

            return decoderResult?.AsBarCodeText(points);
        }

        public BarCodeText Decode(DetectorResult detectorResult, IDictionary<DecodeHintType, object> hints = null) {
            DecoderResult result = _Decoder.decode(detectorResult.Bits);
            return result.AsBarCodeText(detectorResult.Points.Single());
        }

        /// <summary>
        /// does nothing here
        /// </summary>
        public void Reset() {
            // do nothing
        }

        /// <summary>
        /// This method detects a code in a "pure" image -- that is, pure monochrome image
        /// which contains only an unrotated, unskewed, image of a code, with some white border
        /// around it. This is a specialized method that works exceptionally fast in this special
        /// case.
        ///
        /// <sQrCodeReader.ExtractPureBitsts(BitMatrix)" />
        /// </summary>
        static BitMatrix ExtractPureBits(BitMatrix image) {
            int[] leftTopBlack = image.getTopLeftOnBit();
            int[] rightBottomBlack = image.getBottomRightOnBit();
            if (leftTopBlack == null || rightBottomBlack == null) {
                return null;
            }

            if (!image.TryGetModuleSize(leftTopBlack, out var moduleSize)) {
                return null;
            }

            int top = leftTopBlack[1];
            int bottom = rightBottomBlack[1];
            int left = leftTopBlack[0];
            int right = rightBottomBlack[0];

            int matrixWidth = (right - left + 1) / moduleSize;
            int matrixHeight = (bottom - top + 1) / moduleSize;
            if (matrixWidth <= 0 || matrixHeight <= 0) {
                return null;
            }

            // Push in the "border" by half the module width so that we start
            // sampling in the middle of the module. Just in case the image is a
            // little off, this will help recover.
            int nudge = moduleSize >> 1;
            top += nudge;
            left += nudge;

            // Now just read off the bits
            BitMatrix bits = new BitMatrix(matrixWidth, matrixHeight);
            for (int y = 0; y < matrixHeight; y++) {
                int iOffset = top + y * moduleSize;
                for (int x = 0; x < matrixWidth; x++) {
                    if (image[left + x * moduleSize, iOffset]) {
                        bits[x, y] = true;
                    }
                }
            }
            return bits;
        }

    }

    public static class XDataMatrixReader {

        public static BarCodeText AsBarCodeText(this DecoderResult decoderResult
            , params ResultPoint[] points) {
            BarCodeText result = decoderResult.AsBarCodeText(BarcodeFormat.DATA_MATRIX, points);
            IList<byte[]> byteSegments = decoderResult.ByteSegments;
            if (byteSegments != null) {
                result.PutMetadata(ResultMetadataType.BYTE_SEGMENTS, byteSegments);
            }
            var ecLevel = decoderResult.EcLevel;
            if (ecLevel != null) {
                result.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, ecLevel);
            }
            return result;
        }

        public static bool TryGetModuleSize(this BitMatrix image
            , IReadOnlyList<int> leftTopBlack, out int moduleSize)
        {
            int width = image.Width;
            int x = leftTopBlack[0];
            int y = leftTopBlack[1];
            while (x < width && image[x, y])
            {
                x++;
            }
            if (x == width)
            {
                moduleSize = 0;
                return false;
            }

            moduleSize = x - leftTopBlack[0];
            if (moduleSize == 0)
            {
                return false;
            }
            return true;
        }
    }
}
