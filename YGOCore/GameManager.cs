using System;
using YGOCore.Game;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace YGOCore
{
    public static class GameManager
    {

        static Dictionary<string, GameRoom> m_rooms = new Dictionary<string,GameRoom>();

        public static GameRoom CreateOrGetGame(GameConfig config)
        {
            if (m_rooms.ContainsKey(config.Name))
                return m_rooms[config.Name];
            return CreateRoom(config);
        }

        public static GameRoom GetGame(string name)
        {
            if (m_rooms.ContainsKey(name))
                return m_rooms[name];
            return null;
        }

        public static GameRoom CreateOrGetGetRandomGame(string mode)
        {
            List<GameRoom> filteredRooms = new List<GameRoom>();
            GameRoom[] rooms;
            rooms = new GameRoom[m_rooms.Count];
            m_rooms.Values.CopyTo(rooms, 0);

            foreach (GameRoom room in rooms)
            {
                if (room.Game.State == Game.Enums.GameState.Lobby
                    && room.Game.GetAvailablePlayerPos() > -1
					&& room.Game.Config.Mode == (mode=="s#" ? 0:mode=="m#" ? 1:2)
                    && room.Game.Config.Name.Length > 10
					&& room.Game.Config.Name.Substring (2,6) == "random")
                    // length > 10 在 substring 之前被判断
                    filteredRooms.Add(room);
            }

            int id = Program.Random.Next(0, filteredRooms.Count * 2 + 1);
			if (id >=filteredRooms.Count) id-=filteredRooms.Count;
			if (id >=filteredRooms.Count) return GameManager.CreateRoom(new GameConfig(mode));
            return filteredRooms[id];
        }

        public static GameRoom SpectateRandomGame()
        {
            List<GameRoom> filteredRooms = new List<GameRoom>();
            GameRoom[] rooms;
            rooms = new GameRoom[m_rooms.Count];
            m_rooms.Values.CopyTo(rooms, 0);

            foreach (GameRoom room in rooms)
            {
                if (room.Game.State != Game.Enums.GameState.Lobby)
                    filteredRooms.Add(room);
            }

            if (filteredRooms.Count == 0)
                return null;

            return filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

        public static GameRoom CreateRoom(GameConfig config)
        {
            GameRoom room = new GameRoom(config);
            m_rooms.Add(config.Name, room);
            return room;
        }

        public static void HandleRooms()
        {
            List<string> toRemove = new List<string>();
            foreach (var room in m_rooms)
            {
                if (room.Value.IsOpen)
                    room.Value.HandleGame();
                else
                    toRemove.Add(room.Key);
            }

            foreach (string room in toRemove)
            {
                m_rooms.Remove(room);
                Logger.WriteLine("Game--");
            }
        }

        public static bool GameExists(string name)
        {
            return m_rooms.ContainsKey(name);
        }

        public static string RandomRoomName(string mode)
        {
            while (true) //keep searching till one is found!!
            {
                string GuidString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                GuidString = GuidString.Replace("=", "");
                GuidString = GuidString.Replace("+", "");
                string roomname = mode+"random"+GuidString.Substring(0, 8);				
                if (!m_rooms.ContainsKey(roomname))
                    return roomname;
            }     
        }

    }
}
