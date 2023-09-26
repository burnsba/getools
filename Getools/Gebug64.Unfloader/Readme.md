# Gebug64.Unfloader

C# dotnet6 library, port of [UNFLoader](https://github.com/buu342/N64-UNFLoader), modified for use with gebug romhack. This is the PC application side of things.

On the console, the UNFLoader library provides basic communication support from N64 ROM over USB. The attached flashcart device (e.g., Everdrive) will have its own protocol which UNFLoader automatically manages. The gebug ROM implements another protocol layer on top of UNFLoader. This C# library will parse gebug level messages, as well as understand device specific protocol messages (for supported devices).

See [protocol readme](doc/ProtocolReadme.md) for information about the protocol.

## Supported devices

This library supports:

* Everdrive


## Project structure

The project is organized in the following structure:

```
Gebug64.Unfloader
├── General: general communication, like message bus.
├── Manage:
│   └── ConnectionServiceProvider.cs: High level message manager; core
│                                     communication library that ties everything together.
├── Protocol: Implementations for all layers of protocol communication.
│   ├── Flashcart: Flashcart packet and protocol implementation.
│   │   └── Message: Flashcart specific messages.
│   ├── Gebug: Gebug packet and protocol implementation
│   │   ├── Message: Gebug specific messages.
│   │   │   └── MessageType: Category and Command definitions.
│   │   └── Parameter: Parameter meta definitions.
│   ├── Parse: Parse context classes.
│   └── Unfloader: Unfloader packet and protocol implementation
│       └── Message: Unfloader specific messages.
│           └── MessageType: Unfloader message type.
└── SerialPort: Serial port wrapper and virtual serial port.
```
