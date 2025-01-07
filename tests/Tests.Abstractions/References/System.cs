// ReSharper disable once CheckNamespace

using System.Threading;

namespace System
{
    public static class Extensions
    {
        private static int seedCounter = new Random().Next();

        [ThreadStatic]
        private static Random rng;

        internal static Random Instance
        {
            get
            {
                if (rng == null)
                {
                    var seed = Interlocked.Increment(ref seedCounter);
                    rng = new Random(seed);
                }

                return rng;
            }
        }

        public static int Random() => Instance.Next();

        public static int Random(int maxValue) => Instance.Next(maxValue);

        public static int Random(int minValue, int maxValue) => Instance.Next(minValue, maxValue);

        /// <summary>
        /// Returns an Int32 with a random value across the entire range of possible values.
        /// </summary>
        public static int NextInt32(this Random @this)
        {
            var firstBits = @this.Next(0, 1 << 4) << 28;
            var lastBits = @this.Next(0, 1 << 28);
            return firstBits | lastBits;
        }

        #region NextShort

        public static short NextShort(this Random @this, short minValue, short maxValue)
        {
            var value = @this.Next(minValue, maxValue);
            if (value < short.MinValue)
            {
                value = short.MinValue;
            }
            else if (value > short.MaxValue)
            {
                value = short.MaxValue;
            }

            return (short)value;
        }

        #endregion

        public static T AsEnum<T>(this string @this, bool ignoreCase = true)
        {
            if (!string.IsNullOrWhiteSpace(@this))
            {
                return (T)Enum.Parse(typeof(T), @this, ignoreCase);
            }

            return default;
        }

        public static object AsEnum(this string @this, Type type, bool ignoreCase = true)
        {
            if (!string.IsNullOrWhiteSpace(@this))
            {
                return Enum.Parse(type, @this, ignoreCase);
            }

            return default;
        }

        public static bool Is(this string @this, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (!string.IsNullOrWhiteSpace(@this))
            {
                return @this.Equals(value, comparison);
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return @this == value;
        }

        #region NextLong

        public static long NextLong(this Random @this) => @this.NextInt64();

        public static long NextLong(this Random @this, long maxValue) => @this.NextInt64(0, maxValue);

        public static long NextLong(this Random @this, long minValue, long maxValue) => @this.NextInt64(minValue, maxValue);

        #endregion

        #region NextDecimal

        public static decimal NextDecimal(this Random random) => NextDecimal(random, decimal.MaxValue);

        public static decimal NextDecimal(this Random random, decimal maxValue) => NextDecimal(random, decimal.Zero, maxValue);

        public static decimal NextDecimal(this Random random, decimal minValue, decimal maxValue)
        {
            var nextDecimalSample = 1m;
            while (nextDecimalSample >= 1)
            {
                var a = random.NextInt32();
                var b = random.NextInt32();
                var c = random.Next(542101087);
                nextDecimalSample = new decimal(a, b, c, false, 28);
            }

            return (maxValue * nextDecimalSample) + (minValue * (1 - nextDecimalSample));
        }

        #endregion
    }
}
