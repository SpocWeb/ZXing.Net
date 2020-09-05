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
using ZXing.Common.Detector;

namespace ZXing
{
    /// <summary> Point of interest in an image containing a barcode. </summary>
    /// <author>Sean Owen</author>
    /// <remarks>
    /// Typically, this would be the location of a finder pattern
    /// or the corner of the barcode.
    /// </remarks>
    public class ResultPoint
    {

        public ResultPoint() { }

        public ResultPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; }

        public float Y { get; }

        public override bool Equals(object other) => Equals(other as ResultPoint);

        public bool Equals(ResultPoint otherPoint)
        {
            if (otherPoint == null) {
                return false;
            }
            return X == otherPoint.X && Y == otherPoint.Y;
        }

        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public override string ToString() => $"({X}, {Y})";

        /// <summary>
        /// Orders an array of three ResultPoints in an order [A,B,C] such that AB is less than AC and
        /// BC is less than AC and the angle between BC and BA is less than 180 degrees.
        /// </summary>
        /// <param name="patterns">array of three <see cref="ResultPoint" /> to order</param>
        public static void orderBestPatterns(ResultPoint[] patterns)
        {
            // Find distances between pattern centers
            float zeroOneDistance = distance(patterns[0], patterns[1]);
            float oneTwoDistance = distance(patterns[1], patterns[2]);
            float zeroTwoDistance = distance(patterns[0], patterns[2]);

            ResultPoint pointA, pointB, pointC;
            // Assume one closest to other two is B; A and C will just be guesses at first
            if (oneTwoDistance >= zeroOneDistance && oneTwoDistance >= zeroTwoDistance)
            {
                pointB = patterns[0];
                pointA = patterns[1];
                pointC = patterns[2];
            }
            else if (zeroTwoDistance >= oneTwoDistance && zeroTwoDistance >= zeroOneDistance)
            {
                pointB = patterns[1];
                pointA = patterns[0];
                pointC = patterns[2];
            }
            else
            {
                pointB = patterns[2];
                pointA = patterns[0];
                pointC = patterns[1];
            }

            // Use cross product to figure out whether A and C are correct or flipped.
            // This asks whether BC x BA has a positive z component, which is the arrangement
            // we want for A, B, C. If it's negative, then we've got it flipped around and
            // should swap A and C.
            if (crossProductZ(pointA, pointB, pointC) < 0.0f)
            {
                ResultPoint temp = pointA;
                pointA = pointC;
                pointC = temp;
            }

            patterns[0] = pointA;
            patterns[1] = pointB;
            patterns[2] = pointC;
        }

        public static float distance(ResultPoint pattern1, ResultPoint pattern2)
            => MathUtils.distance(pattern1.X, pattern1.Y, pattern2.X, pattern2.Y);

        /// <summary>
        /// Returns the z component of the cross product between vectors BC and BA.
        /// </summary>
        private static float crossProductZ(ResultPoint pointA, ResultPoint pointB, ResultPoint pointC)
        {
            float bX = pointB.X;
            float bY = pointB.Y;
            return ((pointC.X - bX) * (pointA.Y - bY)) - ((pointC.Y - bY) * (pointA.X - bX));
        }
    }
}