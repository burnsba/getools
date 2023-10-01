﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.EnumModel;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.Asset.Ramrom
{
    /// <summary>
    /// Demo file.
    /// </summary>
    public class RamromFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RamromFile"/> class.
        /// </summary>
        public RamromFile()
        {
            SaveData = new File.SaveData();
        }

        /// <summary>
        /// Save random seed.
        /// </summary>
        public UInt64 RandomSeed { get; set; }

        /// <summary>
        /// Save other random seed.
        /// </summary>
        public UInt64 Randomizer { get; set; }

        /// <summary>
        /// Level id.
        /// </summary>
        /// <remarks>
        /// Demo file format: s32.
        /// </remarks>
        public LevelId LevelId { get; set; }

        /// <summary>
        /// Difficulty.
        /// </summary>
        /// <remarks>
        /// Demo file format: s32.
        /// </remarks>
        public Difficulty Difficulty { get; set; }

        /// <summary>
        /// Number of <see cref="Blockbuf"/> in each capture iteration.
        /// </summary>
        public Int32 SizeCommands { get; set; }

        /// <summary>
        /// File save data.
        /// </summary>
        public File.SaveData SaveData { get; set; }

        /// <summary>
        /// Unused word alignment. <see cref="SaveData"/> ends on half a word.
        /// </summary>
        public short Padding { get; set; }

        /// <summary>
        /// Demo play time.
        /// </summary>
        public Int32 TotalTimeMs { get; set; }

        /// <summary>
        /// Demo size.
        /// </summary>
        public Int32 FileSize { get; set; }

        /// <summary>
        /// Game mode.
        /// </summary>
        /// <remarks>
        /// Demo file format: s32.
        /// </remarks>
        public GameMode Mode { get; set; }

        /// <summary>
        /// Slot number.
        /// </summary>
        public UInt32 SlotNumber { get; set; }

        /// <summary>
        /// Number players.
        /// </summary>
        public UInt32 NumberPlayers { get; set; }

        /// <summary>
        /// Scenario.
        /// </summary>
        /// <remarks>
        /// Probably enum MPSCENARIOS.
        /// </remarks>
        public UInt32 Scenario { get; set; }

        /// <summary>
        /// Multiplayer stage sel.
        /// </summary>
        public UInt32 MultiplayerStageSel { get; set; }

        /// <summary>
        /// Game length.
        /// </summary>
        public UInt32 GameLength { get; set; }

        /// <summary>
        /// Multiplayer weapon set.
        /// </summary>
        public UInt32 MultiplayerWeaponSet { get; set; }

        /// <summary>
        /// Character selected ?.
        /// </summary>
        public UInt32[] MultiplayerChar { get; set; } = new UInt32[4];

        /// <summary>
        /// Handicap ?.
        /// </summary>
        public UInt32[] MultiplayerHandicap { get; set; } = new UInt32[4];

        /// <summary>
        /// Control style ?.
        /// </summary>
        public UInt32[] MultiplayerControlStyle { get; set; } = new UInt32[4];

        /// <summary>
        /// Aim option.
        /// </summary>
        public UInt32 AimOption { get; set; }

        /// <summary>
        /// Multiplayer flags ?.
        /// </summary>
        public UInt32[] MultiplayerFlags { get; set; } = new UInt32[4];

        /// <summary>
        /// Not sure if unused word, or alignment.
        /// </summary>
        public UInt32 Padding2 { get; set; }

        /// <summary>
        /// Replay iteration data.
        /// </summary>
        public List<CaptureIteration> Iterations { get; set; } = new List<CaptureIteration>();
    }
}
