﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// Setup object types / propdef ids.
    /// Cooresponds to type `enum PROPDEF_TYPE`.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "<Justification>")]
    public enum PropDef
    {
        Nothing = 0,
        Door = 1,
        DoorScale = 2,
        StandardProp = 3,
        Key = 4,
        Alarm = 5,
        Cctv = 6,
        Magazine = 7,
        Collectable = 8,
        Guard = 9,
        SingleMonitor = 10,
        MultiMonitor = 11,
        Rack = 12,
        Drone = 13,
        Link = 14,
        Unk15 = 15,
        Unk16 = 16,
        Hat = 17,
        SetGuardAttribute = 18,
        Switch = 19,
        AmmoBox = 20,
        Armour = 21,
        Tag = 22,
        ObjectiveStart = 23,
        ObjectiveEnd = 24,
        ObjectiveDestroyObject = 25,
        ObjectiveCompleteCondition = 26,
        ObjectiveFailCondition = 27,
        ObjectiveCollectObject = 28,
        ObjectiveDepositObject = 29,
        ObjectivePhotograph = 30,
        ObjectiveNull = 31,
        ObjectiveEnterRoom = 32,
        ObjectiveDepositObjectInRoom = 33,
        ObjectiveCopy_Item = 34,
        WatchMenuObjectiveText = 35,
        GasReleasing = 36,
        Rename = 37,
        LockDoor = 38,
        Vehichle = 39,
        Aircraft = 40,
        Unk41 = 41,
        Glass = 42,
        Safe = 43,
        SafeItem = 44,
        Vehicle = 45,
        Cutscene = 46,
        TintedGlass = 47,
        EndProps = 48,
        Max,
    }
}