/*
 * Copyright 2013 ZXing authors
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
using System.Globalization;
using System.Text;
using ZXing.Common;
using ZXing.Common.Detector;
using ZXing.PDF417.Internal.EC;

namespace ZXing.PDF417.Internal
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Guenther Grau</author>
    public static class Pdf417ScanningDecoder
    {

        const int CODEWORD_SKEW_SIZE = 2;

        const int MAX_ERRORS = 3;
        const int MAX_EC_CODEWORDS = 512;
        static readonly ErrorCorrection ERROR_CORRECTION = new ErrorCorrection();

        /// <summary>
        /// Decode the specified image, imageTopLeft, imageBottomLeft, imageTopRight, imageBottomRight, minCodewordWidth
        /// and maxCodewordWidth.
        /// TODO: don't pass in minCodewordWidth and maxCodewordWidth, pass in barcode columns for start and stop pattern
        /// columns. That way width can be deducted from the pattern column.
        /// This approach also allows to detect more details about the barcode, e.g. if a bar type (white or black) is wider 
        /// than it should be. This can happen if the scanner used a bad blackPoint.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="imageTopLeft">Image top left.</param>
        /// <param name="imageBottomLeft">Image bottom left.</param>
        /// <param name="imageTopRight">Image top right.</param>
        /// <param name="imageBottomRight">Image bottom right.</param>
        /// <param name="minCodewordWidth">Minimum codeword width.</param>
        /// <param name="maxCodewordWidth">Max codeword width.</param>
        public static DecoderResult Decode(IRoBitMatrix image,
                                           ResultPoint imageTopLeft,
                                           ResultPoint imageBottomLeft,
                                           ResultPoint imageTopRight,
                                           ResultPoint imageBottomRight,
                                           int minCodewordWidth,
                                           int maxCodewordWidth)
        {
            var boundingBox = BoundingBox.Create(image, imageTopLeft, imageBottomLeft, imageTopRight, imageBottomRight);
            if (boundingBox == null) {
                return null;
            }

            DetectionResultRowIndicatorColumn leftRowIndicatorColumn = null;
            DetectionResultRowIndicatorColumn rightRowIndicatorColumn = null;
            DetectionResult detectionResult;
            for (bool firstPass = true; ; firstPass = false)
            {
                if (imageTopLeft != null)
                {
                    leftRowIndicatorColumn = GetRowIndicatorColumn(image, boundingBox, imageTopLeft, true, minCodewordWidth, maxCodewordWidth);
                }
                if (imageTopRight != null)
                {
                    rightRowIndicatorColumn = GetRowIndicatorColumn(image, boundingBox, imageTopRight, false, minCodewordWidth, maxCodewordWidth);
                }
                detectionResult = Merge(leftRowIndicatorColumn, rightRowIndicatorColumn);
                if (detectionResult == null)
                {
                    // TODO Based on Owen's Comments in <see cref="ZXing.ReaderException"/>, this method has been modified to continue silently
                    // if a barcode was not decoded where it was detected instead of throwing a new exception object.
                    return null;
                }
                var resultBox = detectionResult.Box;
                if (firstPass && resultBox != null &&
                    (resultBox.MinY < boundingBox.MinY || resultBox.MaxY > boundingBox.MaxY))
                {
                    boundingBox = resultBox;
                }
                else
                {
                    break;
                }
            }
            detectionResult.Box = boundingBox;
            int maxBarcodeColumn = detectionResult.ColumnCount + 1;
            detectionResult.DetectionResultColumns[0] = leftRowIndicatorColumn;

            detectionResult.DetectionResultColumns[maxBarcodeColumn] = rightRowIndicatorColumn;

            bool leftToRight = leftRowIndicatorColumn != null;
            for (int barcodeColumnCount = 1; barcodeColumnCount <= maxBarcodeColumn; barcodeColumnCount++)
            {
                int barcodeColumn = leftToRight ? barcodeColumnCount : maxBarcodeColumn - barcodeColumnCount;
                if (detectionResult.DetectionResultColumns[barcodeColumn] != null)
                {
                    // This will be the case for the opposite row indicator column, which doesn't need to be decoded again.
                    continue;
                }
                DetectionResultColumn detectionResultColumn;
                if (barcodeColumn == 0 || barcodeColumn == maxBarcodeColumn)
                {
                    detectionResultColumn = new DetectionResultRowIndicatorColumn(boundingBox, barcodeColumn == 0);
                }
                else
                {
                    detectionResultColumn = new DetectionResultColumn(boundingBox);
                }
                detectionResult.DetectionResultColumns[barcodeColumn] = detectionResultColumn;
                int startColumn = -1;
                int previousStartColumn = startColumn;
                // TODO start at a row for which we know the start position, then detect upwards and downwards from there.
                for (int imageRow = boundingBox.MinY; imageRow <= boundingBox.MaxY; imageRow++)
                {
                    startColumn = GetStartColumn(detectionResult, barcodeColumn, imageRow, leftToRight);
                    if (startColumn < 0 || startColumn > boundingBox.MaxX)
                    {
                        if (previousStartColumn == -1)
                        {
                            continue;
                        }
                        startColumn = previousStartColumn;
                    }
                    Codeword codeword = DetectCodeword(image, boundingBox.MinX, boundingBox.MaxX, leftToRight,
                                                       startColumn, imageRow, minCodewordWidth, maxCodewordWidth);
                    if (codeword != null)
                    {
                        detectionResultColumn.setCodeword(imageRow, codeword);
                        previousStartColumn = startColumn;
                        minCodewordWidth = Math.Min(minCodewordWidth, codeword.Width);
                        maxCodewordWidth = Math.Max(maxCodewordWidth, codeword.Width);
                    }
                }
            }
            return CreateDecoderResult(detectionResult);
        }

        /// <summary>
        /// Merge the specified leftRowIndicatorColumn and rightRowIndicatorColumn.
        /// </summary>
        /// <param name="leftRowIndicatorColumn">Left row indicator column.</param>
        /// <param name="rightRowIndicatorColumn">Right row indicator column.</param>
        static DetectionResult Merge(DetectionResultRowIndicatorColumn leftRowIndicatorColumn,
                                             DetectionResultRowIndicatorColumn rightRowIndicatorColumn)
        {
            if (leftRowIndicatorColumn == null && rightRowIndicatorColumn == null)
            {
                return null;
            }
            BarcodeMetadata barcodeMetadata = GetBarcodeMetadata(leftRowIndicatorColumn, rightRowIndicatorColumn);
            if (barcodeMetadata == null)
            {
                return null;
            }
            BoundingBox boundingBox = BoundingBox.merge(AdjustBoundingBox(leftRowIndicatorColumn),
                                                        AdjustBoundingBox(rightRowIndicatorColumn));

            return new DetectionResult(barcodeMetadata, boundingBox);
        }

        /// <summary>
        /// Adjusts the bounding box.
        /// </summary>
        /// <returns>The bounding box.</returns>
        /// <param name="rowIndicatorColumn">Row indicator column.</param>
        static BoundingBox AdjustBoundingBox(DetectionResultRowIndicatorColumn rowIndicatorColumn)
        {
            int[] rowHeights = rowIndicatorColumn?.GetRowHeights();
            if (rowHeights == null)
            {
                return null;
            }
            int maxRowHeight = GetMax(rowHeights);
            int missingStartRows = 0;
            foreach (int rowHeight in rowHeights)
            {
                missingStartRows += maxRowHeight - rowHeight;
                if (rowHeight > 0)
                {
                    break;
                }
            }
            Codeword[] codewords = rowIndicatorColumn.Codewords;
            for (int row = 0; missingStartRows > 0 && codewords[row] == null; row++)
            {
                missingStartRows--;
            }
            int missingEndRows = 0;
            for (int row = rowHeights.Length - 1; row >= 0; row--)
            {
                missingEndRows += maxRowHeight - rowHeights[row];
                if (rowHeights[row] > 0)
                {
                    break;
                }
            }
            for (int row = codewords.Length - 1; missingEndRows > 0 && codewords[row] == null; row--)
            {
                missingEndRows--;
            }
            return rowIndicatorColumn.Box.addMissingRows(missingStartRows, missingEndRows, rowIndicatorColumn.IsLeft);
        }

        static int GetMax(IReadOnlyList<int> values)
        {
            int maxValue = -1;
            for (var index = values.Count - 1; index >= 0; index--)
            {
                maxValue = Math.Max(maxValue, values[index]);
            }
            return maxValue;
        }

        /// <summary>
        /// Gets the barcode metadata.
        /// </summary>
        /// <returns>The barcode metadata.</returns>
        /// <param name="leftRowIndicatorColumn">Left row indicator column.</param>
        /// <param name="rightRowIndicatorColumn">Right row indicator column.</param>
        static BarcodeMetadata GetBarcodeMetadata(DetectionResultRowIndicatorColumn leftRowIndicatorColumn,
                                                          DetectionResultRowIndicatorColumn rightRowIndicatorColumn)
        {

            BarcodeMetadata leftBarcodeMetadata;
            if (leftRowIndicatorColumn == null ||
                (leftBarcodeMetadata = leftRowIndicatorColumn.GetBarcodeMetadata()) == null)
            {
                return rightRowIndicatorColumn?.GetBarcodeMetadata();
            }
            BarcodeMetadata rightBarcodeMetadata;
            if (rightRowIndicatorColumn == null ||
                (rightBarcodeMetadata = rightRowIndicatorColumn.GetBarcodeMetadata()) == null)
            {
                return leftBarcodeMetadata;
            }

            if (leftBarcodeMetadata.ColumnCount != rightBarcodeMetadata.ColumnCount &&
                leftBarcodeMetadata.ErrorCorrectionLevel != rightBarcodeMetadata.ErrorCorrectionLevel &&
                leftBarcodeMetadata.RowCount != rightBarcodeMetadata.RowCount)
            {
                return null;
            }
            return leftBarcodeMetadata;
        }

        static DetectionResultRowIndicatorColumn GetRowIndicatorColumn(IRoBitMatrix image,
                                                                               BoundingBox boundingBox,
                                                                               ResultPoint startPoint,
                                                                               bool leftToRight,
                                                                               int minCodewordWidth,
                                                                               int maxCodewordWidth)
        {
            DetectionResultRowIndicatorColumn rowIndicatorColumn = new DetectionResultRowIndicatorColumn(boundingBox, leftToRight);
            for (int i = 0; i < 2; i++)
            {
                int increment = i == 0 ? 1 : -1;
                int startColumn = (int)startPoint.X;
                for (int imageRow = (int)startPoint.Y; imageRow <= boundingBox.MaxY &&
                                                        imageRow >= boundingBox.MinY; imageRow += increment)
                {
                    Codeword codeword = DetectCodeword(image, 0, image.Width, leftToRight, startColumn, imageRow,
                                                       minCodewordWidth, maxCodewordWidth);
                    if (codeword != null)
                    {
                        rowIndicatorColumn.setCodeword(imageRow, codeword);
                        if (leftToRight)
                        {
                            startColumn = codeword.StartX;
                        }
                        else
                        {
                            startColumn = codeword.EndX;
                        }
                    }
                }
            }
            return rowIndicatorColumn;
        }

        /// <summary> Adjusts the codeword count. </summary>
        static bool AdjustCodewordCount(DetectionResult detectionResult, IReadOnlyList<BarcodeValue[]> barcodeMatrix)
        {
            var barcodeMatrix01 = barcodeMatrix[0][1];
            int[] numberOfCodewords = barcodeMatrix01.getValue();
            int calculatedNumberOfCodewords = detectionResult.ColumnCount *
                                              detectionResult.RowCount -
                                              GetNumberOfEcCodeWords(detectionResult.ErrorCorrectionLevel);
            if (numberOfCodewords.Length == 0)
            {
                if (calculatedNumberOfCodewords < 1 || calculatedNumberOfCodewords > PDF417Common.MAX_CODEWORDS_IN_BARCODE)
                {
                    return false;
                }
                barcodeMatrix01.setValue(calculatedNumberOfCodewords);
            }
            else if (numberOfCodewords[0] != calculatedNumberOfCodewords)
            {
                if (calculatedNumberOfCodewords >= 1 && calculatedNumberOfCodewords <= PDF417Common.MAX_CODEWORDS_IN_BARCODE)
                {
                    // The calculated one is more reliable as it is derived from the row indicator columns
                    barcodeMatrix01.setValue(calculatedNumberOfCodewords);
                }
            }

            return true;
        }

        /// <summary>
        /// Creates the decoder result.
        /// </summary>
        /// <returns>The decoder result.</returns>
        /// <param name="detectionResult">Detection result.</param>
        static DecoderResult CreateDecoderResult(DetectionResult detectionResult)
        {
            BarcodeValue[][] barcodeMatrix = CreateBarcodeMatrix(detectionResult);
            if (barcodeMatrix == null) {
                return null;
            }

            if (!AdjustCodewordCount(detectionResult, barcodeMatrix))
            {
                return null;
            }
            List<int> erasures = new List<int>();
            int[] codewords = new int[detectionResult.RowCount * detectionResult.ColumnCount];
            List<int[]> ambiguousIndexValuesList = new List<int[]>();
            List<int> ambiguousIndexesList = new List<int>();
            for (int row = 0; row < detectionResult.RowCount; row++)
            {
                for (int column = 0; column < detectionResult.ColumnCount; column++)
                {
                    int[] values = barcodeMatrix[row][column + 1].getValue();
                    int codewordIndex = row * detectionResult.ColumnCount + column;
                    if (values.Length == 0)
                    {
                        erasures.Add(codewordIndex);
                    }
                    else if (values.Length == 1)
                    {
                        codewords[codewordIndex] = values[0];
                    }
                    else
                    {
                        ambiguousIndexesList.Add(codewordIndex);
                        ambiguousIndexValuesList.Add(values);
                    }
                }
            }
            int[][] ambiguousIndexValues = new int[ambiguousIndexValuesList.Count][];
            for (int i = 0; i < ambiguousIndexValues.Length; i++)
            {
                ambiguousIndexValues[i] = ambiguousIndexValuesList[i];
            }
            return CreateDecoderResultFromAmbiguousValues(detectionResult.ErrorCorrectionLevel, codewords,
                                                          erasures.ToArray(), ambiguousIndexesList.ToArray(), ambiguousIndexValues);
        }

        /// <summary>
        /// This method deals with the fact, that the decoding process doesn't always yield a single most likely value. The
        /// current error correction implementation doesn't deal with erasures very well, so it's better to provide a value
        /// for these ambiguous codewords instead of treating it as an erasure. The problem is that we don't know which of
        /// the ambiguous values to choose. We try decode using the first value, and if that fails, we use another of the
        /// ambiguous values and try to decode again. This usually only happens on very hard to read and decode barcodes,
        /// so decoding the normal barcodes is not affected by this.
        /// </summary>
        /// <returns>The decoder result from ambiguous values.</returns>
        /// <param name="ecLevel">Ec level.</param>
        /// <param name="codewords">Codewords.</param>
        /// <param name="erasureArray">contains the indexes of erasures.</param>
        /// <param name="ambiguousIndexes">array with the indexes that have more than one most likely value.</param>
        /// <param name="ambiguousIndexValues">two dimensional array that contains the ambiguous values. The first dimension must
        /// be the same Length as the ambiguousIndexes array.</param>
        static DecoderResult CreateDecoderResultFromAmbiguousValues(int ecLevel,
                                                                            int[] codewords,
                                                                            int[] erasureArray,
                                                                            IReadOnlyList<int> ambiguousIndexes,
                                                                            IReadOnlyList<int[]> ambiguousIndexValues)
        {
            int[] ambiguousIndexCount = new int[ambiguousIndexes.Count];

            int tries = 100;
            while (tries-- > 0)
            {
                for (int i = 0; i < ambiguousIndexCount.Length; i++)
                {
                    codewords[ambiguousIndexes[i]] = ambiguousIndexValues[i][ambiguousIndexCount[i]];
                }
                try
                {
                    var result = DecodeCodewords(codewords, ecLevel, erasureArray);
                    if (result != null) {
                        return result;
                    }
                }
                catch (ReaderException)
                {
                    // ignored, should not happen
                }
                if (ambiguousIndexCount.Length == 0)
                {
                    return null;
                }
                for (int i = 0; i < ambiguousIndexCount.Length; i++) {
                    if (ambiguousIndexCount[i] < ambiguousIndexValues[i].Length - 1)
                    {
                        ambiguousIndexCount[i]++;
                        break;
                    }
                    ambiguousIndexCount[i] = 0;
                    if (i == ambiguousIndexCount.Length - 1)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates the barcode matrix.
        /// </summary>
        /// <returns>The barcode matrix.</returns>
        /// <param name="detectionResult">Detection result.</param>
        static BarcodeValue[][] CreateBarcodeMatrix(DetectionResult detectionResult)
        {
            // Manually setup Jagged Array in C#
            var barcodeMatrix = new BarcodeValue[detectionResult.RowCount][];
            for (int row = 0; row < barcodeMatrix.Length; row++)
            {
                barcodeMatrix[row] = new BarcodeValue[detectionResult.ColumnCount + 2];
                for (int col = 0; col < barcodeMatrix[row].Length; col++)
                {
                    barcodeMatrix[row][col] = new BarcodeValue();
                }
            }

            int column = 0;
            foreach (DetectionResultColumn detectionResultColumn in detectionResult.GetDetectionResultColumns())
            {
                if (detectionResultColumn != null)
                {
                    foreach (Codeword codeword in detectionResultColumn.Codewords)
                    {
                        if (codeword != null)
                        {
                            int rowNumber = codeword.RowNumber;
                            if (rowNumber >= 0)
                            {
                                if (rowNumber >= barcodeMatrix.Length)
                                {
                                    // We have more rows than the barcode metadata allows for, ignore them.
                                    continue;
                                }
                                barcodeMatrix[rowNumber][column].setValue(codeword.Value);
                            }
                        }
                    }
                }
                column++;
            }

            return barcodeMatrix;
        }

        /// <summary>
        /// Tests to see if the Barcode Column is Valid
        /// </summary>
        /// <returns><c>true</c>, if barcode column is valid, <c>false</c> otherwise.</returns>
        /// <param name="detectionResult">Detection result.</param>
        /// <param name="barcodeColumn">Barcode column.</param>
        static bool IsValidBarcodeColumn(DetectionResult detectionResult, int barcodeColumn)
        {
            return (barcodeColumn >= 0) && (barcodeColumn < detectionResult.DetectionResultColumns.Length);
        }

        /// <summary>
        /// Gets the start column.
        /// </summary>
        /// <returns>The start column.</returns>
        /// <param name="detectionResult">Detection result.</param>
        /// <param name="barcodeColumn">Barcode column.</param>
        /// <param name="imageRow">Image row.</param>
        /// <param name="leftToRight">If set to <c>true</c> left to right.</param>
        static int GetStartColumn(DetectionResult detectionResult,
                                          int barcodeColumn,
                                          int imageRow,
                                          bool leftToRight)
        {
            int offset = leftToRight ? 1 : -1;
            Codeword codeword = null;
            if (IsValidBarcodeColumn(detectionResult, barcodeColumn - offset))
            {
                codeword = detectionResult.DetectionResultColumns[barcodeColumn - offset].getCodeword(imageRow);
            }
            if (codeword != null)
            {
                return leftToRight ? codeword.EndX : codeword.StartX;
            }
            codeword = detectionResult.DetectionResultColumns[barcodeColumn].getCodewordNearby(imageRow);
            if (codeword != null)
            {
                return leftToRight ? codeword.StartX : codeword.EndX;
            }
            if (IsValidBarcodeColumn(detectionResult, barcodeColumn - offset))
            {
                codeword = detectionResult.DetectionResultColumns[barcodeColumn - offset].getCodewordNearby(imageRow);
            }
            if (codeword != null)
            {
                return leftToRight ? codeword.EndX : codeword.StartX;
            }
            int skippedColumns = 0;

            while (IsValidBarcodeColumn(detectionResult, barcodeColumn - offset))
            {
                barcodeColumn -= offset;
                foreach (Codeword previousRowCodeword in detectionResult.DetectionResultColumns[barcodeColumn].Codewords)
                {
                    if (previousRowCodeword != null)
                    {
                        return (leftToRight ? previousRowCodeword.EndX : previousRowCodeword.StartX) +
                               offset *
                               skippedColumns *
                               (previousRowCodeword.EndX - previousRowCodeword.StartX);
                    }
                }
                skippedColumns++;
            }
            return leftToRight ? detectionResult.Box.MinX : detectionResult.Box.MaxX;
        }

        /// <summary>
        /// Detects the codeword.
        /// </summary>
        /// <returns>The codeword.</returns>
        /// <param name="image">Image.</param>
        /// <param name="minColumn">Minimum column.</param>
        /// <param name="maxColumn">Max column.</param>
        /// <param name="leftToRight">If set to <c>true</c> left to right.</param>
        /// <param name="startColumn">Start column.</param>
        /// <param name="imageRow">Image row.</param>
        /// <param name="minCodewordWidth">Minimum codeword width.</param>
        /// <param name="maxCodewordWidth">Max codeword width.</param>
        static Codeword DetectCodeword(IRoBitMatrix image,
                                               int minColumn,
                                               int maxColumn,
                                               bool leftToRight,
                                               int startColumn,
                                               int imageRow,
                                               int minCodewordWidth,
                                               int maxCodewordWidth)
        {
            startColumn = AdjustCodewordStartColumn(image, minColumn, maxColumn, leftToRight, startColumn, imageRow);
            // we usually know fairly exact now how long a codeword is. We should provide minimum and maximum expected length
            // and try to adjust the read pixels, e.g. remove single pixel errors or try to cut off exceeding pixels.
            // min and maxCodewordWidth should not be used as they are calculated for the whole barcode an can be inaccurate
            // for the current position
            int[] moduleBitCount = GetModuleBitCount(image, minColumn, maxColumn, leftToRight, startColumn, imageRow);
            if (moduleBitCount == null)
            {
                return null;
            }
            int endColumn;
            int codewordBitCount = MathUtils.Sum(moduleBitCount);
            if (leftToRight)
            {
                endColumn = startColumn + codewordBitCount;
            }
            else
            {
                for (int i = 0; i < (moduleBitCount.Length >> 1); i++)
                {
                    int tmpCount = moduleBitCount[i];
                    moduleBitCount[i] = moduleBitCount[moduleBitCount.Length - 1 - i];
                    moduleBitCount[moduleBitCount.Length - 1 - i] = tmpCount;
                }
                endColumn = startColumn;
                startColumn = endColumn - codewordBitCount;
            }
            // TODO implement check for width and correction of black and white bars
            // use start (and maybe stop pattern) to determine if blackbars are wider than white bars. If so, adjust.
            // should probably done only for codewords with a lot more than 17 bits. 
            // The following fixes 10-1.png, which has wide black bars and small white bars
            //    for (int i = 0; i < moduleBitCount.Length; i++) {
            //      if (i % 2 == 0) {
            //        moduleBitCount[i]--;
            //      } else {
            //        moduleBitCount[i]++;
            //      }
            //    }

            // We could also use the width of surrounding codewords for more accurate results, but this seems
            // sufficient for now
            if (!CheckCodewordSkew(codewordBitCount, minCodewordWidth, maxCodewordWidth))
            {
                // We could try to use the startX and endX position of the codeword in the same column in the previous row,
                // create the bit count from it and normalize it to 8. This would help with single pixel errors.
                return null;
            }

            int decodedValue = PDF417CodewordDecoder.getDecodedValue(moduleBitCount);
            int codeword = PDF417Common.getCodeword(decodedValue);
            if (codeword == -1)
            {
                return null;
            }
            return new Codeword(startColumn, endColumn, GetCodewordBucketNumber(decodedValue), codeword);
        }

        /// <summary>
        /// Gets the module bit count.
        /// </summary>
        /// <returns>The module bit count.</returns>
        /// <param name="image">Image.</param>
        /// <param name="minColumn">Minimum column.</param>
        /// <param name="maxColumn">Max column.</param>
        /// <param name="leftToRight">If set to <c>true</c> left to right.</param>
        /// <param name="startColumn">Start column.</param>
        /// <param name="imageRow">Image row.</param>
        static int[] GetModuleBitCount(IRoBitMatrix image,
                                               int minColumn,
                                               int maxColumn,
                                               bool leftToRight,
                                               int startColumn,
                                               int imageRow)
        {
            int imageColumn = startColumn;
            int[] moduleBitCount = new int[8];
            int moduleNumber = 0;
            int increment = leftToRight ? 1 : -1;
            bool previousPixelValue = leftToRight;
            while ((leftToRight ? imageColumn < maxColumn : imageColumn >= minColumn) &&
                  moduleNumber < moduleBitCount.Length)
            {
                if (image[imageColumn, imageRow] == previousPixelValue)
                {
                    moduleBitCount[moduleNumber]++;
                    imageColumn += increment;
                }
                else
                {
                    moduleNumber++;
                    previousPixelValue = !previousPixelValue;
                }
            }
            if (moduleNumber == moduleBitCount.Length ||
               ((imageColumn == (leftToRight ? maxColumn : minColumn)) &&
                 moduleNumber == moduleBitCount.Length - 1))
            {
                return moduleBitCount;
            }
            return null;
        }

        /// <summary>
        /// Gets the number of EC code words.
        /// </summary>
        /// <returns>The number of EC code words.</returns>
        /// <param name="barcodeEcLevel">Barcode EC level.</param>
        static int GetNumberOfEcCodeWords(int barcodeEcLevel)
        {
            return 2 << barcodeEcLevel;
        }

        /// <summary>
        /// Adjusts the codeword start column.
        /// </summary>
        /// <returns>The codeword start column.</returns>
        /// <param name="image">Image.</param>
        /// <param name="minColumn">Minimum column.</param>
        /// <param name="maxColumn">Max column.</param>
        /// <param name="leftToRight">If set to <c>true</c> left to right.</param>
        /// <param name="codewordStartColumn">Codeword start column.</param>
        /// <param name="imageRow">Image row.</param>
        static int AdjustCodewordStartColumn(IRoBitMatrix image,
                                                     int minColumn,
                                                     int maxColumn,
                                                     bool leftToRight,
                                                     int codewordStartColumn,
                                                     int imageRow)
        {
            int correctedStartColumn = codewordStartColumn;
            int increment = leftToRight ? -1 : 1;
            // there should be no black pixels before the start column. If there are, then we need to start earlier.
            for (int i = 0; i < 2; i++)
            {
                while ((leftToRight ? correctedStartColumn >= minColumn : correctedStartColumn < maxColumn) &&
                         leftToRight == image[correctedStartColumn, imageRow])
                {
                    if (Math.Abs(codewordStartColumn - correctedStartColumn) > CODEWORD_SKEW_SIZE)
                    {
                        return codewordStartColumn;
                    }
                    correctedStartColumn += increment;
                }
                increment = -increment;
                leftToRight = !leftToRight;
            }
            return correctedStartColumn;
        }

        /// <summary>
        /// Checks the codeword for any skew.
        /// </summary>
        /// <returns><c>true</c>, if codeword is within the skew, <c>false</c> otherwise.</returns>
        /// <param name="codewordSize">Codeword size.</param>
        /// <param name="minCodewordWidth">Minimum codeword width.</param>
        /// <param name="maxCodewordWidth">Max codeword width.</param>
        static bool CheckCodewordSkew(int codewordSize, int minCodewordWidth, int maxCodewordWidth)
        {
            return minCodewordWidth - CODEWORD_SKEW_SIZE <= codewordSize &&
                   codewordSize <= maxCodewordWidth + CODEWORD_SKEW_SIZE;
        }

        /// <summary>
        /// Decodes the codewords.
        /// </summary>
        /// <returns>The codewords.</returns>
        /// <param name="codewords">Codewords.</param>
        /// <param name="ecLevel">Ec level.</param>
        /// <param name="erasures">Erasures.</param>
        static DecoderResult DecodeCodewords(int[] codewords, int ecLevel, int[] erasures)
        {
            if (codewords.Length == 0)
            {
                return null;
            }

            int numEcCodewords = 1 << (ecLevel + 1);

            int correctedErrorsCount = CorrectErrors(codewords, erasures, numEcCodewords);
            if (correctedErrorsCount < 0)
            {
                return null;
            }
            if (!VerifyCodewordCount(codewords, numEcCodewords))
            {
                return null;
            }

            // Decode the codewords
            DecoderResult decoderResult = DecodedBitStreamParser.decode(codewords, ecLevel.ToString());
            if (decoderResult != null)
            {
                decoderResult.ErrorsCorrected = correctedErrorsCount;
                decoderResult.Erasures = erasures.Length;
            }
            return decoderResult;
        }

        /// <summary>
        /// Given data and error-correction codewords received, possibly corrupted by errors, attempts to
        /// correct the errors in-place.
        /// </summary>
        /// <returns>The errors.</returns>
        /// <param name="codewords">data and error correction codewords.</param>
        /// <param name="erasures">positions of any known erasures.</param>
        /// <param name="numEcCodewords">number of error correction codewords that are available in codewords.</param>
        static int CorrectErrors(int[] codewords, int[] erasures, int numEcCodewords)
        {
            if (erasures != null &&
                erasures.Length > numEcCodewords / 2 + MAX_ERRORS ||
                numEcCodewords < 0 ||
                numEcCodewords > MAX_EC_CODEWORDS)
            {
                // Too many errors or EC Codewords is corrupted
                return -1;

            }
            if (!ERROR_CORRECTION.decode(codewords, numEcCodewords, erasures, out var errorCount))
            {
                return -1;
            }
            return errorCount;
        }

        /// <summary>
        /// Verifies that all is well with the the codeword array.
        /// </summary>
        /// <param name="codewords">Codewords.</param>
        /// <param name="numEcCodewords">Number EC codewords.</param>
        static bool VerifyCodewordCount(IList<int> codewords, int numEcCodewords)
        {
            if (codewords.Count < 4)
            {
                // Codeword array size should be at least 4 allowing for
                // Count CW, At least one Data CW, Error Correction CW, Error Correction CW
                return false;
            }
            // The first codeword, the Symbol Length Descriptor, shall always encode the total number of data
            // codewords in the symbol, including the Symbol Length Descriptor itself, data codewords and pad
            // codewords, but excluding the number of error correction codewords.
            int numberOfCodewords = codewords[0];
            if (numberOfCodewords > codewords.Count)
            {
                return false;
            }
            if (numberOfCodewords == 0)
            {
                // Reset to the Length of the array - 8 (Allow for at least level 3 Error Correction (8 Error Codewords)
                if (numEcCodewords < codewords.Count)
                {
                    codewords[0] = codewords.Count - numEcCodewords;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the bit count for codeword.
        /// </summary>
        /// <returns>The bit count for codeword.</returns>
        /// <param name="codeword">Codeword.</param>
        static int[] GetBitCountForCodeword(int codeword)
        {
            int[] result = new int[8];
            int previousValue = 0;
            int i = result.Length - 1;
            while (true)
            {
                if ((codeword & 0x1) != previousValue)
                {
                    previousValue = codeword & 0x1;
                    i--;
                    if (i < 0)
                    {
                        break;
                    }
                }
                result[i]++;
                codeword >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Gets the codeword bucket number.
        /// </summary>
        /// <returns>The codeword bucket number.</returns>
        /// <param name="codeword">Codeword.</param>
        static int GetCodewordBucketNumber(int codeword)
            => GetCodewordBucketNumber(GetBitCountForCodeword(codeword));

        static int GetCodewordBucketNumber(IReadOnlyList<int> moduleBitCount)
            => (moduleBitCount[0] - moduleBitCount[2] + moduleBitCount[4] - moduleBitCount[6] + 9) % 9;

        public static string ToString(this IReadOnlyList<BarcodeValue[]> barcodeMatrix)
        {
            StringBuilder formatter = new StringBuilder();
            for (int row = 0; row < barcodeMatrix.Count; row++)
            {
                formatter.AppendFormat(CultureInfo.InvariantCulture, "Row {0,2}: ", row);
                for (int column = 0; column < barcodeMatrix[row].Length; column++)
                {
                    BarcodeValue barcodeValue = barcodeMatrix[row][column];
                    int[] values = barcodeValue.getValue();
                    if (values.Length == 0)
                    {
                        formatter.Append("        ");
                    }
                    else
                    {
                        formatter.AppendFormat(CultureInfo.InvariantCulture, "{0,4}({1,2})", values[0], barcodeValue.getConfidence(values[0]));
                    }
                }
                formatter.Append("\n");
            }
            return formatter.ToString();
        }
    }
}