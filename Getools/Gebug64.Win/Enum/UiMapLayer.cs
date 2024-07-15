using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.Enum
{
    /// <summary>
    /// Layers to show on the UI.
    /// </summary>
    public enum UiMapLayer
    {
        /// <summary>
        /// Default / unknown / unset layer.
        /// </summary>
        DefaultUnknown,

        /// <summary>
        /// BG layer.
        /// </summary>
        Bg,

        /// <summary>
        /// Bond layer.
        /// </summary>
        Bond,

        /// <summary>
        /// Stan layer.
        /// </summary>
        Stan,

        /// <summary>
        /// Pad layer.
        /// </summary>
        SetupPad,

        /// <summary>
        /// Waypoint layer.
        /// </summary>
        SetupPathWaypoint,

        /// <summary>
        /// Patrol layer.
        /// </summary>
        SetupPatrolPath,

        /// <summary>
        /// Alarm layer.
        /// </summary>
        SetupAlarm,

        /// <summary>
        /// Ammo layer.
        /// </summary>
        SetupAmmo,

        /// <summary>
        /// Aicraft layer.
        /// </summary>
        SetupAircraft,

        /// <summary>
        /// Body armor layer.
        /// </summary>
        SetupBodyArmor,

        /// <summary>
        /// Guard layer.
        /// </summary>
        SetupGuard,

        /// <summary>
        /// CCTV layer.
        /// </summary>
        SetupCctv,

        /// <summary>
        /// Collectable layer.
        /// </summary>
        SetupCollectable,

        /// <summary>
        /// Door layer.
        /// </summary>
        SetupDoor,

        /// <summary>
        /// Drone layer.
        /// </summary>
        SetupDrone,

        /// <summary>
        /// Key layer.
        /// </summary>
        SetupKey,

        /// <summary>
        /// Safe layer.
        /// </summary>
        SetupSafe,

        /// <summary>
        /// Monitor layer.
        /// </summary>
        SetupSingleMontior,

        /// <summary>
        /// Prop layer.
        /// </summary>
        SetupStandardProp,

        /// <summary>
        /// Tank layer.
        /// </summary>
        SetupTank,

        /// <summary>
        /// Intro spawn layer.
        /// </summary>
        SetupIntro,
    }
}
