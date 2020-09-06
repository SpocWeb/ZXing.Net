/*
 * Copyright 2010 ZXing authors
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
    /// Represents a parsed result that encodes wifi network information, like SSID and password.
    /// </summary>
    /// <author>Vikram Aggarwal</author>
    public class WifiParsedResult : ParsedResult
    {
        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="networkEncryption"></param>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        public WifiParsedResult(string networkEncryption, string ssid, string password)
           : this(networkEncryption, ssid, password, false)
        {
        }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="networkEncryption"></param>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        /// <param name="hidden"></param>
        public WifiParsedResult(string networkEncryption, string ssid, string password, bool hidden)
           : this(networkEncryption, ssid, password, hidden, null, null, null, null)
        {

        }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="networkEncryption"></param>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        /// <param name="hidden"></param>
        /// <param name="identity"></param>
        /// <param name="anonymousIdentity"></param>
        /// <param name="eapMethod"></param>
        /// <param name="phase2Method"></param>
        public WifiParsedResult(string networkEncryption, string ssid, string password, bool hidden, string identity, string anonymousIdentity, string eapMethod, string phase2Method)
           : base(ParsedResultType.WIFI)
        {
            Ssid = ssid;
            NetworkEncryption = networkEncryption;
            Password = password;
            Hidden = hidden;
            Identity = identity;
            AnonymousIdentity = anonymousIdentity;
            EapMethod = eapMethod;
            Phase2Method = phase2Method;

            var result = new StringBuilder(80);
            maybeAppend(Ssid, result);
            maybeAppend(NetworkEncryption, result);
            maybeAppend(Password, result);
            maybeAppend(Hidden.ToString(), result);
            maybeAppend(Identity, result);
            maybeAppend(AnonymousIdentity, result);
            maybeAppend(EapMethod, result);
            maybeAppend(Phase2Method, result);
            displayResultValue = result.ToString();
        }

        /// <summary>
        /// SSID
        /// </summary>
        public string Ssid { get; }
        /// <summary>
        /// network encryption
        /// </summary>
        public string NetworkEncryption { get; }
        /// <summary>
        /// password
        /// </summary>
        public string Password { get; }
        /// <summary>
        /// hidden flag
        /// </summary>
        public bool Hidden { get; }
        /// <summary>
        /// identity
        /// </summary>
        public string Identity { get; }

        /// <summary>
        /// anonymous
        /// </summary>
        public string AnonymousIdentity { get; }
        /// <summary>
        /// eap
        /// </summary>
        public string EapMethod { get; }
        /// <summary>
        /// phase 2 method
        /// </summary>
        public string Phase2Method { get; }
    }
}