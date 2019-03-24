using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using InfinityScript;

namespace b3helper
{
    public class b3helper : BaseScript
    {
        //HudElem
        private static HudElem[] KillStreakHud = new HudElem[18];
        private static HudElem[] NoKillsHudElem = new HudElem[18];

        //Hud for Information
        private HudElem top;
        private HudElem bottom;
        private HudElem right;
        private HudElem left;

        //Mode
        volatile string MapRotation = "";

        public b3helper()
        {
            //Making and Settings dvars if they are unused and have value.
            Call("setDvarifUninitialized", "sv_hideCommands", "1"); //Done
            Call("setDvarifUninitialized", "sv_gmotd", "^:Welcome to the server."); //Done
            Call("setDvarifUninitialized", "sv_forceSmoke", "1"); //Done
            Call("setDvarifUninitialized", "sv_forceDemoRecording", "1"); //Done
            Call("setDvarifUninitialized", "sv_objText", "^1This is menu text."); //Done
            Call("setDvarifUninitialized", "sv_clientDvars", "1"); //Done
            Call("setDvarifUninitialized", "sv_rate", "60000");
            Call("setDvarifUninitialized", "sv_serverDvars", "1"); //Done
            Call("setDvarifUninitialized", "sv_killStreakCounter", "1"); //Done
            Call("setDvarifUninitialized", "sv_hudEnable", "1"); //Dome
            Call("setDvarifUninitialized", "sv_hudTop", "^1TOP Message"); //Done
            Call("setDvarifUninitialized", "sv_hudBottom", "^1Bottom Message"); //Done
            Call("setDvarifUninitialized", "sv_hudRight", "^1Right Message"); //Done
            Call("setDvarifUninitialized", "sv_hudLeft", "^1Left Message"); //Done
            Call("setDvarifUninitialized", "sv_scrollingSpeed", "30"); //Done
            Call("setDvarifUninitialized", "sv_scrollingHud", "1"); //Done
            Call("setDvarifUninitialized", "sv_b3Execute", "null"); //Done
            Call("setDvarifUninitialized", "sv_b3latency", "1000"); //Done

            //Loading Server Dvars.
            ServerDvars();

            //HudElem For Information
            InformationHuds();

            //Assigning things.
            PlayerConnected += OnPlayerConnect;
        }


        public void ServerDvars()
        {
            if (Call<int>("getDvarInt", "sv_serverDvars") != 0)
            {
                Function.Call("setdevDvar", "sv_network_fps", 200);
                Function.Call("setDvar", "sv_hugeSnapshotSize", 10000);
                Function.Call("setDvar", "sv_hugeSnapshotDelay", 100);
                Function.Call("setDvar", "sv_pingDegradation", 0);
                Function.Call("setDvar", "sv_pingDegradationLimit", 9999);
                Function.Call("setDvar", "sv_acceptableRateThrottle", 9999);
                Function.Call("setDvar", "sv_newRateThrottling", 2);
                Function.Call("setDvar", "sv_minPingClamp", 50);
                Function.Call("setDvar", "sv_cumulThinkTime", 1000);
                Function.Call("setDvar", "sys_lockThreads", "all");
                Function.Call("setDvar", "com_maxFrameTime", 1000);
                Function.Call("setDvar", "com_maxFps", 0);
                Function.Call("setDvar", "sv_voiceQuality", 9);
                Function.Call("setDvar", "maxVoicePacketsPerSec", 1000);
                Function.Call("setDvar", "maxVoicePacketsPerSecForServer", 200);
                Function.Call("setDvar", "cg_everyoneHearsEveryone", 1);
                Function.Call("makedvarserverinfo", "motd", Call<string>("getDvar", "sv_gmotd"));
                Function.Call("makedvarserverinfo", "didyouknow", Call<string>("getDvar", "sv_gmotd"));
                if (Call<int>("getDvarInt", "sv_forceDemoRecording") != 0)
                {
                    Function.Call("setDvar", "cl_demo_enabled", 1);
                }
            }
        }



