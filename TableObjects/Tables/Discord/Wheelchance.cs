using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableObjects.Tables
{
    public partial class Discord
    {
        /// <summary>
        /// The wheel chance for a specific Difficulty.
        /// </summary>
        [Table(Name = "WheelChances")]
        public class WheelChance
        {
            /// <summary>
            /// Gets or sets the denial Chance.
            /// </summary>
            [Column]
            public int Denial { get; set; }

            /// <summary>
            /// Gets or sets the difficulty of this.
            /// </summary>
            [Column(IsPrimaryKey = true)]
            public int Difficulty { get; set; }

            /// <summary>
            /// Gets or sets the orgasm Chance.
            /// </summary>
            [Column]
            public int Orgasm { get; set; }

            /// <summary>
            /// Gets or sets the ruin Chance.
            /// </summary>
            [Column]
            public int Ruin { get; set; }

            /// <summary>
            /// Gets or sets the task Chance.
            /// </summary>
            [Column]
            public int Task { get; set; }

            /// <summary>
            /// Load Chances for Difficulty
            /// </summary>
            /// <param name="difficulty">
            /// The difficulty.
            /// </param>
            /// <returns>
            /// The <see cref="WheelChance"/>.
            /// </returns>
            public static WheelChance Load(int difficulty)
            {
                return (from chances in GetMainTable() where chances.Difficulty == difficulty select chances)
                    .FirstOrDefault();
            }

            /// <summary>
            /// Gets the Main Table
            /// </summary>
            /// <returns>
            /// The <see cref="Table{TEntity}"/>.
            /// </returns>
            private static Table<WheelChance> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<WheelChance>();
            }
        }
    }
}
