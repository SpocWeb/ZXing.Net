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
using ZXing.Common;
using ZXing.Common.Detector;

namespace ZXing.QrCode.Internal
{

    public class LuminanceDetector : ADetector { }

    /// <summary>Detect a QR Code in an image, even if the QR Code is rotated or skewed, or partially obscured. </summary>
    /// <author>Sean Owen</author>
    public class ADetector : IDetector { }

    /// <summary>Detect a QR Code in an image, even if the QR Code is rotated or skewed, or partially obscured. </summary>
    /// <author>Sean Owen</author>
    public class QrDetector : ADetector
    {

        private ResultPointCallback resultPointCallback;

        public readonly IGridSampler Sampler;

        /// <summary> Initializes a new instance of the <see cref="QrDetector"/> class. </summary>
        public QrDetector(IGridSampler sampler) {
            Sampler = sampler;
            //this.Image = sampler.GetImage();
        }

        /// <summary> Initializes a new instance of the <see cref="QrDetector"/> class. </summary>
        public QrDetector(BinaryBitmap image) : this(image.GetBlackMatrix()) { }

        /// <summary> Initializes a new instance of the <see cref="QrDetector"/> class. </summary>
        public QrDetector(BitMatrix image) {
            Sampler = new DefaultGridSampler(image);
        }

        /// <summary> This is only a candidate Image </summary>
        protected internal virtual BitMatrix Image => Sampler.GetImage();

        /// <summary>
        /// Gets the result point callback.
        /// </summary>
        protected internal virtual ResultPointCallback ResultPointCallback => resultPointCallback;

        /// <summary>
        ///   <p>Detects a QR Code in an image.</p>
        /// </summary>
        /// <returns>
        ///   <see cref="DetectorResult"/> encapsulating results of detecting a QR Code
        /// </returns>
        public virtual DetectorResult detect() => detect(null);

        /// <summary>
        ///   <p>Detects a QR Code in an image.</p>
        /// </summary>
        /// <param name="hints">optional hints to detector</param>
        /// <returns>
        ///   <see cref="DetectorResult"/> encapsulating results of detecting a QR Code
        /// </returns>
        public virtual DetectorResult detect(IDictionary<DecodeHintType, object> hints)
        {
            resultPointCallback = hints == null || !hints.ContainsKey(DecodeHintType.NEED_RESULT_POINT_CALLBACK) ? null : (ResultPointCallback)hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK];

            QrPatternFinder finder = new QrPatternFinder(Image, resultPointCallback);
            QrFinderPatternInfo info = finder.find(hints);
            if (info == null) {
                return null;
            }

            return processFinderPatternInfo(info);
        }

        /// <summary>
        /// Processes the finder pattern info.
        /// </summary>
        protected internal virtual DetectorResult processFinderPatternInfo(QrFinderPatternInfo info)
        {
            FinderPattern topLeft = info.TopLeft;
            FinderPattern topRight = info.TopRight;
            FinderPattern bottomLeft = info.BottomLeft;

            float moduleSize = calculateModuleSize(topLeft, topRight, bottomLeft);
            if (moduleSize < 1.0f)
            {
                return null;
            }
            if (!computeDimension(topLeft, topRight, bottomLeft, moduleSize, out var dimension)) {
                return null;
            }

            // QR Code Dimensions determine the Number of Bits saved in them.
            Version provisionalVersion = Version.getProvisionalVersionForDimension(dimension);
            if (provisionalVersion == null) {
                return null;
            }
            int modulesBetweenFpCenters = provisionalVersion.DimensionForVersion - 7;

            AlignmentPattern alignmentPattern = null;
            // Anything above version 1 has another small alignment pattern bottom right
            if (provisionalVersion.AlignmentPatternCenters.Length > 0)
            {

                // Guess where a "bottom right" finder pattern would have been
                float bottomRightX = topRight.X - topLeft.X + bottomLeft.X;
                float bottomRightY = topRight.Y - topLeft.Y + bottomLeft.Y;

                // Estimate that alignment pattern is closer by 3 modules
                // from "bottom right" to known top left location
                float correctionToTopLeft = 1.0f - 3.0f / modulesBetweenFpCenters;
                int estAlignmentX = (int)(topLeft.X + correctionToTopLeft * (bottomRightX - topLeft.X));
                int estAlignmentY = (int)(topLeft.Y + correctionToTopLeft * (bottomRightY - topLeft.Y));

                // Kind of arbitrary -- expand search radius before giving up
                for (int i = 4; i <= 16; i <<= 1)
                {
                    alignmentPattern = findAlignmentInRegion(moduleSize, estAlignmentX, estAlignmentY, i);
                    if (alignmentPattern == null) {
                        continue;
                    }
                    break;
                }
                // If we didn't find alignment pattern... well try anyway without it
            }

            PerspectiveTransform transform = createTransform(topLeft, topRight, bottomLeft, alignmentPattern, dimension);

            BitMatrix bits = Sampler.sampleGrid(dimension, dimension, transform);

            if (bits == null) {
                return null;
            }

            ResultPoint[] points;
            if (alignmentPattern == null)
            {
                points = new ResultPoint[] { bottomLeft, topLeft, topRight };
            }
            else
            {
                points = new ResultPoint[] { bottomLeft, topLeft, topRight, alignmentPattern };
            }
            return new DetectorResult(bits, points);
        }

