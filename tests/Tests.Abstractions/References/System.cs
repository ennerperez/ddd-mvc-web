// ReSharper disable once CheckNamespace

namespace System
{
    public static class Extensions
    {
        
        /// <summary>
        /// Returns an Int32 with a random value across the entire range of
        /// possible values.
        /// </summary>
        public static int NextInt32(this Random rng)
        {
            int firstBits = rng.Next(0, 1 << 4) << 28;
            int lastBits = rng.Next(0, 1 << 28);
            return firstBits | lastBits;
        }

        #region NextLong

        public static long NextLong(this Random random)
        {
            return random.NextInt64();
        }

        public static long NextLong(this Random random, long maxValue)
        {
            return random.NextInt64(0, maxValue);
        }

        public static long NextLong(this Random random, long minValue, long maxValue)
        {
            return random.NextInt64(minValue, maxValue);
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
    }
}
