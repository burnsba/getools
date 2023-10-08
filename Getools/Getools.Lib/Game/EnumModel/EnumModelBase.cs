using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.EnumModel
{
    /// <summary>
    /// Common base class to add additional attributes to enum.
    /// </summary>
    public abstract record EnumModelBase
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Sort order.
        /// </summary>
        public int DisplayOrder { get; init; }

        /// <summary>
        /// Display name.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
