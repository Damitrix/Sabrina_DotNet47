using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class IndexedVideo
    {
        public int Id { get; set; }
        public int PlatformId { get; set; }
        public int CreatorId { get; set; }
        public string Link { get; set; }
        public DateTime? CreationDate { get; set; }
        public string Identification { get; set; }

        public Creator Creator { get; set; }
        public Joiplatform Platform { get; set; }
    }
}
