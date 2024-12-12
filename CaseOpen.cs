using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CaseOpeningSystem
{
    public class CaseItem
    {
        public string Name { get; set; }
        public string WeaponId { get; set; }
        public string Skin { get; set; }
        public float Chance { get; set; }
        public decimal Price { get; set; }
    }

    public class Case
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<CaseItem> Items { get; set; }
    }

    public class PlayerData
    {
        public decimal Balance { get; set; }
        public List<CaseItem> Inventory { get; set; }
    }

    public class CaseOpeningPlugin : BasePlugin
    {
        public override string ModuleName => "Case Opening System";
        public override string ModuleVersion => "1.0.0";

        private Dictionary<string, Case> Cases = new Dictionary<string, Case>();
        private Dictionary<string, PlayerData> PlayerDatabase = new Dictionary<string, PlayerData>();
        private string ConfigPath;
        private string DatabasePath;

        public override void Load(bool hotReload)
        {
            ConfigPath = Path.Combine(ModuleDirectory, "cases.json");
            DatabasePath = Path.Combine(ModuleDirectory, "playerdata.json");

            LoadCases();
            LoadPlayerData();

            RegisterEventHandler<EventPlayerConnected>(OnPlayerConnect);
            RegisterEventHandler<EventPlayerDisconnected>(OnPlayerDisconnect);

            AddCommand("css_cases", "Open cases menu", CommandCases);
            AddCommand("css_balance", "Check balance", CommandBalance);
            AddCommand("css_inventory", "Check inventory", CommandInventory);
            AddCommand("css_addbalance", "Add balance (Admin)", CommandAddBalance);
        }

        private void LoadCases()
        {
            if (!File.Exists(ConfigPath))
            {
                CreateDefaultConfig();
            }

            string json = File.ReadAllText(ConfigPath);
            Cases = JsonSerializer.Deserialize<Dictionary<string, Case>>(json);
        }

        private void CreateDefaultConfig()
        {
            var defaultCases = new Dictionary<string, Case>
            {
                ["Standard Case"] = new Case
                {
                    Name = "Standard Case",
                    Price = 2.49m,
                    Items = new List<CaseItem>
                    {
                        new CaseItem { Name = "AK-47 | Asiimov", WeaponId = "weapon_ak47", Skin = "asiimov", Chance = 0.5f, Price = 50.00m },
                        new CaseItem { Name = "M4A4 | Howl", WeaponId = "weapon_m4a1", Skin = "howl", Chance = 0.1f, Price = 500.00m }
                    }
                }
            };

            string json = JsonSerializer.Serialize(defaultCases, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }

        private void LoadPlayerData()
        {
            if (File.Exists(DatabasePath))
            {
                string json = File.ReadAllText(DatabasePath);
                PlayerDatabase = JsonSerializer.Deserialize<Dictionary<string, PlayerData>>(json);
            }
        }

        private void SavePlayerData()
        {
            string json = JsonSerializer.Serialize(PlayerDatabase, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DatabasePath, json);
        }

        private HookResult OnPlayerConnect(EventPlayerConnected @event)
        {
            string steamId = @event.Player.SteamID.ToString();
            if (!PlayerDatabase.ContainsKey(steamId))
            {
                PlayerDatabase[steamId] = new PlayerData
                {
                    Balance = 0,
                    Inventory = new List<CaseItem>()
                };
                SavePlayerData();
            }
            return HookResult.Continue;
        }

        private HookResult OnPlayerDisconnect(EventPlayerDisconnected @event)
        {
            SavePlayerData();
            return HookResult.Continue;
        }

        [CommandHelper(minArgs: 0, usage: "!cases")]
        private void CommandCases(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            var menu = new ChatMenu("Case Opening System");

            foreach (var caseItem in Cases)
            {
                menu.AddMenuOption($"{caseItem.Value.Name} (${caseItem.Value.Price})", (p, o) => OpenCase(p, caseItem.Key));
            }

            ChatMenus.OpenMenu(player, menu);
        }

        private void OpenCase(CCSPlayerController player, string caseName)
        {
            string steamId = player.SteamID.ToString();
            var playerData = PlayerDatabase[steamId];
            var selectedCase = Cases[caseName];

            if (playerData.Balance < selectedCase.Price)
            {
                player.PrintToChat($" [Case System] You don't have enough balance. Required: ${selectedCase.Price}");
                return;
            }

            playerData.Balance -= selectedCase.Price;

            // Simulate item drop based on chances
            float randomValue = new Random().NextSingle() * 100;
            float currentProbability = 0;
            CaseItem wonItem = null;

            foreach (var item in selectedCase.Items)
            {
                currentProbability += item.Chance;
                if (randomValue <= currentProbability)
                {
                    wonItem = item;
                    break;
                }
            }

            if (wonItem != null)
            {
                playerData.Inventory.Add(wonItem);
                player.PrintToChat($" [Case System] Congratulations! You won: {wonItem.Name}!");
            }

            SavePlayerData();
        }

        [CommandHelper(minArgs: 0, usage: "!balance")]
        private void CommandBalance(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            string steamId = player.SteamID.ToString();
            var playerData = PlayerDatabase[steamId];
            player.PrintToChat($" [Case System] Your balance: ${playerData.Balance}");
        }

        [CommandHelper(minArgs: 0, usage: "!inventory")]
        private void CommandInventory(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            string steamId = player.SteamID.ToString();
            var playerData = PlayerDatabase[steamId];

            var menu = new ChatMenu("Your Inventory");
            foreach (var item in playerData.Inventory)
            {
                menu.AddMenuOption($"{item.Name} (${item.Price})", (p, o) => { });
            }

            ChatMenus.OpenMenu(player, menu);
        }

        [CommandHelper(minArgs: 2, usage: "!addbalance <player> <amount>")]
        private void CommandAddBalance(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            if (!AdminManager.PlayerHasPermissions(player, "@css/root"))
            {
                player.PrintToChat(" [Case System] You don't have permission to use this command.");
                return;
            }

            var targetPlayer = command.GetArg(1);
            if (!decimal.TryParse(command.GetArg(2), out decimal amount))
            {
                player.PrintToChat(" [Case System] Invalid amount specified.");
                return;
            }

            foreach (var p in Server.PlayersInfo)
            {
                if (p.PlayerName.Contains(targetPlayer, StringComparison.OrdinalIgnoreCase))
                {
                    string steamId = p.SteamID.ToString();
                    PlayerDatabase[steamId].Balance += amount;
                    SavePlayerData();
                    player.PrintToChat($" [Case System] Added ${amount} to {p.PlayerName}'s balance.");
                    return;
                }
            }

            player.PrintToChat(" [Case System] Player not found.");
        }
    }
}
