/*
 * Copyright 2008 ZXing authors
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
using System.Text;
using ZXing.Common;

namespace ZXing.OneD
{
    /// <summary>
    ///   <p>Implements decoding of the UPC-A format.</p>
    ///   <author>dswitkin@google.com (Daniel Switkin)</author>
    ///   <author>Sean Owen</author>
    /// </summary>
    public sealed class UpcAReader : UpcEanReader
    {

        readonly UpcEanReader ean13Reader = new Ean13Reader();

        /// <summary>
        ///   <p>Like decodeRow(int, BitArray, java.util.Map), but
        /// allows caller to inform method about where the UPC/EAN start pattern is
        /// found. This allows this to be computed once and reused across many implementations.</p>
        /// </summary>
        public override BarCodeText DecodeRow(int rowNumber,
                                BitArray row,
                                int[] startGuardRange,
                                IDictionary<DecodeHintType, object> hints)
            => maybeReturnResult(ean13Reader.DecodeRow(rowNumber, row, startGuardRange, hints));

        /// <summary>
        ///   <p>Attempts to decode a one-dimensional barcode format given a single row of
        /// an image.</p>
        /// </summary>
        /// <param name="rowNumber">row number from top of the row</param>
        /// <param name="row">the black/white pixel data of the row</param>
        /// <param name="hints">decode hints</param>
        /// <returns>
        ///   <see cref="BarCodeText"/>containing encoded string and start/end of barcode or null, if an error occurs or barcode cannot be found
        /// </returns>
        public override BarCodeText DecodeRow(int rowNumber, BitArray row
            , IDictionary<DecodeHintType, object> hints)
            => maybeReturnResult(ean13Reader.DecodeRow(rowNumber, row, hints));

        /// <summary>
        /// Decodes the specified image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="hints">The hints.</param>
        /// <returns></returns>
        public override BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints)
        {
            return maybeReturnResult(ean13Reader.Decode(image, hints));
        }

        /// <summary>
        /// Get the format of this decoder.
        /// <returns>The 1D format.</returns>
        /// </summary>
        internal override BarcodeFormat BarcodeFormat => BarcodeFormat.UPC_A;

        /// <summary>
        /// Subclasses override this to decode the portion of a barcode between the start
        /// and end guard patterns.
        /// </summary>
        /// <param name="row">row of black/white values to search</param>
        /// <param name="startRange">start/end offset of start guard pattern</param>
        /// <param name="resultString"><see cref="StringBuilder"/>to append decoded chars to</param>
        /// <returns>
        /// horizontal offset of first pixel after the "middle" that was decoded or -1 if decoding could not complete successfully
        /// </returns>
        protected internal override int DecodeMiddle(BitArray row, int[] startRange, StringBuilder resultString)
        {
            return ean13Reader.DecodeMiddle(row, startRange, resultString);
        }

        static BarCodeText maybeReturnResult(BarCodeText result)
        {
            if (result == null) {
                return null;
            }

            var text = result.Text;
            if (text[0] == '0')
            {
                var upcaResult = new BarCodeText(text.Substring(1), null, result.ResultPoints, BarcodeFormat.UPC_A);
                if (result.ResultMetadata != null)
                {
                    upcaResult.PutAllMetadata(result.ResultMetadata);
                }
                return upcaResult;
            }
            return null;
        }
    }
}