// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ban.cs" company="">
//
// </copyright>
// <summary>
//   Defines the Ban type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
    using System;

    using DSharpPlus.Entities;

    using Sabrina.Entities.Persistent;

    using Tables = TableObjects.Tables;

    internal sealed class Ban : WheelOutcome
    {
        public override Tables.Discord.SlaveReport.Outcome Outcome { get; protected set; }
        public override TimeSpan DenialTime { get; protected set; }
        public override TimeSpan WheelLockedTime { get; protected set; }
        public override string Text { get; protected set; }
        public override DiscordEmbed Embed { get; protected set; }
        public override int Chance { get; protected set; } = 10;

        private static string[] bans = new[] { "all porn besides anime/hentai", "all porn besides foot-related porn" };

        private readonly int minBanTime = 2;
        private readonly int maxBanTime = 7 + 1;

        private readonly int minEdgeAmount = 7;
        private readonly int maxEdgeAmount = 15 + 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ban"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public Ban(Tables.Discord.SlaveReport.Outcome outcome, Tables.Discord.UserSetting settings) : base(outcome, settings)
        {
            if (settings.WheelTaskPreference == null)
            {
                settings.WheelTaskPreference = (int)Tables.Discord.UserSetting.WheelPreferenceSetting.Default;
                settings.Save();
            }

            if (settings.WheelTaskPreference != null && ((Tables.Discord.UserSetting.WheelPreferenceSetting)settings.WheelTaskPreference).HasFlag(Tables.Discord.UserSetting.WheelPreferenceSetting.Task))
            {
                this.Chance *= 6;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            int time = Helpers.RandomGenerator.RandomInt(this.minBanTime, this.maxBanTime);
            settings.WheelDifficulty = settings.WheelDifficulty ?? 2;

            if (Helpers.RandomGenerator.RandomInt(0, 2) == 0)
            {
                builder.Title = "Content ban!";
                builder.Description =
                    $"You are banned from {bans[Helpers.RandomGenerator.RandomInt(0, bans.Length)]} for {time} days! If you already had the same ban, consider it reset.";
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Now reroll!"
                };
            }
            else
            {
                int edgeAmount = Helpers.RandomGenerator.RandomInt(this.minEdgeAmount, this.maxEdgeAmount) * 2;

                switch ((Tables.Discord.UserSetting.WheelDifficultySetting)settings.WheelDifficulty)
                {
                    case Tables.Discord.UserSetting.WheelDifficultySetting.Baby:
                        edgeAmount = edgeAmount / 4;
                        break;

                    case Tables.Discord.UserSetting.WheelDifficultySetting.Easy:
                        edgeAmount = edgeAmount / 2;
                        break;

                    case Tables.Discord.UserSetting.WheelDifficultySetting.Hard:
                        edgeAmount = edgeAmount * 2;
                        break;

                    case Tables.Discord.UserSetting.WheelDifficultySetting.Masterbater:
                        edgeAmount = edgeAmount * 4;
                        break;
                }

                builder.Title = "Edging Task!";
                builder.Description =
                    $"You'll have to Edge {edgeAmount} times a Day, for {time} days! If you already had the same task, consider it reset.";
                builder.Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Now reroll!"
                };
            }

            this.Embed = builder.Build();
            this.Outcome = Tables.Discord.SlaveReport.Outcome.Task;
        }
    }
}