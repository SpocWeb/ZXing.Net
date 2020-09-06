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
using System.Collections.Generic;

namespace ZXing
{
    /// <summary> Encapsulates the result of decoding a barcode within an image. </summary>
    public sealed class BarCodeText //: IBarCodeText
    {
        /// <returns>raw text encoded by the barcode, if applicable, otherwise <code>null</code></returns>
        public string Text { get; }

        /// <returns>raw bytes encoded by the barcode, if applicable, otherwise <code>null</code></returns>
        public byte[] RawBytes { get; }

        /// <summary>points related to the barcode in the image. </summary>
        /// <remarks>
        /// These are typically points identifying finder patterns or the corners of the barcode.
        /// The exact meaning is specific to the type of barcode that was decoded.
        /// </remarks>
        public ResultPoint[] ResultPoints { get; private set; }

        public BarcodeFormat BarcodeFormat { get; }

        /// <summary> optional metadata about what was detected about the barcode, like orientation. </summary>
        public IDictionary<ResultMetadataType, object> ResultMetadata { get; private set; }

        public DateTime Timestamp { get; }

        /// <summary> how many bits of <see cref="RawBytes"/> are valid; typically 8 times its length </summary>
        public int NumBits { get; }

        public BarCodeText(string text, byte[] rawBytes, ResultPoint[] resultPoints, BarcodeFormat format)
            : this(text, rawBytes, 8 * rawBytes?.Length ?? 0, resultPoints, format, DateTime.Now) { }

        public BarCodeText(string text, byte[] rawBytes, int numBits, ResultPoint[] resultPoints, BarcodeFormat format)
            : this(text, rawBytes, numBits, resultPoints, format, DateTime.Now) { }

        public BarCodeText(string text, byte[] rawBytes, ResultPoint[] resultPoints, BarcodeFormat format, DateTime timestamp)
            : this(text, rawBytes, 8 * rawBytes?.Length ?? 0, resultPoints, format, timestamp) { }

        public BarCodeText(string text, byte[] rawBytes, int numBits, ResultPoint[] resultPoints
            , BarcodeFormat format, DateTime timestamp)
        {
            if (text == null && rawBytes == null)
            {
                throw new ArgumentException("Text and bytes are null");
            }

            Text = text;
            RawBytes = rawBytes;
            NumBits = numBits;
            ResultPoints = resultPoints;
            BarcodeFormat = format;
            ResultMetadata = null;
            Timestamp = timestamp;
        }

        public void PutMetadata(ResultMetadataType type, object value)
        {
            if (ResultMetadata == null)
            {
                ResultMetadata = new Dictionary<ResultMetadataType, object>();
            }

            ResultMetadata[type] = value;
        }

        public void PutAllMetadata(IDictionary<ResultMetadataType, object> metadata)
        {
            if (metadata == null)
            {
                return;
            }

            if (ResultMetadata == null)
            {
                ResultMetadata = new Dictionary<ResultMetadataType, object>();
            }

            ResultMetadata.AddRange(metadata);
        }
        /// <summary> Adds the result points. </summary>
        public void AddResultPoints(params ResultPoint[] newPoints)
        {
            var oldPoints = ResultPoints;
            if (oldPoints == null)
            {
                ResultPoints = newPoints; //TODO: make a Copy!
            }
            else if (newPoints != null && newPoints.Length > 0)
            {
                var allPoints = new ResultPoint[oldPoints.Length + newPoints.Length];
                Array.Copy(oldPoints, 0, allPoints, 0, oldPoints.Length);
                Array.Copy(newPoints, 0, allPoints, oldPoints.Length, newPoints.Length);
                ResultPoints = allPoints;
            }
        }

        public override string ToString() => Text ?? GetType().Name + "[" + RawBytes.Length + " bytes]";

    }

    public static class XDictionary {

        public static void AddRange<TK, TV>(this IDictionary<TK, TV> resultMetadata
            , IEnumerable<KeyValuePair<TK, TV>> metadata)
        {
            foreach (var entry in metadata)
            {
                resultMetadata[entry.Key] = entry.Value;
            }
        }

    }
}