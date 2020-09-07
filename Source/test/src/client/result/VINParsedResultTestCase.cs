/*
 * Copyright 2014 ZXing authors
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
    [TestFixture]
   public class VinParsedResultTestCase
   {

      [Test]
      public void TestNotVin()
      {
         var fakeResult = new BarCodeText("1M8GDM9A1KP042788", null, null, null, BarcodeFormat.CODE_39);
         var result = ResultParser.ParseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.TEXT, result.Type);
         fakeResult = new BarCodeText("1M8GDM9AXKP042788", null, null, null, BarcodeFormat.CODE_128);
         result = ResultParser.ParseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.TEXT, result.Type);
      }

      [Test]
      public void TestVin()
      {
         DoTest("1M8GDM9AXKP042788", "1M8", "GDM9AX", "KP042788", "US", "GDM9A", 1989, 'P', "042788");
         DoTest("I1M8GDM9AXKP042788", "1M8", "GDM9AX", "KP042788", "US", "GDM9A", 1989, 'P', "042788");
         DoTest("LJCPCBLCX11000237", "LJC", "PCBLCX", "11000237", "CN", "PCBLC", 2001, '1', "000237");
      }

      static void DoTest(string contents,
                                 string wmi,
                                 string vds,
                                 string vis,
                                 string country,
                                 string attributes,
                                 int year,
                                 char plant,
                                 string sequential)
      {
         var fakeResult = new BarCodeText(contents, null, null, null, BarcodeFormat.CODE_39);
         var result = ResultParser.ParseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.VIN, result.Type);
         var vinResult = (VINParsedResult) result;
         Assert.AreEqual(wmi, vinResult.WorldManufacturerID);
         Assert.AreEqual(vds, vinResult.VehicleDescriptorSection);
         Assert.AreEqual(vis, vinResult.VehicleIdentifierSection);
         Assert.AreEqual(country, vinResult.CountryCode);
         Assert.AreEqual(attributes, vinResult.VehicleAttributes);
         Assert.AreEqual(year, vinResult.ModelYear);
         Assert.AreEqual(plant, vinResult.PlantCode);
         Assert.AreEqual(sequential, vinResult.SequentialNumber);
      }
   }
}