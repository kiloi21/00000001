using CounterStrikeSharp;
using CounterStrikeSharp.Entities;
using CounterStrikeSharp.Enums;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;



public class HitmanPlugin : BasePlugin
{
    public override string Name => "Hitman Mode";
    public override string Description => "Custom Hitman";
    public override string Author => "merror";
    public override string Version => "1.0";


    private CCSPlayer _hitman;
    private CCSPlayer _target;


    public override void OnPluginStart()
    {
        RegisterEventHandler<RoundStart>(OnRoundStart);
        RegisterEventHandler<PlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<PlayerSpawn>(OnPlayerSpawn);
        RegisterEventHandler<PlayerButtonPress>(OnPlayerButtonPress);
    }



    private void OnRoundStart(RoundStartEventArgs ev)
    {

        List<CCSPlayer> players = Utilities.GetPlayers().Where(p => p.IsAlive()).ToList();
        if (players.Count < 2) return;


        _hitman = players[new Random().Next(players.Count)];
        do
        {
            _target = players[new Random().Next(players.Count)];
        } while (_target == _hitman);


        _hitman.PrintToChat("You are the Hitman! Eliminate {_target.GetName()}");
        _target.PrintToChat("You are the Target! Survive the round.");


        // Agent (Hitman) specific settings
        _hitman.GiveWeapon(WeaponId.Awp); // Or any desired starting weapon
        _hitman.GiveWeapon(WeaponId.Decoy);
        _hitman.GiveWeapon(WeaponId.C4);


        foreach (var weapon in _hitman.Weapons)
        {
            if (weapon != null && weapon.IsValid)
            {
                weapon.Inaccuracy = 0; // No spread
            }
        }
    }


    private void OnPlayerSpawn(PlayerSpawnEventArgs ev)
    {
        if (ev.Player == _hitman)
        {
            // Ensure Hitman settings persist after respawn
            foreach (var weapon in _hitman.Weapons)
            {
                if (weapon != null && weapon.IsValid)
                {
                    weapon.Inaccuracy = 0;
                }
            }
        }
    }


    private void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        if (ev.Victim == _target && ev.Attacker == _hitman)
        {
            Utilities.PrintToChatAll("Hitman eliminated the Target!");
            Server.EndRound(RoundEndReason.TargetBombed); // Or a more suitable reason
        }
        else if (ev.Victim == _hitman)
        {
            Utilities.PrintToChatAll("Hitman was eliminated!");
            Server.EndRound(RoundEndReason.TerroristsWin); // Or a more suitable reason
        }

    }



    private void OnPlayerButtonPress(PlayerButtonPressEventArgs ev)
    {
        if (ev.Player == _hitman)
        {
            if (ev.Button == Button.Use)
            {
                HandleDisguise(ev.Player);
            }
            else if (ev.Button == Button.Attack2)
            {
                // Implement prop/dummy moving logic here (if possible with CS2 API)
                ev.Player.PrintToChat("Prop moving is not yet implemented."); // Placeholder
            }
            else if (ev.Button == Button.Attack)
            {
                HandleC4Plant(ev.Player);
            }
        }
    }



    private void HandleDisguise(CCSPlayer player)
    {
        // Find closest dead player
        CCSPlayer closestDeadPlayer = null;
        float closestDistance = float.MaxValue;


        foreach (var p in Utilities.GetPlayers())
        {
            if (!p.IsAlive() && p != player)
            {
                float distance = player.DistanceTo(p);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDeadPlayer = p;
                }
            }
        }


        if (closestDeadPlayer != null)
        {
            // Implement disguise logic here (e.g., model swap if possible)
            player.PrintToChat("Disguise functionality is not fully implemented yet."); // Placeholder
        }
        else
        {
            player.PrintToChat("No nearby dead players to disguise as.");
        }
    }


    private void HandleC4Plant(CCSPlayer player)
    {
        // Implement C4 trap logic here.  This will likely involve creating a custom entity or using existing entities in a creative way.
        player.PrintToChat("C4 trap functionality is not fully implemented yet."); // Placeholder
    }



}

