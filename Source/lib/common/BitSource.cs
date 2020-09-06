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

namespace ZXing.Common
{
    /// <summary> <p>This provides an easy abstraction to read bits at a time from a sequence of bytes, where the
    /// number of bits read is not often a multiple of 8.</p>
    /// 
    /// <p>This class is thread-safe but not reentrant. Unless the caller modifies the bytes array
    /// it passed in, in which case all bets are off.</p>
    /// 
    /// </summary>
    /// <author>  Sean Owen
    /// </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    public sealed class BitSource
    {
        private readonly byte[] _Bytes;

        /// <param name="bytes">bytes from which this will read bits. Bits will be read from the first byte first.
        /// Bits are read within a byte from most-significant to least-significant bit.
        /// </param>
        public BitSource(byte[] bytes)
        {
            this._Bytes = bytes;
        }

        /// <summary>
        /// index of next bit in current byte which would be read by the next call to {@link #readBits(int)}.
        /// </summary>
        public int BitOffset { get; set; }

        /// <summary>
        /// index of next byte in input byte array which would be read by the next call to {@link #readBits(int)}.
        /// </summary>
        public int ByteOffset { get; set; }

        /// <param name="numBits">number of bits to read
        /// </param>
        /// <returns> int representing the bits read. The bits will appear as the least-significant
        /// bits of the int
        /// </returns>
        /// <exception cref="ArgumentException">if numBits isn't in [1,32] or more than is available</exception>
        public int ReadBits(int numBits)
        {
            if (numBits < 1 || numBits > 32 || numBits > Available())
            {
                throw new ArgumentException(numBits.ToString(), "numBits");
            }

            int result = 0;

            // First, read remainder from current byte
            if (BitOffset > 0)
            {
                int bitsLeft = 8 - BitOffset;
                int toRead = numBits < bitsLeft ? numBits : bitsLeft;
                int bitsToNotRead = bitsLeft - toRead;
                int mask = (0xFF >> (8 - toRead)) << bitsToNotRead;
                result = (_Bytes[ByteOffset] & mask) >> bitsToNotRead;
                numBits -= toRead;
                BitOffset += toRead;
                if (BitOffset == 8)
                {
                    BitOffset = 0;
                    ByteOffset++;
                }
            }

            // Next read whole bytes
            if (numBits <= 0) {
                return result;
            }
            while (numBits >= 8) {
                result = (result << 8) | (_Bytes[ByteOffset] & 0xFF);
                ByteOffset++;
                numBits -= 8;
            }

            // Finally read a partial byte
            if (numBits > 0) {
                int bitsToNotRead = 8 - numBits;
                int mask = (0xFF >> bitsToNotRead) << bitsToNotRead;
                result = (result << numBits) | ((_Bytes[ByteOffset] & mask) >> bitsToNotRead);
                BitOffset += numBits;
            }

            return result;
        }

        /// <returns> number of bits that can be read successfully
        /// </returns>
        public int Available()
        {
            return 8 * (_Bytes.Length - ByteOffset) - BitOffset;
        }
    }
}