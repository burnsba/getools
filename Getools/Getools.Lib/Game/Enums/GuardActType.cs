using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Enums
{
    /// <summary>
    /// ACT_TYPE.
    /// Action Type to be performed by chr (canonical names).
    /// </summary>
    public enum GuardActType
    {
        ActInit,
        ActStand,
        ActKneel,
        ActAnim,
        ActDie,
        ActDead,
        ActArgh,
        ActPreargh,
        ActAttack,
        ActAttackWalk,
        ActAttackRoll, // 10
        ActSideStep,
        ActJumpout,
        ActRunPos,
        ActPatrol,
        ActGoPos,
        ActSurrender,
        ActLookAtTarget,
        ActSurprised,
        ActStartAlarm,
        ActThrowGrenade, // 20
        ActTurnDir,
        ActTest,
        ActBondIntro,
        ActBondDie,
        ActBondMulti,

        /*** Pd Only
        Act_Bot_Attackstand
        Act_Bot_Attackkneel
        Act_Bot_Attackstrafe
        Act_Druggeddrop
        Act_Druggedko
        Act_Druggedcomingup
        Act_Attackamount
        Act_Robotattack
        Act_Skjump
        **/

        ActNull,
        ActTypeMax,

        ActInvalidData,
    }
}
