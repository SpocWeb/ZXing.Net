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

using System;
using System.Text;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Represents a parsed result that encodes contact information, like that in an address book entry.
    /// </summary>
    /// <author>Sean Owen</author>
    public sealed class AddressBookParsedResult : ParsedResult
    {
        private readonly string[] names;
        private readonly string[] nicknames;
        private readonly string pronunciation;
        private readonly string[] phoneNumbers;
        private readonly string[] phoneTypes;
        private readonly string[] emails;
        private readonly string[] emailTypes;
        private readonly string instantMessenger;
        private readonly string note;
        private readonly string[] addresses;
        private readonly string[] addressTypes;
        private readonly string org;
        private readonly string birthday;
        private readonly string title;
        private readonly string[] urls;
        private readonly string[] geo;

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="names"></param>
        /// <param name="phoneNumbers"></param>
        /// <param name="phoneTypes"></param>
        /// <param name="emails"></param>
        /// <param name="emailTypes"></param>
        /// <param name="addresses"></param>
        /// <param name="addressTypes"></param>
        public AddressBookParsedResult(string[] names,
                                   string[] phoneNumbers,
                                   string[] phoneTypes,
                                   string[] emails,
                                   string[] emailTypes,
                                   string[] addresses,
                                   string[] addressTypes)
           : this(names,
             null,
             null,
             phoneNumbers,
             phoneTypes,
             emails,
             emailTypes,
             null,
             null,
             addresses,
             addressTypes,
             null,
             null,
             null,
             null,
             null)
        {
        }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="names"></param>
        /// <param name="nicknames"></param>
        /// <param name="pronunciation"></param>
        /// <param name="phoneNumbers"></param>
        /// <param name="phoneTypes"></param>
        /// <param name="emails"></param>
        /// <param name="emailTypes"></param>
        /// <param name="instantMessenger"></param>
        /// <param name="note"></param>
        /// <param name="addresses"></param>
        /// <param name="addressTypes"></param>
        /// <param name="org"></param>
        /// <param name="birthday"></param>
        /// <param name="title"></param>
        /// <param name="urls"></param>
        /// <param name="geo"></param>
        public AddressBookParsedResult(string[] names,
                                       string[] nicknames,
                                       string pronunciation,
                                       string[] phoneNumbers,
                                       string[] phoneTypes,
                                       string[] emails,
                                       string[] emailTypes,
                                       string instantMessenger,
                                       string note,
                                       string[] addresses,
                                       string[] addressTypes,
                                       string org,
                                       string birthday,
                                       string title,
                                       string[] urls,
                                       string[] geo)
           : base(ParsedResultType.ADDRESSBOOK)
        {
            if (phoneNumbers != null && phoneTypes != null && phoneNumbers.Length != phoneTypes.Length)
            {
                throw new ArgumentException("Phone numbers and types lengths differ");
            }
            if (emails != null && emailTypes != null && emails.Length != emailTypes.Length)
            {
                throw new ArgumentException("Emails and types lengths differ");
            }
            if (addresses != null && addressTypes != null && addresses.Length != addressTypes.Length)
            {
                throw new ArgumentException("Addresses and types lengths differ");
            }

            this.names = names;
            this.nicknames = nicknames;
            this.pronunciation = pronunciation;
            this.phoneNumbers = phoneNumbers;
            this.phoneTypes = phoneTypes;
            this.emails = emails;
            this.emailTypes = emailTypes;
            this.instantMessenger = instantMessenger;
            this.note = note;
            this.addresses = addresses;
            this.addressTypes = addressTypes;
            this.org = org;
            this.birthday = birthday;
            this.title = title;
            this.urls = urls;
            this.geo = geo;

            displayResultValue = getDisplayResult();
        }

        /// <summary>
        /// the names
        /// </summary>
        public string[] Names => names;

        /// <summary>
        /// the nicknames
        /// </summary>
        public string[] Nicknames => nicknames;

        /// <summary>
        /// In Japanese, the name is written in kanji, which can have multiple readings. Therefore a hint
        /// is often provided, called furigana, which spells the name phonetically.
        /// </summary>
        /// <return>The pronunciation of the getNames() field, often in hiragana or katakana.</return>
        public string Pronunciation => pronunciation;

        /// <summary>
        /// the phone numbers
        /// </summary>
        public string[] PhoneNumbers => phoneNumbers;

        /// <return>optional descriptions of the type of each phone number. It could be like "HOME", but,
        /// there is no guaranteed or standard format.</return>
        public string[] PhoneTypes => phoneTypes;

        /// <summary>
        /// the e-mail addresses
        /// </summary>
        public string[] Emails => emails;

        /// <return>optional descriptions of the type of each e-mail. It could be like "WORK", but,
        /// there is no guaranteed or standard format.</return>
        public string[] EmailTypes => emailTypes;

        /// <summary>
        /// the instant messenger addresses
        /// </summary>
        public string InstantMessenger => instantMessenger;

        /// <summary>
        /// the note field
        /// </summary>
        public string Note => note;

        /// <summary>
        /// the addresses
        /// </summary>
        public string[] Addresses => addresses;

        /// <return>optional descriptions of the type of each e-mail. It could be like "WORK", but,
        /// there is no guaranteed or standard format.</return>
        public string[] AddressTypes => addressTypes;

        /// <summary>
        /// the title
        /// </summary>
        public string Title => title;

        /// <summary>
        /// the organisations
        /// </summary>
        public string Org => org;

        /// <summary>
        /// the urls
        /// </summary>
        public string[] URLs => urls;

        /// <return>birthday formatted as yyyyMMdd (e.g. 19780917)</return>
        public string Birthday => birthday;

        /// <return>a location as a latitude/longitude pair</return>
        public string[] Geo => geo;

        private string getDisplayResult()
        {
            var result = new StringBuilder(100);
            maybeAppend(names, result);
            maybeAppend(nicknames, result);
            maybeAppend(pronunciation, result);
            maybeAppend(title, result);
            maybeAppend(org, result);
            maybeAppend(addresses, result);
            maybeAppend(phoneNumbers, result);
            maybeAppend(emails, result);
            maybeAppend(instantMessenger, result);
            maybeAppend(urls, result);
            maybeAppend(birthday, result);
            maybeAppend(geo, result);
            maybeAppend(note, result);
            return result.ToString();
        }
    }
}