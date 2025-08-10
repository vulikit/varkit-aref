using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace varkit_aref
{
    public partial class varkit_aref : BasePlugin, IPluginConfig<ArefConfig>
    {
        public override string ModuleName => "varkit-aref";
        public override string ModuleVersion => "1.0.0";
        public override string ModuleAuthor => "varkit";
        public ArefConfig Config { get; set; }
        public string prefix { get; set; }
        public bool GameActive = false;
        public Timer dtim;
        public bool OnlyHS = false;
        public bool UnlimitedAmmo = false;

        public override void Load(bool hotReload)
        {
            Console.WriteLine(" ");
            Console.WriteLine("                  _    _ _ ");
            Console.WriteLine("                 | |  (_) |");
            Console.WriteLine(" __   ____ _ _ __| | ___| |_");
            Console.WriteLine(" \\ \\ / / _` | '__| |/ / | __|");
            Console.WriteLine("  \\ V / (_| | |  |   <| | |_");
            Console.WriteLine("   \\_/ \\__,_|_|  |_|\\_\\_|\\__|");
            Console.WriteLine("		>> Version: " + ModuleVersion);
            Console.WriteLine(" ");
        }

        public void OnConfigParsed(ArefConfig config)
        {
            Config = config;
            prefix = config.Prefix.ReplaceColorTags();
        }

        [ConsoleCommand("css_aref")]
        public void CommandAref(CCSPlayerController? caller, CommandInfo command)
        {
            if (!Config.ArefYetki.Any(p => AdminManager.PlayerHasPermissions(caller, p)))
            {
                caller.PrintToChat($" {ChatColors.Red}Yetkiniz bulunmamaktadır.");
                return;
            }
            if (caller == null) return;

            var left = "<img src='https://raw.githubusercontent.com/vulikit/varkit-resources/refs/heads/main/deagleleft.png'>";
            var right = "<img src='https://raw.githubusercontent.com/vulikit/varkit-resources/refs/heads/main/deagleright.png'>";
            WasdMenu menu = new WasdMenu($"{left}<font color='#c655dc'> Aref Menüsü </font>{right}", this);
            menu.AddItem($"<font color='#67dc55'> Aref Başlat </font>", (p, o) => StartAref());
            menu.AddItem($"<font color='#5594dc'> Aref Durdur </font>", (p, o) => StopAref());
            menu.AddItem($"<font color='#d255dc'> Aref Ayarlar </font>", (p, o) => ShowSettingsMenu(p, menu));

            menu.Display(caller, 0);
        }

        private void ShowSettingsMenu(CCSPlayerController player, WasdMenu prevMenu)
        {
            var left = "<img src='https://raw.githubusercontent.com/vulikit/varkit-resources/refs/heads/main/deagleleft.png'>";
            var right = "<img src='https://raw.githubusercontent.com/vulikit/varkit-resources/refs/heads/main/deagleright.png'>";
            WasdMenu menuAyar = new WasdMenu($"{left}<font color='#c655dc'> Aref Ayarları </font>{right}", this);
            menuAyar.AddItem($"<font color='#74dc55'> Sadece Headshot [{CheckOnlyHS()}] </font>", (p, o) =>
            {
                OnlyHS = !OnlyHS;
                p.PrintToChat(prefix + $" {ChatColors.LightRed}Sadece Headshot {ChatColors.White}artık {(OnlyHS ? ChatColors.LightYellow + "açık" : ChatColors.LightRed + "kapalı")}.");
                ShowSettingsMenu(p, prevMenu);
            });

            menuAyar.AddItem($"<font color='#74dc55'> Sınırsız Mermi [{CheckUnlimitedAmmo()}] </font>", (p, o) =>
            {
                UnlimitedAmmo = !UnlimitedAmmo;
                p.PrintToChat(prefix + $" {ChatColors.LightRed}Sınırsız Mermi {ChatColors.White}artık {(UnlimitedAmmo ? ChatColors.LightYellow + "açık" : ChatColors.LightRed + "kapalı")}.");
                ShowSettingsMenu(p, prevMenu);
            });

            menuAyar.PrevMenu = prevMenu;
            menuAyar.Display(player, 0);
        }

        public string CheckOnlyHS()
        {
            return OnlyHS ? "<font color='#16b836'>✔</font>" : "<font color='#b8161b'>✘</font>";
        }

        public string CheckUnlimitedAmmo()
        {
            return UnlimitedAmmo ? "<font color='#16b836'>✔</font>" : "<font color='#b8161b'>✘</font>";
        }

        public void StartAref()
        {
            GameActive = true;
            foreach (var item in Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }))
            {
                item.RemoveWeapons();
                item.GiveNamedItem(CsItem.Deagle);
                SetPlayerInvisible(item);
            }
            Server.ExecuteCommand("mp_teammates_are_enemies 1");
            Server.PrintToChatAll(prefix + $" {ChatColors.LightRed}AREF {ChatColors.White}oyunu başladı!");
            Server.PrintToChatAll(prefix + $" {ChatColors.LightYellow}AREF {ChatColors.White}oyunu başladı!");
            Server.PrintToChatAll(prefix + $" {ChatColors.LightRed}AREF {ChatColors.White}oyunu başladı!");

            dtim = AddTimer(0.2f, () =>
            {
                if (!GameActive && !UnlimitedAmmo) return;
                foreach (var player in Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }))
                {
                    var playerPawn = player.PlayerPawn.Value;
                    if (playerPawn != null && playerPawn.WeaponServices != null)
                    {
                        var weapons = playerPawn.WeaponServices.MyWeapons;
                        if (weapons != null)
                        {
                            foreach (var weaponHandle in weapons)
                            {
                                var weapon = weaponHandle.Value;
                                if (weapon != null && weapon.IsValid && weapon.DesignerName == "weapon_deagle" && UnlimitedAmmo)
                                {
                                    weapon.Clip1 = 999;
                                    weapon.ReserveAmmo[0] = 999;
                                    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_iClip1");
                                    Utilities.SetStateChanged(weapon, "CBasePlayerWeapon", "m_pReserveAmmo");
                                }
                            }
                        }
                    }
                }
            }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT);
        }

        public void StopAref()
        {
            GameActive = false;
            foreach (var item in Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }))
            {
                item.GiveNamedItem(CsItem.Knife);
                SetPlayerVisible(item);
            }
            dtim?.Kill();
            dtim = null;
            Server.ExecuteCommand("mp_teammates_are_enemies 0");
            Server.PrintToChatAll(prefix + $" {ChatColors.LightRed}AREF {ChatColors.White}oyunu bitti!");
        }

        public void SetPlayerInvisible(CCSPlayerController player)
        {
            var playerPawnValue = player.PlayerPawn.Value;
            if (playerPawnValue == null || !playerPawnValue.IsValid)
            {
                Console.WriteLine("Player pawn is not valid.");
                return;
            }

            if (playerPawnValue != null && playerPawnValue.IsValid)
            {
                playerPawnValue.Render = Color.FromArgb(0, 0, 0, 0);
                Utilities.SetStateChanged(playerPawnValue, "CBaseModelEntity", "m_clrRender");
            }

            var activeWeapon = playerPawnValue!.WeaponServices?.ActiveWeapon.Value;
            if (activeWeapon != null && activeWeapon.IsValid)
            {
                activeWeapon.Render = Color.FromArgb(0, 0, 0, 0);
                activeWeapon.ShadowStrength = 0.0f;
                Utilities.SetStateChanged(activeWeapon, "CBaseModelEntity", "m_clrRender");
            }

            var myWeapons = playerPawnValue.WeaponServices?.MyWeapons;
            if (myWeapons != null)
            {
                foreach (var gun in myWeapons)
                {
                    var weapon = gun.Value;
                    if (weapon != null)
                    {
                        weapon.Render = Color.FromArgb(0, 0, 0, 0);
                        weapon.ShadowStrength = 0.0f;
                        Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");
                    }
                }
            }
        }

        public void SetPlayerVisible(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || !player.PawnIsAlive)
                return;

            var playerPawnValue = player.PlayerPawn.Value;
            if (playerPawnValue == null)
                return;

            playerPawnValue.Render = Color.FromArgb(255, 255, 255, 255);
            Utilities.SetStateChanged(playerPawnValue, "CBaseModelEntity", "m_clrRender");
        }

        public void CheckLast()
        {
            AddTimer(0.3f, () =>
            {
                if (Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }).Count() == 1)
                {
                    var user = Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }).FirstOrDefault();
                    StopAref();
                    Server.PrintToChatAll(prefix + $" {ChatColors.LightRed}{user.PlayerName} {ChatColors.White}adlı oyuncu {ChatColors.LightYellow}aref {ChatColors.White}oyununu kazandı.");
                }
            });
        }

        [GameEventHandler]
        public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            if (GameActive)
            {
                CheckLast();
            }
            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult EventRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            GameActive = false;
            foreach (var item in Utilities.GetPlayers().Where(p => p is { TeamNum: 2, PawnIsAlive: true, IsValid: true, IsBot: false, IsHLTV: false }))
            {
                item.GiveNamedItem(CsItem.Knife);
                SetPlayerVisible(item);
            }
            dtim?.Kill();
            dtim = null;
            Server.ExecuteCommand("mp_teammates_are_enemies 0");
            return HookResult.Continue;
        }

        [GameEventHandler]
        public HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
        {
            if (!GameActive) return HookResult.Continue;

            var attacker = @event.Attacker;
            var victim = @event.Userid;

            if (attacker == null || victim == null || !attacker.IsValid || !victim.IsValid) return HookResult.Continue;

            if (OnlyHS && @event.Hitgroup != 1)
            {
                victim.PlayerPawn.Value.Health += @event.DmgHealth;
                victim.PlayerPawn.Value.ArmorValue += @event.DmgArmor;
                Utilities.SetStateChanged(victim.PlayerPawn.Value, "CCSPlayerPawn", "m_iHealth");
                Utilities.SetStateChanged(victim.PlayerPawn.Value, "CCSPlayerPawn", "m_ArmorValue");
            }

            return HookResult.Continue;
        }
    }
}