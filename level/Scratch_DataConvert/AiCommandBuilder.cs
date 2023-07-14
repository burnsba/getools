using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class AiCommandBuilder
    {
        /// <summary>
        /// goto the next label (command 02) - skips all commands between command and goto label - continues executing after found label
        /// </summary>
        public static AiCommandDescription GotoNext
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "goto_next",
                    CommandId = 0,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// like goto_next, but it starts scanning label from start of list
        /// </summary>
        public static AiCommandDescription GotoFirst
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "goto_first",
                    CommandId = 1,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// label marker for ai list - used for all commands that return true
        /// </summary>
        public static AiCommandDescription Label
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "label",
                    CommandId = 2,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "id", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// halt the ai list - frees engine to start executing next ai list until all lists have been executed for game tick.
        /// </summary>
        /// <remarks>
        /// offscreen/idle guards will take 14 game ticks instead of 1 tick on ai_sleep
        /// </remarks>
        public static AiCommandDescription AiSleep
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "ai_sleep",
                    CommandId = 3,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// used for ai list parser to check when list ends
        /// </summary>
        /// <remarks>
        /// not recommended to execute this command - to finish a list create an infinite loop (goto_loop_infinite) or jump to GLIST_END_ROUTINE when list has finished tasks
        /// </remarks>
        public static AiCommandDescription AiListEnd
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "ai_list_end",
                    CommandId = 4,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// set chr num's current ai list program counter to beginning of a list
        /// </summary>
        /// <remarks>
        /// not recommended to goto an obj list (10XX)
        /// </remarks>
        public static AiCommandDescription JumpToAiList
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "jump_to_ai_list",
                    CommandId = 5,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "ai_list", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// store a list ptr in current chr struct - used for command 07 return
        /// </summary>
        /// <remarks>
        /// not recommended to set stored list to an obj list (10XX)
        /// </remarks>
        public static AiCommandDescription SetReturnAiList
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "set_return_ai_list",
                    CommandId = 6,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "ai_list", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// jump the return ai list set in chr struct - pointer set by command 06. used for subroutine lists. if list pointer isn't set, game will crash
        /// </summary>
        /// <remarks>
        /// after return, set chr->aioffset to top of ai list
        /// </remarks>
        public static AiCommandDescription JumpToReturnAiList
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "jump_to_return_ai_list",
                    CommandId = 7,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// reset guard back to idle pose - can be used to stop guards in place
        /// </summary>
        public static AiCommandDescription GuardAnimationStop
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_animation_stop",
                    CommandId = 8,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// make guard kneel on one knee
        /// </summary>
        public static AiCommandDescription GuardKneel
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_kneel",
                    CommandId = 9,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// set guard to playback animation
        /// </summary>
        /// <remarks>
        ///  start/end set to -1/-1 will playback the entire animation length. interpolation time will set how long it will take to transition from the previous state. if interpolation time is too low it may crash! - use 0x10 if unsure. start/end keyframe uses animation 30 tick units - interpolation use 60 tick units. use ANIM_# flags for bitfield argument
        /// </remarks>
        public static AiCommandDescription GuardPlayAnimation
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_play_animation",
                    CommandId = 10,
                    CommandLengthBytes = 9,
                    NumberParameters = 5,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "animation_id", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "start_time30", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "end_time30", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "interpol_time60", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is in animation playback state (ACT_ANIM), goto label
        /// </summary>
        public static AiCommandDescription IfGuardPlayingAnimation
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_playing_animation",
                    CommandId = 11,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// guard points if bond is directly in front of guard, else command is ignored
        /// </summary>
        /// <remarks>
        /// global ai list GLIST_FIRE_RAND_ANIM_SUBROUTINE skips this command if bitfield flag BITFIELD_DONT_POINT_AT_BOND is on
        /// </remarks>
        public static AiCommandDescription GuardPointsAtBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_points_at_bond",
                    CommandId = 12,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// set guard to playback animation - used when shots land near guard
        /// </summary>
        public static AiCommandDescription GuardLooksAroundSelf
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_looks_around_self",
                    CommandId = 13,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// trigger guard to sidestep, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiCommandDescription GuardTrySidestepping
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_sidestepping",
                    CommandId = 14,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to hop sideways, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiCommandDescription GuardTryHoppingSideways
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_hopping_sideways",
                    CommandId = 15,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to run sideways of bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// direction is random
        /// </remarks>
        public static AiCommandDescription GuardTryRunningToSide
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_running_to_side",
                    CommandId = 16,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to walk and fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond needs to be at long distance away from guard to work
        /// </remarks>
        public static AiCommandDescription GuardTryFiringWalk
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_firing_walk",
                    CommandId = 17,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to run and fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond needs to be at long distance away from guard to work
        /// </remarks>
        public static AiCommandDescription GuardTryFiringRun
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_firing_run",
                    CommandId = 18,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to roll on ground then fire at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// bond cannot be too close to guard or it won't work
        /// </remarks>
        public static AiCommandDescription GuardTryFiringRoll
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_firing_roll",
                    CommandId = 19,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// make guard aim/fire their weapon at target, goto label if successful
        /// </summary>
        /// <remarks>
        /// bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiCommandDescription GuardTryFireOrAimAtTarget
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target",
                    CommandId = 20,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "target", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// make guard kneel and aim/fire their weapon at target, goto label if successful
        /// </summary>
        /// <remarks>
        /// bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiCommandDescription GuardTryFireOrAimAtTargetKneel
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target_kneel",
                    CommandId = 21,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "target", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// update guard's aim/fire target, goto label if successful
        /// </summary>
        /// <remarks>
        /// this command only works if guard is currently aiming at a target. bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument
        /// </remarks>
        public static AiCommandDescription GuardTryFireOrAimAtTargetUpdate
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_fire_or_aim_at_target_update",
                    CommandId = 22,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "target", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// make guard continuously face target, goto label if successful
        /// </summary>
        /// <remarks>
        /// if guard was shot while facing target, guard will snap out of facing state. bitfield argument is used to set the target type (pad/bond/chr). use TARGET_# flags for bitfield argument. command can't use TARGET_AIM_ONLY flag
        /// </remarks>
        public static AiCommandDescription GuardTryFacingTarget
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_facing_target",
                    CommandId = 23,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "target", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// hit chr's body part with item's damage, play reaction to hit location
        /// </summary>
        /// <remarks>
        /// command does not trigger item's fire sfx. item's damage uses body part damage modifier. use HIT_# define for hit part number
        /// </remarks>
        public static AiCommandDescription ChrHitBodyPartWithItemDamage
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_hit_body_part_with_item_damage",
                    CommandId = 24,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "part_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// chr hits chr's body part with held item, play reaction to hit location
        /// </summary>
        /// <remarks>
        /// command does not trigger item's fire sfx or chr firing animation. item's damage uses body part damage modifier. use HIT_# define for hit part number
        /// </remarks>
        public static AiCommandDescription ChrHitChrBodyPartWithHeldItem
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_hit_chr_body_part_with_held_item",
                    CommandId = 25,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_num_target", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "part_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger guard to throw a grenade at bond, goto label if successful
        /// </summary>
        /// <remarks>
        /// a rng byte is generated and compared again chr->grenadeprob, if rng byte is less than grenadeprob throw grenade and goto label, else do nothing. chr->grenadeprob default is 0 - to change use setup object 12 or command 8D
        /// </remarks>
        public static AiCommandDescription GuardTryThrowingGrenade
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_throwing_grenade",
                    CommandId = 26,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// spawn and drop item with prop model from guard, goto label if successful
        /// </summary>
        /// <remarks>
        /// dropped item uses item type (08) with model number - they can be picked up. grenade/mines will be dropped live - this is used for cradle (list #0411)
        /// </remarks>
        public static AiCommandDescription GuardTryDroppingItem
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_dropping_item",
                    CommandId = 27,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "prop_num", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// makes the guard run to pad
        /// </summary>
        public static AiCommandDescription GuardRunsToPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_runs_to_pad",
                    CommandId = 28,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// makes the guard run to guard->padpreset1 (PAD_PRESET - 9000)
        /// </summary>
        public static AiCommandDescription GuardRunsToPadPreset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_runs_to_pad_preset",
                    CommandId = 29,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// makes the guard walk to pad
        /// </summary>
        public static AiCommandDescription GuardWalksToPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_walks_to_pad",
                    CommandId = 30,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// makes the guard sprint to pad
        /// </summary>
        public static AiCommandDescription GuardSprintsToPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_sprints_to_pad",
                    CommandId = 31,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// makes guard walk a predefined path within setup
        /// </summary>
        /// <remarks>
        /// usually paired with goto GLIST_DETECT_BOND_DEAF_NO_CLONE_NO_IDLE_ANIM or GLIST_DETECT_BOND_NO_CLONE_NO_IDLE_ANIM
        /// </remarks>
        public static AiCommandDescription GuardStartPatrol
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_start_patrol",
                    CommandId = 32,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "path_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// makes a guard surrender and drop all attached and held items
        /// </summary>
        /// <remarks>
        /// will not drop items embedded within guard
        /// </remarks>
        public static AiCommandDescription GuardSurrenders
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_surrenders",
                    CommandId = 33,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// sets guard to fade away - fade time is 90 ticks (1.5 seconds). when the fade finishes, automatically remove guard
        /// </summary>
        /// <remarks>
        /// guard collision is ignored during fade - will not drop items
        /// </remarks>
        public static AiCommandDescription GuardRemoveFade
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_remove_fade",
                    CommandId = 34,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// instantly remove chr unlike above command
        /// </summary>
        /// <remarks>
        /// will not drop items
        /// </remarks>
        public static AiCommandDescription ChrRemoveInstant
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_remove_instant",
                    CommandId = 35,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// guard activates alarm assigned to pad, goto label if successful
        /// </summary>
        /// <remarks>
        /// command doesn't care what object type is at pad, as long as the object isn't destroyed. command also checks if guard is alive before activating alarm. when triggering alarm, guard will be set to state ACT_STARTALARM and play animation
        /// </remarks>
        public static AiCommandDescription GuardTryTriggeringAlarmAtPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_triggering_alarm_at_pad",
                    CommandId = 36,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// activates alarm
        /// </summary>
        public static AiCommandDescription AlarmOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "alarm_on",
                    CommandId = 37,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// deactivates alarm
        /// </summary>
        public static AiCommandDescription AlarmOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "alarm_off",
                    CommandId = 38,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// command no longer exists, never goto label
        /// </summary>
        public static AiCommandDescription RemovedCommand27
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "removed_command27",
                    CommandId = 39,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to run to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't run to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiCommandDescription GuardTryRunningToBondPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_running_to_bond_position",
                    CommandId = 40,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to walk to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't walk to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiCommandDescription GuardTryWalkingToBondPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_walking_to_bond_position",
                    CommandId = 41,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to sprint to bond, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't sprint to bond (guard has died) or bond is at an unreachable area (no navigation pads in area)
        /// </remarks>
        public static AiCommandDescription GuardTrySprintingToBondPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_sprinting_to_bond_position",
                    CommandId = 42,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// command no longer exists, never goto label
        /// </summary>
        public static AiCommandDescription RemovedCommand2B
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "removed_command2B",
                    CommandId = 43,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to run to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't run to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiCommandDescription GuardTryRunningToChrPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_running_to_chr_position",
                    CommandId = 44,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to walk to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't walk to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiCommandDescription GuardTryWalkingToChrPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_walking_to_chr_position",
                    CommandId = 45,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is able to sprint to chr, goto label
        /// </summary>
        /// <remarks>
        /// don't goto label if guard can't sprint to chr (guard has died) or chr is at an unreachable area (no navigation pads in area) or chr doesn't exist
        /// </remarks>
        public static AiCommandDescription GuardTrySprintingToChrPosition
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_sprinting_to_chr_position",
                    CommandId = 46,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard has stopped moving, goto label
        /// </summary>
        /// <remarks>
        /// check if guard isn't looking for bond or if guard has finished moving to destination
        /// </remarks>
        public static AiCommandDescription IfGuardHasStoppedMoving
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_has_stopped_moving",
                    CommandId = 47,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr has died (or in dying state), goto label
        /// </summary>
        public static AiCommandDescription IfChrDyingOrDead
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_dying_or_dead",
                    CommandId = 48,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr doesn't exist (died and faded/not spawned), goto label
        /// </summary>
        /// <remarks>
        /// this command is used to check if chr has finished dying animation and faded away, or chr num is free
        /// </remarks>
        public static AiCommandDescription IfChrDoesNotExist
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_does_not_exist",
                    CommandId = 49,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// check vision for bond, goto label if spotted bond
        /// </summary>
        /// <remarks>
        /// uses chr->visionrange while checking for bond. once bond has been spotted, check if bond and guard are within line of sight (ignores facing direction). injured guards will also set spotted Bond state (won't work with invincible/armored guards). if bond breaks line of sight, do not goto label. if bond has broken line of sight for more than 10 seconds, reset spotted bond state. when using with command 3E, make sure 32 takes priority over command 3E
        /// </remarks>
        public static AiCommandDescription IfGuardSeesBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_sees_bond",
                    CommandId = 50,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// generate a random byte and store to chr->random
        /// </summary>
        /// <remarks>
        /// random byte range is 00-FF (unsigned)
        /// </remarks>
        public static AiCommandDescription RandomGenerateSeed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "random_generate_seed",
                    CommandId = 51,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if chr->random < byte, goto label
        /// </summary>
        /// <remarks>
        /// compare is unsigned
        /// </remarks>
        public static AiCommandDescription IfRandomSeedLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_random_seed_less_than",
                    CommandId = 52,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "cbyte", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->random > byte, goto label
        /// </summary>
        /// <remarks>
        /// compare is unsigned
        /// </remarks>
        public static AiCommandDescription IfRandomSeedGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_random_seed_greater_than",
                    CommandId = 53,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "cbyte", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if alarm is activated, goto label
        /// </summary>
        /// <remarks>
        /// this command works but is unused in retail game, use command 37 instead
        /// </remarks>
        public static AiCommandDescription IfAlarmIsOnUnused
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_alarm_is_on_unused",
                    CommandId = 54,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if alarm is activated, goto label
        /// </summary>
        public static AiCommandDescription IfAlarmIsOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_alarm_is_on",
                    CommandId = 55,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if gas leak event triggered, goto label
        /// </summary>
        /// <remarks>
        /// once gas leak event has started, always goto label
        /// </remarks>
        public static AiCommandDescription IfGasIsLeaking
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_gas_is_leaking",
                    CommandId = 56,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard heard bond fire weapon, goto label
        /// </summary>
        /// <remarks>
        /// uses chr->hearingscale while listening for bond. to check if bond has shot within the last 10 seconds, use command 3F
        /// </remarks>
        public static AiCommandDescription IfGuardHeardBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_heard_bond",
                    CommandId = 57,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard sees another guard shot (from anyone), goto label
        /// </summary>
        /// <remarks>
        /// guard friendly fire (if flagged) will trigger this command to goto label. command checks if chr->chrseeshot is set to valid chrnum (not -1). does not work with shot invincible/armoured guards
        /// </remarks>
        public static AiCommandDescription IfGuardSeeAnotherGuardShot
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_see_another_guard_shot",
                    CommandId = 58,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard sees another guard die (from anyone), goto label
        /// </summary>
        /// <remarks>
        /// when a guard in sight switches to ACT_DIE/ACT_DEAD, goto label. command checks if chr->chrseedie is set to valid chrnum (not -1)
        /// </remarks>
        public static AiCommandDescription IfGuardSeeAnotherGuardDie
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_see_another_guard_die",
                    CommandId = 59,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard and bond are within line of sight, goto label
        /// </summary>
        /// <remarks>
        /// line of sight uses clipping - ignores facing direction of bond/guard. if prop/guard is in the way do not goto label. does not use chr->visionrange for line of sight check. use command 32 to check using chr->visionrange and command 42 to account for bond's view
        /// </remarks>
        public static AiCommandDescription IfGuardAndBondWithinLineOfSight
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_and_bond_within_line_of_sight",
                    CommandId = 60,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard and bond are within partial line of sight, goto label
        /// </summary>
        /// <remarks>
        /// unused command, functions like above but only goto label if bond is half occluded by clipping (not blocked or within full view)
        /// </remarks>
        public static AiCommandDescription IfGuardAndBondWithinPartialLineOfSight
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_and_bond_within_partial_line_of_sight",
                    CommandId = 61,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard was shot (from anyone) or saw Bond within the last 10 seconds, goto label
        /// </summary>
        /// <remarks>
        /// command will not count guard as shot if they are invincible/have armour. if guard saw Bond (using command 32) in the last 10 seconds, goto label. when using with command 32, make sure 32 takes priority over command 3E. if guard was injured within the last 10 seconds, goto label when finished injury reaction animation (will not work with invincible/armored guards). to check if guard was hit/damaged use commands 7E/F8 instead, or check if guard flags CHRFLAG_WAS_DAMAGED/CHRFLAG_WAS_HIT are set using command 9F/A2
        /// </remarks>
        public static AiCommandDescription IfGuardWasShotOrSeenBondWithinLast10Secs
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_was_shot_or_seen_bond_within_last_10_secs",
                    CommandId = 62,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard heard bond fire weapon within the last 10 seconds, goto label
        /// </summary>
        /// <remarks>
        /// uses chr->hearingscale while listening for bond. to check if bond has now fired weapon instead of within the last 10 seconds, use command 39
        /// </remarks>
        public static AiCommandDescription IfGuardHeardBondWithinLast10Secs
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_heard_bond_within_last_10_secs",
                    CommandId = 63,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is in same room with chr, goto label
        /// </summary>
        public static AiCommandDescription IfGuardInRoomWithChr
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_in_room_with_chr",
                    CommandId = 64,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard has not been seen before on screen, goto label
        /// </summary>
        /// <remarks>
        /// when bond has seen guard, it will add a flag to chr->chrflags. the seen flag will be kept true for duration of level
        /// </remarks>
        public static AiCommandDescription IfGuardHasNotBeenSeen
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_has_not_been_seen",
                    CommandId = 65,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is currently being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// portals will affect this command's output. if guard is being culled off screen, command will not goto label
        /// </remarks>
        public static AiCommandDescription IfGuardIsOnScreen
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_is_on_screen",
                    CommandId = 66,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if the room containing guard is being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// only checks if room is being rendered, not if bond can see guard. to check if guard is being rendered use command 42 instead.
        /// </remarks>
        public static AiCommandDescription IfGuardRoomContainingSelfIsOnScreen
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_room_containing_self_is_on_screen",
                    CommandId = 67,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if room containing pad is being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// only checks if room is being rendered, not if bond can see inside room
        /// </remarks>
        public static AiCommandDescription IfRoomContainingPadIsOnScreen
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_room_containing_pad_is_on_screen",
                    CommandId = 68,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond is looking/aiming at guard, goto label
        /// </summary>
        /// <remarks>
        /// also checks if crosshair is aiming at guard
        /// </remarks>
        public static AiCommandDescription IfGuardIsTargetedByBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_is_targeted_by_bond",
                    CommandId = 69,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's shot missed/landed near guard, goto label
        /// </summary>
        /// <remarks>
        /// command will sometimes goto label if guard was shot - use command 3E instead to check if guard was shot recently (more consistent)
        /// </remarks>
        public static AiCommandDescription IfGuardShotFromBondMissed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_shot_from_bond_missed",
                    CommandId = 70,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's counter-clockwise direction to bond < direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): 00: no rotation, never goto label because degrees are always above 0 40: bond and guard within 9-to-12 o'clock (90 degrees) 80: bond is on guard's left-side (180 degrees) C0: bond and guard within 3-to-12 o'clock (270 degrees) FF: full rotation, always goto label except for a tiny degree (0-359 degrees)
        /// </remarks>
        public static AiCommandDescription IfGuardCounterClockwiseDirectionToBondLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_to_bond_less_than",
                    CommandId = 71,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "direction", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's counter-clockwise direction to bond > direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): FF: no rotation, never goto label except for a tiny degree (0-1 degrees) C0: bond and guard within 12-to-3 o'clock (90 degrees) 80: bond on guard's right-side (180 degrees) 40: bond and guard within 12-to-9 o'clock (270 degrees) 00: full rotation, always goto label
        /// </remarks>
        public static AiCommandDescription IfGuardCounterClockwiseDirectionToBondGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_to_bond_greater_than",
                    CommandId = 72,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "direction", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's counter-clockwise direction to guard < direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): 00: no rotation, never goto label because degrees are always above 0 40: guard and bond within 9-to-12 o'clock (90 degrees) 80: guard is on bond's left-side (180 degrees) C0: guard and bond within 3-to-12 o'clock (270 degrees) FF: full rotation, always goto label except for a tiny degree (0-359 degrees)
        /// </remarks>
        public static AiCommandDescription IfGuardCounterClockwiseDirectionFromBondLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_from_bond_less_than",
                    CommandId = 73,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "direction", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's counter-clockwise direction to guard > direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): FF: no rotation, never goto label except for a tiny degree (0-1 degrees) C0: guard and bond within 12-to-3 o'clock (90 degrees) 80: guard on bond's right-side (180 degrees) 40: guard and bond within 12-to-9 o'clock (270 degrees) 00: full rotation, always goto label
        /// </remarks>
        public static AiCommandDescription IfGuardCounterClockwiseDirectionFromBondGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_from_bond_greater_than",
                    CommandId = 74,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "direction", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's distance to bond < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfGuardDistanceToBondLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_distance_to_bond_less_than",
                    CommandId = 75,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's distance to bond > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfGuardDistanceToBondGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_distance_to_bond_greater_than",
                    CommandId = 76,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr's distance to pad < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfChrDistanceToPadLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_distance_to_pad_less_than",
                    CommandId = 77,
                    CommandLengthBytes = 7,
                    NumberParameters = 4,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr's distance to pad > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfChrDistanceToPadGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_distance_to_pad_greater_than",
                    CommandId = 78,
                    CommandLengthBytes = 7,
                    NumberParameters = 4,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's distance to chr < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfGuardDistanceToChrLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_distance_to_chr_less_than",
                    CommandId = 79,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's distance to chr > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfGuardDistanceToChrGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_distance_to_chr_greater_than",
                    CommandId = 80,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's distance to any chr < distance argument, set chr->padpreset1 to found guard's chrnum and goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter. command does not pick the closest found chr, but whoever was first found within the distance argument. if no guards were found within distance range, do not goto label
        /// </remarks>
        public static AiCommandDescription GuardTrySettingChrPresetToGuardWithinDistance
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_setting_chr_preset_to_guard_within_distance",
                    CommandId = 81,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's distance to pad < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfBondDistanceToPadLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_distance_to_pad_less_than",
                    CommandId = 82,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's distance to pad > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiCommandDescription IfBondDistanceToPadGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_distance_to_pad_greater_than",
                    CommandId = 83,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr id in same room with pad, goto label
        /// </summary>
        public static AiCommandDescription IfChrInRoomWithPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_in_room_with_pad",
                    CommandId = 84,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond in same room with pad, goto label
        /// </summary>
        public static AiCommandDescription IfBondInRoomWithPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_in_room_with_pad",
                    CommandId = 85,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond collected tagged object, goto label
        /// </summary>
        public static AiCommandDescription IfBondCollectedObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_collected_object",
                    CommandId = 86,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if item exists in level and is stationary (not moving/in mid-air), goto label
        /// </summary>
        /// <remarks>
        /// used to check if bond threw an item in level. also checks if item was attached to an object (item is stationary within level). so make sure command 58 takes priority over command 57 when using both commands
        /// </remarks>
        public static AiCommandDescription IfItemIsStationaryWithinLevel
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_item_is_stationary_within_level",
                    CommandId = 87,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if item was thrown onto tagged object, goto label
        /// </summary>
        /// <remarks>
        /// used to check if bond threw an item onto a tagged object. if used with command 57, make sure command 58 take priority over command 57
        /// </remarks>
        public static AiCommandDescription IfItemIsAttachedToObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_item_is_attached_to_object",
                    CommandId = 88,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond has an item equipped (currently held), goto label
        /// </summary>
        public static AiCommandDescription IfBondHasItemEquipped
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_has_item_equipped",
                    CommandId = 89,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged object exists in level, goto label
        /// </summary>
        public static AiCommandDescription IfObjectExists
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_exists",
                    CommandId = 90,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged object is not destroyed, goto label
        /// </summary>
        public static AiCommandDescription IfObjectNotDestroyed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_not_destroyed",
                    CommandId = 91,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged object was activated since last check, goto label
        /// </summary>
        /// <remarks>
        /// when executed, it will clear tagged object's activated flag. only bond and command 5E can activate tagged objects. bond cannot activate destroyed objects
        /// </remarks>
        public static AiCommandDescription IfObjectWasActivated
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_was_activated",
                    CommandId = 92,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond used a gadget item on a tagged object since last check, goto label
        /// </summary>
        /// <remarks>
        /// gadgets are a pre-defined list of items set to gadget flag: ITEM_BOMBDEFUSER ITEM_DATATHIEF ITEM_DOORDECODER ITEM_EXPLOSIVEFLOPPY ITEM_DATTAPE
        /// </remarks>
        public static AiCommandDescription IfBondUsedGadgetOnObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_used_gadget_on_object",
                    CommandId = 93,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// activate a tagged object
        /// </summary>
        /// <remarks>
        /// command does not check if object has been destroyed
        /// </remarks>
        public static AiCommandDescription ObjectActivate
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_activate",
                    CommandId = 94,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// destroy/explode a tagged object
        /// </summary>
        /// <remarks>
        /// only works if object is not destroyed. cannot destroy invincible objects
        /// </remarks>
        public static AiCommandDescription ObjectDestroy
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_destroy",
                    CommandId = 95,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// drop tagged object held/attached to chr
        /// </summary>
        /// <remarks>
        /// item must be held/attached to a chr. embedded objects will not drop, only works with attached objects. props can be damaged on drop
        /// </remarks>
        public static AiCommandDescription ObjectDropFromChr
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_drop_from_chr",
                    CommandId = 96,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// make chr drop all concealed attachments
        /// </summary>
        /// <remarks>
        /// item must be attached to chr, to drop held items use command 62. embedded objects will not drop, only works with attached objects. props can be damaged on drop
        /// </remarks>
        public static AiCommandDescription ChrDropAllConcealedItems
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_drop_all_concealed_items",
                    CommandId = 97,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// make chr drop all held items
        /// </summary>
        /// <remarks>
        /// items must be held by chr, to drop concealed attachments use command 61. embedded objects will not drop, only works with attached objects
        /// </remarks>
        public static AiCommandDescription ChrDropAllHeldItems
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_drop_all_held_items",
                    CommandId = 98,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// force bond to instantly collect a tagged object
        /// </summary>
        /// <remarks>
        /// does not trigger bottom text telling player they collected an item
        /// </remarks>
        public static AiCommandDescription BondCollectObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_collect_object",
                    CommandId = 99,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// makes chr hold tagged object
        /// </summary>
        /// <remarks>
        /// if chr's hands are occupied, object will be equipped as an concealed attachment. but if tagged object's handedness flag is free on guard then guard will equip weapon. tagged object's prop must have a holding position command within the model file
        /// </remarks>
        public static AiCommandDescription ChrEquipObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_equip_object",
                    CommandId = 100,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// move object to pad
        /// </summary>
        /// <remarks>
        /// if object is assigned to padextra type, then object scale will be lost after moving to target pad. object will inherit rotation from target pad
        /// </remarks>
        public static AiCommandDescription ObjectMoveToPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_move_to_pad",
                    CommandId = 101,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// open tagged door
        /// </summary>
        /// <remarks>
        /// open tagged door even if locked
        /// </remarks>
        public static AiCommandDescription DoorOpen
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "door_open",
                    CommandId = 102,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// close tagged door
        /// </summary>
        public static AiCommandDescription DoorClose
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "door_close",
                    CommandId = 103,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged door state matches any of bitfield argument, goto label
        /// </summary>
        /// <remarks>
        /// use DOOR_STATE_# flags for door state argument. flags can be combined
        /// </remarks>
        public static AiCommandDescription IfDoorStateEqual
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_door_state_equal",
                    CommandId = 104,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "door_state", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged door has been opened before, goto label
        /// </summary>
        /// <remarks>
        /// if tagged door is open by default in setup, then it must be closed before it will check if opened again
        /// </remarks>
        public static AiCommandDescription IfDoorHasBeenOpenedBefore
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_door_has_been_opened_before",
                    CommandId = 105,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set tagged door's lock with flags
        /// </summary>
        /// <remarks>
        /// use DOOR_LOCK_# flags for lock argument. lock flags are same as used within setup for doors and keys
        /// </remarks>
        public static AiCommandDescription DoorSetLock
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "door_set_lock",
                    CommandId = 106,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "lock_flag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// unset tagged door's lock with flags
        /// </summary>
        /// <remarks>
        /// use DOOR_LOCK_# flags for lock argument. lock flags are same as used within setup for doors and keys
        /// </remarks>
        public static AiCommandDescription DoorUnsetLock
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "door_unset_lock",
                    CommandId = 107,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "lock_flag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged door's lock flags matches any lock flag argument, goto label
        /// </summary>
        /// <remarks>
        /// use DOOR_LOCK_# flags for lock argument. lock flags are same as used within setup for doors and keys
        /// </remarks>
        public static AiCommandDescription IfDoorLockEqual
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_door_lock_equal",
                    CommandId = 108,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "lock_flag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if objective # completed, goto label
        /// </summary>
        /// <remarks>
        /// ignores difficulty settings. for example - if game on agent and player completes an unlisted 00 agent objective, checking that objective num will goto label
        /// </remarks>
        public static AiCommandDescription IfObjectiveNumComplete
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_objective_num_complete",
                    CommandId = 109,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "obj_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// unknown command, goto label
        /// </summary>
        /// <remarks>
        /// sets chr->padpreset1 bitfield (hex): 0001: sets to nearest pad to path to bond 0004: ??? 0008: ??? 0010: ??? 0020: ???
        /// </remarks>
        public static AiCommandDescription GuardTryUnknown6E
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_unknown6E",
                    CommandId = 110,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "unknown_flag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// unknown command, goto label
        /// </summary>
        /// <remarks>
        /// sets chr->padpreset1 bitfield (hex): 0001: set to nearest pad in direction of bond 0004: ??? 0008: ??? 0010: ??? 0020: ???
        /// </remarks>
        public static AiCommandDescription GuardTryUnknown6F
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_unknown6F",
                    CommandId = 111,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "unknown_flag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current difficulty < difficulty argument, goto label
        /// </summary>
        /// <remarks>
        /// provided argument will compare the following difficult settings 01: agent only 02: agent/secret agent 03: agent/secret agent/00 agent
        /// </remarks>
        public static AiCommandDescription IfGameDifficultyLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_game_difficulty_less_than",
                    CommandId = 112,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "argument", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current difficulty > difficulty argument, goto label
        /// </summary>
        /// <remarks>
        /// provided argument will compare the following difficult settings 00: secret agent/00 agent/007 01: 00 agent/007 02: 007 only
        /// </remarks>
        public static AiCommandDescription IfGameDifficultyGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_game_difficulty_greater_than",
                    CommandId = 113,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "argument", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current mission time (in seconds) < seconds argument, goto label
        /// </summary>
        /// <remarks>
        /// converts (unsigned) seconds to float and compares against mission timer
        /// </remarks>
        public static AiCommandDescription IfMissionTimeLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_mission_time_less_than",
                    CommandId = 114,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current mission time (in seconds) > seconds argument, goto label
        /// </summary>
        /// <remarks>
        /// converts (unsigned) seconds to float and compares against mission timer
        /// </remarks>
        public static AiCommandDescription IfMissionTimeGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_mission_time_greater_than",
                    CommandId = 115,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if system powered on time (in minutes) < minutes argument, goto label
        /// </summary>
        /// <remarks>
        /// converts (unsigned) minutes to float and compares against system time
        /// </remarks>
        public static AiCommandDescription IfSystemPowerTimeLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_system_power_time_less_than",
                    CommandId = 116,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "minutes", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if system powered on time (in minutes) > minutes argument, goto label
        /// </summary>
        /// <remarks>
        /// converts (unsigned) minutes to float and compares against system time
        /// </remarks>
        public static AiCommandDescription IfSystemPowerTimeGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_system_power_time_greater_than",
                    CommandId = 117,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "minutes", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current level id < level id argument, goto label
        /// </summary>
        /// <remarks>
        /// level id uses LEVELID enum values, not briefing menu stage number
        /// </remarks>
        public static AiCommandDescription IfLevelIdLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_level_id_less_than",
                    CommandId = 118,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "level_id", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current level id > level id argument, goto label
        /// </summary>
        /// <remarks>
        /// level id uses LEVELID enum values, not briefing menu stage number
        /// </remarks>
        public static AiCommandDescription IfLevelIdGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_level_id_greater_than",
                    CommandId = 119,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "level_id", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's hits taken < hit_num, goto label
        /// </summary>
        /// <remarks>
        /// compares signed byte against chr->numarghs. hits count even if guard is invincible
        /// </remarks>
        public static AiCommandDescription IfGuardHitsLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_hits_less_than",
                    CommandId = 120,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "hit_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard's hits taken > hit_num, goto label
        /// </summary>
        /// <remarks>
        /// compares signed byte against chr->numarghs. hits count even if guard is invincible
        /// </remarks>
        public static AiCommandDescription IfGuardHitsGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_hits_greater_than",
                    CommandId = 121,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "hit_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's shot missed/landed near guard total < missed_num, goto label
        /// </summary>
        /// <remarks>
        /// compares signed byte against chr->numclosearghs
        /// </remarks>
        public static AiCommandDescription IfGuardHitsMissedLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_hits_missed_less_than",
                    CommandId = 122,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "missed_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's shot missed/landed near guard total > missed_num, goto label
        /// </summary>
        /// <remarks>
        /// compares signed byte argument against chr->numclosearghs
        /// </remarks>
        public static AiCommandDescription IfGuardHitsMissedGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_hits_missed_greater_than",
                    CommandId = 123,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "missed_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr's health < health argument, goto label
        /// </summary>
        /// <remarks>
        /// argument is unsigned. converted to float and compares different between chr->maxdamage - chr->damage. default guard health is 40 (0x28), or after float conversion 4.0f. armour is tested
        /// </remarks>
        public static AiCommandDescription IfChrHealthLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_health_less_than",
                    CommandId = 124,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "health", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr's health > health argument, goto label
        /// </summary>
        /// <remarks>
        /// argument is unsigned. converted to float and compares different between chr->maxdamage - chr->damage. default guard health is 40 (0x28), or after float conversion 4.0f. armour is tested
        /// </remarks>
        public static AiCommandDescription IfChrHealthGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_health_greater_than",
                    CommandId = 125,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "health", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr has taken damage since last check, goto label
        /// </summary>
        /// <remarks>
        /// checks chr->chrflags if CHRFLAG_WAS_DAMAGED is set. if true, unset flag and goto label. CHRFLAG_WAS_DAMAGED is set if guard took damage (not invincible)
        /// </remarks>
        public static AiCommandDescription IfChrWasDamagedSinceLastCheck
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_was_damaged_since_last_check",
                    CommandId = 126,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's health < health argument, goto label
        /// </summary>
        /// <remarks>
        /// does not check armour. health argument is unsigned, argument range is between 00 and FF, with FF equal to 100% health
        /// </remarks>
        public static AiCommandDescription IfBondHealthLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_health_less_than",
                    CommandId = 127,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "health", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's health > health argument, goto label
        /// </summary>
        /// <remarks>
        /// does not check armour. health argument is unsigned, argument range is between 00 and FF, with FF equal to 100% health
        /// </remarks>
        public static AiCommandDescription IfBondHealthGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_health_greater_than",
                    CommandId = 128,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "health", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->flags byte value to byte argument
        /// </summary>
        /// <remarks>
        /// argument is unsigned. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte1Set
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_1_set",
                    CommandId = 129,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "set_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// add byte argument to chr->flags byte value
        /// </summary>
        /// <remarks>
        /// argument is unsigned, add value is clamped at 0xFF (255 dec). this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte1Add
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_1_add",
                    CommandId = 130,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "add_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// subtract byte argument from chr->flags byte value
        /// </summary>
        /// <remarks>
        /// argument is unsigned, subtract value is clamped at 0. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte1Subtract
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_1_subtract",
                    CommandId = 131,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "subtract_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->flags byte value < byte argument, goto label
        /// </summary>
        /// <remarks>
        /// argument is unsigned. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription IfLocalByte1LessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_byte_1_less_than",
                    CommandId = 132,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "compare_byte", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->flags byte value < chr->random, goto label
        /// </summary>
        /// <remarks>
        /// chr->random must be pre-generated by command 33 before comparing. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription IfLocalByte1LessThanRandomSeed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_byte_1_less_than_random_seed",
                    CommandId = 133,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->flags2 byte value to byte argument
        /// </summary>
        /// <remarks>
        /// argument is unsigned. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte2Set
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_2_set",
                    CommandId = 134,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "set_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// add byte argument to chr->flags2 byte value
        /// </summary>
        /// <remarks>
        /// argument is unsigned, add value is clamped at 0xFF (255 dec). this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte2Add
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_2_add",
                    CommandId = 135,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "add_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// subtract byte argument from chr->flags2 byte value
        /// </summary>
        /// <remarks>
        /// argument is unsigned, subtract value is clamped at 0. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription LocalByte2Subtract
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_byte_2_subtract",
                    CommandId = 136,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "subtract_byte", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->flags2 byte value < byte argument, goto label
        /// </summary>
        /// <remarks>
        /// argument is unsigned. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription IfLocalByte2LessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_byte_2_less_than",
                    CommandId = 137,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "compare_byte", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->flags2 byte value < chr->random, goto label
        /// </summary>
        /// <remarks>
        /// chr->random must be pre-generated by command 33 before comparing. this is a private byte that is stored in chr struct. it can be used for anything. default value is 0
        /// </remarks>
        public static AiCommandDescription IfLocalByte2LessThanRandomSeed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_byte_2_less_than_random_seed",
                    CommandId = 138,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's hearing scale - the higher the value, the further away guard can hear bond's gunfire
        /// </summary>
        /// <remarks>
        /// sets to chr->hearingscale. default value is 0x03E8 (1000 dec). argument is converted to float and divided by 1000 before setting to hearingscale
        /// </remarks>
        public static AiCommandDescription GuardSetHearingScale
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_hearing_scale",
                    CommandId = 139,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "hearing_scale", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's vision range - the smaller the value, the longer the guard takes to detect bond with command 32. does not affect firing distance
        /// </summary>
        /// <remarks>
        /// sets to chr->visionrange. default value is 0x0064 (100 dec). argument is unsigned and converted to float before setting to hearingscale
        /// </remarks>
        public static AiCommandDescription GuardSetVisionRange
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_vision_range",
                    CommandId = 140,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "vision_range", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's grenade probability - used for rng comparison by command 1A. the higher the value, the likelyhood of guard throwing a grenade
        /// </summary>
        /// <remarks>
        /// sets to chr->grenadeprob - 0 by default. argument is unsigned. the only way to make guards throw grenades is by using this command or assigning setup object 0x12 to chr
        /// </remarks>
        public static AiCommandDescription GuardSetGrenadeProbability
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_grenade_probability",
                    CommandId = 141,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "grenade_prob", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's chr num
        /// </summary>
        /// <remarks>
        /// sets to chr->chrnum - commonly used for respawning guards
        /// </remarks>
        public static AiCommandDescription GuardSetChrNum
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_chr_num",
                    CommandId = 142,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's total health - the higher the value, the more shots needed to kill guard.
        /// </summary>
        /// <remarks>
        /// sets to chr->maxdamage. default health is 4.0f (0x0028/40 dec for argument). argument is converted to float and divided by 10 before setting to maxdamage. if difficulty mode 007 is active, command will use 007 health modifier
        /// </remarks>
        public static AiCommandDescription GuardSetHealthTotal
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_health_total",
                    CommandId = 143,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "total_health", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's armour value - the higher the value, the higher the armour. armoured guards will not show hit reactions. they also don't instantly die from explosions, instead taking damaged based on how close they are to explosions like bond. to any setup designers reading this, please use armour sparingly!
        /// </summary>
        /// <remarks>
        /// subtracts from chr->damage - negative damage means guard has armour. instead of storing armour as a separate chr variable, we reuse the current damage and read negative damage as armour. technically this command should be titled 'guard_remove_damage' but its used mostly for adding armour to guards. argument is converted to float and divided by 10 before subtracting chr->damage. if difficulty mode 007 is active, command will use 007 health modifier. argument is unsigned - 0xFFFF will be set to 6553.5f armour, or -6553.5f damage
        /// </remarks>
        public static AiCommandDescription GuardSetArmour
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_armour",
                    CommandId = 144,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "armour_value", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's speed rating - controls how quickly the guard animates.
        /// </summary>
        /// <remarks>
        /// sets to chr->speedrating. default speed is 0 - argument is signed. negative values will make guard animate slower - this affects firing animations. command does not use 007 reaction speed modifier. do not use values above/below 100 or it may crash
        /// </remarks>
        public static AiCommandDescription GuardSetSpeedRating
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_speed_rating",
                    CommandId = 145,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "speed_rating", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's argh rating - controls how quickly the guard recovers from being shot. range is -100 to 100 (100 show almost no hit reaction)
        /// </summary>
        /// <remarks>
        /// sets to chr->arghrating. default value is 0 - argument is signed. negative values will make guard animate slower - this affects firing animations. command does not use 007 reaction speed modifier
        /// </remarks>
        public static AiCommandDescription GuardSetArghRating
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_argh_rating",
                    CommandId = 146,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "speed_rating", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard's accuracy rating - controls how accurately the guard fires their weapon
        /// </summary>
        /// <remarks>
        /// sets to chr->accuracyrating. default value is 0 and ranges from -100 to 100, argument is signed byte. command does not use 007 accuracy modifier
        /// </remarks>
        public static AiCommandDescription GuardSetAccuracyRating
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_accuracy_rating",
                    CommandId = 147,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "accuracy_rating", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->BITFIELD on
        /// </summary>
        /// <remarks>
        /// can be used to store a custom flag per chr, useful for missions. global lists use flag 01, which is defined as BITFIELD_DONT_POINT_AT_BOND. other bits are free to use for setup's ai lists. can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription GuardBitfieldSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_bitfield_set_on",
                    CommandId = 148,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->BITFIELD off
        /// </summary>
        /// <remarks>
        /// can be used to store a custom flag per chr, useful for missions. global lists use flag 01, which is defined as BITFIELD_DONT_POINT_AT_BOND. other bits are free to use for setup's ai lists. can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription GuardBitfieldSetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_bitfield_set_off",
                    CommandId = 149,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if any bits in argument are set on in chr->BITFIELD, goto label
        /// </summary>
        /// <remarks>
        /// can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription IfGuardBitfieldIsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_bitfield_is_set_on",
                    CommandId = 150,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->BITFIELD on
        /// </summary>
        /// <remarks>
        /// can be used to store a custom flag per chr, useful for missions. global lists use flag 01, which is defined as BITFIELD_DONT_POINT_AT_BOND. other bits are free to use for setup's ai lists
        /// </remarks>
        public static AiCommandDescription ChrBitfieldSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_bitfield_set_on",
                    CommandId = 151,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->BITFIELD off
        /// </summary>
        /// <remarks>
        /// can be used to store a custom flag per chr, useful for missions. global lists use flag 01, which is defined as BITFIELD_DONT_POINT_AT_BOND. other bits are free to use for setup's ai lists
        /// </remarks>
        public static AiCommandDescription ChrBitfieldSetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_bitfield_set_off",
                    CommandId = 152,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if any bits in argument are set on in chr->BITFIELD, goto label
        /// </summary>
        public static AiCommandDescription IfChrBitfieldIsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_bitfield_is_set_on",
                    CommandId = 153,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set bits in objective bitfield on
        /// </summary>
        /// <remarks>
        /// can be used to store a mission unique objective flag, which can be linked to mission objectives. it can also be used to store miscellaneous flags used by other ai lists. if a mission objective is changed while in third person, it will not be updated on the briefing page - all mission objectives status are locked while in third person
        /// </remarks>
        public static AiCommandDescription ObjectiveBitfieldSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "objective_bitfield_set_on",
                    CommandId = 154,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// set bits in objective bitfield off
        /// </summary>
        /// <remarks>
        /// can be used to store a mission unique objective flag, which can be linked to mission objectives. it can also be used to store miscellaneous flags used by other ai lists. if a mission objective is changed while in third person, it will not be updated on the briefing page - all mission objectives status are locked while in third person
        /// </remarks>
        public static AiCommandDescription ObjectiveBitfieldSetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "objective_bitfield_set_off",
                    CommandId = 155,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// if bits in objective bitfield are set on, goto label
        /// </summary>
        /// <remarks>
        /// can check multiple flags at once
        /// </remarks>
        public static AiCommandDescription IfObjectiveBitfieldIsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_objective_bitfield_is_set_on",
                    CommandId = 156,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrflags on
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines. command can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription GuardFlagsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_flags_set_on",
                    CommandId = 157,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrflags off
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines. can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription GuardFlagsSetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_flags_set_off",
                    CommandId = 158,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// if bits is set on in chr->chrflags, goto label
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines. can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiCommandDescription IfGuardFlagsIsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_flags_is_set_on",
                    CommandId = 159,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrflags on
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines
        /// </remarks>
        public static AiCommandDescription ChrFlagsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_flags_set_on",
                    CommandId = 160,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrflags off
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines
        /// </remarks>
        public static AiCommandDescription ChrFlagsSetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_flags_set_off",
                    CommandId = 161,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// if bits is set on in chr->chrflags, goto label
        /// </summary>
        /// <remarks>
        /// chr->chrflags are not ai list or setup exclusive, they are controlled by many parts of the engine. bitfield uses CHRFLAG_# defines
        /// </remarks>
        public static AiCommandDescription IfChrFlagsIsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_flags_is_set_on",
                    CommandId = 162,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set object->propflags on
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG_# defines
        /// </remarks>
        public static AiCommandDescription ObjectFlags1SetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_flags_1_set_on",
                    CommandId = 163,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// set object->propflags off
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG_# defines
        /// </remarks>
        public static AiCommandDescription ObjectFlags1SetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_flags_1_set_off",
                    CommandId = 164,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// if bits is set on in object->propflags, goto label
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG_# defines
        /// </remarks>
        public static AiCommandDescription IfObjectFlags1IsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_flags_1_is_set_on",
                    CommandId = 165,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set object->propflags2 on
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG2_# defines
        /// </remarks>
        public static AiCommandDescription ObjectFlags2SetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_flags_2_set_on",
                    CommandId = 166,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// set object->propflags2 off
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG2_# defines
        /// </remarks>
        public static AiCommandDescription ObjectFlags2SetOff
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_flags_2_set_off",
                    CommandId = 167,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                    }
                };
            }
        }

        /// <summary>
        /// if bits is set on in object->propflags2, goto label
        /// </summary>
        /// <remarks>
        /// bitfield uses PROPFLAG2_# defines
        /// </remarks>
        public static AiCommandDescription IfObjectFlags2IsSetOn
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_flags_2_is_set_on",
                    CommandId = 168,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard->chrpreset1 to chr_preset
        /// </summary>
        /// <remarks>
        /// can be used by obj ai lists
        /// </remarks>
        public static AiCommandDescription GuardSetChrPreset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_chr_preset",
                    CommandId = 169,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_preset", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrpreset1 to chr_preset
        /// </summary>
        public static AiCommandDescription ChrSetChrPreset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_set_chr_preset",
                    CommandId = 170,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_preset", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set guard->padpreset1 to pad_preset
        /// </summary>
        /// <remarks>
        /// can be used by obj ai lists
        /// </remarks>
        public static AiCommandDescription GuardSetPadPreset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_set_pad_preset",
                    CommandId = 171,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad_preset", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->padpreset1 to pad_preset
        /// </summary>
        public static AiCommandDescription ChrSetPadPreset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_set_pad_preset",
                    CommandId = 172,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad_preset", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// debug comment
        /// </summary>
        /// <remarks>
        /// may have originally printed to stderr on host sgi devkit. command is variable length must end with null terminator character '\0' (debug_log_end)
        /// </remarks>
        public static AiCommandDescription DebugLog
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "debug_log",
                    CommandId = 173,
                    CommandLengthBytes = 50,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// reset and start chr->timer60
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription LocalTimerResetStart
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_timer_reset_start",
                    CommandId = 174,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// reset chr->timer60
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription LocalTimerReset
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_timer_reset",
                    CommandId = 175,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// pauses chr->timer60 (does not reset value)
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription LocalTimerStop
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_timer_stop",
                    CommandId = 176,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// start chr->timer60 (does not reset value)
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription LocalTimerStart
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "local_timer_start",
                    CommandId = 177,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if chr->timer60 is not active (paused), goto label
        /// </summary>
        /// <remarks>
        /// by default, chr->timer60 is inactive
        /// </remarks>
        public static AiCommandDescription IfLocalTimerHasStopped
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_timer_has_stopped",
                    CommandId = 178,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->timer60 < time60, goto label
        /// </summary>
        /// <remarks>
        /// time60 argument is converted to float from unsigned int and compared. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription IfLocalTimerLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_timer_less_than",
                    CommandId = 179,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "time60", ByteLength = 3 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr->timer60 > time60, goto label
        /// </summary>
        /// <remarks>
        /// time60 argument is converted to float from unsigned int and compared. chr->timer60 only counts up
        /// </remarks>
        public static AiCommandDescription IfLocalTimerGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_local_timer_greater_than",
                    CommandId = 180,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "time60", ByteLength = 3 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// shows the hud countdown
        /// </summary>
        public static AiCommandDescription HudCountdownShow
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_countdown_show",
                    CommandId = 181,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// hides the hud countdown
        /// </summary>
        /// <remarks>
        /// can be used as a hidden global timer for objective logic
        /// </remarks>
        public static AiCommandDescription HudCountdownHide
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_countdown_hide",
                    CommandId = 182,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// set the hud countdown
        /// </summary>
        /// <remarks>
        /// to make the timer count up, set to 0 and start timer
        /// </remarks>
        public static AiCommandDescription HudCountdownSet
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_countdown_set",
                    CommandId = 183,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "seconds", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// stops the hud countdown
        /// </summary>
        public static AiCommandDescription HudCountdownStop
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_countdown_stop",
                    CommandId = 184,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// start the hud countdown
        /// </summary>
        public static AiCommandDescription HudCountdownStart
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_countdown_start",
                    CommandId = 185,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if hud countdown isn't active (paused), goto label
        /// </summary>
        /// <remarks>
        /// by default, hud countdown is inactive
        /// </remarks>
        public static AiCommandDescription IfHudCountdownHasStopped
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_hud_countdown_has_stopped",
                    CommandId = 186,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if hud countdown < seconds, goto label
        /// </summary>
        /// <remarks>
        /// if seconds argument is 0, it will only goto label if timer is less than zero (counting up). seconds value is unsigned and can't test negative values
        /// </remarks>
        public static AiCommandDescription IfHudCountdownLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_hud_countdown_less_than",
                    CommandId = 187,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if hud countdown > seconds, goto label
        /// </summary>
        /// <remarks>
        /// if seconds argument is 0, it will only goto label if timer is greater than zero (counting down). seconds value is unsigned and can't test negative values
        /// </remarks>
        public static AiCommandDescription IfHudCountdownGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_hud_countdown_greater_than",
                    CommandId = 188,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// spawn chr at pad, goto label if successful
        /// </summary>
        /// <remarks>
        /// if out of memory/can't spawn chr, do not goto label. if pad is blocked, attempt to spawn chr around pad. bitfield uses SPAWN_# defines
        /// </remarks>
        public static AiCommandDescription ChrTrySpawningAtPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_try_spawning_at_pad",
                    CommandId = 189,
                    CommandLengthBytes = 12,
                    NumberParameters = 6,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "body_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "head_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// spawn a chr next to another chr, goto label if successful
        /// </summary>
        /// <remarks>
        /// if out of memory/can't spawn chr, do not goto label. bitfield uses SPAWN_# defines. target chr must still exist in level or else command will crash. command will not spawn chr if target chr has been seen before (CHRFLAG_HAS_BEEN_ON_SCREEN)
        /// </remarks>
        public static AiCommandDescription ChrTrySpawningNextToUnseenChr
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_try_spawning_next_to_unseen_chr",
                    CommandId = 190,
                    CommandLengthBytes = 11,
                    NumberParameters = 6,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "body_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "head_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "chr_num_target", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// spawn weapon for guard, goto label if successful
        /// </summary>
        /// <remarks>
        /// if out of memory/can't spawn item/hands occupied, do not goto label. spawned prop must have a holding position command within the model file, else use conceal flag so guard does not attempt to hold prop
        /// </remarks>
        public static AiCommandDescription GuardTrySpawningItem
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_spawning_item",
                    CommandId = 191,
                    CommandLengthBytes = 9,
                    NumberParameters = 4,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "prop_num", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "prop_bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// spawn hat for guard, goto label if successful
        /// </summary>
        /// <remarks>
        /// if out of memory/can't spawn item/already have hat, do not goto label. spawned hat must have a holding position command within the model file
        /// </remarks>
        public static AiCommandDescription GuardTrySpawningHat
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_try_spawning_hat",
                    CommandId = 192,
                    CommandLengthBytes = 8,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "prop_num", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "prop_bitfield", ByteLength = 4 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard has clone flag on, spawn a new guard - goto label if successful
        /// </summary>
        /// <remarks>
        /// clone flag is stored in chr->chrflags which is assigned at setup init. newly spawned guard is placed in front of original guard
        /// </remarks>
        public static AiCommandDescription ChrTrySpawningClone
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_try_spawning_clone",
                    CommandId = 193,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// print text slot to bottom left part of screen (where pickup text is located)
        /// </summary>
        /// <remarks>
        /// if text slot is not currently allocated in memory, game will softlock. expects string to end with \n character
        /// </remarks>
        public static AiCommandDescription TextPrintBottom
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "text_print_bottom",
                    CommandId = 194,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "text_slot", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// print text slot to top part of screen
        /// </summary>
        /// <remarks>
        /// if text slot is not currently allocated in memory, game will softlock. ensure that end of text has a \n character or text background will be misaligned
        /// </remarks>
        public static AiCommandDescription TextPrintTop
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "text_print_top",
                    CommandId = 195,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "text_slot", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// play a sound effect
        /// </summary>
        /// <remarks>
        /// channel argument range is 0-7. use a channel if you plan on modifying sfx volume with commands C5-CA. if you don't plan on doing this, use a invalid channel such as -1. this will play the sfx but not bother initializing channel data for commands C5-CA. if a sfx is already occupying channel, retriggering sfx will overwrite old sfx slot data and no longer can be used by commands C5-CA
        /// </remarks>
        public static AiCommandDescription SfxPlay
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_play",
                    CommandId = 196,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "sound_num", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set a occupied sfx channel to emit from a tagged object
        /// </summary>
        /// <remarks>
        /// panning is not calculated (mono), only affects volume. decay argument is number of ticks to fully transition from max volume to target volume
        /// </remarks>
        public static AiCommandDescription SfxEmitFromObject
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_emit_from_object",
                    CommandId = 197,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "vol_decay_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set a occupied sfx channel to emit from a pad
        /// </summary>
        /// <remarks>
        /// panning is not calculated (mono), only affects volume. decay argument is number of ticks to fully transition from max volume to target volume
        /// </remarks>
        public static AiCommandDescription SfxEmitFromPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_emit_from_pad",
                    CommandId = 198,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "vol_decay_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set occupied sfx channel's volume
        /// </summary>
        /// <remarks>
        /// time argument is number of ticks to fade between current volume to target volume. volume argument is signed. range is 0x0000-0x7FFF
        /// </remarks>
        public static AiCommandDescription SfxSetChannelVolume
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_set_channel_volume",
                    CommandId = 199,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "target_volume", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "transition_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// fade out occupied sfx channel's volume by volume percent
        /// </summary>
        /// <remarks>
        /// time argument is number of ticks to fade between current volume to target volume. volume argument is signed. range is 0x0000-0x7FFF (0-100%)
        /// </remarks>
        public static AiCommandDescription SfxFadeChannelVolume
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_fade_channel_volume",
                    CommandId = 200,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "fade_volume_percent", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "fade_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// stop playing sfx in occupied sfx channel
        /// </summary>
        public static AiCommandDescription SfxStopChannel
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "sfx_stop_channel",
                    CommandId = 201,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if sfx channel's volume is < volume argument, goto label
        /// </summary>
        /// <remarks>
        /// if sfx channel is free (no audio playing), goto label. volume argument is signed. range is 0x0000-0x7FFF
        /// </remarks>
        public static AiCommandDescription IfSfxChannelVolumeLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_sfx_channel_volume_less_than",
                    CommandId = 202,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "volume", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// makes vehicle follow a predefined path within setup
        /// </summary>
        public static AiCommandDescription VehicleStartPath
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "vehicle_start_path",
                    CommandId = 203,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "path_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// sets vehicle speed, usually paired with command CB
        /// </summary>
        /// <remarks>
        /// arguments are unsigned. 1000 units = 1 meter per second travel speed. acceleration_time60 is number of game ticks to reach top speed (lower = faster)
        /// </remarks>
        public static AiCommandDescription VehicleSpeed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "vehicle_speed",
                    CommandId = 204,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "top_speed", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "acceleration_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// sets aircraft's rotor speed
        /// </summary>
        /// <remarks>
        /// arguments are unsigned. argument scale is 10 units per degree, per tick. acceleration_time60 is number of game ticks to reach top speed (lower = faster)
        /// </remarks>
        public static AiCommandDescription AircraftRotorSpeed
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "aircraft_rotor_speed",
                    CommandId = 205,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "rotor_speed", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "acceleration_time60", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// if camera mode equal to INTRO_CAM/FADESWIRL_CAM (viewing mission intro), goto label
        /// </summary>
        /// <remarks>
        /// if setup lacks intro camera structs, intro will be skipped
        /// </remarks>
        public static AiCommandDescription IfCameraIsInIntro
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_camera_is_in_intro",
                    CommandId = 206,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if camera mode equal to SWIRL_CAM (moving to back of bond's head), goto label
        /// </summary>
        /// <remarks>
        /// if setup lacks swirl points, intro swirl will be skipped
        /// </remarks>
        public static AiCommandDescription IfCameraIsInBondSwirl
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_camera_is_in_bond_swirl",
                    CommandId = 207,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// change the screen bank of a tagged tv monitor
        /// </summary>
        /// <remarks>
        /// if tagged object has multiple screens, use screen index argument to set. if tagged object has one screen, screen index is ignored
        /// </remarks>
        public static AiCommandDescription TvChangeScreenBank
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "tv_change_screen_bank",
                    CommandId = 208,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "screen_index", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "screen_bank", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond is controlling tank, goto label
        /// </summary>
        public static AiCommandDescription IfBondInTank
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_in_tank",
                    CommandId = 209,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// exits the level
        /// </summary>
        /// <remarks>
        /// recommend not to use this command, instead goto GLIST_EXIT_LEVEL for exit cutscene list. retail game has a glitch with hires mode that needs to execute this command in a loop, check cuba's 1000 list
        /// </remarks>
        public static AiCommandDescription ExitLevel
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "exit_level",
                    CommandId = 210,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// switch back to first person view
        /// </summary>
        /// <remarks>
        /// unused command, never used in retail game. tagged items within inventory will become invalid after command - only weapons are safe. command must have 3 ai_sleep commands before executing this command or else engine will crash on console (use camera_transition_to_bond). mission time is resumed on return to first person view
        /// </remarks>
        public static AiCommandDescription CameraReturnToBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "camera_return_to_bond",
                    CommandId = 211,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// change view to pad and look at bond
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. mission time is paused while in third person
        /// </remarks>
        public static AiCommandDescription CameraLookAtBondFromPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "camera_look_at_bond_from_pad",
                    CommandId = 212,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// change view to tagged camera's position and rotation
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. only look at bond if flag is set. unused flag may have separated look at bond as x/y flags instead of a single flag - for retail unused flag does nothing. mission time is paused while in third person
        /// </remarks>
        public static AiCommandDescription CameraSwitch
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "camera_switch",
                    CommandId = 213,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "look_at_bond_flag", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "unused_flag", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's y axis position < position argument, goto label
        /// </summary>
        /// <remarks>
        /// checks if bond's y axis is below the provided argument. command uses world units. argument is signed and scale is 1:1 to in-game position. bond's point of view is accounted for by command (like debug manpos)
        /// </remarks>
        public static AiCommandDescription IfBondYPosLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_y_pos_less_than",
                    CommandId = 214,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "y_pos", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// hide hud elements, lock player control and stop mission time. command is commonly used for exit mission lists
        /// </summary>
        /// <remarks>
        /// argument flag will not hide element on command execution. this is needed for dialog/hud countdown while in cinema mode. flags can be combined together to show multiple elements. sequential executions of D7 can be used to hide more elements, but once an element has been hidden it cannot be shown again until command D8 is executed. bond can take damage while in locked state. use HUD_# flags for bitfield argument
        /// </remarks>
        public static AiCommandDescription HudHideAndLockControlsAndPauseMissionTime
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_hide_and_lock_controls_and_pause_mission_time",
                    CommandId = 215,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// show all hud elements, unlock player control and resume mission time
        /// </summary>
        /// <remarks>
        /// should only be executed after D7 command
        /// </remarks>
        public static AiCommandDescription HudShowAllAndUnlockControlsAndResumeMissionTime
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "hud_show_all_and_unlock_controls_and_resume_mission_time",
                    CommandId = 216,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// teleport chr to pad, goto label if successful
        /// </summary>
        public static AiCommandDescription ChrTryTeleportingToPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_try_teleporting_to_pad",
                    CommandId = 217,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// fades the screen out to black
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiCommandDescription ScreenFadeToBlack
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "screen_fade_to_black",
                    CommandId = 218,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// fades the screen from black
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiCommandDescription ScreenFadeFromBlack
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "screen_fade_from_black",
                    CommandId = 219,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// when screen fade has completed (from/to black), goto label
        /// </summary>
        /// <remarks>
        /// fade duration is 1 second
        /// </remarks>
        public static AiCommandDescription IfScreenFadeCompleted
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_screen_fade_completed",
                    CommandId = 220,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// hide all characters in level - including bond's third person model. execute this before switching to exit camera or bond will disappear
        /// </summary>
        /// <remarks>
        /// hidden characters will halt their ai list execution until unhidden
        /// </remarks>
        public static AiCommandDescription ChrHideAll
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_hide_all",
                    CommandId = 221,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// show all characters previously hidden by command DD
        /// </summary>
        public static AiCommandDescription ChrShowAll
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_show_all",
                    CommandId = 222,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// instantly open tagged door
        /// </summary>
        /// <remarks>
        /// mostly used for cutscenes, doesn't trigger door opening sfx. open tagged door even if locked
        /// </remarks>
        public static AiCommandDescription DoorOpenInstant
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "door_open_instant",
                    CommandId = 223,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// remove the item held by hand index
        /// </summary>
        /// <remarks>
        /// does not drop item, instead clears holding item flag for hand index
        /// </remarks>
        public static AiCommandDescription ChrRemoveItemInHand
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "chr_remove_item_in_hand",
                    CommandId = 224,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "hand_index", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if the number of active players < argument, goto label
        /// </summary>
        /// <remarks>
        /// single player always has a total of active players set to 1
        /// </remarks>
        public static AiCommandDescription IfNumberOfActivePlayersLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_number_of_active_players_less_than",
                    CommandId = 225,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "number", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond's total ammo for item < ammo_total argument, goto label
        /// </summary>
        /// <remarks>
        /// ammo_total argument is signed. total ammo also accounts for loaded gun
        /// </remarks>
        public static AiCommandDescription IfBondItemTotalAmmoLessThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_item_total_ammo_less_than",
                    CommandId = 226,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "ammo_total", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// forces bond to equip an item - only works in first person
        /// </summary>
        /// <remarks>
        /// can be used for any item, even if bond doesn't have it in inventory
        /// </remarks>
        public static AiCommandDescription BondEquipItem
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_equip_item",
                    CommandId = 227,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// forces bond to equip an item - only works in third person (cinema)
        /// </summary>
        /// <remarks>
        /// can be used for any item, even if bond doesn't have it in inventory
        /// </remarks>
        public static AiCommandDescription BondEquipItemCinema
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_equip_item_cinema",
                    CommandId = 228,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "item_num", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// forces bond to move in X/Z direction
        /// </summary>
        /// <remarks>
        /// only works when bond has been locked by command D7. used for dam jump. argument is signed and scale is 1:1 to in-game position. speed is number of world units per tick
        /// </remarks>
        public static AiCommandDescription BondSetLockedVelocity
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_set_locked_velocity",
                    CommandId = 229,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "x_speed60", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "z_speed60", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if tagged object in the same room with pad, goto label
        /// </summary>
        public static AiCommandDescription IfObjectInRoomWithPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_object_in_room_with_pad",
                    CommandId = 230,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is in firing state (ACT_ATTACK) and TARGET_180_RANGE is set, goto label
        /// </summary>
        public static AiCommandDescription IfGuardIsFiringAndUsing180RangeFlag
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_is_firing_and_using_180_range_flag",
                    CommandId = 231,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if guard is in firing state (ACT_ATTACK), goto label
        /// </summary>
        public static AiCommandDescription IfGuardIsFiring
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_guard_is_firing",
                    CommandId = 232,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// instantly switch fog to the next fog's slot
        /// </summary>
        /// <remarks>
        /// this command can't be stopped after executing. level must have a fog assigned or will crash!
        /// </remarks>
        public static AiCommandDescription SwitchFogInstantly
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "switch_fog_instantly",
                    CommandId = 233,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if player pressed any button, fade to black and exit level
        /// </summary>
        /// <remarks>
        /// this command activates a state where game will fade to black when button input is detected from controller 1. command does not pause mission time
        /// </remarks>
        public static AiCommandDescription TriggerFadeAndExitLevelOnButtonPress
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "trigger_fade_and_exit_level_on_button_press",
                    CommandId = 234,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if bond has died/been killed, goto label
        /// </summary>
        public static AiCommandDescription IfBondIsDead
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_is_dead",
                    CommandId = 235,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// disables bond damage and ability to pick up items
        /// </summary>
        /// <remarks>
        /// commonly used for level exit ai lists - prevents bond dying after triggering exit cutscene. use command F3 to check if flag is set on
        /// </remarks>
        public static AiCommandDescription BondDisableDamageAndPickups
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_disable_damage_and_pickups",
                    CommandId = 236,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// set bond's left/right weapons to be invisible
        /// </summary>
        public static AiCommandDescription BondHideWeapons
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_hide_weapons",
                    CommandId = 237,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// change view to orbit a pad with set speed
        /// </summary>
        /// <remarks>
        /// command must have a bond_hide_weapons command and 3 ai_sleep commands before executing this command or else engine will crash (use camera_transition_from_bond). if camera mode is already in third person then you don't need to do the above. arguments: lat_distance: camera distance from pad, 100 units per meter. argument is unsigned vert_distance: camera distance from pad, 100 units per meter. argument is signed orbit_speed: speed to orbit around pad, argument is signed. unit format uses compass direction like target commands (14-17). generally stick to a low range as it is used for delta timing (0100-FF00) pad: pad for camera to target and orbit around y_pos_offset: offset the relative y position for pad (boom/jib), argument is signed initial_rotation: uses compass direction like target commands (14-17) but inverted - hex N: 0000 E: C000 S: 8000: W: 4000 mission time is paused while in third person
        /// </remarks>
        public static AiCommandDescription CameraOrbitPad
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "camera_orbit_pad",
                    CommandId = 238,
                    CommandLengthBytes = 13,
                    NumberParameters = 6,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "lat_distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "vert_distance", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "orbit_speed60", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "pad", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "y_pos_offset", ByteLength = 2 },
                        new AiCommandParameterDescription(){ ParameterName = "initial_rotation", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// trigger credits crawl
        /// </summary>
        /// <remarks>
        /// credits text and positions are stored in setup intro struct
        /// </remarks>
        public static AiCommandDescription CreditsRoll
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "credits_roll",
                    CommandId = 239,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// credits crawl has finished, goto label
        /// </summary>
        public static AiCommandDescription IfCreditsHasCompleted
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_credits_has_completed",
                    CommandId = 240,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if all objectives for current difficulty has been completed, goto label
        /// </summary>
        /// <remarks>
        /// uses objective difficulty settings within setup, briefing file settings are not referenced. ensure both setup and briefing files are consistent
        /// </remarks>
        public static AiCommandDescription IfObjectiveAllCompleted
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_objective_all_completed",
                    CommandId = 241,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if current bond equal to folder actor index, goto label
        /// </summary>
        /// <remarks>
        /// in retail release only index 0 works. originally this would have checked which bond (brosnan/connery/moore/dalton) is currently used, with each briefing folder using a different bond likeness in-game. however rare didn't have the license to use the other actor's faces so this feature was removed. command is only used for cuba (credits)
        /// </remarks>
        public static AiCommandDescription IfFolderActorIsEqual
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_folder_actor_is_equal",
                    CommandId = 242,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "bond_actor_index", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if bond damage and ability to pick up items disabled, goto label
        /// </summary>
        /// <remarks>
        /// used to check when bond has exited level, usually to stop guards from spawning during mission cinema. use command EC to set state on
        /// </remarks>
        public static AiCommandDescription IfBondDamageAndPickupsDisabled
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_bond_damage_and_pickups_disabled",
                    CommandId = 243,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// play level's x track for duration
        /// </summary>
        /// <remarks>
        /// seconds arguments are unsigned, available music slots range is 0-3. stopped duration argument is used by command F5. when using F5 to stop a music slot, the xtrack will continue to play until this or total time reaches 0. if you don't want this to happen, set the seconds stopped duration argument to 0
        /// </remarks>
        public static AiCommandDescription MusicXtrackPlay
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "music_xtrack_play",
                    CommandId = 244,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "music_slot", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "seconds_stopped_duration", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "seconds_total_duration", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// stop currently playing x track in slot
        /// </summary>
        /// <remarks>
        /// music slots range is 0-3. use slot -1 to stop all xtrack slots instantly. when stopping a music slot, it will let the track continue to play until the seconds stopped duration time or total time (set by command F4) reaches zero. this is ignored when using music slot -1
        /// </remarks>
        public static AiCommandDescription MusicXtrackStop
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "music_xtrack_stop",
                    CommandId = 245,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "music_slot", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// triggers explosions around the player, will continue forever
        /// </summary>
        /// <remarks>
        /// does not trigger level exit or killed in action state
        /// </remarks>
        public static AiCommandDescription TriggerExplosionsAroundBond
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "trigger_explosions_around_bond",
                    CommandId = 246,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// if total civilians killed > argument, goto label
        /// </summary>
        /// <remarks>
        /// guards flagged with CHRFLAG_COUNT_DEATH_AS_CIVILIAN will count towards total when killed. usually set for scientists/civilians/innocent NPCs
        /// </remarks>
        public static AiCommandDescription IfKilledCiviliansGreaterThan
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_killed_civilians_greater_than",
                    CommandId = 247,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "civilians_killed", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if chr was shot since last check, goto label
        /// </summary>
        /// <remarks>
        /// checks chr->chrflags if CHRFLAG_WAS_HIT is set. if true, unset flag and goto label. CHRFLAG_WAS_HIT is set even if guard is invincible
        /// </remarks>
        public static AiCommandDescription IfChrWasShotSinceLastCheck
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "if_chr_was_shot_since_last_check",
                    CommandId = 248,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiCommandParameterDescription(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// sets briefing status to killed in action, automatic mission failure
        /// </summary>
        /// <remarks>
        /// does not kill the player, only changes the mission status
        /// </remarks>
        public static AiCommandDescription BondKilledInAction
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "bond_killed_in_action",
                    CommandId = 249,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// makes guard raise their arms for half a second
        /// </summary>
        public static AiCommandDescription GuardRaisesArms
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "guard_raises_arms",
                    CommandId = 250,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// trigger gas leak event and slowly transition fog to the next fog's slot
        /// </summary>
        /// <remarks>
        /// this command triggers a gas leak. for the level egypt, this command will not trigger a gas leak, but instead will only transition the fog. this command can't be stopped after executing. level must have a fog assigned or will crash!
        /// </remarks>
        public static AiCommandDescription GasLeakAndFadeFog
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "gas_leak_and_fade_fog",
                    CommandId = 251,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<AiCommandParameterDescription>()
                };
            }
        }

        /// <summary>
        /// launch a tagged object like a rocket
        /// </summary>
        /// <remarks>
        /// if tagged object can't be turned upright, object will be destroyed instead. can be used to drop attached props
        /// </remarks>
        public static AiCommandDescription ObjectRocketLaunch
        {
            get
            {
                return new AiCommandDescription()
                {
                    DecompName = "object_rocket_launch",
                    CommandId = 252,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<AiCommandParameterDescription>()
                    {
                        new AiCommandParameterDescription(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        public static AiCommandBlock ParseBytes(byte[] bytes)
        {
            int position = 0;
            byte b;
            var results = new AiCommandBlock();

            while (true)
            {
                b = bytes[position++];
                var commandDescription = AiCommandById[b];
                var commandParameters = new List<AiCommandParameter>();
                for (int i = 0; i < commandDescription.NumberParameters; i++)
                {
                    var len = commandDescription.CommandParameters[i].ByteLength;
                    int val = 0;
                    for (var j = 0; j < len; j++)
                    {
                        val |= bytes[position + j] << (8 * j);
                    }
                    commandParameters.Add(new AiCommandParameter(commandDescription.CommandParameters[i].ParameterName, len, val));
                    position += len;
                }

                var aic = new AiCommand(commandDescription)
                {
                    CommandParameters = commandParameters,
                };

                results.Commands.Add(aic);

                if (commandDescription.CommandId == AiListEnd.CommandId
                    || position >= bytes.Length)
                {
                    break;
                }
            }

            return results;
        }

        public static Dictionary<int, AiCommandDescription> AiCommandById = new Dictionary<int, AiCommandDescription>()
        {
            { GotoNext.CommandId, GotoNext },
            { GotoFirst.CommandId, GotoFirst },
            { Label.CommandId, Label },
            { AiSleep.CommandId, AiSleep },
            { AiListEnd.CommandId, AiListEnd },
            { JumpToAiList.CommandId, JumpToAiList },
            { SetReturnAiList.CommandId, SetReturnAiList },
            { JumpToReturnAiList.CommandId, JumpToReturnAiList },
            { GuardAnimationStop.CommandId, GuardAnimationStop },
            { GuardKneel.CommandId, GuardKneel },
            { GuardPlayAnimation.CommandId, GuardPlayAnimation },
            { IfGuardPlayingAnimation.CommandId, IfGuardPlayingAnimation },
            { GuardPointsAtBond.CommandId, GuardPointsAtBond },
            { GuardLooksAroundSelf.CommandId, GuardLooksAroundSelf },
            { GuardTrySidestepping.CommandId, GuardTrySidestepping },
            { GuardTryHoppingSideways.CommandId, GuardTryHoppingSideways },
            { GuardTryRunningToSide.CommandId, GuardTryRunningToSide },
            { GuardTryFiringWalk.CommandId, GuardTryFiringWalk },
            { GuardTryFiringRun.CommandId, GuardTryFiringRun },
            { GuardTryFiringRoll.CommandId, GuardTryFiringRoll },
            { GuardTryFireOrAimAtTarget.CommandId, GuardTryFireOrAimAtTarget },
            { GuardTryFireOrAimAtTargetKneel.CommandId, GuardTryFireOrAimAtTargetKneel },
            { GuardTryFireOrAimAtTargetUpdate.CommandId, GuardTryFireOrAimAtTargetUpdate },
            { GuardTryFacingTarget.CommandId, GuardTryFacingTarget },
            { ChrHitBodyPartWithItemDamage.CommandId, ChrHitBodyPartWithItemDamage },
            { ChrHitChrBodyPartWithHeldItem.CommandId, ChrHitChrBodyPartWithHeldItem },
            { GuardTryThrowingGrenade.CommandId, GuardTryThrowingGrenade },
            { GuardTryDroppingItem.CommandId, GuardTryDroppingItem },
            { GuardRunsToPad.CommandId, GuardRunsToPad },
            { GuardRunsToPadPreset.CommandId, GuardRunsToPadPreset },
            { GuardWalksToPad.CommandId, GuardWalksToPad },
            { GuardSprintsToPad.CommandId, GuardSprintsToPad },
            { GuardStartPatrol.CommandId, GuardStartPatrol },
            { GuardSurrenders.CommandId, GuardSurrenders },
            { GuardRemoveFade.CommandId, GuardRemoveFade },
            { ChrRemoveInstant.CommandId, ChrRemoveInstant },
            { GuardTryTriggeringAlarmAtPad.CommandId, GuardTryTriggeringAlarmAtPad },
            { AlarmOn.CommandId, AlarmOn },
            { AlarmOff.CommandId, AlarmOff },
            { RemovedCommand27.CommandId, RemovedCommand27 },
            { GuardTryRunningToBondPosition.CommandId, GuardTryRunningToBondPosition },
            { GuardTryWalkingToBondPosition.CommandId, GuardTryWalkingToBondPosition },
            { GuardTrySprintingToBondPosition.CommandId, GuardTrySprintingToBondPosition },
            { RemovedCommand2B.CommandId, RemovedCommand2B },
            { GuardTryRunningToChrPosition.CommandId, GuardTryRunningToChrPosition },
            { GuardTryWalkingToChrPosition.CommandId, GuardTryWalkingToChrPosition },
            { GuardTrySprintingToChrPosition.CommandId, GuardTrySprintingToChrPosition },
            { IfGuardHasStoppedMoving.CommandId, IfGuardHasStoppedMoving },
            { IfChrDyingOrDead.CommandId, IfChrDyingOrDead },
            { IfChrDoesNotExist.CommandId, IfChrDoesNotExist },
            { IfGuardSeesBond.CommandId, IfGuardSeesBond },
            { RandomGenerateSeed.CommandId, RandomGenerateSeed },
            { IfRandomSeedLessThan.CommandId, IfRandomSeedLessThan },
            { IfRandomSeedGreaterThan.CommandId, IfRandomSeedGreaterThan },
            { IfAlarmIsOnUnused.CommandId, IfAlarmIsOnUnused },
            { IfAlarmIsOn.CommandId, IfAlarmIsOn },
            { IfGasIsLeaking.CommandId, IfGasIsLeaking },
            { IfGuardHeardBond.CommandId, IfGuardHeardBond },
            { IfGuardSeeAnotherGuardShot.CommandId, IfGuardSeeAnotherGuardShot },
            { IfGuardSeeAnotherGuardDie.CommandId, IfGuardSeeAnotherGuardDie },
            { IfGuardAndBondWithinLineOfSight.CommandId, IfGuardAndBondWithinLineOfSight },
            { IfGuardAndBondWithinPartialLineOfSight.CommandId, IfGuardAndBondWithinPartialLineOfSight },
            { IfGuardWasShotOrSeenBondWithinLast10Secs.CommandId, IfGuardWasShotOrSeenBondWithinLast10Secs },
            { IfGuardHeardBondWithinLast10Secs.CommandId, IfGuardHeardBondWithinLast10Secs },
            { IfGuardInRoomWithChr.CommandId, IfGuardInRoomWithChr },
            { IfGuardHasNotBeenSeen.CommandId, IfGuardHasNotBeenSeen },
            { IfGuardIsOnScreen.CommandId, IfGuardIsOnScreen },
            { IfGuardRoomContainingSelfIsOnScreen.CommandId, IfGuardRoomContainingSelfIsOnScreen },
            { IfRoomContainingPadIsOnScreen.CommandId, IfRoomContainingPadIsOnScreen },
            { IfGuardIsTargetedByBond.CommandId, IfGuardIsTargetedByBond },
            { IfGuardShotFromBondMissed.CommandId, IfGuardShotFromBondMissed },
            { IfGuardCounterClockwiseDirectionToBondLessThan.CommandId, IfGuardCounterClockwiseDirectionToBondLessThan },
            { IfGuardCounterClockwiseDirectionToBondGreaterThan.CommandId, IfGuardCounterClockwiseDirectionToBondGreaterThan },
            { IfGuardCounterClockwiseDirectionFromBondLessThan.CommandId, IfGuardCounterClockwiseDirectionFromBondLessThan },
            { IfGuardCounterClockwiseDirectionFromBondGreaterThan.CommandId, IfGuardCounterClockwiseDirectionFromBondGreaterThan },
            { IfGuardDistanceToBondLessThan.CommandId, IfGuardDistanceToBondLessThan },
            { IfGuardDistanceToBondGreaterThan.CommandId, IfGuardDistanceToBondGreaterThan },
            { IfChrDistanceToPadLessThan.CommandId, IfChrDistanceToPadLessThan },
            { IfChrDistanceToPadGreaterThan.CommandId, IfChrDistanceToPadGreaterThan },
            { IfGuardDistanceToChrLessThan.CommandId, IfGuardDistanceToChrLessThan },
            { IfGuardDistanceToChrGreaterThan.CommandId, IfGuardDistanceToChrGreaterThan },
            { GuardTrySettingChrPresetToGuardWithinDistance.CommandId, GuardTrySettingChrPresetToGuardWithinDistance },
            { IfBondDistanceToPadLessThan.CommandId, IfBondDistanceToPadLessThan },
            { IfBondDistanceToPadGreaterThan.CommandId, IfBondDistanceToPadGreaterThan },
            { IfChrInRoomWithPad.CommandId, IfChrInRoomWithPad },
            { IfBondInRoomWithPad.CommandId, IfBondInRoomWithPad },
            { IfBondCollectedObject.CommandId, IfBondCollectedObject },
            { IfItemIsStationaryWithinLevel.CommandId, IfItemIsStationaryWithinLevel },
            { IfItemIsAttachedToObject.CommandId, IfItemIsAttachedToObject },
            { IfBondHasItemEquipped.CommandId, IfBondHasItemEquipped },
            { IfObjectExists.CommandId, IfObjectExists },
            { IfObjectNotDestroyed.CommandId, IfObjectNotDestroyed },
            { IfObjectWasActivated.CommandId, IfObjectWasActivated },
            { IfBondUsedGadgetOnObject.CommandId, IfBondUsedGadgetOnObject },
            { ObjectActivate.CommandId, ObjectActivate },
            { ObjectDestroy.CommandId, ObjectDestroy },
            { ObjectDropFromChr.CommandId, ObjectDropFromChr },
            { ChrDropAllConcealedItems.CommandId, ChrDropAllConcealedItems },
            { ChrDropAllHeldItems.CommandId, ChrDropAllHeldItems },
            { BondCollectObject.CommandId, BondCollectObject },
            { ChrEquipObject.CommandId, ChrEquipObject },
            { ObjectMoveToPad.CommandId, ObjectMoveToPad },
            { DoorOpen.CommandId, DoorOpen },
            { DoorClose.CommandId, DoorClose },
            { IfDoorStateEqual.CommandId, IfDoorStateEqual },
            { IfDoorHasBeenOpenedBefore.CommandId, IfDoorHasBeenOpenedBefore },
            { DoorSetLock.CommandId, DoorSetLock },
            { DoorUnsetLock.CommandId, DoorUnsetLock },
            { IfDoorLockEqual.CommandId, IfDoorLockEqual },
            { IfObjectiveNumComplete.CommandId, IfObjectiveNumComplete },
            { GuardTryUnknown6E.CommandId, GuardTryUnknown6E },
            { GuardTryUnknown6F.CommandId, GuardTryUnknown6F },
            { IfGameDifficultyLessThan.CommandId, IfGameDifficultyLessThan },
            { IfGameDifficultyGreaterThan.CommandId, IfGameDifficultyGreaterThan },
            { IfMissionTimeLessThan.CommandId, IfMissionTimeLessThan },
            { IfMissionTimeGreaterThan.CommandId, IfMissionTimeGreaterThan },
            { IfSystemPowerTimeLessThan.CommandId, IfSystemPowerTimeLessThan },
            { IfSystemPowerTimeGreaterThan.CommandId, IfSystemPowerTimeGreaterThan },
            { IfLevelIdLessThan.CommandId, IfLevelIdLessThan },
            { IfLevelIdGreaterThan.CommandId, IfLevelIdGreaterThan },
            { IfGuardHitsLessThan.CommandId, IfGuardHitsLessThan },
            { IfGuardHitsGreaterThan.CommandId, IfGuardHitsGreaterThan },
            { IfGuardHitsMissedLessThan.CommandId, IfGuardHitsMissedLessThan },
            { IfGuardHitsMissedGreaterThan.CommandId, IfGuardHitsMissedGreaterThan },
            { IfChrHealthLessThan.CommandId, IfChrHealthLessThan },
            { IfChrHealthGreaterThan.CommandId, IfChrHealthGreaterThan },
            { IfChrWasDamagedSinceLastCheck.CommandId, IfChrWasDamagedSinceLastCheck },
            { IfBondHealthLessThan.CommandId, IfBondHealthLessThan },
            { IfBondHealthGreaterThan.CommandId, IfBondHealthGreaterThan },
            { LocalByte1Set.CommandId, LocalByte1Set },
            { LocalByte1Add.CommandId, LocalByte1Add },
            { LocalByte1Subtract.CommandId, LocalByte1Subtract },
            { IfLocalByte1LessThan.CommandId, IfLocalByte1LessThan },
            { IfLocalByte1LessThanRandomSeed.CommandId, IfLocalByte1LessThanRandomSeed },
            { LocalByte2Set.CommandId, LocalByte2Set },
            { LocalByte2Add.CommandId, LocalByte2Add },
            { LocalByte2Subtract.CommandId, LocalByte2Subtract },
            { IfLocalByte2LessThan.CommandId, IfLocalByte2LessThan },
            { IfLocalByte2LessThanRandomSeed.CommandId, IfLocalByte2LessThanRandomSeed },
            { GuardSetHearingScale.CommandId, GuardSetHearingScale },
            { GuardSetVisionRange.CommandId, GuardSetVisionRange },
            { GuardSetGrenadeProbability.CommandId, GuardSetGrenadeProbability },
            { GuardSetChrNum.CommandId, GuardSetChrNum },
            { GuardSetHealthTotal.CommandId, GuardSetHealthTotal },
            { GuardSetArmour.CommandId, GuardSetArmour },
            { GuardSetSpeedRating.CommandId, GuardSetSpeedRating },
            { GuardSetArghRating.CommandId, GuardSetArghRating },
            { GuardSetAccuracyRating.CommandId, GuardSetAccuracyRating },
            { GuardBitfieldSetOn.CommandId, GuardBitfieldSetOn },
            { GuardBitfieldSetOff.CommandId, GuardBitfieldSetOff },
            { IfGuardBitfieldIsSetOn.CommandId, IfGuardBitfieldIsSetOn },
            { ChrBitfieldSetOn.CommandId, ChrBitfieldSetOn },
            { ChrBitfieldSetOff.CommandId, ChrBitfieldSetOff },
            { IfChrBitfieldIsSetOn.CommandId, IfChrBitfieldIsSetOn },
            { ObjectiveBitfieldSetOn.CommandId, ObjectiveBitfieldSetOn },
            { ObjectiveBitfieldSetOff.CommandId, ObjectiveBitfieldSetOff },
            { IfObjectiveBitfieldIsSetOn.CommandId, IfObjectiveBitfieldIsSetOn },
            { GuardFlagsSetOn.CommandId, GuardFlagsSetOn },
            { GuardFlagsSetOff.CommandId, GuardFlagsSetOff },
            { IfGuardFlagsIsSetOn.CommandId, IfGuardFlagsIsSetOn },
            { ChrFlagsSetOn.CommandId, ChrFlagsSetOn },
            { ChrFlagsSetOff.CommandId, ChrFlagsSetOff },
            { IfChrFlagsIsSetOn.CommandId, IfChrFlagsIsSetOn },
            { ObjectFlags1SetOn.CommandId, ObjectFlags1SetOn },
            { ObjectFlags1SetOff.CommandId, ObjectFlags1SetOff },
            { IfObjectFlags1IsSetOn.CommandId, IfObjectFlags1IsSetOn },
            { ObjectFlags2SetOn.CommandId, ObjectFlags2SetOn },
            { ObjectFlags2SetOff.CommandId, ObjectFlags2SetOff },
            { IfObjectFlags2IsSetOn.CommandId, IfObjectFlags2IsSetOn },
            { GuardSetChrPreset.CommandId, GuardSetChrPreset },
            { ChrSetChrPreset.CommandId, ChrSetChrPreset },
            { GuardSetPadPreset.CommandId, GuardSetPadPreset },
            { ChrSetPadPreset.CommandId, ChrSetPadPreset },
            { DebugLog.CommandId, DebugLog },
            { LocalTimerResetStart.CommandId, LocalTimerResetStart },
            { LocalTimerReset.CommandId, LocalTimerReset },
            { LocalTimerStop.CommandId, LocalTimerStop },
            { LocalTimerStart.CommandId, LocalTimerStart },
            { IfLocalTimerHasStopped.CommandId, IfLocalTimerHasStopped },
            { IfLocalTimerLessThan.CommandId, IfLocalTimerLessThan },
            { IfLocalTimerGreaterThan.CommandId, IfLocalTimerGreaterThan },
            { HudCountdownShow.CommandId, HudCountdownShow },
            { HudCountdownHide.CommandId, HudCountdownHide },
            { HudCountdownSet.CommandId, HudCountdownSet },
            { HudCountdownStop.CommandId, HudCountdownStop },
            { HudCountdownStart.CommandId, HudCountdownStart },
            { IfHudCountdownHasStopped.CommandId, IfHudCountdownHasStopped },
            { IfHudCountdownLessThan.CommandId, IfHudCountdownLessThan },
            { IfHudCountdownGreaterThan.CommandId, IfHudCountdownGreaterThan },
            { ChrTrySpawningAtPad.CommandId, ChrTrySpawningAtPad },
            { ChrTrySpawningNextToUnseenChr.CommandId, ChrTrySpawningNextToUnseenChr },
            { GuardTrySpawningItem.CommandId, GuardTrySpawningItem },
            { GuardTrySpawningHat.CommandId, GuardTrySpawningHat },
            { ChrTrySpawningClone.CommandId, ChrTrySpawningClone },
            { TextPrintBottom.CommandId, TextPrintBottom },
            { TextPrintTop.CommandId, TextPrintTop },
            { SfxPlay.CommandId, SfxPlay },
            { SfxEmitFromObject.CommandId, SfxEmitFromObject },
            { SfxEmitFromPad.CommandId, SfxEmitFromPad },
            { SfxSetChannelVolume.CommandId, SfxSetChannelVolume },
            { SfxFadeChannelVolume.CommandId, SfxFadeChannelVolume },
            { SfxStopChannel.CommandId, SfxStopChannel },
            { IfSfxChannelVolumeLessThan.CommandId, IfSfxChannelVolumeLessThan },
            { VehicleStartPath.CommandId, VehicleStartPath },
            { VehicleSpeed.CommandId, VehicleSpeed },
            { AircraftRotorSpeed.CommandId, AircraftRotorSpeed },
            { IfCameraIsInIntro.CommandId, IfCameraIsInIntro },
            { IfCameraIsInBondSwirl.CommandId, IfCameraIsInBondSwirl },
            { TvChangeScreenBank.CommandId, TvChangeScreenBank },
            { IfBondInTank.CommandId, IfBondInTank },
            { ExitLevel.CommandId, ExitLevel },
            { CameraReturnToBond.CommandId, CameraReturnToBond },
            { CameraLookAtBondFromPad.CommandId, CameraLookAtBondFromPad },
            { CameraSwitch.CommandId, CameraSwitch },
            { IfBondYPosLessThan.CommandId, IfBondYPosLessThan },
            { HudHideAndLockControlsAndPauseMissionTime.CommandId, HudHideAndLockControlsAndPauseMissionTime },
            { HudShowAllAndUnlockControlsAndResumeMissionTime.CommandId, HudShowAllAndUnlockControlsAndResumeMissionTime },
            { ChrTryTeleportingToPad.CommandId, ChrTryTeleportingToPad },
            { ScreenFadeToBlack.CommandId, ScreenFadeToBlack },
            { ScreenFadeFromBlack.CommandId, ScreenFadeFromBlack },
            { IfScreenFadeCompleted.CommandId, IfScreenFadeCompleted },
            { ChrHideAll.CommandId, ChrHideAll },
            { ChrShowAll.CommandId, ChrShowAll },
            { DoorOpenInstant.CommandId, DoorOpenInstant },
            { ChrRemoveItemInHand.CommandId, ChrRemoveItemInHand },
            { IfNumberOfActivePlayersLessThan.CommandId, IfNumberOfActivePlayersLessThan },
            { IfBondItemTotalAmmoLessThan.CommandId, IfBondItemTotalAmmoLessThan },
            { BondEquipItem.CommandId, BondEquipItem },
            { BondEquipItemCinema.CommandId, BondEquipItemCinema },
            { BondSetLockedVelocity.CommandId, BondSetLockedVelocity },
            { IfObjectInRoomWithPad.CommandId, IfObjectInRoomWithPad },
            { IfGuardIsFiringAndUsing180RangeFlag.CommandId, IfGuardIsFiringAndUsing180RangeFlag },
            { IfGuardIsFiring.CommandId, IfGuardIsFiring },
            { SwitchFogInstantly.CommandId, SwitchFogInstantly },
            { TriggerFadeAndExitLevelOnButtonPress.CommandId, TriggerFadeAndExitLevelOnButtonPress },
            { IfBondIsDead.CommandId, IfBondIsDead },
            { BondDisableDamageAndPickups.CommandId, BondDisableDamageAndPickups },
            { BondHideWeapons.CommandId, BondHideWeapons },
            { CameraOrbitPad.CommandId, CameraOrbitPad },
            { CreditsRoll.CommandId, CreditsRoll },
            { IfCreditsHasCompleted.CommandId, IfCreditsHasCompleted },
            { IfObjectiveAllCompleted.CommandId, IfObjectiveAllCompleted },
            { IfFolderActorIsEqual.CommandId, IfFolderActorIsEqual },
            { IfBondDamageAndPickupsDisabled.CommandId, IfBondDamageAndPickupsDisabled },
            { MusicXtrackPlay.CommandId, MusicXtrackPlay },
            { MusicXtrackStop.CommandId, MusicXtrackStop },
            { TriggerExplosionsAroundBond.CommandId, TriggerExplosionsAroundBond },
            { IfKilledCiviliansGreaterThan.CommandId, IfKilledCiviliansGreaterThan },
            { IfChrWasShotSinceLastCheck.CommandId, IfChrWasShotSinceLastCheck },
            { BondKilledInAction.CommandId, BondKilledInAction },
            { GuardRaisesArms.CommandId, GuardRaisesArms },
            { GasLeakAndFadeFog.CommandId, GasLeakAndFadeFog },
            { ObjectRocketLaunch.CommandId, ObjectRocketLaunch },
        };
    }


}
