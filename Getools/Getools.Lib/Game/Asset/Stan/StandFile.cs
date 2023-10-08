using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Complete stan.
    /// Make sure to call <see cref="SetFormat"/> when converting between beta and normal formats.
    /// </summary>
    public class StandFile
    {
        /// <summary>
        /// C headers to #include when building .c files.
        /// </summary>
        public static List<string> IncludeHeaders = new List<string>()
            {
                "ultra64.h",
                "stan.h",
            };

        /// <summary>
        /// Formats available for reading in a <see cref="Stan.StandFile"/>.
        /// </summary>
        public static List<DataFormats> SupportedInputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

        /// <summary>
        /// Formats available to output a <see cref="Stan.StandFile"/>.
        /// </summary>
        public static List<DataFormats> SupportedOutputFormats = new List<DataFormats>()
            {
                DataFormats.C,
                DataFormats.BetaC,
                DataFormats.Json,
                DataFormats.Bin,
                DataFormats.BetaBin,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="StandFile"/> class.
        /// </summary>
        public StandFile()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandFile"/> class.
        /// </summary>
        /// <param name="format">Stan format.</param>
        public StandFile(TypeFormat format)
        {
            Format = format;
        }

        /// <summary>
        /// Header.
        /// </summary>
        public StandFileHeader? Header { get; set; }

        /// <summary>
        /// List of tiles.
        /// </summary>
        public List<StandTile> Tiles { get; set; } = new List<StandTile>();

        /// <summary>
        /// Footer.
        /// </summary>
        public StandFileFooter? Footer { get; set; }

        /// <summary>
        /// Gets or sets explanation for how object should be serialized to JSON.
        /// Make sure to call <see cref="SetFormat"/> when converting between beta and normal formats.
        /// </summary>
        public TypeFormat Format { get; set; }

        /// <summary>
        /// Sets the format. Visits children and updates any format specific values.
        /// Calling this method manually should be followed by a call to <see cref="DeserializeFix"/>.
        /// Make sure to call this when converting between beta and normal formats.
        /// </summary>
        /// <param name="format">Format.</param>
        public void SetFormat(TypeFormat format)
        {
            Format = format;

            foreach (var tile in Tiles)
            {
                tile.SetFormat(format);
            }

            // perhaps a bit hacky, but the beta stan file ends the tile sequence with a normal tile.
            // So try to adjust for that here.
            var last = Tiles.Last();
            if (format == TypeFormat.Beta && !object.ReferenceEquals(null, last))
            {
                if (last.PointCount == 0 && !last.Points.Any())
                {
                    last.SetFormat(TypeFormat.Normal);
                }
            }
        }

        /// <summary>
        /// Should be called after deserializing. Cleans up values/properties
        /// based on the known format.
        /// </summary>
        public void DeserializeFix()
        {
            if (object.ReferenceEquals(null, Header))
            {
                throw new NullReferenceException();
            }

            if (Tiles.Any())
            {
                // check if variable name was parsed from .c file, if so then
                // resolve to a tile or throw.
                // Otherwise, set the pointer to the first tile (if there are any).
                if (!object.ReferenceEquals(null, Header.FirstTilePointer)
                    && !string.IsNullOrEmpty(Header.FirstTilePointer.AddressOfVariableName)
                    && Header.FirstTilePointer.IsNull)
                {
                    var pointsTo = Tiles.FirstOrDefault(x => x.VariableName == Header.FirstTilePointer.AddressOfVariableName);

                    if (object.ReferenceEquals(null, pointsTo))
                    {
                        throw new NullReferenceException($"Error trying to resolve first-tile pointer in header. Address of variable name is \"{Header.FirstTilePointer.AddressOfVariableName}\", but no tile's {nameof(StandTile.VariableName)} match");
                    }

                    Header.FirstTilePointer.AssignPointer(pointsTo);
                }
                else
                {
                    Header.FirstTilePointer = new PointerVariable(Tiles.First());
                }

                Header.FirstTilePointer.BaseDataOffset = 1 * Config.TargetWordSize;
            }

            if (object.ReferenceEquals(null, Header.FirstTilePointer))
            {
                throw new NullReferenceException("stan header pointer to first tile (lib object) is null");
            }

            int offset = Header.BaseDataSize;
            foreach (var tile in Tiles)
            {
                tile.BaseDataOffset = offset;
                tile.DeserializeFix();

                offset += tile.GetDataSizeOf();
            }

            if (string.IsNullOrEmpty(Header.FirstTilePointer.AddressOfVariableName))
            {
                if (object.ReferenceEquals(null, Header.FirstTilePointer))
                {
                    throw new NullReferenceException();
                }

                // can only assign name after DeserializeFix is called on the tile
                var st = (StandTile?)Header.FirstTilePointer.Dereference();

                if (object.ReferenceEquals(null, st) || string.IsNullOrEmpty(st.VariableName))
                {
                    throw new NullReferenceException();
                }

                Header.FirstTilePointer.AddressOfVariableName = st.VariableName;
            }
        }

        /// <summary>
        /// Calculates the .data section size of this file.
        /// </summary>
        /// <returns>Compile size, without .rodata.</returns>
        public int GetDataSizeOf()
        {
            if (object.ReferenceEquals(null, Header))
            {
                throw new NullReferenceException();
            }

            int tileSize = Tiles.Sum(x => x.GetDataSizeOf());

            var dataSize = Header.GetDataSizeOf()
                + tileSize
                + StandFileFooter.GetDataSizeOf();

            return BitUtility.Align16(dataSize);
        }

        /// <summary>
        /// Builds the entire .c file describing stan and writes to stream at the current position.
        /// </summary>
        /// <param name="sw">Stream to write to</param>
        internal void WriteToCFile(StreamWriter sw)
        {
            if (object.ReferenceEquals(null, Header))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, Footer))
            {
                throw new NullReferenceException();
            }

            sw.WriteLine("/*");

            foreach (var prefix in Config.COutputPrefix)
            {
                sw.WriteLine($"* {prefix}");
            }

            sw.WriteLine($"* {DateTime.Now.ToLongDateString()} {DateTime.Now.ToLongTimeString()}");

            var assemblyInfo = Utility.GetAutoGeneratedAssemblyVersion();

            sw.WriteLine($"* {assemblyInfo}");
            sw.WriteLine("*/");
            sw.WriteLine();

            foreach (var filename in StandFile.IncludeHeaders)
            {
                sw.WriteLine($"#include \"{filename}\"");
            }

            sw.WriteLine();

            if (!Tiles.Any())
            {
                throw new InvalidOperationException("No tiles in stan");
            }

            string tileName = Tiles.First().VariableName!;

            if (string.IsNullOrEmpty(tileName))
            {
                throw new NullReferenceException("Variable name not set");
            }

            string tilePointer = "&" + tileName;
            string tileForwardDeclaration = $"{Tiles.First().GetCTypeName()} {tileName};";

            sw.WriteLine("// forward declarations");
            sw.WriteLine(tileForwardDeclaration);
            sw.WriteLine();

            sw.Write(Header.ToCDeclaration(tilePointer));
            sw.WriteLine();

            foreach (var tile in Tiles)
            {
                sw.Write(tile.ToCDeclaration());
                sw.WriteLine();
            }

            sw.WriteLine();

            sw.Write(Footer.ToCDeclaration());
            sw.WriteLine();
            sw.WriteLine();
        }

        /// <summary>
        /// Adds lib objects to the file, so they can be compiled into .bin
        /// in the correct order.
        /// </summary>
        /// <param name="file">File to add stan to.</param>
        internal void AddToMipsFile(MipsFile file)
        {
            if (object.ReferenceEquals(null, Header))
            {
                throw new NullReferenceException();
            }

            if (object.ReferenceEquals(null, Footer))
            {
                throw new NullReferenceException();
            }

            Header.Collect(file);

            foreach (var tile in Tiles)
            {
                tile.Collect(file);
            }

            Footer.Collect(file);
        }

        /// <summary>
        /// Builds the entire .bin file describing stan and writes to stream at the current position.
        /// </summary>
        /// <param name="bw">Binary stream to write to.</param>
        internal void WriteToBinFile(BinaryWriter bw)
        {
            var file = new MipsFile();

            AddToMipsFile(file);

            file.Assemble();
            var fileContents = file.GetLinkedFile();

            bw.Write(fileContents);
        }
    }
}
