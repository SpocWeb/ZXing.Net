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

using System;
using System.Collections.Generic;
using ZXing.Common;

namespace ZXing.OneD
{
    /// <summary>
    /// This object renders a CODE128 code as a <see cref="BitMatrix" />.
    /// 
    /// <author>erik.barbara@gmail.com (Erik Barbara)</author>
    /// </summary>
    public sealed class Code128Writer : OneDimensionalCodeWriter
    {

        const int CODE_START_A = 103;
        const int CODE_START_B = 104;
        const int CODE_START_C = 105;
        const int CODE_CODE_A = 101;
        const int CODE_CODE_B = 100;
        const int CODE_CODE_C = 99;
        const int CODE_STOP = 106;

        // Dummy characters used to specify control characters in input
        const char ESCAPE_FNC_1 = '\u00f1';
        const char ESCAPE_FNC_2 = '\u00f2';
        const char ESCAPE_FNC_3 = '\u00f3';
        const char ESCAPE_FNC_4 = '\u00f4';

        const int CODE_FNC_1 = 102; // Code A, Code B, Code C
        const int CODE_FNC_2 = 97; // Code A, Code B
        const int CODE_FNC_3 = 96; // Code A, Code B
        const int CODE_FNC_4_A = 101; // Code A
        const int CODE_FNC_4_B = 100; // Code B

        // Results of minimal lookahead for code C
        enum CType
        {
            UNCODABLE,
            ONE_DIGIT,
            TWO_DIGITS,
            FNC_1
        }

        bool forceCodesetB;

        static readonly IList<BarcodeFormat> supportedWriteFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128 };

        /// <summary>
        /// returns supported formats
        /// </summary>
        protected override IList<BarcodeFormat> SupportedWriteFormats => supportedWriteFormats;

        /// <summary>
        /// Encode the contents following specified format.
        /// {@code width} and {@code height} are required size. This method may return bigger size
        /// {@code BitMatrix} when specified size is too small. The user can set both {@code width} and
        /// {@code height} to zero to get minimum size barcode. If negative value is set to {@code width}
        /// or {@code height}, {@code IllegalArgumentException} is thrown.
        /// </summary>
        public override BitMatrix Encode(string contents,
            BarcodeFormat format,
            int width,
            int height,
            IDictionary<EncodeHintType, object> hints = null)
        {
            forceCodesetB = (hints != null &&
                             hints.ContainsKey(EncodeHintType.CODE128_FORCE_CODESET_B) &&
                             hints[EncodeHintType.CODE128_FORCE_CODESET_B] != null &&
                             Convert.ToBoolean(hints[EncodeHintType.CODE128_FORCE_CODESET_B].ToString()));
            ;
            if (hints != null &&
                hints.ContainsKey(EncodeHintType.GS1_FORMAT) &&
                hints[EncodeHintType.GS1_FORMAT] != null &&
                Convert.ToBoolean(hints[EncodeHintType.GS1_FORMAT].ToString()))
            {
                // append the FNC1 character at the first position if not already present
                if (!string.IsNullOrEmpty(contents) && contents[0] != ESCAPE_FNC_1) {
                    contents = ESCAPE_FNC_1 + contents;
                }
            }

            return base.Encode(contents, format, width, height, hints);
        }
        /// <summary>
        /// Encode the contents following specified format.
        /// </summary>
        public override bool[] Encode(string contents)
        {
            int length = contents.Length;
            // Check content
            for (int i = 0; i < length; i++)
            {
                char c = contents[i];
                switch (c)
                {
                    case ESCAPE_FNC_1:
                    case ESCAPE_FNC_2:
                    case ESCAPE_FNC_3:
                    case ESCAPE_FNC_4:
                        break;
                    default:
                        if (c > 127)
                            // support for FNC4 isn't implemented, no full Latin-1 character set available at the moment
                        {
                            throw new ArgumentException("Bad character in input: " + c);
                        }
                        break;
                }
            }

            var patterns = new List<int[]>(); // temporary storage for patterns
            int checkSum = 0;
            int checkWeight = 1;
            int codeSet = 0; // selected code (CODE_CODE_B or CODE_CODE_C)
            int position = 0; // position in contents

            while (position < length)
            {
                //Select code to use
                int newCodeSet = chooseCode(contents, position, codeSet);

                //Get the pattern index
                int patternIndex;
                if (newCodeSet == codeSet)
                {
                    // Encode the current character
                    // First handle escapes
                    switch (contents[position])
                    {
                        case ESCAPE_FNC_1:
                            patternIndex = CODE_FNC_1;
                            break;
                        case ESCAPE_FNC_2:
                            patternIndex = CODE_FNC_2;
                            break;
                        case ESCAPE_FNC_3:
                            patternIndex = CODE_FNC_3;
                            break;
                        case ESCAPE_FNC_4:
                            if (newCodeSet == CODE_CODE_A) {
                                patternIndex = CODE_FNC_4_A;
                            } else {
                                patternIndex = CODE_FNC_4_B;
                            }
                            break;
                        default:
                            // Then handle normal characters otherwise
                            switch (codeSet)
                            {
                                case CODE_CODE_A:
                                    patternIndex = contents[position] - ' ';
                                    if (patternIndex < 0)
                                    {
                                        // everything below a space character comes behind the underscore in the code patterns table
                                        patternIndex += '`';
                                    }
                                    break;
                                case CODE_CODE_B:
                                    patternIndex = contents[position] - ' ';
                                    break;
                                default:
                                    // CODE_CODE_C
                                    patternIndex = int.Parse(contents.Substring(position, 2));
                                    position++; // Also incremented below
                                    break;
                            }
                            break;
                    }
                    position++;
                }
                else
                {
                    // Should we change the current code?
                    // Do we have a code set?
                    if (codeSet == 0)
                    {
                        // No, we don't have a code set
                        switch (newCodeSet)
                        {
                            case CODE_CODE_A:
                                patternIndex = CODE_START_A;
                                break;
                            case CODE_CODE_B:
                                patternIndex = CODE_START_B;
                                break;
                            default:
                                patternIndex = CODE_START_C;
                                break;
                        }
                    }
                    else
                    {
                        // Yes, we have a code set
                        patternIndex = newCodeSet;
                    }
                    codeSet = newCodeSet;
                }

                // Get the pattern
                patterns.Add(Code128Reader.CODE_PATTERNS[patternIndex]);

                // Compute checksum
                checkSum += patternIndex * checkWeight;
                if (position != 0)
                {
                    checkWeight++;
                }
            }

            // Compute and append checksum
            checkSum %= 103;
            patterns.Add(Code128Reader.CODE_PATTERNS[checkSum]);

            // Append stop code
            patterns.Add(Code128Reader.CODE_PATTERNS[CODE_STOP]);

            // Compute code width
            int codeWidth = 0;
            foreach (int[] pattern in patterns)
            {
                foreach (int width in pattern)
                {
                    codeWidth += width;
                }
            }

            // Compute result
            var result = new bool[codeWidth];
            int pos = 0;
            foreach (int[] pattern in patterns)
            {
                pos += AppendPattern(result, pos, pattern, true);
            }

            return result;
        }


