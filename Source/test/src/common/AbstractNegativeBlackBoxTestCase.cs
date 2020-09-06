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
using System.IO;
using System.Linq;
#if !SILVERLIGHT
using System.Drawing;
#else
using System.Windows.Media.Imaging;
#endif
using NUnit.Framework;

namespace ZXing.Common.Test
{
   /// <summary>
   /// This abstract class looks for negative results, i.e. it only allows a certain number of false
   /// positives in images which should not decode. This helps ensure that we are not too lenient.
   ///
   /// <author>dswitkin@google.com (Daniel Switkin)</author>
   /// </summary>
   [TestFixture]
   public abstract class AbstractNegativeBlackBoxTestCase : AbstractBlackBoxTestCase
   {
#if !SILVERLIGHT
      private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
      private static readonly DanielVaughan.Logging.ILog Log = DanielVaughan.Logging.LogManager.GetLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif

      private readonly List<TestResult> _TestResults;

      private class TestResult
      {
         private readonly int _FalsePositivesAllowed;
         private readonly float _Rotation;

         public TestResult(int falsePositivesAllowed, float rotation)
         {
            this._FalsePositivesAllowed = falsePositivesAllowed;
            this._Rotation = rotation;
         }

         public int GetFalsePositivesAllowed()
         {
            return _FalsePositivesAllowed;
         }

         public float GetRotation()
         {
            return _Rotation;
         }
      }

      // Use the multiformat reader to evaluate all decoders in the system.
      protected AbstractNegativeBlackBoxTestCase(string testBasePathSuffix)
         : base(testBasePathSuffix, new MultiFormatReader(), null)
      {
         _TestResults = new List<TestResult>();
      }

      protected void AddTest(int falsePositivesAllowed, float rotation)
      {
         _TestResults.Add(new TestResult(falsePositivesAllowed, rotation));
      }

      [Test]
      [Ignore("2020-09-03 Fails in BaseLine")]
      public new void TestBlackBox()
      {
         Assert.IsFalse(_TestResults.Count == 0);

         var imageFiles = GetImageFiles();
         int[] falsePositives = new int[_TestResults.Count];
         foreach (var testImage in imageFiles)
         {
            var absPath = Path.GetFullPath(testImage);
            Log.InfoFormat("Starting {0}", absPath);

#if !SILVERLIGHT
            var image = new Bitmap(Image.FromFile(testImage));
#else
            var image = new WriteableBitmap(0, 0);
            image.SetSource(File.OpenRead(testImage));
#endif
            for (int x = 0; x < _TestResults.Count; x++)
            {
               TestResult testResult = _TestResults[x];
               if (!CheckForFalsePositives(image, testResult.GetRotation()))
               {
                  falsePositives[x]++;
               }
            }
         }

         int totalFalsePositives = 0;
         int totalAllowed = 0;

         for (int x = 0; x < _TestResults.Count; x++)
         {
            TestResult testResult = _TestResults[x];
            totalFalsePositives += falsePositives[x];
            totalAllowed += testResult.GetFalsePositivesAllowed();
         }

         if (totalFalsePositives < totalAllowed)
         {
            Log.InfoFormat("+++ Test too lax by {0} images", totalAllowed - totalFalsePositives);
         }
         else if (totalFalsePositives > totalAllowed)
         {
            Log.InfoFormat("--- Test failed by {0} images", totalFalsePositives - totalAllowed);
         }

         for (int x = 0; x < _TestResults.Count; x++)
         {
            TestResult testResult = _TestResults[x];
            Log.InfoFormat("Rotation {0} degrees: {1} of {2} images were false positives ({3} allowed)",
                           (int) testResult.GetRotation(), falsePositives[x], imageFiles.Count(),
                           testResult.GetFalsePositivesAllowed());
            Assert.IsTrue(falsePositives[x] <= testResult.GetFalsePositivesAllowed(), "Rotation " + testResult.GetRotation() + " degrees: Too many false positives found");
         }
      }

      /// <summary>
      /// Make sure ZXing does NOT find a barcode in the image.
      ///
      /// <param name="image">The image to test</param>
      /// <param name="rotationInDegrees">The amount of rotation to apply</param>
      /// <returns>true if nothing found, false if a non-existent barcode was detected</returns>
      /// </summary>
#if !SILVERLIGHT
      private bool CheckForFalsePositives(Bitmap image, float rotationInDegrees)
#else
      private bool checkForFalsePositives(WriteableBitmap image, float rotationInDegrees)
#endif
      {
         var rotatedImage = RotateImage(image, rotationInDegrees);
         var source = new BitmapLuminanceSource(rotatedImage);
         var bitmap = new BinaryBitmap(new TwoDBinarizer(source));
         var result = GetReader().Decode(bitmap);
         if (result != null)
         {
            Log.InfoFormat("Found false positive: '{0}' with format '{1}' (rotation: {2})",
                           result.Text, result.BarcodeFormat, (int) rotationInDegrees);
            return false;
         }

            // Try "try harder" getMode
            var hints = new Dictionary<DecodeHintType, object>
            {
                [DecodeHintType.TRY_HARDER] = true
            };
            result = GetReader().Decode(bitmap, hints);
         if (result != null)
         {
            Log.InfoFormat("Try harder found false positive: '{0}' with format '{1}' (rotation: {2})",
                           result.Text, result.BarcodeFormat, (int) rotationInDegrees);
            return false;
         }
         return true;
      }
   }
}