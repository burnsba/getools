﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Message.CommandParameter
{
    public interface ICommandParameter<T>
    {
        T Value { get; set; }
    }
}