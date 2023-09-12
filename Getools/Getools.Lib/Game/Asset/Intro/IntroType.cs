using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.Intro
{
    /// <summary>
    /// Type of intro definitions.
    /// Matches enum INTRO_TYPE.
    /// </summary>
    public enum IntroType
    {
        /// <summary>
        /// Spawn.
        /// </summary>
        Spawn = 0,

        /// <summary>
        /// Starting weapon, for each hand.
        /// </summary>
        StartWeapon = 1,

        /// <summary>
        /// Starting ammo amounts.
        /// </summary>
        StartAmmo = 2,

        /// <summary>
        /// Swirl cam definition (2nd cinema).
        /// </summary>
        SwirlCam = 3,

        /// <summary>
        /// Fixed intro cam definition (1st cinema).
        /// </summary>
        IntroCam = 4,

        /// <summary>
        /// Bond's cuffs.
        /// </summary>
        Cuff = 5,

        /// <summary>
        /// End of level camera.
        /// </summary>
        FixedCam = 6,

        /// <summary>
        /// Set watch time for level start.
        /// </summary>
        WatchTime = 7,

        /// <summary>
        /// Credits.
        /// </summary>
        Credits = 8,

        /// <summary>
        /// Ends intro section in setup.
        /// </summary>
        EndIntro = 9,
    }
}
