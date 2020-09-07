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
using ZXing.Common;
using ZXing.Common.Detector;

namespace ZXing.Datamatrix.Internal
{
    /// <summary> can detect a Data Matrix Code in an image </summary>
    /// <remarks>
    /// Even if the Data Matrix Code is rotated or skewed, or partially obscured.</p>
    /// </remarks>
    /// <author>Sean Owen</author>
    public sealed class Detector
    {
        /// <summary> This is only a candidate Image </summary>
        readonly BitMatrix Image;

       readonly IGridSampler Sampler;
        private readonly WhiteRectangleDetector _RectangleDetector;

        /// <summary>
        /// Initializes a new instance of the <see cref="Detector"/> class.
        /// </summary>
        public Detector(IGridSampler sampler)
        {
            this.Sampler = sampler;
            Image = sampler.GetImage();
            _RectangleDetector = WhiteRectangleDetector.Create(Image);
        }

        /// <summary>
        /// <p>Detects a Data Matrix Code in an image.</p>
        /// </summary>
        /// <returns><see cref="DetectorResult" />encapsulating results of detecting a Data Matrix Code or null</returns>
        public DetectorResult Detect()
        {
            ResultPoint[] cornerPoints = _RectangleDetector?.Detect();
            if (cornerPoints == null) {
                return null;
            }

            ResultPoint[] points = DetectSolid1(cornerPoints);
            points = DetectSolid2(points);
            points[3] = CorrectTopRight(points);
            if (points[3] == null)
            {
                return null;
            }
            points = ShiftToModuleCenter(points);

            ResultPoint topLeft = points[0];
            ResultPoint bottomLeft = points[1];
            ResultPoint bottomRight = points[2];
            ResultPoint topRight = points[3];

            int dimensionTop = TransitionsBetween(topLeft, topRight) + 1;
            int dimensionRight = TransitionsBetween(bottomRight, topRight) + 1;
            if ((dimensionTop & 0x01) == 1)
            {
                dimensionTop += 1;
            }
            if ((dimensionRight & 0x01) == 1)
            {
                dimensionRight += 1;
            }

            if (4 * dimensionTop < 7 * dimensionRight && 4 * dimensionRight < 7 * dimensionTop)
            {
                // The matrix is square
                dimensionTop = dimensionRight = Math.Max(dimensionTop, dimensionRight);
            }

            BitMatrix bits = SampleGrid(Sampler,
                topLeft,
                bottomLeft,
                bottomRight,
                topRight,
                dimensionTop,
                dimensionRight);

            return new DetectorResult(bits, new[] { topLeft, bottomLeft, bottomRight, topRight });
        }

        private static ResultPoint ShiftPoint(ResultPoint point, ResultPoint to, int div)
        {
            float x = (to.X - point.X) / (div + 1);
            float y = (to.Y - point.Y) / (div + 1);
            return new ResultPoint(point.X + x, point.Y + y);
        }

        private static ResultPoint MoveAway(ResultPoint point, float fromX, float fromY)
        {
            float x = point.X;
            float y = point.Y;

            if (x < fromX)
            {
                x -= 1;
            }
            else
            {
                x += 1;
            }

            if (y < fromY)
            {
                y -= 1;
            }
            else
            {
                y += 1;
            }

            return new ResultPoint(x, y);
        }

        /// <summary> Detect a solid side which has minimum transition. </summary>
        private ResultPoint[] DetectSolid1(IReadOnlyList<ResultPoint> cornerPoints)
        {
            // 0  2
            // 1  3
            ResultPoint pointA = cornerPoints[0];
            ResultPoint pointB = cornerPoints[1];
            ResultPoint pointC = cornerPoints[3];
            ResultPoint pointD = cornerPoints[2];

            int trAb = TransitionsBetween(pointA, pointB);
            int trBc = TransitionsBetween(pointB, pointC);
            int trCd = TransitionsBetween(pointC, pointD);
            int trDa = TransitionsBetween(pointD, pointA);

            // 0..3
            // :  :
            // 1--2
            int min = trAb;
            ResultPoint[] points = { pointD, pointA, pointB, pointC };
            if (min > trBc)
            {
                min = trBc;
                points[0] = pointA;
                points[1] = pointB;
                points[2] = pointC;
                points[3] = pointD;
            }
            if (min > trCd)
            {
                min = trCd;
                points[0] = pointB;
                points[1] = pointC;
                points[2] = pointD;
                points[3] = pointA;
            }
            if (min > trDa)
            {
                points[0] = pointC;
                points[1] = pointD;
                points[2] = pointA;
                points[3] = pointB;
            }

            return points;
        }

