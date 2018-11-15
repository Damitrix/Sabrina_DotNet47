﻿using Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Sabrina.Entities.Persistent
{
    public class Link
    {
        public string CreatorName;
        public string FileName;
        public ContentType Type;
        public string Url;

        public enum ContentType
        {
            Video,
            Picture
        }

        public static async Task<List<Link>> LoadAll()
        {
            var allLinks = new List<Link>();
            foreach (string file in Directory.GetFiles(Config.BotFileFolders.WheelLinks))
            {
                allLinks.Add(await Load(file));
            }

            return allLinks;
        }

        public void Delete()
        {
            string fileLocation = $"{Config.BotFileFolders.WheelLinks}/{this.FileName}.xml";
            File.Delete(fileLocation);
        }

        public void Save()
        {
            var fileId = 0;

            string fileLocation = string.Empty;

            do
            {
                fileLocation = $"{Config.BotFileFolders.WheelLinks}/{fileId}.xml";
                fileId++;
            }
            while (File.Exists(fileLocation));

            XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Link) })[0];

            using (var stream = File.Create(fileLocation))
            {
                xmlSerializer.Serialize(stream, this);
            }
        }

        private static Link DeserializeLink(string fileLocation)
        {
            using (var reader = File.OpenRead(fileLocation))
            {
                using (XmlReader xmlReader = XmlReader.Create(reader))
                {
                    XmlSerializer xmlSerializer = XmlSerializer.FromTypes(new[] { typeof(Link) })[0];
                    Link link = (Link)xmlSerializer.Deserialize(xmlReader);
                    link.FileName = Path.GetFileNameWithoutExtension(fileLocation);
                    return link;
                }
            }
        }

        private static async Task<Link> Load(string fileLocation)
        {
            return await Task.Run(() => DeserializeLink(fileLocation));
        }
    }
}