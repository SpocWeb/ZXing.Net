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

/*
 * These authors would like to acknowledge the Spanish Ministry of Industry,
 * Tourism and Trade, for the support in the project TSI020301-2008-2
 * "PIRAmIDE: Personalizable Interactions with Resources on AmI-enabled
 * Mobile Dynamic Environments", led by Treelogic
 * ( http://www.treelogic.com/ ):
 *
 *   http://www.piramidepse.com/
 */

using System;
using System.Collections.Generic;
using ZXing.Common;
using ZXing.Common.Detector;
using ZXing.OneD.RSS.Expanded.Decoders;

namespace ZXing.OneD.RSS.Expanded
{
    /// <summary>
    /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
    /// <author>Eduardo Castillejo, University of Deusto (eduardo.castillejo@deusto.es)</author>
    /// </summary>
    public sealed class RssExpandedReader : AbstractRSSReader
    {

        static readonly int[] SYMBOL_WIDEST = {7, 5, 4, 3, 1};
        static readonly int[] EVEN_TOTAL_SUBSET = {4, 20, 52, 104, 204};
        static readonly int[] GSUM = {0, 348, 1388, 2948, 3988};

        static readonly int[][] FINDER_PATTERNS =
        {
            new[] {1, 8, 4, 1}, // A
            new[] {3, 6, 4, 1}, // B
            new[] {3, 4, 6, 1}, // C
            new[] {3, 2, 8, 1}, // D
            new[] {2, 6, 5, 1}, // E
            new[] {2, 2, 9, 1} // F
        };

        static readonly int[][] WEIGHTS =
        {
            new[] {1, 3, 9, 27, 81, 32, 96, 77},
            new[] {20, 60, 180, 118, 143, 7, 21, 63},
            new[] {189, 145, 13, 39, 117, 140, 209, 205},
            new[] {193, 157, 49, 147, 19, 57, 171, 91},
            new[] {62, 186, 136, 197, 169, 85, 44, 132},
            new[] {185, 133, 188, 142, 4, 12, 36, 108},
            new[] {113, 128, 173, 97, 80, 29, 87, 50},
            new[] {150, 28, 84, 41, 123, 158, 52, 156},
            new[] {46, 138, 203, 187, 139, 206, 196, 166},
            new[] {76, 17, 51, 153, 37, 111, 122, 155},
            new[] {43, 129, 176, 106, 107, 110, 119, 146},
            new[] {16, 48, 144, 10, 30, 90, 59, 177},
            new[] {109, 116, 137, 200, 178, 112, 125, 164},
            new[] {70, 210, 208, 202, 184, 130, 179, 115},
            new[] {134, 191, 151, 31, 93, 68, 204, 190},
            new[] {148, 22, 66, 198, 172, 94, 71, 2},
            new[] {6, 18, 54, 162, 64, 192, 154, 40},
            new[] {120, 149, 25, 75, 14, 42, 126, 167},
            new[] {79, 26, 78, 23, 69, 207, 199, 175},
            new[] {103, 98, 83, 38, 114, 131, 182, 124},
            new[] {161, 61, 183, 127, 170, 88, 53, 159},
            new[] {55, 165, 73, 8, 24, 72, 5, 15},
            new[] {45, 135, 194, 160, 58, 174, 100, 89}
        };

        const int FINDER_PAT_A = 0;
        const int FINDER_PAT_B = 1;
        const int FINDER_PAT_C = 2;
        const int FINDER_PAT_D = 3;
        const int FINDER_PAT_E = 4;
        const int FINDER_PAT_F = 5;

