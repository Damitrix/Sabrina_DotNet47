using DSharpPlus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Sabrina.Models;
using System.IO;
using HtmlAgilityPack;
using System.Net.Cache;
using System.Security.Policy;
using System.Web;
using Configuration;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Sabrina.Entities;
using SkiaSharp;
using Convert = System.Convert;
using Microsoft.ML;
using Microsoft.ML.Runtime.Api;
using Microsoft.ML.Legacy.Data;
using Microsoft.ML.Runtime;
using Microsoft.ML.Runtime.Data;
using Microsoft.ML.Trainers.FastTree.Internal;
using DataKind = Microsoft.ML.Runtime.Data.DataKind;
using Timer = System.Timers.Timer;

namespace Sabrina.Bots
{
    class SankakuBot
    {
        private const string _domain = "chan.sankakucomplex.com";
        private const string _baseUrl = "https://" + _domain;
        private const string _postBaseUrl = _baseUrl + "/post/show/";
        private DiscordClient _client;
        private Timer _scrapeTimer;
        private Timer _postTimer;

        public SankakuBot(DiscordClient client)
        {
            _client = client;
            Task.Run(InitializeAsync);
        }

        public async Task InitializeAsync()
        {
            if (Config.SankakuLogin == null || Config.SankakuPassword == null)
            {
                Console.WriteLine("No Login info for Sankaku provided. Skipping Initialization of SankakuBot.");
                return;
            }

            _client.MessageReactionAdded += Client_MessageReactionAdded;
            _client.MessageReactionRemoved += Client_MessageReactionRemoved;

            var scrapingTask = Task.Run(async () => await ScrapeNewest());

            _scrapeTimer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds)
            {
                AutoReset = true
            };

            _scrapeTimer.Elapsed += async (object o, ElapsedEventArgs e) => await ScrapeNewest();

            _postTimer = new Timer(TimeSpan.FromMinutes(30).TotalMilliseconds)
            {
                AutoReset = true
            };
            _postTimer.Elapsed += async (object sender, ElapsedEventArgs e) => await _postTimer_Elapsed();

            _postTimer.Start();

            DiscordContext context = new DiscordContext();

            if (!context.SankakuPost.Any() || context.SankakuPost.OrderByDescending(sp => sp.Date).First().Date < DateTime.Now - TimeSpan.FromMinutes(20))
            {
                await _postTimer_Elapsed();
            }
        }

