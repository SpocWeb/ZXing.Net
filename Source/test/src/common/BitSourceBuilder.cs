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

using System.IO;

namespace ZXing.Common.Test
{
   /// <summary>
   /// Class that lets one easily build an array of bytes by appending bits at a time.
   ///
   /// <author>Sean Owen</author>
   /// </summary>
   public sealed class BitSourceBuilder
   {
      private MemoryStream _Output;
      private int _NextByte;
      private int _BitsLeftInNextByte;

      public BitSourceBuilder()
      {
         _Output = new MemoryStream();
         _NextByte = 0;
         _BitsLeftInNextByte = 8;
      }

      public void Write(int value, int numBits)
      {
         if (numBits <= _BitsLeftInNextByte)
         {
            _NextByte <<= numBits;
            _NextByte |= value;
            _BitsLeftInNextByte -= numBits;
            if (_BitsLeftInNextByte == 0)
            {
               _Output.WriteByte((byte) _NextByte);
               _NextByte = 0;
               _BitsLeftInNextByte = 8;
            }
         }
         else
         {
            int bitsToWriteNow = _BitsLeftInNextByte;
            int numRestOfBits = numBits - bitsToWriteNow;
            int mask = 0xFF >> (8 - bitsToWriteNow);
            int valueToWriteNow = ((int)((uint)value >> numRestOfBits)) & mask;
            Write(valueToWriteNow, bitsToWriteNow);
            Write(value, numRestOfBits);
         }
      }

      public byte[] ToByteArray()
      {
         if (_BitsLeftInNextByte < 8)
         {
            Write(0, _BitsLeftInNextByte);
         }
         return _Output.ToArray();
      }
   }
}