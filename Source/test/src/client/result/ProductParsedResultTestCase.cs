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
    /// Tests <see cref="ProductParsedResult" />.
    ///
    /// <author>Sean Owen</author>
    /// </summary>
    [TestFixture]
   public sealed class ProductParsedResultTestCase
   {
      [Test]
      public void TestProduct()
      {
         DoTest("123456789012", "123456789012", BarcodeFormat.UPC_A);
         DoTest("00393157", "00393157", BarcodeFormat.EAN_8);
         DoTest("5051140178499", "5051140178499", BarcodeFormat.EAN_13);
         DoTest("01234565", "012345000065", BarcodeFormat.UPC_E);
      }

      static void DoTest(string contents, string normalized, BarcodeFormat format)
      {
         ZXing.BarCodeText fakeResult = new ZXing.BarCodeText(contents, null, null, null, format);
         ParsedResult result = ResultParser.parseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.PRODUCT, result.Type);
         ProductParsedResult productResult = (ProductParsedResult)result;
         Assert.AreEqual(contents, productResult.ProductID);
         Assert.AreEqual(normalized, productResult.NormalizedProductID);
      }
   }
}