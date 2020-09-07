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

namespace ZXing.Client.Result.Test
{
    /// <summary>
    /// Tests <see cref="URIParsedResult" />.
    ///
    /// <author>Sean Owen</author>
    /// </summary>
    [TestFixture]
    public sealed class UriParsedResultTestCase
    {
        [Test]
        public void TestBookmarkDocomo()
        {
            DoTest("MEBKM:URL:google.com;;", "http://google.com", null);
            DoTest("MEBKM:URL:http://google.com;;", "http://google.com", null);
            DoTest("MEBKM:URL:google.com;TITLE:Google;", "http://google.com", "Google");
        }

        [Test]
        public void TestUri()
        {
            DoTest("google.com", "http://google.com", null);
            DoTest("123.com", "http://123.com", null);
            DoTest("http://google.com", "http://google.com", null);
            DoTest("https://google.com", "https://google.com", null);
            DoTest("google.com:443", "http://google.com:443", null);
            DoTest(
                "https://www.google.com/calendar/hosted/google.com/embed?mode=AGENDA&force_login=true&src=google.com_726f6f6d5f6265707075@resource.calendar.google.com",
                "https://www.google.com/calendar/hosted/google.com/embed?mode=AGENDA&force_login=true&src=google.com_726f6f6d5f6265707075@resource.calendar.google.com",
                null);
            DoTest("otpauth://remoteaccess?devaddr=00%a1b2%c3d4&devname=foo&key=bar",
                "otpauth://remoteaccess?devaddr=00%a1b2%c3d4&devname=foo&key=bar",
                null);
            DoTest("s3://amazon.com:8123", "s3://amazon.com:8123", null);
            DoTest("HTTP://R.BEETAGG.COM/?12345", "HTTP://R.BEETAGG.COM/?12345", null);
        }

        [Test]
        public void TestNotUri()
        {
            DoTestNotUri("google.c");
            DoTestNotUri(".com");
            DoTestNotUri(":80/");
            DoTestNotUri("ABC,20.3,AB,AD");
            DoTestNotUri("http://google.com?q=foo bar");
            DoTestNotUri("12756.501");
            DoTestNotUri("google.50");
            DoTestNotUri("foo.bar.bing.baz.foo.bar.bing.baz");
        }

        [Test]
        public void TestUrlto()
        {
            DoTest("urlto::bar.com", "http://bar.com", null);
            DoTest("urlto::http://bar.com", "http://bar.com", null);
            DoTest("urlto:foo:bar.com", "http://bar.com", "foo");
        }

        [Test]
        public void TestGarbage()
        {
            DoTestNotUri("Da65cV1g^>%^f0bAbPn1CJB6lV7ZY8hs0Sm:DXU0cd]GyEeWBz8]bUHLB");
            DoTestNotUri("DEA\u0003\u0019M\u0006\u0000\b√•\u0000¬áHO\u0000X$\u0001\u0000\u001Fwfc\u0007!√æ¬ì¬ò" +
                         "\u0013\u0013¬æZ{√π√é√ù√ö¬óZ¬ß¬®+y_zb√±k\u00117¬∏\u000E¬Ü√ú\u0000\u0000\u0000\u0000" +
                         "\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000" +
                         "\u0000\u0000\u0000\u0000\u0000\u0000\u0000\u0000¬£.ux");
        }

        [Test]
        public void TestIsPossiblyMalicious()
        {
            DoTestIsPossiblyMalicious("http://google.com", false);
            DoTestIsPossiblyMalicious("http://google.com@evil.com", true);
            DoTestIsPossiblyMalicious("http://google.com:@evil.com", true);
            DoTestIsPossiblyMalicious("google.com:@evil.com", false);
            DoTestIsPossiblyMalicious("https://google.com:443", false);
            DoTestIsPossiblyMalicious("https://google.com:443/", false);
            DoTestIsPossiblyMalicious("https://evil@google.com:443", true);
            DoTestIsPossiblyMalicious("http://google.com/foo@bar", false);
            DoTestIsPossiblyMalicious("http://google.com/@@", false);
        }

