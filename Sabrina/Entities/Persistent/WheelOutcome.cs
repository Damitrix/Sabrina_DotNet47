// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WheelOutcome.cs" company="">
//
// </copyright>
// <summary>
//   Defines the WheelOutcome type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities.Persistent
{
    using System;

    using DSharpPlus.Entities;

    /// <summary>
    /// The wheel outcome.
    /// </summary>
    internal abstract class WheelOutcome
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WheelOutcome"/> class.
        /// </summary>
        /// <param name="outcome">
        /// The outcome.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected WheelOutcome(TableObjects.Tables.Discord.SlaveReport.Outcome outcome, TableObjects.Tables.Discord.UserSetting settings)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            this.Outcome = outcome;
        }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        public abstract TableObjects.Tables.Discord.SlaveReport.Outcome Outcome { get; protected set; }

        /// <summary>
        /// Gets or sets the denial time.
        /// </summary>
        public abstract TimeSpan DenialTime { get; protected set; }

        /// <summary>
        /// Gets or sets the wheel locked time.
        /// </summary>
        public abstract TimeSpan WheelLockedTime { get; protected set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public abstract string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the embed.
        /// </summary>
        public abstract DiscordEmbed Embed { get; protected set; }

        /// <summary>
        /// Gets or sets the chance.
        /// </summary>
        public abstract int Chance { get; protected set; }
    }
}