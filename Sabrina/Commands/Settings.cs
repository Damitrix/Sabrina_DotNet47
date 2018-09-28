// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Settings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Commands
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Sabrina.Entities;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tables = TableObjects.Tables;

    /// <summary>
    /// The settings.
    /// </summary>
    [Group("settings")]
    [Aliases("setting")]
    internal class Settings : BaseCommandModule
    {
        /// <summary>
        /// The confirm regex.
        /// </summary>
        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";

        /// <summary>
        /// The yes regex.
        /// </summary>
        private const string YesRegex = "[Yy][Ee]?[Ss]?";

        /// <summary>
        /// The no regex.
        /// </summary>
        private const string NoRegex = "[Nn][Oo]?";

        /// <summary>
        /// The dependencies.
        /// </summary>
        private readonly Dependencies dep;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="d">
        /// The dependencies.
        /// </param>
        public Settings(Dependencies d)
        {
            this.dep = d;
        }

        /// <summary>
        /// Setup Settings and similar for the current User.
        /// </summary>
        /// <param name="ctx">
        /// The Command Context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        [Command("setup")]
        [Aliases("configure")]
        [Description("Configure your Settings")]
        public async Task SetupAsync(CommandContext ctx)
        {
            var dm = await ctx.Member.CreateDmChannelAsync();

            await dm.SendMessageAsync($"Hey there {ctx.Message.Author.Username}.");
            await Task.Delay(6000);
            await dm.TriggerTypingAsync();
            await Task.Delay(3000);
            await dm.SendMessageAsync($"I see you want to change your Settings, so here is how this is gonna go.{Environment.NewLine}It's really simple, so don't worry.");
            await Task.Delay(1000);
            await dm.TriggerTypingAsync();
            await Task.Delay(3000);

            bool userAgrees = false;

            while (!userAgrees)
            {
                await dm.SendMessageAsync($"I'm gonna send you a bunch of Questions.{Environment.NewLine}" +
                $"After each Question, you will have **4 Minutes** to write your answer.{Environment.NewLine}" +
                $"Don't worry, this is not a Quiz. If you can't answer, the Setup will be canceled, and you can just start anew.{Environment.NewLine}" +
                $"Each Question will also have possible answers written below. Some may require you, to input a text, some may only require a number.{Environment.NewLine}" +
                $"For every Question, there will be a \"Default\" option. If you don't know what to take / don't have a preference, take this.");

                await dm.TriggerTypingAsync();
                await Task.Delay(12000);

                var builder = new DiscordEmbedBuilder()
                {
                    Title = "Do you understand?",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("I understand.", "Yes");
                builder.AddField("What?", "No");

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await this.dep.Interactivity.WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id && x.Author.Id == ctx.Member.Id
                                               && Regex.IsMatch(x.Content, ConfirmRegex),
                            TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"You did not respond in time. Your Settings will not be saved.");
                    return;
                }

                if (Regex.IsMatch(m.Message.Content, YesRegex))
                {
                    userAgrees = true;
                }
                else
                {
                    await dm.SendMessageAsync($"Ok, let me explain it to you.{Environment.NewLine}" +
                        $"I've just sent you a Question. Directly underneath it, you can see a fat \"**I understand**\", and a smaller \"Yes\". Right?{Environment.NewLine}" +
                        $"The \"**I understand**\", is basically just a Description of your possible answer. In this case, the Answer is \"Yes\".{Environment.NewLine}" +
                        $"So you can either answer exactly with \"Yes\", or exactly with \"No\".{Environment.NewLine}" +
                        $"So let's try this again.");
                    await dm.TriggerTypingAsync();
                    await Task.Delay(10000);
                }
            }

            await dm.SendMessageAsync($"Splendid! Now that you know how this works, let's start with the Settings! Just Gimi a second to check if you already have some saved.");
            await dm.TriggerTypingAsync();

            var userSettings = Tables.Discord.UserSetting.Load(ctx.User);

            if (userSettings == null)
            {
                userSettings = new Tables.Discord.UserSetting();
                userSettings.UserId = Convert.ToInt64(ctx.Message.Author.Id);
            }

            await Task.Delay(1000);
            await dm.SendMessageAsync($"There we go. First Question!");
            await dm.TriggerTypingAsync();
            await Task.Delay(4000);

            int? wheelDifficulty = null;

            while (!wheelDifficulty.HasValue)
            {
                var builder = new DiscordEmbedBuilder()
                {
                    Title = "How Difficult would you like the Wheel to be? Lower Difficulties will lower your required Edges, but also your Chance for a good ending :)",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("Easiest Setting. Almost no Edges, will leave you in ruins.", Tables.Discord.UserSetting.WheelDifficultySetting.Baby.ToString());
                builder.AddField(
                    "Easy Setting, for when you are just starting with Edging.",
                    Tables.Discord.UserSetting.WheelDifficultySetting.Easy.ToString());
                builder.AddField(
                    "Default. This is how the Wheel was before the Settings arrived, and how it is before you set up the settings. About 5% Chance to Cum.",
                    Tables.Discord.UserSetting.WheelDifficultySetting.Default.ToString());
                builder.AddField("Pretty Challenging.", Tables.Discord.UserSetting.WheelDifficultySetting.Hard.ToString());
                builder.AddField("This will make every single roll Hardcore. High risk, High reward though.", Tables.Discord.UserSetting.WheelDifficultySetting.Masterbater.ToString());

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await this.dep.Interactivity.WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id
                         && x.Author.Id == ctx.Member.Id,
                    TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"You did not respond in time. Your Settings will not be saved.");
                    return;
                }

                if (Enum.TryParse(m.Message.Content, out Tables.Discord.UserSetting.WheelDifficultySetting wheelDifficultySetting))
                {
                    wheelDifficulty = (int)wheelDifficultySetting;
                }
                else
                {
                    await dm.SendMessageAsync($"You have to precisely enter the name of one of the possible Difficulties.");
                }
            }

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Ok. Next Question!");
            await dm.TriggerTypingAsync();
            await Task.Delay(2000);

            userSettings.WheelDifficulty = wheelDifficulty;

            Tables.Discord.UserSetting.WheelPreferenceSetting? wheelTaskPreference = null;

            while (wheelTaskPreference == null)
            {
                var builder = new DiscordEmbedBuilder()
                {
                    Title = "What kind of task do you prefer? There are no penalties here.",
                    Url = "http://IJustWantThisToBeBlue.com"
                };

                builder.AddField("Edge for 15 Minutes. 30 second Cooldown.", Tables.Discord.UserSetting.WheelPreferenceSetting.Time.ToString());
                builder.AddField("Edge 10 times.", Tables.Discord.UserSetting.WheelPreferenceSetting.Amount.ToString());
                builder.AddField("Edge 10 times per day, for the next 2 Days.", Tables.Discord.UserSetting.WheelPreferenceSetting.Task.ToString());
                builder.AddField("No preference", Tables.Discord.UserSetting.WheelPreferenceSetting.Default.ToString());

                await dm.SendMessageAsync(embed: builder.Build());

                var m = await this.dep.Interactivity.WaitForMessageAsync(
                    x => x.Channel.Id == dm.Id
                         && x.Author.Id == ctx.Member.Id,
                    TimeSpan.FromSeconds(240));

                if (m == null)
                {
                    await dm.SendMessageAsync($"You did not respond in time. Your Settings will not be saved.");
                    return;
                }

                if (Enum.TryParse(m.Message.Content, out Tables.Discord.UserSetting.WheelPreferenceSetting wheelPreferenceSetting))
                {
                    wheelTaskPreference = wheelPreferenceSetting;
                }
                else
                {
                    await dm.SendMessageAsync($"You have to precisely enter the name of one of the possible Difficulties.");
                }
            }

            userSettings.WheelTaskPreference = (int)wheelTaskPreference;

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"That's it for now already! Let me just save that real quick...");
            await dm.TriggerTypingAsync();

            try
            {
                userSettings.Save();
            }
            catch (Exception ex)
            {
                await dm.SendMessageAsync($"**Uhh... something seems to have gone badly wrong here...{Environment.NewLine}" +
                    $"If you see Salem around here somewhere, tell him the following:**");
                await dm.TriggerTypingAsync();
                await Task.Delay(5000);

                string msgToSend = ex.Message;
                while (msgToSend.Length > 1999)
                {
                    await dm.SendMessageAsync(msgToSend.Substring(0, 1999));
                    await dm.TriggerTypingAsync();
                    await Task.Delay(2000);
                    msgToSend = msgToSend.Substring(1999);
                }

                await dm.SendMessageAsync(msgToSend);

                return;
            }

            await dm.SendMessageAsync($"Saved!");

            await dm.TriggerTypingAsync();
            await Task.Delay(1000);
            await dm.SendMessageAsync($"Nice. You can now start using the Wheel with your brand new set of settings \\*-\\*{Environment.NewLine}" +
                                        $"These might get more over time. Sabrina will remind you to revisit them, when it's time.");
        }
    }
}