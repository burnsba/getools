﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public interface IAiVariableCommand : IAiConcreteCommand
    {
        byte[] CommandData { get; set; }
    }
}