using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;

namespace HitmanMode
{
    public class HitmanPlugin : BasePlugin
    {
        public override string ModuleName => "HitmanMode";
        public override string ModuleAuthor => "....";
        public override string ModuleVersion => "1.0";

        public override void Load(bool hotReload)
        {
            RegisterEventListener<EventPlayerSpawn>(OnPlayerSpawn);
            RegisterEventListener<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventListener<EventPlayerUse>(OnPlayerUse);
            RegisterEventListener<EventPlayerAttack>(OnPlayerAttack);
            RegisterEventListener<EventPlayerHurt>(OnPlayerHurt);
            RegisterEventListener<EventEntityCreated>(OnEntityCreated);
        }

        private void OnPlayerSpawn(EventPlayerSpawn @event, string name)
        {
            var player = @event.Userid;
            if (player.Team == CsTeam.T)
            {
                player.GiveNamedItem("weapon_c4");
                player.GiveNamedItem("weapon_hegrenade");
            }
            else if (player.Team == CsTeam.CT)
            {
                player.GiveNamedItem("weapon_deagle");
                player.GiveNamedItem("weapon_m4a1");
                player.GiveNamedItem("weapon_hegrenade");
                player.GiveNamedItem("weapon_flashbang");
                player.GiveNamedItem("weapon_smokegrenade");
            }
        }

        private void OnPlayerDeath(EventPlayerDeath @event, string name)
        {
            var victim = @event.Userid;
            var attacker = @event.Attacker;
            if (attacker != null && attacker.Team == CsTeam.T)
            {
                attacker.PrintToChat("You can disguise as the killed player by pressing USE (E).");
            }
        }

        private void OnPlayerUse(EventPlayerUse @event, string name)
        {
            var player = @event.Userid;
            if (player.Team == CsTeam.T)
            {
                var target = player.EyePosition.FindEntitiesInSphere(50).FirstOrDefault(entity => entity is CCSPlayerController && entity.IsAlive == false);
                if (target != null)
                {
                    player.DisguiseAs(target);
                }
            }
        }

        private void OnPlayerAttack(EventPlayerAttack @event, string name)
        {
            var player = @event.Userid;
            if (player.Team == CsTeam.T && player.ActiveWeapon.Classname == "weapon_c4")
            {
                var c4 = player.ActiveWeapon;
                c4.SetKeyValue("m_flC4Blow", "9999");
                c4.SetKeyValue("m_bStartedArming", "1");
                c4.SetKeyValue("m_hOwnerEntity", player.Handle.ToString());
                c4.Teleport(player.EyePosition + player.EyeDirection * 50, player.EyeAngles, null);
            }
        }

        private void OnPlayerHurt(EventPlayerHurt @event, string name)
        {
            var victim = @event.Userid;
            var attacker = @event.Attacker;
            if (attacker != null && attacker.Team == CsTeam.CT)
            {
                attacker.WeaponSpreadScale = 0;
            }
        }

        private void OnEntityCreated(EventEntityCreated @event, string name)
        {
            var entity = @event.Entity;
            if (entity.Classname == "weapon_hegrenade" && entity.Owner.Team == CsTeam.T)
            {
                entity.GlowManager.AddEffect(new GlowEffect
                {
                    Color = new Color(255, 0, 0),
                    RenderWhenOccluded = true,
                    RenderWhenUnoccluded = false
                });
            }
        }
    }
}
