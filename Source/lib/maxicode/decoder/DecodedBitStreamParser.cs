/*
 * Copyright 2011 ZXing authors
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
using ZXing.Common;

namespace ZXing.Maxicode.Internal
{
    /// <summary>
    /// <p>MaxiCodes can encode text or structured information as bits in one of several modes,
    /// with multiple character sets in one code. This class decodes the bits back into text.</p>
    ///
    /// <author>mike32767</author>
    /// <author>Manuel Kasten</author>
    /// </summary>
    public static class DecodedBitStreamParser
    {

        const char SHIFTA = '\uFFF0';
        const char SHIFTB = '\uFFF1';
        const char SHIFTC = '\uFFF2';
        const char SHIFTD = '\uFFF3';
        const char SHIFTE = '\uFFF4';
        const char TWOSHIFTA = '\uFFF5';
        const char THREESHIFTA = '\uFFF6';
        const char LATCHA = '\uFFF7';
        const char LATCHB = '\uFFF8';
        const char LOCK = '\uFFF9';
        const char ECI = '\uFFFA';
        const char NS = '\uFFFB';
        const char PAD = '\uFFFC';
        const char FS = '\u001C';
        const char GS = '\u001D';
        const char RS = '\u001E';
        const string NINE_DIGITS = "000000000";
        const string THREE_DIGITS = "000";

        static string[] _SETS = {
                                 "\nABCDEFGHIJKLMNOPQRSTUVWXYZ"+ECI+FS+GS+RS+NS+' '+PAD+"\"#$%&'()*+,-./0123456789:"+SHIFTB+SHIFTC+SHIFTD+SHIFTE+LATCHB,
                                 "`abcdefghijklmnopqrstuvwxyz"+ECI+FS+GS+RS+NS+'{'+PAD+"}~\u007F;<=>?[\\]^_ ,./:@!|"+PAD+TWOSHIFTA+THREESHIFTA+PAD+SHIFTA+SHIFTC+SHIFTD+SHIFTE+LATCHA,
                                 "\u00C0\u00C1\u00C2\u00C3\u00C4\u00C5\u00C6\u00C7\u00C8\u00C9\u00CA\u00CB\u00CC\u00CD\u00CE\u00CF\u00D0\u00D1\u00D2\u00D3\u00D4\u00D5\u00D6\u00D7\u00D8\u00D9\u00DA"+ECI+FS+GS+RS+"\u00DB\u00DC\u00DD\u00DE\u00DF\u00AA\u00AC\u00B1\u00B2\u00B3\u00B5\u00B9\u00BA\u00BC\u00BD\u00BE\u0080\u0081\u0082\u0083\u0084\u0085\u0086\u0087\u0088\u0089"+LATCHA+' '+LOCK+SHIFTD+SHIFTE+LATCHB,
                                 "\u00E0\u00E1\u00E2\u00E3\u00E4\u00E5\u00E6\u00E7\u00E8\u00E9\u00EA\u00EB\u00EC\u00ED\u00EE\u00EF\u00F0\u00F1\u00F2\u00F3\u00F4\u00F5\u00F6\u00F7\u00F8\u00F9\u00FA"+ECI+FS+GS+RS+NS+"\u00FB\u00FC\u00FD\u00FE\u00FF\u00A1\u00A8\u00AB\u00AF\u00B0\u00B4\u00B7\u00B8\u00BB\u00BF\u008A\u008B\u008C\u008D\u008E\u008F\u0090\u0091\u0092\u0093\u0094"+LATCHA+' '+SHIFTC+LOCK+SHIFTE+LATCHB,
                                 "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\n\u000B\u000C\r\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A"+ECI+PAD+PAD+'\u001B'+NS+FS+GS+RS+"\u001F\u009F\u00A0\u00A2\u00A3\u00A4\u00A5\u00A6\u00A7\u00A9\u00AD\u00AE\u00B6\u0095\u0096\u0097\u0098\u0099\u009A\u009B\u009C\u009D\u009E"+LATCHA+' '+SHIFTC+SHIFTD+LOCK+LATCHB,
                                 "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u0009\n\u000B\u000C\r\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F\u0020\u0021\"\u0023\u0024\u0025\u0026\u0027\u0028\u0029\u002A\u002B\u002C\u002D\u002E\u002F\u0030\u0031\u0032\u0033\u0034\u0035\u0036\u0037\u0038\u0039\u003A\u003B\u003C\u003D\u003E\u003F"
                              };

        public static DecoderResult Decode(byte[] bytes, int mode)
        {
            StringBuilder result = new StringBuilder(144);
            switch (mode)
            {
                case 2:
                case 3:
                    string postcode;
                    if (mode == 2)
                    {
                        int pc = GetPostCode2(bytes);
                        var df = "0000000000".Substring(0, GetPostCode2Length(bytes));
                        postcode = pc.ToString(df);
                    }
                    else
                    {
                        postcode = GetPostCode3(bytes);
                    }
                    string country = GetCountry(bytes).ToString(THREE_DIGITS);
                    string service = GetServiceClass(bytes).ToString(THREE_DIGITS);
                    result.Append(GetMessage(bytes, 10, 84));
                    if (result.ToString().StartsWith("[)>" + RS + "01" + GS))
                    {
                        result.Insert(9, postcode + GS + country + GS + service + GS);
                    }
                    else
                    {
                        result.Insert(0, postcode + GS + country + GS + service + GS);
                    }
                    break;
                case 4:
                    result.Append(GetMessage(bytes, 1, 93));
                    break;
                case 5:
                    result.Append(GetMessage(bytes, 1, 77));
                    break;
            }

            return new DecoderResult(bytes, result.ToString(), null, mode.ToString());
        }

        static int GetBit(int bit, byte[] bytes)
        {
            bit--;
            return (bytes[bit / 6] & (1 << (5 - bit % 6))) == 0 ? 0 : 1;
        }

        static int GetInt(byte[] bytes, byte[] x)
        {
            if (x.Length == 0) {
                throw new ArgumentException("x");
            }

            int val = 0;
            for (int i = 0; i < x.Length; i++)
            {
                val += GetBit(x[i], bytes) << (x.Length - i - 1);
            }
            return val;
        }

        static int GetCountry(byte[] bytes)
        {
            return GetInt(bytes, new byte[] { 53, 54, 43, 44, 45, 46, 47, 48, 37, 38 });
        }

        static int GetServiceClass(byte[] bytes)
        {
            return GetInt(bytes, new byte[] { 55, 56, 57, 58, 59, 60, 49, 50, 51, 52 });
        }

        static int GetPostCode2Length(byte[] bytes)
        {
            return GetInt(bytes, new byte[] { 39, 40, 41, 42, 31, 32 });
        }

        static int GetPostCode2(byte[] bytes)
        {
            return GetInt(bytes, new byte[] {33, 34, 35, 36, 25, 26, 27, 28, 29, 30, 19,
        20, 21, 22, 23, 24, 13, 14, 15, 16, 17, 18, 7, 8, 9, 10, 11, 12, 1, 2});
        }

        static string GetPostCode3(byte[] bytes)
        {
            return new string(
               new[]
                  {
                  _SETS[0][GetInt(bytes, new byte[] {39, 40, 41, 42, 31, 32})],
                  _SETS[0][GetInt(bytes, new byte[] {33, 34, 35, 36, 25, 26})],
                  _SETS[0][GetInt(bytes, new byte[] {27, 28, 29, 30, 19, 20})],
                  _SETS[0][GetInt(bytes, new byte[] {21, 22, 23, 24, 13, 14})],
                  _SETS[0][GetInt(bytes, new byte[] {15, 16, 17, 18, 7, 8})],
                  _SETS[0][GetInt(bytes, new byte[] {9, 10, 11, 12, 1, 2})],
                  }
               );
        }

        static string GetMessage(byte[] bytes, int start, int len)
        {
            StringBuilder sb = new StringBuilder();
            int shift = -1;
            int set = 0;
            int lastset = 0;
            for (int i = start; i < start + len; i++)
            {
                char c = _SETS[set][bytes[i]];
                switch (c)
                {
                    case LATCHA:
                        set = 0;
                        shift = -1;
                        break;
                    case LATCHB:
                        set = 1;
                        shift = -1;
                        break;
                    case SHIFTA:
                    case SHIFTB:
                    case SHIFTC:
                    case SHIFTD:
                    case SHIFTE:
                        lastset = set;
                        set = c - SHIFTA;
                        shift = 1;
                        break;
                    case TWOSHIFTA:
                        lastset = set;
                        set = 0;
                        shift = 2;
                        break;
                    case THREESHIFTA:
                        lastset = set;
                        set = 0;
                        shift = 3;
                        break;
                    case NS:
                        int nsval = (bytes[++i] << 24) + (bytes[++i] << 18) + (bytes[++i] << 12) + (bytes[++i] << 6) + bytes[++i];
                        sb.Append(nsval.ToString(NINE_DIGITS));
                        break;
                    case LOCK:
                        shift = -1;
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
                if (shift-- == 0)
                {
                    set = lastset;
                }
            }
            while (sb.Length > 0 && sb[sb.Length - 1] == PAD)
            {
                sb.Length = sb.Length - 1;
            }
            return sb.ToString();
        }
    }
}