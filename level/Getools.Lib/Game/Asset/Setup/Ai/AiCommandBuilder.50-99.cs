using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Asset.Setup.Ai
{
    /// <summary>
    /// <see cref="AiCommandBuilder"/> command bytes 50-99.
    /// </summary>
    public partial class AiCommandBuilder
    {
        /// <summary>
        /// check vision for bond, goto label if spotted bond
        /// </summary>
        /// <remarks>
        /// uses chr->visionrange while checking for bond. once bond has been spotted, check if bond and guard are within line of sight (ignores facing direction). injured guards will also set spotted Bond state (won't work with invincible/armored guards). if bond breaks line of sight, do not goto label. if bond has broken line of sight for more than 10 seconds, reset spotted bond state. when using with command 3E, make sure 32 takes priority over command 3E
        /// </remarks>
        public static AiFixedCommandDescription IfGuardSeesBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_sees_bond",
                    CommandId = 50,
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
        /// generate a random byte and store to chr->random
        /// </summary>
        /// <remarks>
        /// random byte range is 00-FF (unsigned)
        /// </remarks>
        public static AiFixedCommandDescription RandomGenerateSeed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "random_generate_seed",
                    CommandId = 51,
                    CommandLengthBytes = 1,
                    NumberParameters = 0,
                    CommandParameters = new List<IAiParameter>(),
                };
            }
        }

        /// <summary>
        /// if chr->random < byte, goto label
        /// </summary>
        /// <remarks>
        /// compare is unsigned
        /// </remarks>
        public static AiFixedCommandDescription IfRandomSeedLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_random_seed_less_than",
                    CommandId = 52,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "cbyte", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if chr->random > byte, goto label
        /// </summary>
        /// <remarks>
        /// compare is unsigned
        /// </remarks>
        public static AiFixedCommandDescription IfRandomSeedGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_random_seed_greater_than",
                    CommandId = 53,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "cbyte", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if alarm is activated, goto label
        /// </summary>
        /// <remarks>
        /// this command works but is unused in retail game, use command 37 instead
        /// </remarks>
        public static AiFixedCommandDescription IfAlarmIsOnUnused
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_alarm_is_on_unused",
                    CommandId = 54,
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
        /// if alarm is activated, goto label
        /// </summary>
        public static AiFixedCommandDescription IfAlarmIsOn
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_alarm_is_on",
                    CommandId = 55,
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
        /// if gas leak event triggered, goto label
        /// </summary>
        /// <remarks>
        /// once gas leak event has started, always goto label
        /// </remarks>
        public static AiFixedCommandDescription IfGasIsLeaking
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_gas_is_leaking",
                    CommandId = 56,
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
        /// if guard heard bond fire weapon, goto label
        /// </summary>
        /// <remarks>
        /// uses chr->hearingscale while listening for bond. to check if bond has shot within the last 10 seconds, use command 3F
        /// </remarks>
        public static AiFixedCommandDescription IfGuardHeardBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_heard_bond",
                    CommandId = 57,
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
        /// if guard sees another guard shot (from anyone), goto label
        /// </summary>
        /// <remarks>
        /// guard friendly fire (if flagged) will trigger this command to goto label. command checks if chr->chrseeshot is set to valid chrnum (not -1). does not work with shot invincible/armoured guards
        /// </remarks>
        public static AiFixedCommandDescription IfGuardSeeAnotherGuardShot
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_see_another_guard_shot",
                    CommandId = 58,
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
        /// if guard sees another guard die (from anyone), goto label
        /// </summary>
        /// <remarks>
        /// when a guard in sight switches to ACT_DIE/ACT_DEAD, goto label. command checks if chr->chrseedie is set to valid chrnum (not -1)
        /// </remarks>
        public static AiFixedCommandDescription IfGuardSeeAnotherGuardDie
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_see_another_guard_die",
                    CommandId = 59,
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
        /// if guard and bond are within line of sight, goto label
        /// </summary>
        /// <remarks>
        /// line of sight uses clipping - ignores facing direction of bond/guard. if prop/guard is in the way do not goto label. does not use chr->visionrange for line of sight check. use command 32 to check using chr->visionrange and command 42 to account for bond's view
        /// </remarks>
        public static AiFixedCommandDescription IfGuardAndBondWithinLineOfSight
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_and_bond_within_line_of_sight",
                    CommandId = 60,
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
        /// if guard and bond are within partial line of sight, goto label
        /// </summary>
        /// <remarks>
        /// unused command, functions like above but only goto label if bond is half occluded by clipping (not blocked or within full view)
        /// </remarks>
        public static AiFixedCommandDescription IfGuardAndBondWithinPartialLineOfSight
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_and_bond_within_partial_line_of_sight",
                    CommandId = 61,
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
        /// if guard was shot (from anyone) or saw Bond within the last 10 seconds, goto label
        /// </summary>
        /// <remarks>
        /// command will not count guard as shot if they are invincible/have armour. if guard saw Bond (using command 32) in the last 10 seconds, goto label. when using with command 32, make sure 32 takes priority over command 3E. if guard was injured within the last 10 seconds, goto label when finished injury reaction animation (will not work with invincible/armored guards). to check if guard was hit/damaged use commands 7E/F8 instead, or check if guard flags CHRFLAG_WAS_DAMAGED/CHRFLAG_WAS_HIT are set using command 9F/A2
        /// </remarks>
        public static AiFixedCommandDescription IfGuardWasShotOrSeenBondWithinLast10Secs
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_was_shot_or_seen_bond_within_last_10_secs",
                    CommandId = 62,
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
        /// if guard heard bond fire weapon within the last 10 seconds, goto label
        /// </summary>
        /// <remarks>
        /// uses chr->hearingscale while listening for bond. to check if bond has now fired weapon instead of within the last 10 seconds, use command 39
        /// </remarks>
        public static AiFixedCommandDescription IfGuardHeardBondWithinLast10Secs
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_heard_bond_within_last_10_secs",
                    CommandId = 63,
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
        /// if guard is in same room with chr, goto label
        /// </summary>
        public static AiFixedCommandDescription IfGuardInRoomWithChr
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_in_room_with_chr",
                    CommandId = 64,
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
        /// if guard has not been seen before on screen, goto label
        /// </summary>
        /// <remarks>
        /// when bond has seen guard, it will add a flag to chr->chrflags. the seen flag will be kept true for duration of level
        /// </remarks>
        public static AiFixedCommandDescription IfGuardHasNotBeenSeen
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_has_not_been_seen",
                    CommandId = 65,
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
        /// if guard is currently being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// portals will affect this command's output. if guard is being culled off screen, command will not goto label
        /// </remarks>
        public static AiFixedCommandDescription IfGuardIsOnScreen
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_is_on_screen",
                    CommandId = 66,
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
        /// if the room containing guard is being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// only checks if room is being rendered, not if bond can see guard. to check if guard is being rendered use command 42 instead.
        /// </remarks>
        public static AiFixedCommandDescription IfGuardRoomContainingSelfIsOnScreen
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_room_containing_self_is_on_screen",
                    CommandId = 67,
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
        /// if room containing pad is being rendered on screen, goto label
        /// </summary>
        /// <remarks>
        /// only checks if room is being rendered, not if bond can see inside room
        /// </remarks>
        public static AiFixedCommandDescription IfRoomContainingPadIsOnScreen
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_room_containing_pad_is_on_screen",
                    CommandId = 68,
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
        /// if bond is looking/aiming at guard, goto label
        /// </summary>
        /// <remarks>
        /// also checks if crosshair is aiming at guard
        /// </remarks>
        public static AiFixedCommandDescription IfGuardIsTargetedByBond
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_is_targeted_by_bond",
                    CommandId = 69,
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
        /// if bond's shot missed/landed near guard, goto label
        /// </summary>
        /// <remarks>
        /// command will sometimes goto label if guard was shot - use command 3E instead to check if guard was shot recently (more consistent)
        /// </remarks>
        public static AiFixedCommandDescription IfGuardShotFromBondMissed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_shot_from_bond_missed",
                    CommandId = 70,
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
        /// if guard's counter-clockwise direction to bond < direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): 00: no rotation, never goto label because degrees are always above 0 40: bond and guard within 9-to-12 o'clock (90 degrees) 80: bond is on guard's left-side (180 degrees) C0: bond and guard within 3-to-12 o'clock (270 degrees) FF: full rotation, always goto label except for a tiny degree (0-359 degrees)
        /// </remarks>
        public static AiFixedCommandDescription IfGuardCounterClockwiseDirectionToBondLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_to_bond_less_than",
                    CommandId = 71,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "direction", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's counter-clockwise direction to bond > direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): FF: no rotation, never goto label except for a tiny degree (0-1 degrees) C0: bond and guard within 12-to-3 o'clock (90 degrees) 80: bond on guard's right-side (180 degrees) 40: bond and guard within 12-to-9 o'clock (270 degrees) 00: full rotation, always goto label
        /// </remarks>
        public static AiFixedCommandDescription IfGuardCounterClockwiseDirectionToBondGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_to_bond_greater_than",
                    CommandId = 72,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "direction", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's counter-clockwise direction to guard < direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): 00: no rotation, never goto label because degrees are always above 0 40: guard and bond within 9-to-12 o'clock (90 degrees) 80: guard is on bond's left-side (180 degrees) C0: guard and bond within 3-to-12 o'clock (270 degrees) FF: full rotation, always goto label except for a tiny degree (0-359 degrees)
        /// </remarks>
        public static AiFixedCommandDescription IfGuardCounterClockwiseDirectionFromBondLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_from_bond_less_than",
                    CommandId = 73,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "direction", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's counter-clockwise direction to guard > direction argument, goto label
        /// </summary>
        /// <remarks>
        /// direction input (hex): FF: no rotation, never goto label except for a tiny degree (0-1 degrees) C0: guard and bond within 12-to-3 o'clock (90 degrees) 80: guard on bond's right-side (180 degrees) 40: guard and bond within 12-to-9 o'clock (270 degrees) 00: full rotation, always goto label
        /// </remarks>
        public static AiFixedCommandDescription IfGuardCounterClockwiseDirectionFromBondGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_counter_clockwise_direction_from_bond_greater_than",
                    CommandId = 74,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "direction", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's distance to bond < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfGuardDistanceToBondLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_distance_to_bond_less_than",
                    CommandId = 75,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's distance to bond > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfGuardDistanceToBondGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_distance_to_bond_greater_than",
                    CommandId = 76,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if chr's distance to pad < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfChrDistanceToPadLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_distance_to_pad_less_than",
                    CommandId = 77,
                    CommandLengthBytes = 7,
                    NumberParameters = 4,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if chr's distance to pad > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfChrDistanceToPadGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_distance_to_pad_greater_than",
                    CommandId = 78,
                    CommandLengthBytes = 7,
                    NumberParameters = 4,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's distance to chr < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfGuardDistanceToChrLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_distance_to_chr_less_than",
                    CommandId = 79,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's distance to chr > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfGuardDistanceToChrGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_guard_distance_to_chr_greater_than",
                    CommandId = 80,
                    CommandLengthBytes = 5,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "chr_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if guard's distance to any chr < distance argument, set chr->padpreset1 to found guard's chrnum and goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter. command does not pick the closest found chr, but whoever was first found within the distance argument. if no guards were found within distance range, do not goto label
        /// </remarks>
        public static AiFixedCommandDescription GuardTrySettingChrPresetToGuardWithinDistance
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "guard_try_setting_chr_preset_to_guard_within_distance",
                    CommandId = 81,
                    CommandLengthBytes = 4,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's distance to pad < distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfBondDistanceToPadLessThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_distance_to_pad_less_than",
                    CommandId = 82,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond's distance to pad > distance argument, goto label
        /// </summary>
        /// <remarks>
        /// argument scale is 10 units per meter
        /// </remarks>
        public static AiFixedCommandDescription IfBondDistanceToPadGreaterThan
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_distance_to_pad_greater_than",
                    CommandId = 83,
                    CommandLengthBytes = 6,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "distance", ByteLength = 2 },
                        new AiParameter() { ParameterName = "pad", ByteLength = 2 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if chr id in same room with pad, goto label
        /// </summary>
        public static AiFixedCommandDescription IfChrInRoomWithPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_chr_in_room_with_pad",
                    CommandId = 84,
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
        /// if bond in same room with pad, goto label
        /// </summary>
        public static AiFixedCommandDescription IfBondInRoomWithPad
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_in_room_with_pad",
                    CommandId = 85,
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
        /// if bond collected tagged object, goto label
        /// </summary>
        public static AiFixedCommandDescription IfBondCollectedObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_collected_object",
                    CommandId = 86,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if item exists in level and is stationary (not moving/in mid-air), goto label
        /// </summary>
        /// <remarks>
        /// used to check if bond threw an item in level. also checks if item was attached to an object (item is stationary within level). so make sure command 58 takes priority over command 57 when using both commands
        /// </remarks>
        public static AiFixedCommandDescription IfItemIsStationaryWithinLevel
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_item_is_stationary_within_level",
                    CommandId = 87,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if item was thrown onto tagged object, goto label
        /// </summary>
        /// <remarks>
        /// used to check if bond threw an item onto a tagged object. if used with command 57, make sure command 58 take priority over command 57
        /// </remarks>
        public static AiFixedCommandDescription IfItemIsAttachedToObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_item_is_attached_to_object",
                    CommandId = 88,
                    CommandLengthBytes = 4,
                    NumberParameters = 3,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond has an item equipped (currently held), goto label
        /// </summary>
        public static AiFixedCommandDescription IfBondHasItemEquipped
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_has_item_equipped",
                    CommandId = 89,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "item_num", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if tagged object exists in level, goto label
        /// </summary>
        public static AiFixedCommandDescription IfObjectExists
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_exists",
                    CommandId = 90,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if tagged object is not destroyed, goto label
        /// </summary>
        public static AiFixedCommandDescription IfObjectNotDestroyed
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_not_destroyed",
                    CommandId = 91,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if tagged object was activated since last check, goto label
        /// </summary>
        /// <remarks>
        /// when executed, it will clear tagged object's activated flag. only bond and command 5E can activate tagged objects. bond cannot activate destroyed objects
        /// </remarks>
        public static AiFixedCommandDescription IfObjectWasActivated
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_object_was_activated",
                    CommandId = 92,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// if bond used a gadget item on a tagged object since last check, goto label
        /// </summary>
        /// <remarks>
        /// gadgets are a pre-defined list of items set to gadget flag: ITEM_BOMBDEFUSER ITEM_DATATHIEF ITEM_DOORDECODER ITEM_EXPLOSIVEFLOPPY ITEM_DATTAPE
        /// </remarks>
        public static AiFixedCommandDescription IfBondUsedGadgetOnObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "if_bond_used_gadget_on_object",
                    CommandId = 93,
                    CommandLengthBytes = 3,
                    NumberParameters = 2,
                    CommandParameters = new List<IAiParameter>()
                    {
                        new AiParameter() { ParameterName = "object_tag", ByteLength = 1 },
                        new AiParameter() { ParameterName = "label", ByteLength = 1 },
                    },
                };
            }
        }

        /// <summary>
        /// activate a tagged object
        /// </summary>
        /// <remarks>
        /// command does not check if object has been destroyed
        /// </remarks>
        public static AiFixedCommandDescription ObjectActivate
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_activate",
                    CommandId = 94,
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
        /// destroy/explode a tagged object
        /// </summary>
        /// <remarks>
        /// only works if object is not destroyed. cannot destroy invincible objects
        /// </remarks>
        public static AiFixedCommandDescription ObjectDestroy
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_destroy",
                    CommandId = 95,
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
        /// drop tagged object held/attached to chr
        /// </summary>
        /// <remarks>
        /// item must be held/attached to a chr. embedded objects will not drop, only works with attached objects. props can be damaged on drop
        /// </remarks>
        public static AiFixedCommandDescription ObjectDropFromChr
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "object_drop_from_chr",
                    CommandId = 96,
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
        /// make chr drop all concealed attachments
        /// </summary>
        /// <remarks>
        /// item must be attached to chr, to drop held items use command 62. embedded objects will not drop, only works with attached objects. props can be damaged on drop
        /// </remarks>
        public static AiFixedCommandDescription ChrDropAllConcealedItems
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_drop_all_concealed_items",
                    CommandId = 97,
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
        /// make chr drop all held items
        /// </summary>
        /// <remarks>
        /// items must be held by chr, to drop concealed attachments use command 61. embedded objects will not drop, only works with attached objects
        /// </remarks>
        public static AiFixedCommandDescription ChrDropAllHeldItems
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "chr_drop_all_held_items",
                    CommandId = 98,
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
        /// force bond to instantly collect a tagged object
        /// </summary>
        /// <remarks>
        /// does not trigger bottom text telling player they collected an item
        /// </remarks>
        public static AiFixedCommandDescription BondCollectObject
        {
            get
            {
                return new AiFixedCommandDescription()
                {
                    DecompName = "bond_collect_object",
                    CommandId = 99,
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
