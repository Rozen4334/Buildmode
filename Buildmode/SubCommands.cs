using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace Buildmode
{
    class SubCommands
    {
        public static void Help(CommandArgs args)
        {

        }

        public static void Settings(CommandArgs args)
        {

        }

        public static void Buffs(CommandArgs args)
        {
            if (args.Parameters.Count > 1)
            {
                var data = Data.Read(args.Player.Account.ID);
                switch (args.Parameters[1])
                {
                    case "add":
                        if (args.Parameters.Count == 2)
                            args.Player.SendInfoMessage("This command adds buffs. Valid syntax: /bm buffs add <buffs (by name or id)>[]");
                        else for (int i = 2; i < args.Parameters.Count; i++)
                            {
                                if (int.TryParse(args.Parameters[i], out int id))
                                {
                                    if (Config.Settings.Blacklist.Any(x => x == id))
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' has not been added because it is a banned buff.");
                                        continue;
                                    }
                                    if (id > 0 && id < Main.maxBuffTypes)
                                    {
                                        if (data.Any(x => x == id))
                                        {
                                            args.Player.SendErrorMessage($"You already have '{Lang.GetBuffName(id)}'!");
                                            continue;
                                        }
                                        data.Add(id);
                                        args.Player.SendSuccessMessage($"Succesfully added '{Lang.GetBuffName(id)}'.");
                                    }
                                    else
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' has not been added as this is not a valid buff ID.");
                                        continue;
                                    }
                                }
                                else
                                {
                                    var found = TShock.Utils.GetBuffByName(args.Parameters[i]);
                                    if (found.Count == 0)
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' is not a valid buff. This buff has not been added.");
                                        continue;
                                    }
                                    if (found.Count > 1)
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' was not added as multiple matches were found.");
                                        continue;
                                    }
                                    if (Config.Settings.Blacklist.Any(x => x == found.FirstOrDefault()))
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' has not been added because it is a banned buff.");
                                        continue;
                                    }
                                    if (data.Any(x => x == id))
                                    {
                                        args.Player.SendErrorMessage($"You already have '{Lang.GetBuffName(found.FirstOrDefault())}'!");
                                        continue;
                                    }
                                    data.Add(found.FirstOrDefault());
                                    args.Player.SendSuccessMessage($"Succesfully added '{Lang.GetBuffName(id)}'.");
                                }
                            }
                        Data.Update(data, args.Player.Account.ID);
                        break;
                    case "remove":
                        if (args.Parameters.Count == 2)
                            args.Player.SendInfoMessage("This command removes buffs. Valid syntax: /bm buffs remove <buffs (by name or id)>[] (all) < removes all buffs but the base ones.");
                        else if (args.Parameters[2] == "all")
                        {
                            args.Player.SendSuccessMessage("All custom buffs have been removed.");
                            Data.Reset(args.Player.Account.ID);
                        }
                        else for (int i = 2; i < args.Parameters.Count; i++)
                            if (int.TryParse(args.Parameters[i], out int id))
                            {
                                if (data.Any(x => x == id))
                                {
                                    if (Config.Settings.BaseBuffs.Any(x => x == id))
                                    {
                                        args.Player.SendErrorMessage($"'{args.Parameters[i]}' has not been removed as it is a base buff.");
                                        continue;
                                    }
                                    else
                                    {
                                        args.Player.SendSuccessMessage($"Removed '{Lang.GetBuffName(id)}'.");
                                        data.Remove(id);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                var found = TShock.Utils.GetBuffByName(args.Parameters[i]);
                                if (found.Count > 1)
                                {
                                    args.Player.SendErrorMessage($"'{args.Parameters[i]}' was not removed as multiple matches were found:");
                                    continue;
                                }
                                if (found.Count == 0)
                                {
                                    args.Player.SendErrorMessage($"'{args.Parameters[i]}' was not removed as no buff by this name exists.");
                                    continue;
                                }
                                if (Config.Settings.BaseBuffs.Any(x => x == found.FirstOrDefault()))
                                {
                                    args.Player.SendErrorMessage($"'{args.Parameters[i]}' was not removed as it is a base buff.");
                                    continue;
                                }
                                if (data.Any(x => x == found.FirstOrDefault()))
                                {
                                    args.Player.SendSuccessMessage($"Removed '{Lang.GetBuffName(found.FirstOrDefault())}'.");
                                    data.Remove(found.FirstOrDefault());
                                    continue;
                                }
                            }
                        Data.Update(data, args.Player.Account.ID);
                        break;
                    case "list":
                    default:
                        args.Player.SendErrorMessage("Invalid syntax. Valid syntax: /bm buffs <add/remove> <buffs (by name or id)>[]");
                        return;
                }
            }
        }

        public static void Toggle(CommandArgs args)
        {
            var user = Buildmode.BMUsers[args.Player.Index];
            args.Player.SendSuccessMessage($"{(user.Enabled ? "En" : "Dis")}abled buildmode.");
        }
    }
}
