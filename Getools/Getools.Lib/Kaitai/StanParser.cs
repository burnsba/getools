using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.Error;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Intro;
using Getools.Lib.Game.Asset.SetupObject;
using Getools.Lib.Game.Asset.Stan;

namespace Getools.Lib.Kaitai
{
    /// <summary>
    /// Wrapper for Kaitai parser generated code.
    /// </summary>
    public static class StanParser
    {
        /// <summary>
        /// Reads a .bin file from disk and parses it using the Kaitai definition.
        /// </summary>
        /// <param name="path">Path to file to parse.</param>
        /// <returns>Newly created file.</returns>
        /// <remarks>
        /// All Kaitai struct objects are cleaned up / translated into
        /// cooresponding project objects.
        /// </remarks>
        public static StandFile ParseBin(string path)
        {
            var kaitaiObject = Gen.Stan.FromFile(path);

            var stan = Convert(kaitaiObject);

            if (stan.Tiles == null || !stan.Tiles.Any())
            {
                throw new Getools.Lib.Error.BadFileFormatException("Error, read stan file without any points");
            }

            stan.SetFormat(TypeFormat.Normal);

            stan.DeserializeFix();

            return stan;
        }

        /// <summary>
        /// Converts Kaitai struct stan object to library stan object.
        /// </summary>
        /// <param name="kaitaiObject">Kaitai struct object.</param>
        /// <returns>Library object.</returns>
        private static StandFile Convert(Gen.Stan kaitaiObject)
        {
            var result = new StandFile();

            result.Header = Convert(kaitaiObject.HeaderBlock);
            result.Tiles = Convert(kaitaiObject.Tiles);
            result.Footer = Convert(kaitaiObject.Footer);

            return result;
        }

        private static StandFileHeader Convert(Gen.Stan.StandFileHeader kaitaiObject)
        {
            var result = new StandFileHeader();

            result.Unknown1 = ParseHelpers.ToCPointer(kaitaiObject.Unknown1);
            result.UnknownHeaderData = kaitaiObject.UnknownHeaderData.ToList();

            return result;
        }

        private static List<StandTile> Convert(List<Gen.Stan.StandTile> kaitaiObjects)
        {
            var results = new List<StandTile>();

            int index = 0;
            foreach (var tile in kaitaiObjects)
            {
                var libTile = Convert(tile);

                libTile.OrderIndex = index;

                results.Add(libTile);

                index++;
            }

            return results;
        }

        private static StandTile Convert(Gen.Stan.StandTile kaitaiObject)
        {
            var result = new StandTile();

            result.InternalName = (int)kaitaiObject.InternalName;
            result.Room = (byte)kaitaiObject.Room;
            result.Flags = (byte)kaitaiObject.Flags;
            result.R = (byte)kaitaiObject.Red;
            result.G = (byte)kaitaiObject.Green;
            result.B = (byte)kaitaiObject.Blue;
            result.PointCount = (byte)kaitaiObject.PointCount;
            result.FirstPoint = (byte)kaitaiObject.FirstPoint;
            result.SecondPoint = (byte)kaitaiObject.SecondPoint;
            result.ThirdPoint = (byte)kaitaiObject.ThirdPoint;

            foreach (var p in kaitaiObject.Points)
            {
                result.Points.Add(Convert(p));
            }

            if (!result.Points.Any())
            {
                // the last tile before the footer is empty, so this will only be an error if the pointcount is greater than zero.
                if (result.PointCount > 0)
                {
                    throw new BadFileFormatException($"Error, {nameof(result.PointCount)} > 0, but there are no points");
                }
            }

            return result;
        }

        private static StandTilePoint Convert(Gen.Stan.StandTilePoint kaitaiObject)
        {
            var result = new StandTilePoint();

            result.X = kaitaiObject.X;
            result.Y = kaitaiObject.Y;
            result.Z = kaitaiObject.Z;
            result.Link = kaitaiObject.Link;

            return result;
        }

        private static StandFileFooter Convert(Gen.Stan.StandTileFooter kaitaiObject)
        {
            var result = new StandFileFooter();

            result.C = kaitaiObject.Unstric;
            result.Unknown3 = ParseHelpers.ToCPointer(kaitaiObject.Unknown3);
            result.Unknown4 = ParseHelpers.ToCPointer(kaitaiObject.Unknown4);
            result.Unknown5 = ParseHelpers.ToCPointer(kaitaiObject.Unknown5);

            return result;
        }
    }
}
