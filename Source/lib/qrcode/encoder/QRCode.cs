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

using System.Text;

namespace ZXing.QrCode.Internal
{
    /// <author>satorux@google.com (Satoru Takabayashi) - creator</author>
    /// <author>dswitkin@google.com (Daniel Switkin) - ported from C++</author>
    public sealed class QrCode
    {
        /// <summary>
        /// 
        /// </summary>
        public static int NUM_MASK_PATTERNS = 8;

        public QrCode()
        {
            MaskPattern = -1;
        }

        public Mode Mode { get; set; }

        public ErrorCorrectionLevel EcLevel { get; set; }

        public Version Version { get; set; }

        public int MaskPattern { get; set; }

        public ByteMatrix Matrix { get; set; }

        public override string ToString()
        {
            var result = new StringBuilder(200);
            result.Append("<<\n");
            result.Append(" mode: ");
            result.Append(Mode);
            result.Append("\n ecLevel: ");
            result.Append(EcLevel);
            result.Append("\n version: ");
            if (Version == null) {
                result.Append("null");
            } else {
                result.Append(Version);
            }
            result.Append("\n maskPattern: ");
            result.Append(MaskPattern);
            if (Matrix == null)
            {
                result.Append("\n matrix: null\n");
            }
            else
            {
                result.Append("\n matrix:\n");
                result.Append(Matrix);
            }
            result.Append(">>\n");
            return result.ToString();
        }

        public static bool IsValidMaskPattern(int maskPattern)
            => maskPattern >= 0 && maskPattern < NUM_MASK_PATTERNS;

    }
}