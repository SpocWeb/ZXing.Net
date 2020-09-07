/*
 * Copyright 2013 ZXing authors
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

namespace ZXing.Common.ReedSolomon.Test
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Rustam Abdullaev</author>
    [TestFixture]
   public sealed class GenericGfPolyTestCase
   {
       private static readonly GenericGf FIELD = GenericGf.QR_CODE_FIELD_256;

       [Test]
        public void TestPolynomialString()
       {
           Assert.That(FIELD.Zero.ToString(), Is.EqualTo("0"));
           Assert.That(FIELD.BuildMonomial(0, -1).ToString(), Is.EqualTo("-1"));
           var p = new GenericGfPoly(FIELD, new int[] { 3, 0, -2, 1, 1 });
           Assert.That(p.ToString(), Is.EqualTo("a^25x^4 - ax^2 + x + 1"));
           p = new GenericGfPoly(FIELD, new int[] { 3 });
           Assert.That(p.ToString(), Is.EqualTo("a^25"));
        }

       [Test]
        public void TestZero()
       {
           Assert.That(FIELD.Zero, Is.EqualTo(FIELD.BuildMonomial(1, 0)));
           Assert.That(FIELD.Zero, Is.EqualTo(FIELD.BuildMonomial(1, 2).Multiply(0)));
       }
    }
}