using System;
using System.Text;


namespace BigIntegerLibrary
{

    /// <summary>
    /// Integer inefficiently represented internally using base-10 digits, in order to allow a
    /// visual representation as a base-10 string. Only for internal use.
    /// </summary>
    sealed class Base10BigInteger
    {

        #region Fields

        /// <summary>
        /// 10 numeration base for string representation, very inefficient for computations.
        /// </summary>
        private const long NumberBase = 10;

        /// <summary>
        /// Maximum size for numbers is up to 10240 binary digits or approximately (safe to use) 3000 decimal digits.
        /// The maximum size is, in fact, double the previously specified amount, in order to accommodate operations'
        /// overflow.
        /// </summary>
        private const int MaxSize = BigInteger.MAX_SIZE * 5;


        /// Integer constants
        private static readonly Base10BigInteger Zero = new Base10BigInteger();
        private static readonly Base10BigInteger One = new Base10BigInteger(1);


        /// <summary>
        /// The array of digits of the number.
        /// </summary>
        private DigitContainer _Digits;

        /// <summary>
        /// The actual number of digits of the number.
        /// </summary>
        private int _Size;

        /// <summary>
        /// The number sign.
        /// </summary>
        private Sign _Sign;


        #endregion


        #region Internal Fields


        /// <summary>
        /// Sets the number sign.
        /// </summary>
        internal Sign NumberSign
        {
            set => _Sign = value;
        }


        #endregion


        #region Constructors


        /// <summary>
        /// Default constructor, intializing the Base10BigInteger with zero.
        /// </summary>
        public Base10BigInteger()
        {
            _Digits = new DigitContainer();
            _Size = 1;
            _Digits[_Size] = 0;
            _Sign = Sign.POSITIVE;
        }

        /// <summary>
        /// Constructor creating a new Base10BigInteger as a conversion of a regular base-10 long.
        /// </summary>
        /// <param name="n">The base-10 long to be converted</param>
        public Base10BigInteger(long n)
        {
            _Digits = new DigitContainer();
            _Sign = Sign.POSITIVE;

            if (n == 0)
            {
                _Size = 1;
                _Digits[_Size] = 0;
            }

            else
            {
                if (n < 0)
                {
                    n = -n;
                    _Sign = Sign.NEGATIVE;
                }

                _Size = 0;
                while (n > 0)
                {
                    _Digits[_Size] = n % NumberBase;
                    n /= NumberBase;
                    _Size++;
                }
            }
        }

        /// <summary>
        /// Constructor creating a new Base10BigInteger as a copy of an existing Base10BigInteger.
        /// </summary>
        /// <param name="n">The Base10BigInteger to be copied</param>
        public Base10BigInteger(Base10BigInteger n)
        {
            _Digits = new DigitContainer();
            _Size = n._Size;
            _Sign = n._Sign;

            for (int i = 0; i < n._Size; i++)
            {
                _Digits[i] = n._Digits[i];
            }
        }


        #endregion


        #region Public Methods


