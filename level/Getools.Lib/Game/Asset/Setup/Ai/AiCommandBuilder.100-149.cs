using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    public partial class AiCommandBuilder
    {
        /// <summary>
        /// makes chr hold tagged object
        /// </summary>
        /// <remarks>
        /// if chr's hands are occupied, object will be equipped as an concealed attachment. but if tagged object's handedness flag is free on guard then guard will equip weapon. tagged object's prop must have a holding position command within the model file
        /// </remarks>
        public static AiFixedCommandDescription ChrEquipObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_equip_object",
                    CommandId = 100,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
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
        public static AiFixedCommandDescription ObjectMoveToPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_move_to_pad",
                    CommandId = 101,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "pad", ByteLength = 2 },
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
        public static AiFixedCommandDescription DoorOpen
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "door_open",
                    CommandId = 102,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// close tagged door
        /// </summary>
        public static AiFixedCommandDescription DoorClose
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "door_close",
                    CommandId = 103,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfDoorStateEqual
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_door_state_equal",
                    CommandId = 104,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "door_state", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfDoorHasBeenOpenedBefore
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_door_has_been_opened_before",
                    CommandId = 105,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription DoorSetLock
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "door_set_lock",
                    CommandId = 106,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "lock_flag", ByteLength = 1 },
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
        public static AiFixedCommandDescription DoorUnsetLock
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "door_unset_lock",
                    CommandId = 107,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "lock_flag", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfDoorLockEqual
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_door_lock_equal",
                    CommandId = 108,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "lock_flag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfObjectiveNumComplete
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_objective_num_complete",
                    CommandId = 109,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "obj_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardTryUnknown6E
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_unknown6E",
                    CommandId = 110,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "unknown_flag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardTryUnknown6F
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_unknown6F",
                    CommandId = 111,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "unknown_flag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGameDifficultyLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_game_difficulty_less_than",
                    CommandId = 112,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "argument", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGameDifficultyGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_game_difficulty_greater_than",
                    CommandId = 113,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "argument", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfMissionTimeLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_mission_time_less_than",
                    CommandId = 114,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfMissionTimeGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_mission_time_greater_than",
                    CommandId = 115,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "seconds", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfSystemPowerTimeLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_system_power_time_less_than",
                    CommandId = 116,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "minutes", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfSystemPowerTimeGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_system_power_time_greater_than",
                    CommandId = 117,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "minutes", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLevelIdLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_level_id_less_than",
                    CommandId = 118,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "level_id", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLevelIdGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_level_id_greater_than",
                    CommandId = 119,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "level_id", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGuardHitsLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_hits_less_than",
                    CommandId = 120,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "hit_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGuardHitsGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_hits_greater_than",
                    CommandId = 121,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "hit_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGuardHitsMissedLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_hits_missed_less_than",
                    CommandId = 122,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "missed_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfGuardHitsMissedGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_hits_missed_greater_than",
                    CommandId = 123,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "missed_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfChrHealthLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_health_less_than",
                    CommandId = 124,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "health", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfChrHealthGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_health_greater_than",
                    CommandId = 125,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "health", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfChrWasDamagedSinceLastCheck
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_was_damaged_since_last_check",
                    CommandId = 126,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfBondHealthLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_health_less_than",
                    CommandId = 127,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "health", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfBondHealthGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_health_greater_than",
                    CommandId = 128,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "health", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte1Set
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_1_set",
                    CommandId = 129,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "set_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte1Add
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_1_add",
                    CommandId = 130,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "add_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte1Subtract
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_1_subtract",
                    CommandId = 131,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "subtract_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLocalByte1LessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_byte_1_less_than",
                    CommandId = 132,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "compare_byte", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLocalByte1LessThanRandomSeed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_byte_1_less_than_random_seed",
                    CommandId = 133,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte2Set
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_2_set",
                    CommandId = 134,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "set_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte2Add
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_2_add",
                    CommandId = 135,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "add_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription LocalByte2Subtract
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_byte_2_subtract",
                    CommandId = 136,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "subtract_byte", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLocalByte2LessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_byte_2_less_than",
                    CommandId = 137,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "compare_byte", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLocalByte2LessThanRandomSeed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_byte_2_less_than_random_seed",
                    CommandId = 138,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetHearingScale
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_hearing_scale",
                    CommandId = 139,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "hearing_scale", ByteLength = 2 },
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
        public static AiFixedCommandDescription GuardSetVisionRange
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_vision_range",
                    CommandId = 140,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "vision_range", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetGrenadeProbability
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_grenade_probability",
                    CommandId = 141,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "grenade_prob", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetChrNum
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_chr_num",
                    CommandId = 142,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetHealthTotal
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_health_total",
                    CommandId = 143,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "total_health", ByteLength = 2 },
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
        public static AiFixedCommandDescription GuardSetArmour
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_armour",
                    CommandId = 144,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "armour_value", ByteLength = 2 },
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
        public static AiFixedCommandDescription GuardSetSpeedRating
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_speed_rating",
                    CommandId = 145,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "speed_rating", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetArghRating
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_argh_rating",
                    CommandId = 146,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "speed_rating", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetAccuracyRating
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_accuracy_rating",
                    CommandId = 147,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "accuracy_rating", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardBitfieldSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_bitfield_set_on",
                    CommandId = 148,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardBitfieldSetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_bitfield_set_off",
                    CommandId = 149,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }
    }
}
