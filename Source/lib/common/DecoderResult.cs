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

namespace ZXing.Common
{
    /// <summary>
    /// Encapsulates the result of decoding a matrix of bits. This typically
    /// applies to 2D barcode formats. For now it contains the raw bytes obtained,
    /// as well as a String interpretation of those bytes, if applicable.
    /// <author>Sean Owen</author>
    /// </summary>
    public sealed class DecoderResult
    {
        /// <summary> raw bytes representing the result, or null if not applicable </summary>
        public byte[] RawBytes { get; }

        /// <summary> how many bits of<see cref="RawBytes"/> are valid; typically 8 times its length </summary>
        public int NumBits { get; }

        /// <summary> text representation of the result </summary>
        public string Text { get; }

        /// <summary> list of byte segments in the result, or null if not applicable </summary>
        public IList<byte[]> ByteSegments { get; }

        /// <summary> name of error correction level used, or null if not applicable </summary>
        public string EcLevel { get; }

        /// <summary> gets a value which describe if structure append data was found </summary>
        public bool StructuredAppend => StructuredAppendParity >= 0 && StructuredAppendSequenceNumber >= 0;

        /// <summary> number of errors corrected, or null if not applicable </summary>
        public int ErrorsCorrected { get; set; }

        /// <summary>
        /// gives the sequence number of the result if structured append was found </summary>
        public int StructuredAppendSequenceNumber { get; }

        /// <summary>
        /// number of erasures corrected, or null if not applicable </summary>
        public int Erasures { get; set; }

        /// <summary>
        /// gives the parity information if structured append was found </summary>
        public int StructuredAppendParity { get; }

        /// <summary>
        /// Miscellanseous data value for the various decoders </summary>
        /// <value>The other.</value>
        public object Other { get; set; }

        public DecoderResult(byte[] rawBytes, string text, IList<byte[]> byteSegments, string ecLevel)
           : this(rawBytes, text, byteSegments, ecLevel, -1, -1) { }

        public DecoderResult(byte[] rawBytes, string text, IList<byte[]> byteSegments, string ecLevel, int saSequence, int saParity)
           : this(rawBytes, 8 * rawBytes?.Length ?? 0, text, byteSegments, ecLevel, saSequence, saParity)
        {
        }

        public DecoderResult(byte[] rawBytes, int numBits, string text, IList<byte[]> byteSegments, string ecLevel)
           : this(rawBytes, numBits, text, byteSegments, ecLevel, -1, -1) { }

        public DecoderResult(byte[] rawBytes, int numBits, string text, IList<byte[]> byteSegments, string ecLevel, int saSequence, int saParity)
        {
            if (rawBytes == null && text == null)
            {
                throw new ArgumentException();
            }
            RawBytes = rawBytes;
            NumBits = numBits;
            Text = text;
            ByteSegments = byteSegments;
            EcLevel = ecLevel;
            StructuredAppendParity = saParity;
            StructuredAppendSequenceNumber = saSequence;
        }
    }
}