        private static PerspectiveTransform createTransform(ResultPoint topLeft, ResultPoint topRight, ResultPoint bottomLeft, ResultPoint alignmentPattern, int dimension)
        {
            float dimMinusThree = dimension - 3.5f;
            float bottomRightX;
            float bottomRightY;
            float sourceBottomRightX;
            float sourceBottomRightY;
            if (alignmentPattern != null)
            {
                bottomRightX = alignmentPattern.X;
                bottomRightY = alignmentPattern.Y;
                sourceBottomRightX = sourceBottomRightY = dimMinusThree - 3.0f;
            }
            else
            {
                // Don't have an alignment pattern, just make up the bottom-right point
                bottomRightX = topRight.X - topLeft.X + bottomLeft.X;
                bottomRightY = topRight.Y - topLeft.Y + bottomLeft.Y;
                sourceBottomRightX = sourceBottomRightY = dimMinusThree;
            }

            return XTrafo.quadrilateralToQuadrilateral(
               3.5f,
               3.5f,
               dimMinusThree,
               3.5f,
               sourceBottomRightX,
               sourceBottomRightY,
               3.5f,
               dimMinusThree,
               topLeft.X,
               topLeft.Y,
               topRight.X,
               topRight.Y,
               bottomRightX,
               bottomRightY,
               bottomLeft.X,
               bottomLeft.Y);
        }

        /// <summary> <p>Computes the dimension (number of modules on a size) of the QR Code based on the position
        /// of the finder patterns and estimated module size.</p>
        /// </summary>
        private static bool computeDimension(ResultPoint topLeft, ResultPoint topRight, ResultPoint bottomLeft, float moduleSize, out int dimension)
        {
            int tltrCentersDimension = MathUtils.round(ResultPoint.distance(topLeft, topRight) / moduleSize);
            int tlblCentersDimension = MathUtils.round(ResultPoint.distance(topLeft, bottomLeft) / moduleSize);
            dimension = ((tltrCentersDimension + tlblCentersDimension) >> 1) + 7;
            switch (dimension & 0x03)
            {
                // mod 4
                case 0:
                    dimension++;
                    break;
                // 1? do nothing
                case 2:
                    dimension--;
                    break;
                case 3:
                    return true;
            }
            return true;
        }

        /// <summary>
        ///   <p>Computes an average estimated module size based on estimated derived from the positions
        /// of the three finder patterns.</p>
        /// </summary>
        /// <param name="topLeft">detected top-left finder pattern center</param>
        /// <param name="topRight">detected top-right finder pattern center</param>
        /// <param name="bottomLeft">detected bottom-left finder pattern center</param>
        /// <returns>estimated module size</returns>
        protected internal virtual float calculateModuleSize(ResultPoint topLeft, ResultPoint topRight, ResultPoint bottomLeft)
        {
            // Take the average
            return (calculateModuleSizeOneWay(topLeft, topRight) + calculateModuleSizeOneWay(topLeft, bottomLeft)) / 2.0f;
        }

        /// <summary> <p>Estimates module size based on two finder patterns -- it uses
        /// {@link #sizeOfBlackWhiteBlackRunBothWays(int, int, int, int)} to figure the
        /// width of each, measuring along the axis between their centers.</p>
        /// </summary>
        private float calculateModuleSizeOneWay(ResultPoint pattern, ResultPoint otherPattern)
        {
            float moduleSizeEst1 = sizeOfBlackWhiteBlackRunBothWays((int)pattern.X, (int)pattern.Y, (int)otherPattern.X, (int)otherPattern.Y);
            float moduleSizeEst2 = sizeOfBlackWhiteBlackRunBothWays((int)otherPattern.X, (int)otherPattern.Y, (int)pattern.X, (int)pattern.Y);
            if (float.IsNaN(moduleSizeEst1))
            {
                return moduleSizeEst2 / 7.0f;
            }
            if (float.IsNaN(moduleSizeEst2))
            {
                return moduleSizeEst1 / 7.0f;
            }
            // Average them, and divide by 7 since we've counted the width of 3 black modules,
            // and 1 white and 1 black module on either side. Ergo, divide sum by 14.
            return (moduleSizeEst1 + moduleSizeEst2) / 14.0f;
        }

