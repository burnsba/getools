using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Config
{
    public interface ISettingsViewModel
    {
        bool IsDirty { get; set; }
    }
}
