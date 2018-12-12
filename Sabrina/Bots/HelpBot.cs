using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Configuration;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Sabrina.Models;

namespace Sabrina.Bots
{
    internal class HelpBot
    {
        private readonly DiscordClient _client;
        private readonly DiscordContext _context;

        private Timer _helpPostTimer;

        public HelpBot(DiscordClient client)
        {
            _context = new DiscordContext();
            _client = client;

            SetTimer();
        }

        private async Task OnTimerElapse()
        {
            foreach (var channelId in _context.SabrinaSettings.Select(s => s.WheelChannel))
            {
                if (channelId != null)
                {
                    var channel = await _client.GetChannelAsync(Convert.ToUInt64(channelId));

                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            IconUrl =
                        "https://cdn.discordapp.com/avatars/450771319479599114/d7dd3e2ec296542f170e84c264de78ce.png",
                            Name = "Sabrina"
                        },
                        Color = DiscordColor.VeryDarkGray,
                        Description =
                    "Hey Guys and Gals, i'm Sabrina. Since Mistress can't tend to every single one of your pathetic little needs, i'm here to help her out." +
                    Environment.NewLine +
                    "I've got a bunch of neat little Commands to ~~torture~~ help you. You'll probably only ever need 3 though." +
                    Environment.NewLine + Environment.NewLine +
                    "``//orgasmwheel``" + Environment.NewLine +
                    "Use this, to spin the \"Wheel of Misfortune\". It contains fun little Tasks and \"Rewards\", that Mistress Aki herself has created. " +
                    "(That means, if you're unhappy with your outcome, you know where to complain.... if you dare to.)" +
                    Environment.NewLine + Environment.NewLine +
                    "``//denialtime``" + Environment.NewLine +
                    "This will show you, when exactly " + Environment.NewLine +
                    "    a) You are able to spin again" + Environment.NewLine +
                    "    b) You are not denied anymore" + Environment.NewLine +
                    "Which means, that you may spin the wheel while denied. But that also means, that you can also not be denied, while being excluded from the wheel." +
                    Environment.NewLine + Environment.NewLine +
                    "``//settings setup``" +
                    Environment.NewLine +
                    "When you issue this command, i will assist you with setting up the difficulty of the wheel and other stuffs. Just wait for my dm.",
                        Title = "Introduction",
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            IconUrl = "https://cdn.discordapp.com/avatars/249216025931939841/a_94cf2ac609424257706d6a611f5dd7aa.gif",
                            Text = "If something doesn't seem right, please complain to Salem :)"
                        }
                    };

                    await channel.SendMessageAsync(embed: builder.Build());

                    (await _context.SabrinaSettings.FirstAsync()).LastWheelHelpPost = DateTime.Now;
                    await _context.SaveChangesAsync();
                    SetTimer();
                }
            }
        }

        private void SetTimer()
        {
            DateTime? lastHelpPost = _context.SabrinaSettings.First().LastWheelHelpPost;
            TimeSpan nextPost = new TimeSpan();

            if (lastHelpPost != null)
            {
                nextPost = (TimeSpan.FromDays(1) - (DateTime.Now - lastHelpPost)).Value;
            }
            else
            {
                nextPost = TimeSpan.FromMilliseconds(1);
            }

            _helpPostTimer = new Timer
            {
                AutoReset = false,
                Enabled = true,
                Interval = nextPost.TotalMilliseconds
            };
            _helpPostTimer.Elapsed += async (sender, args) => await OnTimerElapse();
            _helpPostTimer.Start();
        }
    }
}