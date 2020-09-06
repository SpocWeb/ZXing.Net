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

namespace ZXing.Common.Test
{
   /// <summary>
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class PerspectiveTransformTestCase
   {
      private static float _EPSILON = 1.0E-4f;

      [Test]
      public void TestSquareToQuadrilateral()
      {
         PerspectiveTransform pt = XTrafo.SquareToQuadrilateral(
             2.0f, 3.0f, 10.0f, 4.0f, 16.0f, 15.0f, 4.0f, 9.0f);
         AssertPointEquals(2.0f, 3.0f, 0.0f, 0.0f, pt);
         AssertPointEquals(10.0f, 4.0f, 1.0f, 0.0f, pt);
         AssertPointEquals(4.0f, 9.0f, 0.0f, 1.0f, pt);
         AssertPointEquals(16.0f, 15.0f, 1.0f, 1.0f, pt);
         AssertPointEquals(6.535211f, 6.8873234f, 0.5f, 0.5f, pt);
         AssertPointEquals(48.0f, 42.42857f, 1.5f, 1.5f, pt);
      }

      [Test]
      public void TestQuadrilateralToQuadrilateral()
      {
         PerspectiveTransform pt = XTrafo.QuadrilateralToQuadrilateral(
             2.0f, 3.0f, 10.0f, 4.0f, 16.0f, 15.0f, 4.0f, 9.0f,
             103.0f, 110.0f, 300.0f, 120.0f, 290.0f, 270.0f, 150.0f, 280.0f);
         AssertPointEquals(103.0f, 110.0f, 2.0f, 3.0f, pt);
         AssertPointEquals(300.0f, 120.0f, 10.0f, 4.0f, pt);
         AssertPointEquals(290.0f, 270.0f, 16.0f, 15.0f, pt);
         AssertPointEquals(150.0f, 280.0f, 4.0f, 9.0f, pt);
         AssertPointEquals(7.1516876f, -64.60185f, 0.5f, 0.5f, pt);
         AssertPointEquals(328.09116f, 334.16385f, 50.0f, 50.0f, pt);
      }

      private static void AssertPointEquals(float expectedX,
                                            float expectedY,
                                            float sourceX,
                                            float sourceY,
                                            PerspectiveTransform pt)
      {
         float[] points = { sourceX, sourceY };
         pt.TransformPoints(points);
         Assert.AreEqual(expectedX, points[0], _EPSILON);
         Assert.AreEqual(expectedY, points[1], _EPSILON);
      }
   }
}
