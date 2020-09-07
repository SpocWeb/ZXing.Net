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
using System.Collections.Generic;
using System.Text;

namespace ZXing.Common.ReedSolomon
{
    /// <summary>
    /// <p>Represents a polynomial whose coefficients are elements of a GF.
    /// Instances of this class are immutable.</p>
    /// <p>Much credit is due to William Rucklidge
    /// since portions of this code are an indirect
    /// port of his C++ Reed-Solomon implementation.</p>
    /// </summary>
    /// <author>Sean Owen</author>
    public sealed class GenericGfPoly
    {
        private readonly GenericGf _Field;
        private readonly int[] _Coefficients;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGfPoly"/> class.
        /// </summary>
        /// <param name="field">the {@link GenericGF} instance representing the field to use
        /// to perform computations</param>
        /// <param name="coefficients">coefficients as ints representing elements of GF(size), arranged
        /// from most significant (highest-power term) coefficient to least significant</param>
        /// <exception cref="ArgumentException">if argument is null or empty,
        /// or if leading coefficient is 0 and this is not a
        /// constant polynomial (that is, it is not the monomial "0")</exception>
        public GenericGfPoly(GenericGf field, int[] coefficients)
        {
            if (coefficients.Length == 0)
            {
                throw new ArgumentException();
            }
            _Field = field;
            int coefficientsLength = coefficients.Length;
            if (coefficientsLength > 1 && coefficients[0] == 0)
            {
                // Leading term must be non-zero for anything except the constant polynomial "0"
                int firstNonZero = 1;
                while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0)
                {
                    firstNonZero++;
                }
                if (firstNonZero == coefficientsLength)
                {
                    _Coefficients = new[] { 0 };
                }
                else
                {
                    _Coefficients = new int[coefficientsLength - firstNonZero];
                    Array.Copy(coefficients,
                        firstNonZero,
                        _Coefficients,
                        0,
                        _Coefficients.Length);
                }
            }
            else
            {
                _Coefficients = coefficients;
            }
        }

        public IReadOnlyList<int> Coefficients => _Coefficients;

        /// <summary>
        /// degree of this polynomial
        /// </summary>
        public int Degree => _Coefficients.Length - 1;

        /// <summary>
        /// Gets a value indicating whether this <see cref="GenericGfPoly"/> is zero.
        /// </summary>
        /// <value>true iff this polynomial is the monomial "0"</value>
        public bool IsZero => _Coefficients[0] == 0;

        /// <summary>
        /// coefficient of x^degree term in this polynomial
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns>coefficient of x^degree term in this polynomial</returns>
        public int GetCoefficient(int degree)
        {
            return _Coefficients[_Coefficients.Length - 1 - degree];
        }

        /// <summary>
        /// evaluation of this polynomial at a given point
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>evaluation of this polynomial at a given point</returns>
        public int EvaluateAt(int a)
        {
            int result = 0;
            if (a == 0)
            {
                // Just return the x^0 coefficient
                return GetCoefficient(0);
            }
            if (a == 1)
            {
                // Just the sum of the coefficients
                foreach (var coefficient in _Coefficients)
                {
                    result = GenericGf.AddOrSubtract(result, coefficient);
                }
                return result;
            }
            result = _Coefficients[0];
            int size = _Coefficients.Length;
            for (int i = 1; i < size; i++)
            {
                result = GenericGf.AddOrSubtract(_Field.Multiply(a, result), _Coefficients[i]);
            }
            return result;
        }

        public GenericGfPoly AddOrSubtract(GenericGfPoly other)
        {
            if (!_Field.Equals(other._Field))
            {
                throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
            }
            if (IsZero)
            {
                return other;
            }
            if (other.IsZero)
            {
                return this;
            }

            int[] smallerCoefficients = _Coefficients;
            int[] largerCoefficients = other._Coefficients;
            if (smallerCoefficients.Length > largerCoefficients.Length)
            {
                int[] temp = smallerCoefficients;
                smallerCoefficients = largerCoefficients;
                largerCoefficients = temp;
            }
            int[] sumDiff = new int[largerCoefficients.Length];
            int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
            // Copy high-order terms only found in higher-degree polynomial's coefficients
            Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);

