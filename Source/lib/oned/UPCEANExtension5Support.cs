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
using ZXing.Common;

namespace ZXing.OneD
{
    /**
     * @see UPCEANExtension2Support
     */
    internal sealed class UPCEANExtension5Support
    {

        static readonly int[] CHECK_DIGIT_ENCODINGS = {
         0x18, 0x14, 0x12, 0x11, 0x0C, 0x06, 0x03, 0x0A, 0x09, 0x05
      };

        readonly int[] decodeMiddleCounters = new int[4];
        readonly StringBuilder decodeRowStringBuffer = new StringBuilder();

        internal BarCodeText decodeRow(int rowNumber, BitArray row, int[] extensionStartRange)
        {
            StringBuilder result = decodeRowStringBuffer;
            result.Length = 0;
            int end = decodeMiddle(row, extensionStartRange, result);
            if (end < 0) {
                return null;
            }

            string resultString = result.ToString();
            IDictionary<ResultMetadataType, object> extensionData = parseExtensionString(resultString);

            BarCodeText extensionResult =
                new BarCodeText(resultString,
                           null,
                           new[] {
                       new ResultPoint((extensionStartRange[0] + extensionStartRange[1]) / 2.0f, rowNumber),
                       new ResultPoint(end, rowNumber),
                      },
                           BarcodeFormat.UPC_EAN_EXTENSION);
            if (extensionData != null)
            {
                extensionResult.PutAllMetadata(extensionData);
            }
            return extensionResult;
        }

        int decodeMiddle(BitArray row, int[] startRange, StringBuilder resultString)
        {
            int[] counters = decodeMiddleCounters;
            counters[0] = 0;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;
            int end = row.Size;
            int rowOffset = startRange[1];

            int lgPatternFound = 0;

            for (int x = 0; x < 5 && rowOffset < end; x++)
            {
                if (!UpcEanReader.DecodeDigit(row, counters, rowOffset, UpcEanReader.L_AND_G_PATTERNS, out var bestMatch)) {
                    return -1;
                }
                resultString.Append((char)('0' + bestMatch % 10));
                foreach (int counter in counters)
                {
                    rowOffset += counter;
                }
                if (bestMatch >= 10)
                {
                    lgPatternFound |= 1 << (4 - x);
                }
                if (x != 4)
                {
                    // Read off separator if not last
                    rowOffset = row.GetNextSet(rowOffset);
                    rowOffset = row.GetNextUnset(rowOffset);
                }
            }

            if (resultString.Length != 5)
            {
                return -1;
            }

            if (!determineCheckDigit(lgPatternFound, out var checkDigit)) {
                return -1;
            }

            if (extensionChecksum(resultString.ToString()) != checkDigit)
            {
                return -1;
            }

            return rowOffset;
        }

        static int extensionChecksum(string s)
        {
            int length = s.Length;
            int sum = 0;
            for (int i = length - 2; i >= 0; i -= 2)
            {
                sum += s[i] - '0';
            }
            sum *= 3;
            for (int i = length - 1; i >= 0; i -= 2)
            {
                sum += s[i] - '0';
            }
            sum *= 3;
            return sum % 10;
        }

        static bool determineCheckDigit(int lgPatternFound, out int checkDigit)
        {
            for (checkDigit = 0; checkDigit < 10; checkDigit++)
            {
                if (lgPatternFound == CHECK_DIGIT_ENCODINGS[checkDigit])
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Parses the extension string.
        /// </summary>
        /// <param name="raw">raw content of extension</param>
        /// <returns>formatted interpretation of raw content as a {@link Map} mapping
        /// one {@link ResultMetadataType} to appropriate value, or {@code null} if not known</returns>
        static IDictionary<ResultMetadataType, object> parseExtensionString(string raw)
        {
            if (raw.Length != 5)
            {
                return null;
            }
            object value = parseExtension5String(raw);
            if (value == null)
            {
                return null;
            }
            IDictionary<ResultMetadataType, object> result = new Dictionary<ResultMetadataType, object>
            {
                [ResultMetadataType.SUGGESTED_PRICE] = value
            };
            return result;
        }

        static string parseExtension5String(string raw)
        {
            string currency;
            switch (raw[0])
            {
                case '0':
                    currency = "£";
                    break;
                case '5':
                    currency = "$";
                    break;
                case '9':
                    // Reference: http://www.jollytech.com
                    if ("90000".Equals(raw))
                    {
                        // No suggested retail price
                        return null;
                    }
                    if ("99991".Equals(raw))
                    {
                        // Complementary
                        return "0.00";
                    }
                    if ("99990".Equals(raw))
                    {
                        return "Used";
                    }
                    // Otherwise... unknown currency?
                    currency = "";
                    break;
                default:
                    currency = "";
                    break;
            }
            int rawAmount = int.Parse(raw.Substring(1));
            string unitsString = (rawAmount / 100).ToString();
            int hundredths = rawAmount % 100;
            string hundredthsString = hundredths < 10 ? "0" + hundredths : hundredths.ToString();
            return currency + unitsString + '.' + hundredthsString;
        }
    }
}