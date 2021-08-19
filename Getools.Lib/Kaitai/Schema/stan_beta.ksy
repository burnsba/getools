meta:
  id: stan_beta
  file-extension: bin
  endian: be
seq:
  - id: header_block
    type: stand_file_header
  - id: tiles
    type: beta_stand_tile
    repeat: until
    repeat-until: _.debug_name.offset == 0
  - id: footer
    type: stand_tile_footer
types:
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
  stand_file_header:
    seq:
      - id: unknown1
        type: u4
      - id: tile_offset
        type: u4
      - id: unknown_header_data
        size: (tile_offset - _io.pos)
  beta_stand_tile_point:
    seq:
      - id: x
        type: f4
      - id: y
        type: f4
      - id: z
        type: f4
      - id: link
        type: u4
  beta_stand_tile_tail:
    seq:
      - id: point_count
        type: u1
      - id: first_point
        type: u1
      - id: second_point
        type: u1
      - id: third_point
        type: u1
      - id: points
        type: beta_stand_tile_point
        repeat: expr
        repeat-expr: point_count
  beta_stand_tile:
    seq:
      - id: debug_name
        type: string_pointer
      - id: flags 
        type: b4
      - id: red
        type: b4
      - id: green
        type: b4
      - id: blue
        type: b4
      - id: beta_unknown
        type: u2
      - id: tail
        type: beta_stand_tile_tail
        if: debug_name.offset > 0
  stand_tile_footer:
    seq:
      - id: unstric
        type: strz
        encoding: ASCII
        terminator: 0
      - id: string_pad
        size: (4 - _io.pos) % 4
      - id: unknown3
        type: u4
      - id: unknown4
        type: u4
      - id: unknown5
        type: u4
      - id: unknown_remaining
        size-eos: true