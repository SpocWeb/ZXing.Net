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

using System.Text;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Represents a parsed result that encodes an SMS message, including recipients, subject and body text.
    /// </summary>
    /// <author>Sean Owen</author>
    public sealed class SMSParsedResult : ParsedResult
    {
        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="number"></param>
        /// <param name="via"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public SMSParsedResult(string number,
                               string via,
                               string subject,
                               string body)
           : this(new[] { number }, new[] { via }, subject, body)
        {
        }
        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="vias"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public SMSParsedResult(string[] numbers,
                               string[] vias,
                               string subject,
                               string body)
           : base(ParsedResultType.SMS)
        {
            Numbers = numbers;
            Vias = vias;
            Subject = subject;
            Body = body;
            SMSURI = getSMSURI();

            var result = new StringBuilder(100);
            maybeAppend(Numbers, result);
            maybeAppend(Subject, result);
            maybeAppend(Body, result);
            displayResultValue = result.ToString();
        }

        string getSMSURI()
        {
            var result = new StringBuilder();
            result.Append("sms:");
            bool first = true;
            for (int i = 0; i < Numbers.Length; i++)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    result.Append(',');
                }
                result.Append(Numbers[i]);
                if (Vias != null && Vias[i] != null)
                {
                    result.Append(";via=");
                    result.Append(Vias[i]);
                }
            }
            bool hasBody = Body != null;
            bool hasSubject = Subject != null;
            if (hasBody || hasSubject)
            {
                result.Append('?');
                if (hasBody)
                {
                    result.Append("body=");
                    result.Append(Body);
                }
                if (hasSubject)
                {
                    if (hasBody)
                    {
                        result.Append('&');
                    }
                    result.Append("subject=");
                    result.Append(Subject);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// numbers
        /// </summary>
        public string[] Numbers { get; }
        /// <summary>
        ///  vias
        /// </summary>
        public string[] Vias { get; }
        /// <summary>
        /// subject
        /// </summary>
        public string Subject { get; }
        /// <summary>
        /// body
        /// </summary>
        public string Body { get; }
        /// <summary>
        /// sms uri
        /// </summary>
        public string SMSURI { get; }
    }
}