using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// bondconstants.h typedef enum PROPDEF_TYPE
    /// </remarks>
    public enum SetupObjectType
    {
        Nothing = 0,
        Door = 1,
        DoorScale = 2,
        Prop = 3,
        Key = 4,
        Alarm = 5,
        Cctv = 6,
        Magazine = 7,
        Collectable = 8,
        Guard = 9,
        Monitor = 10,
        MultiMonitor = 11,
        Rack = 12,
        Autogun = 13,
        Link = 14,
        Unk15 = 15,
        Unk16 = 16,
        Hat = 17,
        GuardAttribute = 18,
        Switch = 19,
        Ammo = 20,
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
        Tank = 45,
        Camerapos = 46,
        TintedGlass = 47,
        End = 48,
        Max
    }
}
