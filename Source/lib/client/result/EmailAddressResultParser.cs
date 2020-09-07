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

using System.Text.RegularExpressions;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Represents a result that encodes an e-mail address, either as a plain address
    /// like "joe@example.org" or a mailto: URL like "mailto:joe@example.org".
    /// </summary>
    /// <author>Sean Owen</author>
    internal sealed class EmailAddressResultParser : ResultParser
    {

        static readonly Regex COMMA = new Regex(","
#if !(SILVERLIGHT4 || SILVERLIGHT5 || NETFX_CORE || PORTABLE || UNITY || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2)
                                             , RegexOptions.Compiled);
#else
);
#endif

        public override ParsedResult parse(BarCodeText result)
        {
            string rawText = result.Text;
            if (rawText == null)
            {
                return null;
            }

            if (rawText.ToLower().StartsWith("mailto:"))
            {
                // If it starts with mailto:, assume it is definitely trying to be an email address
                string hostEmail = rawText.Substring(7);
                int queryStart = hostEmail.IndexOf('?');
                if (queryStart >= 0)
                {
                    hostEmail = hostEmail.Substring(0, queryStart);
                }
                hostEmail = urlDecode(hostEmail);
                string[] tos = null;
                if (!string.IsNullOrEmpty(hostEmail))
                {
                    tos = COMMA.Split(hostEmail);
                }
                var nameValues = parseNameValuePairs(rawText);
                string[] ccs = null;
                string[] bccs = null;
                string subject = null;
                string body = null;
                if (nameValues != null)
                {
                    if (tos == null)
                    {
                        if (nameValues.TryGetValue("to", out var tosString) && tosString != null)
                        {
                            tos = COMMA.Split(tosString);
                        }
                    }
                    if (nameValues.TryGetValue("cc", out var ccString) && ccString != null)
                    {
                        ccs = COMMA.Split(ccString);
                    }
                    if (nameValues.TryGetValue("bcc", out var bccString) && bccString != null)
                    {
                        bccs = COMMA.Split(bccString);
                    }
                    nameValues.TryGetValue("subject", out subject);
                    nameValues.TryGetValue("body", out body);
                }
                return new EmailAddressParsedResult(tos, ccs, bccs, subject, body);
            }
            if (!EmailDoCoMoResultParser.isBasicallyValidEmailAddress(rawText))
            {
                return null;
            }
            return new EmailAddressParsedResult(rawText);
        }
    }
}