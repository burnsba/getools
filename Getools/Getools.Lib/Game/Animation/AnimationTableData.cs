using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Getools.Lib.Game.Animation
{
    /// <summary>
    /// Animation table constants, hard coded from ROM.
    /// </summary>
    public class AnimationTableData
    {
        /// <summary>
        /// Flag to indicate whether <see cref="_lookup"/> has been populated yet.
        /// </summary>
        private static bool _init = false;

        /// <summary>
        /// Maps rom address to class property.
        /// </summary>
        private static Dictionary<int, AnimationTableData> _lookup = new Dictionary<int, AnimationTableData>();

        private AnimationTableData()
        {
            Name = "unknown";
            Address = 1;
        }

#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1516 // Elements should be separated by blank line
        public static AnimationTableData ANIM_idle { get; } = new AnimationTableData { Name = "idle", Address = 0x1C, };
        public static AnimationTableData ANIM_fire_standing { get; } = new AnimationTableData { Name = "fire_standing", Address = 0x144, };
        public static AnimationTableData ANIM_fire_standing_fast { get; } = new AnimationTableData { Name = "fire_standing_fast", Address = 0x214, };
        public static AnimationTableData ANIM_fire_hip { get; } = new AnimationTableData { Name = "fire_hip", Address = 0x318, };
        public static AnimationTableData ANIM_fire_shoulder_left { get; } = new AnimationTableData { Name = "fire_shoulder_left", Address = 0x3C4, };
        public static AnimationTableData ANIM_fire_turn_right1 { get; } = new AnimationTableData { Name = "fire_turn_right1", Address = 0x610, };
        public static AnimationTableData ANIM_fire_turn_right2 { get; } = new AnimationTableData { Name = "fire_turn_right2", Address = 0x814, };
        public static AnimationTableData ANIM_fire_kneel_right_leg { get; } = new AnimationTableData { Name = "fire_kneel_right_leg", Address = 0x990, };
        public static AnimationTableData ANIM_fire_kneel_left_leg { get; } = new AnimationTableData { Name = "fire_kneel_left_leg", Address = 0xB84, };
        public static AnimationTableData ANIM_fire_kneel_left { get; } = new AnimationTableData { Name = "fire_kneel_left", Address = 0xDB4, };
        public static AnimationTableData ANIM_fire_kneel_right { get; } = new AnimationTableData { Name = "fire_kneel_right", Address = 0x1028, };
        public static AnimationTableData ANIM_fire_roll_left { get; } = new AnimationTableData { Name = "fire_roll_left", Address = 0x1334, };
        public static AnimationTableData ANIM_fire_roll_right1 { get; } = new AnimationTableData { Name = "fire_roll_right1", Address = 0x1578, };
        public static AnimationTableData ANIM_fire_roll_left_fast { get; } = new AnimationTableData { Name = "fire_roll_left_fast", Address = 0x17B4, };
        public static AnimationTableData ANIM_hit_left_shoulder { get; } = new AnimationTableData { Name = "hit_left_shoulder", Address = 0x186C, };
        public static AnimationTableData ANIM_hit_right_shoulder { get; } = new AnimationTableData { Name = "hit_right_shoulder", Address = 0x1984, };
        public static AnimationTableData ANIM_hit_left_arm { get; } = new AnimationTableData { Name = "hit_left_arm", Address = 0x1A6C, };
        public static AnimationTableData ANIM_hit_right_arm { get; } = new AnimationTableData { Name = "hit_right_arm", Address = 0x1B54, };
        public static AnimationTableData ANIM_hit_left_hand { get; } = new AnimationTableData { Name = "hit_left_hand", Address = 0x1C9C, };
        public static AnimationTableData ANIM_hit_right_hand { get; } = new AnimationTableData { Name = "hit_right_hand", Address = 0x1E40, };
        public static AnimationTableData ANIM_hit_left_leg { get; } = new AnimationTableData { Name = "hit_left_leg", Address = 0x1F84, };
        public static AnimationTableData ANIM_hit_right_leg { get; } = new AnimationTableData { Name = "hit_right_leg", Address = 0x2134, };
        public static AnimationTableData ANIM_death_genitalia { get; } = new AnimationTableData { Name = "death_genitalia", Address = 0x282C, };
        public static AnimationTableData ANIM_hit_neck { get; } = new AnimationTableData { Name = "hit_neck", Address = 0x299C, };
        public static AnimationTableData ANIM_death_neck { get; } = new AnimationTableData { Name = "death_neck", Address = 0x2E64, };
        public static AnimationTableData ANIM_death_stagger_back_to_wall { get; } = new AnimationTableData { Name = "death_stagger_back_to_wall", Address = 0x2F94, };
        public static AnimationTableData ANIM_death_forward_face_down { get; } = new AnimationTableData { Name = "death_forward_face_down", Address = 0x30B8, };
        public static AnimationTableData ANIM_death_forward_spin_face_up { get; } = new AnimationTableData { Name = "death_forward_spin_face_up", Address = 0x31DC, };
        public static AnimationTableData ANIM_death_backward_fall_face_up1 { get; } = new AnimationTableData { Name = "death_backward_fall_face_up1", Address = 0x32C8, };
        public static AnimationTableData ANIM_death_backward_spin_face_down_right { get; } = new AnimationTableData { Name = "death_backward_spin_face_down_right", Address = 0x33AC, };
        public static AnimationTableData ANIM_death_backward_spin_face_up_right { get; } = new AnimationTableData { Name = "death_backward_spin_face_up_right", Address = 0x34D4, };
        public static AnimationTableData ANIM_death_backward_spin_face_down_left { get; } = new AnimationTableData { Name = "death_backward_spin_face_down_left", Address = 0x35C8, };
        public static AnimationTableData ANIM_death_backward_spin_face_up_left { get; } = new AnimationTableData { Name = "death_backward_spin_face_up_left", Address = 0x36D8, };
        public static AnimationTableData ANIM_death_forward_face_down_hard { get; } = new AnimationTableData { Name = "death_forward_face_down_hard", Address = 0x384C, };
        public static AnimationTableData ANIM_death_forward_face_down_soft { get; } = new AnimationTableData { Name = "death_forward_face_down_soft", Address = 0x39C0, };
        public static AnimationTableData ANIM_death_fetal_position_right { get; } = new AnimationTableData { Name = "death_fetal_position_right", Address = 0x3AF0, };
        public static AnimationTableData ANIM_death_fetal_position_left { get; } = new AnimationTableData { Name = "death_fetal_position_left", Address = 0x3C10, };
        public static AnimationTableData ANIM_death_backward_fall_face_up2 { get; } = new AnimationTableData { Name = "death_backward_fall_face_up2", Address = 0x3D04, };
        public static AnimationTableData ANIM_side_step_left { get; } = new AnimationTableData { Name = "side_step_left", Address = 0x3D9C, };
        public static AnimationTableData ANIM_fire_roll_right2 { get; } = new AnimationTableData { Name = "fire_roll_right2", Address = 0x3FA0, };
        public static AnimationTableData ANIM_walking { get; } = new AnimationTableData { Name = "walking", Address = 0x4018, };
        public static AnimationTableData ANIM_sprinting { get; } = new AnimationTableData { Name = "sprinting", Address = 0x4070, };
        public static AnimationTableData ANIM_running { get; } = new AnimationTableData { Name = "running", Address = 0x40D4, };
        public static AnimationTableData ANIM_bond_eye_walk { get; } = new AnimationTableData { Name = "bond_eye_walk", Address = 0x4144, };
        public static AnimationTableData ANIM_bond_eye_fire { get; } = new AnimationTableData { Name = "bond_eye_fire", Address = 0x4298, };
        public static AnimationTableData ANIM_bond_watch { get; } = new AnimationTableData { Name = "bond_watch", Address = 0x42C8, };
        public static AnimationTableData ANIM_surrendering_armed { get; } = new AnimationTableData { Name = "surrendering_armed", Address = 0x4384, };
        public static AnimationTableData ANIM_surrendering_armed_drop_weapon { get; } = new AnimationTableData { Name = "surrendering_armed_drop_weapon", Address = 0x4504, };
        public static AnimationTableData ANIM_fire_walking { get; } = new AnimationTableData { Name = "fire_walking", Address = 0x4574, };
        public static AnimationTableData ANIM_fire_running { get; } = new AnimationTableData { Name = "fire_running", Address = 0x45CC, };
        public static AnimationTableData ANIM_null50 { get; } = new AnimationTableData { Name = "null50", Address = 1, };
        public static AnimationTableData ANIM_null51 { get; } = new AnimationTableData { Name = "null51", Address = 1, };
        public static AnimationTableData ANIM_fire_jump_to_side_left { get; } = new AnimationTableData { Name = "fire_jump_to_side_left", Address = 0x47BC, };
        public static AnimationTableData ANIM_fire_jump_to_side_right { get; } = new AnimationTableData { Name = "fire_jump_to_side_right", Address = 0x4A40, };
        public static AnimationTableData ANIM_hit_butt_long { get; } = new AnimationTableData { Name = "hit_butt_long", Address = 0x4CE0, };
        public static AnimationTableData ANIM_hit_butt_short { get; } = new AnimationTableData { Name = "hit_butt_short", Address = 0x4F14, };
        public static AnimationTableData ANIM_death_head { get; } = new AnimationTableData { Name = "death_head", Address = 0x51C4, };
        public static AnimationTableData ANIM_death_left_leg { get; } = new AnimationTableData { Name = "death_left_leg", Address = 0x540C, };
        public static AnimationTableData ANIM_slide_right { get; } = new AnimationTableData { Name = "slide_right", Address = 0x54A0, };
        public static AnimationTableData ANIM_slide_left { get; } = new AnimationTableData { Name = "slide_left", Address = 0x5554, };
        public static AnimationTableData ANIM_jump_backwards { get; } = new AnimationTableData { Name = "jump_backwards", Address = 0x5684, };
        public static AnimationTableData ANIM_extending_left_hand { get; } = new AnimationTableData { Name = "extending_left_hand", Address = 0x5744, };
        public static AnimationTableData ANIM_fire_throw_grenade { get; } = new AnimationTableData { Name = "fire_throw_grenade", Address = 0x5964, };
        public static AnimationTableData ANIM_spotting_bond { get; } = new AnimationTableData { Name = "spotting_bond", Address = 0x5D10, };
        public static AnimationTableData ANIM_look_around { get; } = new AnimationTableData { Name = "look_around", Address = 0x5EF0, };
        public static AnimationTableData ANIM_fire_standing_one_handed_weapon { get; } = new AnimationTableData { Name = "fire_standing_one_handed_weapon", Address = 0x60D4, };
        public static AnimationTableData ANIM_fire_standing_draw_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_standing_draw_one_handed_weapon_fast", Address = 0x6254, };
        public static AnimationTableData ANIM_fire_standing_draw_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_standing_draw_one_handed_weapon_slow", Address = 0x637C, };
        public static AnimationTableData ANIM_fire_hip_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_hip_one_handed_weapon_fast", Address = 0x6484, };
        public static AnimationTableData ANIM_fire_hip_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_hip_one_handed_weapon_slow", Address = 0x6554, };
        public static AnimationTableData ANIM_fire_hip_forward_one_handed_weapon { get; } = new AnimationTableData { Name = "fire_hip_forward_one_handed_weapon", Address = 0x6644, };
        public static AnimationTableData ANIM_fire_standing_right_one_handed_weapon { get; } = new AnimationTableData { Name = "fire_standing_right_one_handed_weapon", Address = 0x6738, };
        public static AnimationTableData ANIM_fire_step_right_one_handed_weapon { get; } = new AnimationTableData { Name = "fire_step_right_one_handed_weapon", Address = 0x6808, };
        public static AnimationTableData ANIM_fire_standing_left_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_standing_left_one_handed_weapon_slow", Address = 0x694C, };
        public static AnimationTableData ANIM_fire_standing_left_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_standing_left_one_handed_weapon_fast", Address = 0x6A18, };
        public static AnimationTableData ANIM_fire_kneel_forward_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_kneel_forward_one_handed_weapon_slow", Address = 0x6C18, };
        public static AnimationTableData ANIM_fire_kneel_forward_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_kneel_forward_one_handed_weapon_fast", Address = 0x6D50, };
        public static AnimationTableData ANIM_fire_kneel_right_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_kneel_right_one_handed_weapon_slow", Address = 0x6F08, };
        public static AnimationTableData ANIM_fire_kneel_right_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_kneel_right_one_handed_weapon_fast", Address = 0x700C, };
        public static AnimationTableData ANIM_fire_kneel_left_one_handed_weapon_slow { get; } = new AnimationTableData { Name = "fire_kneel_left_one_handed_weapon_slow", Address = 0x71D0, };
        public static AnimationTableData ANIM_fire_kneel_left_one_handed_weapon_fast { get; } = new AnimationTableData { Name = "fire_kneel_left_one_handed_weapon_fast", Address = 0x7304, };
        public static AnimationTableData ANIM_fire_kneel_left_one_handed_weapon { get; } = new AnimationTableData { Name = "fire_kneel_left_one_handed_weapon", Address = 0x7430, };
        public static AnimationTableData ANIM_aim_walking_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_walking_one_handed_weapon", Address = 0x74A4, };
        public static AnimationTableData ANIM_aim_walking_left_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_walking_left_one_handed_weapon", Address = 0x7514, };
        public static AnimationTableData ANIM_aim_walking_right_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_walking_right_one_handed_weapon", Address = 0x7588, };
        public static AnimationTableData ANIM_aim_running_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_running_one_handed_weapon", Address = 0x75EC, };
        public static AnimationTableData ANIM_aim_running_right_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_running_right_one_handed_weapon", Address = 0x7650, };
        public static AnimationTableData ANIM_aim_running_left_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_running_left_one_handed_weapon", Address = 0x76B8, };
        public static AnimationTableData ANIM_aim_sprinting_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_sprinting_one_handed_weapon", Address = 0x7714, };
        public static AnimationTableData ANIM_running_one_handed_weapon { get; } = new AnimationTableData { Name = "running_one_handed_weapon", Address = 0x777C, };
        public static AnimationTableData ANIM_sprinting_one_handed_weapon { get; } = new AnimationTableData { Name = "sprinting_one_handed_weapon", Address = 0x77D4, };
        public static AnimationTableData ANIM_null91 { get; } = new AnimationTableData { Name = "null91", Address = 1, };
        public static AnimationTableData ANIM_null92 { get; } = new AnimationTableData { Name = "null92", Address = 1, };
        public static AnimationTableData ANIM_null93 { get; } = new AnimationTableData { Name = "null93", Address = 1, };
        public static AnimationTableData ANIM_null94 { get; } = new AnimationTableData { Name = "null94", Address = 1, };
        public static AnimationTableData ANIM_null95 { get; } = new AnimationTableData { Name = "null95", Address = 1, };
        public static AnimationTableData ANIM_null96 { get; } = new AnimationTableData { Name = "null96", Address = 1, };
        public static AnimationTableData ANIM_draw_one_handed_weapon_and_look_around { get; } = new AnimationTableData { Name = "draw_one_handed_weapon_and_look_around", Address = 0x78C8, };
        public static AnimationTableData ANIM_draw_one_handed_weapon_and_stand_up { get; } = new AnimationTableData { Name = "draw_one_handed_weapon_and_stand_up", Address = 0x7AA8, };
        public static AnimationTableData ANIM_aim_one_handed_weapon_left_right { get; } = new AnimationTableData { Name = "aim_one_handed_weapon_left_right", Address = 0x7C4C, };
        public static AnimationTableData ANIM_cock_one_handed_weapon_and_turn_around { get; } = new AnimationTableData { Name = "cock_one_handed_weapon_and_turn_around", Address = 0x7D04, };
        public static AnimationTableData ANIM_holster_one_handed_weapon_and_cross_arms { get; } = new AnimationTableData { Name = "holster_one_handed_weapon_and_cross_arms", Address = 0x7DD8, };
        public static AnimationTableData ANIM_cock_one_handed_weapon_turn_around_and_stand_up { get; } = new AnimationTableData { Name = "cock_one_handed_weapon_turn_around_and_stand_up", Address = 0x7F0C, };
        public static AnimationTableData ANIM_draw_one_handed_weapon_and_turn_around { get; } = new AnimationTableData { Name = "draw_one_handed_weapon_and_turn_around", Address = 0x7FB4, };
        public static AnimationTableData ANIM_step_forward_and_hold_one_handed_weapon { get; } = new AnimationTableData { Name = "step_forward_and_hold_one_handed_weapon", Address = 0x8080, };
        public static AnimationTableData ANIM_holster_one_handed_weapon_and_adjust_suit { get; } = new AnimationTableData { Name = "holster_one_handed_weapon_and_adjust_suit", Address = 0x8164, };
        public static AnimationTableData ANIM_idle_unarmed { get; } = new AnimationTableData { Name = "idle_unarmed", Address = 0x8194, };
        public static AnimationTableData ANIM_walking_unarmed { get; } = new AnimationTableData { Name = "walking_unarmed", Address = 0x8204, };
        public static AnimationTableData ANIM_fire_walking_dual_wield { get; } = new AnimationTableData { Name = "fire_walking_dual_wield", Address = 0x8274, };
        public static AnimationTableData ANIM_fire_walking_dual_wield_hands_crossed { get; } = new AnimationTableData { Name = "fire_walking_dual_wield_hands_crossed", Address = 0x82E0, };
        public static AnimationTableData ANIM_fire_running_dual_wield { get; } = new AnimationTableData { Name = "fire_running_dual_wield", Address = 0x8340, };
        public static AnimationTableData ANIM_fire_running_dual_wield_hands_crossed { get; } = new AnimationTableData { Name = "fire_running_dual_wield_hands_crossed", Address = 0x83A4, };
        public static AnimationTableData ANIM_fire_sprinting_dual_wield { get; } = new AnimationTableData { Name = "fire_sprinting_dual_wield", Address = 0x8404, };
        public static AnimationTableData ANIM_fire_sprinting_dual_wield_hands_crossed { get; } = new AnimationTableData { Name = "fire_sprinting_dual_wield_hands_crossed", Address = 0x845C, };
        public static AnimationTableData ANIM_walking_female { get; } = new AnimationTableData { Name = "walking_female", Address = 0x84C4, };
        public static AnimationTableData ANIM_running_female { get; } = new AnimationTableData { Name = "running_female", Address = 0x8520, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield", Address = 0x8698, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield_left { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield_left", Address = 0x8800, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield_right { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield_right", Address = 0x8978, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield_hands_crossed { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield_hands_crossed", Address = 0x8AAC, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield_hands_crossed_left { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield_hands_crossed_left", Address = 0x8BF0, };
        public static AnimationTableData ANIM_fire_kneel_dual_wield_hands_crossed_right { get; } = new AnimationTableData { Name = "fire_kneel_dual_wield_hands_crossed_right", Address = 0x8D28, };
        public static AnimationTableData ANIM_fire_standing_dual_wield { get; } = new AnimationTableData { Name = "fire_standing_dual_wield", Address = 0x8E1C, };
        public static AnimationTableData ANIM_fire_standing_dual_wield_left { get; } = new AnimationTableData { Name = "fire_standing_dual_wield_left", Address = 0x8F2C, };
        public static AnimationTableData ANIM_fire_standing_dual_wield_right { get; } = new AnimationTableData { Name = "fire_standing_dual_wield_right", Address = 0x9084, };
        public static AnimationTableData ANIM_fire_standing_dual_wield_hands_crossed_left { get; } = new AnimationTableData { Name = "fire_standing_dual_wield_hands_crossed_left", Address = 0x9194, };
        public static AnimationTableData ANIM_fire_standing_dual_wield_hands_crossed_right { get; } = new AnimationTableData { Name = "fire_standing_dual_wield_hands_crossed_right", Address = 0x92EC, };
        public static AnimationTableData ANIM_fire_standing_aiming_down_sights { get; } = new AnimationTableData { Name = "fire_standing_aiming_down_sights", Address = 0x9444, };
        public static AnimationTableData ANIM_fire_kneel_aiming_down_sights { get; } = new AnimationTableData { Name = "fire_kneel_aiming_down_sights", Address = 0x95FC, };
        public static AnimationTableData ANIM_hit_taser { get; } = new AnimationTableData { Name = "hit_taser", Address = 0x97BC, };
        public static AnimationTableData ANIM_death_explosion_forward { get; } = new AnimationTableData { Name = "death_explosion_forward", Address = 0x98C8, };
        public static AnimationTableData ANIM_death_explosion_left1 { get; } = new AnimationTableData { Name = "death_explosion_left1", Address = 0x9A2C, };
        public static AnimationTableData ANIM_death_explosion_back_left { get; } = new AnimationTableData { Name = "death_explosion_back_left", Address = 0x9B48, };
        public static AnimationTableData ANIM_death_explosion_back1 { get; } = new AnimationTableData { Name = "death_explosion_back1", Address = 0x9C4C, };
        public static AnimationTableData ANIM_death_explosion_right { get; } = new AnimationTableData { Name = "death_explosion_right", Address = 0x9D5C, };
        public static AnimationTableData ANIM_death_explosion_forward_right1 { get; } = new AnimationTableData { Name = "death_explosion_forward_right1", Address = 0x9E44, };
        public static AnimationTableData ANIM_death_explosion_back2 { get; } = new AnimationTableData { Name = "death_explosion_back2", Address = 0x9F48, };
        public static AnimationTableData ANIM_death_explosion_forward_roll { get; } = new AnimationTableData { Name = "death_explosion_forward_roll", Address = 0xA094, };
        public static AnimationTableData ANIM_death_explosion_forward_face_down { get; } = new AnimationTableData { Name = "death_explosion_forward_face_down", Address = 0xA1B8, };
        public static AnimationTableData ANIM_death_explosion_left2 { get; } = new AnimationTableData { Name = "death_explosion_left2", Address = 0xA2F8, };
        public static AnimationTableData ANIM_death_explosion_forward_right2 { get; } = new AnimationTableData { Name = "death_explosion_forward_right2", Address = 0xA424, };
        public static AnimationTableData ANIM_death_explosion_forward_right2_alt { get; } = new AnimationTableData { Name = "death_explosion_forward_right2_alt", Address = 0xA538, };
        public static AnimationTableData ANIM_death_explosion_forward_right3 { get; } = new AnimationTableData { Name = "death_explosion_forward_right3", Address = 0xA650, };
        public static AnimationTableData ANIM_null143 { get; } = new AnimationTableData { Name = "null143", Address = 1, };
        public static AnimationTableData ANIM_null144 { get; } = new AnimationTableData { Name = "null144", Address = 1, };
        public static AnimationTableData ANIM_null145 { get; } = new AnimationTableData { Name = "null145", Address = 1, };
        public static AnimationTableData ANIM_null146 { get; } = new AnimationTableData { Name = "null146", Address = 1, };
        public static AnimationTableData ANIM_running_hands_up { get; } = new AnimationTableData { Name = "running_hands_up", Address = 0xA6B0, };
        public static AnimationTableData ANIM_sprinting_hands_up { get; } = new AnimationTableData { Name = "sprinting_hands_up", Address = 0xA704, };
        public static AnimationTableData ANIM_aim_and_blow_one_handed_weapon { get; } = new AnimationTableData { Name = "aim_and_blow_one_handed_weapon", Address = 0xA8BC, };
        public static AnimationTableData ANIM_aim_one_handed_weapon_left { get; } = new AnimationTableData { Name = "aim_one_handed_weapon_left", Address = 0xA94C, };
        public static AnimationTableData ANIM_aim_one_handed_weapon_right { get; } = new AnimationTableData { Name = "aim_one_handed_weapon_right", Address = 0xA9DC, };
        public static AnimationTableData ANIM_conversation { get; } = new AnimationTableData { Name = "conversation", Address = 0xACAC, };
        public static AnimationTableData ANIM_drop_weapon_and_show_fight_stance { get; } = new AnimationTableData { Name = "drop_weapon_and_show_fight_stance", Address = 0xB174, };
        public static AnimationTableData ANIM_yawning { get; } = new AnimationTableData { Name = "yawning", Address = 0xB2AC, };
        public static AnimationTableData ANIM_swatting_flies { get; } = new AnimationTableData { Name = "swatting_flies", Address = 0xB528, };
        public static AnimationTableData ANIM_scratching_leg { get; } = new AnimationTableData { Name = "scratching_leg", Address = 0xB6B0, };
        public static AnimationTableData ANIM_scratching_butt { get; } = new AnimationTableData { Name = "scratching_butt", Address = 0xB7C8, };
        public static AnimationTableData ANIM_adjusting_crotch { get; } = new AnimationTableData { Name = "adjusting_crotch", Address = 0xB854, };
        public static AnimationTableData ANIM_sneeze { get; } = new AnimationTableData { Name = "sneeze", Address = 0xB9A8, };
        public static AnimationTableData ANIM_conversation_cleaned { get; } = new AnimationTableData { Name = "conversation_cleaned", Address = 0xBC40, };
        public static AnimationTableData ANIM_conversation_listener { get; } = new AnimationTableData { Name = "conversation_listener", Address = 0xBF80, };
        public static AnimationTableData ANIM_startled_and_looking_around { get; } = new AnimationTableData { Name = "startled_and_looking_around", Address = 0xC224, };
        public static AnimationTableData ANIM_laughing_in_disbelief { get; } = new AnimationTableData { Name = "laughing_in_disbelief", Address = 0xC410, };
        public static AnimationTableData ANIM_surrendering_unarmed { get; } = new AnimationTableData { Name = "surrendering_unarmed", Address = 0xC544, };
        public static AnimationTableData ANIM_coughing_standing { get; } = new AnimationTableData { Name = "coughing_standing", Address = 0xC838, };
        public static AnimationTableData ANIM_coughing_kneel1 { get; } = new AnimationTableData { Name = "coughing_kneel1", Address = 0xCB78, };
        public static AnimationTableData ANIM_coughing_kneel2 { get; } = new AnimationTableData { Name = "coughing_kneel2", Address = 0xCE6C, };
        public static AnimationTableData ANIM_standing_up { get; } = new AnimationTableData { Name = "standing_up", Address = 0xD0A8, };
        public static AnimationTableData ANIM_null169 { get; } = new AnimationTableData { Name = "null169", Address = 1, };
        public static AnimationTableData ANIM_dancing { get; } = new AnimationTableData { Name = "dancing", Address = 0xD348, };
        public static AnimationTableData ANIM_dancing_one_handed_weapon { get; } = new AnimationTableData { Name = "dancing_one_handed_weapon", Address = 0xD54C, };
        public static AnimationTableData ANIM_keyboard_right_hand1 { get; } = new AnimationTableData { Name = "keyboard_right_hand1", Address = 0xD5E4, };
        public static AnimationTableData ANIM_keyboard_right_hand2 { get; } = new AnimationTableData { Name = "keyboard_right_hand2", Address = 0xD668, };
        public static AnimationTableData ANIM_keyboard_left_hand { get; } = new AnimationTableData { Name = "keyboard_left_hand", Address = 0xD6F8, };
        public static AnimationTableData ANIM_keyboard_right_hand_tapping { get; } = new AnimationTableData { Name = "keyboard_right_hand_tapping", Address = 0xD728, };
        public static AnimationTableData ANIM_bond_eye_fire_alt { get; } = new AnimationTableData { Name = "bond_eye_fire_alt", Address = 0xD89C, };
        public static AnimationTableData ANIM_dam_jump { get; } = new AnimationTableData { Name = "dam_jump", Address = 0xDBE4, };
        public static AnimationTableData ANIM_surface_vent_jump { get; } = new AnimationTableData { Name = "surface_vent_jump", Address = 0xDD20, };
        public static AnimationTableData ANIM_cradle_jump { get; } = new AnimationTableData { Name = "cradle_jump", Address = 0xE05C, };
        public static AnimationTableData ANIM_cradle_fall { get; } = new AnimationTableData { Name = "cradle_fall", Address = 0xE08C, };
        public static AnimationTableData ANIM_credits_bond_kissing { get; } = new AnimationTableData { Name = "credits_bond_kissing", Address = 0xE0BC, };
        public static AnimationTableData ANIM_credits_natalya_kissing { get; } = new AnimationTableData { Name = "credits_natalya_kissing", Address = 0xE18C, };

#pragma warning restore SA1516 // Elements should be separated by blank line
#pragma warning restore SA1600 // Elements should be documented

        /// <summary>
        /// Animation name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ROM address. Untranslated to address space.
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// Lookup ROM address to convert to class property.
        /// </summary>
        /// <param name="address">This is untranslated address (<see cref="ANIM_idle"/> starts at 0x1c).</param>
        /// <returns>Property or <see cref="AnimationTableData.ANIM_null50"/>.</returns>
        public static AnimationTableData AddressToAnim(int address)
        {
            BuildLookup();

            if (!_lookup.ContainsKey(address))
            {
                return AnimationTableData.ANIM_null50;
            }

            return _lookup[address];
        }

        private static void BuildLookup()
        {
            if (_init)
            {
                return;
            }

            _init = true;

            var props = typeof(AnimationTableData)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(x =>
                    x.PropertyType == typeof(AnimationTableData)
                    && x.Name.StartsWith("ANIM_"));

            foreach (var prop in props)
            {
                var instance = (AnimationTableData)prop.GetValue(null)!;

                // removed properties all have address 0x1.
                if (!_lookup.ContainsKey(instance.Address))
                {
                    _lookup.Add(instance.Address, instance);
                }
            }
        }
    }
}
