using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    public class DeviceManagerResolver : IDeviceManagerResolver
    {
        private IDeviceManager? _device = null;

        public DeviceManagerResolver()
        { }

        public void CreateOnceDeviceManager(IFlashcart flashcart)
        {
            if (object.ReferenceEquals(null, _device))
            {
                _device = new DeviceManager(flashcart);
            }
        }

        public IDeviceManager? GetDeviceManager()
        {
            return _device;
        }
    }
}
