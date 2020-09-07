/*
 * Copyright (C) 2014 ZXing authors
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

using System.Text;

using NUnit.Framework;

namespace ZXing.PDF417.Internal.Test
{
   public sealed class Pdf417EncoderTestCase
   {
      [Test]
      public void TestEncodeAuto()
      {
         var encoded = Pdf417HighLevelEncoder.EncodeHighLevel(
            "ABCD", Compaction.AUTO, Encoding.UTF8, false);
         Assert.AreEqual("\u039f\u001A\u0385ABCD", encoded);
      }

      [Test]
      public void TestEncodeAutoWithSpecialChars()
      {
         //Just check if this does not throw an exception
         Pdf417HighLevelEncoder.EncodeHighLevel(
            "1%§s ?aG$", Compaction.AUTO, Encoding.UTF8, false);
      }
 
      [Test]
      public void TestEncodeIso88591WithSpecialChars()
      {
	      // Just check if this does not throw an exception
         Pdf417HighLevelEncoder.EncodeHighLevel("asdfg§asd", Compaction.AUTO, Encoding.GetEncoding("ISO8859-1"), false);
      }

      [Test]
      public void TestEncodeText()
      {
         var encoded = Pdf417HighLevelEncoder.EncodeHighLevel(
            "ABCD", Compaction.TEXT, Encoding.UTF8, false);
         Assert.AreEqual("Ο\u001A\u0001?", encoded);
      }

      [Test]
      public void TestEncodeNumeric()
      {
         var encoded = Pdf417HighLevelEncoder.EncodeHighLevel(
            "1234", Compaction.NUMERIC, Encoding.UTF8, false);
         Assert.AreEqual("\u039f\u001A\u0386\f\u01b2", encoded);
      }

      [Test]
      public void TestEncodeByte()
      {
         var encoded = Pdf417HighLevelEncoder.EncodeHighLevel(
            "abcd", Compaction.BYTE, Encoding.UTF8, false);
         Assert.AreEqual("\u039f\u001A\u0385abcd", encoded);
      }
   }
}