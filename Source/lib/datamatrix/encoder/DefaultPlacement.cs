/*
 * Copyright 2006 Jeremias Maerki.
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

namespace ZXing.Datamatrix.Encoder
{
    /// <summary>
    /// Symbol Character Placement Program. Adapted from Annex M.1 in ISO/IEC 16022:2000(E).
    /// </summary>
    public class DefaultPlacement
    {

        readonly string codewords;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="codewords">the codewords to place</param>
        /// <param name="numcols">the number of columns</param>
        /// <param name="numrows">the number of rows</param>
        public DefaultPlacement(string codewords, int numcols, int numrows)
        {
            this.codewords = codewords;
            this.Numcols = numcols;
            this.Numrows = numrows;
            Bits = new byte[numcols * numrows];
            SupportClass.Fill(Bits, (byte)2); //Initialize with "not set" value
        }
        /// <summary>
        /// num rows
        /// </summary>
        public int Numrows { get; }

        /// <summary>
        /// num cols
        /// </summary>
        public int Numcols { get; }

        /// <summary>
        /// bits
        /// </summary>
        public byte[] Bits { get; }

        /// <summary>
        /// get a specific bit
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool getBit(int col, int row)
        {
            return Bits[row * Numcols + col] == 1;
        }

        void setBit(int col, int row, bool bit)
        {
            Bits[row * Numcols + col] = (byte)(bit ? 1 : 0);
        }

        bool noBit(int col, int row)
        {
            return Bits[row * Numcols + col] == 2;
        }
        /// <summary>
        /// place
        /// </summary>
        public void place()
        {
            int pos = 0;
            int row = 4;
            int col = 0;

            do
            {
                // repeatedly first check for one of the special corner cases, then...
                if ((row == Numrows) && (col == 0))
                {
                    corner1(pos++);
                }
                if ((row == Numrows - 2) && (col == 0) && ((Numcols % 4) != 0))
                {
                    corner2(pos++);
                }
                if ((row == Numrows - 2) && (col == 0) && (Numcols % 8 == 4))
                {
                    corner3(pos++);
                }
                if ((row == Numrows + 4) && (col == 2) && ((Numcols % 8) == 0))
                {
                    corner4(pos++);
                }
                // sweep upward diagonally, inserting successive characters...
                do
                {
                    if ((row < Numrows) && (col >= 0) && noBit(col, row))
                    {
                        utah(row, col, pos++);
                    }
                    row -= 2;
                    col += 2;
                } while (row >= 0 && (col < Numcols));
                row++;
                col += 3;

                // and then sweep downward diagonally, inserting successive characters, ...
                do
                {
                    if ((row >= 0) && (col < Numcols) && noBit(col, row))
                    {
                        utah(row, col, pos++);
                    }
                    row += 2;
                    col -= 2;
                } while ((row < Numrows) && (col >= 0));
                row += 3;
                col++;

                // ...until the entire array is scanned
            } while ((row < Numrows) || (col < Numcols));

            // Lastly, if the lower right-hand corner is untouched, fill in fixed pattern
            if (noBit(Numcols - 1, Numrows - 1))
            {
                setBit(Numcols - 1, Numrows - 1, true);
                setBit(Numcols - 2, Numrows - 2, true);
            }
        }

        void module(int row, int col, int pos, int bit)
        {
            if (row < 0)
            {
                row += Numrows;
                col += 4 - ((Numrows + 4) % 8);
            }
            if (col < 0)
            {
                col += Numcols;
                row += 4 - ((Numcols + 4) % 8);
            }
            // Note the conversion:
            int v = codewords[pos];
            v &= 1 << (8 - bit);
            setBit(col, row, v != 0);
        }

        /// <summary>
        /// Places the 8 bits of a utah-shaped symbol character in ECC200.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <param name="pos">character position</param>
        void utah(int row, int col, int pos)
        {
            module(row - 2, col - 2, pos, 1);
            module(row - 2, col - 1, pos, 2);
            module(row - 1, col - 2, pos, 3);
            module(row - 1, col - 1, pos, 4);
            module(row - 1, col, pos, 5);
            module(row, col - 2, pos, 6);
            module(row, col - 1, pos, 7);
            module(row, col, pos, 8);
        }

        void corner1(int pos)
        {
            module(Numrows - 1, 0, pos, 1);
            module(Numrows - 1, 1, pos, 2);
            module(Numrows - 1, 2, pos, 3);
            module(0, Numcols - 2, pos, 4);
            module(0, Numcols - 1, pos, 5);
            module(1, Numcols - 1, pos, 6);
            module(2, Numcols - 1, pos, 7);
            module(3, Numcols - 1, pos, 8);
        }

        void corner2(int pos)
        {
            module(Numrows - 3, 0, pos, 1);
            module(Numrows - 2, 0, pos, 2);
            module(Numrows - 1, 0, pos, 3);
            module(0, Numcols - 4, pos, 4);
            module(0, Numcols - 3, pos, 5);
            module(0, Numcols - 2, pos, 6);
            module(0, Numcols - 1, pos, 7);
            module(1, Numcols - 1, pos, 8);
        }

        void corner3(int pos)
        {
            module(Numrows - 3, 0, pos, 1);
            module(Numrows - 2, 0, pos, 2);
            module(Numrows - 1, 0, pos, 3);
            module(0, Numcols - 2, pos, 4);
            module(0, Numcols - 1, pos, 5);
            module(1, Numcols - 1, pos, 6);
            module(2, Numcols - 1, pos, 7);
            module(3, Numcols - 1, pos, 8);
        }

        void corner4(int pos)
        {
            module(Numrows - 1, 0, pos, 1);
            module(Numrows - 1, Numcols - 1, pos, 2);
            module(0, Numcols - 3, pos, 3);
            module(0, Numcols - 2, pos, 4);
            module(0, Numcols - 1, pos, 5);
            module(1, Numcols - 3, pos, 6);
            module(1, Numcols - 2, pos, 7);
            module(1, Numcols - 1, pos, 8);
        }
    }
}