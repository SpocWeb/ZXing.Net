/*
 * Copyright 2012 ZXing authors
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
using ZXing.PDF417.Internal;

namespace ZXing.PDF417
{
    /// <summary>
    /// <author>Jacob Haynes</author>
    /// <author>qwandor@google.com (Andrew Walbran)</author>
    /// </summary>
    public sealed class Pdf417Writer : IBarCodeWriter
    {
        /// <summary>
        /// default white space (margin) around the code
        /// </summary>
        const int WHITE_SPACE = 30;

        /// <summary>
        /// default error correction level
        /// </summary>
        const int DEFAULT_ERROR_CORRECTION_LEVEL = 2;
        /// <summary>
        /// default aspect ratio
        /// </summary>
        const int DEFAULT_ASPECT_RATIO = 4;

        /// <summary>
        /// </summary>
        /// <param name="contents">The contents to encode in the barcode</param>
        /// <param name="format">The barcode format to generate</param>
        /// <param name="width">The preferred width in pixels</param>
        /// <param name="height">The preferred height in pixels</param>
        /// <param name="hints">Additional parameters to supply to the encoder</param>
        /// <returns>
        /// The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)
        /// </returns>
        public BitMatrix Encode(string contents,
                                BarcodeFormat format,
                                int width,
                                int height,
                                IDictionary<EncodeHintType, object> hints = null)
        {
            if (format != BarcodeFormat.PDF_417)
            {
                throw new ArgumentException("Can only encode PDF_417, but got " + format);
            }

            var encoder = new Internal.PDF417();
            var margin = WHITE_SPACE;
            var errorCorrectionLevel = DEFAULT_ERROR_CORRECTION_LEVEL;
            var aspectRatio = DEFAULT_ASPECT_RATIO;

            if (hints != null)
            {
                if (hints.ContainsKey(EncodeHintType.PDF417_COMPACT) && hints[EncodeHintType.PDF417_COMPACT] != null)
                {
                    encoder.setCompact(Convert.ToBoolean(hints[EncodeHintType.PDF417_COMPACT].ToString()));
                }
                if (hints.ContainsKey(EncodeHintType.PDF417_COMPACTION) && hints[EncodeHintType.PDF417_COMPACTION] != null)
                {
                    if (Enum.IsDefined(typeof(Compaction), hints[EncodeHintType.PDF417_COMPACTION].ToString()))
                    {
                        var compactionEnum = (Compaction)Enum.Parse(typeof(Compaction), hints[EncodeHintType.PDF417_COMPACTION].ToString(), true);
                        encoder.setCompaction(compactionEnum);
                    }
                }
                if (hints.ContainsKey(EncodeHintType.PDF417_DIMENSIONS))
                {
                    var dimensions = (Dimensions)hints[EncodeHintType.PDF417_DIMENSIONS];
                    encoder.setDimensions(dimensions.MaxCols,
                                          dimensions.MinCols,
                                          dimensions.MaxRows,
                                          dimensions.MinRows);
                }
                if (hints.ContainsKey(EncodeHintType.MARGIN) && hints[EncodeHintType.MARGIN] != null)
                {
                    margin = Convert.ToInt32(hints[EncodeHintType.MARGIN].ToString());
                }
                if (hints.ContainsKey(EncodeHintType.PDF417_ASPECT_RATIO) && hints[EncodeHintType.PDF417_ASPECT_RATIO] != null)
                {
                    var value = hints[EncodeHintType.PDF417_ASPECT_RATIO];
                    if (value is PDF417AspectRatio ||
                        value is int)
                    {
                        aspectRatio = (int)value;
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(PDF417AspectRatio), value.ToString()))
                        {
                            var aspectRatioEnum = (PDF417AspectRatio)Enum.Parse(typeof(PDF417AspectRatio), value.ToString(), true);
                            aspectRatio = (int)aspectRatioEnum;
                        }
                    }
                }
                if (hints.ContainsKey(EncodeHintType.PDF417_IMAGE_ASPECT_RATIO) && hints[EncodeHintType.PDF417_IMAGE_ASPECT_RATIO] != null)
                {
                    var value = hints[EncodeHintType.PDF417_IMAGE_ASPECT_RATIO];
                    try
                    {
                        encoder.setDesiredAspectRatio(Convert.ToSingle(value));
                    }
                    catch
                    {
                        // User passed in something that wasn't convertible to single.
                    }
                }
                if (hints.ContainsKey(EncodeHintType.ERROR_CORRECTION) && hints[EncodeHintType.ERROR_CORRECTION] != null)
                {
                    var value = hints[EncodeHintType.ERROR_CORRECTION];
                    if (value is PDF417ErrorCorrectionLevel ||
                        value is int)
                    {
                        errorCorrectionLevel = (int)value;
                    }
                    else
                    {
                        if (Enum.IsDefined(typeof(PDF417ErrorCorrectionLevel), value.ToString()))
                        {
                            var errorCorrectionLevelEnum = (PDF417ErrorCorrectionLevel)Enum.Parse(typeof(PDF417ErrorCorrectionLevel), value.ToString(), true);
                            errorCorrectionLevel = (int)errorCorrectionLevelEnum;
                        }
                    }
                }
                if (hints.ContainsKey(EncodeHintType.CHARACTER_SET))
                {
#if !SILVERLIGHT || WINDOWS_PHONE
                    var encoding = (string)hints[EncodeHintType.CHARACTER_SET];
                    if (encoding != null)
                    {
                        encoder.setEncoding(encoding);
                    }
#else
               // Silverlight supports only UTF-8 and UTF-16 out-of-the-box
               encoder.setEncoding("UTF-8");
#endif
                }
                if (hints.ContainsKey(EncodeHintType.DISABLE_ECI) && hints[EncodeHintType.DISABLE_ECI] != null)
                {
                    encoder.setDisableEci(Convert.ToBoolean(hints[EncodeHintType.DISABLE_ECI].ToString()));
                }

                // Check for PDF417 Macro options
                if (hints.ContainsKey(EncodeHintType.PDF417_MACRO_META_DATA))
                {
                    encoder.setMetaData((PDF417MacroMetadata)hints[EncodeHintType.PDF417_MACRO_META_DATA]);
                }
            }

            return BitMatrixFromEncoder(encoder, contents, errorCorrectionLevel, width, height, margin, aspectRatio);
        }

        /// <summary>
        /// Takes encoder, accounts for width/height, and retrieves bit matrix
        /// </summary>
        static BitMatrix BitMatrixFromEncoder(Internal.PDF417 encoder,
                                                      string contents,
                                                      int errorCorrectionLevel,
                                                      int width,
                                                      int height,
                                                      int margin,
                                                      int aspectRatio)
        {
            if (width >= height) {
                encoder.generateBarcodeLogic(contents, errorCorrectionLevel, width, height, ref aspectRatio);
            } else {
                encoder.generateBarcodeLogic(contents, errorCorrectionLevel, height, width, ref aspectRatio);
            }

            sbyte[][] originalScale = encoder.BarcodeMatrix.getScaledMatrix(1, aspectRatio);
            bool rotated = false;
            if (height > width != originalScale[0].Length < originalScale.Length)
            {
                originalScale = RotateArray(originalScale);
                rotated = true;
            }

            int scaleX = width / originalScale[0].Length;
            int scaleY = height / originalScale.Length;

            int scale;
            if (scaleX < scaleY)
            {
                scale = scaleX;
            }
            else
            {
                scale = scaleY;
            }

            if (scale > 1)
            {
                sbyte[][] scaledMatrix =
                   encoder.BarcodeMatrix.getScaledMatrix(scale, scale * aspectRatio);
                if (rotated)
                {
                    scaledMatrix = RotateArray(scaledMatrix);
                }
                return BitMatrixFromBitArray(scaledMatrix, margin);
            }
            return BitMatrixFromBitArray(originalScale, margin);
        }

        /// <summary>
        /// This takes an array holding the values of the PDF 417
        /// </summary>
        /// <param name="input">a byte array of information with 0 is black, and 1 is white</param>
        /// <param name="margin">border around the barcode</param>
        /// <returns>BitMatrix of the input</returns>
        static BitMatrix BitMatrixFromBitArray(IReadOnlyList<sbyte[]> input, int margin)
        {
            // Creates the bit matrix with extra space for whitespace
            var output = new BitMatrix(input[0].Length + 2 * margin, input.Count + 2 * margin);
            var yOutput = output.Height - margin - 1;
            for (int y = 0; y < input.Count; y++, yOutput--)
            {
                var currentInput = input[y];
                var currentInputLength = currentInput.Length;
                for (int x = 0; x < currentInputLength; x++)
                {
                    // Zero is white in the bytematrix
                    if (currentInput[x] == 1)
                    {
                        output[x + margin, yOutput] = true;
                    }
                }
            }
            return output;
        }

        /// <summary>
        /// Takes and rotates the it 90 degrees
        /// </summary>
        static sbyte[][] RotateArray(IReadOnlyList<sbyte[]> bitArray)
        {
            sbyte[][] temp = new sbyte[bitArray[0].Length][];
            for (int idx = 0; idx < bitArray[0].Length; idx++)
            {
                temp[idx] = new sbyte[bitArray.Count];
            }

            for (int ii = 0; ii < bitArray.Count; ii++)
            {
                // This makes the direction consistent on screen when rotating the
                // screen;
                int inverse = bitArray.Count - ii - 1;
                for (int jj = 0; jj < bitArray[0].Length; jj++)
                {
                    temp[jj][inverse] = bitArray[ii][jj];
                }
            }
            return temp;
        }
    }
}