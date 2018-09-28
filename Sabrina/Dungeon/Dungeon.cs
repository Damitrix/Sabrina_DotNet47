using Sabrina.Dungeon.Rooms;

using System;
using System.Collections.Generic;
using System.Linq;
using Sabrina.Entities;
using static Sabrina.Dungeon.Rooms.Room;

namespace Sabrina.Dungeon
{
    public class DungeonLogic
    {
        public class Item
        {
            public int Id;
            public string Name;
        }

        public class Session
        {
            public long UserId;
            public DateTime Start;
            public DateTime LastActionTime;
            public Room CurrentRoom;
            public Dungeon CurrentDungeon;
        }

        public class Dungeon
        {
            private readonly int startChance = 5000;
            private int chanceToDivert;
            public List<List<Room>> Rooms;
            public DungeonDifficulty Difficulty { get; private set; }
            public DungeonLength Length { get; private set; }

            public Dungeon(int level, DungeonDifficulty difficulty, DungeonLength length)
            {
                this.chanceToDivert = this.startChance;
                bool isFinished = false;

                while (!isFinished)
                {
                    List<Room> cRoomLayer = new List<Room>();

                    this.Rooms.Add(cRoomLayer);
                }
            }

            private List<Room> GenerateLayer(List<Room> previousLayer)
            {
                List<Room> generatedLayer = new List<Room>();

                foreach (Room prevRoom in previousLayer)
                {
                    int chanceDifference = this.startChance - this.chanceToDivert;

                    int minNumberForDouble = chanceDifference - chanceDifference;
                    int minNumberForSingle = chanceDifference;

                    int chance = Helpers.RandomGenerator.RandomInt(0, this.startChance);

                    this.chanceToDivert -= ((int)DungeonLength.Endless - (int)this.Length);

                    if (this.chanceToDivert < minNumberForDouble)
                    {
                        //Generate Two Rooms
                        var room1 = this.GenerateNormalRoom();
                        room1.AdjacentRooms = new Room[] { prevRoom };
                        generatedLayer.Add(room1);
                        var room2 = this.GenerateNormalRoom();
                        room2.AdjacentRooms = new Room[] { prevRoom };
                        generatedLayer.Add(room2);
                    }
                    else if (this.chanceToDivert < minNumberForSingle)
                    {
                        //Generate a Single Room
                        var room = this.GenerateNormalRoom();
                        room.AdjacentRooms = new Room[] { prevRoom };
                        generatedLayer.Add(room);
                    }
                    else
                    {
                        //Converge with another Room
                    }
                }

                if (!generatedLayer.Any())
                {
                    throw new NullReferenceException("Could not add a Room to the Current Layer.");
                }
                return generatedLayer;
            }

            private Room GenerateNormalRoom()
            {
                int maxRoomChance = 100;
                int lootRoomChance = (int)this.Difficulty * 4;

                Room generatedRoom = null;

                if (Helpers.RandomGenerator.RandomInt(0, maxRoomChance) < lootRoomChance)
                {
                    generatedRoom = GenerateRoom(RoomType.Loot, this.Difficulty);
                }
                else
                {
                    generatedRoom = GenerateRoom(RoomType.Task, this.Difficulty);
                }

                if (generatedRoom == null)
                {
                    throw new NullReferenceException("A suiting Room could not be generated");
                }
                return generatedRoom;
            }

            public enum DungeonLength
            {
                Tiny = 5,
                Small = 10,
                Medium = 20,
                Long = 35,
                Xxl = 50,
                Endless = 100
            }

            public enum DungeonDifficulty
            {
                ChildsPlay = 1,
                Easy = 2,
                Moderate = 3,
                Medium = 4,
                Hard = 5,
                VeryHard = 6,
                Catastrophical = 6
            }
        }
    }
}