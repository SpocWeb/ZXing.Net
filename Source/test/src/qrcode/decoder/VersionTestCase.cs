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

using NUnit.Framework;

namespace ZXing.QrCode.Internal.Test
{
    /// <summary>
    /// <author>Sean Owen</author>
    /// </summary>
    [TestFixture]
    public sealed class VersionTestCase
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestBadVersion()
        {
            Version.GetVersionForNumber(0);
        }

        [Test]
        public void TestVersionForNumber()
        {
            for (int i = 1; i <= 40; i++)
            {
                CheckVersion(Version.GetVersionForNumber(i), i, 4 * i + 17);
            }
        }

        private static void CheckVersion(Version version, int number, int dimension)
        {
            Assert.IsNotNull(version);
            Assert.AreEqual(number, version.VersionNumber);
            Assert.IsNotNull(version.AlignmentPatternCenters);
            if (number > 1)
            {
                Assert.IsTrue(version.AlignmentPatternCenters.Length > 0);
            }
            Assert.AreEqual(dimension, version.DimensionForVersion);
            Assert.IsNotNull(version.GetEcBlocksForLevel(ErrorCorrectionLevel.H));
            Assert.IsNotNull(version.GetEcBlocksForLevel(ErrorCorrectionLevel.L));
            Assert.IsNotNull(version.GetEcBlocksForLevel(ErrorCorrectionLevel.M));
            Assert.IsNotNull(version.GetEcBlocksForLevel(ErrorCorrectionLevel.Q));
            Assert.IsNotNull(version.BuildFunctionPattern());
        }

        [Test]
        public void TestGetProvisionalVersionForDimension()
        {
            for (int i = 1; i <= 40; i++)
            {
                Assert.AreEqual(i, Version.GetProvisionalVersionForDimension(4 * i + 17).VersionNumber);
            }
        }

        [Test]
        public void TestDecodeVersionInformation()
        {
            // Spot check
            DoTestVersion(7, 0x07C94);
            DoTestVersion(12, 0x0C762);
            DoTestVersion(17, 0x1145D);
            DoTestVersion(22, 0x168C9);
            DoTestVersion(27, 0x1B08E);
            DoTestVersion(32, 0x209D5);
        }

        private static void DoTestVersion(int expectedVersion, int mask)
        {
            Version version = Version.DecodeVersionInformation(mask);
            Assert.IsNotNull(version);
            Assert.AreEqual(expectedVersion, version.VersionNumber);
        }
    }
}