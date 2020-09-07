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

namespace ZXing.QrCode.Internal.Test
{
   /// <summary>
   /// <author>satorux@google.com (Satoru Takabayashi) - creator</author>
   /// <author>mysen@google.com (Chris Mysen) - ported from C++</author>
   /// </summary>
   [TestFixture]
   public sealed class MaskUtilTestCase
   {
      [Test]
      public void TestApplyMaskPenaltyRule1()
      {
         var matrix = new ByteMatrix(4, 1);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(2, 0, 0);
         matrix.Set(3, 0, 0);
         Assert.AreEqual(0, MaskUtil.applyMaskPenaltyRule1(matrix));
         // Horizontal.
         matrix = new ByteMatrix(6, 1);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(2, 0, 0);
         matrix.Set(3, 0, 0);
         matrix.Set(4, 0, 0);
         matrix.Set(5, 0, 1);
         Assert.AreEqual(3, MaskUtil.applyMaskPenaltyRule1(matrix));
         matrix.Set(5, 0, 0);
         Assert.AreEqual(4, MaskUtil.applyMaskPenaltyRule1(matrix));
         // Vertical.
         matrix = new ByteMatrix(1, 6);
         matrix.Set(0, 0, 0);
         matrix.Set(0, 1, 0);
         matrix.Set(0, 2, 0);
         matrix.Set(0, 3, 0);
         matrix.Set(0, 4, 0);
         matrix.Set(0, 5, 1);
         Assert.AreEqual(3, MaskUtil.applyMaskPenaltyRule1(matrix));
         matrix.Set(0, 5, 0);
         Assert.AreEqual(4, MaskUtil.applyMaskPenaltyRule1(matrix));
      }

      [Test]
      public void TestApplyMaskPenaltyRule2()
      {
         var matrix = new ByteMatrix(1, 1);
         matrix.Set(0, 0, 0);
         Assert.AreEqual(0, MaskUtil.applyMaskPenaltyRule2(matrix));
         matrix = new ByteMatrix(2, 2);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(0, 1, 0);
         matrix.Set(1, 1, 1);
         Assert.AreEqual(0, MaskUtil.applyMaskPenaltyRule2(matrix));
         matrix = new ByteMatrix(2, 2);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(0, 1, 0);
         matrix.Set(1, 1, 0);
         Assert.AreEqual(3, MaskUtil.applyMaskPenaltyRule2(matrix));
         matrix = new ByteMatrix(3, 3);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(2, 0, 0);
         matrix.Set(0, 1, 0);
         matrix.Set(1, 1, 0);
         matrix.Set(2, 1, 0);
         matrix.Set(0, 2, 0);
         matrix.Set(1, 2, 0);
         matrix.Set(2, 2, 0);
         // Four instances of 2x2 blocks.
         Assert.AreEqual(3*4, MaskUtil.applyMaskPenaltyRule2(matrix));
      }

      [Test]
      public void TestApplyMaskPenaltyRule3()
      {
         // Horizontal 00001011101.
         var matrix = new ByteMatrix(11, 1);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 0);
         matrix.Set(2, 0, 0);
         matrix.Set(3, 0, 0);
         matrix.Set(4, 0, 1);
         matrix.Set(5, 0, 0);
         matrix.Set(6, 0, 1);
         matrix.Set(7, 0, 1);
         matrix.Set(8, 0, 1);
         matrix.Set(9, 0, 0);
         matrix.Set(10, 0, 1);
         Assert.AreEqual(40, MaskUtil.applyMaskPenaltyRule3(matrix));
         // Horizontal 10111010000.
         matrix = new ByteMatrix(11, 1);
         matrix.Set(0, 0, 1);
         matrix.Set(1, 0, 0);
         matrix.Set(2, 0, 1);
         matrix.Set(3, 0, 1);
         matrix.Set(4, 0, 1);
         matrix.Set(5, 0, 0);
         matrix.Set(6, 0, 1);
         matrix.Set(7, 0, 0);
         matrix.Set(8, 0, 0);
         matrix.Set(9, 0, 0);
         matrix.Set(10, 0, 0);
         Assert.AreEqual(40, MaskUtil.applyMaskPenaltyRule3(matrix));
         // Vertical 00001011101.
         matrix = new ByteMatrix(1, 11);
         matrix.Set(0, 0, 0);
         matrix.Set(0, 1, 0);
         matrix.Set(0, 2, 0);
         matrix.Set(0, 3, 0);
         matrix.Set(0, 4, 1);
         matrix.Set(0, 5, 0);
         matrix.Set(0, 6, 1);
         matrix.Set(0, 7, 1);
         matrix.Set(0, 8, 1);
         matrix.Set(0, 9, 0);
         matrix.Set(0, 10, 1);
         Assert.AreEqual(40, MaskUtil.applyMaskPenaltyRule3(matrix));
         // Vertical 10111010000.
         matrix = new ByteMatrix(1, 11);
         matrix.Set(0, 0, 1);
         matrix.Set(0, 1, 0);
         matrix.Set(0, 2, 1);
         matrix.Set(0, 3, 1);
         matrix.Set(0, 4, 1);
         matrix.Set(0, 5, 0);
         matrix.Set(0, 6, 1);
         matrix.Set(0, 7, 0);
         matrix.Set(0, 8, 0);
         matrix.Set(0, 9, 0);
         matrix.Set(0, 10, 0);
         Assert.AreEqual(40, MaskUtil.applyMaskPenaltyRule3(matrix));
      }

