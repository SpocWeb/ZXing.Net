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

using System.Globalization;
using System.Threading;

using NUnit.Framework;

namespace ZXing.Client.Result.Test
{
    /// <summary>
    /// Tests <see cref="ParsedResult" />.
    ///
    /// <author>Sean Owen</author>
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    /// </summary>
    [TestFixture]
   public sealed class ParsedReaderResultTestCase
   {
      [SetUp]
      public void SetUp()
      {
         //Locale.setDefault(Locale.ENGLISH);
         //TimeZone.setDefault(TimeZone.getTimeZone("GMT"));
         Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
      }

      [Test]
      public void TestTextType()
      {
         DoTestResult("", "", ParsedResultType.TEXT);
         DoTestResult("foo", "foo", ParsedResultType.TEXT);
         DoTestResult("Hi.", "Hi.", ParsedResultType.TEXT);
         DoTestResult("This is a test", "This is a test", ParsedResultType.TEXT);
         DoTestResult("This is a test\nwith newlines", "This is a test\nwith newlines",
             ParsedResultType.TEXT);
         DoTestResult("This: a test with lots of @ nearly-random punctuation! No? OK then.",
             "This: a test with lots of @ nearly-random punctuation! No? OK then.",
             ParsedResultType.TEXT);
      }

      [Test]
      public void TestBookmarkType()
      {
         DoTestResult("MEBKM:URL:google.com;;", "http://google.com", ParsedResultType.URI);
         DoTestResult("MEBKM:URL:google.com;TITLE:Google;;", "Google\nhttp://google.com",
             ParsedResultType.URI);
         DoTestResult("MEBKM:TITLE:Google;URL:google.com;;", "Google\nhttp://google.com",
             ParsedResultType.URI);
         DoTestResult("MEBKM:URL:http://google.com;;", "http://google.com", ParsedResultType.URI);
         DoTestResult("MEBKM:URL:HTTPS://google.com;;", "HTTPS://google.com", ParsedResultType.URI);
      }

      [Test]
      public void TestUrltoType()
      {
         DoTestResult("urlto:foo:bar.com", "foo\nhttp://bar.com", ParsedResultType.URI);
         DoTestResult("URLTO:foo:bar.com", "foo\nhttp://bar.com", ParsedResultType.URI);
         DoTestResult("URLTO::bar.com", "http://bar.com", ParsedResultType.URI);
         DoTestResult("URLTO::http://bar.com", "http://bar.com", ParsedResultType.URI);
      }

