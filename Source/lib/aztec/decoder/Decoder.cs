/*
 * Copyright 2010 ZXing authors
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
using ZXing.Common.ReedSolomon;

namespace ZXing.Aztec.Internal
{
    /// <summary>
    /// The main class which implements Aztec Code decoding -- as opposed to locating and extracting
    /// the Aztec Code from an image.
    /// </summary>
    /// <author>David Olivier</author>
    public sealed class Decoder
    {

        enum Table
        {
            UPPER,
            LOWER,
            MIXED,
            DIGIT,
            PUNCT,
            BINARY
        }

        static readonly string[] UPPER_TABLE =
        {
         "CTRL_PS", " ", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
         "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "CTRL_LL", "CTRL_ML", "CTRL_DL", "CTRL_BS"
      };

        static readonly string[] LOWER_TABLE =
        {
         "CTRL_PS", " ", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p",
         "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "CTRL_US", "CTRL_ML", "CTRL_DL", "CTRL_BS"
      };

        static readonly string[] MIXED_TABLE =
        {
         "CTRL_PS", " ", "\x1", "\x2", "\x3", "\x4", "\x5", "\x6", "\x7", "\b", "\t", "\n",
         "\xB", "\f", "\r", "\x1B", "\x1C", "\x1D", "\x1E", "\x1F", "@", "\\", "^", "_",
         "`", "|", "~", "\x7F", "CTRL_LL", "CTRL_UL", "CTRL_PL", "CTRL_BS"
      };

        static readonly string[] PUNCT_TABLE =
        {
         "", "\r", "\r\n", ". ", ", ", ": ", "!", "\"", "#", "$", "%", "&", "'", "(", ")",
         "*", "+", ",", "-", ".", "/", ":", ";", "<", "=", ">", "?", "[", "]", "{", "}", "CTRL_UL"
      };

        static readonly string[] DIGIT_TABLE =
        {
         "CTRL_PS", " ", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ",", ".", "CTRL_UL", "CTRL_US"
      };

        static readonly IDictionary<Table, string[]> CODE_TABLES = new Dictionary<Table, string[]>
      {
         {Table.UPPER, UPPER_TABLE},
         {Table.LOWER, LOWER_TABLE},
         {Table.MIXED, MIXED_TABLE},
         {Table.PUNCT, PUNCT_TABLE},
         {Table.DIGIT, DIGIT_TABLE},
         {Table.BINARY, null}
      };

        static readonly IDictionary<char, Table> CODE_TABLE_MAP = new Dictionary<char, Table>
      {
         {'U', Table.UPPER},
         {'L', Table.LOWER},
         {'M', Table.MIXED},
         {'P', Table.PUNCT},
         {'D', Table.DIGIT},
         {'B', Table.BINARY}
      };

        AztecDetectorResult _Data;

        /// <summary>
        /// Decodes the specified detector result.
        /// </summary>
        /// <param name="detectorResult">The detector result.</param>
        /// <returns></returns>
        public DecoderResult Decode(AztecDetectorResult detectorResult)
        {
            _Data = detectorResult;
            var matrix = detectorResult.Bits;
            var rawBits = ExtractBits(matrix);
            if (rawBits == null) {
                return null;
            }

            var correctedBits = CorrectBits(rawBits);
            if (correctedBits == null) {
                return null;
            }

            var result = GetEncodedData(correctedBits);
            if (result == null) {
                return null;
            }

            var rawBytes = ConvertBoolArrayToByteArray(correctedBits);

            return new DecoderResult(rawBytes, correctedBits.Length, result, null, null);
        }

        /// <summary>
        /// This method is used for testing the high-level encoder
        /// </summary>
        /// <param name="correctedBits"></param>
        /// <returns></returns>
        public static string HighLevelDecode(bool[] correctedBits)
        {
            return GetEncodedData(correctedBits);
        }

        /// <summary>
        /// Gets the string encoded in the aztec code bits
        /// </summary>
        /// <param name="correctedBits">The corrected bits.</param>
        /// <returns>the decoded string</returns>
        static string GetEncodedData(IReadOnlyList<bool> correctedBits)
        {
            var endIndex = correctedBits.Count;
            var latchTable = Table.UPPER; // table most recently latched to
            var shiftTable = Table.UPPER; // table to use for the next read
            var strTable = UPPER_TABLE;
            var result = new StringBuilder(20);
            var index = 0;

            while (index < endIndex)
            {
                if (shiftTable == Table.BINARY)
                {
                    if (endIndex - index < 5)
                    {
                        break;
                    }
                    int length = ReadCode(correctedBits, index, 5);
                    index += 5;
                    if (length == 0)
                    {
                        if (endIndex - index < 11)
                        {
                            break;
                        }
                        length = ReadCode(correctedBits, index, 11) + 31;
                        index += 11;
                    }
                    for (int charCount = 0; charCount < length; charCount++)
                    {
                        if (endIndex - index < 8)
                        {
                            index = endIndex; // Force outer loop to exit
                            break;
                        }
                        int code = ReadCode(correctedBits, index, 8);
                        result.Append((char)code);
                        index += 8;
                    }
                    // Go back to whatever mode we had been in
                    shiftTable = latchTable;
                    strTable = CODE_TABLES[shiftTable];
                }
                else
                {
                    int size = shiftTable == Table.DIGIT ? 4 : 5;
                    if (endIndex - index < size)
                    {
                        break;
                    }
                    int code = ReadCode(correctedBits, index, size);
                    index += size;
                    string str = GetCharacter(strTable, code);
                    if (str.StartsWith("CTRL_"))
                    {
                        // Table changes
                        // ISO/IEC 24778:2008 prescribes ending a shift sequence in the mode from which it was invoked.
                        // That's including when that mode is a shift.
                        // Our test case dlusbs.png for issue #642 exercises that.
                        latchTable = shiftTable;  // Latch the current mode, so as to return to Upper after U/S B/S
                        shiftTable = GetTable(str[5]);
                        strTable = CODE_TABLES[shiftTable];
                        if (str[6] == 'L')
                        {
                            latchTable = shiftTable;
                        }
                    }
                    else
                    {
                        result.Append(str);
                        // Go back to whatever mode we had been in
                        shiftTable = latchTable;
                        strTable = CODE_TABLES[shiftTable];
                    }
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// gets the table corresponding to the char passed
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns></returns>
        static Table GetTable(char t)
        {
            if (!CODE_TABLE_MAP.ContainsKey(t)) {
                return CODE_TABLE_MAP['U'];
            }
            return CODE_TABLE_MAP[t];
        }

        /// <summary>
        /// Gets the character (or string) corresponding to the passed code in the given table
        /// </summary>
        /// <param name="table">the table used</param>
        /// <param name="code">the code of the character</param>
        /// <returns></returns>
        static string GetCharacter(IReadOnlyList<string> table, int code) => table[code];

        /// <summary>Performs RS error correction on an array of bits. </summary>
        bool[] CorrectBits(IReadOnlyList<bool> rawBits)
        {
            GenericGf gf;
            int codewordSize;

            if (_Data.NbLayers <= 2)
            {
                codewordSize = 6;
                gf = GenericGf.AZTEC_DATA_6;
            }
            else if (_Data.NbLayers <= 8)
            {
                codewordSize = 8;
                gf = GenericGf.AZTEC_DATA_8;
            }
            else if (_Data.NbLayers <= 22)
            {
                codewordSize = 10;
                gf = GenericGf.AZTEC_DATA_10;
            }
            else
            {
                codewordSize = 12;
                gf = GenericGf.AZTEC_DATA_12;
            }

            int numDataCodewords = _Data.NbDatablocks;
            int numCodewords = rawBits.Count / codewordSize;
            if (numCodewords < numDataCodewords) {
                return null;
            }

            int offset = rawBits.Count % codewordSize;
            int numEcCodewords = numCodewords - numDataCodewords;

            int[] dataWords = new int[numCodewords];
            for (int i = 0; i < numCodewords; i++, offset += codewordSize)
            {
                dataWords[i] = ReadCode(rawBits, offset, codewordSize);
            }

            var rsDecoder = new ReedSolomonDecoder(gf);
            if (!rsDecoder.Decode(dataWords, numEcCodewords)) {
                return null;
            }

            // Now perform the un-stuffing operation.
            // First, count how many bits are going to be thrown out as stuffing
            int mask = (1 << codewordSize) - 1;
            int stuffedBits = 0;
            for (int i = 0; i < numDataCodewords; i++)
            {
                int dataWord = dataWords[i];
                if (dataWord == 0 || dataWord == mask)
                {
                    return null;
                }
                if (dataWord == 1 || dataWord == mask - 1)
                {
                    stuffedBits++;
                }
            }
            // Now, actually unpack the bits and remove the stuffing
            bool[] correctedBits = new bool[numDataCodewords * codewordSize - stuffedBits];
            int index = 0;
            for (int i = 0; i < numDataCodewords; i++)
            {
                int dataWord = dataWords[i];
                if (dataWord == 1 || dataWord == mask - 1)
                {
                    // next codewordSize-1 bits are all zeros or all ones
                    SupportClass.Fill(correctedBits, index, index + codewordSize - 1, dataWord > 1);
                    index += codewordSize - 1;
                }
                else
                {
                    for (int bit = codewordSize - 1; bit >= 0; --bit)
                    {
                        correctedBits[index++] = (dataWord & (1 << bit)) != 0;
                    }
                }
            }

            if (index != correctedBits.Length) {
                return null;
            }

            return correctedBits;
        }

        /// <summary>
        /// Gets the array of bits from an Aztec Code matrix
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>the array of bits</returns>
        bool[] ExtractBits(IRoBitMatrix matrix)
        {
            bool compact = _Data.Compact;
            int layers = _Data.NbLayers;
            int baseMatrixSize = (compact ? 11 : 14) + layers * 4; // not including alignment lines
            int[] alignmentMap = new int[baseMatrixSize];
            bool[] rawBits = new bool[TotalBitsInLayer(layers, compact)];

            if (compact)
            {
                for (int i = 0; i < alignmentMap.Length; i++)
                {
                    alignmentMap[i] = i;
                }
            }
            else
            {
                int matrixSize = baseMatrixSize + 1 + 2 * ((baseMatrixSize / 2 - 1) / 15);
                int origCenter = baseMatrixSize / 2;
                int center = matrixSize / 2;
                for (int i = 0; i < origCenter; i++)
                {
                    int newOffset = i + i / 15;
                    alignmentMap[origCenter - i - 1] = center - newOffset - 1;
                    alignmentMap[origCenter + i] = center + newOffset + 1;
                }
            }
            for (int i = 0, rowOffset = 0; i < layers; i++)
            {
                int rowSize = (layers - i) * 4 + (compact ? 9 : 12);
                // The top-left most point of this layer is <low, low> (not including alignment lines)
                int low = i * 2;
                // The bottom-right most point of this layer is <high, high> (not including alignment lines)
                int high = baseMatrixSize - 1 - low;
                // We pull bits from the two 2 x rowSize columns and two rowSize x 2 rows
                for (int j = 0; j < rowSize; j++)
                {
                    int columnOffset = j * 2;
                    for (int k = 0; k < 2; k++)
                    {
                        // left column
                        rawBits[rowOffset + columnOffset + k] =
                           matrix[alignmentMap[low + k], alignmentMap[low + j]];
                        // bottom row
                        rawBits[rowOffset + 2 * rowSize + columnOffset + k] =
                           matrix[alignmentMap[low + j], alignmentMap[high - k]];
                        // right column
                        rawBits[rowOffset + 4 * rowSize + columnOffset + k] =
                           matrix[alignmentMap[high - k], alignmentMap[high - j]];
                        // top row
                        rawBits[rowOffset + 6 * rowSize + columnOffset + k] =
                           matrix[alignmentMap[high - j], alignmentMap[low + k]];
                    }
                }
                rowOffset += rowSize * 8;
            }
            return rawBits;
        }

        /// <summary> Reads a code of given length and at given index in an array of bits </summary>
        static int ReadCode(IReadOnlyList<bool> rawBits, int startIndex, int length)
        {
            int res = 0;
            for (int i = startIndex; i < startIndex + length; i++)
            {
                res <<= 1;
                if (rawBits[i])
                {
                    res |= 1;
                }
            }
            return res;
        }

        /// <summary> Reads a code of length 8 in an array of bits, padding with zeros </summary>
        static byte ReadByte(IReadOnlyList<bool> rawBits, int startIndex)
        {
            int n = rawBits.Count - startIndex;
            if (n >= 8)
            {
                return (byte)ReadCode(rawBits, startIndex, 8);
            }
            return (byte)(ReadCode(rawBits, startIndex, n) << (8 - n));
        }

        /// <summary> Packs a bit array into bytes, most significant bit first </summary>
        public static byte[] ConvertBoolArrayToByteArray(bool[] boolArr)
        {
            byte[] byteArr = new byte[(boolArr.Length + 7) / 8];
            for (int i = 0; i < byteArr.Length; i++)
            {
                byteArr[i] = ReadByte(boolArr, 8 * i);
            }
            return byteArr;
        }

        static int TotalBitsInLayer(int layers, bool compact)
        {
            return ((compact ? 88 : 112) + 16 * layers) * layers;
        }
    }
}