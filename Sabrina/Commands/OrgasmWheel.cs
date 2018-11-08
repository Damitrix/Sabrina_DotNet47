// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrgasmWheel.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the OrgasmWheel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Sabrina.Entities;
    using Sabrina.Entities.Persistent;
    using Sabrina.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;


    /// <summary>
    /// The orgasm wheel Command Group.
    /// </summary>
    internal class OrgasmWheel
    {
        public DiscordContext _context;

        public OrgasmWheel(DiscordContext context)
        {
            _context = new DiscordContext();
        }

        /// <summary>
        /// The wheel outcomes.
        /// </summary>
        private List<WheelOutcome> wheelOutcomes;

        /// <summary>
        /// The spin wheel Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        [Command("oldorgasmwheel")]
        [Description("Spins the old wheel. Just in case the new one is broken.")]
        [Aliases("orgasmwheel1", "orgasmwheel2")]
        public async Task SpinWheelAsync(CommandContext ctx)
        {
            if (ctx.Channel.Name != Configuration.Config.Channels.Wheel)
            {
                return;
            }

            int outcome = Helpers.RandomGenerator.RandomInt(0, 100);

            var line = "Sorry, no Entries yet.";
            if (outcome < 92)
            {
                line = await this.LoadLineAsync($"{Configuration.Config.BotFileFolders.WheelResponses}/Denial.txt");
            }
            else if (outcome < 96)
            {
                line = await this.LoadLineAsync($"{Configuration.Config.BotFileFolders.WheelResponses}/Ruin.txt");
            }
            else
            {
                line = await this.LoadLineAsync($"{Configuration.Config.BotFileFolders.WheelResponses}/Orgasm.txt");
            }

            await ctx.RespondAsync(line);
        }

        /// <summary>
        /// The add link async.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <param name="creator">
        /// The creator.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="url">
        /// The url.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("addlink")]
        [Description("Add a Link to the OrgasmWheel")]
        [RequireRolesAttribute("mistress", "techno kitty", "content creator", "trusted creator")]
        public async Task AddLinkAsync(
            CommandContext ctx,
            [Description("The Person who created the Content")]
            string creator,
            [Description("Picture or Video")] string type,
            [Description("The Link")] string url)
        {
            if (Enum.TryParse(type, out Link.ContentType linkType))
            {
                var link = new Link
                {
                    CreatorName = creator,
                    Type = linkType,
                    Url = url
                };
                link.Save();
                await ctx.RespondAsync("Link added!");
            }
            else
            {
                await ctx.RespondAsync(
                    $"Cannot Parse \"{type}\". Please be sure to use either \"Video\", or \"Picture\".");
            }
        }

        /// <summary>
        /// The show links Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("showlinks")]
        [Description("Shows all Links")]
        [RequireOwner]
        public async Task ShowLinksAsync(CommandContext ctx)
        {
            List<Link> links = await Link.LoadAll();

            var text = "Here are all Links:\n```";

            foreach (var link in links)
            {
                if ((text + link.Url).Length > 1999)
                {
                    text += "```";
                    await ctx.RespondAsync(text);
                    text = "Here are more Links:\n```";
                }

                text += link.Url + "\n";
            }

            text += "```";

            await ctx.RespondAsync(text);
        }

        /// <summary>
        /// The purge links command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("purgelinks")]
        [Description("Purges Links")]
        [RequireOwner]
        public async Task PurgeLinksAsync(CommandContext ctx)
        {
            List<Link> links = await Link.LoadAll();

            var linksToDelete = new List<Link>();

            foreach (var origLink in links)
            {
                foreach (var currentLink in links)
                {
                    if (currentLink.FileName != origLink.FileName && currentLink.Url == origLink.Url)
                    {
                        linksToDelete.Add(currentLink);
                    }
                }
            }

            string outString = string.Empty;

            foreach (var link in linksToDelete)
            {
                outString += link.FileName + "\n";
                link.Delete();
            }

            await ctx.RespondAsync($"I've deleted the duplicates\n{outString}");
        }

        /// <summary>
        /// The orgasm wheel Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("orgasmwheel")]
        [Description("Spins a wheel")]
        public async Task SpinNewWheelAsync(CommandContext ctx)
        {
            var user = await _context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));
            var settings = _context.UserSettings.Find(Convert.ToInt64(ctx.Message.Author.Id));

            if (settings == null)
            {
                settings = new Models.UserSettings();
            }

            var chances = await _context.WheelChances.FindAsync(settings.WheelDifficulty ?? 2);

            if (user.LockTime != null && user.LockTime > DateTime.Now)
            {
                var timeUntilFree = user.LockTime - DateTime.Now;

                var newTimeUntilFree = TimeSpan.FromTicks(timeUntilFree.Value.Ticks * Helpers.RandomGenerator.RandomInt(1, 4));

                if (newTimeUntilFree > TimeSpan.FromDays(365))
                {
                    await ctx.RespondAsync("Fuck off");
                    return;
                }

                if (Helpers.RandomGenerator.RandomInt(0, 4) > 0)
                {
                    await ctx.RespondAsync(
                        $"Oho, it seems like I told you to stay away from spinning the wheel for {TimeResolver.TimeToString(timeUntilFree.Value)}.\n" +
                        $"That means you get another {TimeResolver.TimeToString(newTimeUntilFree)} of no spinning {DiscordEmoji.FromName(ctx.Client, Configuration.Config.Emojis.Blush)}");

                    user.LockTime += newTimeUntilFree;
                    await _context.SaveChangesAsync();
                    return;
                }

                await ctx.RespondAsync(
                    $"I believe i have told you not to spin the wheel for {TimeResolver.TimeToString(timeUntilFree.Value)}...\n" +
                    $"But since i'm awesome, you won't get punishment for it this time. {DiscordEmoji.FromName(ctx.Client, Configuration.Config.Emojis.Blush)}");
            }

            var addedChance = 0;

            int outcomeChanceValue = Helpers.RandomGenerator.RandomInt(
                0,
                chances.Denial + chances.Task + chances.Ruin + chances.Orgasm);
            Console.WriteLine("Chance Rolled: " + outcomeChanceValue);
            Console.WriteLine("Denial Chance: " + chances.Denial);
            Console.WriteLine("TaskChance Chance: " + chances.Task);
            Console.WriteLine("RuinChance Chance: " + chances.Ruin);
            Console.WriteLine("OrgasmChance Chance: " + chances.Orgasm);
            Console.WriteLine("addedChance Chance: " + addedChance);
            var outcome = SlaveReportsExtension.Outcome.task;
            if (outcomeChanceValue < chances.Denial)
            {
                outcome = SlaveReportsExtension.Outcome.denial;
            }
            else if (outcomeChanceValue < chances.Denial + chances.Task)
            {
                outcome = SlaveReportsExtension.Outcome.task;
            }
            else if (outcomeChanceValue < chances.Denial + chances.Task + chances.Ruin)
            {
                outcome = SlaveReportsExtension.Outcome.ruin;
            }
            else
            {
                outcome = SlaveReportsExtension.Outcome.orgasm;
            }

            if (user.DenialTime != null && user.DenialTime > DateTime.Now)
            {
                var timeUntilFree = user.DenialTime - DateTime.Now;
                if (outcome.HasFlag(SlaveReportsExtension.Outcome.orgasm)
                    || outcome.HasFlag(SlaveReportsExtension.Outcome.ruin))
                {
                    await ctx.RespondAsync(
                        $"Haha, I would've let you cum this time, but since you're still denied for {TimeResolver.TimeToString(timeUntilFree.Value)}, "
                        + $"that won't happen {DiscordEmoji.FromName(ctx.Client, Configuration.Config.Emojis.Blush)}.\n" +
                        $"As a punishment, you're gonna do your Task anyways though. Any additional denial time will be added to your existing time.");
                }
                else
                {
                    await ctx.RespondAsync(
                        $"Well, you're still denied for {TimeResolver.TimeToString(timeUntilFree.Value)}.\n"
                        + $"You still want to do something? Then here you go."
                        + $"\nAnd as a bonus, if you get more denial time, it will get added on top of your existing time! {DiscordEmoji.FromName(ctx.Client, Configuration.Config.Emojis.Blush)}");

                }
                await Task.Delay(1500);
                outcome = SlaveReportsExtension.Outcome.denial | SlaveReportsExtension.Outcome.task;
            }

            WheelOutcome wheelOutcome = null;

            while (wheelOutcome == null)
            {
                this.wheelOutcomes = ReflectiveEnumerator.GetEnumerableOfType<WheelOutcome>(outcome, settings, _context).ToList();

                this.wheelOutcomes = this.wheelOutcomes.Where(e => outcome.HasFlag(e.Outcome)).ToList();

                if (this.wheelOutcomes.Count < 1)
                {
                    continue;
                }

                // Choose an outcome by summing up the chance values of all possible outcomes and then generating a random number inside those.
                var combinedChance = 0;

                foreach (var currentOutcome in this.wheelOutcomes)
                {
                    combinedChance += currentOutcome.Chance;
                }

                var chance = 0;
                int minChance = Helpers.RandomGenerator.RandomInt(0, combinedChance);

                foreach (var currentOutcome in this.wheelOutcomes)
                {
                    chance += currentOutcome.Chance;
                    if (minChance < chance)
                    {
                        wheelOutcome = currentOutcome;
                        break;
                    }
                }
            }

            if (user.DenialTime == null)
            {
                user.DenialTime = DateTime.Now;
            }

            if (user.LockTime == null)
            { 
                user.LockTime = DateTime.Now;
            }

            if (user.DenialTime < DateTime.Now)
            {
                user.DenialTime = DateTime.Now;
            }

            if (user.LockTime < DateTime.Now)
            {
                user.LockTime = DateTime.Now;
            }

            user.DenialTime += wheelOutcome.DenialTime;
            user.LockTime += wheelOutcome.WheelLockedTime;
            await _context.SaveChangesAsync();

            if (wheelOutcome.Embed != null)
            {
                await ctx.RespondAsync(embed: wheelOutcome.Embed);
            }
            else
            {
                await ctx.RespondAsync(wheelOutcome.Text);
            }
        }

        /// <summary>
        /// The denial time Command.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Command("denialtime")]
        [Description("Shows how much longer you should not come")]
        [Aliases("denieduntil")]
        public async Task DenialTimeAsync(CommandContext ctx)
        {
            var user = await _context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));

            var denialString = "You have no denial time left.";
            var wheelLockedString = "You can spin the wheel at any time.";

            if (user.DenialTime != null && user.DenialTime > DateTime.Now)
            {
                denialString =
                    $"You still have {TimeResolver.TimeToString(user.DenialTime.Value - DateTime.Now)} of denial left.";

            }

            if (user.LockTime != null && user.LockTime > DateTime.Now)
            {
                wheelLockedString =
                    $"You can spin the wheel again in {TimeResolver.TimeToString(user.LockTime.Value - DateTime.Now)}.";

            }

            await ctx.RespondAsync($"Hey {(await ctx.Client.GetUserAsync(Convert.ToUInt64(user.UserId))).Mention},\n" +
                                   $"{denialString}\n" +
                                   $"{wheelLockedString}");
        }

        /// <summary>
        /// The remove profile async.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <param name="dcUser">
        /// The Discord user.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        [Command("resetuser")]
        [Description("Reset a Users saved Data")]
        [Aliases("ru")]
        [RequireRolesAttribute("mistress", "minion", "techno kitty")]
        public async Task RemoveProfileAsync(CommandContext ctx, [Description("Mention the user here")] DiscordUser dcUser)
        {
            var user = await _context.Users.FindAsync(Convert.ToInt64(ctx.Message.Author.Id));

            user.DenialTime = null;
            user.BanTime = null;
            user.LockTime = null;
            user.SpecialTime = null;
            user.RuinTime = null;

            await _context.SaveChangesAsync();

            await ctx.RespondAsync($"I've reset the Profile of {dcUser.Mention}.");
        }

        /// <summary>
        /// Loads a random Line from a TextDocument
        /// </summary>
        /// <param name="file">
        /// The file path.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        private async Task<string> LoadLineAsync(string file)
        {
            using (var reader = File.OpenText(file))
            {
                string fileText = await reader.ReadToEndAsync();
                string[] lines = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                return lines[Helpers.RandomGenerator.RandomInt(0, lines.Length)];
            }
        }
    }
}