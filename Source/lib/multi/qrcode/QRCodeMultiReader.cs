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
using System.IO;
using System.Text;
using ZXing.Common;
using ZXing.Multi.QrCode.Internal;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace ZXing.Multi.QrCode
{
    /// <summary>
    /// This implementation can detect and decode multiple QR Codes in an image.
    /// </summary>
    public sealed class QRCodeMultiReader : QrCodeReader, IMultipleBarcodeReader
    {
        private static readonly ResultPoint[] NO_POINTS = new ResultPoint[0];

        /// <summary>
        /// Decodes the multiple.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public BarCodeText[] DecodeMultiple(BinaryBitmap image)
        {
            return DecodeMultiple(image, null);
        }

        /// <summary> Decodes multiple QR Codes </summary>
        public BarCodeText[] DecodeMultiple(LuminanceGridSampler image, IDictionary<DecodeHintType, object> hints)
        {
            var detectorResults = new MultiQrDetector(image).DetectMulti(hints);
            return DecodeMultiple(hints, detectorResults);
        }

        /// <summary> Decodes multiple QR Codes </summary>
        public BarCodeText[] DecodeMultiple(BinaryBitmap image, IDictionary<DecodeHintType, object> hints)
        {
            var detectorResults = new MultiQrDetector(image).DetectMulti(hints);
            return DecodeMultiple(hints, detectorResults);
        }

        public BarCodeText[] DecodeMultiple(IDictionary<DecodeHintType, object> hints, DetectorResult[] detectorResults)
        {
            var results = new List<BarCodeText>();
            foreach (var detectorResult in detectorResults)
            {
                var decoderResult = Decoder.decode(detectorResult.Bits, hints);

                if (decoderResult == null) {
                    continue;
                }

                var points = detectorResult.Points;

                // If the code was mirrored: swap the bottom-left and the top-right points.
                if (decoderResult.Other is QrCodeDecoderMetaData data)
                {
                    points = data.ApplyMirroredCorrection(points);
                }

                var result = new BarCodeText(decoderResult.Text, decoderResult.RawBytes, points, BarcodeFormat.QR_CODE);
                var byteSegments = decoderResult.ByteSegments;
                if (byteSegments != null)
                {
                    result.PutMetadata(ResultMetadataType.BYTE_SEGMENTS, byteSegments);
                }

                var ecLevel = decoderResult.ECLevel;
                if (ecLevel != null)
                {
                    result.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, ecLevel);
                }

                if (decoderResult.StructuredAppend)
                {
                    result.PutMetadata(ResultMetadataType.STRUCTURED_APPEND_SEQUENCE, decoderResult.StructuredAppendSequenceNumber);
                    result.PutMetadata(ResultMetadataType.STRUCTURED_APPEND_PARITY, decoderResult.StructuredAppendParity);
                }

                results.Add(result);
            }

            if (results.Count == 0)
            {
                return null;
            }

            results = ProcessStructuredAppend(results);

            return results.ToArray();
        }

        public static List<BarCodeText> ProcessStructuredAppend(List<BarCodeText> results)
        {
            var newResults = new List<BarCodeText>();
            var saResults = new List<BarCodeText>();
            foreach (var result in results)
            {
                if (result.ResultMetadata.ContainsKey(ResultMetadataType.STRUCTURED_APPEND_SEQUENCE))
                {
                    saResults.Add(result);
                }
                else
                {
                    newResults.Add(result);
                }
            }
            if (saResults.Count == 0)
            {
                return results;
            }
            // sort and concatenate the SA list items
            saResults.Sort(SaSequenceSort);
            var newText = new StringBuilder();
            using (var newRawBytes = new MemoryStream())
            using (var newByteSegment = new MemoryStream())
            {
                foreach (BarCodeText saResult in saResults)
                {
                    newText.Append(saResult.Text);
                    byte[] saBytes = saResult.RawBytes;
                    newRawBytes.Write(saBytes, 0, saBytes.Length);
                    if (saResult.ResultMetadata.ContainsKey(ResultMetadataType.BYTE_SEGMENTS))
                    {
                        var byteSegments = (IEnumerable<byte[]>) saResult.ResultMetadata[ResultMetadataType.BYTE_SEGMENTS];
                        if (byteSegments != null)
                        {
                            foreach (byte[] segment in byteSegments)
                            {
                                newByteSegment.Write(segment, 0, segment.Length);
                            }
                        }
                    }
                }

                BarCodeText newResult = new BarCodeText(newText.ToString(), newRawBytes.ToArray(), NO_POINTS, BarcodeFormat.QR_CODE);
                if (newByteSegment.Length > 0)
                {
                    var byteSegmentList = new List<byte[]>
                    {
                        newByteSegment.ToArray()
                    };
                    newResult.PutMetadata(ResultMetadataType.BYTE_SEGMENTS, byteSegmentList);
                }
                newResults.Add(newResult);
            }
            return newResults;
        }

        private static int SaSequenceSort(BarCodeText a, BarCodeText b)
        {
            var aNumber = (int) a.ResultMetadata[ResultMetadataType.STRUCTURED_APPEND_SEQUENCE];
            var bNumber = (int) b.ResultMetadata[ResultMetadataType.STRUCTURED_APPEND_SEQUENCE];
            return aNumber - bNumber;
        }
    }
}