            for (int i = lengthDiff; i < largerCoefficients.Length; i++)
            {
                sumDiff[i] = GenericGf.AddOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }

            return new GenericGfPoly(_Field, sumDiff);
        }

        public GenericGfPoly Multiply(GenericGfPoly other)
        {
            if (!_Field.Equals(other._Field))
            {
                throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
            }
            if (IsZero || other.IsZero)
            {
                return _Field.Zero;
            }
            int[] aCoefficients = _Coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other._Coefficients;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++)
            {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++)
                {
                    product[i + j] = GenericGf.AddOrSubtract(product[i + j],
                        _Field.Multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new GenericGfPoly(_Field, product);
        }

        public GenericGfPoly Multiply(int scalar)
        {
            if (scalar == 0)
            {
                return _Field.Zero;
            }
            if (scalar == 1)
            {
                return this;
            }
            int size = _Coefficients.Length;
            int[] product = new int[size];
            for (int i = 0; i < size; i++)
            {
                product[i] = _Field.Multiply(_Coefficients[i], scalar);
            }
            return new GenericGfPoly(_Field, product);
        }

        public GenericGfPoly MultiplyByMonomial(int degree, int coefficient)
        {
            if (degree < 0)
            {
                throw new ArgumentException();
            }
            if (coefficient == 0)
            {
                return _Field.Zero;
            }
            int size = _Coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++)
            {
                product[i] = _Field.Multiply(_Coefficients[i], coefficient);
            }
            return new GenericGfPoly(_Field, product);
        }

        public GenericGfPoly[] Divide(GenericGfPoly other)
        {
            if (!_Field.Equals(other._Field))
            {
                throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
            }
            if (other.IsZero)
            {
                throw new ArgumentException("Divide by 0");
            }

            GenericGfPoly quotient = _Field.Zero;
            GenericGfPoly remainder = this;

            int denominatorLeadingTerm = other.GetCoefficient(other.Degree);
            int inverseDenominatorLeadingTerm = _Field.Inverse(denominatorLeadingTerm);

            while (remainder.Degree >= other.Degree && !remainder.IsZero)
            {
                int degreeDifference = remainder.Degree - other.Degree;
                int scale = _Field.Multiply(remainder.GetCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
                GenericGfPoly term = other.MultiplyByMonomial(degreeDifference, scale);
                GenericGfPoly iterationQuotient = _Field.BuildMonomial(degreeDifference, scale);
                quotient = quotient.AddOrSubtract(iterationQuotient);
                remainder = remainder.AddOrSubtract(term);
            }

            return new[] { quotient, remainder };
        }

        public override string ToString()
        {
            if (IsZero)
            {
                return "0";
            }
            StringBuilder result = new StringBuilder(8 * Degree);
            for (int degree = Degree; degree >= 0; degree--)
            {
                int coefficient = GetCoefficient(degree);
                if (coefficient != 0)
                {
                    if (coefficient < 0)
                    {
                        if (degree == Degree)
                        {
                            result.Append("-");
                        }
                        else
                        {
                            result.Append(" - ");
                        }
                        coefficient = -coefficient;
                    }
                    else
                    {
                        if (result.Length > 0)
                        {
                            result.Append(" + ");
                        }
                    }
                    if (degree == 0 || coefficient != 1)
                    {
                        int alphaPower = _Field.Log(coefficient);
                        if (alphaPower == 0)
                        {
                            result.Append('1');
                        }
                        else if (alphaPower == 1)
                        {
                            result.Append('a');
                        }
                        else
                        {
                            result.Append("a^");
                            result.Append(alphaPower);
                        }
                    }
                    if (degree != 0)
                    {
                        if (degree == 1)
                        {
                            result.Append('x');
                        }
                        else
                        {
                            result.Append("x^");
                            result.Append(degree);
                        }
                    }
                }
            }
            return result.ToString();
        }
    }
}