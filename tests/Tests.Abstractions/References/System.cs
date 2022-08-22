// ReSharper disable once CheckNamespace

using System.Threading;

namespace System
{
    public static class Extensions
    {
        private static int seedCounter = new Random().Next();

        [ThreadStatic] private static Random rng;

        internal static Random Instance
        {
            get
            {
                if (rng == null)
                {
                    int seed = Interlocked.Increment(ref seedCounter);
                    rng = new Random(seed);
                }

                return rng;
            }
        }

        public static int Random()
        {
            return Instance.Next();
        }

        public static int Random(int maxValue)
        {
            return Instance.Next(maxValue);
        }

        public static int Random(int minValue, int maxValue)
        {
            return Instance.Next(minValue, maxValue);
        }


        /// <summary>
        /// Returns an Int32 with a random value across the entire range of
        /// possible values.
        /// </summary>
        public static int NextInt32(this Random @this)
        {
            int firstBits = @this.Next(0, 1 << 4) << 28;
            int lastBits = @this.Next(0, 1 << 28);
            return firstBits | lastBits;
        }

        #region NextLong

        public static long NextLong(this Random @this)
        {
            return @this.NextInt64();
        }

        public static long NextLong(this Random @this, long maxValue)
        {
            return @this.NextInt64(0, maxValue);
        }

        public static long NextLong(this Random @this, long minValue, long maxValue)
        {
            return @this.NextInt64(minValue, maxValue);
        }

        #endregion

        #region NextDecimal

        public static decimal NextDecimal(this Random random)
        {
            return NextDecimal(random, decimal.MaxValue);
        }

        public static decimal NextDecimal(this Random random, decimal maxValue)
        {
            return NextDecimal(random, decimal.Zero, maxValue);
        }

        public static decimal NextDecimal(this Random random, decimal minValue, decimal maxValue)
        {
            var nextDecimalSample = 1m;
            while (nextDecimalSample >= 1)
            {
                var a = random.NextInt32();
                var b = random.NextInt32();
                var c = random.Next(542101087);
                nextDecimalSample = new Decimal(a, b, c, false, 28);
            }

            return maxValue * nextDecimalSample + minValue * (1 - nextDecimalSample);
        }

        #endregion


        public static T AsEnum<T>(this string @this, bool ignoreCase = true)
        {
            if (!string.IsNullOrWhiteSpace(@this))
                return (T)Enum.Parse(typeof(T), @this, ignoreCase: ignoreCase);
            return default(T);
        }

        public static object AsEnum(this string @this, Type type, bool ignoreCase = true)
        {
            if (!string.IsNullOrWhiteSpace(@this))
                return Enum.Parse(type, @this, ignoreCase: ignoreCase);
            return default;
        }

        public static bool Is(this string @this, string value, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (!string.IsNullOrWhiteSpace(@this))
                return @this.Equals(value, comparison);
            else if (!string.IsNullOrWhiteSpace(value)) 
                return false;
            else
                return @this == value;
        }
    }
}
