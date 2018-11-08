// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpers.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Helpers type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



namespace Sabrina.Entities
{
    using System;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.EventArgs;

    using Sabrina.Entities.Persistent;



    /// <summary>
    /// An helper class
    /// </summary>
    public static class Helpers
    {
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
    }
}