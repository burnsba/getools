using Gebug64.Unfloader.Protocol.Flashcart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Manage
{
    public interface IConnectionServiceProviderResolver
    {
        void CreateOnceDeviceManager(IFlashcart flashcart);
        IConnectionServiceProvider? GetDeviceManager();
    }
}
