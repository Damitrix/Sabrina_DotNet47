using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Sabrina.Entities.Persistent;
using Tables = TableObjects.Tables;

namespace Sabrina.Commands
{
    internal class Edges : BaseCommandModule
    {
        [Command("assignedges")]
        [Description("Assign Edges to yourself")]
        public async Task AssignEdgesAsync(CommandContext ctx, int edges)
        {
            if (edges < 1)
            {
                await ctx.RespondAsync("You cannot assign yourself less than 1 Edge. For obvious reasons");
            }

            var user = Tables.Discord.User.Load(ctx.Message.Author);

            user.WalletEdges += edges;
            user.Save();

            await ctx.RespondAsync(
                $"I've assigned you {edges} edges. Your balance is now {user.WalletEdges} edges.");
        }

        [Command("assignedgesto")]
        [Description("Assign Edges to someone")]
        [RequireRolesAttribute(RoleCheckMode.Any ,"mistress", "aki's cutie")]
        public async Task AssignEdgesToAsync(CommandContext ctx, DiscordUser dcUser, int edges)
        {
            var user = Tables.Discord.User.Load(ctx.Message.Author);

            user.WalletEdges += edges;
            user.Save();

            await ctx.RespondAsync(
                $"I've assigned {dcUser.Username} {edges} edges. Their balance is now {user.WalletEdges} edges.");
        }

        [Command("edge")]
        [Aliases("e")]
        [Description("Remove an Edge from your wallet")]
        [Cooldown(1, 20, CooldownBucketType.User)]
        public async Task HasEdgedAsync(CommandContext ctx)
        {
            var user = Tables.Discord.User.Load(ctx.Message.Author);

            user.WalletEdges -= 1;
            user.TotalEdges += 1;
            user.Save();

            await ctx.RespondAsync(
                $"You've told me you've edged 1 time. Your balance is now {user.WalletEdges} edges.");
        }

        [Command("edges")]
        [Description("Show how much Edges you have left")]
        public async Task DisplayEdges(CommandContext ctx)
        {
            var user = Tables.Discord.User.Load(ctx.Message.Author);

            await ctx.RespondAsync(
                $"Your Edge Balance is {user.WalletEdges} edges.");
        }
    }
}