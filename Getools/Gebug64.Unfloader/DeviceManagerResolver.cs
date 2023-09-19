using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    /// <inheritdoc />
    public class DeviceManagerResolver : IDeviceManagerResolver
    {
        private IDeviceManager? _device = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceManagerResolver"/> class.
        /// </summary>
        public DeviceManagerResolver()
        { }

        /// <inheritdoc />
        public void CreateOnceDeviceManager(IFlashcart flashcart)
        {
            if (object.ReferenceEquals(null, _device))
            {
                _device = new DeviceManager(flashcart);
            }
        }

        /// <inheritdoc />
        public IDeviceManager? GetDeviceManager()
        {
            return _device;
        }
    }
}
