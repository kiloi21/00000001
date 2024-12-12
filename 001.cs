using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Players;

namespace HitmanPlugin
{
    [Plugin("HitmanPlugin", "1.0", "Hitman game mode plugin")]
    public class HitmanPlugin : PluginBase
    {
        private const string HITMAN_ROLE = "Hitman";
        private const string AGENT_ROLE = "Agent";

        private Dictionary<Player, Player> _hitmanDisguises = new Dictionary<Player, Player>();

        [Command("hitman", "Become a hitman")]
        public void BecomeHitman(CommandContext context)
        {
            var player = context.Player;
            if (player.Role != HITMAN_ROLE)
            {
                player.Role = HITMAN_ROLE;
                player.Inventory.Add("m4a1")
                player.Inventory.Add("usp");
                player.Inventory.Add("knife");
                player.Inventory.Add("smokegrenade");
                player.Inventory.Add("c4");
            }
        }

        [Command("agent", "Become an agent")]
        public void BecomeAgent(CommandContext context)
        {
            var player = context.Player;
            if (player.Role != AGENT_ROLE)
            {
                player.Role = AGENT_ROLE;
                player.Inventory.Add("m4a1");
                player.Inventory.Add("usp");
                player.Inventory.Add("knife");
                player.Inventory.Add("smokegrenade");
            }
        }

        [Hook("PlayerUse")]
        public void OnPlayerUse(PlayerUseEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var target = e.Target as Player;
                if (target != null && target.IsDead)
                {
                    _hitmanDisguises[player] = target;
                    player.Model = target.Model;
                }
            }
        }

        [Hook("PlayerSecondaryAttack")]
        public void OnPlayerSecondaryAttack(PlayerSecondaryAttackEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var entity = e.Entity;
                if (entity is Prop || entity is Ragdoll)
                {
                    player.PickupEntity(entity);
                }
            }
        }

        [Hook("GrenadeExplode")]
        public void OnGrenadeExplode(GrenadeExplodeEventArgs e)
        {
            var grenade = e.Grenade;
            if (grenade.Owner.Role == HITMAN_ROLE)
            {
                grenade.Effect = "hitman_grenade";
            }
        }

        [Hook("BombDefuse")]
        public void OnBombDefuse(BombDefuseEventArgs e)
        {
            var bomb = e.Bomb;
            if (bomb.Owner.Role == HITMAN_ROLE)
            {
                bomb.Detonate();
            }
        }

        [Hook("PlayerShoot")]
        public void OnPlayerShoot(PlayerShootEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var target = e.Target as C4;
                if (target != null)
                {
                    target.Detonate();
                }
            }
        }

        [Hook("PlayerMove")]
        public void OnPlayerMove(PlayerMoveEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var target = e.Target as C4;
                if (target != null)
                {
                    target.Detonate();
                }
            }
        }

        [Hook("PlayerSpawn")]
        public void OnPlayerSpawn(PlayerSpawnEventArgs e)
        {
            var player = e.Player;
            if (player.Role == AGENT_ROLE)
            {
                player.Accuracy = 1f;
            }
        }

        [Hook("PlayerUpdate")]
        public void OnPlayerUpdate(PlayerUpdateEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var target = _hitmanDisguises[player];
                if (target != null)
                {
                    player.Model = target.Model;
                }
            }
        }

        [Hook("PlayerWallbang")]
        public void OnPlayerWallbang(PlayerWallbangEventArgs e)
        {
            var player = e.Player;
            if (player.Role == HITMAN_ROLE)
            {
                var target = e.Target as Player;
                if (target != null)
                {
                    player.Wallbang(target);
                }
            }
        }
    }
}
