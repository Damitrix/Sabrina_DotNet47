using System;

using Sabrina.Entities;

namespace Sabrina.Dungeon.Rooms.Task.LesserMob
{
    internal class SimpleEdgeTask : LesserMob
    {
        public override string TextGreeting { get; protected set; }
        public override string TextMain { get; protected set; }
        public override string TextSuccess { get; protected set; }
        public override string TextFailure { get; protected set; }
        public override Content[] Content { get; protected set; }
        public override Room[] AdjacentRooms { get; set; }
        public override int Chance { get => throw new NotImplementedException(); protected set => throw new NotImplementedException(); }

        public SimpleEdgeTask(DungeonLogic.Dungeon.DungeonDifficulty difficulty) : base(difficulty)
        {
        }
    }
}