        /// <summary>
        /// Detect a second solid side next to first solid side.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private ResultPoint[] DetectSolid2(ResultPoint[] points)
        {
            // A..D
            // :  :
            // B--C
            ResultPoint pointA = points[0];
            ResultPoint pointB = points[1];
            ResultPoint pointC = points[2];
            ResultPoint pointD = points[3];

            // Transition detection on the edge is not stable.
            // To safely detect, shift the points to the module center.
            int tr = TransitionsBetween(pointA, pointD);
            ResultPoint pointBs = ShiftPoint(pointB, pointC, (tr + 1) * 4);
            ResultPoint pointCs = ShiftPoint(pointC, pointB, (tr + 1) * 4);
            int trBa = TransitionsBetween(pointBs, pointA);
            int trCd = TransitionsBetween(pointCs, pointD);

            // 0..3
            // |  :
            // 1--2
            if (trBa < trCd)
            {
                // solid sides: A-B-C
                points[0] = pointA;
                points[1] = pointB;
                points[2] = pointC;
                points[3] = pointD;
            }
            else
            {
                // solid sides: B-C-D
                points[0] = pointB;
                points[1] = pointC;
                points[2] = pointD;
                points[3] = pointA;
            }

            return points;
        }

        /// <summary> Calculates the corner position of the white top right module. </summary>
        private ResultPoint CorrectTopRight(IReadOnlyList<ResultPoint> points)
        {
            // A..D
            // |  :
            // B--C
            ResultPoint pointA = points[0];
            ResultPoint pointB = points[1];
            ResultPoint pointC = points[2];
            ResultPoint pointD = points[3];

            // shift points for safe transition detection.
            int trTop = TransitionsBetween(pointA, pointD);
            int trRight = TransitionsBetween(pointB, pointD);
            ResultPoint pointAs = ShiftPoint(pointA, pointB, (trRight + 1) * 4);
            ResultPoint pointCs = ShiftPoint(pointC, pointB, (trTop + 1) * 4);

            trTop = TransitionsBetween(pointAs, pointD);
            trRight = TransitionsBetween(pointCs, pointD);

            ResultPoint candidate1 = new ResultPoint(
                pointD.X + (pointC.X - pointB.X) / (trTop + 1),
                pointD.Y + (pointC.Y - pointB.Y) / (trTop + 1));
            ResultPoint candidate2 = new ResultPoint(
                pointD.X + (pointA.X - pointB.X) / (trRight + 1),
                pointD.Y + (pointA.Y - pointB.Y) / (trRight + 1));

            if (!IsValid(candidate1))
            {
                if (IsValid(candidate2))
                {
                    return candidate2;
                }
                return null;
            }
            if (!IsValid(candidate2))
            {
                return candidate1;
            }

            int sumC1 = TransitionsBetween(pointAs, candidate1) + TransitionsBetween(pointCs, candidate1);
            int sumC2 = TransitionsBetween(pointAs, candidate2) + TransitionsBetween(pointCs, candidate2);

            if (sumC1 > sumC2)
            {
                return candidate1;
            }
            return candidate2;
        }

