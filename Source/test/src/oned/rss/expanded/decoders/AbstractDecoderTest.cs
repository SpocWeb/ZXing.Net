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
using ZXing.Common;
using ZXing.OneD.RSS.Expanded.Test;

namespace ZXing.OneD.RSS.Expanded.Decoders.Test
{
    /// <summary>
    /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
    /// </summary>
    [TestFixture]
   public abstract class AbstractDecoderTest
   {
      protected static string Numeric10 = "..X..XX";
      protected static string Numeric12 = "..X.X.X";
      protected static string Numeric1Fnc1 = "..XXX.X";
      //protected static String numeric_FNC11 = "XXX.XXX";

      protected static string Numeric2Alpha = "....";

      protected static string AlphaA = "X.....";
      protected static string AlphaFnc1 = ".XXXX";
      protected static string Alpha2Numeric = "...";
      protected static string Alpha2Isoiec646 = "..X..";

      protected static string I646B = "X.....X";
      protected static string I646C = "X....X.";
      protected static string I646Fnc1 = ".XXXX";
      protected static string Isoiec6462Alpha = "..X..";

      protected static string CompressedGtin900123456798908 = ".........X..XXX.X.X.X...XX.XXXXX.XXXX.X.";
      protected static string CompressedGtin900000000000008 = "........................................";

      protected static string Compressed15BitWeight1750 = "....XX.XX.X.XX.";
      protected static string Compressed15BitWeight11750 = ".X.XX.XXXX..XX.";
      protected static string Compressed15BitWeight0 = "...............";

      protected static string Compressed20BitWeight1750 = ".........XX.XX.X.XX.";

      protected static string CompressedDateMarch12Th2010 = "....XXXX.X..XX..";
      protected static string CompressedDateEnd = "X..X.XX.........";

      protected static void AssertCorrectBinaryString(string binaryString, string expectedNumber)
      {
         BitArray binary = BinaryUtil.BuildBitArrayFromStringWithoutSpaces(binaryString);
         AbstractExpandedDecoder decoder = AbstractExpandedDecoder.createDecoder(binary);
            string result = decoder.parseInformation();
         Assert.AreEqual(expectedNumber, result);
      }
   }
}