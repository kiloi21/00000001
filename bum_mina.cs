using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Entities;
using System.Runtime.InteropServices;

namespace VIPBumpMine;

public class VIPBumpMinePlugin : BasePlugin
{
    public override string ModuleName => "VIP Bump Mine";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "YourName";

    private const string VIP_FLAG = "@css/vip"; // VIP flag
    private const float BUMP_FORCE = 1000.0f; // Force of the bump
    private const float COOLDOWN_TIME = 3.0f; // Cooldown in seconds
    private Dictionary<int, DateTime> lastUsageTime = new Dictionary<int, DateTime>();

    public override void Load(bool hotReload)
    {
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        AddCommand("css_bumpmine", "Use Bump Mine (VIP Only)", CommandBumpMine);
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player == null || !player.IsValid || !player.PlayerPawn.IsValid) return HookResult.Continue;

        if (AdminManager.PlayerHasPermissions(player, VIP_FLAG))
        {
            player.PrintToChat($" {ChatColors.Green}[VIP] {ChatColors.Default}Type {ChatColors.Red}!bumpmine {ChatColors.Default}to use Bump Mine!");
        }

        return HookResult.Continue;
    }

    private void CommandBumpMine(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null || !player.IsValid || !player.PlayerPawn.IsValid) return;

        // Check if player is VIP
        if (!AdminManager.PlayerHasPermissions(player, VIP_FLAG))
        {
            player.PrintToChat($" {ChatColors.Red}This feature is only available for VIP players!");
            return;
        }

        // Check if player is alive
        if (!player.PawnIsAlive)
        {
            player.PrintToChat($" {ChatColors.Red}You must be alive to use Bump Mine!");
            return;
        }

        int userId = player.UserId!.Value;

        // Check cooldown
        if (lastUsageTime.ContainsKey(userId))
        {
            var timeSinceLastUse = DateTime.Now - lastUsageTime[userId];
            if (timeSinceLastUse.TotalSeconds < COOLDOWN_TIME)
            {
                player.PrintToChat($" {ChatColors.Red}Please wait {(COOLDOWN_TIME - timeSinceLastUse.TotalSeconds):F1} seconds before using Bump Mine again!");
                return;
            }
        }

        // Apply bump force
        var playerPawn = player.PlayerPawn.Value;
        if (playerPawn != null && playerPawn.IsValid)
        {
            Vector velocity = playerPawn.AbsVelocity;
            velocity.Z = BUMP_FORCE;
            playerPawn.AbsVelocity = velocity;

            // Play sound (if available in CS2)
            // Server.ExecuteCommand($"play physics/metal/metal_box_impact_hard3.wav");

            // Update cooldown
            lastUsageTime[userId] = DateTime.Now;

            player.PrintToChat($" {ChatColors.Green}[VIP] {ChatColors.Default}Bump Mine used!");
        }
    }

    public override void Unload(bool hotReload)
    {
        lastUsageTime.Clear();
    }
}
