using System;


namespace EW.Framework
{
    public static class Exts
    {

        public static bool IsPowerOf2(int v)
        {
            return (v & (v - 1)) == 0;
        }

        public enum ISqrtRoundMode { Floor, Nearest, Ceiling }
        public static int ISqrt(int number, ISqrtRoundMode round = ISqrtRoundMode.Floor)
        {
            if (number < 0)
                throw new InvalidOperationException("Attempted to calculate the square root of a negative integer: {0}");

            return (int)ISqrt((uint)number, round);
        }

        public static uint ISqrt(uint number, ISqrtRoundMode round = ISqrtRoundMode.Floor)
        {
            var divisor = 1U << 30;

            var root = 0U;
            var remainder = number;

            // Find the highest term in the divisor
            while (divisor > number)
                divisor >>= 2;

            // Evaluate the root, two bits at a time
            while (divisor != 0)
            {
                if (root + divisor <= remainder)
                {
                    remainder -= root + divisor;
                    root += 2 * divisor;
                }

                root >>= 1;
                divisor >>= 2;
            }

            // Adjust for other rounding modes
            if (round == ISqrtRoundMode.Nearest && remainder > root)
                root += 1;
            else if (round == ISqrtRoundMode.Ceiling && root * root < number)
                root += 1;

            return root;
        }

    }
}