        /// <summary> See {@link #sizeOfBlackWhiteBlackRun(int, int, int, int)}; computes the total width of
        /// a finder pattern by looking for a black-white-black run from the center in the direction
        /// of another point (another finder pattern center), and in the opposite direction too.
        /// </summary>
        private float sizeOfBlackWhiteBlackRunBothWays(int fromX, int fromY, int toX, int toY)
        {

            float result = sizeOfBlackWhiteBlackRun(fromX, fromY, toX, toY);

            // Now count other way -- don't run off image though of course
            float scale = 1.0f;
            int otherToX = fromX - (toX - fromX);
            if (otherToX < 0)
            {
                scale = fromX / (float)(fromX - otherToX);
                otherToX = 0;
            }
            else if (otherToX >= Image.Width)
            {
                scale = (Image.Width - 1 - fromX) / (float)(otherToX - fromX);
                otherToX = Image.Width - 1;
            }
            int otherToY = (int)(fromY - (toY - fromY) * scale);

            scale = 1.0f;
            if (otherToY < 0)
            {
                scale = fromY / (float)(fromY - otherToY);
                otherToY = 0;
            }
            else if (otherToY >= Image.Height)
            {
                scale = (Image.Height - 1 - fromY) / (float)(otherToY - fromY);
                otherToY = Image.Height - 1;
            }
            otherToX = (int)(fromX + (otherToX - fromX) * scale);

            result += sizeOfBlackWhiteBlackRun(fromX, fromY, otherToX, otherToY);
            return result - 1.0f; // -1 because we counted the middle pixel twice
        }

        /// <summary> <p>This method traces a line from a point in the image, in the direction towards another point.
        /// It begins in a black region, and keeps going until it finds white, then black, then white again.
        /// It reports the distance from the start to this point.</p>
        /// 
        /// <p>This is used when figuring out how wide a finder pattern is, when the finder pattern
        /// may be skewed or rotated.</p>
        /// </summary>
        private float sizeOfBlackWhiteBlackRun(int fromX, int fromY, int toX, int toY)
        {
            // Mild variant of Bresenham's algorithm;
            // see http://en.wikipedia.org/wiki/Bresenham's_line_algorithm
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
            int error = -dx >> 1;
            int xStep = fromX < toX ? 1 : -1;
            int yStep = fromY < toY ? 1 : -1;

            // In black pixels, looking for white, first or second time.
            int state = 0;
            // Loop up until x == toX, but not beyond
            int xLimit = toX + xStep;
            for (int x = fromX, y = fromY; x != xLimit; x += xStep)
            {
                int realX = steep ? y : x;
                int realY = steep ? x : y;

                // Does current pixel mean we have moved white to black or vice versa?
                // Scanning black in state 0,2 and white in state 1, so if we find the wrong
                // color, advance to next state or end if we are in state 2 already
                if (state == 1 == Image[realX, realY])
                {
                    if (state == 2)
                    {
                        return MathUtils.distance(x, y, fromX, fromY);
                    }
                    state++;
                }
                error += dy;
                if (error > 0)
                {
                    if (y == toY)
                    {


                        break;
                    }
                    y += yStep;
                    error -= dx;
                }
            }
            // Found black-white-black; give the benefit of the doubt that the next pixel outside the image
            // is "white" so this last point at (toX+xStep,toY) is the right ending. This is really a
            // small approximation; (toX+xStep,toY+yStep) might be really correct. Ignore this.
            if (state == 2)
            {
                return MathUtils.distance(toX + xStep, toY, fromX, fromY);
            }
            // else we didn't find even black-white-black; no estimate is really possible
            return float.NaN;

        }

        /// <summary>
        ///   <p>Attempts to locate an alignment pattern in a limited region of the image, which is
        /// guessed to contain it. This method uses {@link AlignmentPattern}.</p>
        /// </summary>
        /// <param name="overallEstModuleSize">estimated module size so far</param>
        /// <param name="estAlignmentX">x coordinate of center of area probably containing alignment pattern</param>
        /// <param name="estAlignmentY">y coordinate of above</param>
        /// <param name="allowanceFactor">number of pixels in all directions to search from the center</param>
        /// <returns>
        ///   <see cref="AlignmentPattern"/> if found, or null otherwise
        /// </returns>
        protected AlignmentPattern findAlignmentInRegion(float overallEstModuleSize, int estAlignmentX, int estAlignmentY, float allowanceFactor)
        {
            // Look for an alignment pattern (3 modules in size) around where it
            // should be
            int allowance = (int)(allowanceFactor * overallEstModuleSize);
            int alignmentAreaLeftX = Math.Max(0, estAlignmentX - allowance);
            int alignmentAreaRightX = Math.Min(Image.Width - 1, estAlignmentX + allowance);
            if (alignmentAreaRightX - alignmentAreaLeftX < overallEstModuleSize * 3)
            {
                return null;
            }

            int alignmentAreaTopY = Math.Max(0, estAlignmentY - allowance);
            int alignmentAreaBottomY = Math.Min(Image.Height - 1, estAlignmentY + allowance);

            var alignmentFinder = new AlignmentPatternFinder(
               Image,
               alignmentAreaLeftX,
               alignmentAreaTopY,
               alignmentAreaRightX - alignmentAreaLeftX,
               alignmentAreaBottomY - alignmentAreaTopY,
               overallEstModuleSize,
               resultPointCallback);

            return alignmentFinder.find();
        }
    }

    public interface IDetector { }
}