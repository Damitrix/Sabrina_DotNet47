using System;
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
        ///     Users Table
        /// </summary>
        [Table(Name = "Users")]
        public class User
        {
            /// <summary>
            ///     Gets or sets the reason for ban
            /// </summary>
            [Column(Name = "BanReason", DbType = "nvarchar(max)")]
            public string BanReason { get; set; }

            /// <summary>
            ///     Gets or sets time banned
            /// </summary>
            [Column(Name = "BanTime", DbType = "datetime")]
            public DateTime? BanTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets time denied
            /// </summary>
            [Column(Name = "DenialTime", DbType = "datetime")]
            public DateTime? DenialTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets the reason for lock
            /// </summary>
            [Column(Name = "LockReason", DbType = "nvarchar(max)")]
            public string LockReason { get; set; }

            /// <summary>
            ///     Gets or sets time locked from the wheel
            /// </summary>
            [Column(Name = "LockTime", DbType = "datetime")]
            public DateTime? LockTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets the reason for ban
            /// </summary>
            [Column(Name = "MuteReason", DbType = "nvarchar(max)")]
            public string MuteReason { get; set; }

            /// <summary>
            ///     Gets or sets time muted
            /// </summary>
            [Column(Name = "MuteTime", DbType = "datetime")]
            public DateTime? MuteTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets time only ruins are permitted
            /// </summary>
            [Column(Name = "RuinTime", DbType = "datetime")]
            public DateTime? RuinTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets the reason for the special time
            /// </summary>
            [Column(Name = "SpecialReason", DbType = "nvarchar(max)")]
            public string SpecialReason { get; set; }

            /// <summary>
            ///     Gets or sets time for special tasks
            /// </summary>
            [Column(Name = "SpecialTime", DbType = "datetime")]
            public DateTime? SpecialTime { get; set; } = DateTime.Now;

            /// <summary>
            ///     Gets or sets the edges a User has ever did
            /// </summary>
            [Column(Name = "TotalEdges", DbType = "int")]
            public int? TotalEdges { get; set; }

            /// <summary>
            ///     Gets or sets UserId
            /// </summary>
            [Column(IsPrimaryKey = true, DbType = "bigint", Name = "UserID")]
            public ulong UserId { get; set; }

            /// <summary>
            ///     Gets or sets the edges a User has to do
            /// </summary>
            [Column(Name = "WalletEdges", DbType = "int")]
            public int? WalletEdges { get; set; }

            /// <summary>
            ///     Load User from Table by Discord User
            /// </summary>
            /// <param name="user">User provided by Discord</param>
            /// <returns>User from Table</returns>
            public static User Load(DiscordUser user)
            {
                return (from settings in GetMainTable() where settings.UserId == user.Id select settings).FirstOrDefault();
            }

            /// <summary>
            /// Save this Report to the Database
            /// </summary>
            public void Save()
            {
                Table<User> db = GetMainTable();

                if (db.Any(e => e.UserId == this.UserId))
                {
                    IQueryable<User> user = from users in db where users.UserId == this.UserId select users;

                    foreach (var userRow in user)
                    {
                        userRow.UserId = this.UserId;
                        userRow.BanTime = this.BanTime;
                        userRow.BanReason = this.BanReason;
                        userRow.MuteTime = this.MuteTime;
                        userRow.MuteReason = this.MuteReason;
                        userRow.LockTime = this.LockTime;
                        userRow.LockReason = this.LockReason;
                        userRow.DenialTime = this.DenialTime;
                        userRow.RuinTime = this.RuinTime;
                        userRow.SpecialTime = this.SpecialTime;
                        userRow.SpecialReason = this.SpecialReason;
                        userRow.WalletEdges = this.WalletEdges;
                        userRow.TotalEdges = this.TotalEdges;
                    }

                    db.Context.SubmitChanges();
                }
                else
                {
                    db.InsertOnSubmit(this);
                    db.Context.SubmitChanges();
                }
            }

            public void Reset()
            {
                this.BanTime = DateTime.Now;
                this.MuteTime = DateTime.Now;
                this.LockTime = DateTime.Now;
                this.DenialTime = DateTime.Now;
                this.RuinTime = DateTime.Now;
                this.SpecialTime = DateTime.Now;

                this.Save();
            }

            /// <summary>
            ///     Get Main Table
            /// </summary>
            /// <returns>Table of Users</returns>
            private static Table<User> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<User>();
            }
        }
    }
}
