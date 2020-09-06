﻿/**
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
 *
 * This software consists of contributions made by many individuals,
 * listed below:
 *
 * <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
 * <author>Eduardo Castillejo, University of Deusto (eduardo.castillejo@deusto.es)</author>
 *
 * These authors would like to acknowledge the Spanish Ministry of Industry,
 * Tourism and Trade, for the support in the project TSI020301-2008-2
 * "PIRAmIDE: Personalizable Interactions with Resources on AmI-enabled
 * Mobile Dynamic Environments", leaded by Treelogic
 * ( http://www.treelogic.com/ ):
 *
 *   http://www.piramidepse.com/
 *
 */

using System.Collections.Generic;
#if !SILVERLIGHT
#else
using System.Windows.Media.Imaging;
#endif
using NUnit.Framework;
using ZXing.Client.Result;

namespace ZXing.OneD.RSS.Expanded.Test
{
   /// <summary>
   /// <author>Pablo Orduña, University of Deusto (pablo.orduna@deusto.es)</author>
   /// <author>Eduardo Castillejo, University of Deusto (eduardo.castillejo@deusto.es)</author>
   /// </summary>
   [TestFixture]
   public sealed class RssExpandedImage2ResultTestCase
   {
      [Test]
      public void testDecodeRow2result_2()
      {
         // (01)90012345678908(3103)001750
         var path = "2.png";
         var expected =
            new ExpandedProductParsedResult("(01)90012345678908(3103)001750",
               "90012345678908",
               null, null, null, null, null, null,
               "001750",
               ExpandedProductParsedResult.KILOGRAM,
               "3", null, null, null, new Dictionary<string, string>());

         AssertCorrectImage2Result(path, expected);
      }

      static void AssertCorrectImage2Result(string imageFileName, ExpandedProductParsedResult expected)
      {
         var rssExpandedReader = new RssExpandedReader();

         var binaryMap = TestCaseUtil.GetBinaryBitmap("test/data/blackbox/rssexpanded-1", imageFileName);
         var rowNumber = binaryMap.Height / 2;
         var row = binaryMap.GetBlackRow(rowNumber, null);

         var theResult = rssExpandedReader.DecodeRow(rowNumber, row, null);
         Assert.IsNotNull(theResult);

         Assert.AreEqual(BarcodeFormat.RSS_EXPANDED, theResult.BarcodeFormat);

         var result = ResultParser.parseResult(theResult);

         Assert.AreEqual(expected, result);
      }
   }
}