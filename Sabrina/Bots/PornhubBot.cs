using Configuration;

namespace Sabrina.Pornhub
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    using DSharpPlus;
    using DSharpPlus.Entities;

    using HtmlAgilityPack;

    using Sabrina.Entities;

    public class PornhubBot
    {
        public static bool Exit = false;

        public List<Video> IndexedVideos = new List<Video>();

        private readonly DiscordClient client;

        public PornhubBot(DiscordClient client)
        {
            this.client = client;
            Task.Run(this.MainThread);
        }

        private async Task MainThread()
        {
            this.IndexedVideos = (await Video.LoadAll()).ToList();

            while (!Exit)
            {
                foreach (string channel in Config.Pornhub.Channels)
                {
                    var thrownError = false;

                    do
                    {
                        try
                        {
                            string url = $"http://www.pornhub.com/users/{channel}/videos";
                            var web = new HtmlWeb();
                            var doc = web.Load(url);
                            var node = doc.DocumentNode.SelectSingleNode(
                                "//*[@class=\"videos row-3-thumbs\"]/li[1]/div/div[1]/div/a");
                            if (node == null)
                            {
                                continue;
                            }

                            string videoLink = node.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(videoLink))
                            {
                                url = $"http://www.pornhub.com{videoLink}";
                                web = new HtmlWeb();
                                doc = web.Load(url);

                                var titleNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:title").FirstOrDefault();
                                var imageNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:image").FirstOrDefault();

                                if (titleNode == null || imageNode == null)
                                {
                                    await Task.Delay(5000);
                                    doc = web.Load(url);
                                    await Task.Delay(5000);
                                    titleNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:title").FirstOrDefault();
                                    imageNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:image").FirstOrDefault();
                                }

                                if (titleNode == null || imageNode == null)
                                {
                                    thrownError = true;
                                    await Task.Delay(10000);
                                    continue;
                                }

                                var video = new Video
                                                {
                                                    Url = url,
                                                    Creator = channel,
                                                    CreationDate = DateTime.Now,
                                                    ImageUrl = imageNode.GetAttributeValue("content", string.Empty),
                                                    Title = titleNode.GetAttributeValue("content", string.Empty)
                                                };

                                var containsVideo = false;

                                foreach (var cVideo in this.IndexedVideos)
                                    if (cVideo.Url == video.Url)
                                    {
                                        containsVideo = true;
                                        break;
                                    }

                                if (!containsVideo)
                                {
                                    this.IndexedVideos.Add(video);
                                    await video.Save();

                                    var builder = new DiscordEmbedBuilder
                                                      {
                                                          Title = $"{video.Creator} has Uploaded a new Video!",
                                                          Timestamp = DateTime.Now,
                                                          Url = video.Url
                                                      };
                                    builder.AddField("Title", video.Title);
                                    builder.ThumbnailUrl = video.ImageUrl;
                                    builder.AddField("CreationDate", video.CreationDate.ToLongDateString());

                                    foreach (ulong dcChannel in Config.Pornhub.ChannelsToPostTo)
                                        await (await this.client.GetChannelAsync(dcChannel)).SendMessageAsync(
                                            embed: builder.Build());
                                }
                            }

                            thrownError = false;
                            await Task.Delay(10000);
                        }
                        catch
                        {
                            thrownError = true;
                            Console.WriteLine("Error in Pornhub Module. Waiting 10s");
                            await Task.Delay(10000);
                        }

                        await Task.Delay(20000);
                    }
                    while (thrownError);

                    await Task.Delay(50000);
                }

                await Task.Delay(60000);
            }
        }
    }

    public class Video
    {
        public DateTime CreationDate;

        public string Creator;

        public string ImageUrl;

        public string Title;

        public string Url;

        public static async Task<Video[]> LoadAll()
        {
            var videos = new List<Video>();

            using (var conn = new SqlConnection(Config.DataBaseConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand();

                cmd = new SqlCommand("SELECT ID, URL, Creator, Date, ImageUrl, Title FROM PornhubVideos", conn);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                    videos.Add(
                        new Video
                            {
                                CreationDate = DateTime.Parse(reader["Date"].ToString()),
                                Creator = reader["Creator"].ToString(),
                                ImageUrl = reader["ImageUrl"].ToString(),
                                Title = reader["Title"].ToString(),
                                Url = reader["URL"].ToString()
                            });
            }

            return videos.ToArray();
        }

        public async Task Save()
        {
            using (var conn = new SqlConnection(Config.DataBaseConnectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand();

                cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT * FROM PornhubVideos WHERE URL = @Url)"
                    + "  INSERT INTO PornhubVideos (Date, Creator, ImageUrl, Title, URL) VALUES (@Date, @Creator, @ImageUrl, @Title, @Url)",
                    conn);

                cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = this.CreationDate;
                cmd.Parameters.Add("@Creator", SqlDbType.NVarChar).Value = this.Creator;
                cmd.Parameters.Add("@ImageUrl", SqlDbType.NVarChar).Value = this.ImageUrl;
                cmd.Parameters.Add("@Title", SqlDbType.NVarChar).Value = this.Title;
                cmd.Parameters.Add("@Url", SqlDbType.NVarChar).Value = this.Url;

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}