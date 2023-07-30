using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// <see cref="AiCommandBuilder"/> command bytes 0-49.
    /// </summary>
    public partial class AiCommandBuilder
    {
        /// <summary>
        /// goto the next label (command 02) - skips all commands between command and goto label - continues executing after found label
        /// </summary>
        public static AiFixedCommandDescription GotoNext
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "goto_next",
                    CommandId = 0,
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
        /// like goto_next, but it starts scanning label from start of list
        /// </summary>
        public static AiFixedCommandDescription GotoFirst
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "goto_first",
                    CommandId = 1,
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
        /// label marker for ai list - used for all commands that return true
        /// </summary>
        public static AiFixedCommandDescription Label
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "label",
                    CommandId = 2,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "id", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// halt the ai list - frees engine to start executing next ai list until all lists have been executed for game tick.
        /// </summary>
        /// <remarks>
        /// offscreen/idle guards will take 14 game ticks instead of 1 tick on ai_sleep
        /// </remarks>
        public static AiFixedCommandDescription AiSleep
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "ai_sleep",
                    CommandId = 3,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// used for ai list parser to check when list ends
        /// </summary>
        /// <remarks>
        /// not recommended to execute this command - to finish a list create an infinite loop (goto_loop_infinite) or jump to GLIST_END_ROUTINE when list has finished tasks
        /// </remarks>
        public static AiFixedCommandDescription AiListEnd
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "ai_list_end",
                    CommandId = 4,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// set chr num's current ai list program counter to beginning of a list
        /// </summary>
        /// <remarks>
        /// not recommended to goto an obj list (10XX)
        /// </remarks>
        public static AiFixedCommandDescription JumpToAiList
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "jump_to_ai_list",
                    CommandId = 5,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "ai_list", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// store a list ptr in current chr struct - used for command 07 return
        /// </summary>
        /// <remarks>
        /// not recommended to set stored list to an obj list (10XX)
        /// </remarks>
        public static AiFixedCommandDescription SetReturnAiList
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "set_return_ai_list",
                    CommandId = 6,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "ai_list", ByteLength = 2 },
                    },
                };
            }
        }

        /// <summary>
        /// jump the return ai list set in chr struct - pointer set by command 06. used for subroutine lists. if list pointer isn't set, game will crash
        /// </summary>
        /// <remarks>
        /// after return, set chr->aioffset to top of ai list
        /// </remarks>
        public static AiFixedCommandDescription JumpToReturnAiList
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "jump_to_return_ai_list",
                    CommandId = 7,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// reset guard back to idle pose - can be used to stop guards in place
        /// </summary>
        public static AiFixedCommandDescription GuardAnimationStop
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_animation_stop",
                    CommandId = 8,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// make guard kneel on one knee
        /// </summary>
        public static AiFixedCommandDescription GuardKneel
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_kneel",
                    CommandId = 9,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// set guard to playback animation
        /// </summary>
        /// <remarks>
        ///  start/end set to -1/-1 will playback the entire animation length. interpolation time will set how long it will take to transition from the previous state. if interpolation time is too low it may crash! - use 0x10 if unsure. start/end keyframe uses animation 30 tick units - interpolation use 60 tick units. use ANIM_# flags for bitfield argument
        /// </remarks>
        public static AiFixedCommandDescription GuardPlayAnimation
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_play_animation",
                    CommandId = 10,
                    CommandLengthBytes = 9,
                    NumberParameters = 5,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "animation_id", ByteLength = 2 },
                        new AiParameter() { ParameterName = "start_time30", ByteLength = 2 },
                        new AiParameter() { ParameterName = "end_time30", ByteLength = 2 },
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 1 },
                        new AiParameter() { ParameterName = "interpol_time60", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard is in animation playback state (ACT_ANIM), goto label
        /// </summary>
        public static AiFixedCommandDescription IfGuardPlayingAnimation
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_playing_animation",
                    CommandId = 11,
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
        /// guard points if bond is directly in front of guard, else command is ignored
        /// </summary>
        /// <remarks>
        /// global ai list GLIST_FIRE_RAND_ANIM_SUBROUTINE skips this command if bitfield flag BITFIELD_DONT_POINT_AT_BOND is on
        /// </remarks>
        public static AiFixedCommandDescription GuardPointsAtBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_points_at_bond",
                    CommandId = 12,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// set guard to playback animation - used when shots land near guard
        /// </summary>
        public static AiFixedCommandDescription GuardLooksAroundSelf
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_looks_around_self",
                    CommandId = 13,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// trigger guard to sidestep, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiFixedCommandDescription GuardTrySidestepping
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_sidestepping",
                    CommandId = 14,
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
        /// trigger guard to hop sideways, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiFixedCommandDescription GuardTryHoppingSideways
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_hopping_sideways",
                    CommandId = 15,
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
        /// trigger guard to run sideways of bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiFixedCommandDescription GuardTryRunningToSide
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_running_to_side",
                    CommandId = 16,
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
        /// trigger guard to walk and fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond needs to be at long distance away from guard to work
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFiringWalk
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_firing_walk",
                    CommandId = 17,
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
        /// trigger guard to run and fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond needs to be at long distance away from guard to work
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFiringRun
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_firing_run",
                    CommandId = 18,
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
        /// trigger guard to roll on ground then fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond cannot be too close to guard or it won't work
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFiringRoll
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_firing_roll",
                    CommandId = 19,
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
        /// make guard aim/fire their weapon at target, goto label if successful
        /// </summary>
        /// <remarks>
        /// bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFireOrAimAtTarget
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target",
                    CommandId = 20,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 2 },
                        new AiParameter() { ParameterName = "target", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// make guard kneel and aim/fire their weapon at target, goto label if successful
        /// </summary>
        /// <remarks>
        /// bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFireOrAimAtTargetKneel
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target_kneel",
                    CommandId = 21,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 2 },
                        new AiParameter() { ParameterName = "target", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// update guard's aim/fire target, goto label if successful
        /// </summary>
        /// <remarks>
        /// this command only works if guard is currently aiming at a target. bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFireOrAimAtTargetUpdate
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target_update",
                    CommandId = 22,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 2 },
                        new AiParameter() { ParameterName = "target", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// make guard continuously face target, goto label if successful
        /// </summary>
        /// <remarks>
        /// if guard was shot while facing target, guard will snap out of facing state. bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument. command can't use TARGET_AIM_ONLY flag
        /// </remarks>
        public static AiFixedCommandDescription GuardTryFacingTarget
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_facing_target",
                    CommandId = 23,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "bitfield", ByteLength = 2 },
                        new AiParameter() { ParameterName = "target", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// hit chr's body part with item's damage, play reaction to hit location
        /// </summary>
        /// <remarks>
        /// command does not trigger item's fire sfx. item's damage uses body part damage modifier. use HIT_# define for hit part number
        /// </remarks>
        public static AiFixedCommandDescription ChrHitBodyPartWithItemDamage
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_hit_body_part_with_item_damage",
                    CommandId = 24,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "part_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// chr hits chr's body part with held item, play reaction to hit location
        /// </summary>
        /// <remarks>
        /// command does not trigger item's fire sfx or chr firing animation. item's damage uses body part damage modifier. use HIT_# define for hit part number
        /// </remarks>
        public static AiFixedCommandDescription ChrHitChrBodyPartWithHeldItem
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_hit_chr_body_part_with_held_item",
                    CommandId = 25,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "chr_num_target", ByteLength = 1 },
                        new AiParameter() { ParameterName = "part_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// trigger guard to throw a grenade at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// a rng byte is generated and compared again chr->grenadeprob, if rng byte is less than grenadeprob throw grenade and goto label, else do nothing. chr->grenadeprob default is 0 - to change use setup object 12 or command 8D
        /// </remarks>
        public static AiFixedCommandDescription GuardTryThrowingGrenade
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_throwing_grenade",
                    CommandId = 26,
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
        /// spawn and drop item with prop model from guard, goto label if successful
        /// </summary>
        /// <remarks>
        /// dropped item uses item type (08) with model number - they can be picked up. grenade/mines will be dropped live - this is used for cradle (list #0411)
        /// </remarks>
        public static AiFixedCommandDescription GuardTryDroppingItem
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_dropping_item",
                    CommandId = 27,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "prop_num", ByteLength = 2 },
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// makes the guard run to pad
        /// </summary>
        public static AiFixedCommandDescription GuardRunsToPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_runs_to_pad",
                    CommandId = 28,
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
        /// makes the guard run to guard->padpreset1 (PAD_PRESET - 9000)
        /// </summary>
        public static AiFixedCommandDescription GuardRunsToPadPreset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_runs_to_pad_preset",
                    CommandId = 29,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// makes the guard walk to pad
        /// </summary>
        public static AiFixedCommandDescription GuardWalksToPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_walks_to_pad",
                    CommandId = 30,
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
        /// makes the guard sprint to pad
        /// </summary>
        public static AiFixedCommandDescription GuardSprintsToPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_sprints_to_pad",
                    CommandId = 31,
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
        /// makes guard walk a predefined path within setup
        /// </summary>
        /// <remarks>
        /// usually paired with goto GLIST_DETECT_BOND_DEAF_NO_CLONE_NO_IDLE_ANIM or GLIST_DETECT_BOND_NO_CLONE_NO_IDLE_ANIM
        /// </remarks>
        public static AiFixedCommandDescription GuardStartPatrol
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_start_patrol",
                    CommandId = 32,
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
        /// makes a guard surrender and drop all attached and held items
        /// </summary>
        /// <remarks>
        /// will not drop items embedded within guard
        /// </remarks>
        public static AiFixedCommandDescription GuardSurrenders
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_surrenders",
                    CommandId = 33,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// sets guard to fade away - fade time is 90 ticks (1.5 seconds). when the fade finishes, automatically remove guard
        /// </summary>
        /// <remarks>
        /// guard collision is ignored during fade - will not drop items
        /// </remarks>
        public static AiFixedCommandDescription GuardRemoveFade
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_remove_fade",
                    CommandId = 34,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// instantly remove chr unlike above command
        /// </summary>
        /// <remarks>
        /// will not drop items
        /// </remarks>
        public static AiFixedCommandDescription ChrRemoveInstant
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_remove_instant",
                    CommandId = 35,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// guard activates alarm assigned to pad, goto label if successful
        /// </summary>
        /// <remarks>
        /// command doesn't care what object type is at pad, as long as the object isn't destroyed. command also checks if guard is alive before activating alarm. when triggering alarm, guard will be set to state ACT_STARTALARM and play animation
        /// </remarks>
        public static AiFixedCommandDescription GuardTryTriggeringAlarmAtPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_triggering_alarm_at_pad",
                    CommandId = 36,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// activates alarm
        /// </summary>
        public static AiFixedCommandDescription AlarmOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "alarm_on",
                    CommandId = 37,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// deactivates alarm
        /// </summary>
        public static AiFixedCommandDescription AlarmOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "alarm_off",
                    CommandId = 38,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// command no longer exists, never goto label
        /// </summary>
        public static AiFixedCommandDescription RemovedCommand27
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "removed_command27",
                    CommandId = 39,
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
        /// if guard is able to run to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't run to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiFixedCommandDescription GuardTryRunningToBondPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_running_to_bond_position",
                    CommandId = 40,
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
        /// if guard is able to walk to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't walk to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiFixedCommandDescription GuardTryWalkingToBondPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_walking_to_bond_position",
                    CommandId = 41,
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
        /// if guard is able to sprint to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't sprint to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiFixedCommandDescription GuardTrySprintingToBondPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_sprinting_to_bond_position",
                    CommandId = 42,
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
        /// command no longer exists, never goto label
        /// </summary>
        public static AiFixedCommandDescription RemovedCommand2B
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "removed_command2B",
                    CommandId = 43,
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
        /// if guard is able to run to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't run to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiFixedCommandDescription GuardTryRunningToChrPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_running_to_chr_position",
                    CommandId = 44,
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
        /// if guard is able to walk to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't walk to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiFixedCommandDescription GuardTryWalkingToChrPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_walking_to_chr_position",
                    CommandId = 45,
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
        /// if guard is able to sprint to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't sprint to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiFixedCommandDescription GuardTrySprintingToChrPosition
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_sprinting_to_chr_position",
                    CommandId = 46,
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
        /// if guard has stopped moving, goto label
        /// </summary>
        /// <remarks>
        /// check if guard isn't looking for bond or if guard has finished moving to destination
        /// </remarks>
        public static AiFixedCommandDescription IfGuardHasStoppedMoving
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_has_stopped_moving",
                    CommandId = 47,
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
        /// if chr has died (or in dying state), goto label
        /// </summary>
        public static AiFixedCommandDescription IfChrDyingOrDead
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_dying_or_dead",
                    CommandId = 48,
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
        /// if chr doesn't exist (died and faded/not spawned), goto label
        /// </summary>
        /// <remarks>
        /// this command is used to check if chr has finished dying animation and faded away, or chr num is free
        /// </remarks>
        public static AiFixedCommandDescription IfChrDoesNotExist
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_does_not_exist",
                    CommandId = 49,
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
    }
}
