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
    /// <summary> Implementations of this class can, given locations of finder patterns for a QR code in an
    /// image, sample the right points in the image to reconstruct the QR code, accounting for
    /// perspective distortion. It is abstracted since it is relatively expensive and should be allowed
    /// to take advantage of platform-specific optimized implementations, like Sun's Java Advanced
    /// Imaging library, but which may not be available in other environments such as J2ME, and vice
    /// versa.
    /// 
    /// The implementation used can be controlled by calling {@link #setGridSampler(GridSampler)}
    /// with an instance of a class which implements this interface.
    /// </summary>
    /// <author> Sean Owen</author>
    public abstract class GridSampler : IGridSampler
    {

        /// <summary> Samples an image for a square matrix of bits of the given dimension. </summary>
        /// <remarks>
        /// <p> This is used to extract the black/white modules of a 2D barcode
        /// like a QR Code found in an image.
        /// Because this barcode may be rotated or perspective-distorted,
        /// the caller supplies four points in the source image
        /// that define known points in the barcode,
        /// so that the image may be sampled appropriately.</p>
        /// 
        /// <p>The last eight "from" parameters are four X/Y coordinate pairs
        /// of locations of points in the image
        /// that define some significant points in the image to be sample.
        /// For example, these may be the locations of the 3 or 4 finder patterns in a QR Code.</p>
        /// 
        /// <p>The first eight "to" parameters are four X/Y coordinate pairs
        /// measured in the destination <see cref="BitMatrix"/>, from the top left,
        /// where the known points in the image given by the "from" parameters map to.</p>
        /// <p>These 16 parameters define the transformation needed to sample the image.</p>
        /// </remarks>
        /// <param name="image">image to sample</param>
        /// <param name="dimensionX">The dimension X.</param>
        /// <param name="dimensionY">The dimension Y.</param>
        /// <param name="p1ToX">The p1 pre-image X.</param>
        /// <param name="p1ToY">The p1 pre-image  Y.</param>
        /// <param name="p2ToX">The p2 pre-image  X.</param>
        /// <param name="p2ToY">The p2 pre-image  Y.</param>
        /// <param name="p3ToX">The p3 pre-image  X.</param>
        /// <param name="p3ToY">The p3 pre-image  Y.</param>
        /// <param name="p4ToX">The p4 pre-image  X.</param>
        /// <param name="p4ToY">The p4 pre-image  Y.</param>
        /// <param name="p1FromX">The p1 image X.</param>
        /// <param name="p1FromY">The p1 image Y.</param>
        /// <param name="p2FromX">The p2 image X.</param>
        /// <param name="p2FromY">The p2 image Y.</param>
        /// <param name="p3FromX">The p3 image X.</param>
        /// <param name="p3FromY">The p3 image Y.</param>
        /// <param name="p4FromX">The p4 image X.</param>
        /// <param name="p4FromY">The p4 image Y.</param>
        /// <returns>
        /// <see cref="BitMatrix"/> representing a grid of points sampled from the image within a region
        /// defined by the "from" parameters
        /// </returns>
        /// <throws>  ReaderException if image can't be sampled, for example, if the transformation defined </throws>
        public BitMatrix SampleGrid(int dimensionX, int dimensionY
            , float p1ToX, float p1ToY, float p2ToX, float p2ToY, float p3ToX, float p3ToY, float p4ToX, float p4ToY
            , float p1FromX, float p1FromY, float p2FromX, float p2FromY, float p3FromX, float p3FromY, float p4FromX,
            float p4FromY) {
            PerspectiveTransform transform = XTrafo.QuadrilateralToQuadrilateral(
                p1ToX, p1ToY, p2ToX, p2ToY, p3ToX, p3ToY, p4ToX, p4ToY,
                p1FromX, p1FromY, p2FromX, p2FromY, p3FromX, p3FromY, p4FromX, p4FromY);
            return SampleGrid(dimensionX, dimensionY, transform);
        }

        /// <inheritdoc />
        public abstract BitMatrix GetImage();

        /// <summary>
        /// 
        /// </summary>
        public abstract BitMatrix SampleGrid(int dimensionX, int dimensionY, PerspectiveTransform transform);

    }

    public interface IGridSampler
    {
        BitMatrix SampleGrid(int dimensionX, int dimensionY, PerspectiveTransform transform);
        BitMatrix SampleGrid(int dimension1, int dimension2, float low1, float low2, float high1, float low3, float high2, float high3, float low4, float high4, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);
        BitMatrix GetImage();

    }
}