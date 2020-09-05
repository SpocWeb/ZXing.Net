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
using System.Collections.Generic;
using System.Globalization;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Partially implements the iCalendar format's "VEVENT" format for specifying a
    /// calendar event. See RFC 2445. This supports SUMMARY, DTSTART and DTEND fields.
    /// </summary>
    /// <author>  Sean Owen
    /// </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    sealed class VEventResultParser : ResultParser
    {
        public override ParsedResult parse(BarCodeText result)
        {
            string rawText = result.Text;
            if (rawText == null)
            {
                return null;
            }
            int vEventStart = rawText.IndexOf("BEGIN:VEVENT");
            if (vEventStart < 0)
            {
                return null;
            }

            string summary = matchSingleVCardPrefixedField("SUMMARY", rawText);
            string start = matchSingleVCardPrefixedField("DTSTART", rawText);
            if (start == null)
            {
                return null;
            }
            string end = matchSingleVCardPrefixedField("DTEND", rawText);
            string duration = matchSingleVCardPrefixedField("DURATION", rawText);
            string location = matchSingleVCardPrefixedField("LOCATION", rawText);
            string organizer = stripMailto(matchSingleVCardPrefixedField("ORGANIZER", rawText));

            string[] attendees = matchVCardPrefixedField("ATTENDEE", rawText);
            if (attendees != null)
            {
                for (int i = 0; i < attendees.Length; i++)
                {
                    attendees[i] = stripMailto(attendees[i]);
                }
            }
            string description = matchSingleVCardPrefixedField("DESCRIPTION", rawText);

            string geoString = matchSingleVCardPrefixedField("GEO", rawText);
            double latitude;
            double longitude;
            if (geoString == null)
            {
                latitude = double.NaN;
                longitude = double.NaN;
            }
            else
            {
                int semicolon = geoString.IndexOf(';');
                if (semicolon < 0)
                {
                    return null;
                }
#if WindowsCE
            try { latitude = Double.Parse(geoString.Substring(0, semicolon), NumberStyles.Float, CultureInfo.InvariantCulture); }
            catch { return null; }
            try { longitude = Double.Parse(geoString.Substring(semicolon + 1), NumberStyles.Float, CultureInfo.InvariantCulture); }
            catch { return null; }
#else
                if (!double.TryParse(geoString.Substring(0, semicolon), NumberStyles.Float, CultureInfo.InvariantCulture, out latitude))
                    return null;
                if (!double.TryParse(geoString.Substring(semicolon + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out longitude))
                    return null;
#endif
            }

            try
            {
                return new CalendarParsedResult(summary,
                                                start,
                                                end,
                                                duration,
                                                location,
                                                organizer,
                                                attendees,
                                                description,
                                                latitude,
                                                longitude);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string matchSingleVCardPrefixedField(string prefix,
                                                            string rawText)
        {
            var values = VCardResultParser.matchSingleVCardPrefixedField(prefix, rawText, true, false);
            return values == null || values.Count == 0 ? null : values[0];
        }

        private static string[] matchVCardPrefixedField(string prefix, string rawText)
        {
            List<List<string>> values = VCardResultParser.matchVCardPrefixedField(prefix, rawText, true, false);
            if (values == null || values.Count == 0)
            {
                return null;
            }
            int size = values.Count;
            string[] result = new string[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = values[i][0];
            }
            return result;
        }

        private static string stripMailto(string s)
        {
            if (s != null && (s.StartsWith("mailto:") || s.StartsWith("MAILTO:")))
            {
                s = s.Substring(7);
            }
            return s;
        }
    }
}
