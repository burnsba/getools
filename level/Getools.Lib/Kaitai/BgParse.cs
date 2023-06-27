using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Error;
using Getools.Lib.Game.Asset.Bg;
using Getools.Lib.Game.Asset.Setup;
using Getools.Lib.Game.Asset.Stan;
using GzipSharpLib;

namespace Getools.Lib.Kaitai
{
    /// <summary>
    /// Wrapper for Kaitai parser generated code.
    /// </summary>
    public static class BgParse
    {
        private const int _bgFilePointerAdjust = 0x0f000000;

        /// <summary>
        /// Reads a .bin file from disk and parses it using the Kaitai definition.
        /// </summary>
        /// <param name="path">Path to file to parse.</param>
        /// <returns>Newly created file.</returns>
        /// <remarks>
        /// All Kaitai struct objects are cleaned up / translated into
        /// cooresponding project objects.
        /// </remarks>
        public static BgFile ParseBin(string path)
        {
            var kaitaiObject = Gen.Bg.FromFile(path);

            var bg = Convert(kaitaiObject);

            //setup.DeserializeFix();

            return bg;
        }

        /// <summary>
        /// Converts Kaitai struct bg object to library bg object.
        /// </summary>
        /// <param name="kaitaiObject">Kaitai struct object.</param>
        /// <returns>Library object.</returns>
        private static BgFile Convert(Gen.Bg kaitaiObject)
        {
            var result = new BgFile();

            result.Header = Convert(kaitaiObject.HeaderBlock);

            var emptyRoom = Convert(kaitaiObject.HeaderBlock.RoomDataTableIgnore);
            result.RoomDataTable = Convert(kaitaiObject.HeaderBlock.RoomDataTable);
            foreach (var x in emptyRoom.Entries)
            {
                // kaitai struct repeats until the first pointer is null, so there should only be one entry.
                result.RoomDataTable.Entries.Insert(0, x);
            }

            result.Header.RoomDataTablePointer.AssignPointer(result.RoomDataTable);

            result.PortalDataTable = Convert(kaitaiObject.HeaderBlock.PortalDataTable);
            result.Header.PortalDataTablePointer.AssignPointer(result.PortalDataTable);

            result.GlobalVisibilityCommands = Convert(kaitaiObject.HeaderBlock.GlobalVisibilityCommands);
            result.Header.GlobalVisibilityCommandsPointer.AssignPointer(result.GlobalVisibilityCommands);

            ResolvePointTableData(result, kaitaiObject);

            return result;
        }

        private static BgFileHeader Convert(Gen.Bg.BgFileHeader kaitaiObject)
        {
            var result = new BgFileHeader();

            result.Unknown1 = (int)kaitaiObject.Unknown1;

            result.RoomDataTablePointer = new BinPack.PointerVariable();
            result.RoomDataTablePointer.PointedToOffset = (int)(kaitaiObject.RoomDataTablePointer - _bgFilePointerAdjust);

            result.PortalDataTablePointer = new BinPack.PointerVariable();
            result.PortalDataTablePointer.PointedToOffset = (int)(kaitaiObject.PortalDataTablePointer - _bgFilePointerAdjust);

            result.GlobalVisibilityCommandsPointer = new BinPack.PointerVariable();
            result.GlobalVisibilityCommandsPointer.PointedToOffset = (int)(kaitaiObject.GlobalVisibilityCommandsPointer - _bgFilePointerAdjust);

            result.Unknown2 = (int)kaitaiObject.Unknown2;

            return result;
        }

        private static BgFileRoomDataTable Convert(List<Gen.Bg.BgFileRoomDataEntry> kaitaiObjects)
        {
            var result = new BgFileRoomDataTable();
            result.Entries = new List<BgFileRoomDataEntry>();

            int index = 0;
            foreach (var roomEntry in kaitaiObjects)
            {
                var libRoomData = Convert(roomEntry);

                libRoomData.OrderIndex = index;

                result.Entries.Add(libRoomData);

                index++;
            }

            return result;
        }

        private static BgFileRoomDataEntry Convert(Gen.Bg.BgFileRoomDataEntry kaitaiObject)
        {
            var result = new BgFileRoomDataEntry();

            result.PointTablePointer = new BinPack.PointerVariable();
            result.PointTablePointer.PointedToOffset = (int)(kaitaiObject.PointTablePointer - _bgFilePointerAdjust);

            result.PrimaryDisplayList = new BinPack.PointerVariable();
            result.PrimaryDisplayList.PointedToOffset = (int)(kaitaiObject.PrimaryDisplayListPointer - _bgFilePointerAdjust);

            //// TODO: display list support

            result.SecondaryDisplayList = new BinPack.PointerVariable();
            result.SecondaryDisplayList.PointedToOffset = (int)(kaitaiObject.SecondaryDisplayListPointer - _bgFilePointerAdjust);

            result.Coord = new Game.Coord3df()
            {
                X = kaitaiObject.Coord.X,
                Y = kaitaiObject.Coord.Y,
                Z = kaitaiObject.Coord.Z,
            };

            return result;
        }

