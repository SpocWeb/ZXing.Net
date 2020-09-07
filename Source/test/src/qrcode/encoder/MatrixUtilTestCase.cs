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

using NUnit.Framework;

using ZXing.Common;

namespace ZXing.QrCode.Internal.Test
{
    /// <summary>
    /// <author>satorux@google.com (Satoru Takabayashi) - creator</author>
    /// <author>mysen@google.com (Chris Mysen) - ported from C++</author>
    /// </summary>
    [TestFixture]
   public sealed class MatrixUtilTestCase
   {

      [Test]
      public void TestToString()
      {
         ByteMatrix array = new ByteMatrix(3, 3);
         array.set(0, 0, 0);
         array.set(1, 0, 1);
         array.set(2, 0, 0);
         array.set(0, 1, 1);
         array.set(1, 1, 0);
         array.set(2, 1, 1);
         array.set(0, 2, 2);
         array.set(1, 2, 2);
         array.set(2, 2, 2);
            string expected = " 0 1 0\n" + " 1 0 1\n" + "      \n";
         Assert.AreEqual(expected, array.ToString());
      }

      [Test]
      public void TestClearMatrix()
      {
         ByteMatrix matrix = new ByteMatrix(2, 2);
         MatrixUtil.ClearMatrix(matrix);
         Assert.AreEqual(2, matrix[0, 0]);
         Assert.AreEqual(2, matrix[1, 0]);
         Assert.AreEqual(2, matrix[0, 1]);
         Assert.AreEqual(2, matrix[1, 1]);
      }

      [Test]
      public void TestEmbedBasicPatterns()
      {
         {
                // Version 1.
                string expected =
              " 1 1 1 1 1 1 1 0           0 1 1 1 1 1 1 1\n" +
              " 1 0 0 0 0 0 1 0           0 1 0 0 0 0 0 1\n" +
              " 1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\n" +
              " 1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\n" +
              " 1 0 1 1 1 0 1 0           0 1 0 1 1 1 0 1\n" +
              " 1 0 0 0 0 0 1 0           0 1 0 0 0 0 0 1\n" +
              " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
              " 0 0 0 0 0 0 0 0           0 0 0 0 0 0 0 0\n" +
              "             1                            \n" +
              "             0                            \n" +
              "             1                            \n" +
              "             0                            \n" +
              "             1                            \n" +
              " 0 0 0 0 0 0 0 0 1                        \n" +
              " 1 1 1 1 1 1 1 0                          \n" +
              " 1 0 0 0 0 0 1 0                          \n" +
              " 1 0 1 1 1 0 1 0                          \n" +
              " 1 0 1 1 1 0 1 0                          \n" +
              " 1 0 1 1 1 0 1 0                          \n" +
              " 1 0 0 0 0 0 1 0                          \n" +
              " 1 1 1 1 1 1 1 0                          \n";
            ByteMatrix matrix = new ByteMatrix(21, 21);
            MatrixUtil.ClearMatrix(matrix);
            MatrixUtil.EmbedBasicPatterns(Version.GetVersionForNumber(1), matrix);
            Assert.AreEqual(expected, matrix.ToString());
         }
         {
                // Version 2.  Position adjustment pattern should apppear at right
                // bottom corner.
                string expected =
              " 1 1 1 1 1 1 1 0                   0 1 1 1 1 1 1 1\n" +
              " 1 0 0 0 0 0 1 0                   0 1 0 0 0 0 0 1\n" +
              " 1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\n" +
              " 1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\n" +
              " 1 0 1 1 1 0 1 0                   0 1 0 1 1 1 0 1\n" +
              " 1 0 0 0 0 0 1 0                   0 1 0 0 0 0 0 1\n" +
              " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
              " 0 0 0 0 0 0 0 0                   0 0 0 0 0 0 0 0\n" +
              "             1                                    \n" +
              "             0                                    \n" +
              "             1                                    \n" +
              "             0                                    \n" +
              "             1                                    \n" +
              "             0                                    \n" +
              "             1                                    \n" +
              "             0                                    \n" +
              "             1                   1 1 1 1 1        \n" +
              " 0 0 0 0 0 0 0 0 1               1 0 0 0 1        \n" +
              " 1 1 1 1 1 1 1 0                 1 0 1 0 1        \n" +
              " 1 0 0 0 0 0 1 0                 1 0 0 0 1        \n" +
              " 1 0 1 1 1 0 1 0                 1 1 1 1 1        \n" +
              " 1 0 1 1 1 0 1 0                                  \n" +
              " 1 0 1 1 1 0 1 0                                  \n" +
              " 1 0 0 0 0 0 1 0                                  \n" +
              " 1 1 1 1 1 1 1 0                                  \n";
            ByteMatrix matrix = new ByteMatrix(25, 25);
            MatrixUtil.ClearMatrix(matrix);
            MatrixUtil.EmbedBasicPatterns(Version.GetVersionForNumber(2), matrix);
            Assert.AreEqual(expected, matrix.ToString());
         }
      }

