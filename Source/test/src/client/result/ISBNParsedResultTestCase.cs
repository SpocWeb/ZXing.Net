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

namespace ZXing.Client.Result.Test
{
    /// <summary>
    /// Tests <see cref="ISBNParsedResult" />.
    ///
    /// <author>Sean Owen</author>
    /// </summary>
    [TestFixture]
   public sealed class IsbnParsedResultTestCase
   {
      [Test]
      public void TestIsbn()
      {
         DoTest("9784567890123");
      }

      static void DoTest(string contents)
      {
         BarCodeText fakeResult = new BarCodeText(contents, null, null, null, BarcodeFormat.EAN_13);
         ParsedResult result = ResultParser.ParseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.ISBN, result.Type);
         ISBNParsedResult isbnResult = (ISBNParsedResult)result;
         Assert.AreEqual(contents, isbnResult.ISBN);
      }
   }
}