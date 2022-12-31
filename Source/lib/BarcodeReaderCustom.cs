﻿/*
 * Copyright 2012 ZXing.Net authors
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

namespace ZXing
{
    /// <summary>
    /// A smart class to decode the barcode inside a bitmap object
    /// </summary>
    public class BarcodeReader<T> : BarcodeReaderGeneric, IBarcodeReader<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeReader"/> class.
        /// </summary>
        public BarcodeReader(Func<T, LuminanceSource> createLuminanceSource = null)
           : this(null, createLuminanceSource, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeReader"/> class.
        /// </summary>
        /// <param name="reader">Sets the reader which should be used to find and decode the barcode.
        /// If null then MultiFormatReader is used</param>
        /// <param name="createLuminanceSource">Sets the function to create a luminance source object for a bitmap.
        /// If null, an exception is thrown when Decode is called</param>
        /// <param name="createBinarizer">Sets the function to create a binarizer object for a luminance source.
        /// If null then HybridBinarizer is used</param>
        public BarcodeReader(IBarCodeDecoder reader = null,
           Func<T, LuminanceSource> createLuminanceSource = null,
           Func<LuminanceSource, Binarizer> createBinarizer = null
        )
           : this(reader, createLuminanceSource, createBinarizer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeReader"/> class.
        /// </summary>
        /// <param name="reader">Sets the reader which should be used to find and decode the barcode.
        /// If null then MultiFormatReader is used</param>
        /// <param name="createLuminanceSource">Sets the function to create a luminance source object for a bitmap.
        /// If null, an exception is thrown when Decode is called</param>
        /// <param name="createBinarizer">Sets the function to create a binarizer object for a luminance source.
        /// If null then HybridBinarizer is used</param>
        /// <param name="createRgbLuminanceSource">Sets the function to create a luminance source object for a rgb raw byte array.</param>
        public BarcodeReader(IBarCodeDecoder reader = null,
           Func<T, LuminanceSource> createLuminanceSource = null,
           Func<LuminanceSource, Binarizer> createBinarizer = null,
           Func<byte[], int, int, RgbLuminanceSource.BitmapFormat, LuminanceSource> createRgbLuminanceSource = null
        )
           : base(reader, createBinarizer, createRgbLuminanceSource)
        {
            CreateLuminanceSource = createLuminanceSource;
        }

        /// <summary>
        /// Constructor for additional image formats for one BarcodeReader class
        /// </summary>
        /// <param name="reader">Sets the reader which should be used to find and decode the barcode.
        /// If null then MultiFormatReader is used</param>
        /// <param name="createBinarizer">Sets the function to create a binarizer object for a luminance source.
        /// If null then HybridBinarizer is used</param>
        /// <param name="createRGBLuminanceSource">Sets the function to create a luminance source object for a rgb raw byte array.</param>
        protected BarcodeReader(IBarCodeDecoder reader,
           Func<LuminanceSource, Binarizer> createBinarizer,
           Func<byte[], int, int, RgbLuminanceSource.BitmapFormat, LuminanceSource> createRGBLuminanceSource
        )
           : base(reader, createBinarizer, createRGBLuminanceSource)
        {
        }

        /// <summary>
        /// Optional: Gets or sets the function to create a luminance source object for a bitmap.
        /// If null a platform specific default LuminanceSource is used
        /// </summary>
        /// <value>
        /// The function to create a luminance source object.
        /// </value>
        protected Func<T, LuminanceSource> CreateLuminanceSource { get; }

        /// <summary>
        /// Decodes the specified barcode bitmap.
        /// </summary>
        /// <param name="barcodeBitmap">The barcode bitmap.</param>
        /// <returns>the result data or null</returns>
        public BarCodeText Decode(T barcodeBitmap)
        {
            if (CreateLuminanceSource == null)
            {
                throw new InvalidOperationException("You have to declare a luminance source delegate.");
            }

            if (barcodeBitmap == null) {
                throw new ArgumentNullException(nameof(barcodeBitmap));
            }

            var luminanceSource = CreateLuminanceSource(barcodeBitmap);

            return Decode(luminanceSource);
        }

        /// <summary> Tries to find multiple BarCodes from <paramref name="barcodeBitmap"/> </summary>
        /// <returns>the result data or null</returns>
        public BarCodeText[] DecodeMultiple(T barcodeBitmap)
        {
            if (CreateLuminanceSource == null)
            {
                throw new InvalidOperationException("You have to declare a luminance source delegate.");
            }

            if (barcodeBitmap == null) {
                throw new ArgumentNullException(nameof(barcodeBitmap));
            }

            var luminanceSource = CreateLuminanceSource(barcodeBitmap);
            try {
                return DecodeMultiple(luminanceSource);
            } catch {
                return Array.Empty<BarCodeText>();
            }
        }
    }
}