        public void OnPlayerConnect(Entity player)
        {
            //Reseting killstreak on player connect
            player.SetField("playerKillStreak", 0);
            //Client Performance dvar
            if (Call<int>("getDvarInt", "sv_clientDvars") != 0)
            {
                player.SetClientDvar("cg_objectiveText", Call<String>("getDvar", "sv_objText"));
                player.SetClientDvar("sys_lockThreads", "all");
                player.SetClientDvar("com_maxFrameTime", "1000");
                player.SetClientDvar("rate ", Call<string>("getDvar", "sv_rate"));
                player.SpawnedPlayer += () =>
                {
                    player.SetClientDvar("cg_objectiveText", Call<String>("getDvar", "sv_objText"));
                };
            }
            if (Call<int>("getDvarInt", "sv_forceSmoke") != 0)
            {
                player.SetClientDvar("fx_draw", "1");
            }
            if (Call<int>("getDvarInt", "sv_forceDemoRecording") != 0)
            {
                player.SetClientDvar("cl_demo_enabled", "True");
            }

            //Killstreak Related Code
            var killstreakHud = HudElem.CreateFontString(player, "hudsmall", 0.8f);
            killstreakHud?.SetPoint("TOP", "TOP", -9, 2);
            killstreakHud?.SetText("^5Killstreak: ");
            killstreakHud.HideWhenInMenu = true;

            var noKills = HudElem.CreateFontString(player, "hudsmall", 0.8f);
            noKills?.SetPoint("TOP", "TOP", 39, 2);
            noKills?.SetText("^20");
            noKills.HideWhenInMenu = true;

            KillStreakHud[GetEntityNumber(player)] = killstreakHud;
            NoKillsHudElem[GetEntityNumber(player)] = noKills;
        }


        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (!player.HasField("playerKillStreak") || !attacker.HasField("playerKillStreak"))
                return;
            try
            {
                if (player != attacker) //Suicide Alert!
                {
                    attacker.SetField("playerKillStreak", attacker.GetField<int>("playerKillStreak") + 1);
                }
                player.SetField("playerKillStreak", 0);
                var attackerNoKills = NoKillsHudElem[GetEntityNumber(attacker)];
                if (attackerNoKills == null)
                {
                    throw new Exception("AttackerNoKills is null. Attacker: " + attacker.Name);
                }
                attackerNoKills.SetText("^2" + attacker.GetField<int>("playerKillStreak"));
                NoKillsHudElem[GetEntityNumber(attacker)] = attackerNoKills;

                var victimNoKills = NoKillsHudElem[GetEntityNumber(player)];
                if (victimNoKills == null)
                {
                    throw new Exception("VictimNoKills is null. Victim: " + player.Name);
                }
                victimNoKills.SetText("0");
                NoKillsHudElem[GetEntityNumber(player)] = victimNoKills;
            }
            catch (Exception ex)
            {
                Log.Error("Error in Killstreak: " + ex.Message + ex.StackTrace);
                return;
            }

        }


        public override EventEat OnSay2(Entity player, string name, string message)
        {
            try
            {
                message = message.ToLower();
                if ((message.StartsWith("!")) || (message.StartsWith("@")))
                {
                    AfterDelay(Call<int>("getDvarInt", "sv_b3latency"), () =>
                    {
                        if (Call<string>("getDvar", "sv_b3Execute") != "null")
                        {
                            string content = Call<string>("getDvar", "sv_b3Execute");
                            ProcessCommand(content);
                            Call("setDvar", "sv_b3Execute", "null");
                        }
                    });

                    if (Call<int>("getDvarInt", "sv_hideCommands") != 0)
                        return EventEat.EatGame;
                }
            }

            catch (Exception e)
            {
                Log.Error(e);
            }
            return EventEat.EatNone;
        }


        public void InformationHuds()
        {
            if (Call<int>("getDvarInt", "sv_hudEnable") != 0)
            {
                if (Call<string>("getDvar", "sv_hudTop") != "null")
                {
                    top = HudElem.CreateServerFontString("hudbig", 0.5f);
                    top.SetPoint("TOPCENTER", "TOPCENTER", 0, 5);
                    top.HideWhenInMenu = true;
                    top.SetText(Call<string>("getDvar", "sv_hudTop"));
                }
                if (Call<string>("getDvar", "sv_hudRight") != "null")
                {
                    right = HudElem.CreateServerFontString("hudbig", 0.5f);
                    right.SetPoint("TOPRIGHT", "TOPRIGHT", -5, 5);
                    right.HideWhenInMenu = true;
                    right.SetText(Call<string>("getDvar", "sv_hudRight"));
                }
                if (Call<string>("getDvar", "sv_hudRight") != "null")
                {
                    left = HudElem.CreateServerFontString("hudbig", 0.5f);
                    left.SetPoint("TOPLEFT", "TOPLEFT", 6, 105);
                    left.HideWhenInMenu = true;
                    left.SetText(Call<string>("getDvar", "sv_hudLeft"));
                }
                if ((Call<string>("getDvar", "sv_hudBottom") != "null") && (Call<int>("getDvarInt", "sv_scrollingHud") != 0) && (Call<int>("getDvarInt", "sv_scrollingSpeed") != 0))
                {
                    bottom = HudElem.CreateServerFontString("hudbig", 0.4f);
                    bottom.SetPoint("CENTER", "BOTTOM", 0, -5);
                    bottom.Foreground = true;
                    bottom.HideWhenInMenu = true;
                    OnInterval(30000, () =>
                    {
                        bottom.SetText(Call<string>("getDvar", "sv_hudBottom"));
                        bottom.SetPoint("CENTER", "BOTTOM", 1100, -5);
                        bottom.Call("moveovertime", Call<int>("getDvarInt", "sv_scrollingSpeed"));
                        bottom.X = -700f;
                        return true;
                    });

                }
                else if (Call<string>("getDvar", "sv_hudBottom") != "null")
                {
                    bottom = HudElem.CreateServerFontString("hudbig", 0.5f);
                    bottom.SetPoint("BOTTOMCENTER", "BOTTOMCENTER", 0, -5);
                    bottom.HideWhenInMenu = true;
                    bottom.SetText(Call<string>("getDvar", "sv_hudBottom"));
                }
            }

        }

