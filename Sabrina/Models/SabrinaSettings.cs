using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SabrinaSettings
    {
        public long GuildId { get; set; }
        public DateTime? LastTumblrPost { get; set; }
        public DateTime? LastIntroductionPost { get; set; }
        public DateTime? LastTumblrUpdate { get; set; }
    }
}
