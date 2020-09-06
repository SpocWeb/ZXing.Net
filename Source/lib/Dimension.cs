/*
 * Copyright 2012 ZXing authors
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
    /// Simply encapsulates a width and height.
    /// </summary>
    public sealed class Dimension
    {

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Dimension(int width, int height)
        {
            if (width < 0 || height < 0)
            {
                throw new ArgumentException();
            }
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// the width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// the height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object other)
        {
            if (other is Dimension)
            {
                var d = (Dimension)other;
                return Width == d.Width && Height == d.Height;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Width * 32713 + Height;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Width + "x" + Height;
        }
    }
}