﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Edge.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   Defines the Edge type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.WheelOutcomes
{
    using DSharpPlus.Entities;
    using Models;
    using Sabrina.Entities.Persistent;
    using System;

    /// <summary>
    /// The edge Outcome.
    /// </summary>
    internal sealed class Edge : WheelOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        public Edge(SlaveReportsExtension.Outcome outcome, UserSettings settings, DiscordContext context) : base(outcome, settings, context)
        {
            int edgeMinutes = Helpers.RandomGenerator.RandomInt(5, 31);

            if (settings.WheelDifficulty != null)
            {
                switch ((SlaveReportsExtension.WheelDifficultyPreference)settings.WheelDifficulty)
                {
                    case SlaveReportsExtension.WheelDifficultyPreference.Baby:
                        edgeMinutes = Convert.ToInt32((double)edgeMinutes / 2);
                        break;

                    case SlaveReportsExtension.WheelDifficultyPreference.Easy:
                        edgeMinutes = Convert.ToInt32((double)edgeMinutes / 1.5);
                        break;

                    case SlaveReportsExtension.WheelDifficultyPreference.Hard:
                        edgeMinutes = Convert.ToInt32((double)edgeMinutes * 1.5);
                        break;

                    case SlaveReportsExtension.WheelDifficultyPreference.Masterbater:
                        edgeMinutes = Convert.ToInt32((double)edgeMinutes * 2);
                        break;
                }
            }

            string flavorText = "Boring...";

            if (edgeMinutes > 10)
            {
                flavorText = "Too easy...";
            }
            else if (edgeMinutes > 15)
            {
                flavorText = "Not too bad!";
            }
            else if (edgeMinutes > 20)
            {
                flavorText = "Kind of difficult!";
            }
            else if (edgeMinutes > 25)
            {
                flavorText = "Ouch!";
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = flavorText,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "And spin again afterwards~"
                }
            };

            if (Helpers.RandomGenerator.RandomInt(0, 2) == 0)
            {
                if (settings.WheelTaskPreference != null && ((SlaveReportsExtension.WheelTaskPreferenceSetting)settings.WheelTaskPreference).HasFlag(SlaveReportsExtension.WheelTaskPreferenceSetting.Time))
                {
                    this.Chance *= 3;
                }

                this.Text = $"{flavorText} Edge over and over (at least 30s Cooldown) for {edgeMinutes} minutes, then spin again~";

                builder.Description = $"Edge over and over (at least 30s Cooldown) for {edgeMinutes} minutes";

                this.WheelLockedTime = new TimeSpan(0, edgeMinutes, 0);
            }
            else
            {
                if (settings.WheelTaskPreference != null && ((SlaveReportsExtension.WheelTaskPreferenceSetting)settings.WheelTaskPreference).HasFlag(SlaveReportsExtension.WheelTaskPreferenceSetting.Amount))
                {
                    this.Chance *= 3;
                }

                this.Text = $"{flavorText} Edge {edgeMinutes / 2} times, then spin again~";

                builder.Description = $"Edge {edgeMinutes / 2} times";
            }

            this.Embed = builder.Build();
            this.Outcome = SlaveReportsExtension.Outcome.Task;
        }

        /// <summary>
        /// Gets or sets the chance of this being used.
        /// </summary>
        public override int Chance { get; protected set; } = 20;

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public override TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the embed to display to the user.
        /// </summary>
        public override DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public override SlaveReportsExtension.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the text to display to the user.
        /// </summary>
        public override string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public override TimeSpan WheelLockedTime { get; protected set; }
    }
}