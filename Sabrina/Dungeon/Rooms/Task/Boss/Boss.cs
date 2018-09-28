using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Dungeon.Rooms.Task.Boss
{
    internal class Boss : TaskRoom
    {
        public override string TextGreeting { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override string TextMain { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override string TextSuccess { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override string TextFailure { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }
        public override Room[] AdjacentRooms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int Chance { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public Boss(DungeonLogic.Dungeon.DungeonDifficulty difficulty) : base(difficulty)
        {
        }
    }
}