﻿/*
 * Copyright 2013 ZXing.Net authors
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
    /// This object renders a MSI code as a <see cref="BitMatrix"/>.
    /// </summary>
    public sealed class MsiWriter : OneDimensionalCodeWriter
    {

        static readonly int[] startWidths = { 2, 1 };
        static readonly int[] endWidths = { 1, 2, 1 };

        static readonly int[][] numberWidths = {
                                                           new[] { 1, 2, 1, 2, 1, 2, 1, 2 },
                                                           new[] { 1, 2, 1, 2, 1, 2, 2, 1 },
                                                           new[] { 1, 2, 1, 2, 2, 1, 1, 2 },
                                                           new[] { 1, 2, 1, 2, 2, 1, 2, 1 },
                                                           new[] { 1, 2, 2, 1, 1, 2, 1, 2 },
                                                           new[] { 1, 2, 2, 1, 1, 2, 2, 1 },
                                                           new[] { 1, 2, 2, 1, 2, 1, 1, 2 },
                                                           new[] { 1, 2, 2, 1, 2, 1, 2, 1 },
                                                           new[] { 2, 1, 1, 2, 1, 2, 1, 2 },
                                                           new[] { 2, 1, 1, 2, 1, 2, 2, 1 }
                                                        };

        static readonly IList<BarcodeFormat> supportedWriteFormats = new List<BarcodeFormat> { BarcodeFormat.MSI };

        /// <summary>
        /// returns supported formats
        /// </summary>
        protected override IList<BarcodeFormat> SupportedWriteFormats => supportedWriteFormats;

        /// <summary>
        /// Encode the contents to byte array expression of one-dimensional barcode.
        /// Start code and end code should be included in result, and side margins should not be included.
        /// <returns>a {@code boolean[]} of horizontal pixels (false = white, true = black)</returns>
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public override bool[] Encode(string contents)
        {
            var length = contents.Length;
            for (var i = 0; i < length; i++)
            {
                int indexInString = MsiReader.ALPHABET_STRING.IndexOf(contents[i]);
                if (indexInString < 0) {
                    throw new ArgumentException("Requested contents contains a not encodable character: '" + contents[i] + "'");
                }
            }

            var codeWidth = 3 + length * 12 + 4;
            var result = new bool[codeWidth];
            var pos = AppendPattern(result, 0, startWidths, true);
            for (var i = 0; i < length; i++)
            {
                var indexInString = MsiReader.ALPHABET_STRING.IndexOf(contents[i]);
                var widths = numberWidths[indexInString];
                pos += AppendPattern(result, pos, widths, true);
            }
            AppendPattern(result, pos, endWidths, true);
            return result;
        }
    }
}