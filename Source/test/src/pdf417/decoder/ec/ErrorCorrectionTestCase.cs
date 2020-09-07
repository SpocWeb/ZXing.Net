/*
 * Copyright 2012 ZXing authors
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

using ZXing.PDF417.Internal.EC;

namespace ZXing.PDF417.Internal.Test
{
   /// <summary>
   /// @author Sean Owen
   /// </summary>
   public sealed class ErrorCorrectionTestCase : AbstractErrorCorrectionTestCase
   {

       static readonly int[] PDF417_TEST =
         {
            48, 901, 56, 141, 627, 856, 330, 69, 244, 900,
            852, 169, 843, 895, 852, 895, 913, 154, 845, 778,
            387, 89, 869, 901, 219, 474, 543, 650, 169, 201,
            9, 160, 35, 70, 900, 900, 900, 900, 900, 900,
            900, 900, 900, 900, 900, 900, 900, 900
         };

       static readonly int[] PDF417_TEST_WITH_EC =
         {
            48, 901, 56, 141, 627, 856, 330, 69, 244, 900,
            852, 169, 843, 895, 852, 895, 913, 154, 845, 778,
            387, 89, 869, 901, 219, 474, 543, 650, 169, 201,
            9, 160, 35, 70, 900, 900, 900, 900, 900, 900,
            900, 900, 900, 900, 900, 900, 900, 900, 769, 843,
            591, 910, 605, 206, 706, 917, 371, 469, 79, 718,
            47, 777, 249, 262, 193, 620, 597, 477, 450, 806,
            908, 309, 153, 871, 686, 838, 185, 674, 68, 679,
            691, 794, 497, 479, 234, 250, 496, 43, 347, 582,
            882, 536, 322, 317, 273, 194, 917, 237, 420, 859,
            340, 115, 222, 808, 866, 836, 417, 121, 833, 459,
            64, 159
         };

       static readonly int ECC_BYTES = PDF417_TEST_WITH_EC.Length - PDF417_TEST.Length;
       static readonly int ERROR_LIMIT = ECC_BYTES;
       static readonly int MAX_ERRORS = ERROR_LIMIT/2;
       static readonly int MAX_ERASURES = ERROR_LIMIT;

       readonly ErrorCorrection _Ec = new ErrorCorrection();

      [Test]
      public void TestNoError()
      {
         int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
         // no errors
         CheckDecode(received);
      }

      [Test]
      public void testOneError()
      {
         Random random = GetRandom();
         for (int i = 0; i < PDF417_TEST_WITH_EC.Length; i++)
         {
            int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
            received[i] = random.Next(256);
            CheckDecode(received);
         }
      }

      [Test]
      public void TestMaxErrors()
      {
         Random random = GetRandom();
         foreach (int test in PDF417_TEST)
         {
            // # iterations is kind of arbitrary
            int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
            Corrupt(received, MAX_ERRORS, random);
            CheckDecode(received);
         }
      }

      [Test]
      public void TestTooManyErrors()
      {
         int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
         Random random = GetRandom();
         Corrupt(received, MAX_ERRORS + 1, random);
         Assert.IsFalse(CheckDecode(received));
      }

      [Test]
      public void TestMaxErasures()
      {
         Random random = GetRandom();
         foreach (int test in PDF417_TEST)
         {
            // # iterations is kind of arbitrary
            int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
            int[] erasures = Erase(received, MAX_ERASURES, random);
            CheckDecode(received, erasures);
         }
      }

      [Test]
      public void TestTooManyErasures()
      {
         Random random = GetRandom();
         int[] received = (int[]) PDF417_TEST_WITH_EC.Clone();
         int[] erasures = Erase(received, MAX_ERASURES + 1, random);
         Assert.That(CheckDecode(received, erasures), Is.Not.True, "Should not have decoded");
      }

      bool CheckDecode(int[] received)
      {
         return CheckDecode(received, new int[0]);
      }

      bool CheckDecode(int[] received, int[] erasures)
      {
          if (!_Ec.Decode(received, ECC_BYTES, erasures, out var errorCount)) {
              return false;
          }

          for (int i = 0; i < PDF417_TEST.Length; i++)
         {
            if (received[i] != PDF417_TEST[i]) {
                return false;
            }
         }

         return true;
      }
   }
}