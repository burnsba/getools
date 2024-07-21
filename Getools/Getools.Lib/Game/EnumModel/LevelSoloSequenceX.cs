using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Extended enum LevelSoloSequence.
    /// </summary>
    /// <remarks>
    /// typedef enum LEVEL_SOLO_SEQUENCE
    /// </remarks>
    public record LevelSoloSequenceX : EnumModelBase
    {
        /// <summary>
        /// Default / unknown / unset.
        /// </summary>
        public static LevelSoloSequenceX DefaultUnkown { get; } = new LevelSoloSequenceX()
        {
            Id = -1,
            DisplayOrder = 0,
            Name = string.Empty,
        };

        /// <summary>
        /// Dam.
        /// </summary>
        public static LevelSoloSequenceX Dam { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Dam,
            DisplayOrder = 1,
            Name = "Dam",
        };

        /// <summary>
        /// Facility.
        /// </summary>
        public static LevelSoloSequenceX Facility { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Facility,
            DisplayOrder = 2,
            Name = "Facility",
        };

        /// <summary>
        /// Runway.
        /// </summary>
        public static LevelSoloSequenceX Runway { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Runway,
            DisplayOrder = 3,
            Name = "Runway",
        };

        /// <summary>
        /// Surface 1.
        /// </summary>
        public static LevelSoloSequenceX Surface1 { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Surface1,
            DisplayOrder = 4,
            Name = "Surface 1",
        };

        /// <summary>
        /// Bunker 1.
        /// </summary>
        public static LevelSoloSequenceX Bunker1 { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Bunker1,
            DisplayOrder = 5,
            Name = "Bunker 1",
        };

        /// <summary>
        /// Silo.
        /// </summary>
        public static LevelSoloSequenceX Silo { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Silo,
            DisplayOrder = 6,
            Name = "Silo",
        };

        /// <summary>
        /// Frigate.
        /// </summary>
        public static LevelSoloSequenceX Frigate { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Frigate,
            DisplayOrder = 7,
            Name = "Frigate",
        };

        /// <summary>
        /// Surface 2.
        /// </summary>
        public static LevelSoloSequenceX Surface2 { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Surface2,
            DisplayOrder = 8,
            Name = "Surface 2",
        };

        /// <summary>
        /// Bunker 2.
        /// </summary>
        public static LevelSoloSequenceX Bunker2 { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Bunker2,
            DisplayOrder = 9,
            Name = "Bunker 2",
        };

        /// <summary>
        /// Statue.
        /// </summary>
        public static LevelSoloSequenceX Statue { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Statue,
            DisplayOrder = 10,
            Name = "Statue",
        };

        /// <summary>
        /// Archives.
        /// </summary>
        public static LevelSoloSequenceX Archives { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Archives,
            DisplayOrder = 11,
            Name = "Archives",
        };

        /// <summary>
        /// Streets.
        /// </summary>
        public static LevelSoloSequenceX Streets { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Streets,
            DisplayOrder = 12,
            Name = "Streets",
        };

        /// <summary>
        /// Depot.
        /// </summary>
        public static LevelSoloSequenceX Depot { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Depot,
            DisplayOrder = 13,
            Name = "Depot",
        };

        /// <summary>
        /// Train.
        /// </summary>
        public static LevelSoloSequenceX Train { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Train,
            DisplayOrder = 14,
            Name = "Train",
        };

        /// <summary>
        /// Jungle.
        /// </summary>
        public static LevelSoloSequenceX Jungle { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Jungle,
            DisplayOrder = 15,
            Name = "Jungle",
        };

        /// <summary>
        /// Control.
        /// </summary>
        public static LevelSoloSequenceX Control { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Control,
            DisplayOrder = 16,
            Name = "Control",
        };

        /// <summary>
        /// Caverns.
        /// </summary>
        public static LevelSoloSequenceX Caverns { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Caverns,
            DisplayOrder = 17,
            Name = "Caverns",
        };

        /// <summary>
        /// Cradle.
        /// </summary>
        public static LevelSoloSequenceX Cradle { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Cradle,
            DisplayOrder = 18,
            Name = "Cradle",
        };

        /// <summary>
        /// Aztec.
        /// </summary>
        public static LevelSoloSequenceX Aztec { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Aztec,
            DisplayOrder = 19,
            Name = "Aztec",
        };

        /// <summary>
        /// Egypt.
        /// </summary>
        public static LevelSoloSequenceX Egypt { get; } = new LevelSoloSequenceX()
        {
            Id = (int)LevelSoloSequence.Egypt,
            DisplayOrder = 20,
            Name = "Egypt",
        };

        /// <summary>
        /// List of single player stages in order.
        /// </summary>
        public static List<LevelSoloSequenceX> SinglePlayerStages { get; } = new List<LevelSoloSequenceX>()
        {
            Dam,
            Facility,
            Runway,
            Surface1,
            Bunker1,
            Silo,
            Frigate,
            Surface2,
            Bunker2,
            Statue,
            Archives,
            Streets,
            Depot,
            Train,
            Jungle,
            Control,
            Caverns,
            Cradle,
            Aztec,
            Egypt,
        };
    }
}
