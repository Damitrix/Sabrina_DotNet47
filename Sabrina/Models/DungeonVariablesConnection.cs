using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class DungeonVariablesConnection
    {
        public int Id { get; set; }
        public int TextId { get; set; }
        public int VariableId { get; set; }

        public DungeonText Variable { get; set; }
    }
}