      [Test]
      public void TestApplyMaskPenaltyRule4()
      {
         // Dark cell ratio = 0%
         var matrix = new ByteMatrix(1, 1);
         matrix.Set(0, 0, 0);
         Assert.AreEqual(100, MaskUtil.applyMaskPenaltyRule4(matrix));
         // Dark cell ratio = 5%
         matrix = new ByteMatrix(2, 1);
         matrix.Set(0, 0, 0);
         matrix.Set(0, 0, 1);
         Assert.AreEqual(0, MaskUtil.applyMaskPenaltyRule4(matrix));
         // Dark cell ratio = 66.67%
         matrix = new ByteMatrix(6, 1);
         matrix.Set(0, 0, 0);
         matrix.Set(1, 0, 1);
         matrix.Set(2, 0, 1);
         matrix.Set(3, 0, 1);
         matrix.Set(4, 0, 1);
         matrix.Set(5, 0, 0);
         Assert.AreEqual(30, MaskUtil.applyMaskPenaltyRule4(matrix));
      }

      private static bool TestGetDataMaskBitInternal(int maskPattern,
                                             int[][] expected)
      {
         for (int x = 0; x < 6; ++x)
         {
            for (int y = 0; y < 6; ++y)
            {
               if ((expected[y][x] == 1) !=
                   MaskUtil.getDataMaskBit(maskPattern, x, y))
               {
                  return false;
               }
            }
         }
         return true;
      }

      // See mask patterns on the page 43 of JISX0510:2004.
      [Test]
      public void TestGetDataMaskBit()
      {
         int[][] mask0 = {
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {0, 1, 0, 1, 0, 1},
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {0, 1, 0, 1, 0, 1},
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {0, 1, 0, 1, 0, 1},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(0, mask0));
         int[][] mask1 = {
                            new[] {1, 1, 1, 1, 1, 1},
                            new[] {0, 0, 0, 0, 0, 0},
                            new[] {1, 1, 1, 1, 1, 1},
                            new[] {0, 0, 0, 0, 0, 0},
                            new[] {1, 1, 1, 1, 1, 1},
                            new[] {0, 0, 0, 0, 0, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(1, mask1));
         int[][] mask2 = {
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(2, mask2));
         int[][] mask3 = {
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {0, 0, 1, 0, 0, 1},
                            new[] {0, 1, 0, 0, 1, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {0, 0, 1, 0, 0, 1},
                            new[] {0, 1, 0, 0, 1, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(3, mask3));
         int[][] mask4 = {
                            new[] {1, 1, 1, 0, 0, 0},
                            new[] {1, 1, 1, 0, 0, 0},
                            new[] {0, 0, 0, 1, 1, 1},
                            new[] {0, 0, 0, 1, 1, 1},
                            new[] {1, 1, 1, 0, 0, 0},
                            new[] {1, 1, 1, 0, 0, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(4, mask4));
         int[][] mask5 = {
                            new[] {1, 1, 1, 1, 1, 1},
                            new[] {1, 0, 0, 0, 0, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {1, 0, 0, 1, 0, 0},
                            new[] {1, 0, 0, 0, 0, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(5, mask5));
         int[][] mask6 = {
                            new[] {1, 1, 1, 1, 1, 1},
                            new[] {1, 1, 1, 0, 0, 0},
                            new[] {1, 1, 0, 1, 1, 0},
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {1, 0, 1, 1, 0, 1},
                            new[] {1, 0, 0, 0, 1, 1},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(6, mask6));
         int[][] mask7 = {
                            new[] {1, 0, 1, 0, 1, 0},
                            new[] {0, 0, 0, 1, 1, 1},
                            new[] {1, 0, 0, 0, 1, 1},
                            new[] {0, 1, 0, 1, 0, 1},
                            new[] {1, 1, 1, 0, 0, 0},
                            new[] {0, 1, 1, 1, 0, 0},
                         };
         Assert.IsTrue(TestGetDataMaskBitInternal(7, mask7));
      }
   }
}