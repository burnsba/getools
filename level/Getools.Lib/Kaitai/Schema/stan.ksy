meta:
  id: stan
  file-extension: bin
  endian: be
seq:
  - id: header_block
    type: stand_file_header
  - id: tiles
    type: stand_tile
    repeat: until
    repeat-until: _.internal_name == 0 and _.point_count == 0
  - id: footer
    type: stand_tile_footer
types:
  stand_file_header:
    seq:
      - id: unknown1
        type: u4
      - id: tile_offset
        type: u4
      - id: unknown_header_data
        size: (tile_offset - _io.pos)
  stand_tile_point:
    seq:
      - id: x
        type: s2
      - id: y
        type: s2
      - id: z
        type: s2
      - id: link
        type: u2
  stand_tile:
    seq:
      - id: internal_name
        type: b24
      - id: room
        type: u1
      - id: flags
        type: b4
      - id: red
        type: b4
      - id: green
        type: b4
      - id: blue
        type: b4
      - id: point_count
        type: b4
      - id: first_point
        type: b4
      - id: second_point
        type: b4
      - id: third_point
        type: b4
      - id: points
        type: stand_tile_point
        repeat: expr
        repeat-expr: point_count
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