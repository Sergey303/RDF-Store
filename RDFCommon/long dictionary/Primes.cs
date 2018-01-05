using System;

namespace RDFCommon.long_dictionary
{
    public static class Primes
    {
        public static ulong GetNextPrime(ulong start)
        {
            if (start%2 == 0) start++;
            while (true)
            {
                if (IsPrimeByFerma(start)) return start;
                start += 2;
            }
        }

        public static bool IsPrimeByFerma(ulong number)
        {
            if (number == 2)
                return true;
            Random r=new Random(DateTime.Now.Millisecond);
            byte[] randomLong=new byte[8];
            for (int i = 0; i < 1000; i++)
            {
                r.NextBytes(randomLong);
                ulong a = (BitConverter.ToUInt64(randomLong,0) % (number - 2)) + 2;
                if (GetGCD(a, number) != 1)
                    return false;
                if (GetPower(a, number - 1, number) != 1)
                    return false;
            }
            return true;
        }

        private static ulong  GetGCD(ulong a, ulong b)
        {
            if (b == 0)
                return a;
            return GetGCD(b, a % b);
        }

        private static ulong GetMultiply(ulong a, ulong b, ulong m)
        {
            if (b == 1)
                return a;
            if (b % 2 == 0)
            {
                ulong t = GetMultiply(a, b / 2, m);
                return (2 * t) % m;
            }
            return (GetMultiply(a, b - 1, m) + a) % m;
        }

        private static ulong GetPower(ulong a, ulong b, ulong m)
        {
            if (b == 0)
                return 1;
            if (b % 2 == 0)
            {
                ulong t = GetPower(a, b / 2, m);
                return GetMultiply(t, t, m) % m;
            }
            return (GetMultiply(GetPower(a, b - 1, m), a, m)) % m;
        }
    }
}