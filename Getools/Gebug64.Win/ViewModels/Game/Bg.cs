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
    /// Backer class for a Bg object.
    /// </summary>
    public class Bg : GameObject, IMapSelectedObjectViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bg"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public Bg(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(logger, connectionServiceProviderResolver)
        {
        }
    }
}
