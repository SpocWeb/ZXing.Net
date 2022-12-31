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
using System.Collections.Generic;
using ZXing.Common;
using ZXing.Multi;
using ZXing.Multi.QrCode;
using ZXing.Multi.QrCode.Internal;

namespace ZXing
{
    /// <summary>
    /// A smart class to decode the barcode inside a bitmap object
    /// </summary>
    public class BarcodeReaderGeneric : IBarcodeReaderGeneric
    {
        private static readonly Func<LuminanceSource, Binarizer> defaultCreateBinarizer =
           luminanceSource => new TwoDBinarizer(luminanceSource);

        /// <summary>
        /// represents the default function which is called to get a <see cref="RgbLuminanceSource"/> instance from a raw byte array
        /// </summary>
        protected static readonly Func<byte[], int, int, RgbLuminanceSource.BitmapFormat, LuminanceSource> defaultCreateRGBLuminanceSource =
           (rawBytes, width, height, format) => new RgbLuminanceSource(rawBytes, width, height, format);

        private IBarCodeDecoder reader;
        private readonly Func<byte[], int, int, RgbLuminanceSource.BitmapFormat, LuminanceSource> createRGBLuminanceSource;

        private readonly Func<LuminanceSource, Binarizer> createBinarizer;
        private bool usePreviousState;
        private DecodingOptions options;

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public DecodingOptions Options
        {
            get
            {
                if (options == null)
                {
                    options = new DecodingOptions();
                    options.ValueChanged += (o, args) => usePreviousState = false;
                }
                return options;
            }
            set
            {
                if (value != null)
                {
                    options = value;
                    options.ValueChanged += (o, args) => usePreviousState = false;
                }
                else
                {
                    options = null;
                }
                usePreviousState = false;
            }
        }

        /// <summary>
        /// Gets the reader which should be used to find and decode the barcode.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        protected IBarCodeDecoder Reader => reader ?? (reader = new MultiFormatReader());

        /// <summary>
        /// Gets or sets a method which is called if an important point is found
        /// </summary>
        /// <value>
        /// The result point callback.
        /// </value>
        public event Action<ResultPoint> ResultPointFound
        {
            add
            {
                if (!Options.Hints.ContainsKey(DecodeHintType.NEED_RESULT_POINT_CALLBACK))
                {
                    var callback = new ResultPointCallback(OnResultPointFound);
                    Options.Hints[DecodeHintType.NEED_RESULT_POINT_CALLBACK] = callback;
                }
                explicitResultPointFound += value;
                usePreviousState = false;
            }
            remove
            {
                explicitResultPointFound -= value;
                if (explicitResultPointFound == null) {
                    Options.Hints.Remove(DecodeHintType.NEED_RESULT_POINT_CALLBACK);
                }
                usePreviousState = false;
            }
        }

        private event Action<ResultPoint> explicitResultPointFound;

        /// <summary>
        /// event is executed if a result was found via decode
        /// </summary>
        public event Action<BarCodeText> ResultFound;

        /// <summary>
        /// Gets or sets a value indicating whether the image should be automatically rotated.
        /// Rotation is supported for 90, 180 and 270 degrees
        /// </summary>
        /// <value>
        ///   <c>true</c> if image should be rotated; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRotate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the image should be automatically inverted
        /// if no result is found in the original image.
        /// ATTENTION: Please be carefully because it slows down the decoding process if it is used
        /// </summary>
        /// <value>
        ///   <c>true</c> if image should be inverted; otherwise, <c>false</c>.
        /// </value>
        public bool TryInverted { get; set; }

        /// <summary>
        /// Optional: Gets or sets the function to create a binarizer object for a luminance source.
        /// If null then HybridBinarizer is used
        /// </summary>
        /// <value>
        /// The function to create a binarizer object.
        /// </value>
        protected Func<LuminanceSource, Binarizer> CreateBinarizer => createBinarizer ?? defaultCreateBinarizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeReaderGeneric"/> class.
        /// </summary>
        public BarcodeReaderGeneric() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeReaderGeneric"/> class.
        /// </summary>
        /// <param name="reader">Sets the reader which should be used to find and decode the barcode.
        /// If null then MultiFormatReader is used</param>
        /// <param name="createBinarizer">Sets the function to create a binarizer object for a luminance source.
        /// If null then HybridBinarizer is used</param>
        /// <param name="createRGBLuminanceSource">Sets the function to create a luminance source object for a rgb array.
        /// If null the RGBLuminanceSource is used. The handler is only called when Decode with a byte[] array is called.</param>
        public BarcodeReaderGeneric(IBarCodeDecoder reader = null,
           Func<LuminanceSource, Binarizer> createBinarizer = null,
           Func<byte[], int, int, RgbLuminanceSource.BitmapFormat, LuminanceSource> createRGBLuminanceSource = null
           )
        {
            this.reader = reader ?? new MultiFormatReader();
            this.createBinarizer = createBinarizer ?? defaultCreateBinarizer;
            this.createRGBLuminanceSource = createRGBLuminanceSource ?? defaultCreateRGBLuminanceSource;
            usePreviousState = false;
        }


