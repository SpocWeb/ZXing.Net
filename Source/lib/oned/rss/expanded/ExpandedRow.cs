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

using System.Collections.Generic;

namespace ZXing.OneD.RSS.Expanded
{
    /// <summary>
    /// One row of an RSS Expanded Stacked symbol, consisting of 1+ expanded pairs.
    /// </summary>
    public sealed class ExpandedRow
    {

        readonly bool wasReversed;

        internal ExpandedRow(List<ExpandedPair> pairs, int rowNumber, bool wasReversed)
        {
            Pairs = new List<ExpandedPair>(pairs);
            RowNumber = rowNumber;
            this.wasReversed = wasReversed;
        }

        public IReadOnlyList<ExpandedPair> Pairs { get; }

        public int RowNumber { get; }

        internal bool IsEquivalent(List<ExpandedPair> otherPairs)
        {
            return Pairs.Equals(otherPairs);
        }

        public override string ToString()
        {
            return "{ " + Pairs + " }";
        }

        /// <summary>
        /// Two rows are equal if they contain the same pairs in the same order.
        /// </summary>
        public override bool Equals(object o)
        {
            if (!(o is ExpandedRow))
            {
                return false;
            }
            ExpandedRow that = (ExpandedRow) o;
            return Pairs.Equals(that.Pairs) && wasReversed == that.wasReversed;
        }

        public override int GetHashCode()
        {
            return Pairs.GetHashCode() ^ wasReversed.GetHashCode();
        }
    }
}