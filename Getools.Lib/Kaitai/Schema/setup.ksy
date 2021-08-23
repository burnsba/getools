meta:
  id: setup
  file-extension: bin
  endian: be
  
seq:
  - id: pointers
    type: stage_setup_struct
  - id: contents
    type: section_block
    repeat: eos

enums:
  section_id:
    0: pad_section
    10: pad3d_section
    20: object_section
    30: intro_section
    38: sets_prequel
    40: path_links_section 
    50: pad3d_names
    60: path_table_section
    70: path_sets_section
    80: pad_names
    90: ai_list_section
  propdef:
    0: nothing
    0x01: door
    0x03: standard
    0x04: key
    0x05: alarm
    0x06: cctv
    0x07: ammo_mag
    0x08: weapon
    0x09: guard
    0x0a: single_monitor
    0x0b: multi_monitor
    0x0c: hanging_monitor
    0x0d: autogun
    0x11: hat
    0x12: set_guard_attribute
    0x14: ammo_box
    0x15: body_armor
    0x16: tag
    0x17: objective_start
    0x18: end_objective
    0x19: destroy_object
    0x1a: objective_complete_condition
    0x1b: objective_fail_condition
    0x1c: collect_object
    0x1e: objective_photograph_item
    0x20: objective_enter_room
    0x22: objective_copy_item
    0x23: watch_menu_objective_text
    0x25: rename
    0x26: lock
    0x27: vehicle
    0x28: aircraft
    0x2a: glass
    0x2b: safe
    0x2c: safe_item
    0x2d: tank
    0x2e: cutscene
    0x2f: glass_tinted
    0x30: end_props
  introdef:
    0x0: spawn
    0x1: start_weapon
    0x2: start_ammo
    0x3: swirl_cam
    0x4: intro_cam
    0x5: cuff
    0x6: fixed_cam
    0x7: watch_time
    0x8: credits
    0x9: end_intro
