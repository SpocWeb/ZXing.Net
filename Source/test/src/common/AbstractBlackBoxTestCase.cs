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
using System.Globalization;
using System.IO;
using System.Linq;
#if !SILVERLIGHT
using System.Drawing;
#else
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif
using NUnit.Framework;
using ZXing.Multi;

namespace ZXing.Common.Test
{
   /// <summary>
   /// <author>Sean Owen</author>
   /// <author>dswitkin@google.com (Daniel Switkin)</author>
   /// </summary>
   public abstract class AbstractBlackBoxTestCase
   {
#if !SILVERLIGHT
       static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#else
      private static readonly DanielVaughan.Logging.ILog Log = DanielVaughan.Logging.LogManager.GetLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif

      public bool Accept(string dir, string name)
      {
            string lowerCase = name.ToLower(CultureInfo.InvariantCulture);
         return lowerCase.EndsWith(".jpg") || lowerCase.EndsWith(".jpeg") ||
                lowerCase.EndsWith(".gif") || lowerCase.EndsWith(".png");
      }

      readonly string _TestBase;
      readonly IBarCodeDecoder _BarcodeReader;
      readonly BarcodeFormat? _ExpectedFormat;
      readonly List<TestResult> _TestResults;

      public static string BuildTestBase(string testBasePathSuffix)
      {
         // A little workaround to prevent aggravation in my IDE
         if (!Directory.Exists(testBasePathSuffix))
         {
            // try starting with 'core' since the test base is often given as the project root
            return Path.Combine("..\\..\\..\\Source", testBasePathSuffix);
         }
         return testBasePathSuffix;
      }

      protected AbstractBlackBoxTestCase(string testBasePathSuffix,
                                         IBarCodeDecoder barcodeReader,
                                         BarcodeFormat? expectedFormat)
      {
         _TestBase = BuildTestBase(testBasePathSuffix);
         _BarcodeReader = barcodeReader;
         _ExpectedFormat = expectedFormat;
         _TestResults = new List<TestResult>();
      }

      protected void AddTest(int mustPassCount, int tryHarderCount, float rotation)
      {
         AddTest(mustPassCount, tryHarderCount, 0, 0, rotation);
      }

      /// <summary>
      /// Adds a new test for the current directory of images.
      ///
      /// <param name="mustPassCount">The number of images which must decode for the test to pass.</param>
      /// <param name="tryHarderCount">The number of images which must pass using the try harder flag.</param>
      /// <param name="maxMisreads">Maximum number of images which can fail due to successfully reading the wrong contents</param>
      /// <param name="maxTryHarderMisreads">Maximum number of images which can fail due to successfully</param>
      ///                             reading the wrong contents using the try harder flag
      /// <param name="rotation">The rotation in degrees clockwise to use for this test.</param>
      /// </summary>
      protected void AddTest(int mustPassCount,
                             int tryHarderCount,
                             int maxMisreads,
                             int maxTryHarderMisreads,
                             float rotation)
      {
         _TestResults.Add(new TestResult(mustPassCount, tryHarderCount, maxMisreads, maxTryHarderMisreads, rotation));
      }

      protected IEnumerable<string> GetImageFiles()
      {
         Log.Info(_TestBase);
         Log.Info(Environment.CurrentDirectory);
         Assert.IsTrue(Directory.Exists(_TestBase), "Please run from the 'core' directory");
         return Directory.EnumerateFiles(_TestBase).Where(p => Accept(_TestBase, p));
      }

      protected IBarCodeDecoder GetReader()
      {
         return _BarcodeReader;
      }

