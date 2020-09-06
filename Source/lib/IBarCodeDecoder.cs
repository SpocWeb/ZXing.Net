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
using ZXing.QrCode;

namespace ZXing
{
    /// <summary> Decodes an image of a barcode in some format into a String. </summary>
    /// <remarks>
    /// For example, <see cref="QrCodeReader" /> can
    /// decode a QR code. The decoder may optionally receive hints from the caller which may help
    /// it decode more quickly or accurately.
    /// 
    /// See <see cref="MultiFormatReader" />, which attempts to determine what barcode
    /// format is present within the image as well, and then decodes it accordingly.
    /// </remarks>
    /// <author>Sean Owen</author>
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    public interface IBarCodeDecoder
    {

        /// <summary> Locates AND decodes a barcode in some format within an image. </summary>
        /// <param name="image">image of barcode to decode</param>
        /// <param name="hints">passed as a <see cref="IDictionary{TKey, TValue}" />
        /// from <see cref="DecodeHintType" /> to arbitrary data.
        /// The meaning of the data depends upon the hint type.
        /// The implementation may or may not do anything with these hints.
        /// </param>
        /// <returns>String which the barcode encodes</returns>
        /// <remarks>
        /// This should be split into 2 Steps to be able to manage Problems! 
        /// </remarks>
        BarCodeText Decode(BinaryBitmap image, IDictionary<DecodeHintType, object> hints = null);

        /// <summary> Resets any internal state the implementation has after a decode </summary>
        /// <remarks>
        /// To prepare it for reuse.
        /// </remarks>
        void Reset();
    }
}