types:
  ff_list_item:
    seq:
      - id: value
        type: u4
  string_pointer:
    seq:
      - id: offset
        type: u4
    instances:
      deref:
        io: _root._io
        type: strz
        pos: offset
        encoding: ASCII
  coord3d:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
  bbox:
    seq:
      - id: xmin
        type: f4
      - id: xmax
        type: f4
      - id: ymin
        type: f4
      - id: ymax
        type: f4
      - id: zmin
        type: f4
      - id: zmax
        type: f4
  pad:
    seq:
      - id: pos
        type: coord3d
      - id: up
        type: coord3d
      - id: look
        type: coord3d
      - id: plink
        type: string_pointer
      - id: unknown
        type: u4
  pad3d:
    seq:
      - id: pos
        type: coord3d
      - id: up
        type: coord3d
      - id: look
        type: coord3d
      - id: plink
        type: string_pointer
      - id: unknown
        type: u4
      - id: bbox
        type: bbox
  path_link_entry:
    seq:
      - id: pad_neighbor_offset
        type: u4
      - id: pad_index_offset
        type: u4
      - id: empty
        type: u4
    instances:
      pad_neighbor_ids:
        if: pad_neighbor_offset > 0
        io: _root._io
        pos: pad_neighbor_offset
        type: ff_list_item
        repeat: until
        repeat-until: _.value == 0xffffffff
      pad_index_ids:
        if: pad_index_offset > 0
        io: _root._io
        pos: pad_index_offset
        type: ff_list_item
        repeat: until
        repeat-until: _.value == 0xffffffff
  path_table_entry:
    seq:
      - id: unknown_00
        type: u2
      - id: unknown_02
        type: u2
      - id: unknown_pointer
        type: u4
      - id: unknown_08
        type: u4
      - id: unknown_0c
        type: u4
    instances:
      data:
        if: unknown_pointer > 0
        io: _root._io
        pos: unknown_pointer
        type: ff_list_item
        repeat: until
        repeat-until: _.value == 0xffffffff
  path_set_entry:
    seq:
      - id: pointer
        type: u4
      - id: unknown_04
        type: u4
    instances:
      data:
        if: pointer > 0
        io: _root._io
        pos: pointer
        type: ff_list_item
        repeat: until
        repeat-until: _.value == 0xffffffff
  setup_ai_script:
    seq:
      - id: pointer
        type: u4
      - id: id
        type: u4
  stage_setup_struct:
    seq:
      - id: path_tables_offset
        type: u4
      - id: path_links_offset
        type: u4
      - id: intros_offset
        type: u4
      - id: object_list_offset
        type: u4
      - id: path_sets_offset
        type: u4
      - id: ai_list_offset
        type: u4
      - id: pad_list_offset
        type: u4
      - id: pad3d_list_offset
        type: u4
      - id: pad_names_offset
        type: u4
      - id: pad3d_names_offset
        type: u4
    
  object_header_data:
    seq:
      - id: extra_scale
        type: u2
      - id: state
        type: u1
      - id: type
        type: u1
        enum: propdef
  setup_generic_object:
    seq:
      - id: object_id
        type: u2
      - id: preset
        type: u2
      - id: flags_1
        type: u4
      - id: flags_2
        type: u4
      - id: pointer_position_data
        type: u4
      - id: pointer_obj_instance_controller
        type: u4
      - id: unknown_18
        type: u4
      - id: unknown_1c
        type: u4
      - id: unknown_20
        type: u4
      - id: unknown_24
        type: u4
      - id: unknown_28
        type: u4
      - id: unknown_2c
        type: u4
      - id: unknown_30
        type: u4
      - id: unknown_34
        type: u4
      - id: unknown_38
        type: u4
      - id: unknown_3c
        type: u4
      - id: unknown_40
        type: u4
      - id: unknown_44
        type: u4
      - id: unknown_48
        type: u4
      - id: unknown_4c
        type: u4
      - id: unknown_50
        type: u4
      - id: unknown_54
        type: u4
      - id: xpos
        type: u4
      - id: ypos
        type: u4
      - id: zpos
        type: u4
      - id: bitflags
        type: u4
      - id: pointer_collision_block
        type: u4
      - id: unknown_6c
        type: u4
      - id: unknown_70
        type: u4
      - id: health
        type: u2
      - id: max_health
        type: u2
      - id: unknown_78
        type: u4
      - id: unknown_7c
        type: u4
  # type = 0x01
  setup_object_door_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: linked_door_offset
        type: u4
      - id: max_frac
        type: u4
      - id: perim_frac
        type: u4
      - id: accel
        type: u4
      - id: decel
        type: u4
      - id: max_speed
        type: u4
      - id: door_flags
        type: u2
      - id: door_type
        type: u2
      - id: key_flags
        type: u4
      - id: auto_close_frames
        type: u4
      - id: door_open_sound
        type: u4
      - id: frac
        type: u4
      - id: unknown_ac
        type: u4
      - id: unknown_b0
        type: u4
      - id: open_position
        type: f4
      - id: speed
        type: f4
      - id: state
        type: u1
      - id: unknown_bd
        type: u1
      - id: unknown_be
        type: u2
      - id: unknown_c0
        type: u4
      - id: unknown_c4
        type: u2
      - id: sound_type
        type: u1
      - id: fade_time_60
        type: u1
      - id: linked_door_pointer
        type: u4
      - id: laser_fade
        type: u1
      - id: unknown_cd
        type: u1
      - id: unknown_ce
        type: u2
      - id: unknown_d0
        type: u4
      - id: unknown_d4
        type: u4
      - id: unknown_d8
        type: u4
      - id: unknown_dc
        type: u4
      - id: unknown_e0
        type: u4
      - id: unknown_e4
        type: u4
      - id: unknown_e8
        type: u4
      - id: opened_time
        type: u4
      - id: portal_number
        type: u4
      - id: unknown_f4_pointer
        type: u4
      - id: unknown_f8
        type: u4
      - id: timer
        type: u4
  # type = 0x03
  setup_object_standard_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x04
  setup_object_key_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: key
        type: u4
  # type = 0x05
  setup_object_alarm_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x06
  setup_object_cctv_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        size: 108
  # type = 0x07
  setup_object_ammo_mag_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: ammo_type
        type: s4
  # type = 0x08
  setup_object_weapon_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: gun_pickup
        type: u1
      - id: linked_item
        type: u1
      - id: timer
        type: u2
      - id: pointer_linked_item
        type: u4
  # type = 0x09
  setup_object_guard_body:
    seq:
      - id: object_id
        type: u2
      - id: preset
        type: u2
      - id: body_id
        type: u2
      - id: action_path_assignment
        type: u2
      - id: preset_to_trigger
        type: u4
      - id: unknown10
        type: u2
      - id: health
        type: u2
      - id: reaction_time
        type: u2
      - id: head
        type: u2
      - id: pointer_runtime_data
        type: u4
  # type = 0x0a
  setup_object_single_monitor_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: cur_num_cmds_from_start_rotation
        type: u4
      - id: loop_counter
        type: u4
      - id: imgnum_or_ptrheader
        type: u4
      - id: rotation
        type: u4
      - id: cur_hzoom
        type: u4
      - id: cur_hzoom_time
        type: u4
      - id: final_hzoom_time
        type: u4
      - id: initial_hzoom
        type: u4
      - id: final_hzoom
        type: u4
      - id: cur_vzoom
        type: u4
      - id: cur_vzoom_time
        type: u4
      - id: final_vzoom_time
        type: u4
      - id: initial_vzoom
        type: u4
      - id: final_vzoom
        type: u4
      - id: cur_hpos
        type: u4
      - id: cur_hscroll_time
        type: u4
      - id: final_hscroll_time
        type: u4
      - id: initial_hpos
        type: u4
      - id: final_hpos
        type: u4
      - id: cur_vpos
        type: u4
      - id: cur_vscroll_time
        type: u4
      - id: final_vscroll_time
        type: u4
      - id: initial_vpos
        type: u4
      - id: final_vpos
        type: u4
      - id: cur_red
        type: u1
      - id: initial_red
        type: u1
      - id: final_red
        type: u1
      - id: cur_green
        type: u1
      - id: initial_green
        type: u1
      - id: final_green
        type: u1
      - id: cur_blue
        type: u1
      - id: initial_blue
        type: u1
      - id: final_blue
        type: u1
      - id: cur_alpha
        type: u1
      - id: initial_alpha
        type: u1
      - id: final_alpha
        type: u1
      - id: cur_color_transition_time
        type: u4
      - id: final_color_transition_time
        type: u4
      - id: backward_mon_link
        type: u4
      - id: forward_mon_link
        type: u4
      - id: animation_num
        type: u4
  # type = 0x0b
  setup_object_multi_monitor_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        size: 468
  # type = 0x0c
  setup_object_hanging_monitor_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x0d
  setup_object_autogun_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        # size: 104 # original = 104
        size: 88
      #- id: bytes
      #  size: 48
  # type = 0x11
  setup_object_hat_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x12
  setup_object_set_guard_attribute_body:
    seq:
      - id: guard_id
        type: u4
      - id: attribute
        type: u4
  # type = 0x14
  setup_object_ammo_box_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: unused_00
        type: u2
      - id: ammo_9mm
        type: s2
      - id: unused_04
        type: u2
      - id: ammo_9mm_2
        type: s2
      - id: unused_08
        type: u2
      - id: ammo_rifle
        type: s2
      - id: unused_0c
        type: u2
      - id: ammo_shotgun
        type: s2
      - id: unused_10
        type: u2
      - id: ammo_hgrenade
        type: s2
      - id: unused_14
        type: u2
      - id: ammo_rockets
        type: s2
      - id: unused_18
        type: u2
      - id: ammo_remote_mine
        type: s2
      - id: unused_1c
        type: u2
      - id: ammo_proximity_mine
        type: s2
      - id: unused_20
        type: u2
      - id: ammo_timed_mine
        type: s2
      - id: unused_24
        type: u2
      - id: ammo_throwing
        type: s2
      - id: unused_28
        type: u2
      - id: ammo_grenade_launcher
        type: s2
      - id: unused_2c
        type: u2
      - id: ammo_magnum
        type: s2
      - id: unused_30
        type: u2
      - id: ammo_golden
        type: s2
  # type = 0x15
  setup_object_body_armor_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: armor_strength
        type: s4
      - id: armor_percent
        type: s4
  # type = 0x16
  setup_object_tag_body:
    seq:
      - id: tag_id
        type: u2
      - id: value
        type: u2
      - id: unknown_08
        type: u4
      - id: unknown_0c
        type: u4
  # type = 0x17
  setup_object_mission_objective_body:
    seq:
      - id: objective_number
        type: u4
      - id: text_id
        type: u4
      - id: min_difficulty
        type: u4
  # type = 0x18
  setup_object_end_objective_body:
    seq:
      - id: no_value
        size: 0
  # type = 0x1c
  setup_object_collect_object_body:
    seq:
      - id: object_id
        type: u4
  # type = 0x19
  setup_object_destroy_object_body:
    seq:
      - id: object_id
        type: u4
  # type = 0x1a
  setup_object_objective_complete_condition_body:
    seq:
      - id: testval
        type: u4
  # type = 0x1b
  setup_object_objective_fail_condition_body:
    seq:
      - id: testval
        type: u4
  # type = 0x1e
  setup_objective_photograph_item_body:
    seq:
      - id: object_tag_id
        type: u4
      - id: unknown_04
        type: u4
      - id: unknown_08
        type: u4
  # type = 0x20
  setup_objective_enter_room_body:
    seq:
      - id: room
        type: s4
      - id: unknown_04
        type: s4
      - id: unknown_08
        type: s4
  # type = 0x22
  setup_objective_copy_item_body:
    seq:
      - id: object_tag_id
        type: u4
      - id: unknown_04
        type: u2
      - id: unknown_06
        type: u2
  # type = 0x23
  setup_object_watch_menu_objective_body:
    seq:
      - id: menu_option
        type: u4
      - id: text_id
        type: u4
      - id: end
        type: u4
  # type = 0x25
  setup_object_rename_body:
    seq:
      - id: object_offset
        type: u4
      - id: inventory_id
        type: u4
      - id: text1
        type: u4
      - id: text2
        type: u4
      - id: text3
        type: u4
      - id: text4
        type: u4
      - id: text5
        type: u4
      - id: unknown_20
        type: u4
      - id: unknown_24
        type: u4
  # type = 0x26
  setup_object_lock_body:
    seq:
      - id: door
        type: s4
      - id: lock
        type: s4
      - id: empty
        type: s4
  # type = 0x27
  setup_object_vehicle_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        size: 48
  # type = 0x28
  setup_object_aircraft_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        size: 52
  # type = 0x2a
  setup_object_glass_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x2b
  setup_object_safe_body:
    seq:
      - id: object_base
        type: setup_generic_object
  # type = 0x2c
  setup_object_safe_item_body:
    seq:
      - id: item
        type: s4
      - id: safe
        type: s4
      - id: door
        type: s4
      - id: empty
        type: u4
  # type = 0x2d
  setup_object_tank_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: bytes
        size: 96
  # type = 0x2e
  setup_object_cutscene_body:
    seq:
      - id: xcoord
        type: u4
      - id: ycoord
        type: u4
      - id: zcoord
        type: u4
      - id: lat_rot
        type: u4
      - id: vert_rot
        type: u4
      - id: illum_preset
        type: u4
  # type = 0x2f
  setup_object_glass_tinted_body:
    seq:
      - id: object_base
        type: setup_generic_object
      - id: unknown_04
        type: s4
      - id: unknown_08
        type: s4
      - id: unknown_0c
        type: s4
      - id: unknown_10
        type: s4
      - id: unknown_14
        type: s4
  # type = 0x30
  setup_object_end_props:
    seq:
      - id: no_value
        size: 0
  not_supported:
    params:
      - id: type
        type: u1
        enum: propdef
    seq:
      #- id: end
      #  type: u1
      #  enum: propdef
      #  valid:
      #    eq: type
      - id: pos
        type: u4
        valid:
          eq: _io.pos
  setup_object_record:
    seq:
      - id: header
        type: object_header_data
      - id: body
        type:
          switch-on: header.type
          cases:
            'propdef::door': setup_object_door_body
            'propdef::standard': setup_object_standard_body
            'propdef::key': setup_object_key_body
            'propdef::alarm': setup_object_alarm_body
            'propdef::cctv': setup_object_cctv_body
            'propdef::ammo_mag': setup_object_ammo_mag_body
            'propdef::weapon': setup_object_weapon_body
            'propdef::guard': setup_object_guard_body
            'propdef::single_monitor': setup_object_single_monitor_body
            'propdef::multi_monitor': setup_object_multi_monitor_body
            'propdef::hanging_monitor': setup_object_hanging_monitor_body
            'propdef::autogun': setup_object_autogun_body
            'propdef::hat': setup_object_hat_body
            'propdef::set_guard_attribute': setup_object_set_guard_attribute_body
            'propdef::ammo_box': setup_object_ammo_box_body
            'propdef::body_armor': setup_object_body_armor_body
            'propdef::tag': setup_object_tag_body
            'propdef::objective_start': setup_object_mission_objective_body
            'propdef::end_objective': setup_object_end_objective_body
            'propdef::destroy_object': setup_object_destroy_object_body
            'propdef::objective_complete_condition': setup_object_objective_complete_condition_body
            'propdef::objective_fail_condition': setup_object_objective_fail_condition_body
            'propdef::objective_photograph_item': setup_objective_photograph_item_body
            'propdef::objective_enter_room': setup_objective_enter_room_body
            'propdef::objective_copy_item': setup_objective_copy_item_body
            'propdef::collect_object': setup_object_collect_object_body
            'propdef::watch_menu_objective_text': setup_object_watch_menu_objective_body
            'propdef::rename': setup_object_rename_body
            'propdef::lock': setup_object_lock_body
            'propdef::vehicle': setup_object_vehicle_body
            'propdef::aircraft': setup_object_aircraft_body
            'propdef::glass': setup_object_glass_body
            'propdef::safe': setup_object_safe_body
            'propdef::safe_item': setup_object_safe_item_body
            'propdef::tank': setup_object_tank_body
            'propdef::cutscene': setup_object_cutscene_body
            'propdef::glass_tinted': setup_object_glass_tinted_body
            'propdef::end_props': setup_object_end_props
            _ : not_supported(header.type)
  setup_intro_header_data:
    seq:
      - id: extra_scale
        type: u2
      - id: state
        type: u1
      - id: type
        type: u1
        enum: introdef
  # type = 0x0
  setup_intro_spawn_body:
    seq:
      - id: unknown_00
        type: u4
      - id: unknown_04
        type: u4
  # type = 0x1
  setup_intro_start_weapon_body:
    seq:
      - id: right
        type: s4
      - id: left
        type: s4
      - id: set_num
        type: u4
  # type = 0x2
  setup_intro_start_ammo_body:
    seq:
      - id: ammo_type
        type: u4
      - id: quantity
        type: u4
      - id: set
        type: u4
  # type = 0x3
  setup_intro_swirl_cam_body:
    seq:
      - id: unknown_00
        type: u4
      - id: x
        type: u4
      - id: y
        type: u4
      - id: z
        type: u4
      - id: spline_scale
        type: u4
      - id: duration
        type: u4
      - id: flags
        type: u4
  # type = 0x4
  setup_intro_intro_cam_body:
    seq:
      - id: animation
        type: u4
  # type = 0x5
  setup_intro_cuff_body:
    seq:
      - id: cuff_id
        type: u4
  # type = 0x6
  setup_intro_fixed_cam_body:
    seq:
      - id: x
        type: u4
      - id: y
        type: u4
      - id: z
        type: u4
      - id: lat_rot
        type: u4
      - id: vert_rot
        type: u4
      - id: preset
        type: u4
      - id: text_id
        type: u4
      - id: text2_id
        type: u4
      - id: unknown_20
        type: u4
  # type = 0x7
  setup_intro_watch_time_body:
    seq:
      - id: hour
        type: u4
      - id: minute
        type: u4
  # type = 0x8
  setup_intro_credits_body:
    seq:
      - id: no_value
        size: 0
  # type = 0x9
  setup_intro_end_intro_body:
    seq:
      - id: no_value
        size: 0
  setup_intro_record:
    seq:
      - id: header
        type: setup_intro_header_data
      - id: body
        type:
          switch-on: header.type
          cases:
            'introdef::spawn': setup_intro_spawn_body
            'introdef::start_weapon': setup_intro_start_weapon_body
            'introdef::start_ammo': setup_intro_start_ammo_body
            'introdef::swirl_cam': setup_intro_swirl_cam_body
            'introdef::intro_cam': setup_intro_intro_cam_body
            'introdef::cuff': setup_intro_cuff_body
            'introdef::fixed_cam': setup_intro_fixed_cam_body
            'introdef::watch_time': setup_intro_watch_time_body
            'introdef::credits': setup_intro_credits_body
            'introdef::end_intro': setup_intro_end_intro_body
  pad_list:
    seq:
      - id: data
        type: pad
        repeat: until
        repeat-until: _.plink.offset == 0
    instances:
      type:
        value: section_id::pad_section
  pad3d_list:
    seq:
      - id: data
        type: pad3d
        repeat: until
        repeat-until: _.plink.offset == 0
    instances:
      type:
        value: section_id::pad3d_section
  object_list:
    seq:
      - id: data
        type: setup_object_record
        repeat: until
        repeat-until: _.header.type == propdef::end_props
    instances:
      type:
        value: section_id::object_section
  intro_list:
    seq:
      - id: data
        type: setup_intro_record
        repeat: until
        repeat-until: _.header.type == introdef::end_intro
    instances:
      type:
        value: section_id::intro_section
  path_links_section:
    seq:
      - id: data
        type: path_link_entry
        repeat: until
        repeat-until: _.pad_neighbor_offset == 0
    instances:
      type:
        value: section_id::path_links_section
  pad3d_names_list:
    seq:
      - id: data
        type: string_pointer
        repeat: until
        repeat-until: _.offset == 0
    instances:
      type:
        value: section_id::pad3d_names
  path_table_section:
    seq:
      - id: data
        type: path_table_entry
        repeat: until
        repeat-until: _.unknown_00 == 0xffff and _.unknown_02 == 0xffff
    instances:
      type:
        value: section_id::path_table_section
  pad_names_list:
    seq:
      - id: data
        type: string_pointer
        repeat: until
        repeat-until: _.offset == 0
    instances:
      type:
        value: section_id::pad_names
  path_sets_section:
    seq:
      - id: data
        type: path_set_entry
        repeat: until
        repeat-until: _.pointer == 0
    instances:
      type:
        value: section_id::path_sets_section
  ai_list:
    seq:
      - id: data
        type: setup_ai_script
        repeat: until
        repeat-until: _.pointer == 0
    instances:
      type:
        value: section_id::ai_list_section
  filler_block:
    params:
      - id: start_pos
        type: u4
    seq:
      - id: data
        size: 1
        repeat: until
        repeat-until: >-
          _root._io.pos == _root._io.size or
          _root._io.pos == _root.pointers.path_tables_offset or
          _root._io.pos == _root.pointers.path_links_offset or
          _root._io.pos == _root.pointers.intros_offset or
          _root._io.pos == _root.pointers.object_list_offset or
          _root._io.pos == _root.pointers.path_sets_offset or
          _root._io.pos == _root.pointers.ai_list_offset or
          _root._io.pos == _root.pointers.pad_list_offset or
          _root._io.pos == _root.pointers.pad3d_list_offset or
          _root._io.pos == _root.pointers.pad_names_offset or
          _root._io.pos == _root.pointers.pad3d_names_offset
    instances:
      len:
        value: data.size
  section_block:
    seq:
      - id: body
        type:
          switch-on: _root._io.pos
          cases:
            _root.pointers.pad_list_offset: pad_list
            _root.pointers.pad3d_list_offset: pad3d_list
            _root.pointers.object_list_offset: object_list
            _root.pointers.intros_offset: intro_list
            _root.pointers.path_links_offset: path_links_section
            _root.pointers.pad3d_names_offset: pad3d_names_list
            _root.pointers.path_tables_offset: path_table_section
            _root.pointers.pad_names_offset: pad_names_list
            _root.pointers.path_sets_offset: path_sets_section
            _root.pointers.ai_list_offset: ai_list
            _: filler_block(_root._io.pos)
