/*
 * Copyright 2013 ZXing.Net authors
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
using System.Windows.Forms;

using ZXing;

namespace WindowsFormsDemo
{
    public partial class DecodingOptionsForm : Form
    {
        private readonly BarcodeReader _Reader;
        public bool MultipleBarcodes => chkMultipleDecode.Checked;

        public bool MultipleBarcodesOnlyQr => chkMultipleDecodeOnlyQR.Checked;

        public bool UseGlobalHistogramBinarizer => chkUseGlobalHistogramBinarizer.Checked;

        public DecodingOptionsForm(BarcodeReader reader, bool multipleBarcodes, bool multipleBarcodesOnlyQr)
        {
            _Reader = reader;
            InitializeComponent();

            chkMultipleDecode.Checked = multipleBarcodes;
            chkMultipleDecodeOnlyQR.Checked = multipleBarcodesOnlyQr;

            foreach (var val in Enum.GetValues(typeof(BarcodeFormat)))
            {
                var valBarcode = (BarcodeFormat)val;
                if (valBarcode == BarcodeFormat.PLESSEY) {
                    continue;
                }
                var selectedByDefault = valBarcode != BarcodeFormat.MSI &&
                                        valBarcode != BarcodeFormat.IMB;
                if (reader.Options.PossibleFormats != null)
                {
                    selectedByDefault = reader.Options.PossibleFormats.Contains(valBarcode);
                }
                dataGridViewBarcodeFormats.Rows.Add(selectedByDefault, val.ToString());
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            chkTryInverted.Checked = _Reader.TryInverted;
            chkTryHarder.Checked = _Reader.Options.TryHarder;
            chkAutoRotate.Checked = _Reader.AutoRotate;
            chkPureBarcode.Checked = _Reader.Options.PureBarcode;

            chkCode39CheckDigit.Checked = _Reader.Options.AssumeCode39CheckDigit;
            chkCode39ExtendedMode.Checked = _Reader.Options.UseCode39ExtendedMode;
            chkCode39ExtendedModeRelaxed.Checked = _Reader.Options.UseCode39RelaxedExtendedMode;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _Reader.TryInverted = chkTryInverted.Checked;
            _Reader.Options.TryHarder = chkTryHarder.Checked;
            _Reader.AutoRotate = chkAutoRotate.Checked;
            _Reader.Options.PureBarcode = chkPureBarcode.Checked;

            _Reader.Options.AssumeCode39CheckDigit = chkCode39CheckDigit.Checked;
            _Reader.Options.UseCode39ExtendedMode = chkCode39ExtendedMode.Checked;
            _Reader.Options.UseCode39RelaxedExtendedMode = chkCode39ExtendedModeRelaxed.Checked;

            _Reader.Options.PossibleFormats = new List<BarcodeFormat>();

            foreach (DataGridViewRow row in dataGridViewBarcodeFormats.Rows)
            {
                if (((bool)(row.Cells[0].Value)))
                {
                    _Reader.Options.PossibleFormats.Add(
                       (BarcodeFormat)Enum.Parse(typeof(BarcodeFormat), row.Cells[1].Value.ToString()));
                }
            }

            Close();
        }
    }
}
