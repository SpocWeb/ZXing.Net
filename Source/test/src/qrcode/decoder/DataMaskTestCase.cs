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

using NUnit.Framework;
using ZXing.Common;

namespace ZXing.QrCode.Internal.Test
{
   /// <summary>
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class DataMaskTestCase
   {
      [Test]
      public void TestMask0()
      {
         TestMaskAcrossDimensions(0, (i, j) => (i + j) % 2 == 0);
      }

      [Test]
      public void TestMask1()
      {
         TestMaskAcrossDimensions(1, (i, j) => i % 2 == 0);
      }

      [Test]
      public void TestMask2()
      {
         TestMaskAcrossDimensions(2, (i, j) => j % 3 == 0);
      }

      [Test]
      public void TestMask3()
      {
         TestMaskAcrossDimensions(3, (i, j) => (i + j) % 3 == 0);
      }

      [Test]
      public void TestMask4()
      {
         TestMaskAcrossDimensions(4, (i, j) => (i / 2 + j / 3) % 2 == 0);
      }

      [Test]
      public void TestMask5()
      {
         TestMaskAcrossDimensions(5, (i, j) => i * j % 2 + i * j % 3 == 0);
      }

      [Test]
      public void TestMask6()
      {
         TestMaskAcrossDimensions(6, (i, j) => (i * j % 2 + i * j % 3) % 2 == 0);
      }

      [Test]
      public void TestMask7()
      {
         TestMaskAcrossDimensions(7, (i, j) => ((i + j) % 2 + i * j % 3) % 2 == 0);
      }

      private static void TestMaskAcrossDimensions(int reference, Func<int, int, bool> condition)
      {
         for (int version = 1; version <= 40; version++)
         {
            int dimension = 17 + 4 * version;
            TestMask(reference, dimension, condition);
         }
      }

      private static void TestMask(int reference, int dimension, Func<int, int, bool> condition)
      {
         BitMatrix bits = new BitMatrix(dimension);
         DataMask.unmaskBitMatrix(reference, bits, dimension);
         for (int i = 0; i < dimension; i++)
         {
            for (int j = 0; j < dimension; j++)
            {
               Assert.AreEqual(
                   condition(i, j),
                   bits[j, i],
                   "(" + i + ',' + j + ')');
            }
         }
      }
   }
}