      [Test]
      public void TestEmbedTypeInfo()
      {
            // Type info bits = 100000011001110.
            string expected =
           "                 0                        \n" +
           "                 1                        \n" +
           "                 1                        \n" +
           "                 1                        \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                                          \n" +
           "                 1                        \n" +
           " 1 0 0 0 0 0   0 1         1 1 0 0 1 1 1 0\n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                 0                        \n" +
           "                 1                        \n";
         ByteMatrix matrix = new ByteMatrix(21, 21);
         MatrixUtil.ClearMatrix(matrix);
         MatrixUtil.EmbedTypeInfo(ErrorCorrectionLevel.M, 5, matrix);
         Assert.AreEqual(expected, matrix.ToString());
      }

      [Test]
      public void TestEmbedVersionInfo()
      {
            // Version info bits = 000111 110010 010100
            string expected =
           "                     0 0 1                \n" +
           "                     0 1 0                \n" +
           "                     0 1 0                \n" +
           "                     0 1 1                \n" +
           "                     1 1 1                \n" +
           "                     0 0 0                \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           " 0 0 0 0 1 0                              \n" +
           " 0 1 1 1 1 0                              \n" +
           " 1 0 0 1 1 0                              \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n" +
           "                                          \n";
         // Actually, version 7 QR Code has 45x45 matrix but we use 21x21 here
         // since 45x45 matrix is too big to depict.
         ByteMatrix matrix = new ByteMatrix(21, 21);
         MatrixUtil.ClearMatrix(matrix);
         MatrixUtil.MaybeEmbedVersionInfo(Version.GetVersionForNumber(7), matrix);
         Assert.AreEqual(expected, matrix.ToString());
      }

      [Test]
      public void TestEmbedDataBits()
      {
            // Cells other than basic patterns should be filled with zero.
            string expected =
           " 1 1 1 1 1 1 1 0 0 0 0 0 0 0 1 1 1 1 1 1 1\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 1 0 1 1 1 0 1\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\n" +
           " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
           " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n" +
           " 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0\n";
         BitArray bits = new BitArray();
         ByteMatrix matrix = new ByteMatrix(21, 21);
         MatrixUtil.ClearMatrix(matrix);
         MatrixUtil.EmbedBasicPatterns(Version.GetVersionForNumber(1), matrix);
         MatrixUtil.EmbedDataBits(bits, -1, matrix);
         Assert.AreEqual(expected, matrix.ToString());
      }

