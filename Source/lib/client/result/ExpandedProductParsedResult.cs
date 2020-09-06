/*
 * Copyright (C) 2010 ZXing authors
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

/*
 * These authors would like to acknowledge the Spanish Ministry of Industry,
 * Tourism and Trade, for the support in the project TSI020301-2008-2
 * "PIRAmIDE: Personalizable Interactions with Resources on AmI-enabled
 * Mobile Dynamic Environments", led by Treelogic
 * ( http://www.treelogic.com/ ):
 *
 *   http://www.piramidepse.com/
 */

using System;
using System.Collections.Generic;

namespace ZXing.Client.Result
{
    /// <summary>
    /// Represents a parsed result that encodes extended product information as encoded by the RSS format, like weight, price, dates, etc.
    /// </summary>
    /// <author> Antonio Manuel Benjumea Conde, Servinform, S.A.</author>
    /// <author> Agustín Delgado, Servinform, S.A.</author>
    public class ExpandedProductParsedResult : ParsedResult
    {
        /// <summary>
        /// extension for kilogram weight type
        /// </summary>
        public static string KILOGRAM = "KG";
        /// <summary>
        /// extension for pounds weight type
        /// </summary>
        public static string POUND = "LB";

        // For AIS that not exist in this object

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="rawText"></param>
        /// <param name="productID"></param>
        /// <param name="sscc"></param>
        /// <param name="lotNumber"></param>
        /// <param name="productionDate"></param>
        /// <param name="packagingDate"></param>
        /// <param name="bestBeforeDate"></param>
        /// <param name="expirationDate"></param>
        /// <param name="weight"></param>
        /// <param name="weightType"></param>
        /// <param name="weightIncrement"></param>
        /// <param name="price"></param>
        /// <param name="priceIncrement"></param>
        /// <param name="priceCurrency"></param>
        /// <param name="uncommonAIs"></param>
        public ExpandedProductParsedResult(string rawText,
                                           string productID,
                                           string sscc,
                                           string lotNumber,
                                           string productionDate,
                                           string packagingDate,
                                           string bestBeforeDate,
                                           string expirationDate,
                                           string weight,
                                           string weightType,
                                           string weightIncrement,
                                           string price,
                                           string priceIncrement,
                                           string priceCurrency,
                                           IDictionary<string, string> uncommonAIs)
           : base(ParsedResultType.PRODUCT)
        {
            this.RawText = rawText;
            this.ProductID = productID;
            this.Sscc = sscc;
            this.LotNumber = lotNumber;
            this.ProductionDate = productionDate;
            this.PackagingDate = packagingDate;
            this.BestBeforeDate = bestBeforeDate;
            this.ExpirationDate = expirationDate;
            this.Weight = weight;
            this.WeightType = weightType;
            this.WeightIncrement = weightIncrement;
            this.Price = price;
            this.PriceIncrement = priceIncrement;
            this.PriceCurrency = priceCurrency;
            this.UncommonAIs = uncommonAIs;

            displayResultValue = productID;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            if (!(o is ExpandedProductParsedResult))
            {
                return false;
            }

            var other = (ExpandedProductParsedResult)o;

            return equalsOrNull(ProductID, other.ProductID)
                && equalsOrNull(Sscc, other.Sscc)
                && equalsOrNull(LotNumber, other.LotNumber)
                && equalsOrNull(ProductionDate, other.ProductionDate)
                && equalsOrNull(BestBeforeDate, other.BestBeforeDate)
                && equalsOrNull(ExpirationDate, other.ExpirationDate)
                && equalsOrNull(Weight, other.Weight)
                && equalsOrNull(WeightType, other.WeightType)
                && equalsOrNull(WeightIncrement, other.WeightIncrement)
                && equalsOrNull(Price, other.Price)
                && equalsOrNull(PriceIncrement, other.PriceIncrement)
                && equalsOrNull(PriceCurrency, other.PriceCurrency)
                && equalsOrNull(UncommonAIs, other.UncommonAIs);
        }

        private static bool equalsOrNull(object o1, object o2)
        {
            return o1 == null ? o2 == null : o1.Equals(o2);
        }

        private static bool equalsOrNull(IDictionary<string, string> o1, IDictionary<string, string> o2)
        {
            if (o1 == null) {
                return o2 == null;
            }
            if (o1.Count != o2.Count) {
                return false;
            }
            foreach (var entry in o1)
            {
                if (!o2.ContainsKey(entry.Key)) {
                    return false;
                }
                if (!entry.Value.Equals(o2[entry.Key])) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 0;
            hash ^= hashNotNull(ProductID);
            hash ^= hashNotNull(Sscc);
            hash ^= hashNotNull(LotNumber);
            hash ^= hashNotNull(ProductionDate);
            hash ^= hashNotNull(BestBeforeDate);
            hash ^= hashNotNull(ExpirationDate);
            hash ^= hashNotNull(Weight);
            hash ^= hashNotNull(WeightType);
            hash ^= hashNotNull(WeightIncrement);
            hash ^= hashNotNull(Price);
            hash ^= hashNotNull(PriceIncrement);
            hash ^= hashNotNull(PriceCurrency);
            hash ^= hashNotNull(UncommonAIs);
            return hash;
        }

        private static int hashNotNull(object o)
        {
            return o == null ? 0 : o.GetHashCode();
        }

        /// <summary>
        /// the raw text
        /// </summary>
        public string RawText { get; }

        /// <summary>
        /// the product id
        /// </summary>
        public string ProductID { get; }

        /// <summary>
        /// the sscc
        /// </summary>
        public string Sscc { get; }

        /// <summary>
        /// the lot number
        /// </summary>
        public string LotNumber { get; }

        /// <summary>
        /// the production date
        /// </summary>
        public string ProductionDate { get; }

        /// <summary>
        /// the packaging date
        /// </summary>
        public string PackagingDate { get; }

        /// <summary>
        /// the best before date
        /// </summary>
        public string BestBeforeDate { get; }

        /// <summary>
        /// the expiration date
        /// </summary>
        public string ExpirationDate { get; }

        /// <summary>
        /// the weight
        /// </summary>
        public string Weight { get; }

        /// <summary>
        /// the weight type
        /// </summary>
        public string WeightType { get; }

        /// <summary>
        /// the weight increment
        /// </summary>
        public string WeightIncrement { get; }

        /// <summary>
        /// the price
        /// </summary>
        public string Price { get; }

        /// <summary>
        /// the price increment
        /// </summary>
        public string PriceIncrement { get; }

        /// <summary>
        /// the price currency
        /// </summary>
        public string PriceCurrency { get; }

        /// <summary>
        /// the uncommon AIs
        /// </summary>
        public IDictionary<string, string> UncommonAIs { get; }

        /// <summary>
        /// the display representation (raw text)
        /// </summary>
        public override string DisplayResult => RawText;

    }
}