        /// <summary> Shift the edge points to the module center. </summary>
        private ResultPoint[] ShiftToModuleCenter(IReadOnlyList<ResultPoint> points)
        {
            // A..D
            // |  :
            // B--C
            ResultPoint pointA = points[0];
            ResultPoint pointB = points[1];
            ResultPoint pointC = points[2];
            ResultPoint pointD = points[3];

            // calculate pseudo dimensions
            int dimH = TransitionsBetween(pointA, pointD) + 1;
            int dimV = TransitionsBetween(pointC, pointD) + 1;

            // shift points for safe dimension detection
            ResultPoint pointAs = ShiftPoint(pointA, pointB, dimV * 4);
            ResultPoint pointCs = ShiftPoint(pointC, pointB, dimH * 4);

            //  calculate more precise dimensions
            dimH = TransitionsBetween(pointAs, pointD) + 1;
            dimV = TransitionsBetween(pointCs, pointD) + 1;
            if ((dimH & 0x01) == 1)
            {
                dimH += 1;
            }
            if ((dimV & 0x01) == 1)
            {
                dimV += 1;
            }

            // WhiteRectangleDetector returns points inside of the rectangle.
            // I want points on the edges.
            float centerX = (pointA.X + pointB.X + pointC.X + pointD.X) / 4;
            float centerY = (pointA.Y + pointB.Y + pointC.Y + pointD.Y) / 4;
            pointA = MoveAway(pointA, centerX, centerY);
            pointB = MoveAway(pointB, centerX, centerY);
            pointC = MoveAway(pointC, centerX, centerY);
            pointD = MoveAway(pointD, centerX, centerY);

            ResultPoint pointBs;
            ResultPoint pointDs;

            // shift points to the center of each modules
            pointAs = ShiftPoint(pointA, pointB, dimV * 4);
            pointAs = ShiftPoint(pointAs, pointD, dimH * 4);
            pointBs = ShiftPoint(pointB, pointA, dimV * 4);
            pointBs = ShiftPoint(pointBs, pointC, dimH * 4);
            pointCs = ShiftPoint(pointC, pointD, dimV * 4);
            pointCs = ShiftPoint(pointCs, pointB, dimH * 4);
            pointDs = ShiftPoint(pointD, pointC, dimV * 4);
            pointDs = ShiftPoint(pointDs, pointA, dimH * 4);

            return new[] { pointAs, pointBs, pointCs, pointDs };
        }

        private bool IsValid(ResultPoint p)
            => p.X >= 0 && p.X < Image.Width &&
                p.Y > 0 && p.Y < Image.Height;

        private static BitMatrix SampleGrid(IGridSampler sampler,
            ResultPoint topLeft,
            ResultPoint bottomLeft,
            ResultPoint bottomRight,
            ResultPoint topRight,
            int dimensionX,
            int dimensionY)
        {
            return sampler.SampleGrid(
                dimensionX,
                dimensionY,
                0.5f,
                0.5f,
                dimensionX - 0.5f,
                0.5f,
                dimensionX - 0.5f,
                dimensionY - 0.5f,
                0.5f,
                dimensionY - 0.5f,
                topLeft.X,
                topLeft.Y,
                topRight.X,
                topRight.Y,
                bottomRight.X,
                bottomRight.Y,
                bottomLeft.X,
                bottomLeft.Y);
        }

        /// <summary>
        /// Counts the number of black/white transitions between two points, using something like Bresenham's algorithm.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private int TransitionsBetween(ResultPoint from, ResultPoint to)
        {
            // See QR Code Detector, sizeOfBlackWhiteBlackRun()
            int fromX = (int)from.X;
            int fromY = (int)from.Y;
            int toX = (int)to.X;
            int toY = (int)to.Y;
            bool steep = Math.Abs(toY - fromY) > Math.Abs(toX - fromX);
            if (steep)
            {
                int temp = fromX;
                fromX = fromY;
                fromY = temp;
                temp = toX;
                toX = toY;
                toY = temp;
            }

            int dx = Math.Abs(toX - fromX);
            int dy = Math.Abs(toY - fromY);
            int error = -dx / 2;
            int ystep = fromY < toY ? 1 : -1;
            int xstep = fromX < toX ? 1 : -1;
            int transitions = 0;
            bool inBlack = Image[steep ? fromY : fromX, steep ? fromX : fromY];
            for (int x = fromX, y = fromY; x != toX; x += xstep)
            {
                bool isBlack = Image[steep ? y : x, steep ? x : y];
                if (isBlack != inBlack)
                {
                    transitions++;
                    inBlack = isBlack;
                }
                error += dy;
                if (error > 0)
                {
                    if (y == toY)
                    {
                        break;
                    }
                    y += ystep;
                    error -= dx;
                }
            }
            return transitions;
        }
    }
}