      [Test]
      public void TestBuildMatrix()
      {
            // From http://www.swetake.com/qr/qr7.html
            string expected =
           " 1 1 1 1 1 1 1 0 0 1 1 0 0 0 1 1 1 1 1 1 1\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 1 0 0 0 0 0 1\n" +
           " 1 0 1 1 1 0 1 0 0 0 0 1 0 0 1 0 1 1 1 0 1\n" +
           " 1 0 1 1 1 0 1 0 0 1 1 0 0 0 1 0 1 1 1 0 1\n" +
           " 1 0 1 1 1 0 1 0 1 1 0 0 1 0 1 0 1 1 1 0 1\n" +
           " 1 0 0 0 0 0 1 0 0 0 1 1 1 0 1 0 0 0 0 0 1\n" +
           " 1 1 1 1 1 1 1 0 1 0 1 0 1 0 1 1 1 1 1 1 1\n" +
           " 0 0 0 0 0 0 0 0 1 1 0 1 1 0 0 0 0 0 0 0 0\n" +
           " 0 0 1 1 0 0 1 1 1 0 0 1 1 1 1 0 1 0 0 0 0\n" +
           " 1 0 1 0 1 0 0 0 0 0 1 1 1 0 0 1 0 1 1 1 0\n" +
           " 1 1 1 1 0 1 1 0 1 0 1 1 1 0 0 1 1 1 0 1 0\n" +
           " 1 0 1 0 1 1 0 1 1 1 0 0 1 1 1 0 0 1 0 1 0\n" +
           " 0 0 1 0 0 1 1 1 0 0 0 0 0 0 1 0 1 1 1 1 1\n" +
           " 0 0 0 0 0 0 0 0 1 1 0 1 0 0 0 0 0 1 0 1 1\n" +
           " 1 1 1 1 1 1 1 0 1 1 1 1 0 0 0 0 1 0 1 1 0\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 1 0 1 1 1 0 0 0 0 0\n" +
           " 1 0 1 1 1 0 1 0 0 1 0 0 1 1 0 0 1 0 0 1 1\n" +
           " 1 0 1 1 1 0 1 0 1 1 0 1 0 0 0 0 0 1 1 1 0\n" +
           " 1 0 1 1 1 0 1 0 1 1 1 1 0 0 0 0 1 1 1 0 0\n" +
           " 1 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 1 0 1 0 0\n" +
           " 1 1 1 1 1 1 1 0 0 0 1 1 1 1 1 0 1 0 0 1 0\n";
         int[] bytes = {32, 65, 205, 69, 41, 220, 46, 128, 236,
        42, 159, 74, 221, 244, 169, 239, 150, 138,
        70, 237, 85, 224, 96, 74, 219 , 61};
         BitArray bits = new BitArray();
         foreach (char c in bytes)
         {
            bits.AppendBits(c, 8);
         }
         ByteMatrix matrix = new ByteMatrix(21, 21);
         MatrixUtil.BuildMatrix(bits,
                                ErrorCorrectionLevel.H,
                                Version.GetVersionForNumber(1),  // Version 1
                                3,  // Mask pattern 3
                                matrix);
         Assert.AreEqual(expected, matrix.ToString());
      }

      [Test]
      public void TestFindMsbSet()
      {
         Assert.AreEqual(0, MatrixUtil.FindMsbSet(0));
         Assert.AreEqual(1, MatrixUtil.FindMsbSet(1));
         Assert.AreEqual(8, MatrixUtil.FindMsbSet(0x80));
         Assert.AreEqual(32, MatrixUtil.FindMsbSet(-2147483648 /*0x80000000*/));
      }

      [Test]
      public void TestCalculateBchCode()
      {
         // Encoding of type information.
         // From Appendix C in JISX0510:2004 (p 65)
         Assert.AreEqual(0xdc, MatrixUtil.CalculateBchCode(5, 0x537));
         // From http://www.swetake.com/qr/qr6.html
         Assert.AreEqual(0x1c2, MatrixUtil.CalculateBchCode(0x13, 0x537));
         // From http://www.swetake.com/qr/qr11.html
         Assert.AreEqual(0x214, MatrixUtil.CalculateBchCode(0x1b, 0x537));

         // Encoding of version information.
         // From Appendix D in JISX0510:2004 (p 68)
         Assert.AreEqual(0xc94, MatrixUtil.CalculateBchCode(7, 0x1f25));
         Assert.AreEqual(0x5bc, MatrixUtil.CalculateBchCode(8, 0x1f25));
         Assert.AreEqual(0xa99, MatrixUtil.CalculateBchCode(9, 0x1f25));
         Assert.AreEqual(0x4d3, MatrixUtil.CalculateBchCode(10, 0x1f25));
         Assert.AreEqual(0x9a6, MatrixUtil.CalculateBchCode(20, 0x1f25));
         Assert.AreEqual(0xd75, MatrixUtil.CalculateBchCode(30, 0x1f25));
         Assert.AreEqual(0xc69, MatrixUtil.CalculateBchCode(40, 0x1f25));
      }

      // We don't test a lot of cases in this function since we've already
      // tested them in TEST(calculateBCHCode).
      [Test]
      public void TestMakeVersionInfoBits()
      {
         // From Appendix D in JISX0510:2004 (p 68)
         BitArray bits = new BitArray();
         MatrixUtil.MakeVersionInfoBits(Version.GetVersionForNumber(7), bits);
         Assert.AreEqual(" ...XXXXX ..X..X.X ..", bits.ToString());
      }

      // We don't test a lot of cases in this function since we've already
      // tested them in TEST(calculateBCHCode).
      [Test]
      public void TestMakeTypeInfoInfoBits()
      {
         // From Appendix C in JISX0510:2004 (p 65)
         BitArray bits = new BitArray();
         MatrixUtil.MakeTypeInfoBits(ErrorCorrectionLevel.M, 5, bits);
         Assert.AreEqual(" X......X X..XXX.", bits.ToString());
      }
   }
}