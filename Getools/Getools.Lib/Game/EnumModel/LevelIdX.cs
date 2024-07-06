using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Extended enum levelid.
    /// </summary>
    /// <remarks>
    /// typedef enum LEVELID
    /// </remarks>
    public record LevelIdX : EnumModelBase
    {
        /// <summary>
        /// Default / unknown / unset.
        /// </summary>
        public static LevelIdX DefaultUnkown { get; } = new LevelIdX()
        {
            Id = 0,
            DisplayOrder = 0,
            Name = string.Empty,
        };

        /// <summary>
        /// Dam.
        /// </summary>
        public static LevelIdX Dam { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Dam,
            DisplayOrder = 1,
            Name = "Dam",
        };

        /// <summary>
        /// Facility.
        /// </summary>
        public static LevelIdX Facility { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Facility,
            DisplayOrder = 2,
            Name = "Facility",
        };

        /// <summary>
        /// Runway.
        /// </summary>
        public static LevelIdX Runway { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Runway,
            DisplayOrder = 3,
            Name = "Runway",
        };

        /// <summary>
        /// Surface 1.
        /// </summary>
        public static LevelIdX Surface1 { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Surface,
            DisplayOrder = 4,
            Name = "Surface 1",
        };

        /// <summary>
        /// Bunker 1.
        /// </summary>
        public static LevelIdX Bunker1 { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Bunker1,
            DisplayOrder = 5,
            Name = "Bunker 1",
        };

        /// <summary>
        /// Silo.
        /// </summary>
        public static LevelIdX Silo { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Silo,
            DisplayOrder = 6,
            Name = "Silo",
        };

        /// <summary>
        /// Frigate.
        /// </summary>
        public static LevelIdX Frigate { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Frigate,
            DisplayOrder = 7,
            Name = "Frigate",
        };

        /// <summary>
        /// Surface 2.
        /// </summary>
        public static LevelIdX Surface2 { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Surface2,
            DisplayOrder = 8,
            Name = "Surface 2",
        };

        /// <summary>
        /// Bunker 2.
        /// </summary>
        public static LevelIdX Bunker2 { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Bunker2,
            DisplayOrder = 9,
            Name = "Bunker 2",
        };

        /// <summary>
        /// Statue.
        /// </summary>
        public static LevelIdX Statue { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Statue,
            DisplayOrder = 10,
            Name = "Statue",
        };

        /// <summary>
        /// Archives.
        /// </summary>
        public static LevelIdX Archives { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Archives,
            DisplayOrder = 11,
            Name = "Archives",
        };

        /// <summary>
        /// Streets.
        /// </summary>
        public static LevelIdX Streets { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Streets,
            DisplayOrder = 12,
            Name = "Streets",
        };

        /// <summary>
        /// Depot.
        /// </summary>
        public static LevelIdX Depot { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Depot,
            DisplayOrder = 13,
            Name = "Depot",
        };

        /// <summary>
        /// Train.
        /// </summary>
        public static LevelIdX Train { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Train,
            DisplayOrder = 14,
            Name = "Train",
        };

        /// <summary>
        /// Jungle.
        /// </summary>
        public static LevelIdX Jungle { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Jungle,
            DisplayOrder = 15,
            Name = "Jungle",
        };

        /// <summary>
        /// Control.
        /// </summary>
        public static LevelIdX Control { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Control,
            DisplayOrder = 16,
            Name = "Control",
        };

        /// <summary>
        /// Caverns.
        /// </summary>
        public static LevelIdX Caverns { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Caverns,
            DisplayOrder = 17,
            Name = "Caverns",
        };

        /// <summary>
        /// Cradle.
        /// </summary>
        public static LevelIdX Cradle { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Cradle,
            DisplayOrder = 18,
            Name = "Cradle",
        };

        /// <summary>
        /// Aztec.
        /// </summary>
        public static LevelIdX Aztec { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Aztec,
            DisplayOrder = 19,
            Name = "Aztec",
        };

        /// <summary>
        /// Egypt.
        /// </summary>
        public static LevelIdX Egypt { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Egypt,
            DisplayOrder = 20,
            Name = "Egypt",
        };

        /// <summary>
        /// Cuba / credits.
        /// </summary>
        public static LevelIdX Cuba { get; } = new LevelIdX()
        {
            Id = (int)LevelId.Cuba,
            DisplayOrder = 21,
            Name = "Credits",
        };

        /// <summary>
        /// List of single player stages in order.
        /// </summary>
        public static List<LevelIdX> SinglePlayerStages { get; } = new List<LevelIdX>()
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
            Cuba,
        };
    }
}
