using Sabrina.Entities;

namespace Sabrina.Dungeon.Rooms
{
    internal abstract class TaskRoom : Room
    {
        public abstract string TextGreeting { get; protected set; }
        public abstract string TextMain { get; protected set; }
        public abstract string TextSuccess { get; protected set; }
        public abstract string TextFailure { get; protected set; }
        public virtual Content[] Content { get; protected set; }

        public TaskRoom(DungeonLogic.Dungeon.DungeonDifficulty difficulty) : base(RoomType.Task, difficulty)
        {
        }
    }
}