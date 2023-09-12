meta:
  id: bg
  file-extension: bin
  endian: be

seq:
  - id: header_block
    type: bg_file_header
types:
  bg_file_header:
    seq:
      - id: unknown1
        type: u4
      - id: room_data_table_pointer
        type: u4
      - id: portal_data_table_pointer
        type: u4
      - id: global_visibility_commands_pointer
        type: u4
      - id: unknown2
        type: u4
    instances:
      room_data_table_ignore:
        io: _root._io
        pos: room_data_table_pointer & 0xffffff
        type: bg_file_room_data_entry
        repeat: until
        repeat-until: _.point_table_pointer == 0
      room_data_table:
        io: _root._io
        pos: (room_data_table_pointer & 0xffffff) + 24
        type: bg_file_room_data_entry
        repeat: until
        repeat-until: _.point_table_pointer == 0 and _.primary_display_list_pointer == 0
      global_visibility_commands:
        io: _root._io
        pos: global_visibility_commands_pointer & 0xffffff
        type: visibility_command
        repeat: until
        repeat-until: _root._io.pos == (portal_data_table_pointer & 0xffffff)
      portal_data_table:
        io: _root._io
        pos: portal_data_table_pointer & 0xffffff
        type: bg_file_portal_data_entry
        repeat: until
        repeat-until: _.portal_pointer == 0
  bg_file_room_data_entry:
    seq:
      - id: point_table_pointer
        type: u4
      - id: primary_display_list_pointer
        type: u4
      - id: secondary_display_list_pointer
        type: u4
      - id: coord
        type: coord3d
  bg_file_portal_data_entry:
    seq:
      - id: portal_pointer
        type: u4
      - id: connected_room_1
        type: u1
      - id: connected_room_2
        type: u1
      - id: control_flags
        type: u2
    instances:
      portal:
        io: _root._io
        pos: portal_pointer & 0xffffff
        type: bg_file_portal
  bg_file_portal:
    seq:
      - id: number_points
        type: u1
      - id: padding
        type: u1
        repeat: expr
        repeat-expr: 3
      - id: points
        type: coord3d
        repeat: expr
        repeat-expr: number_points
  visibility_command:
    seq:
      - id: command
        type: u4
  coord3d:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
