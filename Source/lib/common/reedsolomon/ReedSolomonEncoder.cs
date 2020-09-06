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

namespace ZXing.Common.ReedSolomon
{
    /// <summary>
    /// Implements Reed-Solomon encoding, as the name implies.
    /// </summary>
    /// <author>Sean Owen</author>
    /// <author>William Rucklidge</author>
    public sealed class ReedSolomonEncoder
    {
        private readonly GenericGf _Field;
        private readonly IList<GenericGfPoly> _CachedGenerators;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="field"></param>
        public ReedSolomonEncoder(GenericGf field)
        {
            this._Field = field;
            _CachedGenerators = new List<GenericGfPoly>
            {
                new GenericGfPoly(field, new[] { 1 })
            };
        }

        private GenericGfPoly BuildGenerator(int degree)
        {
            if (degree >= _CachedGenerators.Count)
            {
                var lastGenerator = _CachedGenerators[_CachedGenerators.Count - 1];
                for (int d = _CachedGenerators.Count; d <= degree; d++)
                {
                    var nextGenerator = lastGenerator.Multiply(new GenericGfPoly(_Field, new[] { 1, _Field.Exp(d - 1 + _Field.GeneratorBase) }));
                    _CachedGenerators.Add(nextGenerator);
                    lastGenerator = nextGenerator;
                }
            }
            return _CachedGenerators[degree];
        }

        /// <summary>
        /// encodes
        /// </summary>
        public void Encode(int[] toEncode, int ecBytes)
        {
            if (ecBytes == 0)
            {
                throw new ArgumentException("No error correction bytes");
            }
            var dataBytes = toEncode.Length - ecBytes;
            if (dataBytes <= 0)
            {
                throw new ArgumentException("No data bytes provided");
            }

            var generator = BuildGenerator(ecBytes);
            var infoCoefficients = new int[dataBytes];
            Array.Copy(toEncode, 0, infoCoefficients, 0, dataBytes);

            var info = new GenericGfPoly(_Field, infoCoefficients);
            info = info.MultiplyByMonomial(ecBytes, 1);

            var remainder = info.Divide(generator)[1];
            var coefficients = remainder.Coefficients;
            var numZeroCoefficients = ecBytes - coefficients.Count;
            for (var i = 0; i < numZeroCoefficients; i++)
            {
                toEncode[dataBytes + i] = 0;
            }

            Array.Copy((int[])coefficients, 0, toEncode, dataBytes + numZeroCoefficients, coefficients.Count);
        }
    }
}