/*
 * Copyright 2006 Jeremias Maerki.
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
using System.Text;

using NUnit.Framework;

using ZXing.Datamatrix.Encoder;

namespace ZXing.Datamatrix.Test
{
   /// <summary>
   /// Tests for {@link HighLevelEncoder}.
   /// </summary>
   [TestFixture]
   public sealed class HighLevelEncodeTestCase
   {
      private static readonly SymbolInfo[] TEST_SYMBOLS =
         {
            new SymbolInfo(false, 3, 5, 8, 8, 1),
            new SymbolInfo(false, 5, 7, 10, 10, 1),
            /*rect*/new SymbolInfo(true, 5, 7, 16, 6, 1),
            new SymbolInfo(false, 8, 10, 12, 12, 1),
            /*rect*/new SymbolInfo(true, 10, 11, 14, 6, 2),
            new SymbolInfo(false, 13, 0, 0, 0, 1),
            new SymbolInfo(false, 77, 0, 0, 0, 1)
            //The last entries are fake entries to test special conditions with C40 encoding
         };

      private static void UseTestSymbols()
      {
         SymbolInfo.overrideSymbolSet(TEST_SYMBOLS);
      }

      private static void ResetSymbols()
      {
         SymbolInfo.overrideSymbolSet(SymbolInfo.PROD_SYMBOLS);
      }

      [Test]
      public void TestAsciiEncodation()
      {

            string visualized = EncodeHighLevel("123456");
         Assert.AreEqual("142 164 186", visualized);

         visualized = EncodeHighLevel("123456£");
         Assert.AreEqual("142 164 186 235 36", visualized);

         visualized = EncodeHighLevel("30Q324343430794<OQQ");
         Assert.AreEqual("160 82 162 173 173 173 137 224 61 80 82 82", visualized);
      }

      [Test]
      public void TestC40EncodationBasic1()
      {

            string visualized = EncodeHighLevel("AIMAIMAIM");
         Assert.AreEqual("230 91 11 91 11 91 11 254", visualized);
         //230 shifts to C40 encodation, 254 unlatches, "else" case
      }

      [Test]
      public void TestC40EncodationBasic2()
      {

            string visualized = EncodeHighLevel("AIMAIAB");
         Assert.AreEqual("230 91 11 90 255 254 67 129", visualized);
         //"B" is normally encoded as "15" (one C40 value)
         //"else" case: "B" is encoded as ASCII

         visualized = EncodeHighLevel("AIMAIAb");
         Assert.AreEqual("66 74 78 66 74 66 99 129", visualized); //Encoded as ASCII
         //Alternative solution:
         //Assert.AreEqual("230 91 11 90 255 254 99 129", visualized);
         //"b" is normally encoded as "Shift 3, 2" (two C40 values)
         //"else" case: "b" is encoded as ASCII

         visualized = EncodeHighLevel("AIMAIMAIMË");
         Assert.AreEqual("230 91 11 91 11 91 11 254 235 76", visualized);
         //Alternative solution:
         //Assert.AreEqual("230 91 11 91 11 91 11 11 9 254", visualized);
         //Expl: 230 = shift to C40, "91 11" = "AIM",
         //"11 9" = "�" = "Shift 2, UpperShift, <char>
         //"else" case

         visualized = EncodeHighLevel("AIMAIMAIMë");
         Assert.AreEqual("230 91 11 91 11 91 11 254 235 108", visualized); //Activate when additional rectangulars are available
         //Expl: 230 = shift to C40, "91 11" = "AIM",
         //"�" in C40 encodes to: 1 30 2 11 which doesn't fit into a triplet
         //"10 243" =
         //254 = unlatch, 235 = Upper Shift, 108 = � = 0xEB/235 - 128 + 1
         //"else" case
      }

      [Test]
      public void TestC40EncodationSpecExample()
      {
            //Example in Figure 1 in the spec
            string visualized = EncodeHighLevel("A1B2C3D4E5F6G7H8I9J0K1L2");
         Assert.AreEqual("230 88 88 40 8 107 147 59 67 126 206 78 126 144 121 35 47 254", visualized);
      }

      [Test]
      public void TestC40EncodationSpecialCases1()
      {

         //Special tests avoiding ultra-long test strings because these tests are only used
         //with the 16x48 symbol (47 data codewords)
         UseTestSymbols();

            string visualized = EncodeHighLevel("AIMAIMAIMAIMAIMAIM");
         Assert.AreEqual("230 91 11 91 11 91 11 91 11 91 11 91 11", visualized);
         //case "a": Unlatch is not required

         visualized = EncodeHighLevel("AIMAIMAIMAIMAIMAI");
         Assert.AreEqual("230 91 11 91 11 91 11 91 11 91 11 90 241", visualized);
         //case "b": Add trailing shift 0 and Unlatch is not required

         visualized = EncodeHighLevel("AIMAIMAIMAIMAIMA");
         Assert.AreEqual("230 91 11 91 11 91 11 91 11 91 11 254 66", visualized);
         //case "c": Unlatch and write last character in ASCII

         ResetSymbols();

         visualized = EncodeHighLevel("AIMAIMAIMAIMAIMAI");
         Assert.AreEqual("230 91 11 91 11 91 11 91 11 91 11 254 66 74 129 237", visualized);

         visualized = EncodeHighLevel("AIMAIMAIMA");
         Assert.AreEqual("230 91 11 91 11 91 11 66", visualized);
         //case "d": Skip Unlatch and write last character in ASCII
      }

      [Test]
      public void TestC40EncodationSpecialCases2()
      {

            string visualized = EncodeHighLevel("AIMAIMAIMAIMAIMAIMAI");
         Assert.AreEqual("230 91 11 91 11 91 11 91 11 91 11 91 11 254 66 74", visualized);
         //available > 2, rest = 2 --> unlatch and encode as ASCII
      }

      [Test]
      public void TestTextEncodation()
      {

            string visualized = EncodeHighLevel("aimaimaim");
         Assert.AreEqual("239 91 11 91 11 91 11 254", visualized);
         //239 shifts to Text encodation, 254 unlatches

         visualized = EncodeHighLevel("aimaimaim'");
         Assert.AreEqual("239 91 11 91 11 91 11 254 40 129", visualized);
         //Assert.AreEqual("239 91 11 91 11 91 11 7 49 254", visualized);
         //This is an alternative, but doesn't strictly follow the rules in the spec.

         visualized = EncodeHighLevel("aimaimaIm");
         Assert.AreEqual("239 91 11 91 11 87 218 110", visualized);

         visualized = EncodeHighLevel("aimaimaimB");
         Assert.AreEqual("239 91 11 91 11 91 11 254 67 129", visualized);

         visualized = EncodeHighLevel("aimaimaim{txt}\u0004");
         Assert.AreEqual("239 91 11 91 11 91 11 16 218 236 107 181 69 254 129 237", visualized);
      }

      [Test]
      public void TestX12Encodation()
      {

            //238 shifts to X12 encodation, 254 unlatches

            string visualized = EncodeHighLevel("ABC>ABC123>AB");
         Assert.AreEqual("238 89 233 14 192 100 207 44 31 67", visualized);

         visualized = EncodeHighLevel("ABC>ABC123>ABC");
         Assert.AreEqual("238 89 233 14 192 100 207 44 31 254 67 68", visualized);

         visualized = EncodeHighLevel("ABC>ABC123>ABCD");
         Assert.AreEqual("238 89 233 14 192 100 207 44 31 96 82 254", visualized);

         visualized = EncodeHighLevel("ABC>ABC123>ABCDE");
         Assert.AreEqual("238 89 233 14 192 100 207 44 31 96 82 70", visualized);

         visualized = EncodeHighLevel("ABC>ABC123>ABCDEF");
         Assert.AreEqual("238 89 233 14 192 100 207 44 31 96 82 254 70 71 129 237", visualized);

         visualized = EncodeHighLevel("ABC>900>8123567");
         Assert.AreEqual("238 89 233 14 141 25 93 254 142 165 197 129", visualized);
      }

      [Test]
      public void TestEdifactEncodation()
      {

            //240 shifts to EDIFACT encodation

            string visualized = EncodeHighLevel(".A.C1.3.DATA.123DATA.123DATA");
         Assert.AreEqual("240 184 27 131 198 236 238 16 21 1 187 28 179 16 21 1 187 28 179 16 21 1",
                         visualized);

         visualized = EncodeHighLevel(".A.C1.3.X.X2..");
         Assert.AreEqual("240 184 27 131 198 236 238 98 230 50 47 47", visualized);

         visualized = EncodeHighLevel(".A.C1.3.X.X2.");
         Assert.AreEqual("240 184 27 131 198 236 238 98 230 50 47 129", visualized);

         visualized = EncodeHighLevel(".A.C1.3.X.X2");
         Assert.AreEqual("240 184 27 131 198 236 238 98 230 50", visualized);

         visualized = EncodeHighLevel(".A.C1.3.X.X");
         Assert.AreEqual("240 184 27 131 198 236 238 98 230 31", visualized);

         visualized = EncodeHighLevel(".A.C1.3.X.");
         Assert.AreEqual("240 184 27 131 198 236 238 98 231 192", visualized);

         visualized = EncodeHighLevel(".A.C1.3.X");
         Assert.AreEqual("240 184 27 131 198 236 238 89", visualized);

         //Checking temporary unlatch from EDIFACT
         visualized = EncodeHighLevel(".XXX.XXX.XXX.XXX.XXX.XXX.üXX.XXX.XXX.XXX.XXX.XXX.XXX");
         Assert.AreEqual("240 185 134 24 185 134 24 185 134 24 185 134 24 185 134 24 185 134 24"
                         + " 124 47 235 125 240" //<-- this is the temporary unlatch
                         + " 97 139 152 97 139 152 97 139 152 97 139 152 97 139 152 97 139 152 89 89",
                         visualized);
      }

      [Test]
      public void TestBase256Encodation()
      {

            //231 shifts to Base256 encodation

            string visualized = EncodeHighLevel("«äöüé»");
         Assert.AreEqual("231 44 108 59 226 126 1 104", visualized);
         visualized = EncodeHighLevel("«äöüéà»");
         Assert.AreEqual("231 51 108 59 226 126 1 141 254 129", visualized);
         visualized = EncodeHighLevel("«äöüéàá»");
         Assert.AreEqual("231 44 108 59 226 126 1 141 36 147", visualized);

         visualized = EncodeHighLevel(" 23£"); //ASCII only (for reference)
         Assert.AreEqual("33 153 235 36 129", visualized);

         visualized = EncodeHighLevel("«äöüé» 234"); //Mixed Base256 + ASCII
         Assert.AreEqual("231 51 108 59 226 126 1 104 99 153 53 129", visualized);

         visualized = EncodeHighLevel("«äöüé» 23£ 1234567890123456789");
         Assert.AreEqual("231 55 108 59 226 126 1 104 99 10 161 167 185 142 164 186 208"
                         + " 220 142 164 186 208 58 129 59 209 104 254 150 45", visualized);

         visualized = EncodeHighLevel(CreateBinaryMessage(20));
         Assert.AreEqual("231 44 108 59 226 126 1 141 36 5 37 187 80 230 123 17 166 60 210 103 253 150",
                         visualized);
         visualized = EncodeHighLevel(CreateBinaryMessage(19)); //padding necessary at the end
         Assert.AreEqual("231 63 108 59 226 126 1 141 36 5 37 187 80 230 123 17 166 60 210 103 1 129",
                         visualized);

         visualized = EncodeHighLevel(CreateBinaryMessage(276));
         AssertStartsWith("231 38 219 2 208 120 20 150 35", visualized);
         AssertEndsWith("146 40 194 129", visualized);

         visualized = EncodeHighLevel(CreateBinaryMessage(277));
         AssertStartsWith("231 38 220 2 208 120 20 150 35", visualized);
         AssertEndsWith("146 40 190 87", visualized);
      }

      private static string CreateBinaryMessage(int len)
      {
         var sb = new StringBuilder();
         sb.Append("«äöüéàá-");
         for (int i = 0; i < len - 9; i++)
         {
            sb.Append('\u00B7');
         }
         sb.Append('»');
         return sb.ToString();
      }

      private static void AssertStartsWith(string expected, string actual)
      {
         if (!actual.StartsWith(expected))
         {
            throw new AssertionException(actual);
         }
      }

      private static void AssertEndsWith(string expected, string actual)
      {
         if (!actual.EndsWith(expected))
         {
            throw new AssertionException(actual);
         }
      }

      [Test]
      public void TestUnlatchingFromC40()
      {

            string visualized = EncodeHighLevel("AIMAIMAIMAIMaimaimaim");
         Assert.AreEqual("230 91 11 91 11 91 11 254 66 74 78 239 91 11 91 11 91 11", visualized);
      }

      [Test]
      public void TestUnlatchingFromText()
      {

            string visualized = EncodeHighLevel("aimaimaimaim12345678");
         Assert.AreEqual("239 91 11 91 11 91 11 91 11 254 142 164 186 208 129 237", visualized);
      }

      [Test]
      public void TestHelloWorld()
      {

            string visualized = EncodeHighLevel("Hello World!");
         Assert.AreEqual("73 239 116 130 175 123 148 64 158 233 254 34", visualized);
      }

      [Test]
      public void TestBug1664266()
      {
            //There was an exception and the encoder did not handle the unlatching from
            //EDIFACT encoding correctly

            string visualized = EncodeHighLevel("CREX-TAN:h");
         Assert.AreEqual("240 13 33 88 181 64 78 124 59 105", visualized);

         visualized = EncodeHighLevel("CREX-TAN:hh");
         Assert.AreEqual("240 13 33 88 181 64 78 124 59 105 105 129", visualized);

         visualized = EncodeHighLevel("CREX-TAN:hhh");
         Assert.AreEqual("240 13 33 88 181 64 78 124 59 105 105 105", visualized);
      }

      [Test]
      public void TestX12Unlatch()
      {
            string visualized = EncodeHighLevel("*DTCP01");
         Assert.AreEqual("238 9 10 104 141 254 50 129", visualized);
      }

      [Test]
      public void TestX12Unlatch2()
      {
            string visualized = EncodeHighLevel("*DTCP0");
         Assert.AreEqual("238 9 10 104 141", visualized);
      }

      [Test]
      public void TestBug3048549()
      {
            //There was an IllegalArgumentException for an illegal character here because
            //of an encoding problem of the character 0x0060 in Java source code.

            string visualized = EncodeHighLevel("fiykmj*Rh2`,e6");
         Assert.AreEqual("239 122 87 154 40 7 171 115 207 12 130 71 155 254 129 237", visualized);

      }

      [Test]
      public void TestMacroCharacters()
      {

            string visualized = EncodeHighLevel("[)>\u001E05\u001D5555\u001C6666\u001E\u0004");
         //Assert.AreEqual("92 42 63 31 135 30 185 185 29 196 196 31 5 129 87 237", visualized);
         Assert.AreEqual("236 185 185 29 196 196 129 56", visualized);
      }

      [Test]
      public void TestEncodingWithStartAsX12AndLatchToEdifactInTheMiddle()
      {

            string visualized = EncodeHighLevel("*MEMANT-1F-MESTECH");
         Assert.AreEqual("238 10 99 164 204 254 240 82 220 70 180 209 83 80 80 200", visualized);
      }

      [Ignore]
      [Test]  
      public void TestDataUrl() {

        byte[] data = {0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
            0x7E, 0x7F, (byte) 0x80, (byte) 0x81, (byte) 0x82};
            string expected = EncodeHighLevel(Encoding.GetEncoding("ISO8859-1").GetString(data, 0, data.Length));
            string visualized = EncodeHighLevel("url(data:text/plain;charset=iso-8859-1,"
                                                + "%00%01%02%03%04%05%06%07%08%09%0A%7E%7F%80%81%82)");
        Assert.AreEqual(expected, visualized);
        Assert.AreEqual("1 2 3 4 5 6 7 8 9 10 11 231 153 173 67 218 112 7", visualized);

        visualized = EncodeHighLevel("url(data:;base64,flRlc3R+)");
        Assert.AreEqual("127 85 102 116 117 127 129 56", visualized);
      }

      private static string EncodeHighLevel(string msg)
      {
            string encoded = HighLevelEncoder.encodeHighLevel(msg);
         //DecodeHighLevel.decode(encoded);
         return Visualize(encoded);
      }

      /// <summary>
      /// Convert a string of char codewords into a different string which lists each character
      /// using its decimal value.
      /// </summary>
      /// <param name="codewords">The codewords.</param>
      /// <returns>the visualized codewords</returns>
      internal static string Visualize(string codewords)
      {
         var sb = new StringBuilder();
         for (int i = 0; i < codewords.Length; i++)
         {
            if (i > 0)
            {
               sb.Append(' ');
            }
            sb.Append((int) codewords[i]);
         }
         return sb.ToString();
      }
   }
}