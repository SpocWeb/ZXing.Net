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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Parses contact information formatted according to the VCard (2.1) format. This is not a complete
    /// implementation but should parse information as commonly encoded in 2D barcodes.
    /// </summary>
    /// <author>Sean Owen</author>
    sealed class VCardResultParser : ResultParser
    {
#if SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE || UNITY || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2
        private static readonly Regex BEGIN_VCARD = new Regex("BEGIN:VCARD", RegexOptions.IgnoreCase);
        private static readonly Regex VCARD_LIKE_DATE = new Regex(@"\A(?:" + "\\d{4}-?\\d{2}-?\\d{2}" + @")\z");
        private static readonly Regex CR_LF_SPACE_TAB = new Regex("\r\n[ \t]");
        private static readonly Regex NEWLINE_ESCAPE = new Regex("\\\\[nN]");
        private static readonly Regex VCARD_ESCAPES = new Regex("\\\\([,;\\\\])");
        private static readonly Regex EQUALS = new Regex("=");
        private static readonly Regex SEMICOLON = new Regex(";");
        private static readonly Regex UNESCAPED_SEMICOLONS = new Regex("(?<!\\\\);+");
        private static readonly Regex COMMA = new Regex(",");
        private static readonly Regex SEMICOLON_OR_COMMA = new Regex("[;,]");
#else
      private static readonly Regex BEGIN_VCARD = new Regex("BEGIN:VCARD", RegexOptions.Compiled | RegexOptions.IgnoreCase);
      private static readonly Regex VCARD_LIKE_DATE = new Regex(@"\A(?:" + "\\d{4}-?\\d{2}-?\\d{2}" + @")\z", RegexOptions.Compiled);
      private static readonly Regex CR_LF_SPACE_TAB = new Regex("\r\n[ \t]", RegexOptions.Compiled);
      private static readonly Regex NEWLINE_ESCAPE = new Regex("\\\\[nN]", RegexOptions.Compiled);
      private static readonly Regex VCARD_ESCAPES = new Regex("\\\\([,;\\\\])", RegexOptions.Compiled);
      private static readonly Regex EQUALS = new Regex("=", RegexOptions.Compiled);
      private static readonly Regex SEMICOLON = new Regex(";", RegexOptions.Compiled);
      private static readonly Regex UNESCAPED_SEMICOLONS = new Regex("(?<!\\\\);+", RegexOptions.Compiled);
      private static readonly Regex COMMA = new Regex(",", RegexOptions.Compiled);
      private static readonly Regex SEMICOLON_OR_COMMA = new Regex("[;,]", RegexOptions.Compiled);
#endif

        public override ParsedResult parse(BarCodeText result)
        {
            // Although we should insist on the raw text ending with "END:VCARD", there's no reason
            // to throw out everything else we parsed just because this was omitted. In fact, Eclair
            // is doing just that, and we can't parse its contacts without this leniency.
            string rawText = result.Text;
            var m = BEGIN_VCARD.Match(rawText);
            if (!m.Success || m.Index != 0)
            {
                return null;
            }
            List<List<string>> names = matchVCardPrefixedField("FN", rawText, true, false);
            if (names == null)
            {
                // If no display names found, look for regular name fields and format them
                names = matchVCardPrefixedField("N", rawText, true, false);
                formatNames(names);
            }
            List<string> nicknameString = matchSingleVCardPrefixedField("NICKNAME", rawText, true, false);
            string[] nicknames = nicknameString == null ? null : COMMA.Split(nicknameString[0]);
            List<List<string>> phoneNumbers = matchVCardPrefixedField("TEL", rawText, true, false);
            List<List<string>> emails = matchVCardPrefixedField("EMAIL", rawText, true, false);
            List<string> note = matchSingleVCardPrefixedField("NOTE", rawText, false, false);
            List<List<string>> addresses = matchVCardPrefixedField("ADR", rawText, true, true);
            List<string> org = matchSingleVCardPrefixedField("ORG", rawText, true, true);
            List<string> birthday = matchSingleVCardPrefixedField("BDAY", rawText, true, false);
            if (birthday != null && !isLikeVCardDate(birthday[0]))
            {
                birthday = null;
            }
            List<string> title = matchSingleVCardPrefixedField("TITLE", rawText, true, false);
            List<List<string>> urls = matchVCardPrefixedField("URL", rawText, true, false);
            List<string> instantMessenger = matchSingleVCardPrefixedField("IMPP", rawText, true, false);
            List<string> geoString = matchSingleVCardPrefixedField("GEO", rawText, true, false);
            string[] geo = geoString == null ? null : SEMICOLON_OR_COMMA.Split(geoString[0]);
            if (geo != null && geo.Length != 2)
            {
                geo = null;
            }
            return new AddressBookParsedResult(toPrimaryValues(names),
                                               nicknames,
                                               null,
                                               toPrimaryValues(phoneNumbers),
                                               toTypes(phoneNumbers),
                                               toPrimaryValues(emails),
                                               toTypes(emails),
                                               toPrimaryValue(instantMessenger),
                                               toPrimaryValue(note),
                                               toPrimaryValues(addresses),
                                               toTypes(addresses),
                                               toPrimaryValue(org),
                                               toPrimaryValue(birthday),
                                               toPrimaryValue(title),
                                               toPrimaryValues(urls),
                                               geo);
        }

        public static List<List<string>> matchVCardPrefixedField(string prefix,
                                                          string rawText,
                                                          bool trim,
                                                          bool parseFieldDivider)
        {
            List<List<string>> matches = null;
            int i = 0;
            int max = rawText.Length;

            while (i < max)
            {
                // At start or after newline, match prefix, followed by optional metadata 
                // (led by ;) ultimately ending in colon
                var matcher = new Regex("(?:^|\n)" + prefix + "(?:;([^:]*))?:", RegexOptions.IgnoreCase);

                if (i > 0)
                {
                    i--; // Find from i-1 not i since looking at the preceding character
                }
                var match = matcher.Match(rawText, i);
                if (!match.Success)
                {
                    break;
                }
                i = match.Index + match.Length;

                string metadataString = match.Groups[1].Value; // group 1 = metadata substring
                List<string> metadata = null;
                bool quotedPrintable = false;
                string quotedPrintableCharset = null;
                string valueType = null;
                if (metadataString != null)
                {
                    foreach (string metadatum in SEMICOLON.Split(metadataString))
                    {
                        if (metadata == null)
                        {
                            metadata = new List<string>(1);
                        }
                        metadata.Add(metadatum);
                        string[] metadatumTokens = EQUALS.Split(metadatum, 2);
                        if (metadatumTokens.Length > 1)
                        {
                            string key = metadatumTokens[0];
                            string value = metadatumTokens[1];
                            if (string.Compare("ENCODING", key, StringComparison.OrdinalIgnoreCase) == 0 &&
                                string.Compare("QUOTED-PRINTABLE", value, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                quotedPrintable = true;
                            }
                            else if (string.Compare("CHARSET", key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                quotedPrintableCharset = value;
                            }
                            else if (string.Compare("VALUE", key, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                valueType = value;
                            }
                        }
                    }
                }

                int matchStart = i; // Found the start of a match here

                while ((i = rawText.IndexOf('\n', i)) >= 0)
                {
                    // Really, end in \r\n
                    if (i < rawText.Length - 1 && // But if followed by tab or space,
                        (rawText[i + 1] == ' ' || // this is only a continuation
                         rawText[i + 1] == '\t'))
                    {
                        i += 2; // Skip \n and continutation whitespace
                    }
                    else if (quotedPrintable && // If preceded by = in quoted printable
                             ((i >= 1 && rawText[i - 1] == '=') || // this is a continuation
                              (i >= 2 && rawText[i - 2] == '=')))
                    {
                        i++; // Skip \n
                    }
                    else
                    {
                        break;
                    }
                }

                if (i < 0)
                {
                    // No terminating end character? uh, done. Set i such that loop terminates and break
                    i = max;
                }
                else if (i > matchStart)
                {
                    // found a match
                    if (matches == null)
                    {
                        matches = new List<List<string>>(1); // lazy init
                    }
                    if (i >= 1 && rawText[i - 1] == '\r')
                    {
                        i--; // Back up over \r, which really should be there
                    }
                    string element = rawText.Substring(matchStart, i - matchStart);
                    if (trim)
                    {
                        element = element.Trim();
                    }
                    if (quotedPrintable)
                    {
                        element = decodeQuotedPrintable(element, quotedPrintableCharset);
                        if (parseFieldDivider)
                        {
                            element = UNESCAPED_SEMICOLONS.Replace(element, "\n").Trim();
                        }
                    }
                    else
                    {
                        if (parseFieldDivider)
                        {
                            element = UNESCAPED_SEMICOLONS.Replace(element, "\n").Trim();
                        }
                        element = CR_LF_SPACE_TAB.Replace(element, "");
                        element = NEWLINE_ESCAPE.Replace(element, "\n");
                        element = VCARD_ESCAPES.Replace(element, "$1");
                    }
                    // Only handle VALUE=uri specially
                    if ("uri".Equals(valueType))
                    {
                        // Don't actually support dereferencing URIs, but use scheme-specific part not URI
                        // as value, to support tel: and mailto:
                        try
                        {
                            Uri uri;
                            if (Uri.TryCreate(element, UriKind.RelativeOrAbsolute, out uri))
                            {
                                element = uri.AbsoluteUri.Replace(uri.Scheme + ':', "");
                            }
                        }
                        catch (Exception)
                        {
                            // ignore
                        }
                    }
                    if (metadata == null)
                    {
                        var matched = new List<string>(1)
                        {
                            element
                        };
                        matches.Add(matched);
                    }
                    else
                    {
                        metadata.Insert(0, element);
                        matches.Add(metadata);
                    }
                    i++;
                }
                else
                {
                    i++;
                }

            }

            return matches;
        }

        private static string decodeQuotedPrintable(string value, string charset)
        {
            int length = value.Length;
            var result = new StringBuilder(length);
            var fragmentBuffer = new MemoryStream();
            for (int i = 0; i < length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '\r':
                    case '\n':
                        break;
                    case '=':
                        if (i < length - 2)
                        {
                            char nextChar = value[i + 1];
                            if (nextChar == '\r' || nextChar == '\n')
                            {
                                // Ignore, it's just a continuation symbol
                            }
                            else
                            {
                                char nextNextChar = value[i + 2];
                                int firstDigit = parseHexDigit(nextChar);
                                int secondDigit = parseHexDigit(nextNextChar);
                                if (firstDigit >= 0 && secondDigit >= 0)
                                {
                                    fragmentBuffer.WriteByte((byte)((firstDigit << 4) | secondDigit));
                                } // else ignore it, assume it was incorrectly encoded
                                i += 2;
                            }
                        }
                        break;
                    default:
                        maybeAppendFragment(fragmentBuffer, charset, result);
                        result.Append(c);
                        break;
                }
            }
            maybeAppendFragment(fragmentBuffer, charset, result);
            return result.ToString();
        }

        private static void maybeAppendFragment(MemoryStream fragmentBuffer,
                                                string charset,
                                                StringBuilder result)
        {
            if (fragmentBuffer.Length > 0)
            {
                byte[] fragmentBytes = fragmentBuffer.ToArray();
                string fragment;
                if (charset == null)
                {
#if WindowsCE
               fragment = Encoding.Default.GetString(fragmentBytes, 0, fragmentBytes.Length);
#else
                    fragment = Encoding.UTF8.GetString(fragmentBytes, 0, fragmentBytes.Length);
#endif
                }
                else
                {
                    try
                    {
                        fragment = Encoding.GetEncoding(charset).GetString(fragmentBytes, 0, fragmentBytes.Length);
                    }
                    catch (Exception)
                    {
#if WindowsCE
                  // WindowsCE doesn't support all encodings. But it is device depended.
                  // So we try here the some different ones
                  if (charset == "ISO-8859-1")
                  {
                     fragment = Encoding.GetEncoding(1252).GetString(fragmentBytes, 0, fragmentBytes.Length);
                  }
                  else
                  {
                     fragment = Encoding.Default.GetString(fragmentBytes, 0, fragmentBytes.Length);
                  }
                  fragment = Encoding.Default.GetString(fragmentBytes, 0, fragmentBytes.Length);
#else
                        fragment = Encoding.UTF8.GetString(fragmentBytes, 0, fragmentBytes.Length);
#endif
                    }
                }
                fragmentBuffer.Seek(0, SeekOrigin.Begin);
                fragmentBuffer.SetLength(0);
                result.Append(fragment);
            }
        }

        internal static List<string> matchSingleVCardPrefixedField(string prefix,
                                                      string rawText,
                                                      bool trim,
                                                      bool parseFieldDivider)
        {
            List<List<string>> values = matchVCardPrefixedField(prefix, rawText, trim, parseFieldDivider);
            return values == null || values.Count == 0 ? null : values[0];
        }

        private static string toPrimaryValue(List<string> list)
        {
            return list == null || list.Count == 0 ? null : list[0];
        }

        private static string[] toPrimaryValues(ICollection<List<string>> lists)
        {
            if (lists == null || lists.Count == 0)
            {
                return null;
            }
            var result = new List<string>(lists.Count);
            foreach (var list in lists)
            {
                string value = list[0];
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
            return SupportClass.toStringArray(result);
        }

        private static string[] toTypes(ICollection<List<string>> lists)
        {
            if (lists == null || lists.Count == 0)
            {
                return null;
            }
            List<string> result = new List<string>(lists.Count);
            foreach (var list in lists)
            {
                string value = list[0];
                if (!string.IsNullOrEmpty(value))
                {
                    string type = null;
                    for (int i = 1; i < list.Count; i++)
                    {
                        string metadatum = list[i];
                        int equals = metadatum.IndexOf('=');
                        if (equals < 0)
                        {
                            // take the whole thing as a usable label
                            type = metadatum;
                            break;
                        }
                        if (string.Compare("TYPE", metadatum.Substring(0, equals), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            type = metadatum.Substring(equals + 1);
                            break;
                        }
                    }
                    result.Add(type);
                }
            }
            return SupportClass.toStringArray(result);
        }

        private static bool isLikeVCardDate(string value)
        {
            return value == null || VCARD_LIKE_DATE.Match(value).Success;
        }

        /**
         * Formats name fields of the form "Public;John;Q.;Reverend;III" into a form like
         * "Reverend John Q. Public III".
         *
         * @param names name values to format, in place
         */
        private static void formatNames(IEnumerable<List<string>> names)
        {
            if (names != null)
            {
                foreach (var list in names)
                {
                    string name = list[0];
                    string[] components = new string[5];
                    int start = 0;
                    int end;
                    int componentIndex = 0;
                    while (componentIndex < components.Length - 1 && (end = name.IndexOf(';', start)) >= 0)
                    {
                        components[componentIndex] = name.Substring(start, end - start);


                        componentIndex++;
                        start = end + 1;
                    }
                    components[componentIndex] = name.Substring(start);
                    StringBuilder newName = new StringBuilder(100);
                    maybeAppendComponent(components, 3, newName);
                    maybeAppendComponent(components, 1, newName);
                    maybeAppendComponent(components, 2, newName);
                    maybeAppendComponent(components, 0, newName);
                    maybeAppendComponent(components, 4, newName);
                    list.Insert(0, newName.ToString().Trim());
                }
            }
        }

        private static void maybeAppendComponent(string[] components, int i, StringBuilder newName)
        {
            if (!string.IsNullOrEmpty(components[i]))
            {
                if (newName.Length > 0)
                {
                    newName.Append(' ');
                }
                newName.Append(components[i]);
            }
        }
    }
}