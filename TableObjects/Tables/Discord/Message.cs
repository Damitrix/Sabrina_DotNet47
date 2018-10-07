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
        [Table(Name = "Messages")]
        public class Message
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="Message"/> class.
            /// </summary>
            /// <param name="authorId">The Author's id</param>
            /// <param name="text">The Messages Text</param>
            /// <param name="channelId">The Channel ID</param>
            /// <param name="creationTime">The Creation Time</param>
            public Message(ulong authorId, string text, ulong channelId, DateTime creationTime)
            {
                this.AuthorId = authorId;
                this.MessageText = text;
                this.ChannelId = channelId;
                this.CreationDate = creationTime;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Message"/> class. 
            /// </summary>
            public Message()
            {
            }

            /// <summary>
            /// Gets or sets the MessageID
            /// </summary>
            [Column(Name = "MessageID", IsPrimaryKey = true, IsDbGenerated = true)]
            public int MessageId { get; set; }

            /// <summary>
            /// Gets or sets the Author ID
            /// </summary>
            [Column(Name = "AuthorID", DbType = "bigint")]
            public ulong AuthorId { get; set; }

            /// <summary>
            /// Gets or sets the Message Text
            /// </summary>
            [Column(Name = "MessageText", DbType = "ntext")]
            public string MessageText { get; set; }

            /// <summary>
            /// Gets or sets the Channel ID
            /// </summary>
            [Column(Name = "ChannelID", DbType = "bigint")]
            public ulong ChannelId { get; set; }

            /// <summary>
            /// Gets or sets the task Chance.
            /// </summary>
            [Column(Name = "CreationDate")]
            public DateTime CreationDate { get; set; }

            /// <summary>
            /// Load a Message by ID
            /// </summary>
            /// <param name="id">
            /// The message id.
            /// </param>
            /// <returns>
            /// The <see cref="Message"/>.
            /// </returns>
            public static Message Load(int id)
            {
                return (from messages in GetMainTable() where messages.MessageId == id select messages)
                    .FirstOrDefault();
            }

            /// <summary>
            /// Saves the newly created Message. Edit is not possible rn.
            /// </summary>
            public void Save()
            {
                Table<Message> db = GetMainTable();
                db.InsertOnSubmit(this);
                db.Context.SubmitChanges();
            }

            /// <summary>
            /// Gets the Main Table
            /// </summary>
            /// <returns>
            /// A <see cref="Table"/>.
            /// </returns>
            private static Table<Message> GetMainTable()
            {
                var db = new DataContext(Configuration.Config.DataBaseConnectionString);
                return db.GetTable<Message>();
            }
        }
    }
}
