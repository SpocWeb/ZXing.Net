/*
 * Copyright 2007 ZXing authors
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

using NUnit.Framework;

namespace ZXing.QrCode.Internal.Test
{
   /// <summary>
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class FormatInformationTestCase
   {
      private static int _MASKED_TEST_FORMAT_INFO = 0x2BED;
      private static int _UNMASKED_TEST_FORMAT_INFO = _MASKED_TEST_FORMAT_INFO ^ 0x5412;

      [Test]
      public void TestBitsDiffering()
      {
         Assert.AreEqual(0, FormatInformation.numBitsDiffering(1, 1));
         Assert.AreEqual(1, FormatInformation.numBitsDiffering(0, 2));
         Assert.AreEqual(2, FormatInformation.numBitsDiffering(1, 2));
         Assert.AreEqual(32, FormatInformation.numBitsDiffering(-1, 0));
      }

      [Test]
      public void TestDecode()
      {
         // Normal case
         FormatInformation expected =
             FormatInformation.decodeFormatInformation(_MASKED_TEST_FORMAT_INFO, _MASKED_TEST_FORMAT_INFO);
         Assert.IsNotNull(expected);
         Assert.AreEqual((byte)0x07, expected.DataMask);
         Assert.AreEqual(ErrorCorrectionLevel.Q, expected.ErrorCorrectionLevel);
         // where the code forgot the mask!
         Assert.AreEqual(expected,
                      FormatInformation.decodeFormatInformation(_UNMASKED_TEST_FORMAT_INFO, _MASKED_TEST_FORMAT_INFO));
      }

      [Test]
      public void TestDecodeWithBitDifference()
      {
         FormatInformation expected =
             FormatInformation.decodeFormatInformation(_MASKED_TEST_FORMAT_INFO, _MASKED_TEST_FORMAT_INFO);
         // 1,2,3,4 bits difference
         Assert.AreEqual(expected, FormatInformation.decodeFormatInformation(
             _MASKED_TEST_FORMAT_INFO ^ 0x01, _MASKED_TEST_FORMAT_INFO ^ 0x01));
         Assert.AreEqual(expected, FormatInformation.decodeFormatInformation(
             _MASKED_TEST_FORMAT_INFO ^ 0x03, _MASKED_TEST_FORMAT_INFO ^ 0x03));
         Assert.AreEqual(expected, FormatInformation.decodeFormatInformation(
             _MASKED_TEST_FORMAT_INFO ^ 0x07, _MASKED_TEST_FORMAT_INFO ^ 0x07));
         Assert.IsNull(FormatInformation.decodeFormatInformation(
             _MASKED_TEST_FORMAT_INFO ^ 0x0F, _MASKED_TEST_FORMAT_INFO ^ 0x0F));
      }

      [Test]
      public void TestDecodeWithMisread()
      {
         FormatInformation expected =
             FormatInformation.decodeFormatInformation(_MASKED_TEST_FORMAT_INFO, _MASKED_TEST_FORMAT_INFO);
         Assert.AreEqual(expected, FormatInformation.decodeFormatInformation(
             _MASKED_TEST_FORMAT_INFO ^ 0x03, _MASKED_TEST_FORMAT_INFO ^ 0x0F));
      }
   }
}