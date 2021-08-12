using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectObjectiveWatchMenu : SetupObjectBase, ISetupObject
    {
        public SetupObjectObjectiveWatchMenu()
            : base(Propdef.WatchMenuObjectiveText)
        {
        }

        public int MenuOption { get; set; }
        public int TextId { get; set; }
        public int End { get; set; }
    }
}