      // This workaround is used because AbstractNegativeBlackBoxTestCase overrides this method but does
      // not return SummaryResults.
      [Test]
      [Ignore("2020-09-03 Fails in BaseLine")]
      public virtual void TestBlackBox()
      {
         Assert.IsFalse(_TestResults.Count == 0);

         IEnumerable<string> imageFiles = GetImageFiles();
         int testCount = _TestResults.Count;

         int[] passedCounts = new int[testCount];
         int[] misreadCounts = new int[testCount];
         int[] tryHarderCounts = new int[testCount];
         int[] tryHarderMisreadCounts = new int[testCount];

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

                string expectedText;
                string expectedTextFile = Path.Combine(Path.GetDirectoryName(absPath), Path.GetFileNameWithoutExtension(absPath) + ".txt");
            if (File.Exists(expectedTextFile))
            {
               expectedText = File.ReadAllText(expectedTextFile, System.Text.Encoding.UTF8);
            }
            else
            {
                    string expectedBinFile = Path.Combine(Path.GetDirectoryName(absPath), Path.GetFileNameWithoutExtension(absPath) + ".bin");
               if (File.Exists(expectedBinFile))
               {
                  // it is only a dirty workaround for some special cases
                  expectedText = File.ReadAllText(expectedBinFile, System.Text.Encoding.GetEncoding("ISO8859-1"));
               }
               else
               {
                  throw new InvalidOperationException("Missing expected result file: " + expectedTextFile);
               }
            }

                string expectedMetadataFile = Path.Combine(Path.GetDirectoryName(absPath), Path.GetFileNameWithoutExtension(absPath) + ".metadata.txt");
            var expectedMetadata = new Dictionary<string, string>();
            if (File.Exists(expectedMetadataFile))
            {
               foreach (var row in File.ReadLines(expectedMetadataFile))
                    {
                        expectedMetadata.Add(row.Split('=')[0], string.Join("=", row.Split('=').Skip(1).ToArray()));
                    }
                }

            for (int x = 0; x < testCount; x++)
            {
               var testResult = _TestResults[x];
               float rotation = testResult.Rotation;
               var rotatedImage = RotateImage(image, rotation);
               LuminanceSource source = new BitmapLuminanceSource(rotatedImage);
               BinaryBitmap bitmap = new BinaryBitmap(new TwoDBinarizer(source));
               try
               {
                  if (Decode(bitmap, rotation, expectedText, expectedMetadata, false))
                  {
                     passedCounts[x]++;
                     Log.Info("   without try-hard ... ok.");
                  }
                  else
                  {
                     misreadCounts[x]++;
                     Log.Info("   without try-hard ... fail.");
                  }
               }
               catch (ReaderException )
               {
                  // continue
                  Log.Info("   without try-hard ... fail (exc).");
               }
               try
               {
                  if (Decode(bitmap, rotation, expectedText, expectedMetadata, true))
                  {
                     tryHarderCounts[x]++;
                     Log.Info("   with try-hard ... ok.");
                  }
                  else
                  {
                     tryHarderMisreadCounts[x]++;
                     Log.Info("   with try-hard ... fail.");
                  }
               }
               catch (ReaderException )
               {
                  // continue
                  Log.Info("   with try-hard ... fail (exc).");
               }
            }
         }

         // Print the results of all tests first
         int totalFound = 0;
         int totalMustPass = 0;
         int totalMisread = 0;
         int totalMaxMisread = 0;
         var imageFilesCount = imageFiles.Count();
         for (int x = 0; x < _TestResults.Count; x++)
         {
            TestResult testResult = _TestResults[x];
            Log.InfoFormat("Rotation {0} degrees:", (int)testResult.Rotation);
            Log.InfoFormat(" {0} of {1} images passed ({2} required)",
                              passedCounts[x], imageFilesCount, testResult.MustPassCount);
            int failed = imageFilesCount - passedCounts[x];
            Log.InfoFormat(" {0} failed due to misreads, {1} not detected",
                              misreadCounts[x], failed - misreadCounts[x]);
            Log.InfoFormat(" {0} of {1} images passed with try harder ({2} required)",
                              tryHarderCounts[x], imageFilesCount, testResult.TryHarderCount);
            failed = imageFilesCount - tryHarderCounts[x];
            Log.InfoFormat(" {0} failed due to misreads, {1} not detected",
                              tryHarderMisreadCounts[x], failed - tryHarderMisreadCounts[x]);
            totalFound += passedCounts[x] + tryHarderCounts[x];
            totalMustPass += testResult.MustPassCount + testResult.TryHarderCount;
            totalMisread += misreadCounts[x] + tryHarderMisreadCounts[x];
            totalMaxMisread += testResult.MaxMisreads + testResult.MaxTryHarderMisreads;
         }

