meta:
  id: ramrom_replay
  file-extension: bin
  endian: be
doc: struct ramromfilestructure
seq:
  - id: random_seed
    type: u8
  - id: randomizer
    type: u8
  - id: stage_num
    doc: enum LEVELID
    type: u4
  - id: difficulty
    doc: enum DIFFICULTY
    type: u4
  - id: size_cmds
    type: u4
  - id: save_file
    type: save_data
  - id: padding
    type: u2
  - id: totaltime_ms
    type: s4
  - id: file_size
    type: s4
  - id: mode
    doc: enum GAMEMODE
    type: u4
  - id: slot_num
    type: u4
  - id: num_players
    type: u4
  - id: scenario
    type: u4
  - id: mpstage_sel
    type: u4
  - id: game_length
    type: u4
  - id: mp_weapon_set
    type: u4
  - id: mp_char
    type: s4
    repeat: expr
    repeat-expr: 4
  - id: mp_handi
    type: s4
    repeat: expr
    repeat-expr: 4
  - id: mp_contstyle
    type: s4
    repeat: expr
    repeat-expr: 4
  - id: aim_option
    type: u4
  - id: mp_flags
    type: s4
    repeat: expr
    repeat-expr: 4
  - id: padding2
    type: u4
  - id: seq_data
    type: ramrom_iter(size_cmds)
    repeat: eos
types:
  save_data:
    doc: struct save_data
    seq:
      - id: chksum1
        type: u4
      - id: chksum2
        type: u4
      - id: completion_bitflags
        type: u1
      - id: flag_007
        type: u1
      - id: music_vol
        type: u1
      - id: sfx_vol
        type: u1
      - id: options
        type: u2
      - id: unlocked_cheats_1
        type: u1
      - id: unlocked_cheats_2
        type: u1
      - id: unlocked_cheats_3
        type: u1
      - id: unused
        type: u1
      - id: times
        type: u1
        repeat: expr
        repeat-expr: 19*4
  ramrom_seed:
    seq:
      - id: speed_frames
        type: u1
      - id: count
        type: u1
      - id: randseed
        type: u1
      - id: check
        type: u1
  ramrom_blockbuf:
    seq:
      - id: stick_x
        type: s1
      - id: stick_y
        type: s1
      - id: button_low
        type: u1
      - id: button_high
        type: u1
  ramrom_iter:
    params:
      - id: size_cmds
        type: u4
    seq:
      - id: head
        type: ramrom_seed
      - id: data
        type: ramrom_blockbuf
        repeat: expr
        repeat-expr: head.count * size_cmds