using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class DungeonText
    {
        public DungeonText()
        {
            DungeonVariablesConnection = new HashSet<DungeonVariablesConnection>();
        }

        public int Id { get; set; }
        public string Text { get; set; }
        public string RoomType { get; set; }
        public string TextType { get; set; }

        public ICollection<DungeonVariablesConnection> DungeonVariablesConnection { get; set; }
    }
}
