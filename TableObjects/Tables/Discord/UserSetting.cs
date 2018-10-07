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
        /// The Setting of one User.
        /// </summary>
        [Table(Name = "UserSettings")]
        public class UserSetting
        {
            /// <summary>
            /// The wheel difficulty.
            /// </summary>
            private WheelDifficultySetting? wheelDifficulty;

            /// <summary>
            /// The wheel task preference.
            /// </summary>
            private WheelPreferenceSetting? wheelTaskPreference;

            /// <summary>
            /// The wheel difficulty setting.
            /// Always name something User-Readable
            /// </summary>
            public enum WheelDifficultySetting
            {
                /// <summary>
                /// Easiest Setting.
                /// </summary>
                Baby,

                /// <summary>
                /// Easy Setting
                /// </summary>
                Easy,

                /// <summary>
                /// The Default Setting. Should never be changed in DB.
                /// </summary>
                Default,

                /// <summary>
                /// Hard Setting.
                /// </summary>
                Hard,

                /// <summary>
                /// The hardest Setting.
                /// </summary>
                Masterbater
            }

            /// <summary>
            /// The wheel preference setting.
            /// Supports <see cref="FlagsAttribute"/>
            /// Always name something User-Readable.
            /// </summary>
            [Flags]
            public enum WheelPreferenceSetting
            {
                /// <summary>
                /// The default setting.
                /// </summary>
                Default = 0,

                /// <summary>
                /// Set to prefer Time-Based
                /// </summary>
                Time = 1,

                /// <summary>
                /// Set to prefer Amount-based
                /// </summary>
                Amount = 2,

                /// <summary>
                /// Set to prefer Task-based
                /// </summary>
                Task = 4
            }

            /// <summary>
            /// Gets or sets the user id. Handled by Discord. So never Change.
            /// </summary>
            [Column(IsPrimaryKey = true, Name = "UserID")]
            public long UserId { get; set; }

            /// <summary>
            /// Gets or sets the wheel difficulty.
            /// </summary>
            [Column]
            public int? WheelDifficulty
            {
                get => (int?)this.wheelDifficulty;
                set => this.wheelDifficulty = (WheelDifficultySetting?)value;
            }

            /// <summary>
            /// Gets or sets the wheel task preference.
            /// </summary>
            [Column]
            public int? WheelTaskPreference
            {
                get => (int?)this.wheelTaskPreference;
                set => this.wheelTaskPreference = (WheelPreferenceSetting?)value;
            }

            /// <summary>
            /// Load a specific User.
            /// </summary>
            /// <param name="user">
            /// The user.
            /// </param>
            /// <returns>
            /// The <see cref="UserSetting"/>.
            /// </returns>
            public static UserSetting Load(DiscordUser user)
            {
                long id = Convert.ToInt64(user.Id);

                return (from settings in GetMainTable() where settings.UserId == id select settings).FirstOrDefault();
            }

            /// <summary>
            /// Save this to DB.
            /// </summary>
            public void Save()
            {
                Table<UserSetting> db = GetMainTable();

                if (db.Any(e => e.UserId == this.UserId))
                {
                    IQueryable<UserSetting> setting = from settings in db where settings.UserId == this.UserId select settings;

                    foreach (var settingRow in setting)
                    {
                        settingRow.WheelDifficulty = this.WheelDifficulty;
                        settingRow.WheelTaskPreference = this.WheelTaskPreference;
                    }

                    db.Context.SubmitChanges();
                }
                else
                {
                    db.InsertOnSubmit(this);
                    db.Context.SubmitChanges();
                }
            }

            private static Table<UserSetting> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<UserSetting>();
            }
        }
    }
}