         int totalTests = imageFilesCount * testCount * 2;
         Log.InfoFormat("Decoded {0} images out of {1} ({2}%, {3} required)",
                           totalFound, totalTests, totalFound * 100 / totalTests, totalMustPass);
         if (totalFound > totalMustPass)
         {
            Log.WarnFormat("+++ Test too lax by {0} images", totalFound - totalMustPass);
         }
         else if (totalFound < totalMustPass)
         {
            Log.WarnFormat("--- Test failed by {0} images", totalMustPass - totalFound);
         }

         if (totalMisread < totalMaxMisread)
         {
            Log.WarnFormat("+++ Test expects too many misreads by {0} images", totalMaxMisread - totalMisread);
         }
         else if (totalMisread > totalMaxMisread)
         {
            Log.WarnFormat("--- Test had too many misreads by {0} images", totalMisread - totalMaxMisread);
         }

         // Then run through again and assert if any failed
         for (int x = 0; x < testCount; x++)
         {
            TestResult testResult = _TestResults[x];
                string label = "Rotation " + testResult.Rotation + " degrees: Too many images failed";
            Assert.IsTrue(passedCounts[x] >= testResult.MustPassCount, label);
            Assert.IsTrue(tryHarderCounts[x] >= testResult.TryHarderCount, "Try harder, " + label);
            label = "Rotation " + testResult.Rotation + " degrees: Too many images misread";
            Assert.IsTrue(misreadCounts[x] <= testResult.MaxMisreads, label);
            Assert.IsTrue(tryHarderMisreadCounts[x] <= testResult.MaxTryHarderMisreads, "Try harder, " + label);
         }
      }

