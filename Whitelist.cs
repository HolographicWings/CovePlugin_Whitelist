using Cove.Server.Plugins;
using Cove.Server;
using Cove.Server.Actor;
using System.Text.RegularExpressions;

namespace Whitelist
{
    public class Whitelist : CovePlugin
    {
        public Whitelist(CoveServer server) : base(server) { }

        private bool WhitelistEnabled = false;
        private const string WhitelistFile = "whitelist.txt";
        private string WhitelistFileFormatted = Path.Combine(Directory.GetCurrentDirectory(), WhitelistFile);
        private List<ulong> _Whitelist = new List<ulong>();

        public override void onInit()
        {
            base.onInit();
            Log("Whitelist plugin loading!");
            readWhitelistFile();

            RegisterCommand("whitelist", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }
                if (args.Length == 0)
                {
                    SendPlayerChatMessage(player, "Usage: <Whitelist add/remove XXX> or <Whitelist on/off>");
                    return;
                }
                string subcommand = args[0].ToLower();

                switch (subcommand)
                {
                    case "add":
                    case "remove":
                        if (args.Length != 2)
                        {
                            SendPlayerChatMessage(player, "Usage: <Whitelist add/remove XXX>");
                            return;
                        }

                        bool _isSteamID = isSteamID(args[1]);
                        ulong? id = null;
                        string? username = null;
                        if (_isSteamID)
                        {
                            if (ulong.TryParse(args[1], out ulong parsedId))
                            {
                                id = parsedId;
                            }
                            if (id == null)
                            {
                                SendPlayerChatMessage(player, "Invalid ID!");
                                return;
                            }
                        }
                        else
                        {
                            username = args[1];
                        }

                        switch (subcommand)
                        {
                            case "add":
                                if (_isSteamID && id != null && !_Whitelist.Contains((ulong)id))
                                {
                                    _Whitelist.Add((ulong)id);
                                    SendPlayerChatMessage(player, $"user ({(ulong)id}) added to whitelist");
                                    Log($"user ({(ulong)id}) added to whitelist");
                                    writeWhitelistFile();
                                }
                                else
                                {
                                    WFPlayer? p = ParentServer.AllPlayers.FirstOrDefault(p => p.Username == username);
                                    if (p != null && !_Whitelist.Contains(p.SteamId.m_SteamID))
                                    {
                                        _Whitelist.Add(p.SteamId.m_SteamID);
                                        SendPlayerChatMessage(player, $"{p.Username} ({p.SteamId.m_SteamID}) added to whitelist");
                                        Log($"{p.Username} ({p.SteamId.m_SteamID}) added to whitelist");
                                        writeWhitelistFile();
                                    }
                                    else
                                    {
                                        SendPlayerChatMessage(player, "Player not found, please use SteamID!");
                                    }
                                }
                                return;
                            case "remove":
                                if (_isSteamID && id != null && _Whitelist.Contains((ulong)id))
                                {
                                    _Whitelist.Remove((ulong)id);
                                    SendPlayerChatMessage(player, $"user ({(ulong)id}) removed from whitelist");
                                    Log($"user ({(ulong)id}) removed from whitelist");
                                    writeWhitelistFile();
                                }
                                else
                                {
                                    WFPlayer? p = ParentServer.AllPlayers.FirstOrDefault(p => p.Username == username);
                                    if (p != null && _Whitelist.Contains(p.SteamId.m_SteamID))
                                    {
                                        _Whitelist.Remove(p.SteamId.m_SteamID);
                                        SendPlayerChatMessage(player, $"{p.Username} ({p.SteamId.m_SteamID}) removed from whitelist");
                                        Log($"{p.Username} ({p.SteamId.m_SteamID}) removed from whitelist");
                                        writeWhitelistFile();
                                    }
                                    else
                                    {
                                        SendPlayerChatMessage(player, "Player not found, please use SteamID!");
                                    }
                                }
                                return;
                            default:
                                SendPlayerChatMessage(player, "Usage: <Whitelist add/remove XXX>");
                                return;
                        }
                    case "on":
                        enableWhitelist(true);
                        SendPlayerChatMessage(player, "Whitelist enabled!");
                        return;
                    case "off":
                        enableWhitelist(false);
                        SendPlayerChatMessage(player, "Whitelist disabled!");
                        return;
                    default:
                        SendPlayerChatMessage(player, "Please use <Whitelist add XXX> or <Whitelist remove XXX>!");
                        return;
                }
            });
            SetCommandDescription("whitelist", "Manage the whitelist, use <Whitelist add XXX> or <Whitelist remove XXX>");

            Log("Whitelist plugin loaded!");

        }
        public override void onEnd()
        {
            base.onEnd();

            Log("Stopping Whitelist plugin!");
        }
        public override void onPlayerJoin(WFPlayer player)
        {
            base.onPlayerJoin(player);
            if (WhitelistEnabled && !_Whitelist.Contains(player.SteamId.m_SteamID))
            {
                ParentServer.kickPlayer(player.SteamId);
            }
        }
        bool isSteamID(string input)
        {
            return Regex.IsMatch(input, @"^7656119\d{10}$");
        }
        public void checkWhitelistFile()
        {
            if (!File.Exists(WhitelistFileFormatted))
            {
                File.WriteAllText(WhitelistFileFormatted, "false\n");
            }
        }
        public void readWhitelistFile()
        {
            checkWhitelistFile();
            string[] lines = File.ReadAllLines(WhitelistFileFormatted);

            if (lines.Length == 0)
            {
                WhitelistEnabled = false;
                return;
            }

            if (!bool.TryParse(lines[0], out WhitelistEnabled))
            {
                WhitelistEnabled = false;
            }
            Log($"Whitelist {(WhitelistEnabled ? "enabled" : "disabled")}!");

            _Whitelist.Clear();
            foreach (var item in lines.Skip(1))
            {
                if (ulong.TryParse(item, out ulong id))
                {
                    if (!_Whitelist.Contains(id)) _Whitelist.Add(id);
                }
            }
        }
        public void writeWhitelistFile()
        {
            checkWhitelistFile();
            List<string> output = new List<string>
            {
                WhitelistEnabled.ToString().ToLower()
            };
            output.AddRange(_Whitelist.Select(x => x.ToString()));
            File.WriteAllLines(WhitelistFileFormatted, output);
        }
        public void enableWhitelist(bool enabled = false)
        {
            WhitelistEnabled = enabled;
            writeWhitelistFile();
            if (enabled)
            {
                foreach (WFPlayer? p in ParentServer.AllPlayers)
                {
                    if (!_Whitelist.Contains(p.SteamId.m_SteamID))
                    {
                        ParentServer.kickPlayer(p.SteamId);
                    }
                }
            }
            Log($"Whitelist {(enabled ? "enabled" : "disabled")}!");
        }
    }
}
