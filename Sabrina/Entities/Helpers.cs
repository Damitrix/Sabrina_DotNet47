// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Helpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Net;

namespace Sabrina.Entities
{
    using System;
    using System.Net.Cache;
    using System.Security.Cryptography;

    /// <summary>
    /// An helper class
    /// </summary>
    public static class Helpers
    {
        public static class RegexHelper
        {
            public const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
            public const string NoRegex = "[Nn][Oo]?";
            public const string YesRegex = "[Yy][Ee]?[Ss]?";
        }

        public static class RandomGenerator
        {
            private static readonly RNGCryptoServiceProvider Rand = new RNGCryptoServiceProvider();

            public static int RandomInt(int min, int max)
            {
                uint scale = uint.MaxValue;
                while (scale == uint.MaxValue)
                {
                    // Get four random bytes.
                    byte[] four_bytes = new byte[4];
                    Rand.GetBytes(four_bytes);

                    // Convert that into an uint.
                    scale = BitConverter.ToUInt32(four_bytes, 0);
                }

                // Add min to the scaled difference between max and min.
                return (int)(min + (max - min) *
                             (scale / (double)uint.MaxValue));
            }
        }

        public static HttpWebRequest CreateRequestWithHeaders(string url)
        {
            HttpWebRequest request =
                HttpWebRequest.CreateHttp(url);
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
            request.Accept =
                "text/html,*/*";
            request.Method = "GET";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36";

            return request;
        }
    }
}