using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    /// <summary>
    /// Resolver should make the device manager at runtime.
    /// </summary>
    public interface IDeviceManagerResolver
    {
        /// <summary>
        /// Creates a device manager for the specified device. If the device
        /// manager already exists then nothing happens.
        /// </summary>
        /// <param name="flashcart">Device to create device manager for.</param>
        void CreateOnceDeviceManager(IFlashcart flashcart);

        /// <summary>
        /// Gets the device manager instance, if it exists.
        /// </summary>
        /// <returns>Device manager.</returns>
        IDeviceManager? GetDeviceManager();
    }
}
