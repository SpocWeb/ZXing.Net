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
using System.Drawing;
using System.IO;
using NUnit.Framework;
using ZXing.Common;
using ZXing.Common.Test;

namespace ZXing.Multi.QrCode.Test
{
    /// <summary>
    /// <author>Sean Owen</author>
    /// </summary>
    public sealed class MultiQrCodeBlackBox1TestCase : AbstractBlackBoxTestCase
    {
        public MultiQrCodeBlackBox1TestCase()
            : base("test/data/blackbox/multi-qrcode-1", new QrCodeMultiReader(), BarcodeFormat.QR_CODE)
        {
            AddTest(2, 2, 0.0f);
            AddTest(2, 2, 90.0f);
            AddTest(2, 2, 180.0f);
            AddTest(2, 2, 270.0f);
        }

        [Test]
        public void TestMultiQrCodes()
        {
            var path = BuildTestBase("test/data/blackbox/multi-qrcode-1");
            var source = new BitmapLuminanceSource((Bitmap) Bitmap.FromFile(Path.Combine(path, "1.png")));
            var bitmap = new BinaryBitmap(new TwoDBinarizer(source));

            var reader = new QrCodeMultiReader();
            var results = reader.DecodeMultiple(bitmap);
            Assert.IsNotNull(results);
            Assert.AreEqual(4, results.Length);

            var barcodeContents = new HashSet<string>();
            foreach (BarCodeText result in results)
            {
                barcodeContents.Add(result.Text);
                Assert.AreEqual(BarcodeFormat.QR_CODE, result.BarcodeFormat);
                var metadata = result.ResultMetadata;
                Assert.IsNotNull(metadata);
            }

            var expectedContents = new HashSet<string>
            {
                "You earned the class a 5 MINUTE DANCE PARTY!!  Awesome!  Way to go!  Let's boogie!",
                "You earned the class 5 EXTRA MINUTES OF RECESS!!  Fabulous!!  Way to go!!",
                "You get to SIT AT MRS. SIGMON'S DESK FOR A DAY!!  Awesome!!  Way to go!! Guess I better clean up! :)",
                "You get to CREATE OUR JOURNAL PROMPT FOR THE DAY!  Yay!  Way to go!  "
            };

            foreach (var expected in expectedContents)
            {
                Assert.That(barcodeContents.Contains(expected), Is.True);
            }
        }

        [Test]
        public void TestProcessStructuredAppend()
        {
            var sa1 = new BarCodeText("SA1", new byte[] { }, null, new ResultPoint[] { }, BarcodeFormat.QR_CODE);
            var sa2 = new BarCodeText("SA2", new byte[] { }, null, new ResultPoint[] { }, BarcodeFormat.QR_CODE);
            var sa3 = new BarCodeText("SA3", new byte[] { }, null, new ResultPoint[] { }, BarcodeFormat.QR_CODE);
            sa1.PutMetadata(ResultMetadataType.STRUCTURED_APPEND_SEQUENCE, (0 << 4) + 2);
            sa1.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, "L");
            sa2.PutMetadata(ResultMetadataType.STRUCTURED_APPEND_SEQUENCE, (1 << 4) + 2);
            sa2.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, "L");
            sa3.PutMetadata(ResultMetadataType.STRUCTURED_APPEND_SEQUENCE, (2 << 4) + 2);
            sa3.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, "L");

            var nsa = new BarCodeText("NotSA", new byte[] { }, null, new ResultPoint[] { }, BarcodeFormat.QR_CODE);
            nsa.PutMetadata(ResultMetadataType.ERROR_CORRECTION_LEVEL, "L");

            var inputs = new List<BarCodeText> {sa3, sa1, nsa, sa2};

            var results = inputs.ProcessStructuredAppend();
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(2));

            var barcodeContents = new HashSet<string>();
            foreach (BarCodeText result in results)
            {
                barcodeContents.Add(result.Text);
            }
            var expectedContents = new HashSet<string>
            {
                "NotSA",
                "SA1SA2SA3"
            };
            Assert.That(barcodeContents, Is.EqualTo(expectedContents));
        }
    }
}
