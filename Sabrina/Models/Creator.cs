using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class Creator
    {
        public Creator()
        {
            CreatorPlatformLink = new HashSet<CreatorPlatformLink>();
            IndexedVideo = new HashSet<IndexedVideo>();
        }

        public int Id { get; set; }
        public long? DiscordUserId { get; set; }
        public string Name { get; set; }

        public ICollection<CreatorPlatformLink> CreatorPlatformLink { get; set; }
        public ICollection<IndexedVideo> IndexedVideo { get; set; }
    }
}
