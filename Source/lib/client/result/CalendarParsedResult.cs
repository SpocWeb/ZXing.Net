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

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Represents a parsed result that encodes a calendar event at a certain time, optionally with attendees and a location.
    /// </summary>
    ///<author>Sean Owen</author>
    public sealed class CalendarParsedResult : ParsedResult
    {
        private static readonly Regex RFC2445_DURATION =
           new Regex(@"\A(?:" + "P(?:(\\d+)W)?(?:(\\d+)D)?(?:T(?:(\\d+)H)?(?:(\\d+)M)?(?:(\\d+)S)?)?" + @")\z"
#if !(SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE || UNITY || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2)
            , RegexOptions.Compiled);
#else
);
#endif

        private static readonly long[] RFC2445_DURATION_FIELD_UNITS =
        {
         7*24*60*60*1000L, // 1 week
         24*60*60*1000L, // 1 day
         60*60*1000L, // 1 hour
         60*1000L, // 1 minute
         1000L, // 1 second
      };

        private static readonly Regex DATE_TIME = new Regex(@"\A(?:" + "[0-9]{8}(T[0-9]{6}Z?)?" + @")\z"
#if !(SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE || UNITY || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2)
         , RegexOptions.Compiled);
#else
);
#endif

        private readonly bool startAllDay;

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="summary"></param>
        /// <param name="startString"></param>
        /// <param name="endString"></param>
        /// <param name="durationString"></param>
        /// <param name="location"></param>
        /// <param name="organizer"></param>
        /// <param name="attendees"></param>
        /// <param name="description"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public CalendarParsedResult(string summary,
           string startString,
           string endString,
           string durationString,
           string location,
           string organizer,
           string[] attendees,
           string description,
           double latitude,
           double longitude)
           : base(ParsedResultType.CALENDAR)
        {
            Summary = summary;
            try
            {
                Start = parseDate(startString);
            }
            catch (Exception pe)
            {
                throw new ArgumentException(pe.ToString());
            }

            if (endString == null)
            {
                long durationMS = parseDurationMS(durationString);
                End = durationMS < 0L ? null : (DateTime?)Start + new TimeSpan(0, 0, 0, 0, (int)durationMS);
            }
            else
            {
                try
                {
                    End = parseDate(endString);
                }
                catch (Exception pe)
                {
                    throw new ArgumentException(pe.ToString());
                }
            }

            startAllDay = startString.Length == 8;
            isEndAllDay = endString != null && endString.Length == 8;

            Location = location;
            Organizer = organizer;
            Attendees = attendees;
            Description = description;
            Latitude = latitude;
            Longitude = longitude;

            var result = new StringBuilder(100);
            maybeAppend(summary, result);
            maybeAppend(format(startAllDay, Start), result);
            maybeAppend(format(isEndAllDay, End), result);
            maybeAppend(location, result);
            maybeAppend(organizer, result);
            maybeAppend(attendees, result);
            maybeAppend(description, result);
            displayResultValue = result.ToString();
        }
        /// <summary>
        /// summary
        /// </summary>
        public string Summary { get; }

        /// <summary>
        /// Gets the start.
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Determines whether [is start all day].
        /// </summary>
        /// <returns>if start time was specified as a whole day</returns>
        public bool isStartAllDay()
        {
            return startAllDay;
        }

        /// <summary>
        /// event end <see cref="DateTime"/>, or null if event has no duration
        /// </summary>
        public DateTime? End { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is end all day.
        /// </summary>
        /// <value>true if end time was specified as a whole day</value>
        public bool isEndAllDay { get; }

        /// <summary>
        /// location
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// organizer
        /// </summary>
        public string Organizer { get; }

        /// <summary>
        /// attendees
        /// </summary>
        public string[] Attendees { get; }

        /// <summary>
        /// description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// latitude
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// longitude
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Parses a string as a date. RFC 2445 allows the start and end fields to be of type DATE (e.g. 20081021)
        /// or DATE-TIME (e.g. 20081021T123000 for local time, or 20081021T123000Z for UTC).
        /// </summary>
        /// <param name="when">The string to parse</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if not a date formatted string</exception>
        private static DateTime parseDate(string when)
        {
            if (!DATE_TIME.Match(when).Success)
            {
                throw new ArgumentException(string.Format("no date format: {0}", when));
            }
            if (when.Length == 8)
            {
                // Show only year/month/day
                // For dates without a time, for purposes of interacting with Android, the resulting timestamp
                // needs to be midnight of that day in GMT. See:
                // http://code.google.com/p/android/issues/detail?id=8330
                // format.setTimeZone(TimeZone.getTimeZone("GMT"));
                return DateTime.ParseExact(when, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            // The when string can be local time, or UTC if it ends with a Z
            if (when.Length == 16 && when[15] == 'Z')
            {
                var milliseconds = parseDateTimeString(when.Substring(0, 15));
                //Calendar calendar = new GregorianCalendar();
                // Account for time zone difference
                //milliseconds += calendar.get(Calendar.ZONE_OFFSET);
                // Might need to correct for daylight savings time, but use target time since
                // now might be in DST but not then, or vice versa
                //calendar.setTime(new Date(milliseconds));
                //return milliseconds + calendar.get(Calendar.DST_OFFSET);
                milliseconds = TimeZoneInfo.ConvertTime(milliseconds, TimeZoneInfo.Local);
                return milliseconds;
            }
            return parseDateTimeString(when);
        }

        private static string format(bool allDay, DateTime? date)
        {
            if (date == null)
            {
                return null;
            }
            if (allDay) {
                return date.Value.ToString("D", CultureInfo.CurrentCulture);
            }
            return date.Value.ToString("F", CultureInfo.CurrentCulture);
        }

        private static long parseDurationMS(string durationString)
        {
            if (durationString == null)
            {
                return -1L;
            }
            var m = RFC2445_DURATION.Match(durationString);
            if (!m.Success)
            {
                return -1L;
            }
            long durationMS = 0L;
            for (int i = 0; i < RFC2445_DURATION_FIELD_UNITS.Length; i++)
            {
                string fieldValue = m.Groups[i + 1].Value;
                if (!string.IsNullOrEmpty(fieldValue))
                {
                    durationMS += RFC2445_DURATION_FIELD_UNITS[i] * int.Parse(fieldValue);
                }
            }
            return durationMS;
        }

        private static DateTime parseDateTimeString(string dateTimeString)
        {
            return DateTime.ParseExact(dateTimeString, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture);
        }
    }
}