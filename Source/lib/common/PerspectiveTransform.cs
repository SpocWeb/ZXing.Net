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

namespace ZXing.Common
{
    public static class XTrafo
    {
        /// <summary>
        /// 
        /// </summary>
        public static PerspectiveTransform QuadrilateralToQuadrilateral(float x0, float y0
            , float x1, float y1
            , float x2, float y2
            , float x3, float y3
            , float x0P, float y0P
            , float x1P, float y1P
            , float x2P, float y2P
            , float x3P, float y3P)
        {

            PerspectiveTransform qToS = QuadriLateralToSquare(x0, y0, x1, y1, x2, y2, x3, y3);
            PerspectiveTransform sToQ = SquareToQuadrilateral(x0P, y0P, x1P, y1P, x2P, y2P, x3P, y3P);
            return sToQ.Times(qToS);
        }

        /// <summary>
        /// 
        /// </summary>
        public static PerspectiveTransform SquareToQuadrilateral(float x0, float y0,
            float x1, float y1,
            float x2, float y2,
            float x3, float y3)
        {
            float dx3 = x0 - x1 + x2 - x3;
            float dy3 = y0 - y1 + y2 - y3;
            if (dx3 == 0.0f && dy3 == 0.0f)
            {
                // faster Affine Trafo
                return new PerspectiveTransform(x1 - x0, x2 - x1, x0,
                    y1 - y0, y2 - y1, y0,
                    0.0f, 0.0f, 1.0f);
            }

            float dx1 = x1 - x2;
            float dx2 = x3 - x2;
            float dy1 = y1 - y2;
            float dy2 = y3 - y2;
            float denominator = dx1 * dy2 - dx2 * dy1;
            float a13 = (dx3 * dy2 - dx2 * dy3) / denominator;
            float a23 = (dx1 * dy3 - dx3 * dy1) / denominator;
            return new PerspectiveTransform(x1 - x0 + a13 * x1, x3 - x0 + a23 * x3, x0,
                y1 - y0 + a13 * y1, y3 - y0 + a23 * y3, y0,
                a13, a23, 1.0f);
        }

        /// <summary> Here, the adjoint serves as the inverse </summary>
        public static PerspectiveTransform QuadriLateralToSquare(float x0, float y0
            , float x1, float y1, float x2, float y2, float x3, float y3)
            =>
                SquareToQuadrilateral(x0, y0, x1, y1, x2, y2, x3, y3).BuildAdjoint();
    }

    /// <summary> <p> Perspective 2D transform. </summary>
    /// <author>Sean Owen</author>
    /// <remarks>
    /// Given four source and four destination points,
    /// it will compute the affine transformation implied between them.
    /// Using homogenous Coordinates. s
    /// The code is based directly upon section 3.4.2
    /// of George Wolberg's "Digital Image Warping"; see pages 54-56.</p>
    /// 
    /// 1 Point is for Translation
    /// 2 Points for the 2 Axes
    /// another Point could be used for Perspective. 
    /// </remarks>
    public sealed class PerspectiveTransform
    {
        public readonly float A11;
        public readonly float A12;
        public readonly float A13;
        public readonly float A21;
        public readonly float A22;
        public readonly float A23;
        public readonly float A31;
        public readonly float A32;
        public readonly float A33;

        /// <summary> Initializing Constructor </summary>
        public PerspectiveTransform(float a11, float a21, float a31, float a12, float a22, float a32, float a13, float a23, float a33)
        {
            this.A11 = a11;
            this.A12 = a12;
            this.A13 = a13;
            this.A21 = a21;
            this.A22 = a22;
            this.A23 = a23;
            this.A31 = a31;
            this.A32 = a32;
            this.A33 = a33;
        }

        /// <summary> Maps <paramref name="xyPoints"/> in place using this Trafo </summary>
        public void TransformPoints(float[] xyPoints)
        {
            int maxI = xyPoints.Length - 1; // points.length must be even
            for (int i = 0; i < maxI; i += 2)
            {
                float x = xyPoints[i];
                float y = xyPoints[i + 1];
                float z = A13 * x + A23 * y + A33;
                xyPoints[i] = (A11 * x + A21 * y + A31) / z;
                xyPoints[i + 1] = (A12 * x + A22 * y + A32) / z;
            } //Division results in Projective Geometry
        }

        /// <summary> Maps <paramref name="xyPoints"/> in place using this Trafo </summary>
        public void TransformPoints(float[] xValues, float[] yValues)
        {
            int n = xValues.Length;
            for (int i = 0; i < n; i++)
            {
                float x = xValues[i];
                float y = yValues[i];
                float z = A13 * x + A23 * y + A33;
                xValues[i] = (A11 * x + A21 * y + A31) / z;
                yValues[i] = (A12 * x + A22 * y + A32) / z;
            } //Division results in Projective Geometry
        }

        /// <summary> Adjoint is the transpose of the coFactor matrix </summary>
        /// <returns></returns>
        internal PerspectiveTransform BuildAdjoint() =>
            new PerspectiveTransform(
                A22 * A33 - A23 * A32,
                A23 * A31 - A21 * A33,
                A21 * A32 - A22 * A31,
                A13 * A32 - A12 * A33,
                A11 * A33 - A13 * A31,
                A12 * A31 - A11 * A32,
                A12 * A23 - A13 * A22,
                A13 * A21 - A11 * A23,
                A11 * A22 - A12 * A21);

        public PerspectiveTransform Times(PerspectiveTransform other) =>
            new PerspectiveTransform(
                A11 * other.A11 + A21 * other.A12 + A31 * other.A13,
                A11 * other.A21 + A21 * other.A22 + A31 * other.A23,
                A11 * other.A31 + A21 * other.A32 + A31 * other.A33,
                A12 * other.A11 + A22 * other.A12 + A32 * other.A13,
                A12 * other.A21 + A22 * other.A22 + A32 * other.A23,
                A12 * other.A31 + A22 * other.A32 + A32 * other.A33,
                A13 * other.A11 + A23 * other.A12 + A33 * other.A13,
                A13 * other.A21 + A23 * other.A22 + A33 * other.A23,
                A13 * other.A31 + A23 * other.A32 + A33 * other.A33);
    }
}