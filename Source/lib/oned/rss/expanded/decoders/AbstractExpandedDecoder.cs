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
using ZXing.Common;

namespace ZXing.OneD.RSS.Expanded.Decoders
{
    /// <summary>
    /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
    /// <author>Eduardo Castillejo, University of Deusto (eduardo.castillejo@deusto.es)</author>
    /// </summary>
    public abstract class AbstractExpandedDecoder
    {
        private readonly BitArray _Information;
        private readonly GeneralAppIdDecoder _GeneralDecoder;

        internal AbstractExpandedDecoder(BitArray information)
        {
            _Information = information;
            _GeneralDecoder = new GeneralAppIdDecoder(information);
        }

        protected BitArray GetInformation() => _Information;

        internal GeneralAppIdDecoder GetGeneralDecoder() => _GeneralDecoder;

        public abstract string ParseInformation();

        public static AbstractExpandedDecoder CreateDecoder(BitArray information)
        {
            if (information[1])
            {
                return new AI01AndOtherAIs(information);
            }
            if (!information[2])
            {
                return new AnyAIDecoder(information);
            }

            int fourBitEncodingMethod = GeneralAppIdDecoder.extractNumericValueFromBitArray(information, 1, 4);

            switch (fourBitEncodingMethod)
            {
                case 4: return new AI013103decoder(information);
                case 5: return new AI01320xDecoder(information);
            }

            int fiveBitEncodingMethod = GeneralAppIdDecoder.extractNumericValueFromBitArray(information, 1, 5);
            switch (fiveBitEncodingMethod)
            {
                case 12: return new AI01392xDecoder(information);
                case 13: return new AI01393xDecoder(information);
            }

            int sevenBitEncodingMethod = GeneralAppIdDecoder.extractNumericValueFromBitArray(information, 1, 7);
            switch (sevenBitEncodingMethod)
            {
                case 56: return new AI013x0x1xDecoder(information, "310", "11");
                case 57: return new AI013x0x1xDecoder(information, "320", "11");
                case 58: return new AI013x0x1xDecoder(information, "310", "13");
                case 59: return new AI013x0x1xDecoder(information, "320", "13");
                case 60: return new AI013x0x1xDecoder(information, "310", "15");
                case 61: return new AI013x0x1xDecoder(information, "320", "15");
                case 62: return new AI013x0x1xDecoder(information, "310", "17");
                case 63: return new AI013x0x1xDecoder(information, "320", "17");
            }

            throw new InvalidOperationException("unknown decoder: " + information);
        }
    }
}
