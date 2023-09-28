meta:
  id: g
  file-extension: g
  endian: be
seq:
  - id: category
    type: u1
  - id: command
    type: u1
  - id: flags
    type: u2
  - id: packet_size
    type: u2
  - id: num_parameters
    type: u1
  - id: reserved
    type: u1
  - id: group_id
    type: u4
  - id: respond_to_group_id
    type: u4
  - id: packet_number
    if: flags & 0x1 == 0x1
    type: u2
  - id: total_packets
    if: flags & 0x1 == 0x1
    type: u2
  - id: parameters
    type: gparameter
    repeat: expr
    repeat-expr: num_parameters
types:
  gparameter:
    seq:
      - id: size
        type: u1
      - id: data1
        if: size == 1
        type: u1
      - id: data2
        if: size == 2
        type: u2
      - id: data4
        if: size == 4
        type: u4
      - id: var_size2
        if: size == 0xfe
        type: u2
      - id: var_data2
        if: size == 0xfe
        size: var_size2
      - id: var_size4
        if: size == 0xfd
        type: u4
      - id: var_data4
        if: size == 0xfd
        size: var_size4