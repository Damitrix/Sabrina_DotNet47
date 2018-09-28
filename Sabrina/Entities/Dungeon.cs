using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sabrina.Entities
{
    class DungeonLogic
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
            public DateTime LastAction;
            public Room CurrentRoom;
        }

        public class Room
        {
            public Room[] AdjacentRooms;
            public Content Content;

            public Room(RoomType type)
            {

            }

            public enum RoomType
            {
                Edging,
                Finish,
                Reward,
                Treasure
            }
        }
    }
}
