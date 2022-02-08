﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Buildmode
{
    [ApiVersion(2, 1)]
    public class Buildmode : TerrariaPlugin
    {
        private Timer _buffTimer;

        private List<int> _enabled = new List<int>();

        private bool[] _state = new bool[256];

        private byte[] _surface;
        private byte[] _rock;
        private byte[] _removeBg;

        private double _time;
        private bool _day;

        public Buildmode(Main game) : base(game)
            => Order = 1;

        public override Version Version
            => new Version(3, 0);

        public override string Author
            => "Rozen4334";

        public override string Name
            => "Buildmode";

        public override string Description
            => "A useful tool for TShock FreeBuild servers.";

        public override void Initialize()
        {
            Data.Initialize();

            _buffTimer = new Timer(1000)
            {
                AutoReset = true
            };
            _buffTimer.Elapsed += Refresh;
            _buffTimer.Start();

            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.NetSendBytes.Register(this, OnSendBytes);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);

            Commands.ChatCommands.Add(new Command("buildmode.toggle", Command, "buildmode", "bm"));

            Commands.ChatCommands.Add(new Command("buildmode.time", Time, "stime", "st"));
        }

        private void OnGreet(GreetPlayerEventArgs args)
            => _enabled.Remove(args.Who);

        private void Refresh(object unused, ElapsedEventArgs args)
        {
            foreach (int i in _enabled)
                Effects(i);
        }

        private void OnUpdate(EventArgs args)
        {
            _time++;

            if (!_day && _time > 32400)
            {
                _time = 0;
                _day = true;
            }
            else if (_day && _time > 54000)
            {
                _time = 0;
                _day = false;
            }
        }

        private void OnPostInitialize(EventArgs args)
        {
            _surface = BitConverter.GetBytes((short)Main.worldSurface);
            _rock = BitConverter.GetBytes((short)Main.rockLayer);
            _removeBg = BitConverter.GetBytes((short)Main.maxTilesY);

            _time = Main.time;
            _day = Main.dayTime;
        }

        private void OnSendBytes(SendBytesEventArgs args)
        {
            var plr = TShock.Players[args.Socket.Id];

            if (plr == null)
                return;

            int i = plr.Index;
            bool enabled = _enabled.Contains(i);

            var type = (PacketTypes)args.Buffer[2];
            switch (type)
            {
                case PacketTypes.WorldInfo:
                    {
                        byte[] surface;
                        byte[] rock;

                        int time;
                        bool day;

                        if (enabled)
                        {
                            surface = _removeBg;
                            rock = _removeBg;

                            time = 27000;

                            if (_state[i])
                                day = true;

                            else
                                day = false;
                        }

                        else
                        {
                            surface = _surface;
                            rock = _rock;

                            time = (int)_time;
                            day = _day;
                        }

                        Buffer.BlockCopy(surface, 0, args.Buffer, 17, 2);
                        Buffer.BlockCopy(rock, 0, args.Buffer, 19, 2);

                        Buffer.BlockCopy(BitConverter.GetBytes(time), 0, args.Buffer, 3, 4);
                        args.Buffer[7] = (byte)(day ? 1 : 0);
                    }
                    break;

                case PacketTypes.TimeSet:
                    {
                        int time;
                        bool day;

                        if (enabled)
                        {
                            time = 27000;
                            day = true;
                        }

                        else
                        {
                            time = (int)_time;
                            day = _day;
                        }

                        args.Buffer[3] = (byte)(day ? 1 : 0);
                        Buffer.BlockCopy(BitConverter.GetBytes(time), 0, args.Buffer, 4, 4);
                    }
                    break;
            }
        }
        
        private void Command(CommandArgs args)
        {
            int i = args.Player.Index;
            int id = args.Player.Account.ID;

            switch (args.Parameters.FirstOrDefault())
            {
                case "buffs":
                case "buff":
                case "b":
                    args.Parameters.RemoveAt(0);
                    switch (args.Parameters.FirstOrDefault())
                    {
                        case "remove":
                        case "r":
                        case "delete":
                        case "del":
                            {
                                int buffId = 0;
                                if (args.Parameters.Count != 2)
                                    args.Player.SendErrorMessage("Please define a buff to remove");

                                else if (!int.TryParse(args.Parameters[1], out buffId))
                                {
                                    var found = TShock.Utils.GetBuffByName(args.Parameters[1]);

                                    if (found.Count == 0)
                                        args.Player.SendErrorMessage("Invalid buff name!");

                                    else if (found.Count > 1)
                                        args.Player.SendMultipleMatchError(found.Select(f => Lang.GetBuffName(f)));

                                    else
                                        buffId = found[0];

                                }

                                if (buffId > 0 && buffId < Main.maxBuffTypes)
                                {
                                    var current = Data.Read(id);

                                    if (current.Contains(buffId))
                                    {
                                        Data.Update(current.Where(x => x != buffId), id);
                                        args.Player.SendSuccessMessage($"Removed {Lang.GetBuffName(buffId)}");
                                    }

                                    else 
                                        args.Player.SendErrorMessage("You do not have this buff so it was not removed.");
                                }
                            }
                            return;

                        case "append":
                        case "add":
                        case "a":
                            {
                                int buffId = 0;
                                if (args.Parameters.Count != 2)
                                    args.Player.SendErrorMessage("Please define a buff to add");

                                else if (!int.TryParse(args.Parameters[1], out buffId))
                                {
                                    var found = TShock.Utils.GetBuffByName(args.Parameters[1]);

                                    if (found.Count == 0)
                                        args.Player.SendErrorMessage("Invalid buff name!");

                                    else if (found.Count > 1)
                                        args.Player.SendMultipleMatchError(found.Select(f => Lang.GetBuffName(f)));

                                    else
                                        buffId = found[0];
                                }

                                if (buffId > 0 && buffId < Main.maxBuffTypes)
                                {
                                    var current = Data.Read(id);
                                    if (current.Contains(buffId))
                                        args.Player.SendErrorMessage("You already have this buff!");

                                    else
                                    {
                                        Data.Update(current.Append(buffId), id);
                                        args.Player.SendSuccessMessage($"Added {Lang.GetBuffName(buffId)}");
                                    }
                                }

                                else 
                                    args.Player.SendErrorMessage("Invalid buff ID!");
                            }
                            return;

                        case "list":
                        case "l":
                            args.Player.SendInfoMessage($"Your current buffs: (Defined by ID)\n{string.Join(", ", Data.Read(args.Player.Account.ID))}");
                            return;
                        default:
                            args.Player.SendErrorMessage("Invalid syntax. Valid syntax: '/buildmode buffs (add/remove/list) <buff>");
                            return;
                    }

                case "day":
                case "d":
                case "daytime":
                    if (!_enabled.Contains(i))
                        _enabled.Add(i);
                    _state[i] = true;

                    args.Player.SendData(PacketTypes.WorldInfo);
                    args.Player.SendSuccessMessage("Daytime buildmode activated.");

                    return;

                case "night":
                case "n":
                case "nighttime":
                    if (!_enabled.Contains(i))
                        _enabled.Add(i);
                    _state[i] = false;

                    args.Player.SendData(PacketTypes.WorldInfo);
                    args.Player.SendSuccessMessage("Nighttime buildmode activated.");

                    return;

                case "disable":
                case "dis":
                case "stop":
                case "break":
                    var success = _enabled.Remove(i);

                    if (success)
                    {
                        args.Player.SendData(PacketTypes.WorldInfo);
                        args.Player.SendSuccessMessage("Buildmode has been disabled.");
                    }
                    else
                        args.Player.SendErrorMessage("Buildmode was not active!");

                    return;

                default:
                    args.Player.SendErrorMessage("Invalid syntax. '/bm (day/night/buffs/disable)");
                    return;
            }
        }

        private void Time(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                double time = _time / 3600.0;
                time += 4.5;
                if (!_day)
                    time += 15.0;
                time %= 24.0;
                args.Player.SendInfoMessage("The current time is {0}:{1:D2}.", (int)Math.Floor(time), (int)Math.Floor((time % 1.0) * 60.0));
                return;
            }
            switch (args.Parameters[0].ToLower())
            {
                case "day":
                    SetTime(true, 0.0);
                    args.Player.SendSuccessMessage("You have changed the server time to 4:30 (4:30 AM).");
                    TSPlayer.All.SendMessage(string.Format("{0} has set the server time to 4:30.", args.Player.Name), Color.CornflowerBlue);
                    break;
                case "night":
                    SetTime(false, 0.0);
                    args.Player.SendSuccessMessage("You have changed the server time to 19:30 (7:30 PM).");
                    TSPlayer.All.SendMessage(string.Format("{0} has set the server time to 19:30.", args.Player.Name), Color.CornflowerBlue);
                    break;
                case "noon":
                    SetTime(true, 27000.0);
                    args.Player.SendSuccessMessage("You have changed the server time to 12:00 (12:00 PM).");
                    TSPlayer.All.SendMessage(string.Format("{0} has set the server time to 12:00.", args.Player.Name), Color.CornflowerBlue);
                    break;
                case "midnight":
                    SetTime(false, 16200.0);
                    args.Player.SendSuccessMessage("You have changed the server time to 0:00 (12:00 AM).");
                    TSPlayer.All.SendMessage(string.Format("{0} has set the server time to 0:00.", args.Player.Name), Color.CornflowerBlue);
                    break;
                default:
                    string[] array = args.Parameters[0].Split(':');
                    if (array.Length != 2)
                    {
                        args.Player.SendErrorMessage("Invalid time string! Proper format: hh:mm, in 24-hour time.");
                        return;
                    }

                    int hours;
                    int minutes;
                    if (!int.TryParse(array[0], out hours) || hours < 0 || hours > 23
                        || !int.TryParse(array[1], out minutes) || minutes < 0 || minutes > 59)
                    {
                        args.Player.SendErrorMessage("Invalid time string! Proper format: hh:mm, in 24-hour time.");
                        return;
                    }

                    decimal time = hours + (minutes / 60.0m);
                    time -= 4.50m;
                    if (time < 0.00m)
                        time += 24.00m;

                    if (time >= 15.00m)
                        SetTime(false, (double)((time - 15.00m) * 3600.0m));

                    else
                        SetTime(true, (double)(time * 3600.0m));

                    args.Player.SendSuccessMessage(string.Format("You have changed the server time to {0}:{1:D2}.", hours, minutes));
                    TSPlayer.All.SendMessage(string.Format("{0} set the server time to {1}:{2:D2}.", args.Player.Name, hours, minutes), Color.CornflowerBlue);
                    break;
            }
        }

        private void Effects(int player)
        {
            var plr = TShock.Players[player];
            foreach (var buff in Data.Read(player))
                plr.SetBuff(buff, 120);
        }

        private void SetTime(bool dayTime, double time)
        {
            _day = dayTime;
            _time = time;
            TSPlayer.Server.SetTime(dayTime, time);
            TSPlayer.All.SendData(PacketTypes.TimeSet, "", dayTime ? 1 : 0, (int)time, Main.sunModY, Main.moonModY);
        }
    }
}
