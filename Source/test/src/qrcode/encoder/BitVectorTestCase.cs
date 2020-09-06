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
   /// <author>dswitkin@google.com (Daniel Switkin) - ported from C++</author>
   /// </summary>
   [TestFixture]
   public class BitVectorTestCase
   {

      private static long GetUnsignedInt(BitArray v, int index)
      {
         long result = 0L;
         for (int i = 0, offset = index << 3; i < 32; i++)
         {
            if (v[offset + i])
            {
               result |= 1L << (31 - i);
            }
         }
         return result;
      }

      [Test]
      public void TestAppendBit()
      {
         BitArray v = new BitArray();
         Assert.AreEqual(0, v.SizeInBytes);
         // 1
         v.AppendBit(true);
         Assert.AreEqual(1, v.Size);
         Assert.AreEqual(0x80000000L, GetUnsignedInt(v, 0));
         // 10
         v.AppendBit(false);
         Assert.AreEqual(2, v.Size);
         Assert.AreEqual(0x80000000L, GetUnsignedInt(v, 0));
         // 101
         v.AppendBit(true);
         Assert.AreEqual(3, v.Size);
         Assert.AreEqual(0xa0000000L, GetUnsignedInt(v, 0));
         // 1010
         v.AppendBit(false);
         Assert.AreEqual(4, v.Size);
         Assert.AreEqual(0xa0000000L, GetUnsignedInt(v, 0));
         // 10101
         v.AppendBit(true);
         Assert.AreEqual(5, v.Size);
         Assert.AreEqual(0xa8000000L, GetUnsignedInt(v, 0));
         // 101010
         v.AppendBit(false);
         Assert.AreEqual(6, v.Size);
         Assert.AreEqual(0xa8000000L, GetUnsignedInt(v, 0));
         // 1010101
         v.AppendBit(true);
         Assert.AreEqual(7, v.Size);
         Assert.AreEqual(0xaa000000L, GetUnsignedInt(v, 0));
         // 10101010
         v.AppendBit(false);
         Assert.AreEqual(8, v.Size);
         Assert.AreEqual(0xaa000000L, GetUnsignedInt(v, 0));
         // 10101010 1
         v.AppendBit(true);
         Assert.AreEqual(9, v.Size);
         Assert.AreEqual(0xaa800000L, GetUnsignedInt(v, 0));
         // 10101010 10
         v.AppendBit(false);
         Assert.AreEqual(10, v.Size);
         Assert.AreEqual(0xaa800000L, GetUnsignedInt(v, 0));
      }

      [Test]
      public void TestAppendBits()
      {
         var v = new BitArray();
         v.AppendBits(0x1, 1);
         Assert.AreEqual(1, v.Size);
         Assert.AreEqual(0x80000000L, GetUnsignedInt(v, 0));
         v = new BitArray();
         v.AppendBits(0xff, 8);
         Assert.AreEqual(8, v.Size);
         Assert.AreEqual(0xff000000L, GetUnsignedInt(v, 0));
         v = new BitArray();
         v.AppendBits(0xff7, 12);
         Assert.AreEqual(12, v.Size);
         Assert.AreEqual(0xff700000L, GetUnsignedInt(v, 0));
      }

      [Test]
      public void TestNumBytes()
      {
         BitArray v = new BitArray();
         Assert.AreEqual(0, v.SizeInBytes);
         v.AppendBit(false);
         // 1 bit was added in the vector, so 1 byte should be consumed.
         Assert.AreEqual(1, v.SizeInBytes);
         v.AppendBits(0, 7);
         Assert.AreEqual(1, v.SizeInBytes);
         v.AppendBits(0, 8);
         Assert.AreEqual(2, v.SizeInBytes);
         v.AppendBits(0, 1);
         // We now have 17 bits, so 3 bytes should be consumed.
         Assert.AreEqual(3, v.SizeInBytes);
      }

      [Test]
      public void TestAppendBitVector()
      {
         BitArray v1 = new BitArray();
         v1.AppendBits(0xbe, 8);
         BitArray v2 = new BitArray();
         v2.AppendBits(0xef, 8);
         v1.AppendBitArray(v2);
         // beef = 1011 1110 1110 1111
         Assert.AreEqual(" X.XXXXX. XXX.XXXX", v1.ToString());
      }

      [Test]
      public void TestXor()
      {
         var v1 = new BitArray();
         v1.AppendBits(0x5555aaaa, 32);
         var v2 = new BitArray();
         v2.AppendBits(-1431677611, 32); // 0xaaaa5555
         v1.Xor(v2);
         Assert.AreEqual(0xffffffffL, GetUnsignedInt(v1, 0));
      }

      [Test]
      public void TestXor2()
      {
         var v1 = new BitArray();
         v1.AppendBits(0x2a, 7); // 010 1010
         var v2 = new BitArray();
         v2.AppendBits(0x55, 7); // 101 0101
         v1.Xor(v2);
         Assert.AreEqual(0xfe000000L, GetUnsignedInt(v1, 0)); // 1111 1110
      }

      [Test]
      public void TestAt()
      {
         BitArray v = new BitArray();
         v.AppendBits(0xdead, 16);  // 1101 1110 1010 1101
         Assert.IsTrue(v[0]);
         Assert.IsTrue(v[1]);
         Assert.IsFalse(v[2]);
         Assert.IsTrue(v[3]);

         Assert.IsTrue(v[4]);
         Assert.IsTrue(v[5]);
         Assert.IsTrue(v[6]);
         Assert.IsFalse(v[7]);

         Assert.IsTrue(v[8]);
         Assert.IsFalse(v[9]);
         Assert.IsTrue(v[10]);
         Assert.IsFalse(v[11]);

         Assert.IsTrue(v[12]);
         Assert.IsTrue(v[13]);
         Assert.IsFalse(v[14]);
         Assert.IsTrue(v[15]);
      }

      [Test]
      public void TestToString()
      {
         BitArray v = new BitArray();
         v.AppendBits(0xdead, 16);  // 1101 1110 1010 1101
         Assert.AreEqual(" XX.XXXX. X.X.XX.X", v.ToString());
      }
   }
}