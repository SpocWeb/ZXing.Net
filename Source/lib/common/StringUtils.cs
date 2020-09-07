/*
 * Copyright (C) 2010 ZXing authors
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
using System.Text;

namespace ZXing.Common
{
    /// <summary>
    /// Common string-related functions.
    /// </summary>
    /// <author>Sean Owen</author>
    /// <author>Alex Dupre</author>
    public static class StringUtils
    {
#if (WINDOWS_PHONE || SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE || NETSTANDARD)
        const string PLATFORM_DEFAULT_ENCODING = "UTF-8";
#else
      private static readonly string PLATFORM_DEFAULT_ENCODING = Encoding.Default.WebName;
#endif
        /// <summary>
        /// SJIS
        /// </summary>
        public static string SHIFT_JIS = "SJIS";
        /// <summary>
        /// GB2312
        /// </summary>
        public static string GB2312 = "GB2312";

        const string EUC_JP = "EUC-JP";
        const string UTF8 = "UTF-8";
        const string ISO88591 = "ISO-8859-1";

        static readonly bool ASSUME_SHIFT_JIS =
           string.Compare(SHIFT_JIS, PLATFORM_DEFAULT_ENCODING, StringComparison.OrdinalIgnoreCase) == 0 ||
           string.Compare(EUC_JP, PLATFORM_DEFAULT_ENCODING, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Guesses the encoding.
        /// </summary>
        /// <param name="bytes">bytes encoding a string, whose encoding should be guessed</param>
        /// <param name="hints">decode hints if applicable</param>
        /// <returns>name of guessed encoding; at the moment will only guess one of:
        /// {@link #SHIFT_JIS}, {@link #UTF8}, {@link #ISO88591}, or the platform
        /// default encoding if none of these can possibly be correct</returns>
        public static string GuessEncoding(byte[] bytes, IDictionary<DecodeHintType, object> hints)
        {
            if (hints != null && hints.ContainsKey(DecodeHintType.CHARACTER_SET))
            {
                string characterSet = (string)hints[DecodeHintType.CHARACTER_SET];
                if (characterSet != null)
                {
                    return characterSet;
                }
            }

            // For now, merely tries to distinguish ISO-8859-1, UTF-8 and Shift_JIS,
            // which should be by far the most common encodings.
            int length = bytes.Length;
            bool canBeIso88591 = true;
            bool canBeShiftJis = true;
            bool canBeUtf8 = true;
            int utf8BytesLeft = 0;
            int utf2BytesChars = 0;
            int utf3BytesChars = 0;
            int utf4BytesChars = 0;
            int sjisBytesLeft = 0;
            int sjisKatakanaChars = 0;
            int sjisCurKatakanaWordLength = 0;
            int sjisCurDoubleBytesWordLength = 0;
            int sjisMaxKatakanaWordLength = 0;
            int sjisMaxDoubleBytesWordLength = 0;
            int isoHighOther = 0;

            bool utf8Bom = bytes.Length > 3 &&
                bytes[0] == 0xEF &&
                bytes[1] == 0xBB &&
                bytes[2] == 0xBF;

            for (int i = 0;
                 i < length && (canBeIso88591 || canBeShiftJis || canBeUtf8);
                 i++)
            {

                int value = bytes[i];

                // UTF-8 stuff
                if (canBeUtf8)
                {
                    if (utf8BytesLeft > 0)
                    {
                        if ((value & 0x80) == 0)
                        {
                            canBeUtf8 = false;
                        }
                        else
                        {
                            utf8BytesLeft--;
                        }
                    }
                    else if ((value & 0x80) != 0)
                    {
                        if ((value & 0x40) == 0)
                        {
                            canBeUtf8 = false;
                        }
                        else
                        {
                            utf8BytesLeft++;
                            if ((value & 0x20) == 0)
                            {
                                utf2BytesChars++;
                            }
                            else
                            {
                                utf8BytesLeft++;
                                if ((value & 0x10) == 0)
                                {
                                    utf3BytesChars++;
                                }
                                else
                                {
                                    utf8BytesLeft++;
                                    if ((value & 0x08) == 0)
                                    {
                                        utf4BytesChars++;
                                    }
                                    else
                                    {
                                        canBeUtf8 = false;
                                    }
                                }
                            }
                        }
                    }
                }

                // ISO-8859-1 stuff
                if (canBeIso88591)
                {
                    if (value > 0x7F && value < 0xA0)
                    {
                        canBeIso88591 = false;
                    }
                    else if (value > 0x9F)
                    {
                        if (value < 0xC0 || value == 0xD7 || value == 0xF7)
                        {
                            isoHighOther++;
                        }
                    }
                }

                // Shift_JIS stuff
                if (canBeShiftJis)
                {
                    if (sjisBytesLeft > 0)
                    {
                        if (value < 0x40 || value == 0x7F || value > 0xFC)
                        {
                            canBeShiftJis = false;
                        }
                        else
                        {
                            sjisBytesLeft--;
                        }
                    }
                    else if (value == 0x80 || value == 0xA0 || value > 0xEF)
                    {
                        canBeShiftJis = false;
                    }
                    else if (value > 0xA0 && value < 0xE0)
                    {
                        sjisKatakanaChars++;
                        sjisCurDoubleBytesWordLength = 0;
                        sjisCurKatakanaWordLength++;
                        if (sjisCurKatakanaWordLength > sjisMaxKatakanaWordLength)
                        {
                            sjisMaxKatakanaWordLength = sjisCurKatakanaWordLength;
                        }
                    }
                    else if (value > 0x7F)
                    {
                        sjisBytesLeft++;
                        //sjisDoubleBytesChars++;
                        sjisCurKatakanaWordLength = 0;
                        sjisCurDoubleBytesWordLength++;
                        if (sjisCurDoubleBytesWordLength > sjisMaxDoubleBytesWordLength)
                        {
                            sjisMaxDoubleBytesWordLength = sjisCurDoubleBytesWordLength;
                        }
                    }
                    else
                    {
                        //sjisLowChars++;
                        sjisCurKatakanaWordLength = 0;
                        sjisCurDoubleBytesWordLength = 0;
                    }
                }
            }

            if (canBeUtf8 && utf8BytesLeft > 0)
            {
                canBeUtf8 = false;
            }
            if (canBeShiftJis && sjisBytesLeft > 0)
            {
                canBeShiftJis = false;
            }

            // Easy -- if there is BOM or at least 1 valid not-single byte character (and no evidence it can't be UTF-8), done
            if (canBeUtf8 && (utf8Bom || utf2BytesChars + utf3BytesChars + utf4BytesChars > 0))
            {
                return UTF8;
            }
            // Easy -- if assuming Shift_JIS or >= 3 valid consecutive not-ascii characters (and no evidence it can't be), done
            if (canBeShiftJis && (ASSUME_SHIFT_JIS || sjisMaxKatakanaWordLength >= 3 || sjisMaxDoubleBytesWordLength >= 3))
            {
                return SHIFT_JIS;
            }
            // Distinguishing Shift_JIS and ISO-8859-1 can be a little tough for short words. The crude heuristic is:
            // - If we saw
            //   - only two consecutive katakana chars in the whole text, or
            //   - at least 10% of bytes that could be "upper" not-alphanumeric Latin1,
            // - then we conclude Shift_JIS, else ISO-8859-1
            if (canBeIso88591 && canBeShiftJis)
            {
                return sjisMaxKatakanaWordLength == 2 && sjisKatakanaChars == 2 || isoHighOther * 10 >= length
                    ? SHIFT_JIS : ISO88591;
            }

            // Otherwise, try in order ISO-8859-1, Shift JIS, UTF-8 and fall back to default platform encoding
            if (canBeIso88591)
            {
                return ISO88591;
            }
            if (canBeShiftJis)
            {
                return SHIFT_JIS;
            }
            if (canBeUtf8)
            {
                return UTF8;
            }
            // Otherwise, we take a wild guess with platform encoding
            return PLATFORM_DEFAULT_ENCODING;
        }
    }
}