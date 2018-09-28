using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Sabrina.Entities;

namespace Sabrina.Commands
{
    [Group("dungeon")]
    internal class Dungeon : BaseCommandModule
    {
        private readonly Dependencies dep;

        public Dungeon(Dependencies d)
        {
            this.dep = d;
        }

        [Command("setdifficulty")]
        public async Task SetDifficulty(CommandContext ctx, string difficulty)
        {
            if (await Helpers.CheckForMuted(ctx))
            {
                return;
            }

            // Sets the difficulty when in a save zone
        }

        [Group("action")]
        private class Action
        {
            [Command("continue")]
            public async Task ContinueRandom(CommandContext ctx)
            {
                if (await Helpers.CheckForMuted(ctx))
                {
                    return;
                }

                // This will Start a Session with of random Length - for some extra xp
            }

            [Command("continue")]
            public async Task ContinueLength(CommandContext ctx, string length)
            {
                if (await Helpers.CheckForMuted(ctx))
                {
                    return;
                }

                // This will start a Session with a set Length
            }
        }
    }
}