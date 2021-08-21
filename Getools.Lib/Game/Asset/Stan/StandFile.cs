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
        public StandFileHeader Header { get; set; }

        /// <summary>
        /// List of tiles.
        /// </summary>
        public List<StandTile> Tiles { get; set; } = new List<StandTile>();

        /// <summary>
        /// Footer.
        /// </summary>
        public StandFileFooter Footer { get; set; }

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
            // need to set the pointer to correct value
            Header.FirstTileOffset = Header.GetDataSizeOf();

            var rodataLocation = GetDataSizeOf();
            int offset = rodataLocation;

            foreach (var tile in Tiles)
            {
                tile.DeserializeFix();

                offset += StandTile.TileNameStringLength;
            }
        }

        /// <summary>
        /// Calculates the .data section size of this file.
        /// </summary>
        /// <returns>Compile size, without .rodata.</returns>
        public int GetDataSizeOf()
        {
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

            string tileName = Tiles.First().VariableName;
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
        /// This is a mini-compiler to build up the .data and .rodata sections to send them to <see cref="AssembledFile"/>.
        /// </summary>
        /// <returns>Assembled file. Call <see cref="AssembledFile.GetLinkedFile"/> to get fully linked file.</returns>
        internal AssembledFile GetAssembledBinFile()
        {
            var file = new AssembledFile();
            var dataSectionOffset = 0;

            var headerBytes = Header.ToByteArray();
            file.AppendData(headerBytes);
            dataSectionOffset += headerBytes.Length;

            foreach (var tile in Tiles)
            {
                var tileBytes = tile.ToByteArray();

                file.AppendData(tileBytes);

                if (tile.Format == TypeFormat.Beta)
                {
                    tile.DebugName.Offset = dataSectionOffset;
                    var pr = new PointerRodata(tile.DebugName, PointerRodata.SizeOfRodataPointer);

                    file.RodataPointers.Add(pr);
                }

                dataSectionOffset += tileBytes.Length;
            }

            file.AppendData(Footer.ToByteArray(dataSectionOffset));

            return file;
        }

        /// <summary>
        /// Builds the entire .bin file describing stan and writes to stream at the current position.
        /// </summary>
        /// <param name="bw">Binary stream to write to.</param>
        internal void WriteToBinFile(BinaryWriter bw)
        {
            var file = GetAssembledBinFile();
            var fileContents = file.GetLinkedFile();

            bw.Write(fileContents);
        }
    }
}