        /// <summary>
        /// Tries to decode a barcode within an image which is given by a luminance source.
        /// That method gives a chance to prepare a luminance source completely before calling
        /// the time consuming decoding method. On the other hand there is a chance to create
        /// a luminance source which is independent from external resources (like Bitmap objects)
        /// and the decoding call can be made in a background thread.
        /// </summary>
        /// <param name="luminanceSource">The luminance source.</param>
        /// <returns></returns>
        public virtual BarCodeText Decode(LuminanceSource luminanceSource)
        {
            var result = default(BarCodeText);
            var binarizer = CreateBinarizer(luminanceSource);
            var binaryBitmap = new BinaryBitmap(binarizer);
            var multiFormatReader = Reader as MultiFormatReader;
            var rotationCount = 0;
            var rotationMaxCount = 1;

            if (AutoRotate)
            {
                Options.Hints[DecodeHintType.TRY_HARDER_WITHOUT_ROTATION] = true;
                rotationMaxCount = 4;
            }
            else
            {
                if (Options.Hints.ContainsKey(DecodeHintType.TRY_HARDER_WITHOUT_ROTATION)) {
                    Options.Hints.Remove(DecodeHintType.TRY_HARDER_WITHOUT_ROTATION);
                }
            }

            for (; rotationCount < rotationMaxCount; rotationCount++)
            {
                if (usePreviousState && multiFormatReader != null)
                {
                    result = multiFormatReader.DecodeWithState(binaryBitmap);
                }
                else
                {
                    result = Reader.Decode(binaryBitmap, Options.Hints);
                    usePreviousState = true;
                }

                if (result == null)
                {
                    if (TryInverted && luminanceSource.InversionSupported)
                    {
                        binaryBitmap = new BinaryBitmap(CreateBinarizer(luminanceSource.Invert()));
                        if (usePreviousState && multiFormatReader != null)
                        {
                            result = multiFormatReader.DecodeWithState(binaryBitmap);
                        }
                        else
                        {
                            result = Reader.Decode(binaryBitmap, Options.Hints);
                            usePreviousState = true;
                        }
                    }
                }

                if (result != null ||
                    !luminanceSource.RotateSupported ||
                    !AutoRotate) {
                    break;
                }

                luminanceSource = luminanceSource.RotateCounterClockwise();
                binarizer = CreateBinarizer(luminanceSource);
                binaryBitmap = new BinaryBitmap(binarizer);
            }

            if (result != null)
            {
                if (result.ResultMetadata == null)
                {
                    result.PutMetadata(ResultMetadataType.ORIENTATION, rotationCount * 90);
                }
                else if (!result.ResultMetadata.ContainsKey(ResultMetadataType.ORIENTATION))
                {
                    result.ResultMetadata[ResultMetadataType.ORIENTATION] = rotationCount * 90;
                }
                else
                {
                    // perhaps the core decoder rotates the image already (can happen if TryHarder is specified)
                    result.ResultMetadata[ResultMetadataType.ORIENTATION] = ((int)result.ResultMetadata[ResultMetadataType.ORIENTATION] + rotationCount * 90) % 360;
                }

                OnResultFound(result);
            }

            return result;
        }

