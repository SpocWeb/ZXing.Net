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

using NUnit.Framework;

namespace ZXing.Common.Test
{
   /// <summary>
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class BitSourceTestCase
   {
      [Test]
      public void TestSource()
      {
         byte[] bytes = {1, 2, 3, 4, 5};
         BitSource source = new BitSource(bytes);
         Assert.AreEqual(40, source.Available());
         Assert.AreEqual(0, source.ReadBits(1));
         Assert.AreEqual(39, source.Available());
         Assert.AreEqual(0, source.ReadBits(6));
         Assert.AreEqual(33, source.Available());
         Assert.AreEqual(1, source.ReadBits(1));
         Assert.AreEqual(32, source.Available());
         Assert.AreEqual(2, source.ReadBits(8));
         Assert.AreEqual(24, source.Available());
         Assert.AreEqual(12, source.ReadBits(10));
         Assert.AreEqual(14, source.Available());
         Assert.AreEqual(16, source.ReadBits(8));
         Assert.AreEqual(6, source.Available());
         Assert.AreEqual(5, source.ReadBits(6));
         Assert.AreEqual(0, source.Available());
      }
   }
}
