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

namespace ZXing.Client.Result
{
    /// <summary> Implements the "BIZCARD" address book entry format, though this has been
    /// largely reverse-engineered from examples observed in the wild -- still
    /// looking for a definitive reference.
    /// 
    /// </summary>
    /// <author>  Sean Owen
    /// </author>
    /// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source 
    /// </author>
    sealed class BizcardResultParser : AbstractDoCoMoResultParser
    {
        // Yes, we extend AbstractDoCoMoResultParser since the format is very much
        // like the DoCoMo MECARD format, but this is not technically one of
        // DoCoMo's proposed formats

        public override ParsedResult parse(BarCodeText result)
        {
            string rawText = result.Text;
            if (rawText == null || !rawText.StartsWith("BIZCARD:"))
            {
                return null;
            }
            string firstName = matchSingleDoCoMoPrefixedField("N:", rawText, true);
            string lastName = matchSingleDoCoMoPrefixedField("X:", rawText, true);
            string fullName = buildName(firstName, lastName);
            string title = matchSingleDoCoMoPrefixedField("T:", rawText, true);
            string org = matchSingleDoCoMoPrefixedField("C:", rawText, true);
            string[] addresses = matchDoCoMoPrefixedField("A:", rawText);
            string phoneNumber1 = matchSingleDoCoMoPrefixedField("B:", rawText, true);
            string phoneNumber2 = matchSingleDoCoMoPrefixedField("M:", rawText, true);
            string phoneNumber3 = matchSingleDoCoMoPrefixedField("F:", rawText, true);
            string email = matchSingleDoCoMoPrefixedField("E:", rawText, true);

            return new AddressBookParsedResult(maybeWrap(fullName),
                                               null,
                                               null,
                                               buildPhoneNumbers(phoneNumber1, phoneNumber2, phoneNumber3),
                                               null,
                                               maybeWrap(email),
                                               null,
                                               null,
                                               null,
                                               addresses,
                                               null,
                                               org,
                                               null,
                                               title,
                                               null,
                                               null);
        }

        private static string[] buildPhoneNumbers(string number1, string number2, string number3)
        {
            var numbers = new List<string>();
            if (number1 != null)
            {
                numbers.Add(number1);
            }
            if (number2 != null)
            {
                numbers.Add(number2);
            }
            if (number3 != null)
            {
                numbers.Add(number3);
            }
            var size = numbers.Count;
            if (size == 0)
            {
                return null;
            }
            return SupportClass.toStringArray(numbers);
        }

        private static string buildName(string firstName, string lastName)
        {
            if (firstName == null)
            {
                return lastName;
            }
            return lastName == null ? firstName : firstName + ' ' + lastName;
        }
    }
}