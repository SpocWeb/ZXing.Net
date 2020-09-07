/*
 * Copyright 2008 ZXing authors
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
using System.Text;

using NUnit.Framework;

using ZXing.Common;

namespace ZXing.QrCode.Internal.Test
{
   /// <summary>
   /// <author>satorux@google.com (Satoru Takabayashi) - creator</author>
   /// <author>mysen@google.com (Chris Mysen) - ported from C++</author>
   /// </summary>
   [TestFixture]
   public sealed class EncoderTestCase
   {
      [Test]
      public void TestGetAlphanumericCode()
      {
         // The first ten code points are numbers.
         for (int i = 0; i < 10; ++i)
         {
            Assert.AreEqual(i, Encoder.GetAlphanumericCode('0' + i));
         }

         // The next 26 code points are capital alphabet letters.
         for (int i = 10; i < 36; ++i)
         {
            Assert.AreEqual(i, Encoder.GetAlphanumericCode('A' + i - 10));
         }

         // Others are symbol letters
         Assert.AreEqual(36, Encoder.GetAlphanumericCode(' '));
         Assert.AreEqual(37, Encoder.GetAlphanumericCode('$'));
         Assert.AreEqual(38, Encoder.GetAlphanumericCode('%'));
         Assert.AreEqual(39, Encoder.GetAlphanumericCode('*'));
         Assert.AreEqual(40, Encoder.GetAlphanumericCode('+'));
         Assert.AreEqual(41, Encoder.GetAlphanumericCode('-'));
         Assert.AreEqual(42, Encoder.GetAlphanumericCode('.'));
         Assert.AreEqual(43, Encoder.GetAlphanumericCode('/'));
         Assert.AreEqual(44, Encoder.GetAlphanumericCode(':'));

         // Should return -1 for other letters;
         Assert.AreEqual(-1, Encoder.GetAlphanumericCode('a'));
         Assert.AreEqual(-1, Encoder.GetAlphanumericCode('#'));
         Assert.AreEqual(-1, Encoder.GetAlphanumericCode('\0'));
      }

      [Test]
      public void TestChooseMode()
      {
         // Numeric mode.
         Assert.AreEqual(Mode.NUMERIC, Encoder.ChooseMode("0"));
         Assert.AreEqual(Mode.NUMERIC, Encoder.ChooseMode("0123456789"));
         // Alphanumeric mode.
         Assert.AreEqual(Mode.ALPHANUMERIC, Encoder.ChooseMode("A"));
         Assert.AreEqual(Mode.ALPHANUMERIC,
            Encoder.ChooseMode("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:"));
         // 8-bit byte mode.
         Assert.AreEqual(Mode.BYTE, Encoder.ChooseMode("a"));
         Assert.AreEqual(Mode.BYTE, Encoder.ChooseMode("#"));
         Assert.AreEqual(Mode.BYTE, Encoder.ChooseMode(""));
         // Kanji mode.  We used to use MODE_KANJI for these, but we stopped
         // doing that as we cannot distinguish Shift_JIS from other encodings
         // from data bytes alone.  See also comments in qrcode_encoder.h.

         // AIUE in Hiragana in Shift_JIS
         Assert.AreEqual(Mode.BYTE,
            Encoder.ChooseMode(ShiftJisString(new byte[] {0x8, 0xa, 0x8, 0xa, 0x8, 0xa, 0x8, 0xa6})));

         // Nihon in Kanji in Shift_JIS.
         Assert.AreEqual(Mode.BYTE, Encoder.ChooseMode(ShiftJisString(new byte[] {0x9, 0xf, 0x9, 0x7b})));

         // Sou-Utsu-Byou in Kanji in Shift_JIS.
         Assert.AreEqual(Mode.BYTE, Encoder.ChooseMode(ShiftJisString(new byte[] {0xe, 0x4, 0x9, 0x5, 0x9, 0x61})));
      }

      [Test]
      public void TestEncode()
      {
         var qrCode = Encoder.Encode("ABCDEF", ErrorCorrectionLevel.H);
         const string expected = "<<\n" +
                                 " mode: ALPHANUMERIC\n" +
                                 " ecLevel: H\n" +
                                 " version: 1\n" +
                                 " maskPattern: 4\n" +
                                 " matrix:\n" +
                                 " 1 1 1 1 1 1 1 0 0 1 0 1 0 0 1 1 1 1 1 1 1\n" +
                                 " 1 0 0 0 0 0 1 0 1 0 1 0 1 0 1 0 0 0 0 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 1 0 0 1 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 0 0 0 0 1 0 1 0 0 1 1 0 1 0 0 0 0 0 1\n" +
                                 " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
                                 " 0 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 0 0 0\n" +
                                 " 0 0 0 0 1 1 1 1 0 1 1 0 1 0 1 1 0 0 0 1 0\n" +
                                 " 0 0 0 0 1 1 0 1 1 1 0 0 1 1 1 1 0 1 1 0 1\n" +
                                 " 1 0 0 0 0 1 1 0 0 1 0 1 0 0 0 1 1 1 0 1 1\n" +
                                 " 1 0 0 1 1 1 0 0 1 1 1 1 0 0 0 0 1 0 0 0 0\n" +
                                 " 0 1 1 1 1 1 1 0 1 0 1 0 1 1 1 0 0 1 1 0 0\n" +
                                 " 0 0 0 0 0 0 0 0 1 1 0 0 0 1 1 0 0 0 1 0 1\n" +
                                 " 1 1 1 1 1 1 1 0 1 1 1 1 0 0 0 0 0 1 1 0 0\n" +
                                 " 1 0 0 0 0 0 1 0 1 1 0 1 0 0 0 1 0 1 1 1 1\n" +
                                 " 1 0 1 1 1 0 1 0 1 0 0 1 0 0 0 1 1 0 0 1 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 0 1 1 0 1 0 0 0 0 1 1 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 0 1 1 0 0 0 0\n" +
                                 " 1 0 0 0 0 0 1 0 0 1 0 0 1 0 0 1 1 0 0 0 1\n" +
                                 " 1 1 1 1 1 1 1 0 0 0 1 0 0 1 0 0 0 0 1 1 1\n" +
                                 ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      [Test]
      public void TestEncodeWithVersion()
      {
         var hints = new QrCodeEncodingOptions {QrVersion = 7};
         QrCode qrCode = Encoder.Encode("ABCDEF", ErrorCorrectionLevel.H, hints.Hints);
         Assert.IsTrue(qrCode.ToString().Contains(" version: 7\n"));
      }

      [Test]
      public void TestEncodeWithVersionString()
      {
         var hints = new QrCodeEncodingOptions();
         hints.Hints[EncodeHintType.QR_VERSION] = "7";
         QrCode qrCode = Encoder.Encode("ABCDEF", ErrorCorrectionLevel.H, hints.Hints);
         Assert.IsTrue(qrCode.ToString().Contains(" version: 7\n"));
      }

      [Test]
      [ExpectedException(typeof(WriterException))]
      public void TestEncodeWithVersionTooSmall()
      {
         var hints = new QrCodeEncodingOptions {QrVersion = 3};
         Encoder.Encode("THISMESSAGEISTOOLONGFORAQRCODEVERSION3", ErrorCorrectionLevel.H, hints.Hints);
      }

      [Test]
      public void TestSimpleUtf8Eci()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "UTF-8"
            };
            var qrCode = Encoder.Encode("hello", ErrorCorrectionLevel.H, hints);
         const string expected = "<<\n" +
                                 " mode: BYTE\n" +
                                 " ecLevel: H\n" +
                                 " version: 1\n" +
                                 " maskPattern: 6\n" +
                                 " matrix:\n" +
                                 " 1 1 1 1 1 1 1 0 0 0 1 1 0 0 1 1 1 1 1 1 1\n" +
                                 " 1 0 0 0 0 0 1 0 0 0 1 1 0 0 1 0 0 0 0 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 1 0 0 1 1 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 1 0 0 0 1 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 1 1 1 0 1 0 0 1 1 0 0 0 1 0 1 1 1 0 1\n" +
                                 " 1 0 0 0 0 0 1 0 0 0 0 1 0 0 1 0 0 0 0 0 1\n" +
                                 " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
                                 " 0 0 0 0 0 0 0 0 0 1 1 1 1 0 0 0 0 0 0 0 0\n" +
                                 " 0 0 0 1 1 0 1 1 0 0 0 0 1 0 0 0 0 1 1 0 0\n" +
                                 " 0 0 0 0 0 0 0 0 1 1 0 1 0 0 1 0 1 1 1 1 1\n" +
                                 " 1 1 0 0 0 1 1 1 0 0 0 1 1 0 0 1 0 1 0 1 1\n" +
                                 " 0 0 0 0 1 1 0 0 1 0 0 0 0 0 1 0 1 1 0 0 0\n" +
                                 " 0 1 1 0 0 1 1 0 0 1 1 1 0 1 1 1 1 1 1 1 1\n" +
                                 " 0 0 0 0 0 0 0 0 1 1 1 0 1 1 1 1 1 1 1 1 1\n" +
                                 " 1 1 1 1 1 1 1 0 1 0 1 0 0 0 1 0 0 0 0 0 0\n" +
                                 " 1 0 0 0 0 0 1 0 0 1 0 0 0 1 0 0 0 1 1 0 0\n" +
                                 " 1 0 1 1 1 0 1 0 1 0 0 0 1 0 1 0 0 0 1 0 0\n" +
                                 " 1 0 1 1 1 0 1 0 1 1 1 1 0 1 0 0 1 0 1 1 0\n" +
                                 " 1 0 1 1 1 0 1 0 0 1 1 1 0 0 1 0 0 1 0 1 1\n" +
                                 " 1 0 0 0 0 0 1 0 0 0 0 0 0 1 1 0 1 1 0 0 0\n" +
                                 " 1 1 1 1 1 1 1 0 0 0 0 1 0 1 0 0 1 0 1 0 0\n" +
                                 ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      [Test]
      public void TestEncodeKanjiMode()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "Shift_JIS"
            };
            // Nihon in Kanji
            var qrCode = Encoder.Encode("\u65e5\u672c", ErrorCorrectionLevel.M, hints);
         const string expected =
            "<<\n" +
            " mode: KANJI\n" +
            " ecLevel: M\n" +
            " version: 1\n" +
            " maskPattern: 0\n" +
            " matrix:\n" +
            " 1 1 1 1 1 1 1 0 0 1 0 1 0 0 1 1 1 1 1 1 1\n" +
            " 1 0 0 0 0 0 1 0 1 1 0 0 0 0 1 0 0 0 0 0 1\n" +
            " 1 0 1 1 1 0 1 0 0 1 1 1 1 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 0 0 0 0 1 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 1 1 1 1 0 1 0 1 1 1 0 1\n" +
            " 1 0 0 0 0 0 1 0 0 1 1 1 0 0 1 0 0 0 0 0 1\n" +
            " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
            " 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0\n" +
            " 1 0 1 0 1 0 1 0 0 0 1 0 1 0 0 0 1 0 0 1 0\n" +
            " 1 1 0 1 0 0 0 1 0 1 1 1 0 1 0 1 0 1 0 0 0\n" +
            " 0 1 0 0 0 0 1 1 1 1 1 1 0 1 1 1 0 1 0 1 0\n" +
            " 1 1 1 0 0 1 0 1 0 0 0 1 1 1 0 1 1 0 1 0 0\n" +
            " 0 1 1 0 0 1 1 0 1 1 0 1 0 1 1 1 0 1 0 0 1\n" +
            " 0 0 0 0 0 0 0 0 1 0 1 0 0 0 1 0 0 0 1 0 1\n" +
            " 1 1 1 1 1 1 1 0 0 0 0 0 1 0 0 0 1 0 0 1 1\n" +
            " 1 0 0 0 0 0 1 0 0 0 1 0 0 0 1 0 0 0 1 1 1\n" +
            " 1 0 1 1 1 0 1 0 1 0 0 0 1 0 1 0 1 0 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 0 0 0 1 0 1 0 1 0 1 0 1 0\n" +
            " 1 0 1 1 1 0 1 0 1 0 1 1 0 1 1 1 0 0 1 0 1\n" +
            " 1 0 0 0 0 0 1 0 0 0 0 1 1 1 0 1 1 1 0 1 0\n" +
            " 1 1 1 1 1 1 1 0 1 1 0 1 0 1 1 1 0 0 1 0 0\n" +
            ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      [Test]
      public void TestEncodeShiftjisNumeric()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "Shift_JIS"
            };
            var qrCode = Encoder.Encode("0123", ErrorCorrectionLevel.M, hints);
         const string expected =
            "<<\n" +
            " mode: NUMERIC\n" +
            " ecLevel: M\n" +
            " version: 1\n" +
            " maskPattern: 2\n" +
            " matrix:\n" +
            " 1 1 1 1 1 1 1 0 0 1 1 0 1 0 1 1 1 1 1 1 1\n" +
            " 1 0 0 0 0 0 1 0 0 1 0 0 1 0 1 0 0 0 0 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 0 0 0 0 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 0 1 1 1 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 1 0 1 1 0 1 0 1 1 1 0 1\n" +
            " 1 0 0 0 0 0 1 0 1 1 0 0 1 0 1 0 0 0 0 0 1\n" +
            " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
            " 0 0 0 0 0 0 0 0 1 1 1 1 1 0 0 0 0 0 0 0 0\n" +
            " 1 0 1 1 1 1 1 0 0 1 1 0 1 0 1 1 1 1 1 0 0\n" +
            " 1 1 0 0 0 1 0 0 1 0 1 0 1 0 0 1 0 0 1 0 0\n" +
            " 0 1 1 0 1 1 1 1 0 1 1 1 0 1 0 0 1 1 0 1 1\n" +
            " 1 0 1 1 0 1 0 1 0 0 1 0 0 0 0 1 1 0 1 0 0\n" +
            " 0 0 1 0 0 1 1 1 0 0 0 1 0 1 0 0 1 0 1 0 0\n" +
            " 0 0 0 0 0 0 0 0 1 1 0 1 1 1 1 0 0 1 0 0 0\n" +
            " 1 1 1 1 1 1 1 0 0 0 1 0 1 0 1 1 0 0 0 0 0\n" +
            " 1 0 0 0 0 0 1 0 1 1 0 1 1 1 1 0 0 1 0 1 0\n" +
            " 1 0 1 1 1 0 1 0 1 0 1 0 1 0 0 1 0 0 1 0 0\n" +
            " 1 0 1 1 1 0 1 0 1 1 1 0 1 0 0 1 0 0 1 0 0\n" +
            " 1 0 1 1 1 0 1 0 1 1 0 1 0 1 0 0 1 1 1 0 0\n" +
            " 1 0 0 0 0 0 1 0 0 0 1 0 0 0 0 1 1 0 1 1 0\n" +
            " 1 1 1 1 1 1 1 0 1 1 0 1 0 1 0 0 1 1 1 0 0\n" +
            ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      [Test]
      public void TestEncodeGs1WithStringTypeHint()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.GS1_FORMAT] = "true"
            };
            QrCode qrCode = Encoder.Encode("100001%11171218", ErrorCorrectionLevel.H, hints);
         VerifyGs1EncodedData(qrCode);
      }

      [Test]
      public void TestEncodeGs1WithBooleanTypeHint()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.GS1_FORMAT] = true
            };
            var qrCode = Encoder.Encode("100001%11171218", ErrorCorrectionLevel.H, hints);
         VerifyGs1EncodedData(qrCode);
      }

      [Test]
      public void TestDoesNotEncodeGs1WhenBooleanTypeHintExplicitlyFalse()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.GS1_FORMAT] = false
            };
            var qrCode = Encoder.Encode("ABCDEF", ErrorCorrectionLevel.H, hints);
         VerifyNotGs1EncodedData(qrCode);
      }

      [Test]
      public void TestDoesNotEncodeGs1WhenStringTypeHintExplicitlyFalse()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.GS1_FORMAT] = "false"
            };
            var qrCode = Encoder.Encode("ABCDEF", ErrorCorrectionLevel.H, hints);
         VerifyNotGs1EncodedData(qrCode);
      }

      [Test]
      public void TestGs1ModeHeaderWithEci()
      {
            var hints = new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.CHARACTER_SET] = "UTF-8",
                [EncodeHintType.GS1_FORMAT] = true
            };
            var qrCode = Encoder.Encode("hello", ErrorCorrectionLevel.H, hints);
         var expected =
            "<<\n" +
            " mode: BYTE\n" +
            " ecLevel: H\n" +
            " version: 1\n" +
            " maskPattern: 5\n" +
            " matrix:\n" +
            " 1 1 1 1 1 1 1 0 1 0 1 1 0 0 1 1 1 1 1 1 1\n" +
            " 1 0 0 0 0 0 1 0 0 1 1 0 0 0 1 0 0 0 0 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 1 1 0 0 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 1 0 1 1 1 0 1\n" +
            " 1 0 1 1 1 0 1 0 1 0 1 0 0 0 1 0 1 1 1 0 1\n" +
            " 1 0 0 0 0 0 1 0 0 1 1 1 1 0 1 0 0 0 0 0 1\n" +
            " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
            " 0 0 0 0 0 0 0 0 1 0 1 1 1 0 0 0 0 0 0 0 0\n" +
            " 0 0 0 0 0 1 1 0 0 1 1 0 0 0 1 0 1 0 1 0 1\n" +
            " 0 1 0 1 1 0 0 1 0 1 1 1 1 1 1 0 1 1 1 0 1\n" +
            " 0 1 0 1 1 1 1 0 1 1 0 0 0 1 0 1 0 1 1 0 0\n" +
            " 1 1 1 1 0 1 0 1 0 0 1 0 1 0 0 1 1 1 1 0 0\n" +
            " 1 0 0 1 0 0 1 1 0 1 1 0 1 0 1 0 0 1 0 0 1\n" +
            " 0 0 0 0 0 0 0 0 1 1 1 1 1 0 1 0 1 0 0 1 0\n" +
            " 1 1 1 1 1 1 1 0 0 0 1 1 0 0 1 0 0 0 1 1 0\n" +
            " 1 0 0 0 0 0 1 0 1 1 0 0 0 0 1 0 1 1 1 0 0\n" +
            " 1 0 1 1 1 0 1 0 0 1 0 0 1 0 1 0 1 0 0 0 1\n" +
            " 1 0 1 1 1 0 1 0 0 0 0 0 1 1 1 0 1 1 1 1 0\n" +
            " 1 0 1 1 1 0 1 0 0 0 1 0 0 1 0 0 1 0 1 1 1\n" +
            " 1 0 0 0 0 0 1 0 0 1 0 0 0 1 1 0 0 1 1 1 1\n" +
            " 1 1 1 1 1 1 1 0 0 1 1 1 0 1 1 0 1 0 0 1 0\n" +
            ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }


      [Test]
      public void TestAppendModeInfo()
      {
         BitArray bits = new BitArray();
         Encoder.AppendModeInfo(Mode.NUMERIC, bits);
         Assert.AreEqual(" ...X", bits.ToString());
      }

      [Test]
      public void TestAppendLengthInfo()
      {
         var bits = new BitArray();
         Encoder.AppendLengthInfo(1, // 1 letter (1/1).
                                  Version.GetVersionForNumber(1),
                                  Mode.NUMERIC,
                                  bits);
         Assert.AreEqual(" ........ .X", bits.ToString()); // 10 bits.
         bits = new BitArray();
         Encoder.AppendLengthInfo(2, // 2 letters (2/1).
                                  Version.GetVersionForNumber(10),
                                  Mode.ALPHANUMERIC,
                                  bits);
         Assert.AreEqual(" ........ .X.", bits.ToString()); // 11 bits.
         bits = new BitArray();
         Encoder.AppendLengthInfo(255, // 255 letter (255/1).
                                  Version.GetVersionForNumber(27),
                                  Mode.BYTE,
                                  bits);
         Assert.AreEqual(" ........ XXXXXXXX", bits.ToString()); // 16 bits.
         bits = new BitArray();
         Encoder.AppendLengthInfo(512, // 512 letters (1024/2).
                                  Version.GetVersionForNumber(40),
                                  Mode.KANJI,
                                  bits);
         Assert.AreEqual(" ..X..... ....", bits.ToString()); // 12 bits.
      }

      [Test]
      public void TestAppendBytes()
      {
         // Should use appendNumericBytes.
         // 1 = 01 = 0001 in 4 bits.
         var bits = new BitArray();
         Encoder.AppendBytes("1", Mode.NUMERIC, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual(" ...X", bits.ToString());
         // Should use appendAlphanumericBytes.
         // A = 10 = 0xa = 001010 in 6 bits
         bits = new BitArray();
         Encoder.AppendBytes("A", Mode.ALPHANUMERIC, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual(" ..X.X.", bits.ToString());
         // Lower letters such as 'a' cannot be encoded in MODE_ALPHANUMERIC.
         try
         {
            Encoder.AppendBytes("a", Mode.ALPHANUMERIC, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         }
         catch (WriterException)
         {
            // good
         }
         // Should use append8BitBytes.
         // 0x61, 0x62, 0x63
         bits = new BitArray();
         Encoder.AppendBytes("abc", Mode.BYTE, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual(" .XX....X .XX...X. .XX...XX", bits.ToString());
         // Anything can be encoded in QRCode.MODE_8BIT_BYTE.
         Encoder.AppendBytes("\0", Mode.BYTE, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         // Should use appendKanjiBytes.
         // 0x93, 0x5f
         bits = new BitArray();
         Encoder.AppendBytes(ShiftJisString(new byte[] {0x93, 0x5f}), Mode.KANJI, bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual(" .XX.XX.. XXXXX", bits.ToString());
      }

      [Test]
      public void TestTerminateBits()
      {
         var v = new BitArray();
         Encoder.TerminateBits(0, v);
         Assert.AreEqual("", v.ToString());
         v = new BitArray();
         Encoder.TerminateBits(1, v);
         Assert.AreEqual(" ........", v.ToString());
         v = new BitArray();
         v.AppendBits(0, 3); // Append 000
         Encoder.TerminateBits(1, v);
         Assert.AreEqual(" ........", v.ToString());
         v = new BitArray();
         v.AppendBits(0, 5); // Append 00000
         Encoder.TerminateBits(1, v);
         Assert.AreEqual(" ........", v.ToString());
         v = new BitArray();
         v.AppendBits(0, 8); // Append 00000000
         Encoder.TerminateBits(1, v);
         Assert.AreEqual(" ........", v.ToString());
         v = new BitArray();
         Encoder.TerminateBits(2, v);
         Assert.AreEqual(" ........ XXX.XX..", v.ToString());
         v = new BitArray();
         v.AppendBits(0, 1); // Append 0
         Encoder.TerminateBits(3, v);
         Assert.AreEqual(" ........ XXX.XX.. ...X...X", v.ToString());
      }

      [Test]
      public void TestGetNumDataBytesAndNumEcBytesForBlockId()
      {
         int[] numDataBytes = new int[1];
         int[] numEcBytes = new int[1];
         // Version 1-H.
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(26, 9, 1, 0, numDataBytes, numEcBytes);
         Assert.AreEqual(9, numDataBytes[0]);
         Assert.AreEqual(17, numEcBytes[0]);

         // Version 3-H.  2 blocks.
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(70, 26, 2, 0, numDataBytes, numEcBytes);
         Assert.AreEqual(13, numDataBytes[0]);
         Assert.AreEqual(22, numEcBytes[0]);
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(70, 26, 2, 1, numDataBytes, numEcBytes);
         Assert.AreEqual(13, numDataBytes[0]);
         Assert.AreEqual(22, numEcBytes[0]);

         // Version 7-H. (4 + 1) blocks.
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(196, 66, 5, 0, numDataBytes, numEcBytes);
         Assert.AreEqual(13, numDataBytes[0]);
         Assert.AreEqual(26, numEcBytes[0]);
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(196, 66, 5, 4, numDataBytes, numEcBytes);
         Assert.AreEqual(14, numDataBytes[0]);
         Assert.AreEqual(26, numEcBytes[0]);

         // Version 40-H. (20 + 61) blocks.
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(3706, 1276, 81, 0, numDataBytes, numEcBytes);
         Assert.AreEqual(15, numDataBytes[0]);
         Assert.AreEqual(30, numEcBytes[0]);
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(3706, 1276, 81, 20, numDataBytes, numEcBytes);
         Assert.AreEqual(16, numDataBytes[0]);
         Assert.AreEqual(30, numEcBytes[0]);
         Encoder.GetNumDataBytesAndNumEcBytesForBlockId(3706, 1276, 81, 80, numDataBytes, numEcBytes);
         Assert.AreEqual(16, numDataBytes[0]);
         Assert.AreEqual(30, numEcBytes[0]);
      }

      [Test]
      public void TestInterleaveWithEcBytes()
      {
         byte[] dataBytes = {32, 65, 205, 69, 41, 220, 46, 128, 236};
         var @in = new BitArray();
         foreach (byte dataByte in dataBytes)
         {
            @in.AppendBits(dataByte, 8);
         }
         var @out = Encoder.InterleaveWithEcBytes(@in, 26, 9, 1);
         byte[] expected =
            {
               // Data bytes.
               32, 65, 205, 69, 41, 220, 46, 128, 236,
               // Error correction bytes.
               42, 159, 74, 221, 244, 169, 239, 150, 138, 70,
               237, 85, 224, 96, 74, 219, 61,
            };
         Assert.AreEqual(expected.Length, @out.SizeInBytes);
         var outArray = new byte[expected.Length];
         @out.ToBytes(0, outArray, 0, expected.Length);
         // Can't use Arrays.equals(), because outArray may be longer than out.sizeInBytes()
         for (int x = 0; x < expected.Length; x++)
         {
            Assert.AreEqual(expected[x], outArray[x]);
         }
         // Numbers are from http://www.swetake.com/qr/qr8.html
         dataBytes = new byte[]
            {
               67, 70, 22, 38, 54, 70, 86, 102, 118, 134, 150, 166, 182,
               198, 214, 230, 247, 7, 23, 39, 55, 71, 87, 103, 119, 135,
               151, 166, 22, 38, 54, 70, 86, 102, 118, 134, 150, 166,
               182, 198, 214, 230, 247, 7, 23, 39, 55, 71, 87, 103, 119,
               135, 151, 160, 236, 17, 236, 17, 236, 17, 236,
               17
            };
         @in = new BitArray();
         foreach (byte dataByte in dataBytes)
         {
            @in.AppendBits(dataByte, 8);
         }
         @out = Encoder.InterleaveWithEcBytes(@in, 134, 62, 4);
         expected = new byte[]
            {
               // Data bytes.
               67, 230, 54, 55, 70, 247, 70, 71, 22, 7, 86, 87, 38, 23, 102, 103, 54, 39,
               118, 119, 70, 55, 134, 135, 86, 71, 150, 151, 102, 87, 166,
               160, 118, 103, 182, 236, 134, 119, 198, 17, 150,
               135, 214, 236, 166, 151, 230, 17, 182,
               166, 247, 236, 198, 22, 7, 17, 214, 38, 23, 236, 39,
               17,
               // Error correction bytes.
               175, 155, 245, 236, 80, 146, 56, 74, 155, 165,
               133, 142, 64, 183, 132, 13, 178, 54, 132, 108, 45,
               113, 53, 50, 214, 98, 193, 152, 233, 147, 50, 71, 65,
               190, 82, 51, 209, 199, 171, 54, 12, 112, 57, 113, 155, 117,
               211, 164, 117, 30, 158, 225, 31, 190, 242, 38,
               140, 61, 179, 154, 214, 138, 147, 87, 27, 96, 77, 47,
               187, 49, 156, 214,
            };
         Assert.AreEqual(expected.Length, @out.SizeInBytes);
         outArray = new byte[expected.Length];
         @out.ToBytes(0, outArray, 0, expected.Length);
         for (int x = 0; x < expected.Length; x++)
         {
            Assert.AreEqual(expected[x], outArray[x]);
         }
      }

      [Test]
      public void TestAppendNumericBytes()
      {
         // 1 = 01 = 0001 in 4 bits.
         var bits = new BitArray();
         Encoder.AppendNumericBytes("1", bits);
         Assert.AreEqual(" ...X", bits.ToString());
         // 12 = 0xc = 0001100 in 7 bits.
         bits = new BitArray();
         Encoder.AppendNumericBytes("12", bits);
         Assert.AreEqual(" ...XX..", bits.ToString());
         // 123 = 0x7b = 0001111011 in 10 bits.
         bits = new BitArray();
         Encoder.AppendNumericBytes("123", bits);
         Assert.AreEqual(" ...XXXX. XX", bits.ToString());
         // 1234 = "123" + "4" = 0001111011 + 0100
         bits = new BitArray();
         Encoder.AppendNumericBytes("1234", bits);
         Assert.AreEqual(" ...XXXX. XX.X..", bits.ToString());
         // Empty.
         bits = new BitArray();
         Encoder.AppendNumericBytes("", bits);
         Assert.AreEqual("", bits.ToString());
      }

      [Test]
      public void TestAppendAlphanumericBytes()
      {
         // A = 10 = 0xa = 001010 in 6 bits
         var bits = new BitArray();
         Encoder.AppendAlphanumericBytes("A", bits);
         Assert.AreEqual(" ..X.X.", bits.ToString());
         // AB = 10 * 45 + 11 = 461 = 0x1cd = 00111001101 in 11 bits
         bits = new BitArray();
         Encoder.AppendAlphanumericBytes("AB", bits);
         Assert.AreEqual(" ..XXX..X X.X", bits.ToString());
         // ABC = "AB" + "C" = 00111001101 + 001100
         bits = new BitArray();
         Encoder.AppendAlphanumericBytes("ABC", bits);
         Assert.AreEqual(" ..XXX..X X.X..XX. .", bits.ToString());
         // Empty.
         bits = new BitArray();
         Encoder.AppendAlphanumericBytes("", bits);
         Assert.AreEqual("", bits.ToString());
         // Invalid data.
         try
         {
            Encoder.AppendAlphanumericBytes("abc", new BitArray());
         }
         catch (WriterException)
         {
            // good
         }
      }

      [Test]
      public void TestAppend8BitBytes()
      {
         // 0x61, 0x62, 0x63
         var bits = new BitArray();
         Encoder.Append8BitBytes("abc", bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual(" .XX....X .XX...X. .XX...XX", bits.ToString());
         // Empty.
         bits = new BitArray();
         Encoder.Append8BitBytes("", bits, Encoder.DEFAULT_BYTE_MODE_ENCODING);
         Assert.AreEqual("", bits.ToString());
      }

      // Numbers are from page 21 of JISX0510:2004
      [Test]
      public void TestAppendKanjiBytes()
      {
         BitArray bits = new BitArray();
         Encoder.AppendKanjiBytes(ShiftJisString(new byte[] { (byte)0x93, 0x5f }), bits);
         Assert.AreEqual(" .XX.XX.. XXXXX", bits.ToString());
         Encoder.AppendKanjiBytes(ShiftJisString(new byte[] { (byte)0xe4, (byte)0xaa }), bits);
         Assert.AreEqual(" .XX.XX.. XXXXXXX. X.X.X.X. X.", bits.ToString());
      }

      // Numbers are from http://www.swetake.com/qr/qr3.html and
      // http://www.swetake.com/qr/qr9.html
      [Test]
      public void TestGenerateEcBytes()
      {
         byte[] dataBytes = {32, 65, 205, 69, 41, 220, 46, 128, 236};
         byte[] ecBytes = Encoder.GenerateEcBytes(dataBytes, 17);
         int[] expected =
            {
               42, 159, 74, 221, 244, 169, 239, 150, 138, 70, 237, 85, 224, 96, 74, 219, 61
            };
         Assert.AreEqual(expected.Length, ecBytes.Length);
         for (int x = 0; x < expected.Length; x++)
         {
            Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
         }
         dataBytes = new byte[]
            {
               67, 70, 22, 38, 54, 70, 86, 102, 118,
               134, 150, 166, 182, 198, 214
            };
         ecBytes = Encoder.GenerateEcBytes(dataBytes, 18);
         expected = new []
            {
               175, 80, 155, 64, 178, 45, 214, 233, 65, 209, 12, 155, 117, 31, 140, 214, 27, 187
            };
         Assert.AreEqual(expected.Length, ecBytes.Length);
         for (int x = 0; x < expected.Length; x++)
         {
            Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
         }
         // High-order zero coefficient case.
         dataBytes = new byte[] {32, 49, 205, 69, 42, 20, 0, 236, 17};
         ecBytes = Encoder.GenerateEcBytes(dataBytes, 17);
         expected = new []
            {
               0, 3, 130, 179, 194, 0, 55, 211, 110, 79, 98, 72, 170, 96, 211, 137, 213
            };
         Assert.AreEqual(expected.Length, ecBytes.Length);
         for (int x = 0; x < expected.Length; x++)
         {
            Assert.AreEqual(expected[x], ecBytes[x] & 0xFF);
         }
      }

      [Test]
      public void TestBugInBitVectorNumBytes()
      {
         // There was a bug in BitVector.sizeInBytes() that caused it to return a
         // smaller-by-one value (ex. 1465 instead of 1466) if the number of bits
         // in the vector is not 8-bit aligned.  In QRCodeEncoder::InitQRCode(),
         // BitVector::sizeInBytes() is used for finding the smallest QR Code
         // version that can fit the given data.  Hence there were corner cases
         // where we chose a wrong QR Code version that cannot fit the given
         // data.  Note that the issue did not occur with MODE_8BIT_BYTE, as the
         // bits in the bit vector are always 8-bit aligned.
         //
         // Before the bug was fixed, the following test didn't pass, because:
         //
         // - MODE_NUMERIC is chosen as all bytes in the data are '0'
         // - The 3518-byte numeric data needs 1466 bytes
         //   - 3518 / 3 * 10 + 7 = 11727 bits = 1465.875 bytes
         //   - 3 numeric bytes are encoded in 10 bits, hence the first
         //     3516 bytes are encoded in 3516 / 3 * 10 = 11720 bits.
         //   - 2 numeric bytes can be encoded in 7 bits, hence the last
         //     2 bytes are encoded in 7 bits.
         // - The version 27 QR Code with the EC level L has 1468 bytes for data.
         //   - 1828 - 360 = 1468
         // - In InitQRCode(), 3 bytes are reserved for a header.  Hence 1465 bytes
         //   (1468 -3) are left for data.
         // - Because of the bug in BitVector::sizeInBytes(), InitQRCode() determines
         //   the given data can fit in 1465 bytes, despite it needs 1466 bytes.
         // - Hence QRCodeEncoder.encode() failed and returned false.
         //   - To be precise, it needs 11727 + 4 (getMode info) + 14 (length info) =
         //     11745 bits = 1468.125 bytes are needed (i.e. cannot fit in 1468
         //     bytes).
         StringBuilder builder = new StringBuilder(3518);
         for (int x = 0; x < 3518; x++)
         {
            builder.Append('0');
         }
         Encoder.Encode(builder.ToString(), ErrorCorrectionLevel.L);
      }

      private void VerifyGs1EncodedData(QrCode qrCode)
      {
            string expected =
           "<<\n" +
               " mode: ALPHANUMERIC\n" +
               " ecLevel: H\n" +
               " version: 2\n" +
               " maskPattern: 4\n" +
               " matrix:\n" +
               " 1 1 1 1 1 1 1 0 0 1 1 1 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
               " 1 0 0 0 0 0 1 0 1 1 0 0 0 0 0 1 1 0 1 0 0 0 0 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 0 0 0 1 1 1 0 1 0 1 0 1 1 1 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 1 1 0 0 1 0 1 1 1 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 0 1 1 1 0 0 0 1 0 1 0 1 1 1 0 1\n" +
               " 1 0 0 0 0 0 1 0 1 1 0 1 1 0 1 1 0 0 1 0 0 0 0 0 1\n" +
               " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
               " 0 0 0 0 0 0 0 0 1 1 0 1 1 0 1 1 0 0 0 0 0 0 0 0 0\n" +
               " 0 0 0 0 1 1 1 1 0 0 1 1 0 0 0 1 1 0 1 1 0 0 0 1 0\n" +
               " 0 1 1 0 1 1 0 0 1 1 1 0 0 0 1 1 1 1 1 1 1 0 0 0 1\n" +
               " 0 0 1 1 1 1 1 0 1 1 1 1 1 0 1 0 0 0 0 0 0 1 1 1 0\n" +
               " 1 0 1 1 1 0 0 1 1 1 0 1 1 1 1 1 0 1 1 0 1 1 1 0 0\n" +
               " 0 1 0 1 0 0 1 1 1 1 1 1 0 0 1 1 0 1 0 0 0 0 0 1 0\n" +
               " 1 0 0 1 1 1 0 0 1 1 0 0 0 1 1 0 1 0 1 0 1 0 0 0 0\n" +
               " 0 0 1 0 0 1 1 1 0 1 1 0 1 1 1 0 1 1 1 0 1 1 1 1 0\n" +
               " 0 0 0 1 1 0 0 1 0 0 1 0 0 1 1 0 0 1 0 0 0 1 1 1 0\n" +
               " 1 1 0 1 0 1 1 0 1 0 1 0 0 0 1 1 1 1 1 1 1 0 0 0 0\n" +
               " 0 0 0 0 0 0 0 0 1 1 0 1 0 0 0 1 1 0 0 0 1 1 0 1 0\n" +
               " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 0 1 0 1 0 0 0 0\n" +
               " 1 0 0 0 0 0 1 0 1 1 0 0 0 1 0 1 1 0 0 0 1 0 1 1 0\n" +
               " 1 0 1 1 1 0 1 0 1 1 1 0 0 0 0 0 1 1 1 1 1 1 0 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 0 0 0 0 1 1 1 0 0 1 1 0 1 0 0 0\n" +
               " 1 0 1 1 1 0 1 0 0 0 1 1 0 1 0 1 1 1 0 1 1 0 0 1 0\n" +
               " 1 0 0 0 0 0 1 0 0 1 1 0 1 1 1 1 1 0 1 0 1 1 0 0 0\n" +
               " 1 1 1 1 1 1 1 0 0 0 1 0 0 0 0 1 1 0 0 1 1 0 0 1 1\n" +
               ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      private void VerifyNotGs1EncodedData(QrCode qrCode)
      {
            string expected =
           "<<\n" +
               " mode: ALPHANUMERIC\n" +
               " ecLevel: H\n" +
               " version: 1\n" +
               " maskPattern: 4\n" +
               " matrix:\n" +
               " 1 1 1 1 1 1 1 0 0 1 0 1 0 0 1 1 1 1 1 1 1\n" +
               " 1 0 0 0 0 0 1 0 1 0 1 0 1 0 1 0 0 0 0 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 1 0 0 1 0 1 0 1 1 1 0 1\n" +
               " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 1 0 1 1 1 0 1\n" +
               " 1 0 0 0 0 0 1 0 1 0 0 1 1 0 1 0 0 0 0 0 1\n" +
               " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
               " 0 0 0 0 0 0 0 0 1 0 0 0 1 0 0 0 0 0 0 0 0\n" +
               " 0 0 0 0 1 1 1 1 0 1 1 0 1 0 1 1 0 0 0 1 0\n" +
               " 0 0 0 0 1 1 0 1 1 1 0 0 1 1 1 1 0 1 1 0 1\n" +
               " 1 0 0 0 0 1 1 0 0 1 0 1 0 0 0 1 1 1 0 1 1\n" +
               " 1 0 0 1 1 1 0 0 1 1 1 1 0 0 0 0 1 0 0 0 0\n" +
               " 0 1 1 1 1 1 1 0 1 0 1 0 1 1 1 0 0 1 1 0 0\n" +
               " 0 0 0 0 0 0 0 0 1 1 0 0 0 1 1 0 0 0 1 0 1\n" +
               " 1 1 1 1 1 1 1 0 1 1 1 1 0 0 0 0 0 1 1 0 0\n" +
               " 1 0 0 0 0 0 1 0 1 1 0 1 0 0 0 1 0 1 1 1 1\n" +
               " 1 0 1 1 1 0 1 0 1 0 0 1 0 0 0 1 1 0 0 1 1\n" +
               " 1 0 1 1 1 0 1 0 0 0 1 1 0 1 0 0 0 0 1 1 1\n" +
               " 1 0 1 1 1 0 1 0 0 1 0 1 0 0 0 1 1 0 0 0 0\n" +
               " 1 0 0 0 0 0 1 0 0 1 0 0 1 0 0 1 1 0 0 0 1\n" +
               " 1 1 1 1 1 1 1 0 0 0 1 0 0 1 0 0 0 0 1 1 1\n" +
               ">>\n";
         Assert.AreEqual(expected, qrCode.ToString());
      }

      private static string ShiftJisString(byte[] bytes)
      {
         try
         {
            return Encoding.GetEncoding("Shift_JIS").GetString(bytes, 0, bytes.Length);
         }
         catch (ArgumentException uee)
         {
            throw new WriterException(uee.ToString());
         }
      }
   }
}