# Gebug Protocol

The gebug protocol expects a header, followed by relevant data, without any closing signifier.

The header always consists of at least one byte, which is the message category.

The category determines how to parse the rest of the header. If there is a command, then the category+command determine how to parse the rest of the header. The size of the command and the number and size of parameters passed are determined in advance. The source code contains the most accurate explanation of the protocol being referenced here.

When a message is received, the PC or console will perform the action described by the command. If required, an `ACK` message will be created to reply. Not all commands will generate an `ACK` response.

-----

It is expected the ROM will have a relatively small message buffer, and that large messages will be split into multiple packets. Generally messages will only require 1 packet, so a packet count is not needed, but for large data transfer a packet number and total packet count will be required in the header.

-----

# Category

The message categories are defined as follows

```
public enum GebugMessageCategory
{
    DefaultUnknown = 0,

    Ack = 1,
    Ramrom = 10,
    Replay = 15,
    SaveState = 20,
    Debug = 25,
    Objectives = 30,
    Cheat = 35,
    Memory = 40,
    Sound = 45,
    Fog = 50,
    Stage = 55,
    Bond = 60,
    Chr = 65,
    Objects = 70,
    File = 75,
    Vi = 80,
    Meta = 90,
    Misc = 95,
}
```

## `ACK` Category

The `ACK` category is special, in that it encapsulates another message. The first 6 bytes of the header should be treated as specific to the `ACK` category, but beginning at byte offset 6 the message can be parsed as a complete message.

The `ACK` message is used to echo back a category+command that was just received and supply values.

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

* 8+: optional. Contains the "ack parameter" response data.

## `Debug` Category

The `Debug` category is for executing debug methods still in the retail version of the game.

`Debug` commands are as follows

```
public enum GebugCmdDebug
{
    DefaultUnknown = 0,

    ShowDebugMenu = 1,
    DebugMenuProcessor = 99,
}
```

### `Debug ShowDebugMenu` Command

Sets `g_BossIsDebugMenuOpen` to the value of the parameter. Note that in order for the debug menu to be interactive, you must be holding C UP and C DOWN when the value is changed.

**Parameters**: 1 byte value.  
**Console reply**: no.  
**`ACK` Parameters**: N/A.

### `Debug DebugMenuProcessor` Command

Calls `debug_menu_case_processer` with the supplied value. Note: This method was added to the gebug ROM. This is the switch statement found in `debug_menu_processor`, but extracted to its own method. The parameter supplied is the value passed to the switch statement.

**Parameters**: 1 byte value.  
**Console reply**: no.  
**`ACK` Parameters**: N/A.

## `Cheat` Category

The `Cheat` category is for native cheat methods. This includes toggling runtime cheats, as well as all of the unlock methods, which are programmed as "cheats".

`Cheat` commands are as follows

```
public enum GebugCmdCheat
{
    DefaultUnknown = 0,

    SetCheatStatus = 10,
    DisableAll = 12,
}
```

### `Cheat SetCheatStatus` Command

Turns a cheat on or off. Note that turning off an "unlock" cheat does nothing.

**Parameter 0**: 1 byte boolean, 0 "turns off" cheat, a non zero value enables the cheat.  
**Parameter 1**: 1 byte cheat id. ROM enum type is `CHEAT_ID`, the C# equivalent is `CheatIdX` .  
**Console reply**: no.  
**`ACK` Parameters**: N/A.  

### `Cheat DisableAll` Command

Disables all runtime cheats. This is a meta method, added to the rom to disable all cheats.

**Parameters**: no.  
**Console reply**: no.  
**`ACK` Parameters**: N/A.

## `Stage` Category

The `Stage` category is for changing or loading the current stage.

`Stage` commands are as follows

```
public enum GebugCmdStage
{
    DefaultUnknown = 0,

    SetStage = 10,
}
```

### `Stage SetStage` Command

Sets `g_MainStageNum` to the supplied value. ROM enum type is `LEVEL_ID`, the C# equivalent is `LevelIdX`.

**Parameters**: 1 byte describing level.  
**Console reply**: no.  
**`ACK` Parameters**: N/A

## `Vi` Category

The `Vi` category handles methods video related methods such as found in `fr.c`, `vi.c`, `viewport.c`, etc.

`Vi` commands are as follows

```
public enum GebugCmdVi
{
    DefaultUnknown = 0,

    GrabFramebuffer = 10,
    SetFov = 20,
    SetZRange = 22,
    CurrentPlayerSetScreenSize = 40,
    CurrentPlayerSetScreenPosition = 42,
    CurrentPlayerSetPerspective = 44,
}
```

### `Vi GrabFramebuffer` Command

Dumps the framebuffer back to PC. There are a couple different ways to go about this, the native indy command accessed `g_ViBackData->framebuf` and `cfb_16`, but I couldn't achieve good results with that. The gebug ROM instead reads from `g_ViFrontData->framebuf` which still occasionally has tearing/artifacts.

**Parameters**: None.  
**Console reply**: yes.  
**`ACK` Parameters 0**: Signed 16 bit integer, containing result from `viGetX()` .  
**`ACK` Parameters 1**: Signed 16 bit integer, containing result from `viGetY()` .  
**`ACK` Parameters 2**: Contains `2 * viGetX() * viGetY()` bytes, as read from `g_ViFrontData->framebuf`. Data should be processed 16 bits at a time, read as N64 native 5551 RGBA format.

## `Meta` Category

The `Meta` consists of commands about the rom, and about the protocol itself.

`Meta` commands are as follows

```
public enum GebugCmdMeta
{
    DefaultUnknown = 0,

    ConfigEcho = 1,
    Ping = 2,
    Version = 10,
}
```

### `Meta Ping` Command

Sends a simple ping command.

**Parameters**: None.  
**Console reply**: yes.  
**`ACK` Parameters**: None.

### `Meta Version` Command

Version information about the ROM running on console.

**Parameters**: None.  
**Console reply**: yes.  
**`ACK` Parameters**: Returns 4 words describing the version as `1.2.3.4`.

## `Misc` Category

The `Misc` category is for single commands that don't fit into other categories.

`Misc` commands are as follows

```
public enum GebugCmdMisc
{
    DefaultUnknown = 0,

    OsTime = 1,
}
```

### `Misc OsTime` Command

Requests current ROM OS time, i.e., `osGetCount()`.

**Parameters**: None.  
**Console reply**: yes.  
**`ACK` Parameters**: Returns 1 word value of OS time.
