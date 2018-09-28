using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sabrina.Entities;

namespace Sabrina.Dungeon.Rooms.Task
{
    class SimpleEdgeTask : TaskRoom
    {
        public SimpleEdgeTask(DungeonLogic.Dungeon.DungeonDifficulty difficulty) : base(difficulty)
        {

        }

        public override Content Content { get; set;}
        public override Room[] AdjacentRooms { get;set;}
        public override int Chance {get;set;}
    }
}