        private static BgFileVisibilityCommandTable Convert(List<Gen.Bg.VisibilityCommand> kaitaiObjects)
        {
            var result = new BgFileVisibilityCommandTable();
            result.Entries = new List<GlobalVisibilityCommand>();

            int index = 0;
            foreach (var roomEntry in kaitaiObjects)
            {
                var libRoomData = Convert(roomEntry);

                libRoomData.OrderIndex = index;

                result.Entries.Add(libRoomData);

                index++;
            }

            return result;
        }

        private static GlobalVisibilityCommand Convert(Gen.Bg.VisibilityCommand kaitaiObject)
        {
            var result = new GlobalVisibilityCommand()
            {
                Command = kaitaiObject.Command,
            };

            return result;
        }

        private static BgFilePortalDataTable Convert(List<Gen.Bg.BgFilePortalDataEntry> kaitaiObjects)
        {
            var result = new BgFilePortalDataTable();
            result.Entries = new List<BgFilePortalDataEntry>();

            int index = 0;
            foreach (var portalDataEntry in kaitaiObjects)
            {
                var libportalDataEntry = Convert(portalDataEntry);

                libportalDataEntry.OrderIndex = index;

                result.Entries.Add(libportalDataEntry);

                index++;
            }

            return result;
        }

        private static BgFilePortalDataEntry Convert(Gen.Bg.BgFilePortalDataEntry kaitaiObject)
        {
            var result = new BgFilePortalDataEntry();

            result.Portal = Convert(kaitaiObject.Portal);

            result.PortalPointer = new BinPack.PointerVariable();
            result.PortalPointer.PointedToOffset = (int)(kaitaiObject.PortalPointer - _bgFilePointerAdjust);
            result.PortalPointer.AssignPointer(result.Portal);

            result.ConnectedRoom1 = kaitaiObject.ConnectedRoom1;
            result.ConnectedRoom2 = kaitaiObject.ConnectedRoom2;
            result.ControlFlags = kaitaiObject.ControlFlags;

            return result;
        }

        private static BgFilePortal Convert(Gen.Bg.BgFilePortal kaitaiObject)
        {
            var result = new BgFilePortal();

            result.NumberPoints = kaitaiObject.NumberPoints;
            result.Points = new List<Game.Coord3df>();

            foreach (var p in kaitaiObject.Points)
            {
                result.Points.Add(new Game.Coord3df()
                {
                    X = p.X,
                    Y = p.Y,
                    Z = p.Z,
                });
            }

            return result;
        }

        private static void ResolvePointTableData(BgFile result, Gen.Bg kaitaiObject)
        {
            var logger = new GzipSharpLib.Logging.Logger();

            var startLengthData = new List<(int, int)>();

            var offsets = result.RoomDataTable.Entries
                .Where(x => x.PointTablePointer.PointedToOffset > 0)
                .Select(x => x.PointTablePointer.PointedToOffset)
                .OrderBy(x => x)
                .ToList();

            for (int i = 0; i < offsets.Count() - 1; i++)
            {
                startLengthData.Add((offsets[i], offsets[i + 1] - offsets[i]));
            }

            kaitaiObject.M_Io.Seek(0);
            var bytes = kaitaiObject.M_Io.ReadBytesFull();

            foreach (var roomData in result.RoomDataTable.Entries)
            {
                var binDataInfo = startLengthData.First(x => x.Item1 == roomData.PointTablePointer.PointedToOffset);
                var binData = new byte[binDataInfo.Item2];
                Array.Copy(bytes, binDataInfo.Item1, binData, 0, binDataInfo.Item2);

                var gzipContext = new GzipSharpLib.Context(logger);

                using (var ms = new MemoryStream(binData))
                {
                    gzipContext.Source = ms;
                    var inflateResult = gzipContext.Execute();

                    if (inflateResult != ReturnCode.Ok)
                    {
                        throw new Exception("Error inflating bg point table data");
                    }

                    if (object.ReferenceEquals(null, gzipContext.Destination))
                    {
                        throw new NullReferenceException("Destination not set.");
                    }

                    var contentResult = gzipContext.Destination.ToArray();

                    roomData.Points = GbiVtxParser.ParseVtxListFromMemory(contentResult);
                }
            }
        }
    }
}
