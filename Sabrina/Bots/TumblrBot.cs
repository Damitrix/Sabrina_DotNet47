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
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Sabrina.Entities.TumblrBlog;
    using Sabrina.Entities.TumblrPost;
    using Sabrina.Models;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    /// <summary>
    ///     The tumblr bot.
    /// </summary>
    internal class TumblrBot
    {
        private DiscordClient _client;
        private DiscordContext _context;
        private DiscordContext _updateContext;
        private Timer _postTimer;
        private Timer _updateTimer;

        public TumblrBot(DiscordClient client, DiscordContext context)
        {
            _context = new DiscordContext();
            _updateContext = new DiscordContext();
            _client = client;
            _postTimer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds)
            {
                AutoReset = true
            };
            _postTimer.Elapsed += _postTimer_Elapsed;
            _postTimer.Start();
            _updateTimer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds)
            {
                AutoReset = true
            };
            _updateTimer.Elapsed += _updateTimer_Elapsed;
            _updateTimer.Start();

            Task.Run(async () => await UpdateDatabase());
        }

        public static async Task PostRandom(DiscordClient client, DiscordContext context)
        {
            TumblrPosts post = null;
            int cDays = 30;
            Random rnd = new Random();

            while (post == null)
            {
                var minDateTime = DateTime.Now - TimeSpan.FromDays(cDays);
                var validPosts = context.TumblrPosts.Where(tPost => tPost.LastPosted == null || tPost.LastPosted < minDateTime);
                var count = validPosts.Count();

                if (count == 0)
                {
                    cDays--;
                    continue;
                }
                foreach (var channel in Configuration.Config.Tumblr.ChannelsToPostTo)
                {
                    var rndInt = rnd.Next(count);
                    post = validPosts.Skip(count - 1).First();

                    var cToPostTo = await client.GetChannelAsync(channel);

                    var builder = new DiscordEmbedBuilder()
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor()
                        {
                            Name = "YourAnimeAddiction"
                        },
                        Color = DiscordColor.Orange,
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = post.TumblrId.ToString()
                        },
                        ImageUrl = GetTumblrPostById(post.TumblrId).Response.Posts[0].Photos.First().AltSizes.OrderByDescending(size => size.Height).First().Url,
                        Title = context.Puns.Skip(new Random().Next(context.Puns.Count() - 1)).First().Text
                    };

                    post.LastPosted = DateTime.Now;

                    await context.SaveChangesAsync();
                    await cToPostTo.SendMessageAsync(embed: builder.Build());
                }
            }
        }

        public async Task CheckLoli(MessageReactionAddEventArgs e)
        {
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
        private static TumblrPost GetTumblrPostsByOffset(int offset)
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts/photo";
            url += "?api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";
            url += "&limit=20";
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
        /// Get's a specific Tumblr Post
        /// </summary>
        /// <param name="id">The ID of the Tumblr Post</param>
        /// <returns>
        ///     The <see cref="TumblrPost" />.
        /// </returns>
        private static TumblrPost GetTumblrPostById(long id)
        {
            string json = string.Empty;
            var url = @"http://api.tumblr.com/v2/blog/deliciousanimefeet.tumblr.com/posts";
            url += $"?id={id}";
            url += "&api_key=uUXKMGxY2yGFCqey98rT9T0jU4ZBke2EgiqPPRhv2eCNIYeuki";

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

        private async void _postTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var lastPost = _context.SabrinaSettings.First().LastTumblrPost;
            if (lastPost != null && lastPost > DateTime.Now - TimeSpan.FromHours(1))
            {
                return;
            }

            await PostRandom();

            _context.SabrinaSettings.First().LastTumblrPost = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        private async void _updateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await UpdateDatabase();
        }

        private async Task PostRandom()
        {
            await PostRandom(_client, _context);
        }

        private async Task UpdateDatabase()
        {
            DateTime? lastUpdate = DateTime.Now;
            try
            {
                lastUpdate = _updateContext.SabrinaSettings.First().LastTumblrUpdate;
            }
            catch(Exception ex)
            {

            }

            var minDate = DateTime.Now - TimeSpan.FromDays(3);
            if (lastUpdate != null && lastUpdate > minDate)
            {
                return;
            }

            int offset = 0;
            var totalPostCount = GetPostCount();

            while (offset < totalPostCount)
            {
                Console.WriteLine($"Indexing at Position {offset} from {totalPostCount}]");
                var posts = GetTumblrPostsByOffset(offset);

                foreach (var post in posts.Response.Posts)
                {
                    if (!_updateContext.TumblrPosts.Any(tPost => tPost.TumblrId == post.Id))
                    {
                        await _updateContext.TumblrPosts.AddAsync(new TumblrPosts()
                        {
                            TumblrId = post.Id,
                            IsLoli = -1,
                            LastPosted = null
                        });
                    }
                }

                offset += posts.Response.Posts.Length;
            }

            _updateContext.SabrinaSettings.First().LastTumblrUpdate = DateTime.Now;
            await _updateContext.SaveChangesAsync();
        }
    }
}