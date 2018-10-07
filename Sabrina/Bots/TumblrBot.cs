// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TumblrBot.cs" company="SalemsTools">
//   Do whatever
// </copyright>
// <summary>
//   The tumblr bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Timers;

namespace Sabrina.Bots
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;

    using Sabrina.Entities;
    using Sabrina.Entities.TumblrBlog;
    using Sabrina.Entities.TumblrPost;

    using Tables = TableObjects.Tables;

    using SabrinaConfig = Configuration.Config;

    /// <summary>
    ///     The tumblr bot.
    /// </summary>
    internal class TumblrBot
    {
        private Timer postTimer;

        /// <summary>
        /// The texts to choose from randomly to post.
        /// </summary>
        /// TODO: Put these in the Database. Or somewhere else. Just not here.
        private static readonly string[] PostingTexts =
            {
                "( ͡° ͜ʖ ͡°)", "*-*", "\"Next on Foot-TV:\"", "<.<    ...    >.>    ...    *foot gib*", "Another one!",
                "ayyy", "De-feet-ed",
                "Did you know, you can click on these Titles, to go to the Original Tumblr Page, where i get these pics from? *shocked*",
                "Give these feet some love...", "Have some new foot-age", "How 'bout toes?",
                "I got a delivery for... \"foot freak\"?", "I hope this doesn't get you on the wrong foot",
                "I'm sure you'll get a kick out of this pic", "Let's ask ourselves one Question. What are feet?",
                "Mhhhh", "Mhhm, that certainly is foot for thought", "More feet they said. It'll be fun they said.",
                "More feet!", "Nya?", "One small step for lewds, one larger step for Pervert-kind", "Oof", "OwO",
                "Picture.Post(\"Foot\");", "Sabrina feeturing Aki", "Salem is the feet of Development",
                "Send your own Idea's to @Salem#9999! (Please, i'm completely out of foot puns now)",
                "THE HORDE DEMANDS MORE!", "These pictures are the sole purpose of me",
                "This one may become one of my favorites", "To feet, or not to feet.",
                "Touch 'em, tickle 'em, lick 'em, fu- cuddle 'em", "UwU",
                "Why not share this picture with your teacher? ^^", "Yesssss"
            };

        /// <summary>
        ///     The client.
        /// </summary>
        private readonly DiscordClient client;
        
        /// <summary>
        ///     Initializes a new instance of the <see cref="TumblrBot" /> class.
        /// </summary>
        /// <param name="client">
        ///     The client of the Bot.
        /// </param>
        public TumblrBot(DiscordClient client)
        {
            this.client = client;
            
            Task.Run(this.Run);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to continue processing or not.
        /// </summary>
        public bool Exit { get; set; } = false;

        /// <summary>
        /// Post a Random Picture
        /// </summary>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public static async Task PostRandom(DiscordClient client)
        {
            var postCount = GetPostCount();

            var randomPostNr = Helpers.RandomGenerator.RandomInt(0, postCount - 1);

            TumblrPost post = GetTumblrPostByOffset(randomPostNr);

            Tables.Discord.TumblrPost databasePost = Tables.Discord.TumblrPost.Load(post.Response.Posts[0].Id);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (databasePost != null)
            {
                if (databasePost.LastPosted != null && DateTime.Now - databasePost.LastPosted < new TimeSpan(30, 0, 0, 0))
                {
                    return;
                }

                if (databasePost.IsLoli == 1)
                {
                    return;
                }

                var link = post.Response.Posts[0].Photos[0].AltSizes.OrderBy(e => e.Width).Last().Url;

                await PostSpecific(client, config, link, post);

                databasePost.LastPosted = DateTime.Now;
                databasePost.Save();
            }
            else
            {
                string link = post.Response.Posts[0].Photos[0].AltSizes.OrderBy(e => e.Width).Last().Url;

                databasePost = new Tables.Discord.TumblrPost
                                   {
                                       TumblrId = post.Response.Posts[0].Id, IsLoli = -1, LastPosted = DateTime.Now
                                   };

                databasePost.Save();

                await PostSpecific(client, config, link, post);
            }
        }

        /// <summary>
        ///     Check Message for loli.
        /// </summary>
        /// <param name="e">
        ///     The Event thrown when a Reaction is created for a Message.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        public async Task CheckLoli(MessageReactionAddEventArgs e)
        {
            DiscordMessage msg;

            try
            {
                 msg = await e.Message.Channel.GetMessageAsync(e.Message.Id);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                await e.Channel.SendMessageAsync("Uhh... i couldn't find the Message somehow. I blame Discord");
                return;
            }
            
            if (!(await msg.Channel.Guild.GetMemberAsync(e.User.Id)).Roles.Any(
                    role => role.Name == "admin" || role.Name == "minion" || role.Name == "techno kitty"))
            {
                return;
            }

            if (SabrinaConfig.Tumblr.ChannelsToPostTo.Contains(e.Channel.Id) && msg.Author == this.client.CurrentUser)
            {
                if (msg.Embeds.Count == 1)
                {
                    DiscordEmbedFooter footer = msg.Embeds[0].Footer;
                    if (footer != null && !string.IsNullOrEmpty(footer.Text))
                    {
                        bool canParse = long.TryParse(footer.Text, out long id);

                        if (canParse)
                        {
                            if (SabrinaConfig.Emojis.Declines.Contains(e.Emoji.GetDiscordName()))
                            {
                                await this.SetLoli(id, true);
                                await msg.RespondAsync("Thanks, i've removed the loli >:|");
                                await msg.DeleteAsync("Loli");
                            }
                            else if (SabrinaConfig.Emojis.Confirms.Contains(e.Emoji))
                            {
                                await this.SetLoli(id, false);
                                await msg.CreateReactionAsync(e.Emoji);
                            }
                        }
                    }
                }
            }
        }
        

        /// <summary>
        ///     Returns count of all Posts
        /// </summary>
        /// <returns>
        ///     A Count of all Posts.
        /// </returns>
        private static int GetPostCount()
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/info";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            TumblrBlog blog = TumblrBlog.FromJson(json);
            return Convert.ToInt32(blog.Response.Blog.Posts);
        }

        /// <summary>
        ///     Gets a specific Tumblr post
        /// </summary>
        /// <param name="offset">
        ///     The offset of the post
        /// </param>
        /// <returns>
        ///     The <see cref="TumblrPost" />.
        /// </returns>
        private static TumblrPost GetTumblrPostByOffset(int offset)
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts/photo";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";
            url += "&limit=1";
            url += $"&offset={offset}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("api_key", "uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki");
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                json = reader.ReadToEnd();
            }

            return TumblrPost.FromJson(json);
        }

        /// <summary>
        ///     Posts the Tumblr Post to Discord
        /// </summary>
        /// <param name="client">
        /// The Client
        /// </param>
        /// <param name="config">
        ///     The config.
        /// </param>
        /// <param name="link">
        ///     The link.
        /// </param>
        /// <param name="post">
        ///     The post.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        private static async Task PostSpecific(DiscordClient client, Configuration config, string link, TumblrPost post)
        {
            foreach (ulong channelId in SabrinaConfig.Tumblr.ChannelsToPostTo)
            {
                config.AppSettings.Settings["LastTumblrPost"].Value = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                config.Save();

                DiscordChannel channel = await client.GetChannelAsync(channelId);

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                                  {
                                      ImageUrl = link,
                                      Color = DiscordColor.Green,
                                      Title =
                                          PostingTexts
                                              [Helpers.RandomGenerator.RandomInt(0, PostingTexts.Length)],
                                      Url = post.Response.Posts[0].PostUrl,
                                      Footer = new DiscordEmbedBuilder.EmbedFooter
                                                   {
                                                       Text = post.Response.Posts[0].Id.ToString()
                                                   },
                                      Author = new DiscordEmbedBuilder.EmbedAuthor
                                                   {
                                                       Name = post.Response.Posts[0].BlogName,
                                                       Url = post.Response.Posts[0].PostUrl
                                                   }
                                  };

                DiscordMessage msg = await channel.SendMessageAsync(embed: builder.Build());
            }
        }
        

        /// <summary>
        ///     The Initial Function.
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.NamingRules",
            "SA1305:FieldNamesMustNotUseHungarianNotation",
            Justification = "Reviewed. Suppression is OK here.")]
        private async Task Run()
        {
            var lastPost = Tables.Discord.TumblrPost.LastPost();

            if (lastPost?.LastPosted == null || DateTime.Now > lastPost.LastPosted.Value + TimeSpan.FromHours(1))
            {
                await PostRandom(this.client);
            }
            else
            {
                await Task.Delay(DateTime.Now - lastPost.LastPosted.Value);
                await PostRandom(this.client);
            }
            
            postTimer = new Timer(TimeSpan.FromHours(1).TotalMilliseconds)
            {
                AutoReset = true
            };
            postTimer.Elapsed += PostTimer_Elapsed;
            postTimer.Start();
        }

        private async void PostTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await PostRandom(this.client);
        }

        /// <summary>
        ///     Set, or unset Post as containing Loli
        /// </summary>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="isLoli">
        ///     The is loli.
        /// </param>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [SuppressMessage(
            "StyleCop.CSharp.DocumentationRules",
            "SA1650:ElementDocumentationMustBeSpelledCorrectly",
            Justification = "Reviewed. Suppression is OK here.")]
        private async Task SetLoli(long id, bool isLoli)
        {
            using (SqlConnection conn = new SqlConnection(SabrinaConfig.DataBaseConnectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    Tables.Discord.TumblrPost tumblrPost = Tables.Discord.TumblrPost.Load(id);
                    tumblrPost.IsLoli = isLoli ? 1 : 0;
                    tumblrPost.Save();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}