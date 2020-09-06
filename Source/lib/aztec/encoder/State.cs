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

using System;
using System.Collections.Generic;
using ZXing.Common;

namespace ZXing.Aztec.Internal
{
    /// <summary>
    /// State represents all information about a sequence necessary to generate the current output.
    /// Note that a state is immutable.
    /// </summary>
    internal sealed class State
    {
        public static readonly State INITIAL_STATE = new State(Token.EMPTY, HighLevelEncoder.MODE_UPPER, 0, 0);

        // The current mode of the encoding (or the mode to which we'll return if
        // we're in Binary Shift mode.
        // The list of tokens that we output.  If we are in Binary Shift mode, this
        // token list does *not* yet included the token for those bytes
        // If non-zero, the number of most recent bytes that should be output
        // in Binary Shift mode.
        // The total number of bits generated (including Binary Shift).

        public State(Token token, int mode, int binaryBytes, int bitCount)
        {
            this.Token = token;
            this.Mode = mode;
            BinaryShiftByteCount = binaryBytes;
            this.BitCount = bitCount;
            // Make sure we match the token
            //int binaryShiftBitCount = (binaryShiftByteCount * 8) +
            //    (binaryShiftByteCount == 0 ? 0 :
            //     binaryShiftByteCount <= 31 ? 10 :
            //     binaryShiftByteCount <= 62 ? 20 : 21);
            //assert this.bitCount == token.getTotalBitCount() + binaryShiftBitCount;
        }

        public int Mode { get; }

        public Token Token { get; }

        public int BinaryShiftByteCount { get; }

        public int BitCount { get; }

        /// <summary>
        /// Create a new state representing this state with a latch to a (not
        /// necessary different) mode, and then a code.
        /// </summary>
        public State latchAndAppend(int mode, int value)
        {
            //assert binaryShiftByteCount == 0;
            int bitCount = this.BitCount;
            Token token = this.Token;
            if (mode != this.Mode)
            {
                int latch = HighLevelEncoder.LATCH_TABLE[this.Mode][mode];
                token = token.add(latch & 0xFFFF, latch >> 16);
                bitCount += latch >> 16;
            }
            int latchModeBitCount = mode == HighLevelEncoder.MODE_DIGIT ? 4 : 5;
            token = token.add(value, latchModeBitCount);
            return new State(token, mode, 0, bitCount + latchModeBitCount);
        }

        /// <summary>
        /// Create a new state representing this state, with a temporary shift
        /// to a different mode to output a single value.
        /// </summary>
        public State shiftAndAppend(int mode, int value)
        {
            //assert binaryShiftByteCount == 0 && this.mode != mode;
            Token token = this.Token;
            int thisModeBitCount = this.Mode == HighLevelEncoder.MODE_DIGIT ? 4 : 5;
            // Shifts exist only to UPPER and PUNCT, both with tokens size 5.
            token = token.add(HighLevelEncoder.SHIFT_TABLE[this.Mode][mode], thisModeBitCount);
            token = token.add(value, 5);
            return new State(token, this.Mode, 0, BitCount + thisModeBitCount + 5);
        }

        /// <summary>
        /// Create a new state representing this state, but an additional character
        /// output in Binary Shift mode.
        /// </summary>
        public State addBinaryShiftChar(int index)
        {
            Token token = this.Token;
            int mode = this.Mode;
            int bitCount = this.BitCount;
            if (this.Mode == HighLevelEncoder.MODE_PUNCT || this.Mode == HighLevelEncoder.MODE_DIGIT)
            {
                //assert binaryShiftByteCount == 0;
                int latch = HighLevelEncoder.LATCH_TABLE[mode][HighLevelEncoder.MODE_UPPER];
                token = token.add(latch & 0xFFFF, latch >> 16);
                bitCount += latch >> 16;
                mode = HighLevelEncoder.MODE_UPPER;
            }
            int deltaBitCount =
               (BinaryShiftByteCount == 0 || BinaryShiftByteCount == 31) ? 18 :
                  (BinaryShiftByteCount == 62) ? 9 : 8;
            State result = new State(token, mode, BinaryShiftByteCount + 1, bitCount + deltaBitCount);
            if (result.BinaryShiftByteCount == 2047 + 31)
            {
                // The string is as long as it's allowed to be.  We should end it.
                result = result.endBinaryShift(index + 1);
            }
            return result;
        }

        /// <summary>
        /// Create the state identical to this one, but we are no longer in
        /// Binary Shift mode.
        /// </summary>
        public State endBinaryShift(int index)
        {
            if (BinaryShiftByteCount == 0)
            {
                return this;
            }
            Token token = this.Token;
            token = token.addBinaryShift(index - BinaryShiftByteCount, BinaryShiftByteCount);
            //assert token.getTotalBitCount() == this.bitCount;
            return new State(token, Mode, 0, BitCount);
        }

        /// <summary>
        /// Returns true if "this" state is better (or equal) to be in than "that"
        /// state under all possible circumstances.
        /// </summary>
        public bool isBetterThanOrEqualTo(State other)
        {
            int newModeBitCount = BitCount + (HighLevelEncoder.LATCH_TABLE[Mode][other.Mode] >> 16);
            if (BinaryShiftByteCount < other.BinaryShiftByteCount)
            {
                // add additional B/S encoding cost of other, if any
                newModeBitCount += calculateBinaryShiftCost(other) - calculateBinaryShiftCost(this);
            }
            else if (BinaryShiftByteCount > other.BinaryShiftByteCount && other.BinaryShiftByteCount > 0)
            {
                // maximum possible additional cost (we end up exceeding the 31 byte boundary and other state can stay beneath it)
                newModeBitCount += 10;
            }
            return newModeBitCount <= other.BitCount;
        }

        public BitArray toBitArray(byte[] text)
        {
            // Reverse the tokens, so that they are in the order that they should
            // be output
            var symbols = new LinkedList<Token>();
            for (Token token = endBinaryShift(text.Length).Token; token != null; token = token.Previous)
            {
                symbols.AddFirst(token);
            }
            BitArray bitArray = new BitArray();
            // Add each token to the result.
            foreach (Token symbol in symbols)
            {
                symbol.appendTo(bitArray, text);
            }
            //assert bitArray.getSize() == this.bitCount;
            return bitArray;
        }

        public override string ToString()
        {
            return string.Format("{0} bits={1} bytes={2}", HighLevelEncoder.MODE_NAMES[Mode], BitCount, BinaryShiftByteCount);
        }

        private static int calculateBinaryShiftCost(State state)
        {
            if (state.BinaryShiftByteCount > 62)
            {
                return 21; // B/S with extended length
            }
            if (state.BinaryShiftByteCount > 31)
            {
                return 20; // two B/S
            }
            if (state.BinaryShiftByteCount > 0)
            {
                return 10; // one B/S
            }
            return 0;
        }
    }
}