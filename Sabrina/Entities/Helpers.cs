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

    using Tables = TableObjects.Tables;

    /// <summary>
    /// An helper class
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// The check for banned.
        /// </summary>
        /// <param name="ctx">
        /// The ctx.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task<bool> CheckForBanned(CommandContext ctx)
        {
            var user = Tables.Discord.User.Load(ctx.User);
            if (user.BanTime != null && user.BanTime > DateTime.Now)
            {
                await ctx.Message.DeleteAsync("User is banned");
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendMessageAsync($"You seem to be banned for the following reason: {user.BanReason}");
                await dm.SendMessageAsync($"That means, you will not be able to send Messages, until {user.BanTime.Value.ToLongDateString()}");
                return true;
            }

            return false;
        }

        public static async Task<bool> CheckForMuted(CommandContext ctx)
        {
            var user = Tables.Discord.User.Load(ctx.Message.Author);
            if (user.MuteTime != null && user.MuteTime > DateTime.Now)
            {
                await ctx.Message.DeleteAsync("User is muted");
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendMessageAsync($"You seem to be muted for the following reason: {user.MuteReason}");
                await dm.SendMessageAsync($"That means, you will not be able to send Messages, until {user.MuteTime.Value.ToLongDateString()} {user.MuteTime.Value.ToLongTimeString()}");
                return true;
            }

            return false;
        }

        public static async Task<bool> CheckForMuted(MessageCreateEventArgs message, DiscordClient client)
        {
            var internalUser = Tables.Discord.User.Load(message.Author);
            if (internalUser.MuteTime != null && internalUser.MuteTime > DateTime.Now)
            {
                await message.Message.DeleteAsync("User is muted");
                await message.Message.Channel.Users.First(user => user.Id == message.Author.Id).SendMessageAsync($"You seem to be muted for the following reason: {internalUser.MuteTime}");
                await message.Message.Channel.Users.First(user => user.Id == message.Author.Id).SendMessageAsync($"That means, you will not be able to send Messages, until {internalUser.MuteTime.Value.ToLongDateString()} {internalUser.MuteTime.Value.ToLongTimeString()}");
                return true;
            }

            return false;
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
    }
}