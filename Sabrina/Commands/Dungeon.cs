using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Sabrina.Entities;

namespace Sabrina.Commands
{
    [Group("dungeon")]
    internal class Dungeon
    {

        [Command("setdifficulty")]
        public async Task SetDifficulty(CommandContext ctx, string difficulty)
        {

            // Sets the difficulty when in a save zone
        }

        [Group("action")]
        private class Action
        {
            [Command("continue")]
            public async Task ContinueRandom(CommandContext ctx)
            {
                // This will Start a Session with of random Length - for some extra xp
            }

            [Command("continueadvanced")]
            public async Task ContinueLength(CommandContext ctx, string length)
            {

                // This will start a Session with a set Length
            }
        }
    }
}