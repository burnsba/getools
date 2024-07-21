using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Extended enum Difficulty.
    /// </summary>
    /// <remarks>
    /// typedef enum DIFFICULTY
    /// </remarks>
    public record DifficultyX : EnumModelBase
    {
        /// <summary>
        /// Default / unknown / unset.
        /// </summary>
        public static DifficultyX Multi { get; } = new DifficultyX()
        {
            Id = (int)Difficulty.Multi,
            DisplayOrder = 0,
            Name = "Multi",
        };

        /// <summary>
        /// Agent.
        /// </summary>
        public static DifficultyX Agent { get; } = new DifficultyX()
        {
            Id = (int)Difficulty.Agent,
            DisplayOrder = 1,
            Name = "Agent",
        };

        /// <summary>
        /// Agent.
        /// </summary>
        public static DifficultyX SecretAgent { get; } = new DifficultyX()
        {
            Id = (int)Difficulty.SecretAgent,
            DisplayOrder = 2,
            Name = "Secret Agent",
        };

        /// <summary>
        /// Agent.
        /// </summary>
        public static DifficultyX Difficulty00 { get; } = new DifficultyX()
        {
            Id = (int)Difficulty.Difficulty00,
            DisplayOrder = 3,
            Name = "00 Agent",
        };

        /// <summary>
        /// Agent.
        /// </summary>
        public static DifficultyX Difficulty007 { get; } = new DifficultyX()
        {
            Id = (int)Difficulty.Difficulty007,
            DisplayOrder = 4,
            Name = "007",
        };

        /// <summary>
        /// List of single player difficulty in order.
        /// </summary>
        public static List<DifficultyX> SinglePlayerDifficulty { get; } = new List<DifficultyX>()
        {
            Agent,
            SecretAgent,
            Difficulty00,
            Difficulty007,
        };

        /// <summary>
        /// List of difficulties that are allowed as parameters for fileUnlockStageInFolderAtDifficulty.
        /// </summary>
        public static List<DifficultyX> UnlockFileDifficulty { get; } = new List<DifficultyX>()
        {
            Agent,
            SecretAgent,
            Difficulty00,
        };
    }
}
