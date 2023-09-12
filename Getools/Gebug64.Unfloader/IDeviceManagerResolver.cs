using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader
{
    public interface IDeviceManagerResolver
    {
        void CreateOnceDeviceManager(IFlashcart flashcart);
        IDeviceManager? GetDeviceManager();
    }
}
