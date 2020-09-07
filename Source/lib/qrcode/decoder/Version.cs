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

using System;
using ZXing.Common;

namespace ZXing.QrCode.Internal
{
    /// <summary>
    /// See ISO 18004:2006 Annex D
    /// </summary>
    /// <author>Sean Owen</author>
    public sealed class Version
    {
        /// <summary> See ISO 18004:2006 Annex D.
        /// Element i represents the raw version bits that specify version i + 7
        /// </summary>
        private static readonly int[] VERSION_DECODE_INFO = {
                                                                0x07C94, 0x085BC, 0x09A99, 0x0A4D3, 0x0BBF6,
                                                                0x0C762, 0x0D847, 0x0E60D, 0x0F928, 0x10B78,
                                                                0x1145D, 0x12A17, 0x13532, 0x149A6, 0x15683,
                                                                0x168C9, 0x177EC, 0x18EC4, 0x191E1, 0x1AFAB,
                                                                0x1B08E, 0x1CC1A, 0x1D33F, 0x1ED75, 0x1F250,
                                                                0x209D5, 0x216F0, 0x228BA, 0x2379F, 0x24B0B,
                                                                0x2542E, 0x26A64, 0x27541, 0x28C69
                                                             };

        private static readonly Version[] VERSIONS = BuildVersions();

        private readonly EcBlocks[] _EcBlocks;

        private Version(int versionNumber, int[] alignmentPatternCenters, params EcBlocks[] ecBlocks)
        {
            this.VersionNumber = versionNumber;
            this.AlignmentPatternCenters = alignmentPatternCenters;
            this._EcBlocks = ecBlocks;
            int total = 0;
            int ecCodewords = ecBlocks[0].EcCodewordsPerBlock;
            Ecb[] ecbArray = ecBlocks[0].GetEcBlocks();
            foreach (var ecBlock in ecbArray)
            {
                total += ecBlock.Count * (ecBlock.DataCodewords + ecCodewords);
            }
            TotalCodewords = total;
        }

        /// <summary>
        /// Gets the version number.
        /// </summary>
        public int VersionNumber { get; }

        /// <summary>
        /// Gets the alignment pattern centers. </summary>
        public int[] AlignmentPatternCenters { get; }

        /// <summary>
        /// Gets the total codewords.
        /// </summary>
        public int TotalCodewords { get; }

        /// <summary>
        /// Gets the dimension for version.
        /// </summary>
        public int DimensionForVersion => 17 + 4 * VersionNumber;

        /// <summary>
        /// Gets the EC blocks for level.
        /// </summary>
        /// <param name="ecLevel">The ec level.</param>
        /// <returns></returns>
        public EcBlocks GetEcBlocksForLevel(ErrorCorrectionLevel ecLevel)
        {
            return _EcBlocks[ecLevel.ordinal()];
        }

