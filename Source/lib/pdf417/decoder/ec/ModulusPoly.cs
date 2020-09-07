/*
 * Copyright 2012 ZXing authors
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
using ZXing.Common.ReedSolomon;

namespace ZXing.PDF417.Internal.EC
{
    /// <summary>
    /// <see cref="GenericGfPoly"/>
    /// </summary>
    /// <author>Sean Owen</author>
    internal sealed class ModulusPoly
    {
        private readonly ModulusGF field;

        public ModulusPoly(ModulusGF field, int[] coefficients)
        {
            if (coefficients.Length == 0)
            {
                throw new ArgumentException();
            }
            this.field = field;
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
                    Coefficients = new[] { 0 };
                }
                else
                {
                    Coefficients = new int[coefficientsLength - firstNonZero];
                    Array.Copy(coefficients,
                               firstNonZero,
                               Coefficients,
                               0,
                               Coefficients.Length);
                }
            }
            else
            {
                Coefficients = coefficients;
            }
        }

        /// <summary>
        /// Gets the coefficients.
        /// </summary>
        /// <value>The coefficients.</value>
        internal int[] Coefficients { get; }

        /// <summary>
        /// degree of this polynomial
        /// </summary>
        internal int Degree => Coefficients.Length - 1;

        /// <summary>
        /// Gets a value indicating whether this instance is zero.
        /// </summary>
        /// <value>true if this polynomial is the monomial "0"
        /// </value>
        internal bool isZero => Coefficients[0] == 0;

        /// <summary>
        /// coefficient of x^degree term in this polynomial
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns>coefficient of x^degree term in this polynomial</returns>
        internal int getCoefficient(int degree)
        {
            return Coefficients[Coefficients.Length - 1 - degree];
        }

        /// <summary>
        /// evaluation of this polynomial at a given point
        /// </summary>
        /// <param name="a">A.</param>
        /// <returns>evaluation of this polynomial at a given point</returns>
        internal int evaluateAt(int a)
        {
            if (a == 0)
            {
                // Just return the x^0 coefficient
                return getCoefficient(0);
            }
            int result = 0;
            if (a == 1)
            {
                // Just the sum of the coefficients
                foreach (var coefficient in Coefficients)
                {
                    result = field.add(result, coefficient);
                }
                return result;
            }
            result = Coefficients[0];
            int size = Coefficients.Length;
            for (int i = 1; i < size; i++)
            {
                result = field.add(field.multiply(a, result), Coefficients[i]);
            }
            return result;
        }

        /// <summary>
        /// Adds another Modulus
        /// </summary>
        /// <param name="other">Other.</param>
        internal ModulusPoly add(ModulusPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("ModulusPolys do not have same ModulusGF field");
            }
            if (isZero)
            {
                return other;
            }
            if (other.isZero)
            {
                return this;
            }

            int[] smallerCoefficients = Coefficients;
            int[] largerCoefficients = other.Coefficients;
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
                sumDiff[i] = field.add(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }

            return new ModulusPoly(field, sumDiff);
        }

        /// <summary>
        /// Subtract another Modulus
        /// </summary>
        /// <param name="other">Other.</param>
        internal ModulusPoly subtract(ModulusPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("ModulusPolys do not have same ModulusGF field");
            }
            if (other.isZero)
            {
                return this;
            }
            return add(other.getNegative());
        }

        /// <summary>
        /// Multiply by another Modulus
        /// </summary>
        /// <param name="other">Other.</param>
        internal ModulusPoly multiply(ModulusPoly other)
        {
            if (!field.Equals(other.field))
            {
                throw new ArgumentException("ModulusPolys do not have same ModulusGF field");
            }
            if (isZero || other.isZero)
            {
                return field.Zero;
            }
            int[] aCoefficients = Coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other.Coefficients;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++)
            {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++)
                {
                    product[i + j] = field.add(product[i + j], field.multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new ModulusPoly(field, product);
        }

        /// <summary>
        /// Returns a Negative version of this instance
        /// </summary>
        internal ModulusPoly getNegative()
        {
            int size = Coefficients.Length;
            int[] negativeCoefficients = new int[size];
            for (int i = 0; i < size; i++)
            {
                negativeCoefficients[i] = field.subtract(0, Coefficients[i]);
            }
            return new ModulusPoly(field, negativeCoefficients);
        }

        /// <summary>
        /// Multiply by a Scalar.
        /// </summary>
        /// <param name="scalar">Scalar.</param>
        internal ModulusPoly multiply(int scalar)
        {
            if (scalar == 0)
            {
                return field.Zero;
            }
            if (scalar == 1)
            {
                return this;
            }
            int size = Coefficients.Length;
            int[] product = new int[size];
            for (int i = 0; i < size; i++)
            {
                product[i] = field.multiply(Coefficients[i], scalar);
            }
            return new ModulusPoly(field, product);
        }

        /// <summary>
        /// Multiplies by a Monomial
        /// </summary>
        /// <returns>The by monomial.</returns>
        /// <param name="degree">Degree.</param>
        /// <param name="coefficient">Coefficient.</param>
        internal ModulusPoly multiplyByMonomial(int degree, int coefficient)
        {
            if (degree < 0)
            {
                throw new ArgumentException();
            }
            if (coefficient == 0)
            {
                return field.Zero;
            }
            int size = Coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++)
            {
                product[i] = field.multiply(Coefficients[i], coefficient);
            }
            return new ModulusPoly(field, product);
        }

        /*
        /// <summary>
        /// Divide by another modulus
        /// </summary>
        /// <param name="other">Other.</param>
        internal ModulusPoly[] divide(ModulusPoly other)
        {
           if (!field.Equals(other.field))
           {
              throw new ArgumentException("ModulusPolys do not have same ModulusGF field");
           }
           if (other.isZero)
           {
              throw new DivideByZeroException();
           }

           ModulusPoly quotient = field.Zero;
           ModulusPoly remainder = this;

           int denominatorLeadingTerm = other.getCoefficient(other.Degree);
           int inverseDenominatorLeadingTerm = field.inverse(denominatorLeadingTerm);

           while (remainder.Degree >= other.Degree && !remainder.isZero)
           {
              int degreeDifference = remainder.Degree - other.Degree;
              int scale = field.multiply(remainder.getCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
              ModulusPoly term = other.multiplyByMonomial(degreeDifference, scale);
              ModulusPoly iterationQuotient = field.buildMonomial(degreeDifference, scale);
              quotient = quotient.add(iterationQuotient);
              remainder = remainder.subtract(term);
           }

           return new ModulusPoly[] { quotient, remainder };
        }
        */

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="ZXing.PDF417.Internal.EC.ModulusPoly"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="ZXing.PDF417.Internal.EC.ModulusPoly"/>.</returns>
        public override string ToString()
        {
            var result = new StringBuilder(8 * Degree);
            for (int degree = Degree; degree >= 0; degree--)
            {
                int coefficient = getCoefficient(degree);
                if (coefficient != 0)
                {
                    if (coefficient < 0)
                    {
                        result.Append(" - ");
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
                        result.Append(coefficient);
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