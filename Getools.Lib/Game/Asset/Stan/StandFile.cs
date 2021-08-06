﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        /// Optional beta footer.
        /// </summary>
        public BetaFooter BetaFooter { get; set; } = null;

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
            foreach (var tile in Tiles)
            {
                tile.DeserializeFix();
            }

            if (!object.ReferenceEquals(null, BetaFooter))
            {
                BetaFooter.DeserializeFix();
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

            sw.Write(Header.ToCDeclaration());
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

            sw.Write(Header.ToBetaCDeclaration());
            sw.WriteLine();

            var count = Tiles.Count();

            foreach (var tile in Tiles)
            {
                sw.Write(tile.ToBetaCDeclaration());
                sw.WriteLine();
            }

            sw.WriteLine();

            sw.Write(Footer.ToBetaCDeclaration());
            sw.WriteLine();

            sw.Write(BetaFooter.ToBetaCDeclaration());
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
            BetaFooter.BetaAppendToBinaryStream(bw);
        }
    }
}
