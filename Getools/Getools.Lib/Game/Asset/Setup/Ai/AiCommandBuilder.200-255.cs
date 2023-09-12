using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// <see cref="AiCommandBuilder"/> command bytes 200-255.
    /// </summary>
    public partial class AiCommandBuilder
    {
        /// <summary>
        /// fade out occupied sfx channel's volume by volume percent
        /// </summary>
        /// <remarks>
        /// time argument is number of ticks to fade between current volume to target volume. volume argument is signed. range is 0x0000-0x7FFF (0-100%)
        /// </remarks>
        public static AiFixedCommandDescription SfxFadeChannelVolume
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_fade_channel_volume",
                    CommandId = 200,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "channel_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "fade_volume_percent", ByteLength = 2 },
                        new AiParameter() { ParameterName = "fade_time60", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// stop playing sfx in occupied sfx channel
        /// </summary>
        public static AiFixedCommandDescription SfxStopChannel
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_stop_channel",
                    CommandId = 201,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "channel_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if sfx channel's volume is < volume argument, goto label
        /// </summary>
        /// <remarks>
        /// if sfx channel is free (no audio playing), goto label. volume argument is signed. range is 0x0000-0x7FFF
        /// </remarks>
        public static AiFixedCommandDescription IfSfxChannelVolumeLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_sfx_channel_volume_less_than",
                    CommandId = 202,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "channel_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "volume", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// makes vehicle follow a predefined path within setup
        /// </summary>
        public static AiFixedCommandDescription VehicleStartPath
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "vehicle_start_path",
                    CommandId = 203,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "path_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// sets vehicle speed, usually paired with command CB
        /// </summary>
        /// <remarks>
        /// arguments are unsigned. 1000 units = 1 meter per second travel speed. acceleration_time60 is number of game ticks to reach top speed (lower = faster)
        /// </remarks>
        public static AiFixedCommandDescription VehicleSpeed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "vehicle_speed",
                    CommandId = 204,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "top_speed", ByteLength = 2 },
                        new AiParameter() { ParameterName = "acceleration_time60", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// sets aircraft's rotor speed
        /// </summary>
        /// <remarks>
        /// arguments are unsigned. argument scale is 10 units per degree, per tick. acceleration_time60 is number of game ticks to reach top speed (lower = faster)
        /// </remarks>
        public static AiFixedCommandDescription AircraftRotorSpeed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "aircraft_rotor_speed",
                    CommandId = 205,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "rotor_speed", ByteLength = 2 },
                        new AiParameter() { ParameterName = "acceleration_time60", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// if camera mode equal to INTRO_CAM/FADESWIRL_CAM (viewing mission intro), goto label
        /// </summary>
        /// <remarks>
        /// if setup lacks intro camera structs, intro will be skipped
        /// </remarks>
        public static AiFixedCommandDescription IfCameraIsInIntro
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_camera_is_in_intro",
                    CommandId = 206,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if camera mode equal to SWIRL_CAM (moving to back of bond's head), goto label
        /// </summary>
        /// <remarks>
        /// if setup lacks swirl points, intro swirl will be skipped
        /// </remarks>
        public static AiFixedCommandDescription IfCameraIsInBondSwirl
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_camera_is_in_bond_swirl",
                    CommandId = 207,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// change the screen bank of a tagged tv monitor
        /// </summary>
        /// <remarks>
        /// if tagged object has multiple screens, use screen index argument to set. if tagged object has one screen, screen index is ignored
        /// </remarks>
        public static AiFixedCommandDescription TvChangeScreenBank
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "tv_change_screen_bank",
                    CommandId = 208,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "screen_index", ByteLength = 1 },
                        new AiParameter() { ParameterName = "screen_bank", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond is controlling tank, goto label
        /// </summary>
        public static AiFixedCommandDescription IfBondInTank
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_in_tank",
                    CommandId = 209,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// exits the level
        /// </summary>
        /// <remarks>
        /// recommend not to use this command, instead goto GLIST_EXIT_LEVEL for exit cutscene list. retail game has a glitch with hires mode that needs to execute this command in a loop, check cuba's 1000 list
        /// </remarks>
        public static AiFixedCommandDescription ExitLevel
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "exit_level",
                    CommandId = 210,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// switch back to first person view
        /// </summary>
        /// <remarks>
        /// unused command, never used in retail game. tagged items within inventory will become invalid after command - only weapons are safe. command must have 3 ai_sleep commands before executing this command or else engine will crash on console (use camera_transition_to_bond). mission time is resumed on return to first person view
        /// </remarks>
        public static AiFixedCommandDescription CameraReturnToBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "camera_return_to_bond",
                    CommandId = 211,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// change view to pad and look at bond
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. mission time is paused while in third person
        /// </remarks>
        public static AiFixedCommandDescription CameraLookAtBondFromPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "camera_look_at_bond_from_pad",
                    CommandId = 212,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// change view to tagged camera's position and rotation
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. only look at bond if flag is set. unused flag may have separated look at bond as x/y flags instead of a single flag - for retail unused flag does nothing. mission time is paused while in third person
        /// </remarks>
        public static AiFixedCommandDescription CameraSwitch
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "camera_switch",
                    CommandId = 213,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "look_at_bond_flag", ByteLength = 2 },
                        new AiParameter() { ParameterName = "unused_flag", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's y axis position < position argument, goto label
        /// </summary>
        /// <remarks>
        /// checks if bond's y axis is below the provided argument. command uses world units. argument is signed and scale is 1:1 to in-game position. bond's point of view is accounted for by command (like debug manpos)
        /// </remarks>
        public static AiFixedCommandDescription IfBondYPosLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_y_pos_less_than",
                    CommandId = 214,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "y_pos", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// hide hud elements, lock player control and stop mission time. command is commonly used for exit mission lists
        /// </summary>
        /// <remarks>
        /// argument flag will not hide element on command execution. this is needed for dialog/hud countdown while in cinema mode. flags can be combined together to show multiple elements. sequential executions of D7 can be used to hide more elements, but once an element has been hidden it cannot be shown again until command D8 is executed. bond can take damage while in locked state. use HUD_# flags for bitfield argument
        /// </remarks>
        public static AiFixedCommandDescription HudHideAndLockControlsAndPauseMissionTime
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_hide_and_lock_controls_and_pause_mission_time",
                    CommandId = 215,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// show all hud elements, unlock player control and resume mission time
        /// </summary>
        /// <remarks>
        /// should only be executed after D7 command
        /// </remarks>
        public static AiFixedCommandDescription HudShowAllAndUnlockControlsAndResumeMissionTime
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_show_all_and_unlock_controls_and_resume_mission_time",
                    CommandId = 216,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// teleport chr to pad, goto label if successful
        /// </summary>
        public static AiFixedCommandDescription ChrTryTeleportingToPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_try_teleporting_to_pad",
                    CommandId = 217,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// fades the screen out to black
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiFixedCommandDescription ScreenFadeToBlack
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "screen_fade_to_black",
                    CommandId = 218,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// fades the screen from black
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiFixedCommandDescription ScreenFadeFromBlack
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "screen_fade_from_black",
                    CommandId = 219,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// when screen fade has completed (from/to black), goto label
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiFixedCommandDescription IfScreenFadeCompleted
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_screen_fade_completed",
                    CommandId = 220,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// hide all characters in level - including bond's third person model. execute this before switching to exit camera or bond will disappear
        /// </summary>
        /// <remarks>
        /// hidden characters will halt their ai list execution until unhidden
        /// </remarks>
        public static AiFixedCommandDescription ChrHideAll
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_hide_all",
                    CommandId = 221,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// show all characters previously hidden by command DD
        /// </summary>
        public static AiFixedCommandDescription ChrShowAll
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_show_all",
                    CommandId = 222,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// instantly open tagged door
        /// </summary>
        /// <remarks>
        /// mostly used for cutscenes, doesn't trigger door opening sfx. open tagged door even if locked
        /// </remarks>
        public static AiFixedCommandDescription DoorOpenInstant
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "door_open_instant",
                    CommandId = 223,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// remove the item held by hand index
        /// </summary>
        /// <remarks>
        /// does not drop item, instead clears holding item flag for hand index
        /// </remarks>
        public static AiFixedCommandDescription ChrRemoveItemInHand
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_remove_item_in_hand",
                    CommandId = 224,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "hand_index", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if the number of active players < argument, goto label
        /// </summary>
        /// <remarks>
        /// single player always has a total of active players set to 1
        /// </remarks>
        public static AiFixedCommandDescription IfNumberOfActivePlayersLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_number_of_active_players_less_than",
                    CommandId = 225,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "number", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's total ammo for item < ammo_total argument, goto label
        /// </summary>
        /// <remarks>
        /// ammo_total argument is signed. total ammo also accounts for loaded gun
        /// </remarks>
        public static AiFixedCommandDescription IfBondItemTotalAmmoLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_item_total_ammo_less_than",
                    CommandId = 226,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "ammo_total", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// forces bond to equip an item - only works in first person
        /// </summary>
        /// <remarks>
        /// can be used for any item, even if bond doesn't have it in inventory
        /// </remarks>
        public static AiFixedCommandDescription BondEquipItem
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_equip_item",
                    CommandId = 227,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// forces bond to equip an item - only works in third person (cinema)
        /// </summary>
        /// <remarks>
        /// can be used for any item, even if bond doesn't have it in inventory
        /// </remarks>
        public static AiFixedCommandDescription BondEquipItemCinema
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_equip_item_cinema",
                    CommandId = 228,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// forces bond to move in X/Z direction
        /// </summary>
        /// <remarks>
        /// only works when bond has been locked by command D7. used for dam jump. argument is signed and scale is 1:1 to in-game position. speed is number of world units per tick
        /// </remarks>
        public static AiFixedCommandDescription BondSetLockedVelocity
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_set_locked_velocity",
                    CommandId = 229,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "x_speed60", ByteLength = 1 },
                        new AiParameter() { ParameterName = "z_speed60", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if tagged object in the same room with pad, goto label
        /// </summary>
        public static AiFixedCommandDescription IfObjectInRoomWithPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_in_room_with_pad",
                    CommandId = 230,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard is in firing state (ACT_ATTACK) and TARGET_180_RANGE is set, goto label
        /// </summary>
        public static AiFixedCommandDescription IfGuardIsFiringAndUsing180RangeFlag
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_is_firing_and_using_180_range_flag",
                    CommandId = 231,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard is in firing state (ACT_ATTACK), goto label
        /// </summary>
        public static AiFixedCommandDescription IfGuardIsFiring
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_is_firing",
                    CommandId = 232,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// instantly switch fog to the next fog's slot
        /// </summary>
        /// <remarks>
        /// this command can't be stopped after executing. level must have a fog assigned or will crash!
        /// </remarks>
        public static AiFixedCommandDescription SwitchFogInstantly
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "switch_fog_instantly",
                    CommandId = 233,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// if player pressed any button, fade to black and exit level
        /// </summary>
        /// <remarks>
        /// this command activates a state where game will fade to black when button input is detected from controller 1. command does not pause mission time
        /// </remarks>
        public static AiFixedCommandDescription TriggerFadeAndExitLevelOnButtonPress
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "trigger_fade_and_exit_level_on_button_press",
                    CommandId = 234,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// if bond has died/been killed, goto label
        /// </summary>
        public static AiFixedCommandDescription IfBondIsDead
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_is_dead",
                    CommandId = 235,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// disables bond damage and ability to pick up items
        /// </summary>
        /// <remarks>
        /// commonly used for level exit ai lists - prevents bond dying after triggering exit cutscene. use command F3 to check if flag is set on
        /// </remarks>
        public static AiFixedCommandDescription BondDisableDamageAndPickups
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_disable_damage_and_pickups",
                    CommandId = 236,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// set bond's left/right weapons to be invisible
        /// </summary>
        public static AiFixedCommandDescription BondHideWeapons
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_hide_weapons",
                    CommandId = 237,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// change view to orbit a pad with set speed
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. arguments: lat_distance: camera distance from pad, 100 units per meter. argument is unsigned vert_distance: camera distance from pad, 100 units per meter. argument is signed orbit_speed: speed to orbit around pad, argument is signed. unit format uses compass direction like target commands (14-17). generally stick to a low range as it is used for delta timing (0100-FF00) pad: pad for camera to target and orbit around y_pos_offset: offset the relative y position for pad (boom/jib), argument is signed initial_rotation: uses compass direction like target commands (14-17) but inverted - hex N: 0000 E: C000 S: 8000: W: 4000 mission time is paused while in third person
        /// </remarks>
        public static AiFixedCommandDescription CameraOrbitPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "camera_orbit_pad",
                    CommandId = 238,
                    CommandLengthBytes = 13,
                    NumberParameters = 6,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "lat_distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "vert_distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "orbit_speed60", ByteLength = 2 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "y_pos_offset", ByteLength = 2 },
                        new AiParameter() { ParameterName = "initial_rotation", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// trigger credits crawl
        /// </summary>
        /// <remarks>
        /// credits text and positions are stored in setup intro struct
        /// </remarks>
        public static AiFixedCommandDescription CreditsRoll
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "credits_roll",
                    CommandId = 239,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// credits crawl has finished, goto label
        /// </summary>
        public static AiFixedCommandDescription IfCreditsHasCompleted
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_credits_has_completed",
                    CommandId = 240,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if all objectives for current difficulty has been completed, goto label
        /// </summary>
        /// <remarks>
        /// uses objective difficulty settings within setup, briefing file settings are not referenced. ensure both setup and briefing files are consistent
        /// </remarks>
        public static AiFixedCommandDescription IfObjectiveAllCompleted
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_objective_all_completed",
                    CommandId = 241,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if current bond equal to folder actor index, goto label
        /// </summary>
        /// <remarks>
        /// in retail release only index 0 works. originally this would have checked which bond (brosnan/connery/moore/dalton) is currently used, with each briefing folder using a different bond likeness in-game. however rare didn't have the license to use the other actor's faces so this feature was removed. command is only used for cuba (credits)
        /// </remarks>
        public static AiFixedCommandDescription IfFolderActorIsEqual
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_folder_actor_is_equal",
                    CommandId = 242,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bond_actor_index", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond damage and ability to pick up items disabled, goto label
        /// </summary>
        /// <remarks>
        /// used to check when bond has exited level, usually to stop guards from spawning during mission cinema. use command EC to set state on
        /// </remarks>
        public static AiFixedCommandDescription IfBondDamageAndPickupsDisabled
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_damage_and_pickups_disabled",
                    CommandId = 243,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// play level's x track for duration
        /// </summary>
        /// <remarks>
        /// seconds arguments are unsigned, available music slots range is 0-3. stopped duration argument is used by command F5. when using F5 to stop a music slot, the xtrack will continue to play until this or total time reaches 0. if you don't want this to happen, set the seconds stopped duration argument to 0
        /// </remarks>
        public static AiFixedCommandDescription MusicXtrackPlay
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "music_xtrack_play",
                    CommandId = 244,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "music_slot", ByteLength = 1 },
                        new AiParameter() { ParameterName = "seconds_stopped_duration", ByteLength = 1 },
                        new AiParameter() { ParameterName = "seconds_total_duration", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// stop currently playing x track in slot
        /// </summary>
        /// <remarks>
        /// music slots range is 0-3. use slot -1 to stop all xtrack slots instantly. when stopping a music slot, it will let the track continue to play until the seconds stopped duration time or total time (set by command F4) reaches zero. this is ignored when using music slot -1
        /// </remarks>
        public static AiFixedCommandDescription MusicXtrackStop
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "music_xtrack_stop",
                    CommandId = 245,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "music_slot", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// triggers explosions around the player, will continue forever
        /// </summary>
        /// <remarks>
        /// does not trigger level exit or killed in action state
        /// </remarks>
        public static AiFixedCommandDescription TriggerExplosionsAroundBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "trigger_explosions_around_bond",
                    CommandId = 246,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// if total civilians killed > argument, goto label
        /// </summary>
        /// <remarks>
        /// guards flagged with CHRFLAG_COUNT_DEATH_AS_CIVILIAN will count towards total when killed. usually set for scientists/civilians/innocent NPCs
        /// </remarks>
        public static AiFixedCommandDescription IfKilledCiviliansGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_killed_civilians_greater_than",
                    CommandId = 247,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "civilians_killed", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if chr was shot since last check, goto label
        /// </summary>
        /// <remarks>
        /// checks chr->chrflags if CHRFLAG_WAS_HIT is set. if true, unset flag and goto label. CHRFLAG_WAS_HIT is set even if guard is invincible
        /// </remarks>
        public static AiFixedCommandDescription IfChrWasShotSinceLastCheck
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_was_shot_since_last_check",
                    CommandId = 248,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// sets briefing status to killed in action, automatic mission failure
        /// </summary>
        /// <remarks>
        /// does not kill the player, only changes the mission status
        /// </remarks>
        public static AiFixedCommandDescription BondKilledInAction
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_killed_in_action",
                    CommandId = 249,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// makes guard raise their arms for half a second
        /// </summary>
        public static AiFixedCommandDescription GuardRaisesArms
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_raises_arms",
                    CommandId = 250,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// trigger gas leak event and slowly transition fog to the next fog's slot
        /// </summary>
        /// <remarks>
        /// this command triggers a gas leak. for the level egypt, this command will not trigger a gas leak, but instead will only transition the fog. this command can't be stopped after executing. level must have a fog assigned or will crash!
        /// </remarks>
        public static AiFixedCommandDescription GasLeakAndFadeFog
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "gas_leak_and_fade_fog",
                    CommandId = 251,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// launch a tagged object like a rocket
        /// </summary>
        /// <remarks>
        /// if tagged object can't be turned upright, object will be destroyed instead. can be used to drop attached props
        /// </remarks>
        public static AiFixedCommandDescription ObjectRocketLaunch
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_rocket_launch",
                    CommandId = 252,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                    },
                };
            }
        }
    }
}
