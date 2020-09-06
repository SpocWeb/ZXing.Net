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
    /// This object renders a ITF code as a <see cref="BitMatrix" />.
    /// 
    /// <author>erik.barbara@gmail.com (Erik Barbara)</author>
    /// </summary>
    public sealed class ITFWriter : OneDimensionalCodeWriter
    {
        private static readonly int[] START_PATTERN = { 1, 1, 1, 1 };
        private static readonly int[] END_PATTERN = { 3, 1, 1 };

        private const int W = 3; // Pixel width of a 3x wide line
        private const int N = 1; // Pixed width of a narrow line

        internal static int[][] PATTERNS = {
         new[] {N, N, W, W, N}, // 0
         new[] {W, N, N, N, W}, // 1
         new[] {N, W, N, N, W}, // 2
         new[] {W, W, N, N, N}, // 3
         new[] {N, N, W, N, W}, // 4
         new[] {W, N, W, N, N}, // 5
         new[] {N, W, W, N, N}, // 6
         new[] {N, N, N, W, W}, // 7
         new[] {W, N, N, W, N}, // 8
         new[] {N, W, N, W, N} // 9
        };

        private static readonly IList<BarcodeFormat> supportedWriteFormats = new List<BarcodeFormat> { BarcodeFormat.ITF };

        /// <summary>
        /// returns supported formats
        /// </summary>
        protected override IList<BarcodeFormat> SupportedWriteFormats => supportedWriteFormats;

        /// <summary>
        /// Encode the contents to bool array expression of one-dimensional barcode.
        /// Start code and end code should be included in result, and side margins should not be included.
        /// <returns>a {@code bool[]} of horizontal pixels (false = white, true = black)</returns>
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public override bool[] Encode(string contents)
        {
            int length = contents.Length;
            if (length % 2 != 0)
            {
                throw new ArgumentException("The length of the input should be even");
            }
            if (length > 80)
            {
                throw new ArgumentException(
                    "Requested contents should be less than 80 digits long, but got " + length);
            }
            for (var i = 0; i < length; i++)
            {
                if (!char.IsDigit(contents[i])) {
                    throw new ArgumentException("Requested contents should only contain digits, but got '" + contents[i] + "'");
                }
            }

            CheckNumeric(contents);

            var result = new bool[9 + 9 * length];
            int pos = AppendPattern(result, 0, START_PATTERN, true);
            for (int i = 0; i < length; i += 2)
            {
                int one = Convert.ToInt32(contents[i].ToString(), 10);
                int two = Convert.ToInt32(contents[i + 1].ToString(), 10);
                int[] encoding = new int[10];
                for (int j = 0; j < 5; j++)
                {
                    encoding[j << 1] = PATTERNS[one][j];
                    encoding[(j << 1) + 1] = PATTERNS[two][j];
                }
                pos += AppendPattern(result, pos, encoding, true);
            }
            AppendPattern(result, pos, END_PATTERN, true);

            return result;
        }
    }
}