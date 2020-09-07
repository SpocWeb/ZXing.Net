/*
 * Copyright 2010 ZXing authors
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

namespace ZXing.OneD
{

    /// <summary>
    /// <p>Decodes PharmaCode</p>
    /// * @author Ruslan Karachun
    /// </summary>
    public sealed class PharmaCodeReader : OneDReader
    {

        static bool _IS_BLACK = true;
        static bool _IS_WHITE = false;

        internal class PixelInterval
        {
            public PixelInterval(bool c, int l)
            {
                Color = c;
                Length = l;
            }

            public bool Color { get; }
            public int Length { get; }
            public int Similar { get; private set; }
            public int Small { get; private set; }
            public int Large { get; private set; }

            public void IncSimilar()
            {
                Similar++;
            }

            public void IncSmall()
            {
                Small++;
            }

            public void IncLarge()
            {
                Large++;
            }
        }

        public static double Mean(double[] m)
        {
            double sum = 0;
            int l = m.Length;
            for (int i = 0; i < l; i++)
            {
                sum += m[i];
            }

            return sum / m.Length;
        }

        /// <summary>
        ///   <p>Attempts to decode a one-dimensional barcode format given a single row of
        /// an image.</p>
        /// </summary>
        /// <param name="rowNumber">row number from top of the row</param>
        /// <param name="row">the black/white pixel data of the row</param>
        /// <param name="hints">decode hints</param>
        /// <returns>
        ///   <see cref="BarCodeText"/>containing encoded string and start/end of barcode or null, if an error occurs or barcode cannot be found
        /// </returns>
        public override BarCodeText DecodeRow(int rowNumber, BitArray row, IDictionary<DecodeHintType, object> hints)
        {
            var gaps = new List<PixelInterval>();

            bool color = row[0];
            int end = row.Size;
            int num = 0;

            for (int i = 0; i < end; i++)
            {
                bool currentColor = row[i];
                if (currentColor == color)
                {
                    num++;
                }
                else
                {
                    gaps.Add(new PixelInterval(color, num));
                    color = currentColor;
                    num = 1;
                }
            }

            gaps.Add(new PixelInterval(color, num));

            int gapsLength = gaps.Count;
            for (int i = 0; i < gapsLength; i++)
            {
                PixelInterval primary = gaps[i];
                bool pColor = primary.Color;
                int pNum = primary.Length; // количество пикселей
                for (int j = 0; j < gapsLength; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    int sNum = gaps[j].Length;
                    bool sColor = gaps[j].Color;
                    double multiplier = pNum > sNum ? (double) pNum / sNum : (double) sNum / pNum;
                    //System.out.println("multiplier: " + multiplier);
                    if (pColor == _IS_WHITE && sColor == _IS_WHITE)
                    {
                        // WHITE WHITE
                        if (multiplier <= 1.2222)
                        {
                            primary.IncSimilar();
                        }
                    }
                    else if (pColor == _IS_WHITE && sColor == _IS_BLACK)
                    {
                        // WHITE BLACK
                        if (multiplier > 1.5 && multiplier < 3.6667 && pNum > sNum)
                        {
                            // White and small black
                            primary.IncSimilar();
                        }
                        else if (multiplier > 1.2727 && multiplier < 2.7778 && pNum < sNum)
                        {
                            // White and large black
                            primary.IncSimilar();
                        }
                    }
                    else if (pColor == _IS_BLACK && sColor == _IS_WHITE)
                    {
                        // BLACK WHITE
                        if (multiplier > 1.5 && multiplier < 3.6667 && pNum < sNum)
                        {
                            // Small black and white
                            primary.IncSimilar();
                            primary.IncSmall();
                        }
                        else if (multiplier > 1.2727 && multiplier < 2.7778 && pNum > sNum)
                        {
                            // large black and white
                            primary.IncSimilar();
                            primary.IncLarge();
                        }
                    }
                    else if (pColor == _IS_BLACK && sColor == _IS_BLACK)
                    {
                        // BLACK BLACK
                        if (multiplier > 2.3333 && multiplier < 4.6667)
                        {
                            primary.IncSimilar();
                            if (pNum > sNum)
                            {
                                primary.IncLarge();
                            }
                            else
                            {
                                primary.IncSmall();
                            }
                        }
                        else if (multiplier < 2)
                        {
                            primary.IncSimilar();
                        }
                    }
                } // j
            } // i

            var iResult = FinalProcessing(gaps);
            if (iResult == null || iResult < 3 || iResult > 131070)
            {
                return null;
            }

            string resultString = iResult.ToString();
//Counter counter = Counter.getInstance(25);
//counter.addCode(iResult);

//    String sRowNumber = Integer.toString(rowNumber);
//    String url = "https://dev.aptinfo.net/p/" + resultString;
//    final HttpRequestFactory requestFactory = new NetHttpTransport().createRequestFactory();
//new Thread(new Runnable()
//{
//    @Override
//        public void run()
//    {
//        try
//        {
//            HttpRequest request = requestFactory.buildGetRequest(new GenericUrl(url));
//            HttpResponse httpResponse = request.execute();
//        }
//        catch (IOException e)
//        {
//            //e.printStackTrace();
//        }
//    }
//}).start();

//    if ( ! counter.isCodeValid(iResult) ) {
//        throw NotFoundException.getNotFoundInstance();
//    }

            float left = 0.0f;
            float right = end - 1;
            return new BarCodeText(resultString, null, row, new[]
                {
                    new ResultPoint(left, rowNumber),
                    new ResultPoint(right, rowNumber)
                },
                BarcodeFormat.PHARMA_CODE
            );

        }


        int? FinalProcessing(IReadOnlyList<PixelInterval> gaps)
        {
            int l = gaps.Count;
            double[]
                similars = new double[l];
            for (int i = 0; i < l; i++)
            {
                similars[i] = gaps[i].Similar;
            }

            double dMean = Mean(similars);
            bool inProgress = false;
            string fStr = "";
            string cStr = "";
            for (int i = 0; i < l; i++)
            {
                PixelInterval gap = gaps[i];
                bool color = gap.Color;
                double sim = gap.Similar;
                if (color == _IS_WHITE && !inProgress && sim < dMean)
                {
                    //System.out.println("start");
                    inProgress = true;
                    continue;
                }

                if (inProgress && sim < dMean)
                {
                    //System.out.println("Similar is " + sim + " < " + dMean + " => BREAK");
                    if (color == _IS_BLACK)
                    {
                        return null;
                    }

                    if (color == _IS_WHITE && i + 1 != l)
                    {
                        return null;
                    }
                }

                if (i + 1 == l && gap.Color == _IS_BLACK)
                {
                    //System.out.println("last gap");
                    return null;
                }

                if (inProgress && color == _IS_BLACK)
                {
                    if (gap.Large > gap.Small)
                    {
                        fStr += '1';
                        cStr += '#';
                    }
                    else
                    {
                        fStr += '0';
                        cStr += '=';
                    }
                }
            }

            //System.out.println("Str: "+ fStr +" "+ cStr);
            string stg2 = '1' + fStr;
            int retVal = Convert.ToInt32(stg2, 2) - 1;
            //System.out.println(ret_val);
            return retVal;
        }
    }
}
