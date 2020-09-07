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

using NUnit.Framework;

namespace ZXing.Client.Result.Test
{
   /// <summary>
   /// Tests <see cref="EmailAddressParsedResult" />.
   ///
   /// <author>Sean Owen</author>
   /// </summary>
   [TestFixture]
   public sealed class EmailAddressParsedResultTestCase
   {
      [Test]
      public void TestEmailAddress()
      {
         DoTest("srowen@example.org", "srowen@example.org", null, null);
         DoTest("mailto:srowen@example.org", "srowen@example.org", null, null);
      }

      [Test]
      public void TestTos()
      {
         DoTest("mailto:srowen@example.org,bob@example.org",
                new string[] {"srowen@example.org", "bob@example.org"},
                null, null, null, null);
         DoTest("mailto:?to=srowen@example.org,bob@example.org",
                new string[] {"srowen@example.org", "bob@example.org"},
                null, null, null, null);
      }

      [Test]
      public void TestCCs()
      {
         DoTest("mailto:?cc=srowen@example.org",
                null,
                new string[] {"srowen@example.org"},
                null, null, null);
         DoTest("mailto:?cc=srowen@example.org,bob@example.org",
                null,
                new string[] {"srowen@example.org", "bob@example.org"},
                null, null, null);
      }

      [Test]
      public void TestBcCs()
      {
         DoTest("mailto:?bcc=srowen@example.org",
                null, null,
                new string[] {"srowen@example.org"},
                null, null);
         DoTest("mailto:?bcc=srowen@example.org,bob@example.org",
                null, null,
                new string[] {"srowen@example.org", "bob@example.org"},
                null, null);
      }

      [Test]
      public void TestAll()
      {
         DoTest("mailto:bob@example.org?cc=foo@example.org&bcc=srowen@example.org&subject=baz&body=buzz",
                new string[] {"bob@example.org"},
                new string[] {"foo@example.org"},
                new string[] {"srowen@example.org"},
                "baz",
                "buzz");
      }

      [Test]
      public void TestEmailDocomo()
      {
         DoTest("MATMSG:TO:srowen@example.org;;", "srowen@example.org", null, null);
         DoTest("MATMSG:TO:srowen@example.org;SUB:Stuff;;", "srowen@example.org", "Stuff", null);
         DoTest("MATMSG:TO:srowen@example.org;SUB:Stuff;BODY:This is some text;;", "srowen@example.org",
                "Stuff", "This is some text");
      }

      [Test]
      public void TestSmtp()
      {
         DoTest("smtp:srowen@example.org", "srowen@example.org", null, null);
         DoTest("SMTP:srowen@example.org", "srowen@example.org", null, null);
         DoTest("smtp:srowen@example.org:foo", "srowen@example.org", "foo", null);
         DoTest("smtp:srowen@example.org:foo:bar", "srowen@example.org", "foo", "bar");
      }

      static void DoTest(string contents,
                                 string to,
                                 string subject,
                                 string body)
      {
         DoTest(contents, new string[] {to}, null, null, subject, body);
      }

      static void DoTest(string contents,
                                 string[] tos,
                                 string[] ccs,
                                 string[] bccs,
                                 string subject,
                                 string body)
      {
         var fakeResult = new BarCodeText(contents, null, null, null, BarcodeFormat.QR_CODE);
         var result = ResultParser.ParseResult(fakeResult);
         Assert.AreEqual(ParsedResultType.EMAIL_ADDRESS, result.Type);
         var emailResult = (EmailAddressParsedResult)result;
         Assert.AreEqual(tos, emailResult.Tos);
         Assert.AreEqual(ccs, emailResult.CCs);
         Assert.AreEqual(bccs, emailResult.BCCs);
         Assert.AreEqual(subject, emailResult.Subject);
         Assert.AreEqual(body, emailResult.Body);
      }
   }
}