      [Test]
      public void TestEmailType()
      {
         DoTestResult("MATMSG:TO:srowen@example.org;;",
                      "srowen@example.org", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("MATMSG:TO:srowen@example.org;SUB:Stuff;;", "srowen@example.org\nStuff",
                      ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("MATMSG:TO:srowen@example.org;SUB:Stuff;BODY:This is some text;;",
                      "srowen@example.org\nStuff\nThis is some text", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("MATMSG:SUB:Stuff;BODY:This is some text;TO:srowen@example.org;;",
                      "srowen@example.org\nStuff\nThis is some text", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("TO:srowen@example.org;SUB:Stuff;BODY:This is some text;;",
                      "TO:srowen@example.org;SUB:Stuff;BODY:This is some text;;", ParsedResultType.TEXT);
      }

      [Test]
      public void TestEmailAddressType()
      {
         DoTestResult("srowen@example.org", "srowen@example.org", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("mailto:srowen@example.org", "srowen@example.org", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("MAILTO:srowen@example.org", "srowen@example.org", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("srowen@example", "srowen@example", ParsedResultType.EMAIL_ADDRESS);
         DoTestResult("srowen", "srowen", ParsedResultType.TEXT);
         DoTestResult("Let's meet @ 2", "Let's meet @ 2", ParsedResultType.TEXT);
      }

      [Test]
      public void TestAddressBookType()
      {
         DoTestResult("MECARD:N:Sean Owen;;", "Sean Owen", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:TEL:+12125551212;N:Sean Owen;;", "Sean Owen\n+12125551212",
             ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:TEL:+12125551212;N:Sean Owen;URL:google.com;;",
             "Sean Owen\n+12125551212\ngoogle.com", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:TEL:+12125551212;N:Sean Owen;URL:google.com;EMAIL:srowen@example.org;",
             "Sean Owen\n+12125551212\nsrowen@example.org\ngoogle.com", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:ADR:76 9th Ave;N:Sean Owen;URL:google.com;EMAIL:srowen@example.org;",
             "Sean Owen\n76 9th Ave\nsrowen@example.org\ngoogle.com", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:BDAY:19760520;N:Sean Owen;URL:google.com;EMAIL:srowen@example.org;",
             "Sean Owen\nsrowen@example.org\ngoogle.com\n19760520", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:ORG:Google;N:Sean Owen;URL:google.com;EMAIL:srowen@example.org;",
             "Sean Owen\nGoogle\nsrowen@example.org\ngoogle.com", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MECARD:NOTE:ZXing Team;N:Sean Owen;URL:google.com;EMAIL:srowen@example.org;",
             "Sean Owen\nsrowen@example.org\ngoogle.com\nZXing Team", ParsedResultType.ADDRESSBOOK);
         DoTestResult("N:Sean Owen;TEL:+12125551212;;", "N:Sean Owen;TEL:+12125551212;;",
             ParsedResultType.TEXT);
      }

      [Test]
      public void TestAddressBookAuType()
      {
         DoTestResult("MEMORY:\r\n", "", ParsedResultType.ADDRESSBOOK);
         DoTestResult("MEMORY:foo\r\nNAME1:Sean\r\n", "Sean\nfoo", ParsedResultType.ADDRESSBOOK);
         DoTestResult("TEL1:+12125551212\r\nMEMORY:\r\n", "+12125551212", ParsedResultType.ADDRESSBOOK);
      }

      [Test]
      public void TestBizcard()
      {
         DoTestResult("BIZCARD:N:Sean;X:Owen;C:Google;A:123 Main St;M:+12225551212;E:srowen@example.org;",
             "Sean Owen\nGoogle\n123 Main St\n+12225551212\nsrowen@example.org", ParsedResultType.ADDRESSBOOK);
      }

      [Test]
      public void TestUpca()
      {
         DoTestResult("123456789012", "123456789012", ParsedResultType.PRODUCT, BarcodeFormat.UPC_A);
         DoTestResult("1234567890123", "1234567890123", ParsedResultType.PRODUCT, BarcodeFormat.UPC_A);
         DoTestResult("12345678901", "12345678901", ParsedResultType.TEXT);
      }

      [Test]
      public void TestUpce()
      {
         DoTestResult("01234565", "01234565", ParsedResultType.PRODUCT, BarcodeFormat.UPC_E);
      }

      [Test]
      public void TestEan()
      {
         DoTestResult("00393157", "00393157", ParsedResultType.PRODUCT, BarcodeFormat.EAN_8);
         DoTestResult("00393158", "00393158", ParsedResultType.TEXT);
         DoTestResult("5051140178499", "5051140178499", ParsedResultType.PRODUCT, BarcodeFormat.EAN_13);
         DoTestResult("5051140178490", "5051140178490", ParsedResultType.TEXT);
      }

      [Test]
      public void TestIsbn()
      {
         DoTestResult("9784567890123", "9784567890123", ParsedResultType.ISBN, BarcodeFormat.EAN_13);
         DoTestResult("9794567890123", "9794567890123", ParsedResultType.ISBN, BarcodeFormat.EAN_13);
         DoTestResult("97845678901", "97845678901", ParsedResultType.TEXT);
         DoTestResult("97945678901", "97945678901", ParsedResultType.TEXT);
      }

      [Test]
      public void TestUri()
      {
         DoTestResult("http://google.com", "http://google.com", ParsedResultType.URI);
         DoTestResult("google.com", "http://google.com", ParsedResultType.URI);
         DoTestResult("https://google.com", "https://google.com", ParsedResultType.URI);
         DoTestResult("HTTP://google.com", "HTTP://google.com", ParsedResultType.URI);
         DoTestResult("http://google.com/foobar", "http://google.com/foobar", ParsedResultType.URI);
         DoTestResult("https://google.com:443/foobar", "https://google.com:443/foobar", ParsedResultType.URI);
         DoTestResult("google.com:443", "http://google.com:443", ParsedResultType.URI);
         DoTestResult("google.com:443/", "http://google.com:443/", ParsedResultType.URI);
         DoTestResult("google.com:443/foobar", "http://google.com:443/foobar", ParsedResultType.URI);
         DoTestResult("http://google.com:443/foobar", "http://google.com:443/foobar", ParsedResultType.URI);
         DoTestResult("https://google.com:443/foobar", "https://google.com:443/foobar", ParsedResultType.URI);
         DoTestResult("ftp://google.com/fake", "ftp://google.com/fake", ParsedResultType.URI);
         DoTestResult("gopher://google.com/obsolete", "gopher://google.com/obsolete", ParsedResultType.URI);
      }

      [Test]
      public void TestGeo()
      {
         DoTestResult("geo:1,2", "1.0, 2.0", ParsedResultType.GEO);
         DoTestResult("GEO:1,2", "1.0, 2.0", ParsedResultType.GEO);
         DoTestResult("geo:1,2,3", "1.0, 2.0, 3.0m", ParsedResultType.GEO);
         DoTestResult("geo:80.33,-32.3344,3.35", "80.33, -32.3344, 3.35m", ParsedResultType.GEO);
         DoTestResult("geo", "geo", ParsedResultType.TEXT);
         DoTestResult("geography", "geography", ParsedResultType.TEXT);
      }

      [Test]
      public void TestTel()
      {
         DoTestResult("tel:+15551212", "+15551212", ParsedResultType.TEL);
         DoTestResult("TEL:+15551212", "+15551212", ParsedResultType.TEL);
         DoTestResult("tel:212 555 1212", "212 555 1212", ParsedResultType.TEL);
         DoTestResult("tel:2125551212", "2125551212", ParsedResultType.TEL);
         DoTestResult("tel:212-555-1212", "212-555-1212", ParsedResultType.TEL);
         DoTestResult("tel", "tel", ParsedResultType.TEXT);
         DoTestResult("telephone", "telephone", ParsedResultType.TEXT);
      }

      [Test]
      public void TestVCard()
      {
         DoTestResult("BEGIN:VCARD\r\nEND:VCARD", "", ParsedResultType.ADDRESSBOOK);
         DoTestResult("BEGIN:VCARD\r\nN:Owen;Sean\r\nEND:VCARD", "Sean Owen",
             ParsedResultType.ADDRESSBOOK);
         DoTestResult("BEGIN:VCARD\r\nVERSION:2.1\r\nN:Owen;Sean\r\nEND:VCARD", "Sean Owen",
             ParsedResultType.ADDRESSBOOK);
         DoTestResult("BEGIN:VCARD\r\nADR;HOME:123 Main St\r\nVERSION:2.1\r\nN:Owen;Sean\r\nEND:VCARD",
             "Sean Owen\n123 Main St", ParsedResultType.ADDRESSBOOK);
         DoTestResult("BEGIN:VCARD", "", ParsedResultType.ADDRESSBOOK);
      }

      [TestCase("BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504T123456Z\r\nDTEND:20080505T234555Z\r\nEND:VEVENT\r\nEND:VCALENDAR", "foo\nSunday, May 4, 2008 12:34:56 PM\nMonday, May 5, 2008 11:45:55 PM", ParsedResultType.CALENDAR, TestName = "VEvent: UTC times - 1")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504T123456Z\r\nDTEND:20080505T234555Z\r\nEND:VEVENT", "foo\nSunday, May 4, 2008 12:34:56 PM\nMonday, May 5, 2008 11:45:55 PM", ParsedResultType.CALENDAR, TestName = "VEvent: UTC times - 1")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504T123456\r\nDTEND:20080505T234555\r\nEND:VEVENT", "foo\nSunday, May 4, 2008 12:34:56 PM\nMonday, May 5, 2008 11:45:55 PM", ParsedResultType.CALENDAR, TestName = "VEvent: Local times")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504\r\nDTEND:20080505\r\nEND:VEVENT", "foo\nSunday, May 4, 2008\nMonday, May 5, 2008", ParsedResultType.CALENDAR, TestName = "VEvent: Date only (all day event)")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504T123456Z\r\nEND:VEVENT", "foo\nSunday, May 4, 2008 12:34:56 PM", ParsedResultType.CALENDAR, TestName = "VEvent: Start time only - 1")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504T123456\r\nEND:VEVENT", "foo\nSunday, May 4, 2008 12:34:56 PM", ParsedResultType.CALENDAR, TestName = "VEvent: Start time only - 2")]
      [TestCase("BEGIN:VEVENT\r\nSUMMARY:foo\r\nDTSTART:20080504\r\nEND:VEVENT", "foo\nSunday, May 4, 2008", ParsedResultType.CALENDAR, TestName = "VEvent: Start time only - 3")]
      [TestCase("BEGIN:VEVENT\r\nDTEND:20080505T\r\nEND:VEVENT", "BEGIN:VEVENT\r\nDTEND:20080505T\r\nEND:VEVENT", ParsedResultType.TEXT, TestName = "VEvent: Start time only - 4")]
         // Yeah, it's OK that this is thought of as maybe a URI as long as it's not CALENDAR
         // Make sure illegal entries without newlines don't crash
      [TestCase("BEGIN:VEVENTSUMMARY:EventDTSTART:20081030T122030ZDTEND:20081030T132030ZEND:VEVENT", "BEGIN:VEVENTSUMMARY:EventDTSTART:20081030T122030ZDTEND:20081030T132030ZEND:VEVENT", ParsedResultType.URI, TestName = "VEvent: Illegal entries shouldn't crash")]
      public void TestVEvent(string content, string goldenresult, ParsedResultType type)
      {
         DoTestResult(content, goldenresult, type);
      }

      [Test]
      public void TestSms()
      {
         DoTestResult("sms:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("SMS:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("sms:+15551212;via=999333", "+15551212", ParsedResultType.SMS);
         DoTestResult("sms:+15551212?subject=foo&body=bar", "+15551212\nfoo\nbar", ParsedResultType.SMS);
         DoTestResult("sms:+15551212,+12124440101", "+15551212\n+12124440101", ParsedResultType.SMS);
      }

      [Test]
      public void TestSmsto()
      {
         DoTestResult("SMSTO:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("smsto:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("smsto:+15551212:subject", "+15551212\nsubject", ParsedResultType.SMS);
         DoTestResult("smsto:+15551212:My message", "+15551212\nMy message", ParsedResultType.SMS);
         // Need to handle question mark in the subject
         DoTestResult("smsto:+15551212:What's up?", "+15551212\nWhat's up?", ParsedResultType.SMS);
         // Need to handle colon in the subject
         DoTestResult("smsto:+15551212:Directions: Do this", "+15551212\nDirections: Do this",
             ParsedResultType.SMS);
         DoTestResult("smsto:212-555-1212:Here's a longer message. Should be fine.",
             "212-555-1212\nHere's a longer message. Should be fine.",
             ParsedResultType.SMS);
      }

      [Test]
      public void TestMms()
      {
         DoTestResult("mms:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("MMS:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("mms:+15551212;via=999333", "+15551212", ParsedResultType.SMS);
         DoTestResult("mms:+15551212?subject=foo&body=bar", "+15551212\nfoo\nbar", ParsedResultType.SMS);
         DoTestResult("mms:+15551212,+12124440101", "+15551212\n+12124440101", ParsedResultType.SMS);
      }

      [Test]
      public void TestMmsto()
      {
         DoTestResult("MMSTO:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("mmsto:+15551212", "+15551212", ParsedResultType.SMS);
         DoTestResult("mmsto:+15551212:subject", "+15551212\nsubject", ParsedResultType.SMS);
         DoTestResult("mmsto:+15551212:My message", "+15551212\nMy message", ParsedResultType.SMS);
         DoTestResult("mmsto:+15551212:What's up?", "+15551212\nWhat's up?", ParsedResultType.SMS);
         DoTestResult("mmsto:+15551212:Directions: Do this", "+15551212\nDirections: Do this",
             ParsedResultType.SMS);
         DoTestResult("mmsto:212-555-1212:Here's a longer message. Should be fine.",
             "212-555-1212\nHere's a longer message. Should be fine.", ParsedResultType.SMS);
      }

      /*
      [Test]
      public void testNDEFText() {
        doTestResult(new byte[] {(byte)0xD1,(byte)0x01,(byte)0x05,(byte)0x54,
                                 (byte)0x02,(byte)0x65,(byte)0x6E,(byte)0x68,
                                 (byte)0x69},
                     ParsedResultType.TEXT);
      }

      [Test]
      public void testNDEFURI() {
        doTestResult(new byte[] {(byte)0xD1,(byte)0x01,(byte)0x08,(byte)0x55,
                                 (byte)0x01,(byte)0x6E,(byte)0x66,(byte)0x63,
                                 (byte)0x2E,(byte)0x63,(byte)0x6F,(byte)0x6D},
                     ParsedResultType.URI);
      }

      [Test]
      public void testNDEFSmartPoster() {
        doTestResult(new byte[] {(byte)0xD1,(byte)0x02,(byte)0x2F,(byte)0x53,
                                 (byte)0x70,(byte)0x91,(byte)0x01,(byte)0x0E,
                                 (byte)0x55,(byte)0x01,(byte)0x6E,(byte)0x66,
                                 (byte)0x63,(byte)0x2D,(byte)0x66,(byte)0x6F,
                                 (byte)0x72,(byte)0x75,(byte)0x6D,(byte)0x2E,
                                 (byte)0x6F,(byte)0x72,(byte)0x67,(byte)0x11,
                                 (byte)0x03,(byte)0x01,(byte)0x61,(byte)0x63,
                                 (byte)0x74,(byte)0x00,(byte)0x51,(byte)0x01,
                                 (byte)0x12,(byte)0x54,(byte)0x05,(byte)0x65,
                                 (byte)0x6E,(byte)0x2D,(byte)0x55,(byte)0x53,
                                 (byte)0x48,(byte)0x65,(byte)0x6C,(byte)0x6C,
                                 (byte)0x6F,(byte)0x2C,(byte)0x20,(byte)0x77,
                                 (byte)0x6F,(byte)0x72,(byte)0x6C,(byte)0x64},
                     ParsedResultType.NDEF_SMART_POSTER);
      }
      */

      static void DoTestResult(string contents,
                                       string goldenResult,
                                       ParsedResultType type)
      {
         DoTestResult(contents, goldenResult, type, BarcodeFormat.QR_CODE); // QR code is arbitrary
      }

      static void DoTestResult(string contents,
                                       string goldenResult,
                                       ParsedResultType type,
                                       BarcodeFormat format)
      {
         BarCodeText fakeResult = new BarCodeText(contents, null, null, null, format);
         ParsedResult result = ResultParser.ParseResult(fakeResult);
         Assert.IsNotNull(result);
         Assert.AreEqual(type, result.Type);

            string displayResult = result.DisplayResult;
         Assert.AreEqual(goldenResult, displayResult);
      }
   }
}