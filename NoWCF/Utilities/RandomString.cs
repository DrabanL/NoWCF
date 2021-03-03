using System;
using System.Collections.Generic;
using System.Linq;

namespace NoWCF.Utilities
{
    public static class RandomString
    {
        private static readonly Random _random = new Random();
        private static readonly char[] _charactersBank = "abcdedfhijkelmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static IEnumerable<char> _gen(int len)
        {
            for (int i = 0; i < len; ++i)
                yield return _charactersBank[_random.Next(0, _charactersBank.Length - 1)];
        }

        public static string Next(int len) => new string(_gen(len).ToArray());
    }
}