        static CType findCType(string value, int start)
        {
            int last = value.Length;
            if (start >= last)
            {
                return CType.UNCODABLE;
            }
            char c = value[start];
            if (c == ESCAPE_FNC_1)
            {
                return CType.FNC_1;
            }
            if (c < '0' || c > '9')
            {
                return CType.UNCODABLE;
            }
            if (start + 1 >= last)
            {
                return CType.ONE_DIGIT;
            }
            c = value[start + 1];
            if (c < '0' || c > '9')
            {
                return CType.ONE_DIGIT;
            }
            return CType.TWO_DIGITS;
        }

        int chooseCode(string value, int start, int oldCode)
        {
            CType lookahead = findCType(value, start);
            if (lookahead == CType.ONE_DIGIT)
            {
                if (oldCode == CODE_CODE_A)
                {
                    return CODE_CODE_A;
                }
                return CODE_CODE_B;
            }
            if (lookahead == CType.UNCODABLE)
            {
                if (start < value.Length)
                {
                    var c = value[start];
                    if (c < ' ' || (oldCode == CODE_CODE_A && (c < '`' || (c >= ESCAPE_FNC_1 && c <= ESCAPE_FNC_4))))
                    {
                        // can continue in code A, encodes ASCII 0 to 95 or FNC1 to FNC4
                        return CODE_CODE_A;
                    }
                }
                return CODE_CODE_B; // no choice
            }
            if (oldCode == CODE_CODE_A && lookahead == CType.FNC_1)
            {
                return CODE_CODE_A;
            }
            if (oldCode == CODE_CODE_C)
            {
                // can continue in code C
                return CODE_CODE_C;
            }
            if (oldCode == CODE_CODE_B)
            {
                if (lookahead == CType.FNC_1)
                {
                    return CODE_CODE_B; // can continue in code B
                }
                // Seen two consecutive digits, see what follows
                lookahead = findCType(value, start + 2);
                if (lookahead == CType.UNCODABLE || lookahead == CType.ONE_DIGIT)
                {
                    return CODE_CODE_B; // not worth switching now
                }
                if (lookahead == CType.FNC_1)
                {
                    // two digits, then FNC_1...
                    lookahead = findCType(value, start + 3);
                    if (lookahead == CType.TWO_DIGITS)
                    {
                        // then two more digits, switch
                        return forceCodesetB ? CODE_CODE_B : CODE_CODE_C;
                    }
                    return CODE_CODE_B; // otherwise not worth switching
                }
                // At this point, there are at least 4 consecutive digits.
                // Look ahead to choose whether to switch now or on the next round.
                int index = start + 4;
                while ((lookahead = findCType(value, index)) == CType.TWO_DIGITS)
                {
                    index += 2;
                }
                if (lookahead == CType.ONE_DIGIT)
                {
                    // odd number of digits, switch later
                    return CODE_CODE_B;
                }
                return forceCodesetB ? CODE_CODE_B : CODE_CODE_C; // even number of digits, switch now
            }
            // Here oldCode == 0, which means we are choosing the initial code
            if (lookahead == CType.FNC_1)
            {
                // ignore FNC_1
                lookahead = findCType(value, start + 1);
            }
            if (lookahead == CType.TWO_DIGITS)
            {
                // at least two digits, start in code C
                return forceCodesetB ? CODE_CODE_B : CODE_CODE_C;
            }
            return CODE_CODE_B;
        }
    }
}