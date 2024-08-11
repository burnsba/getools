using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Manage;
using Gebug64.Win.Mvvm;
using Getools.Lib.Game.Enums;
using Microsoft.Extensions.Logging;

namespace Gebug64.Win.ViewModels.Game
{
    /// <summary>
    /// Related / backer object that the <see cref="ViewModels.Map.MapObject"/> is drawing.
    /// </summary>
    public class GameObject : ViewModelBase
    {
        /// <summary>
        /// Logger.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Connection service provider.
        /// </summary>
        protected readonly IConnectionServiceProviderResolver _connectionServiceProviderResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameObject"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="connectionServiceProviderResolver">Connection service provider.</param>
        public GameObject(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
        {
            _logger = logger;
            _connectionServiceProviderResolver = connectionServiceProviderResolver;
        }

        /// <summary>
        /// Optional setup object propdef type.
        /// </summary>
        public PropDef? PropDefType { get; set; } = null;

        /// <summary>
        /// Associated "primary key" for the <see cref="Getools.Lib.Game.Enums.PropDef"/> type of object in the setup file.
        /// </summary>
        public int LayerInstanceId { get; set; } = -1;

        /// <summary>
        /// The index of the <see cref="Getools.Lib.Game.Enums.PropDef"/> type of object in the setup file.
        /// </summary>
        public int LayerIndexId { get; set; } = -1;

        /// <summary>
        /// Gets the preferred id to show in mouseover text or other UI elements.
        /// </summary>
        public virtual int PreferredId
        {
            get
            {
                return LayerIndexId;
            }
        }

        /// <summary>
        /// Convert <see cref="PreferredId"/> to string.
        /// </summary>
        public virtual string PreferredDisplayId
        {
            get
            {
                return PreferredId.ToString();
            }
        }
    }
}