        /// <summary>
        /// Determines whether the specified Base10BigInteger is equal to the current Base10BigInteger.
        /// </summary>
        /// <param name="other">The Base10BigInteger to compare with the current Base10BigInteger</param>
        /// <returns>True if the specified Base10BigInteger is equal to the current Base10BigInteger,
        /// false otherwise</returns>
        public bool Equals(Base10BigInteger other)
        {
            if (_Sign != other._Sign) {
                return false;
            }
            if (_Size != other._Size) {
                return false;
            }

            for (int i = 0; i < _Size; i++)
            {
                if (_Digits[i] != other._Digits[i]) {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current Base10BigInteger.
        /// </summary>
        /// <param name="o">The System.Object to compare with the current Base10BigInteger</param>
        /// <returns>True if the specified System.Object is equal to the current Base10BigInteger,
        /// false otherwise</returns>
        public override bool Equals(object o)
        {
            if ((o is Base10BigInteger) == false) {
                return false;
            }

            return Equals((Base10BigInteger)o);
        }

        /// <summary>
        /// Serves as a hash function for the Base10BigInteger type.
        /// </summary>
        /// <returns>A hash code for the current Base10BigInteger</returns>
        public override int GetHashCode()
        {
            int result = 0;

            for (int i = 0; i < _Size; i++)
            {
                result = result + (int)_Digits[i];
            }

            return result;
        }

        /// <summary>
        /// String representation of the current Base10BigInteger, converted to its base-10 representation.
        /// </summary>
        /// <returns>The string representation of the current Base10BigInteger</returns>
        public override string ToString()
        {
            StringBuilder output;

            if (_Sign == Sign.NEGATIVE)
            {
                output = new StringBuilder(_Size + 1);
                output.Append('-');
            }

            else {
                output = new StringBuilder(_Size);
            }

            for (int i = _Size - 1; i >= 0; i--)
            {
                output.Append(_Digits[i]);
            }

            return output.ToString();
        }

        /// <summary>
        /// Base10BigInteger inverse with respect to addition.
        /// </summary>
        /// <param name="n">The Base10BigInteger whose opposite is to be computed</param>
        /// <returns>The Base10BigInteger inverse with respect to addition</returns>
        public static Base10BigInteger Opposite(Base10BigInteger n)
        {
            Base10BigInteger res = new Base10BigInteger(n);

            if (res != Zero)
            {
                if (res._Sign == Sign.POSITIVE) {
                    res._Sign = Sign.NEGATIVE;
                } else {
                    res._Sign = Sign.POSITIVE;
                }
            }

            return res;
        }

        /// <summary>
        /// Greater test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &gt; b, false otherwise</returns>
        public static bool Greater(Base10BigInteger a, Base10BigInteger b)
        {
            if (a._Sign != b._Sign)
            {
                if ((a._Sign == Sign.NEGATIVE) && (b._Sign == Sign.POSITIVE)) {
                    return false;
                }

                if ((a._Sign == Sign.POSITIVE) && (b._Sign == Sign.NEGATIVE)) {
                    return true;
                }
            }

            else
            {
                if (a._Sign == Sign.POSITIVE)
                {
                    if (a._Size > b._Size) {
                        return true;
                    }
                    if (a._Size < b._Size) {
                        return false;
                    }
                    for (int i = (a._Size) - 1; i >= 0; i--)
                    {
                        if (a._Digits[i] > b._Digits[i]) {
                            return true;
                        } else if (a._Digits[i] < b._Digits[i]) {
                            return false;
                        }
                    }
                }

                else
                {
                    if (a._Size < b._Size) {
                        return true;
                    }
                    if (a._Size > b._Size) {
                        return false;
                    }
                    for (int i = (a._Size) - 1; i >= 0; i--)
                    {
                        if (a._Digits[i] < b._Digits[i]) {
                            return true;
                        } else if (a._Digits[i] > b._Digits[i]) {
                            return false;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Greater or equal test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &gt;= b, false otherwise</returns>
        public static bool GreaterOrEqual(Base10BigInteger a, Base10BigInteger b)
        {
            return Greater(a, b) || Equals(a, b);
        }

        /// <summary>
        /// Smaller test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &lt; b, false otherwise</returns>
        public static bool Smaller(Base10BigInteger a, Base10BigInteger b)
        {
            return !GreaterOrEqual(a, b);
        }

        /// <summary>
        /// Smaller or equal test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &lt;= b, false otherwise</returns>
        public static bool SmallerOrEqual(Base10BigInteger a, Base10BigInteger b)
        {
            return !Greater(a, b);
        }

        /// <summary>
        /// Computes the absolute value of a Base10BigInteger.
        /// </summary>
        /// <param name="n">The Base10BigInteger whose absolute value is to be computed</param>
        /// <returns>The absolute value of the given BigInteger</returns>
        public static Base10BigInteger Abs(Base10BigInteger n)
        {
            Base10BigInteger res = new Base10BigInteger(n);
            res._Sign = Sign.POSITIVE;
            return res;
        }

        /// <summary>
        /// Addition operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the addition</returns>
        public static Base10BigInteger Addition(Base10BigInteger a, Base10BigInteger b)
        {
            Base10BigInteger res = null;

            if ((a._Sign == Sign.POSITIVE) && (b._Sign == Sign.POSITIVE))
            {
                if (a >= b) {
                    res = Add(a, b);
                } else {
                    res = Add(b, a);
                }

                res._Sign = Sign.POSITIVE;
            }

            if ((a._Sign == Sign.NEGATIVE) && (b._Sign == Sign.NEGATIVE))
            {
                if (a <= b) {
                    res = Add(-a, -b);
                } else {
                    res = Add(-b, -a);
                }

                res._Sign = Sign.NEGATIVE;
            }

            if ((a._Sign == Sign.POSITIVE) && (b._Sign == Sign.NEGATIVE))
            {
                if (a >= (-b))
                {
                    res = Subtract(a, -b);
                    res._Sign = Sign.POSITIVE;
                }
                else
                {
                    res = Subtract(-b, a);
                    res._Sign = Sign.NEGATIVE;
                }
            }

            if ((a._Sign == Sign.NEGATIVE) && (b._Sign == Sign.POSITIVE))
            {
                if ((-a) <= b)
                {
                    res = Subtract(b, -a);
                    res._Sign = Sign.POSITIVE;
                }
                else
                {
                    res = Subtract(-a, b);
                    res._Sign = Sign.NEGATIVE;
                }
            }

            return res;
        }

        /// <summary>
        /// Subtraction operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the subtraction</returns>
        public static Base10BigInteger Subtraction(Base10BigInteger a, Base10BigInteger b)
        {
            Base10BigInteger res = null;

            if ((a._Sign == Sign.POSITIVE) && (b._Sign == Sign.POSITIVE))
            {
                if (a >= b)
                {
                    res = Subtract(a, b);
                    res._Sign = Sign.POSITIVE;
                }
                else
                {
                    res = Subtract(b, a);
                    res._Sign = Sign.NEGATIVE;
                }
            }

            if ((a._Sign == Sign.NEGATIVE) && (b._Sign == Sign.NEGATIVE))
            {
                if (a <= b)
                {
                    res = Subtract(-a, -b);
                    res._Sign = Sign.NEGATIVE;
                }
                else
                {
                    res = Subtract(-b, -a);
                    res._Sign = Sign.POSITIVE;
                }
            }

            if ((a._Sign == Sign.POSITIVE) && (b._Sign == Sign.NEGATIVE))
            {
                if (a >= (-b)) {
                    res = Add(a, -b);
                } else {
                    res = Add(-b, a);
                }

                res._Sign = Sign.POSITIVE;
            }

            if ((a._Sign == Sign.NEGATIVE) && (b._Sign == Sign.POSITIVE))
            {
                if ((-a) >= b) {
                    res = Add(-a, b);
                } else {
                    res = Add(b, -a);
                }

                res._Sign = Sign.NEGATIVE;
            }

            return res;
        }

        /// <summary>
        /// Multiplication operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the multiplication</returns>
        public static Base10BigInteger Multiplication(Base10BigInteger a, Base10BigInteger b)
        {
            if ((a == Zero) || (b == Zero)) {
                return Zero;
            }

            Base10BigInteger res = Multiply(Abs(a), Abs(b));
            if (a._Sign == b._Sign) {
                res._Sign = Sign.POSITIVE;
            } else {
                res._Sign = Sign.NEGATIVE;
            }

            return res;
        }


        #endregion


        #region Overloaded Operators


        /// <summary>
        /// Implicit conversion operator from long to Base10BigInteger.
        /// </summary>
        /// <param name="n">The long to be converted to a Base10BigInteger</param>
        /// <returns>The Base10BigInteger converted from the given long</returns>
        public static implicit operator Base10BigInteger(long n)
        {
            return new Base10BigInteger(n);
        }

        /// <summary>
        /// Equality test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a == b, false otherwise</returns>
        public static bool operator ==(Base10BigInteger a, Base10BigInteger b)
        {
            return Equals(a, b);
        }

        /// <summary>
        /// Inequality test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a != b, false otherwise</returns>
        public static bool operator !=(Base10BigInteger a, Base10BigInteger b)
        {
            return !Equals(a, b);
        }

        /// <summary>
        /// Greater test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &gt; b, false otherwise</returns>
        public static bool operator >(Base10BigInteger a, Base10BigInteger b)
        {
            return Greater(a, b);
        }

        /// <summary>
        /// Smaller test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &lt; b, false otherwise</returns>
        public static bool operator <(Base10BigInteger a, Base10BigInteger b)
        {
            return Smaller(a, b);
        }

        /// <summary>
        /// Greater or equal test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &gt;= b, false otherwise</returns>
        public static bool operator >=(Base10BigInteger a, Base10BigInteger b)
        {
            return GreaterOrEqual(a, b);
        }

        /// <summary>
        /// Smaller or equal test between two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>True if a &lt;= b, false otherwise</returns>
        public static bool operator <=(Base10BigInteger a, Base10BigInteger b)
        {
            return SmallerOrEqual(a, b);
        }

        /// <summary>
        /// Base10BigInteger inverse with respect to addition.
        /// </summary>
        /// <param name="n">The Base10BigInteger whose opposite is to be computed</param>
        /// <returns>The Base10BigInteger inverse with respect to addition</returns>
        public static Base10BigInteger operator -(Base10BigInteger n)
        {
            return Opposite(n);
        }

        /// <summary>
        /// Addition operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the addition</returns>
        public static Base10BigInteger operator +(Base10BigInteger a, Base10BigInteger b)
        {
            return Addition(a, b);
        }

        /// <summary>
        /// Subtraction operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the subtraction</returns>
        public static Base10BigInteger operator -(Base10BigInteger a, Base10BigInteger b)
        {
            return Subtraction(a, b);
        }

        /// <summary>
        /// Multiplication operation of two Base10BigIntegers.
        /// </summary>
        /// <param name="a">The 1st Base10BigInteger</param>
        /// <param name="b">The 2nd Base10BigInteger</param>
        /// <returns>The Base10BigInteger result of the multiplication</returns>
        public static Base10BigInteger operator *(Base10BigInteger a, Base10BigInteger b)
        {
            return Multiplication(a, b);
        }

        /// <summary>
        /// Incremetation by one operation of a Base10BigInteger.
        /// </summary>
        /// <param name="n">The Base10BigInteger to be incremented by one</param>
        /// <returns>The Base10BigInteger result of incrementing by one</returns>
        public static Base10BigInteger operator ++(Base10BigInteger n)
        {
            Base10BigInteger res = n + One;
            return res;
        }

        /// <summary>
        /// Decremetation by one operation of a Base10BigInteger.
        /// </summary>
        /// <param name="n">The Base10BigInteger to be decremented by one</param>
        /// <returns>The Base10BigInteger result of decrementing by one</returns>
        public static Base10BigInteger operator --(Base10BigInteger n)
        {
            Base10BigInteger res = n - One;
            return res;
        }


        #endregion


        #region Private Methods


        /// <summary>
        /// Adds two BigNumbers a and b, where a >= b, a, b non-negative.
        /// </summary>
        private static Base10BigInteger Add(Base10BigInteger a, Base10BigInteger b)
        {
            Base10BigInteger res = new Base10BigInteger(a);
            long trans = 0, temp;
            int i;

            for (i = 0; i < b._Size; i++)
            {
                temp = res._Digits[i] + b._Digits[i] + trans;
                res._Digits[i] = temp % NumberBase;
                trans = temp / NumberBase;
            }

            for (i = b._Size; ((i < a._Size) && (trans > 0)); i++)
            {
                temp = res._Digits[i] + trans;
                res._Digits[i] = temp % NumberBase;
                trans = temp / NumberBase;
            }

            if (trans > 0)
            {
                res._Digits[res._Size] = trans % NumberBase;
                res._Size++;
                trans /= NumberBase;
            }

            return res;
        }

        /// <summary>
        /// Subtracts the Base10BigInteger b from the Base10BigInteger a, where a >= b, a, b non-negative.
        /// </summary>
        private static Base10BigInteger Subtract(Base10BigInteger a, Base10BigInteger b)
        {
            Base10BigInteger res = new Base10BigInteger(a);
            int i;
            long temp, trans = 0;
            bool reducible = true;

            for (i = 0; i < b._Size; i++)
            {
                temp = res._Digits[i] - b._Digits[i] - trans;
                if (temp < 0)
                {
                    trans = 1;
                    temp += NumberBase;
                }
                else {
                    trans = 0;
                }
                res._Digits[i] = temp;
            }

            for (i = b._Size; ((i < a._Size) && (trans > 0)); i++)
            {
                temp = res._Digits[i] - trans;
                if (temp < 0)
                {
                    trans = 1;
                    temp += NumberBase;
                }
                else {
                    trans = 0;
                }
                res._Digits[i] = temp;
            }

            while ((res._Size - 1 > 0) && (reducible == true))
            {
                if (res._Digits[res._Size - 1] == 0) {
                    res._Size--;
                } else {
                    reducible = false;
                }
            }

            return res;
        }

        /// <summary>
        /// Multiplies two Base10BigIntegers.
        /// </summary>
        private static Base10BigInteger Multiply(Base10BigInteger a, Base10BigInteger b)
        {
            int i, j;
            long temp, trans = 0;

            Base10BigInteger res = new Base10BigInteger();
            res._Size = a._Size + b._Size - 1;
            for (i = 0; i < res._Size + 1; i++)
            {
                res._Digits[i] = 0;
            }

            for (i = 0; i < a._Size; i++)
            {
                if (a._Digits[i] != 0) {
                    for (j = 0; j < b._Size; j++)
                    {
                        if (b._Digits[j] != 0) {
                            res._Digits[i + j] += a._Digits[i] * b._Digits[j];
                        }
                    }
                }
            }

            for (i = 0; i < res._Size; i++)
            {
                temp = res._Digits[i] + trans;
                res._Digits[i] = temp % NumberBase;
                trans = temp / NumberBase;
            }

            if (trans > 0)
            {
                res._Digits[res._Size] = trans % NumberBase;
                res._Size++;
                trans /= NumberBase;
            }

            return res;
        }


        #endregion



        private class DigitContainer
        {
            private readonly long[][] _Digits;
            private const int ChunkSize = 32;
            private const int ChunkSizeDivisionShift = 5;
            private const int ChunkCount = Base10BigInteger.MaxSize >> ChunkSizeDivisionShift;

            public DigitContainer()
            {
                _Digits = new long[ChunkCount][];
            }

            public long this[int index]
            {
                get
                {
                    var chunkIndex = index >> ChunkSizeDivisionShift;
                    var chunk = _Digits[chunkIndex];
                    return chunk?[index % ChunkSize] ?? 0;
                }
                set
                {
                    var chunkIndex = index >> ChunkSizeDivisionShift;
                    var chunk = _Digits[chunkIndex] ?? (_Digits[chunkIndex] = new long[ChunkSize]);
                    chunk[index % ChunkSize] = value;
                }
            }
        }
    }
}
