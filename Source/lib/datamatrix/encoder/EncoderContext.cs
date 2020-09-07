/*
 * Copyright 2006-2007 Jeremias Maerki.
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
using System.Text;

namespace ZXing.Datamatrix.Encoder
{
    internal sealed class EncoderContext
    {

        private SymbolShapeHint shape;
        private Dimension minSize;
        private Dimension maxSize;
        private int skipAtEnd;
        private static readonly Encoding encoding;

        static EncoderContext()
        {
#if !(WindowsCE || SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE)
            encoding = Encoding.GetEncoding("ISO-8859-1");
#elif WindowsCE
         try
         {
            encoding = Encoding.GetEncoding("ISO-8859-1");
         }
         catch (PlatformNotSupportedException)
         {
            encoding = Encoding.GetEncoding(1252);
         }
#else
         // not fully correct but what else
         encoding = Encoding.GetEncoding("UTF-8");
#endif
        }

        public EncoderContext(string msg)
        {
            //From this point on Strings are not Unicode anymore!
            var msgBinary = encoding.GetBytes(msg);
            var sb = new StringBuilder(msgBinary.Length);
            var c = msgBinary.Length;
            for (int i = 0; i < c; i++)
            {
                // TODO: does it works in .Net the same way?
                var ch = (char)(msgBinary[i] & 0xff);
                if (ch == '?' && msg[i] != '?')
                {
                    throw new ArgumentException("Message contains characters outside " + encoding.WebName + " encoding.");
                }
                sb.Append(ch);
            }
            Message = sb.ToString(); //Not Unicode here!
            shape = SymbolShapeHint.FORCE_NONE;
            Codewords = new StringBuilder(msg.Length);
            NewEncoding = -1;
        }

        public void setSymbolShape(SymbolShapeHint shape)
        {
            this.shape = shape;
        }

        public void setSizeConstraints(Dimension minSize, Dimension maxSize)
        {
            this.minSize = minSize;
            this.maxSize = maxSize;
        }

        public void setSkipAtEnd(int count)
        {
            skipAtEnd = count;
        }

        public char CurrentChar => Message[Pos];

        public char Current => Message[Pos];

        public void writeCodewords(string codewords)
        {
            Codewords.Append(codewords);
        }

        public void writeCodeword(char codeword)
        {
            Codewords.Append(codeword);
        }

        public int CodewordCount => Codewords.Length;

        public void signalEncoderChange(int encoding)
        {
            NewEncoding = encoding;
        }

        public void resetEncoderSignal()
        {
            NewEncoding = -1;
        }

        public bool HasMoreCharacters => Pos < TotalMessageCharCount;

        private int TotalMessageCharCount => Message.Length - skipAtEnd;

        public int RemainingCharacters => TotalMessageCharCount - Pos;

        public void updateSymbolInfo()
        {
            updateSymbolInfo(CodewordCount);
        }

        public void updateSymbolInfo(int len)
        {
            if (SymbolInfo == null || len > SymbolInfo.dataCapacity)
            {
                SymbolInfo = SymbolInfo.lookup(len, shape, minSize, maxSize, true);
            }
        }

        public void resetSymbolInfo()
        {
            SymbolInfo = null;
        }

        public int Pos { get; set; }

        public StringBuilder Codewords { get; }

        public SymbolInfo SymbolInfo { get; set; }

        public int NewEncoding { get; set; }

        public string Message { get; }

        public bool Fnc1CodewordIsWritten { get; set; }
    }
}