using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Manage;
using Getools.Lib.Game;
using Getools.Lib.Game.Enums;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Backer class for a Stan object.
    /// </summary>
    public class Stan : GameObject, IMapSelectedObjectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Stan"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public Stan(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(logger, connectionServiceProviderResolver)
        {
        }

        /// <summary>
        /// Set to <see cref="Getools.Lib.Game.Asset.Stan.StandTile.InternalName"/>.
        /// </summary>
        public override int PreferredId
        {
            get
            {
                return LayerInstanceId;
            }
        }

        /// <summary>
        /// Convert <see cref="PreferredId"/> to string.
        /// </summary>
        public override string PreferredDisplayId
        {
            get
            {
                return "0x" + PreferredId.ToString("X6");
            }
        }
    }
}
