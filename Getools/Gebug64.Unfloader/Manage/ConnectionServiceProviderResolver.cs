using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;

namespace Gebug64.Unfloader.Manage
{
    /// <summary>
    /// This class is to allow dropping references to the existing service provider (set to null),
    /// then later retrieve a new reference to the same instance.
    /// </summary>
    public class ConnectionServiceProviderResolver : IConnectionServiceProviderResolver
    {
        private IConnectionServiceProvider? _device = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionServiceProviderResolver"/> class.
        /// </summary>
        public ConnectionServiceProviderResolver()
        {
        }

        /// <inheritdoc />
        public void CreateOnceDeviceManager(IFlashcart flashcart)
        {
            if (object.ReferenceEquals(null, _device))
            {
                _device = new ConnectionServiceProvider(flashcart);
            }
        }

        /// <inheritdoc />
        public IConnectionServiceProvider? GetDeviceManager()
        {
            return _device;
        }
    }
}