        static readonly int[][] FINDER_PATTERN_SEQUENCES =
        {
            new[] {FINDER_PAT_A, FINDER_PAT_A},
            new[] {FINDER_PAT_A, FINDER_PAT_B, FINDER_PAT_B},
            new[] {FINDER_PAT_A, FINDER_PAT_C, FINDER_PAT_B, FINDER_PAT_D},
            new[] {FINDER_PAT_A, FINDER_PAT_E, FINDER_PAT_B, FINDER_PAT_D, FINDER_PAT_C},
            new[] {FINDER_PAT_A, FINDER_PAT_E, FINDER_PAT_B, FINDER_PAT_D, FINDER_PAT_D, FINDER_PAT_F},
            new[] {FINDER_PAT_A, FINDER_PAT_E, FINDER_PAT_B, FINDER_PAT_D, FINDER_PAT_E, FINDER_PAT_F, FINDER_PAT_F},
            new[] {FINDER_PAT_A, FINDER_PAT_A, FINDER_PAT_B, FINDER_PAT_B, FINDER_PAT_C, FINDER_PAT_C, FINDER_PAT_D, FINDER_PAT_D},
            new[] {FINDER_PAT_A, FINDER_PAT_A, FINDER_PAT_B, FINDER_PAT_B, FINDER_PAT_C, FINDER_PAT_C, FINDER_PAT_D, FINDER_PAT_E, FINDER_PAT_E},
            new[] {FINDER_PAT_A, FINDER_PAT_A, FINDER_PAT_B, FINDER_PAT_B, FINDER_PAT_C, FINDER_PAT_C, FINDER_PAT_D, FINDER_PAT_E, FINDER_PAT_F, FINDER_PAT_F},
            new[] {FINDER_PAT_A, FINDER_PAT_A, FINDER_PAT_B, FINDER_PAT_B, FINDER_PAT_C, FINDER_PAT_D, FINDER_PAT_D, FINDER_PAT_E, FINDER_PAT_E, FINDER_PAT_F, FINDER_PAT_F},
        };

        const int MAX_PAIRS = 11;

        readonly List<ExpandedPair> _Pairs = new List<ExpandedPair>(MAX_PAIRS);
        readonly List<ExpandedRow> _Rows = new List<ExpandedRow>();
        readonly int[] _StartEnd = new int[2];
        bool _StartFromEven;

        public IReadOnlyList<ExpandedPair> Pairs => _Pairs;

