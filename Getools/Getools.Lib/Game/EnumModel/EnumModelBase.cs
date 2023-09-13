using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.EnumModel
{
    public abstract record EnumModelBase
    {
        public int Id { get; init; }
        public int DisplayOrder { get; init; }
        public string Name { get; init; }
    }
}
