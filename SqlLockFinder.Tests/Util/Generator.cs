using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlLockFinder.Tests.Util
{
    public static class Generator
    {
        public static int GetRandomNumber(int maxNumber, int minNumber = 0)
        {
            if (maxNumber < 1)
                throw new System.Exception("The maxNumber value should be greater than 1");
            var b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            var r = new System.Random(seed);
            return r.Next(minNumber == 0 ? 1 : minNumber, maxNumber);
        }

        public static double GetRandomDouble()
        {
            var b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            var seed = (b[0] & 0x7f) << 24 | b[1] << 16 | b[2] << 8 | b[3];
            var r = new System.Random(seed);
            return r.NextDouble();
        }

        public static decimal GetRandomDecimal(int decimals = 6)
        {
            return (decimal)Math.Round(GetRandomDouble(), decimals);
        }

        public static bool GetRandomBoolean()
        {
            var random = new Random().NextDouble();
            return random >= 0.5;
        }

        public static string GetRandomString(int length)
        {
            var array = new string[54]
            {
                "0", "2", "3", "4", "5", "6", "8", "9",
                "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
                "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
            };
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < length; i++) sb.Append(array[GetRandomNumber(53)]);
            return sb.ToString();
        }

        public static DateTime GetRandomDateTime()
        {
            var datetime = DateTime.Now.AddDays(GetRandomNumber(100));
            return datetime;
        }

        public static byte[] GetRandomByteArray(int length)
        {
            var result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)(GetRandomNumber(2) - 1);
            }
            return result;
        }

        public static T GetRandomEnum<T>()
        {
            var enumNames = Enum.GetNames(typeof(T));
            return GetRandomEnum<T>(enumNames);
        }

        public static T GetRandomEnumWithExcludedValues<T>(params T[] excludedValues)
        {
            var enumNames = Enum.GetNames(typeof(T))
                .Where(current => excludedValues.All(excluded => excluded.ToString() != current))
                .ToList();
            return GetRandomEnum<T>(enumNames);
        }

        private static T GetRandomEnum<T>(IEnumerable<string> enumNames)
        {
            var randomValue = GetRandomValue(enumNames.ToArray());
            return (T)Enum.Parse(typeof(T), randomValue);
        }

        public static T GetRandomValue<T>(params T[] values)
        {
            var valueList = values.ToList();
            return valueList.ElementAt(GetRandomNumber(valueList.Count));
        }

        public static char GetRandomChar()
        {
            var chars = Enumerable.Range(0, char.MaxValue + 1)
                .Select(i => (char)i)
                .Where(c => char.IsSymbol(c))
                .ToArray();
            var totalChars = chars.Length;
            var random = GetRandomNumber(totalChars);

            return chars[random];
        }

        public static int[] GetRandomIntArray(int length)
        {
            var result = new int[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = GetRandomNumber(10000) - 1;
            }
            return result;
        }
    }
}