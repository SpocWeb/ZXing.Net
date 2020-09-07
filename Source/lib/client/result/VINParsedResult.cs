/*
 * Copyright 2014 ZXing authors
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
    /// Represents a parsed result that encodes a Vehicle Identification Number (VIN).
    /// </summary>
    public class VINParsedResult : ParsedResult
    {
        /// <summary>
        /// VIN
        /// </summary>
        public string VIN { get; }
        /// <summary>
        /// manufacturer id
        /// </summary>
        public string WorldManufacturerID { get; }
        /// <summary>
        /// vehicle descriptor section
        /// </summary>
        public string VehicleDescriptorSection { get; }
        /// <summary>
        /// vehicle identifier section
        /// </summary>
        public string VehicleIdentifierSection { get; }
        /// <summary>
        /// country code
        /// </summary>
        public string CountryCode { get; }
        /// <summary>
        /// vehicle attributes
        /// </summary>
        public string VehicleAttributes { get; }
        /// <summary>
        /// model year
        /// </summary>
        public int ModelYear { get; }
        /// <summary>
        /// plant code
        /// </summary>
        public char PlantCode { get; }
        /// <summary>
        /// sequential number
        /// </summary>
        public string SequentialNumber { get; }

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="worldManufacturerID"></param>
        /// <param name="vehicleDescriptorSection"></param>
        /// <param name="vehicleIdentifierSection"></param>
        /// <param name="countryCode"></param>
        /// <param name="vehicleAttributes"></param>
        /// <param name="modelYear"></param>
        /// <param name="plantCode"></param>
        /// <param name="sequentialNumber"></param>
        public VINParsedResult(string vin,
                               string worldManufacturerID,
                               string vehicleDescriptorSection,
                               string vehicleIdentifierSection,
                               string countryCode,
                               string vehicleAttributes,
                               int modelYear,
                               char plantCode,
                               string sequentialNumber)
           : base(ParsedResultType.VIN)
        {
            VIN = vin;
            WorldManufacturerID = worldManufacturerID;
            VehicleDescriptorSection = vehicleDescriptorSection;
            VehicleIdentifierSection = vehicleIdentifierSection;
            CountryCode = countryCode;
            VehicleAttributes = vehicleAttributes;
            ModelYear = modelYear;
            PlantCode = plantCode;
            SequentialNumber = sequentialNumber;
        }
        /// <summary>
        /// a user friendly representation
        /// </summary>
        public override string DisplayResult
        {
            get
            {
                var result = new StringBuilder(50);
                result.Append(WorldManufacturerID).Append(' ');
                result.Append(VehicleDescriptorSection).Append(' ');
                result.Append(VehicleIdentifierSection).Append('\n');
                if (CountryCode != null)
                {
                    result.Append(CountryCode).Append(' ');
                }
                result.Append(ModelYear).Append(' ');
                result.Append(PlantCode).Append(' ');
                result.Append(SequentialNumber).Append('\n');
                return result.ToString();
            }
        }
    }
}