        /// <summary>
        /// Tries to decode barcodes within an image which is given by a luminance source.
        /// That method gives a chance to prepare a luminance source completely before calling
        /// the time consuming decoding method. On the other hand there is a chance to create
        /// a luminance source which is independent from external resources (like Bitmap objects)
        /// and the decoding call can be made in a background thread.
        /// </summary>
        /// <param name="luminanceSource">The luminance source.</param>
        /// <returns></returns>
        public virtual BarCodeText[] DecodeMultiple(LuminanceSource luminanceSource)
        {
            var results = default(BarCodeText[]);
            var binarizer = CreateBinarizer(luminanceSource);
            var binaryBitmap = new BinaryBitmap(binarizer);
            var rotationCount = 0;
            var rotationMaxCount = 1;
            IMultipleBarcodeReader multiReader = null;

            if (AutoRotate)
            {
                Options.Hints[DecodeHintType.TRY_HARDER_WITHOUT_ROTATION] = true;
                rotationMaxCount = 4;
            }

            var formats = Options.PossibleFormats;
            if (formats?.Count == 1 && formats.Contains(BarcodeFormat.QR_CODE))
            {
                multiReader = new QrCodeMultiReader();
            }
            else
            {
                multiReader = new GenericMultipleBarcodeReader(Reader);
            }

            var gridSampler = new LuminanceGridSampler(luminanceSource);
            for (; rotationCount < rotationMaxCount; rotationCount++)
            {
                var multiQrDetector = new MultiQrDetector(binaryBitmap);
                var detectorResults = multiQrDetector.DetectMulti(Options.Hints);

                results = multiReader.DecodeMultiple(detectorResults, Options.Hints);
                //results = multiReader.DecodeMultiple(binaryBitmap, Options.Hints);
                //results = multiReader.decodeMultiple(gridSampler, Options.Hints);

                if (results == null)
                {
                    if (TryInverted && luminanceSource.InversionSupported)
                    {
                        binaryBitmap = new BinaryBitmap(CreateBinarizer(luminanceSource.Invert()));
                        results = multiReader.DecodeMultiple(binaryBitmap, Options.Hints);
                    }
                }

                if (results != null ||
                    !luminanceSource.RotateSupported ||
                    !AutoRotate) {
                    break;
                }

                binaryBitmap = new BinaryBitmap(CreateBinarizer(luminanceSource.RotateCounterClockwise()));
            }

            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.ResultMetadata == null)
                    {
                        result.PutMetadata(ResultMetadataType.ORIENTATION, rotationCount * 90);
                    }
                    else if (!result.ResultMetadata.ContainsKey(ResultMetadataType.ORIENTATION))
                    {
                        result.ResultMetadata[ResultMetadataType.ORIENTATION] = rotationCount * 90;
                    }
                    else
                    {
                        // perhaps the core decoder rotates the image already (can happen if TryHarder is specified)
                        result.ResultMetadata[ResultMetadataType.ORIENTATION] =
                           ((int)result.ResultMetadata[ResultMetadataType.ORIENTATION] + rotationCount * 90) % 360;
                    }
                }

                OnResultsFound(results);
            }

            return results;
        }

        /// <summary>
        /// raises the ResultFound event
        /// </summary>
        /// <param name="results"></param>
        protected void OnResultsFound(IEnumerable<BarCodeText> results)
        {
            if (ResultFound == null)
            {
                return;
            }

            foreach (var result in results)
            {
                ResultFound(result);
            }
        }

        /// <summary>
        /// raises the ResultFound event
        /// </summary>
        /// <param name="result"></param>
        protected void OnResultFound(BarCodeText result)
        {
            ResultFound?.Invoke(result);
        }

        /// <summary>
        /// calls the explicitResultPointFound action
        /// </summary>
        /// <param name="resultPoint"></param>
        protected void OnResultPointFound(ResultPoint resultPoint)
        {
            explicitResultPointFound?.Invoke(resultPoint);
        }

        /// <summary>
        /// Decodes the specified barcode bitmap.
        /// </summary>
        /// <param name="rawRGB">The image as byte[] array.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        /// <returns>
        /// the result data or null
        /// </returns>
        public BarCodeText Decode(byte[] rawRGB, int width, int height, RgbLuminanceSource.BitmapFormat format)
        {
            if (rawRGB == null) {
                throw new ArgumentNullException("rawRGB");
            }

            var luminanceSource = createRGBLuminanceSource(rawRGB, width, height, format);

            return Decode(luminanceSource);
        }

        /// <summary>
        /// Decodes the specified barcode bitmap.
        /// </summary>
        /// <param name="rawRGB">The image as byte[] array.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        /// <returns>
        /// the result data or null
        /// </returns>
        public BarCodeText[] DecodeMultiple(byte[] rawRGB, int width, int height, RgbLuminanceSource.BitmapFormat format)
        {
            if (rawRGB == null) {
                throw new ArgumentNullException("rawRGB");
            }

            var luminanceSource = createRGBLuminanceSource(rawRGB, width, height, format);

            return DecodeMultiple(luminanceSource);
        }
    }
}
