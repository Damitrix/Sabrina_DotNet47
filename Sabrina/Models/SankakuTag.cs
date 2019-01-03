﻿using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class SankakuTag
    {
        public SankakuTag()
        {
            SankakuImageTag = new HashSet<SankakuImageTag>();
            SankakuTagBlacklist = new HashSet<SankakuTagBlacklist>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<SankakuImageTag> SankakuImageTag { get; set; }
        public virtual ICollection<SankakuTagBlacklist> SankakuTagBlacklist { get; set; }
    }
}
