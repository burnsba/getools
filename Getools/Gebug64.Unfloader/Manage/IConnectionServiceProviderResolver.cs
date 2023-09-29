using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart;

namespace Gebug64.Unfloader.Manage
{
    /// <summary>
    /// Interface to define class that will get the service provider for a given flashcart.
    /// </summary>
    public interface IConnectionServiceProviderResolver
    {
        /// <summary>
        /// Creates the service provider for the given flashcart if it does
        /// not already exist.
        /// </summary>
        /// <param name="flashcart">Flashcart to manage.</param>
        void CreateOnceDeviceManager(IFlashcart flashcart);

        /// <summary>
        /// Gets the registered service provider.
        /// </summary>
        /// <returns>Existing service provider if it exists, or null.</returns>
        IConnectionServiceProvider? GetDeviceManager();
    }
}
