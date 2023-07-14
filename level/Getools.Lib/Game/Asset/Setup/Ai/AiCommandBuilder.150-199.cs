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
        /// if any bits in argument are set on in chr->BITFIELD, goto label
        /// </summary>
        /// <remarks>
        /// can be used by obj ai lists, obj lists are free to utilize the entire spectrum of flags
        /// </remarks>
        public static AiFixedCommandDescription IfGuardBitfieldIsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_bitfield_is_set_on",
                    CommandId = 150,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ChrBitfieldSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_bitfield_set_on",
                    CommandId = 151,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
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
        public static AiFixedCommandDescription ChrBitfieldSetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_bitfield_set_off",
                    CommandId = 152,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// if any bits in argument are set on in chr->BITFIELD, goto label
        /// </summary>
        public static AiFixedCommandDescription IfChrBitfieldIsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_bitfield_is_set_on",
                    CommandId = 153,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ObjectiveBitfieldSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "objective_bitfield_set_on",
                    CommandId = 154,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription ObjectiveBitfieldSetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "objective_bitfield_set_off",
                    CommandId = 155,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription IfObjectiveBitfieldIsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_objective_bitfield_is_set_on",
                    CommandId = 156,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardFlagsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_flags_set_on",
                    CommandId = 157,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription GuardFlagsSetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_flags_set_off",
                    CommandId = 158,
                    CommandLengthBytes = 5,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription IfGuardFlagsIsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_flags_is_set_on",
                    CommandId = 159,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ChrFlagsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_flags_set_on",
                    CommandId = 160,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription ChrFlagsSetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_flags_set_off",
                    CommandId = 161,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription IfChrFlagsIsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_flags_is_set_on",
                    CommandId = 162,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ObjectFlags1SetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_flags_1_set_on",
                    CommandId = 163,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription ObjectFlags1SetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_flags_1_set_off",
                    CommandId = 164,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription IfObjectFlags1IsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_flags_1_is_set_on",
                    CommandId = 165,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ObjectFlags2SetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_flags_2_set_on",
                    CommandId = 166,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription ObjectFlags2SetOff
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_flags_2_set_off",
                    CommandId = 167,
                    CommandLengthBytes = 6,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
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
        public static AiFixedCommandDescription IfObjectFlags2IsSetOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_flags_2_is_set_on",
                    CommandId = 168,
                    CommandLengthBytes = 7,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetChrPreset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_chr_preset",
                    CommandId = 169,
                    CommandLengthBytes = 2,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_preset", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->chrpreset1 to chr_preset
        /// </summary>
        public static AiFixedCommandDescription ChrSetChrPreset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_set_chr_preset",
                    CommandId = 170,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "chr_preset", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardSetPadPreset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_set_pad_preset",
                    CommandId = 171,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "pad_preset", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// set chr->padpreset1 to pad_preset
        /// </summary>
        public static AiFixedCommandDescription ChrSetPadPreset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_set_pad_preset",
                    CommandId = 172,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "pad_preset", ByteLength = 2 },
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
        public static AiVariableCommandDescription DebugLog
        {
            get
            {
                return new AiVariableCommandDescription()
                {
                    DecompName = "debug_log",
                    CommandId = 173,
                    CommandLengthBytes = -1,
                };
            }
        }

        /// <summary>
        /// reset and start chr->timer60
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiFixedCommandDescription LocalTimerResetStart
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_timer_reset_start",
                    CommandId = 174,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// reset chr->timer60
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiFixedCommandDescription LocalTimerReset
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_timer_reset",
                    CommandId = 175,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// pauses chr->timer60 (does not reset value)
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiFixedCommandDescription LocalTimerStop
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_timer_stop",
                    CommandId = 176,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// start chr->timer60 (does not reset value)
        /// </summary>
        /// <remarks>
        /// local timer is different to hud countdown. local timer is unique for each chr, while hud countdown is global for the entire mission. chr->timer60 only counts up
        /// </remarks>
        public static AiFixedCommandDescription LocalTimerStart
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "local_timer_start",
                    CommandId = 177,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// if chr->timer60 is not active (paused), goto label
        /// </summary>
        /// <remarks>
        /// by default, chr->timer60 is inactive
        /// </remarks>
        public static AiFixedCommandDescription IfLocalTimerHasStopped
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_timer_has_stopped",
                    CommandId = 178,
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
        /// if chr->timer60 < time60, goto label
        /// </summary>
        /// <remarks>
        /// time60 argument is converted to float from unsigned int and compared. chr->timer60 only counts up
        /// </remarks>
        public static AiFixedCommandDescription IfLocalTimerLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_timer_less_than",
                    CommandId = 179,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "time60", ByteLength = 3 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription IfLocalTimerGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_local_timer_greater_than",
                    CommandId = 180,
                    CommandLengthBytes = 5,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "time60", ByteLength = 3 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
                    }
                };
            }
        }

        /// <summary>
        /// shows the hud countdown
        /// </summary>
        public static AiFixedCommandDescription HudCountdownShow
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_countdown_show",
                    CommandId = 181,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// hides the hud countdown
        /// </summary>
        /// <remarks>
        /// can be used as a hidden global timer for objective logic
        /// </remarks>
        public static AiFixedCommandDescription HudCountdownHide
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_countdown_hide",
                    CommandId = 182,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// set the hud countdown
        /// </summary>
        /// <remarks>
        /// to make the timer count up, set to 0 and start timer
        /// </remarks>
        public static AiFixedCommandDescription HudCountdownSet
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_countdown_set",
                    CommandId = 183,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "seconds", ByteLength = 2 },
                    }
                };
            }
        }

        /// <summary>
        /// stops the hud countdown
        /// </summary>
        public static AiFixedCommandDescription HudCountdownStop
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_countdown_stop",
                    CommandId = 184,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// start the hud countdown
        /// </summary>
        public static AiFixedCommandDescription HudCountdownStart
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "hud_countdown_start",
                    CommandId = 185,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>()
                };
            }
        }

        /// <summary>
        /// if hud countdown isn't active (paused), goto label
        /// </summary>
        /// <remarks>
        /// by default, hud countdown is inactive
        /// </remarks>
        public static AiFixedCommandDescription IfHudCountdownHasStopped
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_hud_countdown_has_stopped",
                    CommandId = 186,
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
        /// if hud countdown < seconds, goto label
        /// </summary>
        /// <remarks>
        /// if seconds argument is 0, it will only goto label if timer is less than zero (counting up). seconds value is unsigned and can't test negative values
        /// </remarks>
        public static AiFixedCommandDescription IfHudCountdownLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_hud_countdown_less_than",
                    CommandId = 187,
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
        /// if hud countdown > seconds, goto label
        /// </summary>
        /// <remarks>
        /// if seconds argument is 0, it will only goto label if timer is greater than zero (counting down). seconds value is unsigned and can't test negative values
        /// </remarks>
        public static AiFixedCommandDescription IfHudCountdownGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_hud_countdown_greater_than",
                    CommandId = 188,
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
        /// spawn chr at pad, goto label if successful
        /// </summary>
        /// <remarks>
        /// if out of memory/can't spawn chr, do not goto label. if pad is blocked, attempt to spawn chr around pad. bitfield uses SPAWN_# defines
        /// </remarks>
        public static AiFixedCommandDescription ChrTrySpawningAtPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_try_spawning_at_pad",
                    CommandId = 189,
                    CommandLengthBytes = 12,
                    NumberParameters = 6,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "body_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "head_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "pad", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ChrTrySpawningNextToUnseenChr
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_try_spawning_next_to_unseen_chr",
                    CommandId = 190,
                    CommandLengthBytes = 11,
                    NumberParameters = 6,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "body_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "head_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "chr_num_target", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardTrySpawningItem
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_spawning_item",
                    CommandId = 191,
                    CommandLengthBytes = 9,
                    NumberParameters = 4,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "prop_num", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "prop_bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription GuardTrySpawningHat
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_spawning_hat",
                    CommandId = 192,
                    CommandLengthBytes = 8,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "prop_num", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "prop_bitfield", ByteLength = 4 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription ChrTrySpawningClone
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_try_spawning_clone",
                    CommandId = 193,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "ai_list", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "label", ByteLength = 1 },
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
        public static AiFixedCommandDescription TextPrintBottom
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "text_print_bottom",
                    CommandId = 194,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "text_slot", ByteLength = 2 },
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
        public static AiFixedCommandDescription TextPrintTop
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "text_print_top",
                    CommandId = 195,
                    CommandLengthBytes = 3,
                    NumberParameters = 1,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "text_slot", ByteLength = 2 },
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
        public static AiFixedCommandDescription SfxPlay
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_play",
                    CommandId = 196,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "sound_num", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "channel_num", ByteLength = 1 },
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
        public static AiFixedCommandDescription SfxEmitFromObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_emit_from_object",
                    CommandId = 197,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "vol_decay_time60", ByteLength = 2 },
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
        public static AiFixedCommandDescription SfxEmitFromPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_emit_from_pad",
                    CommandId = 198,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "pad", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "vol_decay_time60", ByteLength = 2 },
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
        public static AiFixedCommandDescription SfxSetChannelVolume
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "sfx_set_channel_volume",
                    CommandId = 199,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter(){ ParameterName = "channel_num", ByteLength = 1 },
                        new AiParameter(){ ParameterName = "target_volume", ByteLength = 2 },
                        new AiParameter(){ ParameterName = "transition_time60", ByteLength = 2 },
                    }
                };
            }
        }
    }
}
