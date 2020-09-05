﻿/*
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

namespace ZXing.OneD.RSS.Expanded
{
    /// <summary>
    /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
    /// </summary>
    public sealed class ExpandedPair
    {
        internal bool MayBeLast { get; private set; }
        internal DataCharacter LeftChar { get; private set; }
        internal DataCharacter RightChar { get; private set; }
        public FinderPattern FinderPattern { get; private set; }

        public ExpandedPair(DataCharacter leftChar,
            DataCharacter rightChar,
            FinderPattern finderPattern)
        {
            LeftChar = leftChar;
            RightChar = rightChar;
            FinderPattern = finderPattern;
        }

        public bool MustBeLast => RightChar == null;

        public override String ToString()
        {
            return
                "[ " + LeftChar + " , " + RightChar + " : " +
                (FinderPattern == null ? "null" : FinderPattern.Value.ToString()) + " ]";
        }

        public override bool Equals(Object o)
        {
            if (!(o is ExpandedPair))
            {
                return false;
            }
            ExpandedPair that = (ExpandedPair) o;
            return
                EqualsOrNull(LeftChar, that.LeftChar) &&
                EqualsOrNull(RightChar, that.RightChar) &&
                EqualsOrNull(FinderPattern, that.FinderPattern);
        }

        private static bool EqualsOrNull(Object o1, Object o2)
        {
            return o1 == null ? o2 == null : o1.Equals(o2);
        }

        public override int GetHashCode()
        {
            return hashNotNull(LeftChar) ^ hashNotNull(RightChar) ^ hashNotNull(FinderPattern);
        }

        private static int hashNotNull(Object o)
        {
            return o == null ? 0 : o.GetHashCode();
        }
    }
}