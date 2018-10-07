using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableObjects.Tables
{
    public partial class Discord
    {
        /// <summary>
        /// Post from Tumblr. can be saved to DB.
        /// </summary>
        [Table(Name = "TumblrPosts")]
        public class TumblrPost
        {
            /// <summary>
            /// Gets or sets a value, showing if the Post is loli.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
            [Column]
            public long IsLoli { get; set; } = -1;

            /// <summary>
            /// Gets or sets the time, this was last posted.
            /// </summary>
            [Column]
            public DateTime? LastPosted { get; set; }

            /// <summary>
            /// Gets or sets the tumblr id.
            /// </summary>
            [Column(IsPrimaryKey = true, IsDbGenerated = false, Name = "TumblrID")]
            public long TumblrId { get; set; }

            public static TumblrPost LastPost()
            {
                return (from posts in GetMainTable() orderby posts.LastPosted descending select posts).FirstOrDefault();
            }

            /// <summary>
            /// Loads a Tumblr Post.
            /// </summary>
            /// <param name="tumblrId">
            /// The tumblr id.
            /// </param>
            /// <returns>
            /// The <see cref="TumblrPost"/>.
            /// </returns>
            public static TumblrPost Load(long tumblrId)
            {
                return (from posts in GetMainTable() where posts.TumblrId == tumblrId select posts).FirstOrDefault();
            }

            /// <summary>
            /// Save this to DB
            /// </summary>
            public void Save()
            {
                Table<TumblrPost> db = GetMainTable();

                if (db.Any(post => post.TumblrId == this.TumblrId))
                {
                    IQueryable<TumblrPost> tumblrPosts = from posts in db where posts.TumblrId == this.TumblrId select posts;

                    foreach (var post in tumblrPosts)
                    {
                        post.IsLoli = this.IsLoli;
                        post.LastPosted = this.LastPosted;
                    }

                    db.Context.SubmitChanges();
                }
                else
                {
                    db.InsertOnSubmit(this);
                    db.Context.SubmitChanges();
                }
            }

            private static Table<TumblrPost> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<TumblrPost>();
            }
        }
    }
}
