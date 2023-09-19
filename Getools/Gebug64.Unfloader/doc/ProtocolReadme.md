# Gebug Protocol

The gebug protocol expects a header, followed by relevant data, without any closing signifier.

The header always consists of at least one byte, which is the message category.

The category determines how to parse the rest of the header. If there is a command, then the category+command determine how to parse the rest of the header. The size of the command and the number and size of parameters passed are determined in advance. The source code contains the most accurate explanation of the protocol being referenced here.

-----

It is expected the ROM will have a relatively small message buffer, and that large messages will be split into multiple packets. Generally messages will only require 1 packet, so a packet count is not needed, but for large data transfer a packet number and total packet count will be required in the header.

-----

# Category

## `ACK` Category

The `ACK` category is special, in that in encapsulates another message. The first 6 bytes of the header should be treated as specific to the `ACK` category, but beginning at byte offset 6 the message can be parsed as a complete message.

### Header

The header consists of 8 bytes

* 0: unsigned byte. The current message category.
* 1-2: unsigned 16 bit int. Current packet count in this message, at least 1.
* 3-4: unsigned 16 bit int. Total number of packets in this message, at least 1.
* 5: unused. Set to the value of `0xaa`.
* 6: unsigned byte. The replied to message category.
* 7: unsigned byte. The replied to message command.

### Body

Message body depends on the replied to message category + replied to message command.
