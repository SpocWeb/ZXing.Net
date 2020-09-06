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

using NUnit.Framework;

namespace ZXing.OneD.RSS.Expanded.Decoders.Test
{
   /// <summary>
   /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
   /// </summary>
   public class Ai013X0X1XDecoderTest : AbstractDecoderTest
   {
      private static string _HEADER_310_X_11 = "..XXX...";
      private static string _HEADER_320_X_11 = "..XXX..X";
      private static string _HEADER_310_X_13 = "..XXX.X.";
      private static string _HEADER_320_X_13 = "..XXX.XX";
      private static string _HEADER_310_X_15 = "..XXXX..";
      private static string _HEADER_320_X_15 = "..XXXX.X";
      private static string _HEADER_310_X_17 = "..XXXXX.";
      private static string _HEADER_320_X_17 = "..XXXXXX";

      [Test]
      public void test01_310X_1X_endDate()
      {
            string data = _HEADER_310_X_11 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateEnd;
            string expected = "(01)90012345678908(3100)001750";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_310X_11_1()
      {
            string data = _HEADER_310_X_11 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3100)001750(11)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_320X_11_1()
      {
            string data = _HEADER_320_X_11 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3200)001750(11)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_310X_13_1()
      {
            string data = _HEADER_310_X_13 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3100)001750(13)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_320X_13_1()
      {
            string data = _HEADER_320_X_13 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3200)001750(13)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_310X_15_1()
      {
            string data = _HEADER_310_X_15 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3100)001750(15)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_320X_15_1()
      {
            string data = _HEADER_320_X_15 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3200)001750(15)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_310X_17_1()
      {
            string data = _HEADER_310_X_17 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3100)001750(17)100312";

         AssertCorrectBinaryString(data, expected);
      }

      [Test]
      public void test01_320X_17_1()
      {
            string data = _HEADER_320_X_17 + CompressedGtin900123456798908 + Compressed20BitWeight1750 + CompressedDateMarch12Th2010;
            string expected = "(01)90012345678908(3200)001750(17)100312";

         AssertCorrectBinaryString(data, expected);
      }
   }
}