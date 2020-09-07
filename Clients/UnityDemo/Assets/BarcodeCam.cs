/*
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

using System.Threading;

using UnityEngine;

using ZXing;
using ZXing.QrCode;

public class BarcodeCam : MonoBehaviour
{
    // Texture for encoding test
    public Texture2D Encoded;

    private WebCamTexture _CamTexture;
    private Thread _QrThread;

    private Color32[] _C;
    private int W, H;

    private Rect _ScreenRect;

    private bool _IsQuit;

    public string LastResult;
    private bool _ShouldEncodeNow;

    void OnGUI()
    {
        GUI.DrawTexture(_ScreenRect, _CamTexture, ScaleMode.ScaleToFit);
    }

    void OnEnable()
    {
        if (_CamTexture != null)
        {
            _CamTexture.Play();
            W = _CamTexture.width;
            H = _CamTexture.height;
        }
    }

    void OnDisable()
    {
        if (_CamTexture != null)
        {
            _CamTexture.Pause();
        }
    }

    void OnDestroy()
    {
        _QrThread.Abort();
        _CamTexture.Stop();
    }

    // It's better to stop the thread by itself rather than abort it.
    void OnApplicationQuit()
    {
        _IsQuit = true;
    }

    void Start()
    {
        Encoded = new Texture2D(256, 256);
        LastResult = "http://www.google.com";
        _ShouldEncodeNow = true;

        _ScreenRect = new Rect(0, 0, Screen.width, Screen.height);

        _CamTexture = new WebCamTexture();
        _CamTexture.requestedHeight = Screen.height; // 480;
        _CamTexture.requestedWidth = Screen.width; //640;
        OnEnable();

        _QrThread = new Thread(DecodeQr);
        _QrThread.Start();
    }

    void Update()
    {
        if (_C == null)
        {
            _C = _CamTexture.GetPixels32();
        }

        // encode the last found
        var textForEncoding = LastResult;
        if (_ShouldEncodeNow &&
            textForEncoding != null)
        {
            var color32 = Encode(textForEncoding, Encoded.width, Encoded.height);
            Encoded.SetPixels32(color32);
            Encoded.Apply();
            _ShouldEncodeNow = false;
        }
    }

    void DecodeQr()
    {
        // create a reader with a custom luminance source
        var barcodeReader = new BarcodeReader { AutoRotate = false, TryHarder = false };

        while (true)
        {
            if (_IsQuit) {
                break;
            }

            try
            {
                // decode the current frame
                var result = barcodeReader.Decode(_C, W, H);
                if (result != null)
                {
                    LastResult = result.Text;
                    _ShouldEncodeNow = true;
                    print(result.Text);
                }

                // Sleep a little bit and set the signal to get the next frame
                Thread.Sleep(200);
                _C = null;
            }
            catch
            {
            }
        }
    }

    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }
}
