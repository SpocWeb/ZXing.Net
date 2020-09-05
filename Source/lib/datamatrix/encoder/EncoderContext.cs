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
        private readonly string msg;
        private SymbolShapeHint shape;
        private Dimension minSize;
        private Dimension maxSize;
        private readonly StringBuilder codewords;
        private int pos;
        private int newEncoding;
        private SymbolInfo symbolInfo;
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
            this.msg = sb.ToString(); //Not Unicode here!
            shape = SymbolShapeHint.FORCE_NONE;
            codewords = new StringBuilder(msg.Length);
            newEncoding = -1;
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

        public char CurrentChar => msg[pos];

        public char Current => msg[pos];

        public void writeCodewords(string codewords)
        {
            this.codewords.Append(codewords);
        }

        public void writeCodeword(char codeword)
        {
            codewords.Append(codeword);
        }

        public int CodewordCount => codewords.Length;

        public void signalEncoderChange(int encoding)
        {
            newEncoding = encoding;
        }

        public void resetEncoderSignal()
        {
            newEncoding = -1;
        }

        public bool HasMoreCharacters => pos < TotalMessageCharCount;

        private int TotalMessageCharCount => msg.Length - skipAtEnd;

        public int RemainingCharacters => TotalMessageCharCount - pos;

        public void updateSymbolInfo()
        {
            updateSymbolInfo(CodewordCount);
        }

        public void updateSymbolInfo(int len)
        {
            if (symbolInfo == null || len > symbolInfo.dataCapacity)
            {
                symbolInfo = SymbolInfo.lookup(len, shape, minSize, maxSize, true);
            }
        }

        public void resetSymbolInfo()
        {
            symbolInfo = null;
        }

        public int Pos
        {
            get => pos;
            set => pos = value;
        }

        public StringBuilder Codewords => codewords;

        public SymbolInfo SymbolInfo => symbolInfo;

        public int NewEncoding => newEncoding;

        public string Message => msg;

        public bool Fnc1CodewordIsWritten { get; set; }
    }
}