      bool Decode(BinaryBitmap source,
                             float rotation,
                             string expectedText,
                             IDictionary<string, string> expectedMetadata,
                             bool tryHarder)
      {

            string suffix = string.Format(" ({0}rotation: {1})", tryHarder ? "try harder, " : "", (int)rotation);

         IDictionary<DecodeHintType, object> hints = new Dictionary<DecodeHintType, object>();
         if (tryHarder)
         {
            hints[DecodeHintType.TRY_HARDER] = true;
         }

         // Try in 'pure' mode mostly to exercise PURE_BARCODE code paths for exceptions;
         // not expected to pass, generally
         BarCodeText result = null;
         try
         {
                var pureHints = new Dictionary<DecodeHintType, object>
                {
                    [DecodeHintType.PURE_BARCODE] = true
                };
                result = _BarcodeReader.Decode(source, pureHints);
         }
         catch (ReaderException )
         {
            // continue
         }

         if (_BarcodeReader is IMultipleBarcodeReader multiReader)
         {
            var expectedResults = expectedText.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            var results = multiReader.DecodeMultiple(source, hints);
            if (results == null) {
                throw new ReaderException();
            }

            if (expectedResults.Length != results.Length)
            {
               Log.InfoFormat("Count mismatch: expected '{0}' results but got '{1}'",
                  expectedResults.Length, results.Length);
               throw new ReaderException();
            }
            foreach (var oneResult in results)
            {
               if (_ExpectedFormat != oneResult.BarcodeFormat)
               {
                  Log.InfoFormat("Format mismatch: expected '{0}' but got '{1}'{2}",
                     _ExpectedFormat, oneResult.BarcodeFormat, suffix);
                  return false;
               }
                    string resultText = oneResult.Text;
               bool found = false;
               foreach (var expectedResult in expectedResults)
               {
                  if (expectedResult.Equals(resultText))
                  {
                     found = true;
                     break;
                  }
               }
               if (!found)
               {
                  Log.InfoFormat("Content was not expected: '{0}'", resultText);
                  return false;
               }
            }
            foreach (var expectedResult in expectedResults)
            {
               bool found = false;
               foreach (var oneResult in results)
               {
                        string resultText = oneResult.Text;
                  if (expectedResult.Equals(resultText))
                  {
                     found = true;
                     break;
                  }
               }
               if (!found)
               {
                  Log.InfoFormat("Content was expected but not found: '{0}'", expectedResult);
                  return false;
               }
            }
         }
         else
         {
            if (result == null) {
                result = _BarcodeReader.Decode(source, hints);
            }
            if (result == null) {
                throw new ReaderException();
            }

            if (_ExpectedFormat != result.BarcodeFormat)
            {
               Log.InfoFormat("Format mismatch: expected '{0}' but got '{1}'{2}",
                  _ExpectedFormat, result.BarcodeFormat, suffix);
               return false;
            }

                string resultText = result.Text;
            if (!expectedText.Equals(resultText))
            {
               Log.InfoFormat("Content mismatch: expected '{0}' but got '{1}'{2}",
                  expectedText, resultText, suffix);
               return false;
            }

            IDictionary<ResultMetadataType, object> resultMetadata = result.ResultMetadata;
            foreach (var metadatum in expectedMetadata)
            {
                Enum.TryParse(metadatum.Key, out ResultMetadataType key);
                    object expectedValue = metadatum.Value;
                    object actualValue = resultMetadata?[key];
               if (!expectedValue.Equals(actualValue))
               {
                  Log.InfoFormat("Metadata mismatch for key '{0}': expected '{1}' but got '{2}'",
                     key, expectedValue, actualValue);
                  return false;
               }
            }
         }

         return true;
      }

#if !SILVERLIGHT
      protected static Bitmap RotateImage(Bitmap original, float degrees)
      {
         if (degrees == 0.0f)
         {
            return original;
         }

         RotateFlipType rotate;
         switch ((int)degrees)
         {
            case 90:
               rotate = RotateFlipType.Rotate90FlipNone;
               break;
            case 180:
               rotate = RotateFlipType.Rotate180FlipNone;
               break;
            case 270:
               rotate = RotateFlipType.Rotate270FlipNone;
               break;
            default:
               throw new NotSupportedException();

         }
         var newRotated = (Bitmap)original.Clone();
         newRotated.RotateFlip(rotate);
         return newRotated;
      }
#else
      protected static WriteableBitmap rotateImage(WriteableBitmap original, float degrees)
      {
         if (degrees == 0.0f)
         {
            return original;
         }

         int width = original.PixelWidth;
         int height = original.PixelHeight;
         int full = Math.Max(width, height);

         Image tempImage2 = new Image();
         tempImage2.Width = full;
         tempImage2.Height = full;
         tempImage2.Source = original;

         // New bitmap has swapped width/height
         WriteableBitmap newRotated = new WriteableBitmap(height, width);


         TransformGroup transformGroup = new TransformGroup();

         // Rotate around centre
         RotateTransform rotate = new RotateTransform();
         rotate.Angle = degrees;
         rotate.CenterX = full / 2;
         rotate.CenterY = full / 2;
         transformGroup.Children.Add(rotate);

         // and transform back to top left corner of new image
         TranslateTransform translate = new TranslateTransform();
         translate.X = -(full - height) / 2;
         translate.Y = -(full - width) / 2;
         transformGroup.Children.Add(translate);

         newRotated.Render(tempImage2, transformGroup);
         newRotated.Invalidate();

         return newRotated;
      }
#endif
   }
}