        [Test]
        public void TestMaliciousUnicode()
        {
            DoTestIsPossiblyMalicious("https://google.com\u2215.evil.com/stuff", true);
            DoTestIsPossiblyMalicious("\u202ehttps://dylankatz.com/moc.elgoog.www//:sptth", true);
        }

        [Test]
        public void TestExotic()
        {
            DoTest("bitcoin:mySD89iqpmptrK3PhHFW9fa7BXiP7ANy3Y", "bitcoin:mySD89iqpmptrK3PhHFW9fa7BXiP7ANy3Y", null);
            DoTest(
                "BTCTX:-TC4TO3$ZYZTC5NC83/SYOV+YGUGK:$BSF0P8/STNTKTKS.V84+JSA$LB+EHCG+8A725.2AZ-NAVX3VBV5K4MH7UL2.2M:" +
                "F*M9HSL*$2P7T*FX.ZT80GWDRV0QZBPQ+O37WDCNZBRM3EQ0S9SZP+3BPYZG02U/LA*89C2U.V1TS.CT1VF3DIN*HN3W-O-" +
                "0ZAKOAB32/.8:J501GJJTTWOA+5/6$MIYBERPZ41NJ6-WSG/*Z48ZH*LSAOEM*IXP81L:$F*W08Z60CR*C*P.JEEVI1F02J07L6+" +
                "W4L1G$/IC*$16GK6A+:I1-:LJ:Z-P3NW6Z6ADFB-F2AKE$2DWN23GYCYEWX9S8L+LF$VXEKH7/R48E32PU+A:9H:8O5",
                "BTCTX:-TC4TO3$ZYZTC5NC83/SYOV+YGUGK:$BSF0P8/STNTKTKS.V84+JSA$LB+EHCG+8A725.2AZ-NAVX3VBV5K4MH7UL2.2M:" +
                "F*M9HSL*$2P7T*FX.ZT80GWDRV0QZBPQ+O37WDCNZBRM3EQ0S9SZP+3BPYZG02U/LA*89C2U.V1TS.CT1VF3DIN*HN3W-O-" +
                "0ZAKOAB32/.8:J501GJJTTWOA+5/6$MIYBERPZ41NJ6-WSG/*Z48ZH*LSAOEM*IXP81L:$F*W08Z60CR*C*P.JEEVI1F02J07L6+" +
                "W4L1G$/IC*$16GK6A+:I1-:LJ:Z-P3NW6Z6ADFB-F2AKE$2DWN23GYCYEWX9S8L+LF$VXEKH7/R48E32PU+A:9H:8O5",
                null);
            DoTest("opc.tcp://test.samplehost.com:4841", "opc.tcp://test.samplehost.com:4841", null);
        }

        static void DoTest(string contents, string uri, string title)
        {
            BarCodeText fakeResult = new BarCodeText(contents, null, null, null, BarcodeFormat.QR_CODE);
            ParsedResult result = ResultParser.ParseResult(fakeResult);
            Assert.AreEqual(ParsedResultType.URI, result.Type);
            URIParsedResult uriResult = (URIParsedResult) result;
            Assert.AreEqual(uri, uriResult.URI);
            Assert.AreEqual(title, uriResult.Title);
        }

        static void DoTestNotUri(string text)
        {
            BarCodeText fakeResult = new BarCodeText(text, null, null, null, BarcodeFormat.QR_CODE);
            ParsedResult result = ResultParser.ParseResult(fakeResult);
            Assert.AreEqual(ParsedResultType.TEXT, result.Type);
            Assert.AreEqual(text, result.DisplayResult);
        }

        static void DoTestIsPossiblyMalicious(string uri, bool malicious)
        {
            var fakeResult = new BarCodeText(uri, null, null, null, BarcodeFormat.QR_CODE);
            ParsedResult result = ResultParser.ParseResult(fakeResult);
            Assert.AreEqual(malicious ? ParsedResultType.TEXT : ParsedResultType.URI, result.Type);
        }
    }
}