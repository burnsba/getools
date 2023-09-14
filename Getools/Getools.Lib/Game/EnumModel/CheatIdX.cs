using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Extended enum CheatId.
    /// </summary>
    /// <remarks>
    /// typedef enum CHEAT_ID
    /// </remarks>
    public record CheatIdX : EnumModelBase
    {
        public static CheatIdX Unused { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unused,
            DisplayOrder = 0,
            Name = "Unused",
        };

        public static CheatIdX ExtraMpChars { get; } = new CheatIdX()
        {
            Id = (int)CheatId.ExtraMpChars,
            DisplayOrder = 1,
            Name = "Extra Multiplayer Chars",
        };

        public static CheatIdX Invincibility { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Invincibility,
            DisplayOrder = 2,
            Name = "Invincibility",
        };

        public static CheatIdX Allguns { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Allguns,
            DisplayOrder = 3,
            Name = "All guns",
        };

        public static CheatIdX Maxammo { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Maxammo,
            DisplayOrder = 4,
            Name = "Max ammo",
        };

        public static CheatIdX DebugReturnSavedRa { get; } = new CheatIdX()
        {
            Id = (int)CheatId.DebugReturnSavedRa,
            DisplayOrder = 5,
            Name = "Debug Return Saved Ra",
        };

        public static CheatIdX DeactivateInvincibility { get; } = new CheatIdX()
        {
            Id = (int)CheatId.DeactivateInvincibility,
            DisplayOrder = 6,
            Name = "Deactivate Invincibility",
        };

        public static CheatIdX Linemode { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Linemode,
            DisplayOrder = 7,
            Name = "Linemode",
        };

        public static CheatIdX Cheat2xHealth { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xHealth,
            DisplayOrder = 8,
            Name = "Cheat 2x Health",
        };

        public static CheatIdX Cheat2xArmor { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xArmor,
            DisplayOrder = 9,
            Name = "Cheat 2x Armor",
        };

        public static CheatIdX Invisibility { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Invisibility,
            DisplayOrder = 10,
            Name = "Invisibility",
        };

        public static CheatIdX InfiniteAmmo { get; } = new CheatIdX()
        {
            Id = (int)CheatId.InfiniteAmmo,
            DisplayOrder = 11,
            Name = "Infinite Ammo",
        };

        public static CheatIdX DkMode { get; } = new CheatIdX()
        {
            Id = (int)CheatId.DkMode,
            DisplayOrder = 12,
            Name = "DK Mode",
        };

        public static CheatIdX ExtraWeapons { get; } = new CheatIdX()
        {
            Id = (int)CheatId.ExtraWeapons,
            DisplayOrder = 13,
            Name = "Extra Weapons",
        };

        public static CheatIdX TinyBond { get; } = new CheatIdX()
        {
            Id = (int)CheatId.TinyBond,
            DisplayOrder = 14,
            Name = "Tiny Bond",
        };

        public static CheatIdX Paintball { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Paintball,
            DisplayOrder = 15,
            Name = "Paintball",
        };

        public static CheatIdX Cheat10xHealth { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat10xHealth,
            DisplayOrder = 16,
            Name = "Cheat 10x Health",
        };

        public static CheatIdX Magnum { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Magnum,
            DisplayOrder = 17,
            Name = "Magnum",
        };

        public static CheatIdX Laser { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Laser,
            DisplayOrder = 18,
            Name = "Laser",
        };

        public static CheatIdX GoldenGun { get; } = new CheatIdX()
        {
            Id = (int)CheatId.GoldenGun,
            DisplayOrder = 19,
            Name = "Golden Gun",
        };

        public static CheatIdX SilverPp7 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.SilverPp7,
            DisplayOrder = 20,
            Name = "Silver Pp7",
        };

        public static CheatIdX GoldPp7 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.GoldPp7,
            DisplayOrder = 21,
            Name = "Gold Pp7",
        };

        public static CheatIdX InvisibilityMp { get; } = new CheatIdX()
        {
            Id = (int)CheatId.InvisibilityMp,
            DisplayOrder = 22,
            Name = "Invisibility Multiplayer",
        };

        public static CheatIdX NoRadarMp { get; } = new CheatIdX()
        {
            Id = (int)CheatId.NoRadarMp,
            DisplayOrder = 23,
            Name = "No Radar Multiplayer",
        };

        public static CheatIdX TurboMode { get; } = new CheatIdX()
        {
            Id = (int)CheatId.TurboMode,
            DisplayOrder = 24,
            Name = "Turbo Mode",
        };

        public static CheatIdX DebugPos { get; } = new CheatIdX()
        {
            Id = (int)CheatId.DebugPos,
            DisplayOrder = 25,
            Name = "Debug Pos",
        };

        public static CheatIdX FastAnimation { get; } = new CheatIdX()
        {
            Id = (int)CheatId.FastAnimation,
            DisplayOrder = 26,
            Name = "Fast Animation",
        };

        public static CheatIdX SlowAnimation { get; } = new CheatIdX()
        {
            Id = (int)CheatId.SlowAnimation,
            DisplayOrder = 27,
            Name = "Slow Animation",
        };

        public static CheatIdX EnemyRockets { get; } = new CheatIdX()
        {
            Id = (int)CheatId.EnemyRockets,
            DisplayOrder = 28,
            Name = "Enemy Rockets",
        };

        public static CheatIdX Cheat2xRocketLauncher { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xRocketLauncher,
            DisplayOrder = 29,
            Name = "Cheat 2x Rocket Launcher",
        };

        public static CheatIdX Cheat2xGrenadeLauncher { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xGrenadeLauncher,
            DisplayOrder = 30,
            Name = "Cheat 2x Grenade Launcher",
        };

        public static CheatIdX Cheat2xRcp90 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xRcp90,
            DisplayOrder = 31,
            Name = "Cheat 2x Rcp90",
        };

        public static CheatIdX Cheat2xThrowingKnife { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xThrowingKnife,
            DisplayOrder = 32,
            Name = "Cheat 2x Throwing Knife",
        };

        public static CheatIdX Cheat2xHuntingKnife { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xHuntingKnife,
            DisplayOrder = 33,
            Name = "Cheat 2x Hunting Knife",
        };

        public static CheatIdX Cheat2xLaser { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Cheat2xLaser,
            DisplayOrder = 34,
            Name = "Cheat 2x Laser",
        };

        public static CheatIdX UnlockPaintball { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockPaintball,
            DisplayOrder = 35,
            Name = "Unlock Paintball",
        };

        public static CheatIdX UnlockInvincible { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockInvincible,
            DisplayOrder = 36,
            Name = "Unlock Invincible",
        };

        public static CheatIdX UnlockDkmode { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockDkmode,
            DisplayOrder = 37,
            Name = "Unlock DK mode",
        };

        public static CheatIdX Unlock2xgl { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xgl,
            DisplayOrder = 38,
            Name = "Unlock 2x grenade launcher",
        };

        public static CheatIdX Unlock2xrl { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xrl,
            DisplayOrder = 39,
            Name = "Unlock 2x rocket launcher",
        };

        public static CheatIdX UnlockTurbo { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockTurbo,
            DisplayOrder = 40,
            Name = "Unlock Turbo Mode",
        };

        public static CheatIdX UnlockNoradar { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockNoradar,
            DisplayOrder = 41,
            Name = "Unlock Noradar",
        };

        public static CheatIdX UnlockTiny { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockTiny,
            DisplayOrder = 42,
            Name = "Unlock Tiny Bond",
        };

        public static CheatIdX Unlock2xtknife { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xtknife,
            DisplayOrder = 43,
            Name = "Unlock 2x throwing knife",
        };

        public static CheatIdX UnlockFast { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockFast,
            DisplayOrder = 44,
            Name = "Unlock Fast",
        };

        public static CheatIdX UnlockInvis { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockInvis,
            DisplayOrder = 45,
            Name = "Unlock Invisibility",
        };

        public static CheatIdX UnlockEnemyrl { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockEnemyrl,
            DisplayOrder = 46,
            Name = "Unlock Enemy Rockets",
        };

        public static CheatIdX UnlockSlow { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockSlow,
            DisplayOrder = 47,
            Name = "Unlock Slow Animation",
        };

        public static CheatIdX UnlockSilverppk { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockSilverppk,
            DisplayOrder = 48,
            Name = "Unlock Silver Ppk",
        };

        public static CheatIdX Unlock2xhknife { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xhknife,
            DisplayOrder = 49,
            Name = "Unlock 2x Hunting Knife",
        };

        public static CheatIdX UnlockInfammo { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockInfammo,
            DisplayOrder = 50,
            Name = "Unlock Infinite ammo",
        };

        public static CheatIdX Unlock2xfnp0 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xfnp0,
            DisplayOrder = 51,
            Name = "Unlock 2x RCP90",
        };

        public static CheatIdX UnlockGoldppk { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockGoldppk,
            DisplayOrder = 52,
            Name = "Unlock Gold ppk",
        };

        public static CheatIdX Unlock2xlaser { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Unlock2xlaser,
            DisplayOrder = 53,
            Name = "Unlock 2x laser",
        };

        public static CheatIdX UnlockAllguns { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockAllguns,
            DisplayOrder = 54,
            Name = "Unlock All guns",
        };

        public static CheatIdX UnlockDam { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockDam,
            DisplayOrder = 55,
            Name = "Unlock Dam",
        };

        public static CheatIdX UnlockFacility { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockFacility,
            DisplayOrder = 56,
            Name = "Unlock Facility",
        };

        public static CheatIdX UnlockRunway { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockRunway,
            DisplayOrder = 57,
            Name = "Unlock Runway",
        };

        public static CheatIdX UnlockSurface { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockSurface,
            DisplayOrder = 58,
            Name = "Unlock Surface 1",
        };

        public static CheatIdX UnlockBunker { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockBunker,
            DisplayOrder = 59,
            Name = "Unlock Bunker 1",
        };

        public static CheatIdX UnlockSilo { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockSilo,
            DisplayOrder = 60,
            Name = "Unlock Silo",
        };

        public static CheatIdX UnlockFrigate { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockFrigate,
            DisplayOrder = 61,
            Name = "Unlock Frigate",
        };

        public static CheatIdX UnlockSurface2 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockSurface2,
            DisplayOrder = 62,
            Name = "Unlock Surface 2",
        };

        public static CheatIdX UnlockBunker2 { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockBunker2,
            DisplayOrder = 63,
            Name = "Unlock Bunker 2",
        };

        public static CheatIdX UnlockStatue { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockStatue,
            DisplayOrder = 64,
            Name = "Unlock Statue",
        };

        public static CheatIdX UnlockArchives { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockArchives,
            DisplayOrder = 65,
            Name = "Unlock Archives",
        };

        public static CheatIdX UnlockStreets { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockStreets,
            DisplayOrder = 66,
            Name = "Unlock Streets",
        };

        public static CheatIdX UnlockDepot { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockDepot,
            DisplayOrder = 67,
            Name = "Unlock Depot",
        };

        public static CheatIdX UnlockTrain { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockTrain,
            DisplayOrder = 68,
            Name = "Unlock Train",
        };

        public static CheatIdX UnlockJungle { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockJungle,
            DisplayOrder = 69,
            Name = "Unlock Jungle",
        };

        public static CheatIdX UnlockControl { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockControl,
            DisplayOrder = 70,
            Name = "Unlock Control",
        };

        public static CheatIdX UnlockCaverns { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockCaverns,
            DisplayOrder = 71,
            Name = "Unlock Caverns",
        };

        public static CheatIdX UnlockCradle { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockCradle,
            DisplayOrder = 72,
            Name = "Unlock Cradle",
        };

        public static CheatIdX UnlockAztec { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockAztec,
            DisplayOrder = 73,
            Name = "Unlock Aztec",
        };

        public static CheatIdX UnlockEgypt { get; } = new CheatIdX()
        {
            Id = (int)CheatId.UnlockEgypt,
            DisplayOrder = 74,
            Name = "Unlock Egypt",
        };

        public static CheatIdX Invalid { get; } = new CheatIdX()
        {
            Id = (int)CheatId.Invalid,
            DisplayOrder = 75,
            Name = "Invalid",
        };

        public static List<CheatIdX> AllRuntime { get; } = new List<CheatIdX>()
        {
            Unused,
            ExtraMpChars,
            Invincibility,
            Allguns,
            Maxammo,
            DebugReturnSavedRa,
            DeactivateInvincibility,
            Linemode,
            Cheat2xHealth,
            Cheat2xArmor,
            Invisibility,
            InfiniteAmmo,
            DkMode,
            ExtraWeapons,
            TinyBond,
            Paintball,
            Cheat10xHealth,
            Magnum,
            Laser,
            GoldenGun,
            SilverPp7,
            GoldPp7,
            InvisibilityMp,
            NoRadarMp,
            TurboMode,
            DebugPos,
            FastAnimation,
            SlowAnimation,
            EnemyRockets,
            Cheat2xRocketLauncher,
            Cheat2xGrenadeLauncher,
            Cheat2xRcp90,
            Cheat2xThrowingKnife,
            Cheat2xHuntingKnife,
            Cheat2xLaser,
        };

        public static List<CheatIdX> AllUnlockRuntime { get; } = new List<CheatIdX>()
        {
            UnlockPaintball,
            UnlockInvincible,
            UnlockDkmode,
            Unlock2xgl,
            Unlock2xrl,
            UnlockTurbo,
            UnlockNoradar,
            UnlockTiny,
            Unlock2xtknife,
            UnlockFast,
            UnlockInvis,
            UnlockEnemyrl,
            UnlockSlow,
            UnlockSilverppk,
            Unlock2xhknife,
            UnlockInfammo,
            Unlock2xfnp0,
            UnlockGoldppk,
            Unlock2xlaser,
            UnlockAllguns,
        };

        public static List<CheatIdX> AllUnlockStage { get; } = new List<CheatIdX>()
        {
            UnlockDam,
            UnlockFacility,
            UnlockRunway,
            UnlockSurface,
            UnlockBunker,
            UnlockSilo,
            UnlockFrigate,
            UnlockSurface2,
            UnlockBunker2,
            UnlockStatue,
            UnlockArchives,
            UnlockStreets,
            UnlockDepot,
            UnlockTrain,
            UnlockJungle,
            UnlockControl,
            UnlockCaverns,
            UnlockCradle,
            UnlockAztec,
            UnlockEgypt,
        };

        public static List<CheatIdX> All { get; } = new List<List<CheatIdX>>()
        {
            AllRuntime,
            AllUnlockRuntime,
            AllUnlockStage,
        }.SelectMany(x => x).ToList();
    }
}
