using DSharpPlus.Entities;

using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Sabrina.Entities
{
    internal class MessagePicture
    {
        private DiscordMessage msg;

        public MessagePicture(DiscordMessage msg, Graphics graphics, int imgWidth)
        {
            this.msg = msg;

            using (Font arialFont = new Font("Arial", 14))
            {
                var textSize = graphics.MeasureString(this.msg.Content, arialFont, imgWidth);

                this.Height = 20 + Convert.ToInt32(textSize.Height);

                if (this.Height < 100)
                {
                    this.Height = 100;
                }
            }
        }

        public int Height { get; private set; } = 50;

        public void AddToImg(Graphics graphics, float yAxis, int imgWidth)
        {
            int yInt = Convert.ToInt32(yAxis);

            string name = this.msg.Author.Username;
            string title = $"Origin: {this.msg.Channel.Name}";

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(this.msg.Author.AvatarUrl);

            var img = Image.FromStream(stream);

            graphics.DrawImage(img, new Point[] { new Point(0, yInt), new Point(100, yInt), new Point(0, yInt + 100) });

            using (Font titleFont = new Font("Arial", 18))
            {
                graphics.DrawString(this.msg.Author.Username, titleFont, Brushes.Cyan, new PointF(100, yInt));
            }

            using (Font arialFont = new Font("Arial", 14))
            {
                var textSize = graphics.MeasureString(this.msg.Content, arialFont, imgWidth);

                graphics.DrawString(this.msg.Content, arialFont, Brushes.White, new RectangleF(new PointF(100, yInt + 20), new SizeF(textSize.Width, textSize.Height)));
            }
        }
    }
}