        /// <summary> <p>Deduces version information purely from QR Code dimensions.</p>
        /// 
        /// </summary>
        /// <param name="dimension">dimension in modules
        /// </param>
        /// <returns><see cref="Version" /> for a QR Code of that dimension or null</returns>
        public static Version GetProvisionalVersionForDimension(int dimension)
        {
            if (dimension % 4 != 1)
            {
                return null;
            }
            try
            {
                return GetVersionForNumber((dimension - 17) >> 2);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the version for number.
        /// </summary>
        /// <param name="versionNumber">The version number.</param>
        /// <returns></returns>
        public static Version GetVersionForNumber(int versionNumber)
        {
            if (versionNumber < 1 || versionNumber > 40)
            {
                throw new ArgumentException();
            }
            return VERSIONS[versionNumber - 1];
        }

        public static Version DecodeVersionInformation(int versionBits)
        {
            int bestDifference = int.MaxValue;
            int bestVersion = 0;
            for (int i = 0; i < VERSION_DECODE_INFO.Length; i++)
            {
                int targetVersion = VERSION_DECODE_INFO[i];
                // Do the version info bits match exactly? done.
                if (targetVersion == versionBits)
                {
                    return GetVersionForNumber(i + 7);
                }
                // Otherwise see if this is the closest to a real version info bit string
                // we have seen so far
                int bitsDifference = FormatInformation.NumBitsDiffering(versionBits, targetVersion);
                if (bitsDifference < bestDifference)
                {
                    bestVersion = i + 7;
                    bestDifference = bitsDifference;
                }
            }
            // We can tolerate up to 3 bits of error since no two version info codewords will
            // differ in less than 8 bits.
            if (bestDifference <= 3)
            {
                return GetVersionForNumber(bestVersion);
            }
            // If we didn't find a close enough match, fail
            return null;
        }

        /// <summary> See ISO 18004:2006 Annex E</summary>
        public BitMatrix BuildFunctionPattern()
        {
            int dimension = DimensionForVersion;
            BitMatrix bitMatrix = new BitMatrix(dimension);

            // Top left finder pattern + separator + format
            bitMatrix.SetRegion(0, 0, 9, 9);
            // Top right finder pattern + separator + format
            bitMatrix.SetRegion(dimension - 8, 0, 8, 9);
            // Bottom left finder pattern + separator + format
            bitMatrix.SetRegion(0, dimension - 8, 9, 8);

            // Alignment patterns
            int max = AlignmentPatternCenters.Length;
            for (int x = 0; x < max; x++)
            {
                int i = AlignmentPatternCenters[x] - 2;
                for (int y = 0; y < max; y++)
                {
                    if ((x != 0 || y != 0 && y != max - 1) && (x != max - 1 || y != 0))
                    {
                        bitMatrix.SetRegion(AlignmentPatternCenters[y] - 2, i, 5, 5);
                    }
                    // else no o alignment patterns near the three finder patterns
                }
            }

            // Vertical timing pattern
            bitMatrix.SetRegion(6, 9, 1, dimension - 17);
            // Horizontal timing pattern
            bitMatrix.SetRegion(9, 6, dimension - 17, 1);

            if (VersionNumber > 6)
            {
                // Version info, top right
                bitMatrix.SetRegion(dimension - 11, 0, 3, 6);
                // Version info, bottom left
                bitMatrix.SetRegion(0, dimension - 11, 6, 3);
            }

            return bitMatrix;
        }

        /// <summary> <p>Encapsulates a set of error-correction blocks in one symbol version. Most versions will
        /// use blocks of differing sizes within one version, so, this encapsulates the parameters for
        /// each set of blocks. It also holds the number of error-correction codewords per block since it
        /// will be the same across all blocks within one version.</p>
        /// </summary>
        public sealed class EcBlocks
        {

            private readonly Ecb[] _EcBlocks;

            internal EcBlocks(int ecCodewordsPerBlock, params Ecb[] ecBlocks)
            {
                this.EcCodewordsPerBlock = ecCodewordsPerBlock;
                this._EcBlocks = ecBlocks;
            }

            /// <summary>
            /// Gets the EC codewords per block.
            /// </summary>
            public int EcCodewordsPerBlock { get; }

            /// <summary>
            /// Gets the num blocks.
            /// </summary>
            public int NumBlocks
            {
                get
                {
                    int total = 0;
                    foreach (var ecBlock in _EcBlocks)
                    {
                        total += ecBlock.Count;
                    }
                    return total;
                }
            }

            /// <summary>
            /// Gets the total EC codewords.
            /// </summary>
            public int TotalEcCodewords => EcCodewordsPerBlock * NumBlocks;

            /// <summary>
            /// Gets the EC blocks.
            /// </summary>
            /// <returns></returns>
            public Ecb[] GetEcBlocks()
            {
                return _EcBlocks;
            }
        }

        /// <summary> <p>Encapsulates the parameters for one error-correction block in one symbol version.
        /// This includes the number of data codewords, and the number of times a block with these
        /// parameters is used consecutively in the QR code version's format.</p>
        /// </summary>
        public sealed class Ecb
        {

            internal Ecb(int count, int dataCodewords)
            {
                this.Count = count;
                this.DataCodewords = dataCodewords;
            }

            /// <summary>
            /// Gets the count.
            /// </summary>
            public int Count { get; }

            /// <summary>
            /// Gets the data codewords.
            /// </summary>
            public int DataCodewords { get; }

        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Convert.ToString(VersionNumber);
        }

        /// <summary> See ISO 18004:2006 6.5.1 Table 9</summary>
        private static Version[] BuildVersions()
        {
            return new[]
               {
               new Version(1, new int[] {},
                           new EcBlocks(7, new Ecb(1, 19)),
                           new EcBlocks(10, new Ecb(1, 16)),
                           new EcBlocks(13, new Ecb(1, 13)),
                           new EcBlocks(17, new Ecb(1, 9))),
               new Version(2, new[] {6, 18},
                           new EcBlocks(10, new Ecb(1, 34)),
                           new EcBlocks(16, new Ecb(1, 28)),
                           new EcBlocks(22, new Ecb(1, 22)),
                           new EcBlocks(28, new Ecb(1, 16))),
               new Version(3, new[] {6, 22},
                           new EcBlocks(15, new Ecb(1, 55)),
                           new EcBlocks(26, new Ecb(1, 44)),
                           new EcBlocks(18, new Ecb(2, 17)),
                           new EcBlocks(22, new Ecb(2, 13))),
               new Version(4, new[] {6, 26},
                           new EcBlocks(20, new Ecb(1, 80)),
                           new EcBlocks(18, new Ecb(2, 32)),
                           new EcBlocks(26, new Ecb(2, 24)),
                           new EcBlocks(16, new Ecb(4, 9))),
               new Version(5, new[] {6, 30},
                           new EcBlocks(26, new Ecb(1, 108)),
                           new EcBlocks(24, new Ecb(2, 43)),
                           new EcBlocks(18, new Ecb(2, 15),
                                        new Ecb(2, 16)),
                           new EcBlocks(22, new Ecb(2, 11),
                                        new Ecb(2, 12))),
               new Version(6, new[] {6, 34},
                           new EcBlocks(18, new Ecb(2, 68)),
                           new EcBlocks(16, new Ecb(4, 27)),
                           new EcBlocks(24, new Ecb(4, 19)),
                           new EcBlocks(28, new Ecb(4, 15))),
               new Version(7, new[] {6, 22, 38},
                           new EcBlocks(20, new Ecb(2, 78)),
                           new EcBlocks(18, new Ecb(4, 31)),
                           new EcBlocks(18, new Ecb(2, 14),
                                        new Ecb(4, 15)),
                           new EcBlocks(26, new Ecb(4, 13),
                                        new Ecb(1, 14))),
               new Version(8, new[] {6, 24, 42},
                           new EcBlocks(24, new Ecb(2, 97)),
                           new EcBlocks(22, new Ecb(2, 38),
                                        new Ecb(2, 39)),
                           new EcBlocks(22, new Ecb(4, 18),
                                        new Ecb(2, 19)),
                           new EcBlocks(26, new Ecb(4, 14),
                                        new Ecb(2, 15))),
               new Version(9, new[] {6, 26, 46},
                           new EcBlocks(30, new Ecb(2, 116)),
                           new EcBlocks(22, new Ecb(3, 36),
                                        new Ecb(2, 37)),
                           new EcBlocks(20, new Ecb(4, 16),
                                        new Ecb(4, 17)),
                           new EcBlocks(24, new Ecb(4, 12),
                                        new Ecb(4, 13))),
               new Version(10, new[] {6, 28, 50},
                           new EcBlocks(18, new Ecb(2, 68),
                                        new Ecb(2, 69)),
                           new EcBlocks(26, new Ecb(4, 43),
                                        new Ecb(1, 44)),
                           new EcBlocks(24, new Ecb(6, 19),
                                        new Ecb(2, 20)),
                           new EcBlocks(28, new Ecb(6, 15),
                                        new Ecb(2, 16))),
               new Version(11, new[] {6, 30, 54},
                           new EcBlocks(20, new Ecb(4, 81)),
                           new EcBlocks(30, new Ecb(1, 50),
                                        new Ecb(4, 51)),
                           new EcBlocks(28, new Ecb(4, 22),
                                        new Ecb(4, 23)),
                           new EcBlocks(24, new Ecb(3, 12),
                                        new Ecb(8, 13))),
               new Version(12, new[] {6, 32, 58},
                           new EcBlocks(24, new Ecb(2, 92),
                                        new Ecb(2, 93)),
                           new EcBlocks(22, new Ecb(6, 36),
                                        new Ecb(2, 37)),
                           new EcBlocks(26, new Ecb(4, 20),
                                        new Ecb(6, 21)),
                           new EcBlocks(28, new Ecb(7, 14),
                                        new Ecb(4, 15))),
               new Version(13, new[] {6, 34, 62},
                           new EcBlocks(26, new Ecb(4, 107)),
                           new EcBlocks(22, new Ecb(8, 37),
                                        new Ecb(1, 38)),
                           new EcBlocks(24, new Ecb(8, 20),
                                        new Ecb(4, 21)),
                           new EcBlocks(22, new Ecb(12, 11),
                                        new Ecb(4, 12))),
               new Version(14, new[] {6, 26, 46, 66},
                           new EcBlocks(30, new Ecb(3, 115),
                                        new Ecb(1, 116)),
                           new EcBlocks(24, new Ecb(4, 40),
                                        new Ecb(5, 41)),
                           new EcBlocks(20, new Ecb(11, 16),
                                        new Ecb(5, 17)),
                           new EcBlocks(24, new Ecb(11, 12),
                                        new Ecb(5, 13))),
               new Version(15, new[] {6, 26, 48, 70},
                           new EcBlocks(22, new Ecb(5, 87),
                                        new Ecb(1, 88)),
                           new EcBlocks(24, new Ecb(5, 41),
                                        new Ecb(5, 42)),
                           new EcBlocks(30, new Ecb(5, 24),
                                        new Ecb(7, 25)),
                           new EcBlocks(24, new Ecb(11, 12),
                                        new Ecb(7, 13))),
               new Version(16, new[] {6, 26, 50, 74},
                           new EcBlocks(24, new Ecb(5, 98),
                                        new Ecb(1, 99)),
                           new EcBlocks(28, new Ecb(7, 45),
                                        new Ecb(3, 46)),
                           new EcBlocks(24, new Ecb(15, 19),
                                        new Ecb(2, 20)),
                           new EcBlocks(30, new Ecb(3, 15),
                                        new Ecb(13, 16))),
               new Version(17, new[] {6, 30, 54, 78},
                           new EcBlocks(28, new Ecb(1, 107),
                                        new Ecb(5, 108)),
                           new EcBlocks(28, new Ecb(10, 46),
                                        new Ecb(1, 47)),
                           new EcBlocks(28, new Ecb(1, 22),
                                        new Ecb(15, 23)),
                           new EcBlocks(28, new Ecb(2, 14),
                                        new Ecb(17, 15))),
               new Version(18, new[] {6, 30, 56, 82},
                           new EcBlocks(30, new Ecb(5, 120),
                                        new Ecb(1, 121)),
                           new EcBlocks(26, new Ecb(9, 43),
                                        new Ecb(4, 44)),
                           new EcBlocks(28, new Ecb(17, 22),
                                        new Ecb(1, 23)),
                           new EcBlocks(28, new Ecb(2, 14),
                                        new Ecb(19, 15))),
               new Version(19, new[] {6, 30, 58, 86},
                           new EcBlocks(28, new Ecb(3, 113),
                                        new Ecb(4, 114)),
                           new EcBlocks(26, new Ecb(3, 44),
                                        new Ecb(11, 45)),
                           new EcBlocks(26, new Ecb(17, 21),
                                        new Ecb(4, 22)),
                           new EcBlocks(26, new Ecb(9, 13),
                                        new Ecb(16, 14))),
               new Version(20, new[] {6, 34, 62, 90},
                           new EcBlocks(28, new Ecb(3, 107),
                                        new Ecb(5, 108)),
                           new EcBlocks(26, new Ecb(3, 41),
                                        new Ecb(13, 42)),
                           new EcBlocks(30, new Ecb(15, 24),
                                        new Ecb(5, 25)),
                           new EcBlocks(28, new Ecb(15, 15),
                                        new Ecb(10, 16))),
               new Version(21, new[] {6, 28, 50, 72, 94},
                           new EcBlocks(28, new Ecb(4, 116),
                                        new Ecb(4, 117)),
                           new EcBlocks(26, new Ecb(17, 42)),
                           new EcBlocks(28, new Ecb(17, 22),
                                        new Ecb(6, 23)),
                           new EcBlocks(30, new Ecb(19, 16),
                                        new Ecb(6, 17))),
               new Version(22, new[] {6, 26, 50, 74, 98},
                           new EcBlocks(28, new Ecb(2, 111),
                                        new Ecb(7, 112)),
                           new EcBlocks(28, new Ecb(17, 46)),
                           new EcBlocks(30, new Ecb(7, 24),
                                        new Ecb(16, 25)),
                           new EcBlocks(24, new Ecb(34, 13))),
               new Version(23, new[] {6, 30, 54, 78, 102},
                           new EcBlocks(30, new Ecb(4, 121),
                                        new Ecb(5, 122)),
                           new EcBlocks(28, new Ecb(4, 47),
                                        new Ecb(14, 48)),
                           new EcBlocks(30, new Ecb(11, 24),
                                        new Ecb(14, 25)),
                           new EcBlocks(30, new Ecb(16, 15),
                                        new Ecb(14, 16))),
               new Version(24, new[] {6, 28, 54, 80, 106},
                           new EcBlocks(30, new Ecb(6, 117),
                                        new Ecb(4, 118)),
                           new EcBlocks(28, new Ecb(6, 45),
                                        new Ecb(14, 46)),
                           new EcBlocks(30, new Ecb(11, 24),
                                        new Ecb(16, 25)),
                           new EcBlocks(30, new Ecb(30, 16),
                                        new Ecb(2, 17))),
               new Version(25, new[] {6, 32, 58, 84, 110},
                           new EcBlocks(26, new Ecb(8, 106),
                                        new Ecb(4, 107)),
                           new EcBlocks(28, new Ecb(8, 47),
                                        new Ecb(13, 48)),
                           new EcBlocks(30, new Ecb(7, 24),
                                        new Ecb(22, 25)),
                           new EcBlocks(30, new Ecb(22, 15),
                                        new Ecb(13, 16))),
               new Version(26, new[] {6, 30, 58, 86, 114},
                           new EcBlocks(28, new Ecb(10, 114),
                                        new Ecb(2, 115)),
                           new EcBlocks(28, new Ecb(19, 46),
                                        new Ecb(4, 47)),
                           new EcBlocks(28, new Ecb(28, 22),
                                        new Ecb(6, 23)),
                           new EcBlocks(30, new Ecb(33, 16),
                                        new Ecb(4, 17))),
               new Version(27, new[] {6, 34, 62, 90, 118},
                           new EcBlocks(30, new Ecb(8, 122),
                                        new Ecb(4, 123)),
                           new EcBlocks(28, new Ecb(22, 45),
                                        new Ecb(3, 46)),
                           new EcBlocks(30, new Ecb(8, 23),
                                        new Ecb(26, 24)),
                           new EcBlocks(30, new Ecb(12, 15),
                                        new Ecb(28, 16))),
               new Version(28, new[] {6, 26, 50, 74, 98, 122},
                           new EcBlocks(30, new Ecb(3, 117),
                                        new Ecb(10, 118)),
                           new EcBlocks(28, new Ecb(3, 45),
                                        new Ecb(23, 46)),
                           new EcBlocks(30, new Ecb(4, 24),
                                        new Ecb(31, 25)),
                           new EcBlocks(30, new Ecb(11, 15),
                                        new Ecb(31, 16))),
               new Version(29, new[] {6, 30, 54, 78, 102, 126},
                           new EcBlocks(30, new Ecb(7, 116),
                                        new Ecb(7, 117)),
                           new EcBlocks(28, new Ecb(21, 45),
                                        new Ecb(7, 46)),
                           new EcBlocks(30, new Ecb(1, 23),
                                        new Ecb(37, 24)),
                           new EcBlocks(30, new Ecb(19, 15),
                                        new Ecb(26, 16))),
               new Version(30, new[] {6, 26, 52, 78, 104, 130},
                           new EcBlocks(30, new Ecb(5, 115),
                                        new Ecb(10, 116)),
                           new EcBlocks(28, new Ecb(19, 47),
                                        new Ecb(10, 48)),
                           new EcBlocks(30, new Ecb(15, 24),
                                        new Ecb(25, 25)),
                           new EcBlocks(30, new Ecb(23, 15),
                                        new Ecb(25, 16))),
               new Version(31, new[] {6, 30, 56, 82, 108, 134},
                           new EcBlocks(30, new Ecb(13, 115),
                                        new Ecb(3, 116)),
                           new EcBlocks(28, new Ecb(2, 46),
                                        new Ecb(29, 47)),
                           new EcBlocks(30, new Ecb(42, 24),
                                        new Ecb(1, 25)),
                           new EcBlocks(30, new Ecb(23, 15),
                                        new Ecb(28, 16))),
               new Version(32, new[] {6, 34, 60, 86, 112, 138},
                           new EcBlocks(30, new Ecb(17, 115)),
                           new EcBlocks(28, new Ecb(10, 46),
                                        new Ecb(23, 47)),
                           new EcBlocks(30, new Ecb(10, 24),
                                        new Ecb(35, 25)),
                           new EcBlocks(30, new Ecb(19, 15),
                                        new Ecb(35, 16))),
               new Version(33, new[] {6, 30, 58, 86, 114, 142},
                           new EcBlocks(30, new Ecb(17, 115),
                                        new Ecb(1, 116)),
                           new EcBlocks(28, new Ecb(14, 46),
                                        new Ecb(21, 47)),
                           new EcBlocks(30, new Ecb(29, 24),
                                        new Ecb(19, 25)),
                           new EcBlocks(30, new Ecb(11, 15),
                                        new Ecb(46, 16))),
               new Version(34, new[] {6, 34, 62, 90, 118, 146},
                           new EcBlocks(30, new Ecb(13, 115),
                                        new Ecb(6, 116)),
                           new EcBlocks(28, new Ecb(14, 46),
                                        new Ecb(23, 47)),
                           new EcBlocks(30, new Ecb(44, 24),
                                        new Ecb(7, 25)),
                           new EcBlocks(30, new Ecb(59, 16),
                                        new Ecb(1, 17))),
               new Version(35, new[] {6, 30, 54, 78, 102, 126, 150},
                           new EcBlocks(30, new Ecb(12, 121),
                                        new Ecb(7, 122)),
                           new EcBlocks(28, new Ecb(12, 47),
                                        new Ecb(26, 48)),
                           new EcBlocks(30, new Ecb(39, 24),
                                        new Ecb(14, 25)),
                           new EcBlocks(30, new Ecb(22, 15),
                                        new Ecb(41, 16))),
               new Version(36, new[] {6, 24, 50, 76, 102, 128, 154},
                           new EcBlocks(30, new Ecb(6, 121),
                                        new Ecb(14, 122)),
                           new EcBlocks(28, new Ecb(6, 47),
                                        new Ecb(34, 48)),
                           new EcBlocks(30, new Ecb(46, 24),
                                        new Ecb(10, 25)),
                           new EcBlocks(30, new Ecb(2, 15),
                                        new Ecb(64, 16))),
               new Version(37, new[] {6, 28, 54, 80, 106, 132, 158},
                           new EcBlocks(30, new Ecb(17, 122),
                                        new Ecb(4, 123)),
                           new EcBlocks(28, new Ecb(29, 46),
                                        new Ecb(14, 47)),
                           new EcBlocks(30, new Ecb(49, 24),
                                        new Ecb(10, 25)),
                           new EcBlocks(30, new Ecb(24, 15),
                                        new Ecb(46, 16))),
               new Version(38, new[] {6, 32, 58, 84, 110, 136, 162},
                           new EcBlocks(30, new Ecb(4, 122),
                                        new Ecb(18, 123)),
                           new EcBlocks(28, new Ecb(13, 46),
                                        new Ecb(32, 47)),
                           new EcBlocks(30, new Ecb(48, 24),
                                        new Ecb(14, 25)),
                           new EcBlocks(30, new Ecb(42, 15),
                                        new Ecb(32, 16))),
               new Version(39, new[] {6, 26, 54, 82, 110, 138, 166},
                           new EcBlocks(30, new Ecb(20, 117),
                                        new Ecb(4, 118)),
                           new EcBlocks(28, new Ecb(40, 47),
                                        new Ecb(7, 48)),
                           new EcBlocks(30, new Ecb(43, 24),
                                        new Ecb(22, 25)),
                           new EcBlocks(30, new Ecb(10, 15),
                                        new Ecb(67, 16))),
               new Version(40, new[] {6, 30, 58, 86, 114, 142, 170},
                           new EcBlocks(30, new Ecb(19, 118),
                                        new Ecb(6, 119)),
                           new EcBlocks(28, new Ecb(18, 47),
                                        new Ecb(31, 48)),
                           new EcBlocks(30, new Ecb(34, 24),
                                        new Ecb(34, 25)),
                           new EcBlocks(30, new Ecb(20, 15),
                                        new Ecb(61, 16)))
               };
        }
    }
}