        /// <summary>
        ///   <p>Attempts to decode a one-dimensional barcode format given a single row of
        /// an image.</p>
        /// </summary>
        /// <param name="rowNumber">row number from top of the row</param>
        /// <param name="row">the black/white pixel data of the row</param>
        /// <param name="hints">decode hints</param>
        /// <returns>
        ///   <see cref="BarCodeText"/>containing encoded string and start/end of barcode or null, if an error occurs or barcode cannot be found
        /// </returns>
        public override BarCodeText DecodeRow(int rowNumber,
            BitArray row,
            IDictionary<DecodeHintType, object> hints)
        {
            // Rows can start with even pattern in case in prev rows there where odd number of patters.
            // So lets try twice
            _Pairs.Clear();
            _StartFromEven = false;
            if (DecodeRow2Pairs(rowNumber, row)) {
                return ConstructResult(_Pairs);
            }

            _Pairs.Clear();
            _StartFromEven = true;
            if (DecodeRow2Pairs(rowNumber, row)) {
                return ConstructResult(_Pairs);
            }
            return null;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public override void Reset()
        {
            _Pairs.Clear();
            _Rows.Clear();
        }

        public bool DecodeRow2Pairs(int rowNumber, BitArray row)
        {
            while (true)
            {
                ExpandedPair nextPair = RetrieveNextPair(row, _Pairs, rowNumber);
                if (nextPair == null) {
                    break;
                }
                _Pairs.Add(nextPair);
                // exit this loop when retrieveNextPair() fails and throws
            }
            if (_Pairs.Count == 0)
            {
                return false;
            }

            // TODO: verify sequence of finder patterns as in checkPairSequence()
            if (CheckChecksum())
            {
                return true;
            }

            bool tryStackedDecode = _Rows.Count != 0;
            StoreRow(rowNumber, false); // TODO: deal with reversed rows
            if (tryStackedDecode)
            {
                // When the image is 180-rotated, then rows are sorted in wrong direction.
                // Try twice with both the directions.
                List<ExpandedPair> ps = CheckRows(false);
                if (ps != null)
                {
                    return true;
                }
                ps = CheckRows(true);
                if (ps != null)
                {
                    return true;
                }
            }

            return false;
        }

        List<ExpandedPair> CheckRows(bool reverse)
        {
            // Limit number of rows we are checking
            // We use recursive algorithm with pure complexity and don't want it to take forever
            // Stacked barcode can have up to 11 rows, so 25 seems reasonable enough
            if (_Rows.Count > 25)
            {
                _Rows.Clear(); // We will never have a chance to get result, so clear it
                return null;
            }

            _Pairs.Clear();
            if (reverse)
            {
                _Rows.Reverse();
            }

            List<ExpandedPair> ps = CheckRows(new List<ExpandedRow>(), 0);

            if (reverse)
            {
                _Rows.Reverse();
            }

            return ps;
        }

        // Try to construct a valid rows sequence
        // Recursion is used to implement backtracking
        List<ExpandedPair> CheckRows(IReadOnlyList<ExpandedRow> collectedRows, int currentRow)
        {
            for (int i = currentRow; i < _Rows.Count; i++)
            {
                ExpandedRow row = _Rows[i];
                _Pairs.Clear();
                int size = collectedRows.Count;
                for (int j = 0; j < size; j++)
                {
                    _Pairs.AddRange(collectedRows[j].Pairs);
                }
                _Pairs.AddRange(row.Pairs);

                if (!IsValidSequence(_Pairs))
                {
                    continue;
                }

                if (CheckChecksum())
                {
                    return _Pairs;
                }

                var rs = new List<ExpandedRow>(collectedRows)
                {
                    row
                };
                // Recursion: try to add more rows
                var result = CheckRows(rs, i + 1);
                if (result == null)
                    // We failed, try the next candidate
                {
                    continue;
                }
                return result;
            }

            return null;
        }

        // Whether the pairs form a valid find pattern sequence,
        // either complete or a prefix
        static bool IsValidSequence(IReadOnlyList<ExpandedPair> pairs)
        {
            foreach (int[] sequence in FINDER_PATTERN_SEQUENCES)
            {
                if (pairs.Count > sequence.Length)
                {
                    continue;
                }

                bool stop = true;
                for (int j = 0; j < pairs.Count; j++)
                {
                    if (pairs[j].FinderPattern.Value != sequence[j])
                    {
                        stop = false;
                        break;
                    }
                }

                if (stop)
                {
                    return true;
                }
            }

            return false;
        }

        void StoreRow(int rowNumber, bool wasReversed)
        {
            // Discard if duplicate above or below; otherwise insert in order by row number.
            int insertPos = 0;
            bool prevIsSame = false;
            bool nextIsSame = false;
            while (insertPos < _Rows.Count)
            {
                ExpandedRow erow = _Rows[insertPos];
                if (erow.RowNumber > rowNumber)
                {
                    nextIsSame = erow.IsEquivalent(_Pairs);
                    break;
                }
                prevIsSame = erow.IsEquivalent(_Pairs);
                insertPos++;
            }
            if (nextIsSame || prevIsSame)
            {
                return;
            }

            // When the row was partially decoded (e.g. 2 pairs found instead of 3),
            // it will prevent us from detecting the barcode.
            // Try to merge partial rows

            // Check whether the row is part of an already detected row
            if (IsPartialRow(_Pairs, _Rows))
            {
                return;
            }

            _Rows.Insert(insertPos, new ExpandedRow(_Pairs, rowNumber, wasReversed));

            RemovePartialRows(_Pairs, _Rows);
        }

        // Remove all the rows that contains only specified pairs 
        static void RemovePartialRows(ICollection<ExpandedPair> pairs, IList<ExpandedRow> rows)
        {
            for (var index = 0; index < rows.Count; index++)
            {
                var r = rows[index];
                if (r.Pairs.Count != pairs.Count)
                {
                    bool allFound = true;
                    foreach (ExpandedPair p in r.Pairs)
                    {
                        if (!pairs.Contains(p))
                        {
                            allFound = false;
                            break;
                        }
                    }
                    if (allFound)
                    {
                        // 'pairs' contains all the pairs from the row 'r'
                        rows.RemoveAt(index);
                    }
                }
            }
        }

        // Returns true when one of the rows already contains all the pairs
        static bool IsPartialRow(IEnumerable<ExpandedPair> pairs, IEnumerable<ExpandedRow> rows)
        {
            foreach (ExpandedRow r in rows)
            {
                var allFound = true;
                foreach (ExpandedPair p in pairs)
                {
                    bool found = false;
                    foreach (ExpandedPair pp in r.Pairs)
                    {
                        if (p.Equals(pp))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        allFound = false;
                        break;
                    }
                }
                if (allFound)
                {
                    // the row 'r' contain all the pairs from 'pairs'
                    return true;
                }
            }
            return false;
        }

        // Only used for unit testing
        public IReadOnlyList<ExpandedRow> Rows => _Rows;

        public static BarCodeText ConstructResult(IReadOnlyList<ExpandedPair> pairs)
        {
            BitArray binary = BitArrayBuilder.buildBitArray(pairs);

            AbstractExpandedDecoder decoder = AbstractExpandedDecoder.CreateDecoder(binary);
            string resultingString = decoder.ParseInformation();
            if (resultingString == null) {
                return null;
            }

            ResultPoint[] firstPoints = pairs[0].FinderPattern.ResultPoints;
            ResultPoint[] lastPoints = pairs[pairs.Count - 1].FinderPattern.ResultPoints;

            return new BarCodeText(resultingString, null, binary,
                new[] {firstPoints[0], firstPoints[1], lastPoints[0], lastPoints[1]},
                BarcodeFormat.RSS_EXPANDED
            );
        }

        bool CheckChecksum()
        {
            ExpandedPair firstPair = _Pairs[0];
            DataCharacter checkCharacter = firstPair.LeftChar;
            DataCharacter firstCharacter = firstPair.RightChar;

            if (firstCharacter == null)
            {
                return false;
            }

            int checksum = firstCharacter.ChecksumPortion;
            int s = 2;

            for (int i = 1; i < _Pairs.Count; ++i)
            {
                ExpandedPair currentPair = _Pairs[i];
                checksum += currentPair.LeftChar.ChecksumPortion;
                s++;
                DataCharacter currentRightChar = currentPair.RightChar;
                if (currentRightChar != null)
                {
                    checksum += currentRightChar.ChecksumPortion;
                    s++;
                }
            }

            checksum %= 211;

            int checkCharacterValue = 211 * (s - 4) + checksum;

            return checkCharacterValue == checkCharacter.Value;
        }

        static int GetNextSecondBar(BitArray row, int initialPos)
        {
            int currentPos;
            if (row[initialPos])
            {
                currentPos = row.GetNextUnset(initialPos);
                currentPos = row.GetNextSet(currentPos);
            }
            else
            {
                currentPos = row.GetNextSet(initialPos);
                currentPos = row.GetNextUnset(currentPos);
            }
            return currentPos;
        }

        // not private for testing
        public ExpandedPair RetrieveNextPair(BitArray row, List<ExpandedPair> previousPairs, int rowNumber)
        {
            bool isOddPattern = previousPairs.Count % 2 == 0;
            if (_StartFromEven)
            {
                isOddPattern = !isOddPattern;
            }

            FinderPattern pattern;

            bool keepFinding = true;
            int forcedOffset = -1;
            do
            {
                if (!FindNextPair(row, previousPairs, forcedOffset)) {
                    return null;
                }
                pattern = ParseFoundFinderPattern(row, rowNumber, isOddPattern);
                if (pattern == null)
                {
                    forcedOffset = GetNextSecondBar(row, _StartEnd[0]);
                }
                else
                {
                    keepFinding = false;
                }
            } while (keepFinding);

            // When stacked symbol is split over multiple rows, there's no way to guess if this pair can be last or not.
            // bool mayBeLast;
            // if (!checkPairSequence(previousPairs, pattern, out mayBeLast))
            //   return null;

            DataCharacter leftChar = DecodeDataCharacter(row, pattern, isOddPattern, true);
            if (leftChar == null) {
                return null;
            }

            if (previousPairs.Count != 0 &&
                previousPairs[previousPairs.Count - 1].MustBeLast)
            {
                return null;
            }

            DataCharacter rightChar = DecodeDataCharacter(row, pattern, isOddPattern, false);

            return new ExpandedPair(leftChar, rightChar, pattern);
        }

        bool FindNextPair(BitArray row, IReadOnlyList<ExpandedPair> previousPairs, int forcedOffset)
        {
            int[] counters = getDecodeFinderCounters();
            counters[0] = 0;
            counters[1] = 0;
            counters[2] = 0;
            counters[3] = 0;

            int width = row.Size;

            int rowOffset;
            if (forcedOffset >= 0)
            {
                rowOffset = forcedOffset;
            }
            else if (previousPairs.Count == 0)
            {
                rowOffset = 0;
            }
            else
            {
                ExpandedPair lastPair = previousPairs[previousPairs.Count - 1];
                rowOffset = lastPair.FinderPattern.StartEnd[1];
            }
            bool searchingEvenPair = previousPairs.Count % 2 != 0;
            if (_StartFromEven)
            {
                searchingEvenPair = !searchingEvenPair;
            }

            bool isWhite = false;
            while (rowOffset < width)
            {
                isWhite = !row[rowOffset];
                if (!isWhite)
                {
                    break;
                }
                rowOffset++;
            }

            int counterPosition = 0;
            int patternStart = rowOffset;
            for (int x = rowOffset; x < width; x++)
            {
                if (row[x] != isWhite)
                {
                    counters[counterPosition]++;
                }
                else
                {
                    if (counterPosition == 3)
                    {
                        if (searchingEvenPair)
                        {
                            ReverseCounters(counters);
                        }

                        if (isFinderPattern(counters))
                        {
                            _StartEnd[0] = patternStart;
                            _StartEnd[1] = x;
                            return true;
                        }

                        if (searchingEvenPair)
                        {
                            ReverseCounters(counters);
                        }

                        patternStart += counters[0] + counters[1];
                        counters[0] = counters[2];
                        counters[1] = counters[3];
                        counters[2] = 0;
                        counters[3] = 0;
                        counterPosition--;
                    }
                    else
                    {
                        counterPosition++;
                    }
                    counters[counterPosition] = 1;
                    isWhite = !isWhite;
                }
            }
            return false;
        }

        static void ReverseCounters(IList<int> counters)
        {
            int length = counters.Count;
            for (int i = 0; i < length / 2; ++i)
            {
                int tmp = counters[i];
                counters[i] = counters[length - i - 1];
                counters[length - i - 1] = tmp;
            }
        }

        FinderPattern ParseFoundFinderPattern(BitArray row, int rowNumber, bool oddPattern)
        {
            // Actually we found elements 2-5.
            int firstCounter;
            int start;
            int end;

            if (oddPattern)
            {
                // If pattern number is odd, we need to locate element 1 *before* the current block.

                int firstElementStart = _StartEnd[0] - 1;
                // Locate element 1
                while (firstElementStart >= 0 && !row[firstElementStart])
                {
                    firstElementStart--;
                }

                firstElementStart++;
                firstCounter = _StartEnd[0] - firstElementStart;
                start = firstElementStart;
                end = _StartEnd[1];

            }
            else
            {
                // If pattern number is even, the pattern is reversed, so we need to locate element 1 *after* the current block.
                start = _StartEnd[0];
                end = row.GetNextUnset(_StartEnd[1] + 1);
                firstCounter = end - _StartEnd[1];
            }

            // Make 'counters' hold 1-4
            int[] counters = getDecodeFinderCounters();
            Array.Copy(counters, 0, counters, 1, counters.Length - 1);

            counters[0] = firstCounter;
            if (!parseFinderValue(counters, FINDER_PATTERNS, out var value)) {
                return null;
            }

            return new FinderPattern(value, new[] {start, end}, start, end, rowNumber);
        }

        public DataCharacter DecodeDataCharacter(BitArray row,
            FinderPattern pattern,
            bool isOddPattern,
            bool leftChar)
        {
            int[] counters = getDataCharacterCounters();
            SupportClass.Fill(counters, 0);

            if (leftChar)
            {
                if (!RecordPatternInReverse(row, pattern.StartEnd[0], counters)) {
                    return null;
                }
            }
            else
            {
                if (!RecordPattern(row, pattern.StartEnd[1], counters)) {
                    return null;
                }
                // reverse it
                for (int i = 0, j = counters.Length - 1; i < j; i++, j--)
                {
                    int temp = counters[i];
                    counters[i] = counters[j];
                    counters[j] = temp;
                }
            } //counters[] has the pixels of the module

            const int numModules = 17; //left and right data characters have all the same length
            float elementWidth = MathUtils.Sum(counters) / (float) numModules;

            // Sanity check: element width for pattern and the character should match
            float expectedElementWidth = (pattern.StartEnd[1] - pattern.StartEnd[0]) / 15.0f;
            if (Math.Abs(elementWidth - expectedElementWidth) / expectedElementWidth > 0.3f)
            {
                return null;
            }

            int[] oddCounts = getOddCounts();
            int[] evenCounts = getEvenCounts();
            float[] oddRoundingErrors = getOddRoundingErrors();
            float[] evenRoundingErrors = getEvenRoundingErrors();

            for (int i = 0; i < counters.Length; i++)
            {
                float divided = 1.0f * counters[i] / elementWidth;
                int rounded = (int) (divided + 0.5f); // Round
                if (rounded < 1)
                {
                    if (divided < 0.3f)
                    {
                        return null;
                    }
                    rounded = 1;
                }
                else if (rounded > 8)
                {
                    if (divided > 8.7f)
                    {
                        return null;
                    }
                    rounded = 8;
                }
                int offset = i >> 1;
                if ((i & 0x01) == 0)
                {
                    oddCounts[offset] = rounded;
                    oddRoundingErrors[offset] = divided - rounded;
                }
                else
                {
                    evenCounts[offset] = rounded;
                    evenRoundingErrors[offset] = divided - rounded;
                }
            }

            if (!AdjustOddEvenCounts(numModules)) {
                return null;
            }

            int weightRowNumber = 4 * pattern.Value + (isOddPattern ? 0 : 2) + (leftChar ? 0 : 1) - 1;

            int oddSum = 0;
            int oddChecksumPortion = 0;
            for (int i = oddCounts.Length - 1; i >= 0; i--)
            {
                if (IsNotA1Left(pattern, isOddPattern, leftChar))
                {
                    int weight = WEIGHTS[weightRowNumber][2 * i];
                    oddChecksumPortion += oddCounts[i] * weight;
                }
                oddSum += oddCounts[i];
            }
            int evenChecksumPortion = 0;
            for (int i = evenCounts.Length - 1; i >= 0; i--)
            {
                if (IsNotA1Left(pattern, isOddPattern, leftChar))
                {
                    int weight = WEIGHTS[weightRowNumber][2 * i + 1];
                    evenChecksumPortion += evenCounts[i] * weight;
                }
            }
            int checksumPortion = oddChecksumPortion + evenChecksumPortion;

            if ((oddSum & 0x01) != 0 || oddSum > 13 || oddSum < 4)
            {
                return null;
            }

            int group = (13 - oddSum) / 2;
            int oddWidest = SYMBOL_WIDEST[group];
            int evenWidest = 9 - oddWidest;
            int vOdd = RSSUtils.getRSSvalue(oddCounts, oddWidest, true);
            int vEven = RSSUtils.getRSSvalue(evenCounts, evenWidest, false);
            int tEven = EVEN_TOTAL_SUBSET[group];
            int gSum = GSUM[group];
            int value = vOdd * tEven + vEven + gSum;

            return new DataCharacter(value, checksumPortion);
        }

        static bool IsNotA1Left(FinderPattern pattern, bool isOddPattern, bool leftChar)
        {
            // A1: pattern.getValue is 0 (A), and it's an oddPattern, and it is a left char
            return !(pattern.Value == 0 && isOddPattern && leftChar);
        }

        bool AdjustOddEvenCounts(int numModules)
        {
            int oddSum = MathUtils.Sum(getOddCounts());
            int evenSum = MathUtils.Sum(getEvenCounts());
            bool incrementOdd = false;
            bool decrementOdd = false;

            if (oddSum > 13)
            {
                decrementOdd = true;
            }
            else if (oddSum < 4)
            {
                incrementOdd = true;
            }
            bool incrementEven = false;
            bool decrementEven = false;
            if (evenSum > 13)
            {
                decrementEven = true;
            }
            else if (evenSum < 4)
            {
                incrementEven = true;
            }

            int mismatch = oddSum + evenSum - numModules;
            bool oddParityBad = (oddSum & 0x01) == 1;
            bool evenParityBad = (evenSum & 0x01) == 0;
            switch (mismatch)
            {
                case 1:
                    if (oddParityBad)
                    {
                        if (evenParityBad)
                        {
                            return false;
                        }
                        decrementOdd = true;
                    }
                    else
                    {
                        if (!evenParityBad)
                        {
                            return false;
                        }
                        decrementEven = true;
                    }
                    break;
                case -1:
                    if (oddParityBad)
                    {
                        if (evenParityBad)
                        {
                            return false;
                        }
                        incrementOdd = true;
                    }
                    else
                    {
                        if (!evenParityBad)
                        {
                            return false;
                        }
                        incrementEven = true;
                    }
                    break;
                case 0:
                    if (oddParityBad)
                    {
                        if (!evenParityBad)
                        {
                            return false;
                        }
                        // Both bad
                        if (oddSum < evenSum)
                        {
                            incrementOdd = true;
                            decrementEven = true;
                        }
                        else
                        {
                            decrementOdd = true;
                            incrementEven = true;
                        }
                    }
                    else
                    {
                        if (evenParityBad)
                        {
                            return false;
                        }
                        // Nothing to do!
                    }
                    break;
                default:
                    return false;
            }

            if (incrementOdd)
            {
                if (decrementOdd)
                {
                    return false;
                }
                increment(getOddCounts(), getOddRoundingErrors());
            }
            if (decrementOdd)
            {
                decrement(getOddCounts(), getOddRoundingErrors());
            }
            if (incrementEven)
            {
                if (decrementEven)
                {
                    return false;
                }
                increment(getEvenCounts(), getOddRoundingErrors());
            }
            if (decrementEven)
            {
                decrement(getEvenCounts(), getEvenRoundingErrors());
            }

            return true;
        }
    }
}