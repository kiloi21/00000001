using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Entities;
using System.Collections.Generic;

namespace HitmanMode
{
    public class HitmanPlugin : BasePlugin
    {
        public override string ModuleName => "Hitman Mode";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => ".....";

        private Dictionary<CCSPlayerController, bool> hitmanPlayers = new Dictionary<CCSPlayerController, bool>();
        private Dictionary<CCSPlayerController, CCSPlayerController> hitmanTargets = new Dictionary<CCSPlayerController, CCSPlayerController>();

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventButtonPress>(OnButtonPress);

            AddCommand("css_hitman", "Toggle Hitman mode", CommandHitman);
        }

        private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
        {
            var player = @event.Player;
            if (player == null || !player.IsValid || !hitmanPlayers.ContainsKey(player)) return HookResult.Continue;

            // Remove weapon spread for Hitman
            foreach (var weapon in player.PlayerPawn.Value.WeaponServices.MyWeapons)
            {
                if (weapon == null) continue;
                weapon.Spread = 0.0f;
            }

            return HookResult.Continue;
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            var victim = @event.Victim;
            var attacker = @event.Attacker;

            if (victim == null || attacker == null) return HookResult.Continue;

            // Handle disguise mechanics
            if (hitmanPlayers.ContainsKey(attacker))
            {
                Server.PrintToChatAll($" {ChatColors.Red}Hitman {ChatColors.Default}can now disguise as their victim!");
            }

            return HookResult.Continue;
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            // Assign random targets to hitmen
            foreach (var hitman in hitmanPlayers.Keys)
            {
                AssignRandomTarget(hitman);
            }

            return HookResult.Continue;
        }

        private HookResult OnButtonPress(EventButtonPress @event, GameEventInfo info)
        {
            var player = @event.Player;
            if (player == null || !hitmanPlayers.ContainsKey(player)) return HookResult.Continue;

            // Handle E key press for disguise
            if (@event.Button == PlayerButtons.Use)
            {
                HandleDisguise(player);
            }

            // Handle right-click for prop movement
            if (@event.Button == PlayerButtons.Attack2)
            {
                HandlePropMovement(player);
            }

            return HookResult.Continue;
        }

        private void HandleDisguise(CCSPlayerController player)
        {
            // Find nearby dead players and apply disguise
            var nearbyCorpse = FindNearestCorpse(player);
            if (nearbyCorpse != null)
            {
                // Apply disguise effect
                Server.PrintToChat(player, $"{ChatColors.Green}You are now disguised!");
            }
        }

        private void HandlePropMovement(CCSPlayerController player)
        {
            // Implement prop movement logic
            var nearbyProp = FindNearestProp(player);
            if (nearbyProp != null)
            {
                // Allow prop movement
                Server.PrintToChat(player, $"{ChatColors.Green}Moving prop...");
            }
        }

        private CCSPlayerController FindNearestCorpse(CCSPlayerController player)
        {
            // Implementation to find nearest dead player
            return null;
        }

        private object FindNearestProp(CCSPlayerController player)
        {
            // Implementation to find nearest prop
            return null;
        }

        private void AssignRandomTarget(CCSPlayerController hitman)
        {
            var possibleTargets = Utilities.GetPlayers().Where(p => p.IsValid && p != hitman && !hitmanPlayers.ContainsKey(p));
            var target = possibleTargets.RandomElement();

            if (target != null)
            {
                hitmanTargets[hitman] = target;
                Server.PrintToChat(hitman, $"{ChatColors.Red}Your target is: {target.PlayerName}");
            }
        }

        [CommandCallback("css_hitman")]
        public void CommandHitman(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            if (hitmanPlayers.ContainsKey(player))
            {
                hitmanPlayers.Remove(player);
                Server.PrintToChat(player, $"{ChatColors.Red}You are no longer a Hitman!");
            }
            else
            {
                hitmanPlayers[player] = true;
                Server.PrintToChat(player, $"{ChatColors.Green}You are now a Hitman!");
                AssignRandomTarget(player);
            }
        }
    }
}
