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
   /// Tests <see cref="CalendarParsedResult" />.
   ///
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class CalendarParsedResultTestCase
   {

       const double EPSILON = 1.0E-10;

       const string DATE_TIME_FORMAT = "yyyyMMdd'T'HHmmss'Z'";

      [SetUp]
      public void SetUp()
      {
         // Locale.setDefault(Locale.ENGLISH);
         // TimeZone.setDefault(TimeZone.getTimeZone("GMT"));
         Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
      }

      [Test]
      public void TestStartEnd()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "DTEND:20080505T234555Z\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, null, "20080504T123456Z", "20080505T234555Z");
      }

      [Test]
      public void TestNoVCalendar()
      {
         DoTest(
             "BEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "DTEND:20080505T234555Z\r\n" +
             "END:VEVENT",
             null, null, null, "20080504T123456Z", "20080505T234555Z");
      }

      [Test]
      public void TestStart()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, null, "20080504T123456Z", null);
      }

      [Test]
      public void TestDuration()
      {
         DoTest(
            "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
            "DTSTART:20080504T123456Z\r\n" +
            "DURATION:P1D\r\n" +
            "END:VEVENT\r\nEND:VCALENDAR",
            null, null, null, "20080504T123456Z", "20080505T123456Z");
         DoTest(
            "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
            "DTSTART:20080504T123456Z\r\n" +
            "DURATION:P1DT2H3M4S\r\n" +
            "END:VEVENT\r\nEND:VCALENDAR",
            null, null, null, "20080504T123456Z", "20080505T143800Z");
      }

      [Test]
      public void TestSummary()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "SUMMARY:foo\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, "foo", null, "20080504T123456Z", null);
      }

      [Test]
      public void TestLocation()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "LOCATION:Miami\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, "Miami", "20080504T123456Z", null);
      }

      [Test]
      public void TestDescription()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "DESCRIPTION:This is a test\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             "This is a test", null, null, "20080504T123456Z", null);
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "DESCRIPTION:This is a test\r\n\t with a continuation\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             "This is a test with a continuation", null, null, "20080504T123456Z", null);
      }

      [Test]
      public void TestGeo()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "GEO:-12.345;-45.678\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, null, "20080504T123456Z", null, null, null, -12.345, -45.678);
      }

      [Test]
      public void TestBadGeo()
      {
         // Not parsed as VEVENT
         var fakeResult = new ZXing.BarCodeText("BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
                                           "GEO:-12.345\r\n" +
                                           "END:VEVENT\r\nEND:VCALENDAR", null, null, null, BarcodeFormat.QR_CODE);
         var result = ResultParser.parseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.TEXT, result.Type);
      }

      [Test]
      public void TestOrganizer()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "ORGANIZER:mailto:bob@example.org\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, null, "20080504T123456Z", null, "bob@example.org", null, double.NaN, double.NaN);
      }

      [Test]
      public void TestAttendees()
      {
         DoTest(
             "BEGIN:VCALENDAR\r\nBEGIN:VEVENT\r\n" +
             "DTSTART:20080504T123456Z\r\n" +
             "ATTENDEE:mailto:bob@example.org\r\n" +
             "ATTENDEE:mailto:alice@example.org\r\n" +
             "END:VEVENT\r\nEND:VCALENDAR",
             null, null, null, "20080504T123456Z", null, null,
             new string[] { "bob@example.org", "alice@example.org" }, double.NaN, double.NaN);
      }

      [Test]
      public void TestVEventEscapes()
      {
         DoTest("BEGIN:VEVENT\n" +
                "CREATED:20111109T110351Z\n" +
                "LAST-MODIFIED:20111109T170034Z\n" +
                "DTSTAMP:20111109T170034Z\n" +
                "UID:0f6d14ef-6cb7-4484-9080-61447ccdf9c2\n" +
                "SUMMARY:Summary line\n" +
                "CATEGORIES:Private\n" +
                "DTSTART;TZID=Europe/Vienna:20111110T110000\n" +
                "DTEND;TZID=Europe/Vienna:20111110T120000\n" +
                "LOCATION:Location\\, with\\, escaped\\, commas\n" +
                "DESCRIPTION:Meeting with a friend\\nlook at homepage first\\n\\n\n" +
                "  \\n\n" +
                "SEQUENCE:1\n" +
                "X-MOZ-GENERATION:1\n" +
                "END:VEVENT",
                "Meeting with a friend\nlook at homepage first\n\n\n  \n",
                "Summary line",
                "Location, with, escaped, commas",
                "20111110T110000Z",
                "20111110T120000Z");
      }

      [Test]
      public void TestAllDayValueDate()
      {
         DoTest("BEGIN:VEVENT\n" +
                "DTSTART;VALUE=DATE:20111110\n" +
                "DTEND;VALUE=DATE:20111110\n" +
                "END:VEVENT",
                null, null, null, "20111110T000000Z", "20111110T000000Z");
      }

      static void DoTest(string contents,
                                 string description,
                                 string summary,
                                 string location,
                                 string startString,
                                 string endString)
      {
         DoTest(contents, description, summary, location, startString, endString, null, null, double.NaN, double.NaN);
      }

      static void DoTest(string contents,
                                 string description,
                                 string summary,
                                 string location,
                                 string startString,
                                 string endString,
                                 string organizer,
                                 string[] attendees,
                                 double latitude,
                                 double longitude)
      {
         var fakeResult = new ZXing.BarCodeText(contents, null, null, null, BarcodeFormat.QR_CODE);
         var result = ResultParser.parseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.CALENDAR, result.Type);
         var calResult = (CalendarParsedResult)result;
         Assert.AreEqual(description, calResult.Description);
         Assert.AreEqual(summary, calResult.Summary);
         Assert.AreEqual(location, calResult.Location);
         Assert.AreEqual(startString, calResult.Start.ToString(DATE_TIME_FORMAT));
         Assert.AreEqual(endString, calResult.End?.ToString(DATE_TIME_FORMAT));
         Assert.AreEqual(organizer, calResult.Organizer);
         Assert.IsTrue(AddressBookParsedResultTestCase.AreEqual(attendees, calResult.Attendees));
         AssertEqualOrNaN(latitude, calResult.Latitude);
         AssertEqualOrNaN(longitude, calResult.Longitude);
      }

      static void AssertEqualOrNaN(double expected, double actual)
      {
         if (double.IsNaN(expected))
         {
            Assert.IsTrue(double.IsNaN(actual));
         }
         else
         {
            Assert.AreEqual(expected, actual, EPSILON);
         }
      }
   }
}