using System;
using System.Collections.Generic;
using System.Linq;

using Sabrina.Dungeon.Rooms.Task;
using Sabrina.Entities;

namespace Sabrina.Dungeon.Rooms
{
    using Sabrina.Dungeon.Rooms.Task.Boss;
    using Sabrina.Dungeon.Rooms.Task.LesserMob;

    public abstract class Room
    {
        public abstract Room[] AdjacentRooms { get; set; }
        public abstract int Chance { get; protected set; }

        public Room(RoomType type, DungeonLogic.Dungeon.DungeonDifficulty difficulty)
        {
        }

        public static Room GenerateRoom(RoomType type, DungeonLogic.Dungeon.DungeonDifficulty difficulty)
        {
            List<Room> rooms = new List<Room>();

            switch (type)
            {
                case RoomType.Task:
                    if (Helpers.RandomGenerator.RandomInt(0, GameBalance.BossChance + GameBalance.LesserMobChance) > GameBalance.BossChance)
                    {
                        rooms = new List<Room>(ReflectiveEnumerator.GetEnumerableOfType<LesserMob>(difficulty).ToList());
                    }
                    else
                    {
                        rooms = new List<Room>(ReflectiveEnumerator.GetEnumerableOfType<Boss>(difficulty).ToList());
                    }

                    break;

                case RoomType.Start:
                    rooms = new List<Room>(ReflectiveEnumerator.GetEnumerableOfType<LesserMob>(difficulty).ToList());
                    break;

                case RoomType.Loot:
                    rooms = new List<Room>(ReflectiveEnumerator.GetEnumerableOfType<LesserMob>(difficulty).ToList());
                    break;

                case RoomType.Finish:
                    rooms = new List<Room>(ReflectiveEnumerator.GetEnumerableOfType<LesserMob>(difficulty).ToList());
                    break;
            }

            return rooms[0] as Room;
        }

        public enum RoomType
        {
            Start,
            Finish,
            Task,
            Loot
        }
    }
}