using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using TShockAPI;

namespace Buildmode
{
    public class Config
    {
        public static Settings Settings = Settings.Read();
    }

    public class Settings
    {
        public int[] BaseBuffs;

        public int[] Blacklist;

        public static Settings Read()
        {
            var path = Path.Combine(TShock.SavePath, "Buildmode.json");
            if (!File.Exists(path))
            {
                File.WriteAllText(path, JsonConvert.SerializeObject(Default(), Formatting.Indented));
                return Default();
            }
            try
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));
            }
            catch { return Default(); }
        }

        private static Settings Default()
        {
            return new Settings()
            {
                BaseBuffs = new int[]
                {
                    BuffID.Builder,
                    BuffID.Mining,
                    BuffID.NightOwl,
                    BuffID.ObsidianSkin,
                    BuffID.WaterWalking,
                    BuffID.Gills
                },
                Blacklist = new int[]
                {
                    BuffID.OnFire,
                    BuffID.OnFire3
                }
            };
        }
    }
}
