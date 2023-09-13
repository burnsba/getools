using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Extended DebugMenuCommand.
    /// </summary>
    public record DebugMenuCommandX : EnumModelBase
    {
        public static DebugMenuCommandX PlayTitle { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PlayTitle,
            DisplayOrder = 6,
            Name = "play title",
        };

        public static DebugMenuCommandX BondDie { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.BondDie,
            DisplayOrder = 7,
            Name = "bond die",
        };

        public static DebugMenuCommandX Invincible { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Invincible,
            DisplayOrder = 14,
            Name = "invincible",
        };

        public static DebugMenuCommandX Visible { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Visible,
            DisplayOrder = 15,
            Name = "visible",
        };

        public static DebugMenuCommandX Collisions { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Collisions,
            DisplayOrder = 16,
            Name = "collisions",
        };

        public static DebugMenuCommandX AllGuns { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.AllGuns,
            DisplayOrder = 17,
            Name = "all guns",
        };

        public static DebugMenuCommandX MaxAmmo { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.MaxAmmo,
            DisplayOrder = 18,
            Name = "max ammo",
        };

        public static DebugMenuCommandX DisplaySpeed { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.DisplaySpeed,
            DisplayOrder = 19,
            Name = "display speed",
        };

        public static DebugMenuCommandX Background { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Background,
            DisplayOrder = 20,
            Name = "background",
        };

        public static DebugMenuCommandX Props { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Props,
            DisplayOrder = 21,
            Name = "props",
        };

        public static DebugMenuCommandX StanHit { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.StanHit,
            DisplayOrder = 22,
            Name = "stan hit",
        };

        public static DebugMenuCommandX StanRegion { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.StanRegion,
            DisplayOrder = 23,
            Name = "stan region",
        };

        public static DebugMenuCommandX StanProblems { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.StanProblems,
            DisplayOrder = 24,
            Name = "stan problems",
        };

        public static DebugMenuCommandX PrintManPos { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PrintManPos,
            DisplayOrder = 25,
            Name = "print man pos",
        };

        public static DebugMenuCommandX PortClose { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PortClose,
            DisplayOrder = 26,
            Name = "port close",
        };

        public static DebugMenuCommandX PortInf { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PortInf,
            DisplayOrder = 27,
            Name = "port inf",
        };

        public static DebugMenuCommandX PortApprox { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PortApprox,
            DisplayOrder = 28,
            Name = "port approx",
        };

        public static DebugMenuCommandX PrRoomLoads { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.PrRoomLoads,
            DisplayOrder = 29,
            Name = "pr room loads",
        };

        public static DebugMenuCommandX ShowMemUse { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ShowMemUse,
            DisplayOrder = 30,
            Name = "show mem use",
        };

        public static DebugMenuCommandX ShowMemBars { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ShowMemBars,
            DisplayOrder = 31,
            Name = "show mem bars",
        };

        public static DebugMenuCommandX GrabRgb { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.GrabRgb,
            DisplayOrder = 32,
            Name = "grab rgb",
        };

        public static DebugMenuCommandX GrabJpeg { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.GrabJpeg,
            DisplayOrder = 33,
            Name = "grab jpeg",
        };

        public static DebugMenuCommandX GrabTask { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.GrabTask,
            DisplayOrder = 34,
            Name = "grab task",
        };

        public static DebugMenuCommandX RecordRamrom0 { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.RecordRamrom0,
            DisplayOrder = 36,
            Name = "record ramrom 0",
        };

        public static DebugMenuCommandX RecordRamrom1 { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.RecordRamrom1,
            DisplayOrder = 37,
            Name = "record ramrom 1",
        };

        public static DebugMenuCommandX RecordRamrom2 { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.RecordRamrom2,
            DisplayOrder = 38,
            Name = "record ramrom 2",
        };

        public static DebugMenuCommandX RecordRamrom3 { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.RecordRamrom3,
            DisplayOrder = 39,
            Name = "record ramrom 3",
        };

        public static DebugMenuCommandX ReplayRamrom { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ReplayRamrom,
            DisplayOrder = 40,
            Name = "replay ramrom",
        };

        public static DebugMenuCommandX SaveRamrom { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.SaveRamrom,
            DisplayOrder = 41,
            Name = "save ramrom",
        };

        public static DebugMenuCommandX LoadRamrom { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.LoadRamrom,
            DisplayOrder = 42,
            Name = "load ramrom",
        };

        public static DebugMenuCommandX AutoYAim { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.AutoYAim,
            DisplayOrder = 43,
            Name = "auto y aim",
        };

        public static DebugMenuCommandX AutoXAim { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.AutoXAim,
            DisplayOrder = 44,
            Name = "auto x aim",
        };

        public static DebugMenuCommandX Command007 { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Command007,
            DisplayOrder = 45,
            Name = "007",
        };

        public static DebugMenuCommandX Agent { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Agent,
            DisplayOrder = 46,
            Name = "agent",
        };

        public static DebugMenuCommandX All { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.All,
            DisplayOrder = 47,
            Name = "all",
        };

        public static DebugMenuCommandX Fast { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Fast,
            DisplayOrder = 48,
            Name = "fast",
        };

        public static DebugMenuCommandX Objectives { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Objectives,
            DisplayOrder = 49,
            Name = "objectives",
        };

        public static DebugMenuCommandX ShowPatrols { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ShowPatrols,
            DisplayOrder = 57,
            Name = "show patrols",
        };

        public static DebugMenuCommandX Intro { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Intro,
            DisplayOrder = 58,
            Name = "intro",
        };

        public static DebugMenuCommandX WorldPos { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.WorldPos,
            DisplayOrder = 61,
            Name = "world pos",
        };

        public static DebugMenuCommandX VisCvg { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.VisCvg,
            DisplayOrder = 63,
            Name = "vis cvg",
        };

        public static DebugMenuCommandX ChrNum { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ChrNum,
            DisplayOrder = 64,
            Name = "chr num",
        };

        public static DebugMenuCommandX Profile { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Profile,
            DisplayOrder = 66,
            Name = "profile",
        };

        public static DebugMenuCommandX ObjLoad { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ObjLoad,
            DisplayOrder = 67,
            Name = "obj load",
        };

        public static DebugMenuCommandX WeaponLoad { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.WeaponLoad,
            DisplayOrder = 68,
            Name = "weapon load",
        };

        public static DebugMenuCommandX Joy2SkyEdit { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Joy2SkyEdit,
            DisplayOrder = 69,
            Name = "joy2 sky edit",
        };

        public static DebugMenuCommandX Joy2HitsEdit { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Joy2HitsEdit,
            DisplayOrder = 70,
            Name = "joy2 hits edit",
        };

        public static DebugMenuCommandX Joy2DetailEdit { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.Joy2DetailEdit,
            DisplayOrder = 71,
            Name = "joy2 detail edit",
        };

        public static DebugMenuCommandX ExplosionInfo { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.ExplosionInfo,
            DisplayOrder = 72,
            Name = "explosion info",
        };

        public static DebugMenuCommandX GunWatchPos { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.GunWatchPos,
            DisplayOrder = 74,
            Name = "gun watch pos",
        };

        public static DebugMenuCommandX TestingManPos { get; } = new DebugMenuCommandX()
        {
            Id = (int)DebugMenuCommand.TestingManPos,
            DisplayOrder = 75,
            Name = "testing man pos",
        };

        public static List<DebugMenuCommandX> ValidCommands { get; } = new List<DebugMenuCommandX>()
        {
            PlayTitle,
            BondDie,
            Invincible,
            Visible,
            Collisions,
            AllGuns,
            MaxAmmo,
            DisplaySpeed,
            Background,
            Props,
            StanHit,
            StanRegion,
            StanProblems,
            PrintManPos,
            PortApprox,
            PrRoomLoads,
            ShowMemUse,
            ShowMemBars,
            RecordRamrom0,
            RecordRamrom1,
            RecordRamrom2,
            RecordRamrom3,
            ReplayRamrom,
            SaveRamrom,
            LoadRamrom,
            AutoYAim,
            AutoXAim,
            Command007,
            Agent,
            All,
            Fast,
            Objectives,
            ShowPatrols,
            Intro,
            WorldPos,
            VisCvg,
            ChrNum,
            Profile,
            ObjLoad,
            WeaponLoad,
            Joy2SkyEdit,
            Joy2HitsEdit,
            Joy2DetailEdit,
            ExplosionInfo,
            GunWatchPos,
            TestingManPos,
        };
    }
}
