﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace TableObjects.Tables
{
    public partial class Discord
    {
        /// <summary>
        /// Table of Slave Reports
        /// </summary>
        [Table(Name = "SlaveReports")]
        public class SlaveReport
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SlaveReport"/> class.
            /// Create new SlaveReport
            /// </summary>
            public SlaveReport()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SlaveReport"/> class.
            /// </summary>
            /// <param name="datetime">
            /// Time of Creation
            /// </param>
            /// <param name="user">
            /// User from Discord
            /// </param>
            /// <param name="edges">
            /// Edges done
            /// </param>
            /// <param name="time">
            /// Timespan of Task
            /// </param>
            /// <param name="outcome">
            /// Outcome of Task
            /// </param>
            public SlaveReport(DateTime datetime, DiscordUser user, int edges, TimeSpan time, Outcome outcome)
            {
                this.TimeOfReport = datetime;
                this.UserId = Convert.ToInt64(user.Id);
                this.Edges = edges;
                this.TimeSpan = time.Ticks;
                this.SessionOutcome = outcome.ToString();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SlaveReport"/> class.
            /// </summary>
            /// <param name="datetime">
            /// Time of Creation
            /// </param>
            /// <param name="user">
            /// User from Discord
            /// </param>
            /// <param name="edges">
            /// Edges done
            /// </param>
            /// <param name="time">
            /// Timespan of Task
            /// </param>
            /// <param name="outcome">
            /// Outcome of Task
            /// </param>
            public SlaveReport(DateTime datetime, DiscordUser user, int edges, TimeSpan time, string outcome)
                : this(datetime, user, edges, time, (Outcome)Enum.Parse(typeof(Outcome), outcome))
            {
            }

            /// <summary>
            /// Outcome of Task for Report
            /// </summary>
            [Flags]
            public enum Outcome
            {
                /// <summary>
                /// No touching allowed.
                /// </summary>
                Denial = 1,

                /// <summary>
                /// Only ruins allowed
                /// </summary>
                Ruin = 2,

                /// <summary>
                /// Have fun :3
                /// </summary>
                Orgasm = 4,

                /// <summary>
                /// A Task for the User to do.
                /// </summary>
                Task = 8
            }

            /// <summary>
            /// Gets or sets the edges to do.
            /// </summary>
            [Column]
            public int Edges { get; set; }

            /// <summary>
            /// Gets or sets the session outcome.
            /// </summary>
            [Column]
            public string SessionOutcome { get; set; }

            /// <summary>
            /// Gets or sets the slave report id. Autogenerated by Database, so please be careful when accessing.
            /// </summary>
            [Column(IsPrimaryKey = true, IsDbGenerated = true)]
            public int SlaveReportId { get; set; }

            /// <summary>
            /// Gets or sets the time of report.
            /// </summary>
            [Column]
            public DateTime TimeOfReport { get; set; }

            /// <summary>
            /// Gets or sets the time span.
            /// </summary>
            [Column]
            public long TimeSpan { get; set; }

            /// <summary>
            /// Gets or sets the user id.
            /// </summary>
            [Column]
            public long UserId { get; set; }

            /// <summary>
            ///     Load all Slave Reports from a User
            /// </summary>
            /// <param name="user">The User to Load</param>
            /// <returns>All Reports for Slave</returns>
            public static IQueryable<SlaveReport> Load(DiscordUser user)
            {
                long id = Convert.ToInt64(user.Id);

                return from report in GetMainTable() where report.UserId == id select report;
            }

            /// <summary>
            ///     Load all Slave reports between two Dates
            /// </summary>
            /// <param name="user">User to Load</param>
            /// <param name="startDate">Start Date</param>
            /// <param name="endDate">End Date</param>
            /// <returns>Multiple Reports for Slave</returns>
            public static IQueryable<SlaveReport> Load(DiscordUser user, DateTime startDate, DateTime endDate)
            {
                IQueryable<SlaveReport> reports = Load(user);

                return from report in reports
                       where report.TimeOfReport > startDate && report.TimeOfReport < endDate
                       select report;
            }

            /// <summary>
            ///     Load all SlaveReports between specified Dates
            /// </summary>
            /// <param name="startDate">Start Date</param>
            /// <param name="endDate">End Date</param>
            /// <returns>Multiple Report of Slave</returns>
            public static IQueryable<SlaveReport> Load(DateTime startDate, DateTime endDate)
            {
                return from report in GetMainTable()
                       where report.TimeOfReport > startDate && report.TimeOfReport < endDate
                       select report;
            }

            /// <summary>
            /// Saves the Report to the DB
            /// </summary>
            public void Save()
            {
                Table<SlaveReport> db = GetMainTable();
                db.InsertOnSubmit(this);
                db.Context.SubmitChanges();
            }

            /// <summary>
            /// Gets the Main Table
            /// </summary>
            /// <returns>Main Table used here</returns>
            private static Table<SlaveReport> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<SlaveReport>();
            }
        }
    }
}
