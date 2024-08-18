using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Protocol.Gebug.Message.MessageType
{
    /// <summary>
    /// Gebug available categories.
    /// </summary>
    public enum GebugMessageCategory
    {
        /// <summary>
        /// Unset / unknown.
        /// </summary>
        DefaultUnknown = 0,

        /// <summary>
        /// Native demo/replay methods.
        /// </summary>
        Ramrom = 10,

        /// <summary>
        /// Expanded gebug record/replay methods.
        /// </summary>
        Replay = 15,

        /// <summary>
        /// Gebug save state methods.
        /// </summary>
        SaveState = 20,

        /// <summary>
        /// Native debug methods.
        /// </summary>
        Debug = 25,

        /// <summary>
        /// Gebug methods to manage objectives.
        /// </summary>
        Objectives = 30,

        /// <summary>
        /// Native cheat methods.
        /// </summary>
        Cheat = 35,

        /// <summary>
        /// Gebug methods to get memory information.
        /// </summary>
        Memory = 40,

        /// <summary>
        /// Gebug methods to manipulate sound effects and music.
        /// </summary>
        Sound = 45,

        /// <summary>
        /// Gebug methods to manipulate fog.
        /// </summary>
        Fog = 50,

        /// <summary>
        /// Gebug methods to manage the stage.
        /// </summary>
        Stage = 55,

        /// <summary>
        /// Gebug methods for Bond specific data.
        /// </summary>
        Bond = 60,

        /// <summary>
        /// Gebug methods for chr (guard) specific data.
        /// </summary>
        Chr = 65,

        /// <summary>
        /// Gebug methods for objects (setup objects).
        /// </summary>
        Objects = 70,

        /// <summary>
        /// Gebug methods for file data.
        /// </summary>
        File = 75,

        /// <summary>
        /// Gebug methods for video information (vi.c, fr.c, viewport.c, etc).
        /// </summary>
        Vi = 80,

        /// <summary>
        /// Gebug meta commands.
        /// </summary>
        Meta = 90,

        /// <summary>
        /// Gebug miscellaneous commands.
        /// </summary>
        Misc = 95,
    }
}