        private async Task Client_MessageReactionRemoved(DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
        {
            var context = new DiscordContext();

            var message = await context.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

            if (message != null)
            {
                int VoteValue = 0;

                string discordName = e.Emoji.GetDiscordName();

                if (Config.Emojis.Confirms.Contains(discordName))
                {
                    VoteValue = 1;
                }
                else if (Config.Emojis.Love.Contains(discordName))
                {
                    VoteValue = 3;
                }
                else if (Config.Emojis.Declines.Contains(discordName))
                {
                    VoteValue = -1;
                }
                else if (Config.Emojis.Hate.Contains(discordName))
                {
                    VoteValue = -3;
                }

                var vote = await context.SankakuImageVote.FirstOrDefaultAsync(sankakuVote => sankakuVote.ImageId == message.ImageId &&
                                                                                             sankakuVote.UserId == Convert.ToInt64(e.User.Id) &&
                                                                                             sankakuVote.VoteValue == VoteValue);

                if (vote != null)
                {
                    context.SankakuImageVote.Remove(vote);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task Client_MessageReactionAdded(DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
        {
            var context = new DiscordContext();

            var message = await context.SankakuPost.FirstOrDefaultAsync(post => post.MessageId == Convert.ToInt64(e.Message.Id));

            if (message != null)
            {
                SankakuImageVote vote = null;

                vote = new SankakuImageVote()
                {
                    ImageId = message.ImageId,
                    UserId = Convert.ToInt64(e.User.Id),
                    VoteValue = 0
                };

                string discordName = e.Emoji.GetDiscordName();

                if (Config.Emojis.Confirms.Contains(discordName))
                {
                    vote.VoteValue = 1;
                }
                else if (Config.Emojis.Love.Contains(discordName))
                {
                    vote.VoteValue = 3;
                }
                else if (Config.Emojis.Declines.Contains(discordName))
                {
                    vote.VoteValue = -1;
                }
                else if (Config.Emojis.Hate.Contains(discordName))
                {
                    vote.VoteValue = -3;
                }

                if (vote.VoteValue != 0)
                {
                    await context.SankakuImageVote.AddAsync(vote);
                }

                await context.SaveChangesAsync();
            }
        }

        private async Task _postTimer_Elapsed()
        {
            DiscordContext context = new DiscordContext();

            foreach (var channelId in context.SabrinaSettings.Select(ss => ss.FeetChannel).Where(cId => cId != null))
            {
                var channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

                try
                {
                    await Task.Run(async () => await PostRandom(channel));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error in Sankakubot PostRandom");
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task ScrapeNewest()
        {
            DiscordContext context = new DiscordContext();

            HttpWebRequest request =
                Helpers.CreateRequestWithHeaders(_baseUrl + "/post/index.content?tags=feet+order%3Arandom+-contentious_content+-furry+-rating%3As&page=3");

            if (request.CookieContainer == null)
            {
                request.CookieContainer = new CookieContainer();
            }

            request.CookieContainer.Add(new Cookie("login", Config.SankakuLogin, "/", _domain));
            request.CookieContainer.Add(new Cookie("pass_hash", Config.SankakuPassword, "/", _domain));

            var data = (await request.GetResponseAsync()).GetResponseStream();
            var doc = new HtmlDocument();
            doc.Load(data);

            var postNodes = doc.DocumentNode.Descendants("span");

            ConcurrentBag<SankakuImage> sankakuImages = new ConcurrentBag<SankakuImage>();

            Parallel.ForEach(postNodes, async postNode =>
            {
                var localContext = new DiscordContext();

                var cPost = new SankakuPost(postNode);

                cPost.GetScoreCount();

                if ((await localContext.SankakuImage.FindAsync(cPost.Id)) != null)
                {
                    return;
                }

                var sankakuImage = new SankakuImage();

                sankakuImage.Id = cPost.Id;
                sankakuImage.Score = cPost.Score;
                sankakuImage.Rating = (int) cPost.imgRating;
                sankakuImage.RatingCount = cPost.ScoreCount != null ? cPost.ScoreCount.Value : 0;

                foreach (var tag in cPost.Tags)
                {
                    var dbTag = localContext.SankakuTag.FirstOrDefault(sTag => sTag.Name == tag);
                    var imgTag = new SankakuImageTag();

                    if (dbTag == null)
                    {
                        dbTag = new SankakuTag() {Name = tag};
                        imgTag.Tag = dbTag;
                    }
                    else
                    {
                        imgTag.TagId = dbTag.Id;
                    }

                    sankakuImage.SankakuImageTag.Add(imgTag);
                }

                sankakuImages.Add(sankakuImage);
            });

            await context.SankakuImage.AddRangeAsync(sankakuImages);
            await context.SaveChangesAsync();
        }

        public static async Task PostRandom(DiscordChannel channel, int skip = 0)
        {
            var context = new DiscordContext();

            SankakuImage imgToPost = null;

            Random rnd = new Random(skip);

            var channelIDLong = Convert.ToInt64(channel.Id);

            var blacklistedTags =
                await context.SankakuTagBlacklist.Where(blackTag => blackTag.ChannelId == channelIDLong).Select(tag => tag.TagId).ToListAsync();

            var startDate = DateTime.Now - TimeSpan.FromDays(90);

            var viableImages = context.SankakuImage.Where(si => si.SankakuPost.Count == 0 && si.Score > 4 && si.RatingCount > 6);

            while (imgToPost == null)
            {
                var firstNotPostedPicture = viableImages.Where(si => !si.SankakuImageTag.Any(tag => blacklistedTags.Contains(tag.TagId)))
                    .Skip(skip).First();

                if (firstNotPostedPicture == null)
                {
                    var oldestPostedPicture = viableImages.Where(si =>
                        si.SankakuPost.OrderByDescending(post => post.Date).First().Date < startDate)
                        .Skip(skip);
                }
                else
                {
                    imgToPost = firstNotPostedPicture;
                }

                if (imgToPost == null)
                {
                    await ScrapeNewest();
                }
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.SpringGreen,
                Title = context.Puns.Skip(rnd.Next(context.Puns.Count() - 1)).First().Text
            };

            var link = HttpUtility.HtmlDecode(await GetOriginalImageLink(imgToPost.Id));

            var request = Helpers.CreateRequestWithHeaders(link);

            var response = request.GetResponse();

            SKImage img = SKImage.FromEncodedData(response.GetResponseStream());

            if (img.ColorType == SKColorType.Bgra8888)
            {
                //Bgra8888 needs an indexed Colortable, so convert it to Rgba8888 first
                var bitmap = new SKBitmap(new SKImageInfo(img.Width, img.Height, SKColorType.Rgba8888));
                var canvas = new SKCanvas(bitmap);
                canvas.DrawImage(img,0,0, null);
                canvas.Flush();
                img = SKImage.FromBitmap(bitmap);
            }

            var imageData = img.Encode(SKEncodedImageFormat.Webp, 70);

            using (var outStream = imageData.AsStream())
            {
                var msg = await channel.SendFileAsync(outStream, builder.Title + ".webp",
                    embed: builder.Build());

                context.SankakuPost.Add(new Models.SankakuPost()
                {
                    Date = DateTime.Now,
                    Image = imgToPost,
                    MessageId = Convert.ToInt64(msg.Id)
                });

                await context.SaveChangesAsync();
            }
        }

        private static async Task<string> GetOriginalImageLink(long id)
        {
            HttpWebRequest request = Helpers.CreateRequestWithHeaders(_postBaseUrl + id);

            var data = (await request.GetResponseAsync()).GetResponseStream();
            var doc = new HtmlDocument();
            doc.Load(data);

            var highResNode = doc.DocumentNode.Descendants("a")
                .First(node => node.Attributes["id"] != null && node.Attributes["id"].Value == "highres");

            return "https:" + highResNode.Attributes["href"].Value;
        }

        public static async Task PostPrediction(long userId, DiscordChannel channel)
        {
            var context = new DiscordContext();

            SankakuImage imgToPost = null;

            Random rnd = new Random();

            var channelIDLong = Convert.ToInt64(channel.Id);

            var blacklistedTags =
                await context.SankakuTagBlacklist.Where(blackTag => blackTag.ChannelId == channelIDLong).Select(tag => tag.TagId).ToListAsync();

            var startDate = DateTime.Now - TimeSpan.FromDays(90);

            var trainedModel = Train(userId);

            int skip = 0;

            while (imgToPost == null)
            {
                //imgToPost = context.SankakuImage.Where(si => si.SankakuPost.Count == 0).Skip(skip).First();

                imgToPost = context.SankakuImage.Find(88415l);

                if (imgToPost == null)
                {
                    await ScrapeNewest();
                }

                List<MLSankakuPost> posts = new List<MLSankakuPost>();

                MLSankakuPost mlPost = new MLSankakuPost()
                {
                    //imgRating = imgToPost.Rating,
                    //score = Convert.ToSingle(imgToPost.Score),
                    //Tags = context.SankakuImageTag.Where(imageTag => imageTag.ImageId == imgToPost.Id).Select(tag => Convert.ToSingle(tag.TagId)).ToArray(),
                    Tags = 1,
                };

                posts.Add(mlPost);

                var prediction = trainedModel.Predict(posts, true);

                if (prediction.First().Score < 1)
                {
                    skip++;
                    imgToPost = null;
                }
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Orange,
                Title = context.Puns.Skip(new Random().Next(context.Puns.Count() - 1)).First().Text
            };

            var link = HttpUtility.HtmlDecode(await GetOriginalImageLink(imgToPost.Id));

            var request = Helpers.CreateRequestWithHeaders(link);

            var response = request.GetResponse();

            SKImage img = SKImage.FromEncodedData(response.GetResponseStream());

            if (img.ColorType == SKColorType.Bgra8888)
            {
                //Bgra8888 needs an indexed Colortable, so convert it to Rgba8888 first
                var bitmap = new SKBitmap(new SKImageInfo(img.Width, img.Height, SKColorType.Rgba8888));
                var canvas = new SKCanvas(bitmap);
                canvas.DrawImage(img, 0, 0, null);
                canvas.Flush();
                img = SKImage.FromBitmap(bitmap);
            }

            var imageData = img.Encode(SKEncodedImageFormat.Jpeg, 75); //Encode in Jpeg instead of Webp because of IOS

            using (var outStream = imageData.AsStream())
            {
                var msg = await channel.SendFileAsync(outStream, builder.Title + ".jpeg",
                    embed: builder.Build());

                context.SankakuPost.Add(new Models.SankakuPost()
                {
                    Date = DateTime.Now,
                    Image = imgToPost,
                    MessageId = Convert.ToInt64(msg.Id)
                });

                await context.SaveChangesAsync();
            }
        }

        private static BatchPredictionEngine<MLSankakuPost, MLSankakuPostLikeagePrediciton> Train(long userId)
        {
            var context = new DiscordContext();

            MLContext mlContext = new MLContext(seed: 0);
            
            var data = GetPosts(userId);

            var schemaDef = SchemaDefinition.Create(typeof(MLSankakuPost));
            
            var trainData = mlContext.CreateStreamingDataView<MLSankakuPost>(data, schemaDef);

            var pipeline = mlContext.Regression.Trainers.FastTree("Label", "Features", numLeaves: 50, numTrees: 50, minDatapointsInLeaves: 20);
            
            var model = pipeline.Fit(trainData);

            return mlContext.CreateBatchPredictionEngine<MLSankakuPost, MLSankakuPostLikeagePrediciton>(trainData, true,
                schemaDef);

            //return model.MakePredictionFunction<MLSankakuPost, MLSankakuPost>(mlContext);
        }

        private static IEnumerable<MLSankakuPost> GetPosts(long userId)
        {
            var outPosts = new List<MLSankakuPost>();
            var context = new DiscordContext();

            var allTags = context.SankakuTag.ToArray();

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 0,
                UserScore = 0
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 1,
                UserScore = 1
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 1,
                UserScore = 1
            });

            outPosts.Add(new MLSankakuPost()
            {
                Tags = 0,
                UserScore = 0
            });

            return outPosts;

            foreach (var vote in context.SankakuImageVote.Where(indVote => indVote.UserId == userId))
            {
                MLSankakuPost post = new MLSankakuPost();

                var tags = context.SankakuImageTag.Where(imageTag => imageTag.ImageId == vote.ImageId)
                    .Select(tag => tag.TagId).ToArray();

                var image = context.SankakuImage.Find(vote.ImageId);

                //post.Tags = new float[allTags.Length];
                int cIndex = 0;

                foreach (var generalTag in allTags)
                {
                    if (tags.Contains(generalTag.Id))
                    {
                        //post.Tags[cIndex] = 1;
                    }
                    else
                    {
                        //post.Tags[cIndex] = 0;
                    }
                }
                
                post.UserScore = vote.VoteValue;
                post.imgRating = vote.Image.Rating;
                post.score = Convert.ToSingle(image.Score);


                outPosts.Add(post);
            }

            return outPosts;
        }

        public class MLSankakuPost
        {
            public int Tags;
            public float score;
            public float imgRating;
            [ColumnName("Label")]
            public float UserScore;

            public MLSankakuPost()
            {

            }
        }

        public class MLSankakuPostLikeagePrediciton
        {
            [ColumnName("Score")]
            public float Score;
        }
            

        private class SankakuPost
        {
            public string[] Tags;
            public long Id;
            public float Score;
            public int? ScoreCount;
            public Rating imgRating;

            public void GetScoreCount()
            {
                var request = Helpers.CreateRequestWithHeaders(_postBaseUrl + Id);
                var response = request.GetResponse();

                var data = response.GetResponseStream();
                var doc = new HtmlDocument();
                doc.Load(data);

                var node = doc.DocumentNode.Descendants("span").FirstOrDefault(spanNode => spanNode.Id == "post-vote-count-" + Id);

                if (node != null && !String.IsNullOrEmpty(node.InnerText))
                {
                    if (int.TryParse(node.InnerText, out int scoreCount))
                    {
                        ScoreCount = scoreCount;
                    }
                }
            }

            public SankakuPost(HtmlNode span)
            {
                Id = int.Parse(span.Attributes["id"].Value.Substring(1, span.Attributes["id"].Value.Length - 1));
                var imgNode = span.Descendants("img").First();
                var title = imgNode.Attributes["title"].Value;
                Tags = title.Split(' ');

                var information = Tags.Where(tag => tag.Contains(':'));
                foreach (var info in information)
                {
                    if (info.ToLower().StartsWith("score"))
                    {
                        Score = float.Parse(info.Split(':')[1]);
                    }
                    else if (info.ToLower().StartsWith("rating"))
                    {
                        Enum.TryParse(info.Split(':')[1], out imgRating);
                    }
                }

                Tags = Tags.Where(tag => !tag.Contains(':')).ToArray();

            }

            public enum Rating
            {
                Safe,
                Questionable,
                Explicit
            }
        }
    }
}
