// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Content.cs" company="SalemsTools">
//   Do Whatever
// </copyright>
// <summary>
//   Defines the Content type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sabrina.Entities
{
    using System.Diagnostics.CodeAnalysis;

    using TableObjects.Tables;

    /// <summary>
    /// The content.
    /// </summary>
    public class Content
    {
        /// <summary>
        /// The link.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public string Link;

        /// <summary>
        /// The outcome.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
        public Discord.SlaveReport.Outcome Outcome;
    }
}