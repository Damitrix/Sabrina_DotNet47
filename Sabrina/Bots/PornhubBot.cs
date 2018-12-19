using System.Net;
using System.Net.Cache;
using Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Sabrina.Models;

namespace Sabrina.Pornhub
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using HtmlAgilityPack;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

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
            while (!Exit)
            {
                var context = new DiscordContext();
                foreach (var platform in context.Joiplatform)
                {
                    foreach (var link in context.CreatorPlatformLink)
                    {
                        Video newestVideo = null;

                        switch (platform.BaseUrl)
                        {
                                case "https://www.pornhub.com":
                                    newestVideo = await GetNewestPornhubVideo(link.Identification);
                                    break;
                        }

                        if (context.IndexedVideo.Any(iv => iv.Identification == newestVideo.ID))
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        var creator = await context.Creator.FindAsync(link.CreatorId);
                        var discordUser = client.GetUserAsync(Convert.ToUInt64(creator.DiscordUserId.Value));

                        IndexedVideo indexedVideo = new IndexedVideo()
                        {
                            CreationDate = DateTime.Now,
                            CreatorId = creator.Id,
                            Identification = newestVideo.ID,
                            Link = newestVideo.Url,
                            PlatformId = platform.Id
                        };

                        await context.IndexedVideo.AddAsync(indexedVideo);

                        DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Gold,
                            Title = $"{newestVideo.Creator} has uploaded a new video!",
                            Url = newestVideo.Url,
                            ThumbnailUrl = newestVideo.ImageUrl
                        };

                        builder.AddField("Title", newestVideo.Title);
                        if (creator.DiscordUserId != null)
                        {
                            builder.AddField("Creator", (await discordUser).Mention);
                        }

                        var embed = builder.Build();

                        await context.SaveChangesAsync();

                        foreach (var updateChannelId in context.SabrinaSettings.Where(ss => ss.ContentChannel != null).Select(ss => ss.ContentChannel))
                        {
                            var updateChannel = await client.GetChannelAsync(Convert.ToUInt64(updateChannelId));

                            await client.SendMessageAsync(updateChannel, embed: embed);
                        }
                    }
                }
                await Task.Delay(120000);
            }
        }

        private async Task<Video> GetNewestPornhubVideo(string userName)
        {
            Video newestVideo = null;

            try
            {
                string url = $"https://www.pornhub.com/users/{userName}/videos";
                var request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.CookieContainer = new CookieContainer();
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
                request.Accept =
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.Method = "GET";
                request.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36";

                var data = (await request.GetResponseAsync()).GetResponseStream();
                var doc = new HtmlDocument();
                doc.Load(data);

                var node = doc.DocumentNode.SelectSingleNode(
                    "//*[@class=\"videos row-3-thumbs\"]/li[1]/div/div[1]/div/a");
                if (node == null)
                {
                    return null;
                }

                string id = node.GetAttributeValue("href", string.Empty).Split(new string[] {"?viewkey="}, StringSplitOptions.RemoveEmptyEntries)[1];
                newestVideo = await Video.FromPornhubId(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return newestVideo;
        }
    }

    public class Video
    {
        public DateTime CreationDate;

        public string Creator;

        public string ImageUrl;

        public string Title;

        public string Url;

        public string ID;

        public static async Task<Video> FromPornhubId(string id)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create($"https://www.pornhub.com/view_video.php?viewkey={id}");
            request.CookieContainer = new CookieContainer();
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.Default);
            request.Accept =
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            request.Method = "GET";
            request.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36";

            var data = (await request.GetResponseAsync()).GetResponseStream();
            var doc = new HtmlDocument();
            doc.Load(data);

            var titleNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:title").FirstOrDefault();
            var imageNode = doc.DocumentNode.SelectNodes("/html/head/meta").Where(e => e.Attributes["property"]?.Value == "og:image").FirstOrDefault();
            var test = doc.DocumentNode.Descendants("div")
                .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First();
            var userName = doc.DocumentNode.Descendants("div")
                .Where(d => d.GetAttributeValue("class", "") == "video-detailed-info").First().Descendants("a").First()
                .InnerText;

            if (titleNode == null || imageNode == null)
            {
                return null;
            }

            var video = new Video
            {
                Url = $"https://www.pornhub.com/view_video.php?viewkey={id}",
                Creator = userName,
                CreationDate = DateTime.Now,
                ImageUrl = imageNode.GetAttributeValue("content", string.Empty),
                Title = titleNode.GetAttributeValue("content", string.Empty),
                ID = id
            };

            return video;
        }
    }
}