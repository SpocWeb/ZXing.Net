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

using NUnit.Framework;

namespace ZXing.OneD.RSS.Expanded.Decoders.Test
{
    /// <summary>
    /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
    /// </summary>
    public class AnyAiDecoderTest : AbstractDecoderTest
   {

       static string _HEADER = ".....";

      [Test]
      public void testAnyAIDecoder_1()
      {
            string data = _HEADER + Numeric10 + Numeric12 + Numeric2Alpha + AlphaA + Alpha2Numeric + Numeric12;
            string expected = "(10)12A12";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void testAnyAIDecoder_2()
      {
            string data = _HEADER + Numeric10 + Numeric12 + Numeric2Alpha + AlphaA + Alpha2Isoiec646 + I646B;
            string expected = "(10)12AB";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void testAnyAIDecoder_3()
      {
            string data = _HEADER + Numeric10 + Numeric2Alpha + Alpha2Isoiec646 + I646B + I646C + Isoiec6462Alpha + AlphaA + Alpha2Numeric + Numeric10;
            string expected = "(10)BCA10";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void testAnyAIDecoder_numericFNC1_secondDigit()
      {
            string data = _HEADER + Numeric10 + Numeric1Fnc1;
            string expected = "(10)1";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void testAnyAIDecoder_alphaFNC1()
      {
            string data = _HEADER + Numeric10 + Numeric2Alpha + AlphaA + AlphaFnc1;
            string expected = "(10)A";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void testAnyAIDecoder_646FNC1()
      {
            string data = _HEADER + Numeric10 + Numeric2Alpha + AlphaA + Isoiec6462Alpha + I646B + I646Fnc1;
            string expected = "(10)AB";

         AssertCorrectBinaryString(data, expected);
      }
   }
}