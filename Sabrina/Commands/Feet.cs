// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Feet.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Feet type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using System.Threading.Tasks;

    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    using Sabrina.Bots;
    using Sabrina.Entities;
    using Sabrina.Models;

    /// <summary>
    /// The feet Command Group.
    /// </summary>
    internal class Feet
    {
        private DiscordContext _context;

        public Feet(DiscordContext context)
        {
            _context = new DiscordContext();
        }

        /// <summary>
        /// The boost feet pics Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        [Command("boost"), Description("Posts some more Feet. Can be used 2 times every 10 minutes"), Cooldown(2, 1, CooldownBucketType.Guild)]
        public async Task BoostFeetPics(CommandContext ctx)
        {
            var picsToPost = Helpers.RandomGenerator.RandomInt(2, 5);
            for (int i = 0; i < picsToPost; i++)
            {
                await TumblrBot.PostRandom(ctx.Client, _context);
            }
        }
    }
}
