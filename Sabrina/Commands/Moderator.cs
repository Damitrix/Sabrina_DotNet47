﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Sabrina.Bots;
using Sabrina.Entities;
using Sabrina.Entities.Persistent;

using DSharpPlus.Interactivity;

namespace Sabrina.Commands
{
    internal class Moderator
    {
        private const string ConfirmRegex = "\\b[Yy][Ee]?[Ss]?\\b|\\b[Nn][Oo]?\\b";
        private const string YesRegex = "[Yy][Ee]?[Ss]?";
        private const string NoRegex = "[Nn][Oo]?";

        [Command("movemsg"), Description("Moves a set amount of Messages from one Channel to another."), RequireRolesAttribute("minion", "techno kitty")]
        [Aliases(new[] { "shitpost" })]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task MoveMsgAsync(CommandContext ctx, [Description("The Channel to move to")] DiscordChannel channel, [Description("Amount of Messages to move")] int msg)
        {
            if (msg < 1)
            {
                return;
            }

            IReadOnlyList<DiscordMessage> messagesList = await ctx.Channel.GetMessagesAsync(limit: msg);
            List<DiscordMessage> messages = messagesList.OrderBy(m => m.Timestamp).ToList();

            int totalHeight = 0;

            List<MessagePicture> msgPicList = new List<MessagePicture>();

            int imgWidth = 600;

            using (Graphics calcGraphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                foreach (var cmsg in messages)
                {
                    var msgPic = new MessagePicture(cmsg, calcGraphics, imgWidth);
                    msgPicList.Add(msgPic);
                    totalHeight += msgPic.Height;
                }
            }

            Bitmap bmp = new Bitmap(imgWidth, totalHeight);

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(Color.FromArgb(54, 57, 63));
                graphics.SmoothingMode = SmoothingMode.HighSpeed;

                float cHeight = 0f;

                foreach (var msgPic in msgPicList)
                {
                    msgPic.AddToImg(graphics, cHeight, imgWidth);
                    cHeight += msgPic.Height;
                }
            }

            using (MemoryStream memory = new MemoryStream())
            {
                var qualityEncoder = Encoder.Quality;
                var quality = (long)80;
                var ratio = new EncoderParameter(qualityEncoder, quality);
                var codecParams = new EncoderParameters(1);
                codecParams.Param[0] = ratio;
                var jpegCodecInfo = ImageCodecInfo.GetImageEncoders().First(e => e.MimeType == "image/jpeg");
                bmp.Save(memory, jpegCodecInfo, codecParams); // Save to JPG

                memory.Position = 0;

                await channel.SendFileAsync(memory, "ChatArchive.jpeg");
            }

            await ctx.Channel.DeleteMessagesAsync(messagesList);
            await ctx.RespondAsync($"I moved some Stuff to {channel.Mention}");
        }

        [Command("purge"), Description("Removes X Messages"), RequireRolesAttribute("mistress", "minion", "techno kitty")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task PurgeMessages(CommandContext ctx, [Description("Amount of Messages to move")] int msgAmount)
        {
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(msgAmount));
        }

        [Command("crash"), Description("Makes the bot crash")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task CrashTask(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivityModule();
            await ctx.RespondAsync("Are you sure?");
            var m = await interactivity.WaitForMessageAsync(
                x => x.Channel.Id == ctx.Channel.Id
                     && x.Author.Id == ctx.Member.Id
                     && Regex.IsMatch(x.Content, ConfirmRegex), TimeSpan.FromSeconds(60));

            string[] possibleQuestions = new[]
            {
                "Are you really sure?", "Are you absolutely sure?", "Are you really really sure?",
                "Are you reeeeaaaallly sure?", "Are you certain?", "Really?", "Last Chance"
            };

            while (m != null && Regex.IsMatch(m.Message.Content, YesRegex))
            {
                await ctx.RespondAsync(possibleQuestions[Helpers.RandomGenerator.RandomInt(0, possibleQuestions.Length)]);
                m = await interactivity.WaitForMessageAsync(
                    x => x.Channel.Id == ctx.Channel.Id
                         && x.Author.Id == ctx.Member.Id
                         && Regex.IsMatch(x.Content, ConfirmRegex), TimeSpan.FromSeconds(60));
            }

            await ctx.RespondAsync("*wipes sweat off*");
        }
    }
}