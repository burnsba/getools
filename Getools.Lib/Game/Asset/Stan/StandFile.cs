using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Getools.Lib.Error;

namespace Getools.Lib.Game.Asset.Stan
{
    /// <summary>
    /// Complete stan.
    /// </summary>
    public class StandFile
    {
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
        /// </summary>
        public TypeFormat Format { get; set; }

        /// <summary>
        /// Sets the format. Visits children and updates any format specific values.
        /// Calling this method manually should be followed by a call to <see cref="DeserializeFix"/>.
        /// </summary>
        /// <param name="format">Format.</param>
        public void SetFormat(TypeFormat format)
        {
            Format = format;

            foreach (var tile in Tiles)
            {
                tile.SetFormat(format);
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

                tile.TileNameOffset = offset;

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
                + Footer.GetDataSizeOf();

            return BitUtility.Align16(dataSize);
        }

        /// <summary>
        /// Reads the stream from the current position. Interprets as the .rodata
        /// section of a binary file.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        internal void ReadRoData(BinaryReader br)
        {
            // nothing to do.
        }

        /// <summary>
        /// Reads the stream from the current position. Interprets as the .rodata
        /// section of a binary file.
        /// </summary>
        /// <param name="br">Stream to read.</param>
        internal void BetaReadRoData(BinaryReader br)
        {
            var rostrings = new List<string>();
            var rodataOffset = new List<long>();

            var buffer = new Byte[16];
            long position = br.BaseStream.Position;
            Byte b;
            int bufferPosition = 0;

            // read 8 character strings until end of file
            while (position < br.BaseStream.Length - 1)
            {
                b = br.ReadByte();
                if (b > 0)
                {
                    buffer[bufferPosition] = b;

                    // track read position to save into tile
                    if (bufferPosition == 0)
                    {
                        rodataOffset.Add(br.BaseStream.Position - 1);
                    }

                    bufferPosition++;
                }
                else if (b == 0)
                {
                    if (buffer[0] > 0)
                    {
                        var pointName = System.Text.Encoding.ASCII.GetString(buffer, 0, bufferPosition);
                        rostrings.Add(pointName);

                        Array.Clear(buffer, 0, 16);
                        bufferPosition = 0;
                    }
                }

                if (bufferPosition >= 16)
                {
                    throw new BadFileFormatException($"Error reading stan, beta point name exceeded buffer length. Stream positiion: {position}");
                }

                position++;
            }

            if (buffer[0] > 0)
            {
                var pointName = System.Text.Encoding.ASCII.GetString(buffer, 0, bufferPosition);
                rostrings.Add(pointName);
            }

            // reading rodata should occur after reading tiles, so if this fails, it should
            // fail as expected:
            if (Tiles.Count != rostrings.Count)
            {
                throw new BadFileFormatException($"Error reading stan: .rodata strings count (={rostrings.Count}) does not match tiles count (={Tiles.Count})");
            }

            for (int i = 0; i < Tiles.Count; i++)
            {
                Tiles[i].TileName = rostrings[i];
                Tiles[i].TileNameOffset = (short)rodataOffset[i];
            }
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

            foreach (var filename in Config.Stan.IncludeHeaders)
            {
                sw.WriteLine($"#include \"{filename}\"");
            }

            sw.WriteLine();

            string tileName = Tiles.First().VariableName;
            string tilePointer = "&" + tileName;
            string tileForwardDeclaration = $"{Config.Stan.TileCTypeName} {tileName};";

            sw.WriteLine("// forward declarations");
            sw.WriteLine(tileForwardDeclaration);
            sw.WriteLine();

            sw.Write(Header.ToCDeclaration(tilePointer));
            sw.WriteLine();

            var count = Tiles.Count();

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
        /// Builds the entire .c file describing stan using the beta format
        /// and writes to stream at the current position.
        /// </summary>
        /// <param name="sw">Stream to write to</param>
        internal void WriteToBetaCFile(StreamWriter sw)
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

            foreach (var filename in Config.Stan.IncludeHeaders)
            {
                sw.WriteLine($"#include \"{filename}\"");
            }

            sw.WriteLine();

            string tileName = Tiles.First().VariableName;
            string tilePointer = "&" + tileName;
            string tileForwardDeclaration = $"{Config.Stan.TileBetaCTypeName} {tileName};";

            sw.WriteLine("// forward declarations");
            sw.WriteLine(tileForwardDeclaration);
            sw.WriteLine();

            sw.Write(Header.ToBetaCDeclaration(tilePointer));
            sw.WriteLine();

            var count = Tiles.Count();

            foreach (var tile in Tiles)
            {
                sw.Write(tile.ToBetaCDeclaration());
                sw.WriteLine();
            }

            sw.WriteLine();

            sw.Write(Footer.ToCDeclaration());
            sw.WriteLine();
            sw.WriteLine();
        }

        /// <summary>
        /// Converts stan to byte array as it would appear in .bin file
        /// and writes to stream at the current position.
        /// </summary>
        /// <param name="bw">Stream to write to</param>
        internal void WriteToBinFile(BinaryWriter bw)
        {
            Header.AppendToBinaryStream(bw);

            foreach (var tile in Tiles)
            {
                tile.AppendToBinaryStream(bw);
            }

            Footer.AppendToBinaryStream(bw);
            AppendRodataToBinaryStream(bw);
        }

        /// <summary>
        /// Converts stan to byte array as it would appear in .bin file using the beta format
        /// and writes to stream at the current position.
        /// </summary>
        /// <param name="bw">Stream to write to</param>
        internal void WriteToBetaBinFile(BinaryWriter bw)
        {
            Header.BetaAppendToBinaryStream(bw);

            foreach (var tile in Tiles)
            {
                tile.BetaAppendToBinaryStream(bw);
            }

            Footer.BetaAppendToBinaryStream(bw);
            AppendBetaRodataToBinaryStream(bw);
        }

        private void AppendRodataToBinaryStream(BinaryWriter bw)
        {
            // nothing to do.
        }

        private void AppendBetaRodataToBinaryStream(BinaryWriter bw)
        {
            var bytes = GetBetaRodataAsByteArray();
            bw.Write(bytes);
        }

        private byte[] GetBetaRodataAsByteArray()
        {
            bool appendEmpty = false;
            var pointsCount = Tiles.Count();

            // only have one example, so not sure if there is supposed to be a null entry at
            // the end, or if it's supposed to pad to a multiple of 16.
            if ((pointsCount & 0x1) > 0)
            {
                appendEmpty = true;
            }

            var allocCount = pointsCount + (appendEmpty ? 1 : 0);

            var results = new byte[allocCount * StandTile.TileNameStringLength];

            int index = 0;
            foreach (var tile in Tiles)
            {
                string s = (tile.TileName.Length >= StandTile.TileNameStringLength)
                    ? tile.TileName.Substring(0, StandTile.TileNameStringLength - 1)
                    : tile.TileName;
                Array.Copy(System.Text.Encoding.ASCII.GetBytes(s), 0, results, index, s.Length);

                index += StandTile.TileNameStringLength;
            }

            return results;
        }
    }
}