        public void ProcessCommand(string message)
        {
            try
            {
                string[] msg = message.Split(' ');
                msg[0] = msg[0].ToLowerInvariant();
                if (msg[0].StartsWith("!afk"))
                {
                    Entity player = FindSinglePlayer(msg[1]);
                    ChangeTeam(player, "spectator");
                }
                if (msg[0].StartsWith("!setafk"))
                {
                    Entity target = FindSinglePlayer(msg[1]);
                    ChangeTeam(target, "spectator");
                }
                if (msg[0].StartsWith("!kill"))
                {
                    Entity target = FindSinglePlayer(msg[1]);
                    target.Call("suicide");
                }
                if (msg[0].StartsWith("!suicide"))
                {
                    Entity player = FindSinglePlayer(msg[1]);
                    player.Call("suicide");
                }
                if (msg[0].StartsWith("!teleport"))
                {
                    Entity teleporter = FindSinglePlayer(msg[1]);
                    Entity reciever = FindSinglePlayer(msg[2]);

                    teleporter.Call("setOrigin", reciever.Origin);
                }
                if (msg[0].StartsWith("!mode"))
                {
                    if (!System.IO.File.Exists($@"admin\{msg[1]}.dsr") && !System.IO.File.Exists($@"players2\{msg[1]}.dsr"))
                    {
                        Utilities.RawSayAll("^1DSR not found.");
                        return;
                    }
                    Mode(msg[1]);
                }
                if (msg[0].StartsWith("!gametype"))
                {
                    if (!System.IO.File.Exists($@"admin\{msg[1]}.dsr") && !System.IO.File.Exists($@"players2\{msg[1]}.dsr"))
                    {
                        Utilities.RawSayAll("^1DSR not found.");
                        return;
                    }
                    string newMap = msg[2];
                    Mode(msg[1], newMap);
                    
                }

            }
            catch (Exception e)
            {
                Log.Error("Error in Command Processing. Error:" + e.Message + e.StackTrace);
            }
        }

        public static int GetEntityNumber(Entity player)
        {
            return player.Call<int>("getentitynumber");
        }

        public void Mode(string dsrname, string map = "")
        {
            if (string.IsNullOrWhiteSpace(map))
                map = Call<string>("getDvar", "mapname");

            if (!string.IsNullOrWhiteSpace(MapRotation))
            {
                Log.Error("ERROR: Modechange already in progress.");
                return;
            }

            map = map.Replace("default:", "");
            using (System.IO.StreamWriter DSPLStream = new System.IO.StreamWriter("players2\\EX.dspl"))
            {
                DSPLStream.WriteLine(map + "," + dsrname + ",1000");
            }
            MapRotation = Call<string>("getDvar", "sv_maprotation");
            OnExitLevel();
            Utilities.ExecuteCommand("sv_maprotation EX");
            Utilities.ExecuteCommand("map_rotate");
            Utilities.ExecuteCommand("sv_maprotation " + MapRotation);
            MapRotation = "";
        }

        public List<Entity> FindPlayers(string identifier, Entity sender = null)
        {
            identifier = identifier.ToLowerInvariant();
            return (from player in Players
                    where player.Name.ToLowerInvariant().Contains(identifier)
                    select player).ToList();
        }

        public Entity FindSinglePlayer(string identifier)
        {
            List<Entity> players = FindPlayers(identifier);
            if (players.Count != 1)
                return null;
            return players[0];
        }

        public void ChangeTeam(Entity player, string team)
        { 
            player.SetField("sessionteam", team);
            player.Notify("menuresponse", "team_marinesopfor", team);
        }
    }
}
