using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Buildmode
{
    public class User
    {
        public string Buffs { get; set; } 
            = string.Join(", ", new List<int>(Config.Settings.BaseBuffs));

        public bool Active { get; set; } 
            = false;

        public bool ShowPlayers { get; set; }
            = false;

        public bool Enabled { get; set; }
            = false;

        public double Time { get; set; } 
            = Main.time;

        public bool Day { get; set; }
            = Main.dayTime;

        public byte[] Surface { get; set; } 
            = BitConverter.GetBytes((short) Main.worldSurface);

        public byte[] Rock { get; set; } 
            = BitConverter.GetBytes((short)Main.rockLayer);

        public byte[] RemoveBg { get; set; } 
            = BitConverter.GetBytes((short)Main.maxTilesY);

        public int Index { get; private set; }

        public int ID { get; private set; }

        public User(TSPlayer player)
        {
            Index = player.Index;
            ID = player.Account.ID;
            if (Data.Read(player, out User user))
            {
                Time = user.Time;
                Day = user.Day;
                ShowPlayers = user.ShowPlayers;
                Buffs = user.Buffs;
            }
        }
    }
}
