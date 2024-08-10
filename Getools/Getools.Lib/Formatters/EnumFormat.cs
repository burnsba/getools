using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game.Enums;

namespace Getools.Lib.Formatters
{
    /// <summary>
    /// Format enum to string.
    /// </summary>
    public class EnumFormat
    {
        /// <summary>
        /// Convert propdef type into friendly name.
        /// </summary>
        /// <param name="pd">Type.</param>
        /// <returns>String.</returns>
        /// <exception cref="NotSupportedException">Throws if not valid enum type.</exception>
        public static string PropDefFriendlyName(PropDef pd)
        {
            switch (pd)
            {
                case PropDef.Nothing: return "Nothing";
                case PropDef.Door: return "Door";
                case PropDef.DoorScale: return "Door Scale";
                case PropDef.StandardProp: return "Standard Prop";
                case PropDef.Key: return "Key";
                case PropDef.Alarm: return "Alarm";
                case PropDef.Cctv: return "CCTV";
                case PropDef.AmmoMag: return "Ammo Magazine";
                case PropDef.Collectable: return "Collectable";
                case PropDef.Guard: return "Guard";
                case PropDef.SingleMonitor: return "Single Monitor";
                case PropDef.MultiMonitor: return "Multi Monitor";
                case PropDef.HangingMonitor: return "Hanging Monitor";
                case PropDef.Drone: return "Drone";
                case PropDef.LinkItems: return "Link guns";
                case PropDef.Unk15: return "Unk15";
                case PropDef.Unk16: return "Unk16";
                case PropDef.Hat: return "Hat";
                case PropDef.SetGuardAttribute: return "Set Guard Attribute";
                case PropDef.LinkProps: return "Link Props";
                case PropDef.AmmoBox: return "Ammo Box";
                case PropDef.Armour: return "Body Armour";
                case PropDef.Tag: return "Tag";
                case PropDef.ObjectiveStart: return "Objective Start";
                case PropDef.ObjectiveEnd: return "Objective End";
                case PropDef.ObjectiveDestroyObject: return "Objective Destroy Object";
                case PropDef.ObjectiveCompleteCondition: return "Objective Complete Condition";
                case PropDef.ObjectiveFailCondition: return "Objective Fail Condition";
                case PropDef.ObjectiveCollectObject: return "Objective Collect Object";
                case PropDef.ObjectiveDepositObject: return "Objective Deposit Object";
                case PropDef.ObjectivePhotograph: return "Objective Photograph";
                case PropDef.ObjectiveNull: return "Objective Null";
                case PropDef.ObjectiveEnterRoom: return "Objective Enter Room";
                case PropDef.ObjectiveDepositObjectInRoom: return "Objective Deposit Object In Room";
                case PropDef.ObjectiveCopy_Item: return "Objective Copy Item";
                case PropDef.WatchMenuObjectiveText: return "Watch Menu Objective Text";
                case PropDef.GasProp: return "Release Gas";
                case PropDef.Rename: return "Rename";
                case PropDef.LockDoor: return "Lock Door";
                case PropDef.Vehicle: return "Vehicle";
                case PropDef.Aircraft: return "Aircraft";
                case PropDef.Unk41: return "Unk41";
                case PropDef.Glass: return "Glass";
                case PropDef.Safe: return "Safe";
                case PropDef.SafeItem: return "Safe Item";
                case PropDef.Tank: return "Tank";
                case PropDef.Cutscene: return "Cutscene";
                case PropDef.TintedGlass: return "Tinted Glass";
                case PropDef.EndProps: return "End Props";
                case PropDef.Max: return "Max";
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Convert GuardActType type into friendly name.
        /// </summary>
        /// <param name="gat">Type.</param>
        /// <returns>String.</returns>
        /// <exception cref="NotSupportedException">Throws if not valid enum type.</exception>
        public static string GuardActTypeFriendlyName(GuardActType gat)
        {
            switch (gat)
            {
                case GuardActType.ActInit: return "Init";
                case GuardActType.ActStand: return "Stand";
                case GuardActType.ActKneel: return "Kneel";
                case GuardActType.ActAnim: return "Anim";
                case GuardActType.ActDie: return "Die";
                case GuardActType.ActDead: return "Dead";
                case GuardActType.ActArgh: return "Argh";
                case GuardActType.ActPreargh: return "Preargh";
                case GuardActType.ActAttack: return "Attack";
                case GuardActType.ActAttackWalk: return "Attack Walk";
                case GuardActType.ActAttackRoll: return "Attack Roll";
                case GuardActType.ActSideStep: return "Side Step";
                case GuardActType.ActJumpout: return "Jumpout";
                case GuardActType.ActRunPos: return "Run Pos";
                case GuardActType.ActPatrol: return "Patrol";
                case GuardActType.ActGoPos: return "Go Pos";
                case GuardActType.ActSurrender: return "Surrender";
                case GuardActType.ActLookAtTarget: return "Look At Target";
                case GuardActType.ActSurprised: return "Surprised";
                case GuardActType.ActStartAlarm: return "Start Alarm";
                case GuardActType.ActThrowGrenade: return "Throw Grenade";
                case GuardActType.ActTurnDir: return "Turn Dir";
                case GuardActType.ActTest: return "Test";
                case GuardActType.ActBondIntro: return "Bond Intro";
                case GuardActType.ActBondDie: return "Bond Die";
                case GuardActType.ActBondMulti: return "Bond Multi";

                case GuardActType.ActNull: /*** fallthrough */
                case GuardActType.ActTypeMax:
                case GuardActType.ActInvalidData: return "Invalid";
            }

            throw new NotSupportedException();
        }
    }
}
