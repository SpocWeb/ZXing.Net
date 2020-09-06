/*
 * Copyright 2014 ZXing authors
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

namespace ZXing.Common.Detector.Test
{
   [TestFixture]
   public class MathUtilsTestCase
   {
      private static float _EPSILON = 1.0E-8f;

      [Test]
      public void TestRound()
      {
         Assert.That(MathUtils.Round(-1.0f), Is.EqualTo(-1));
         Assert.That(MathUtils.Round(0.0f), Is.EqualTo(0));
         Assert.That(MathUtils.Round(1.0f), Is.EqualTo(1));

         Assert.That(MathUtils.Round(1.9f), Is.EqualTo(2));
         Assert.That(MathUtils.Round(2.1f), Is.EqualTo(2));

         Assert.That(MathUtils.Round(2.5f), Is.EqualTo(3));

         Assert.That(MathUtils.Round(-1.9f), Is.EqualTo(-2));
         Assert.That(MathUtils.Round(-2.1f), Is.EqualTo(-2));

         Assert.That(MathUtils.Round(-2.5f), Is.EqualTo(-3)); // This differs from Math.round()

         // doesn't work like java
         //Assert.That(MathUtils.round(int.MaxValue), Is.EqualTo(int.MaxValue));
         Assert.That(MathUtils.Round(int.MinValue), Is.EqualTo(int.MinValue));

         Assert.That(MathUtils.Round(float.PositiveInfinity), Is.EqualTo(int.MaxValue));
         Assert.That(MathUtils.Round(float.NegativeInfinity), Is.EqualTo(int.MinValue));

         Assert.That(MathUtils.Round(float.NaN), Is.EqualTo(0));
      }

      [Test]
      public void TestDistance()
      {
         Assert.AreEqual((float)System.Math.Sqrt(8.0), MathUtils.Distance(1.0f, 2.0f, 3.0f, 4.0f), _EPSILON);
         Assert.AreEqual(0.0f, MathUtils.Distance(1.0f, 2.0f, 1.0f, 2.0f), _EPSILON);

         Assert.AreEqual((float)System.Math.Sqrt(8.0), MathUtils.Distance(1, 2, 3, 4), _EPSILON);
         Assert.AreEqual(0.0f, MathUtils.Distance(1, 2, 1, 2), _EPSILON);
      }

      [Test]
      public void TestSum()
      {
         Assert.That(MathUtils.Sum(new int[] { }), Is.EqualTo(0));
         Assert.That(MathUtils.Sum(new int[] { 1 }), Is.EqualTo(1));
         Assert.That(MathUtils.Sum(new int[] { 1, 3 }), Is.EqualTo(4));
         Assert.That(MathUtils.Sum(new int[] { -1, 1 }), Is.EqualTo(0));
      }
   }
}
