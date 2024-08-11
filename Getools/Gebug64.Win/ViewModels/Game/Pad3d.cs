using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Manage;
using Getools.Lib.Game;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Backer class for a pad or pad3d object.
    /// </summary>
    public class Pad3d : Pad, IMapSelectedObjectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pad3d"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public Pad3d(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(logger, connectionServiceProviderResolver)
        {
        }

        /// <summary>
        /// Bounding box, if pad3d.
        /// </summary>
        public BoundingBoxd Bbox { get; set; } = new BoundingBoxd();
    }
}
