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
    public static class BetaStanParser
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
            var kaitaiObject = Gen.StanBeta.FromFile(path);

            var stan = Convert(kaitaiObject);

            if (stan.Tiles == null || !stan.Tiles.Any())
            {
                throw new Getools.Lib.Error.BadFileFormatException("Error, read stan file without any points");
            }

            stan.SetFormat(TypeFormat.Beta);

            stan.DeserializeFix();

            return stan;
        }

        /// <summary>
        /// Converts Kaitai struct stan object to library stan object.
        /// </summary>
        /// <param name="kaitaiObject">Kaitai struct object.</param>
        /// <returns>Library object.</returns>
        private static StandFile Convert(Gen.StanBeta kaitaiObject)
        {
            var result = new StandFile();

            result.Header = Convert(kaitaiObject.HeaderBlock);
            result.Tiles = Convert(kaitaiObject.Tiles);
            result.Footer = Convert(kaitaiObject.Footer);

            return result;
        }

        private static StandFileHeader Convert(Gen.StanBeta.StandFileHeader kaitaiObject)
        {
            var result = new StandFileHeader();

            result.Unknown1 = ParseHelpers.ToCPointer(kaitaiObject.Unknown1);
            result.UnknownHeaderData = kaitaiObject.UnknownHeaderData.ToList();

            return result;
        }

        private static List<StandTile> Convert(List<Gen.StanBeta.BetaStandTile> kaitaiObjects)
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

        private static StandTile Convert(Gen.StanBeta.BetaStandTile kaitaiObject)
        {
            var result = new StandTile();

            result.DebugName = kaitaiObject.DebugName.Deref;

            result.UnknownBeta = (short)kaitaiObject.BetaUnknown;
            result.Flags = (byte)kaitaiObject.Flags;
            result.R = (byte)kaitaiObject.Red;
            result.G = (byte)kaitaiObject.Green;
            result.B = (byte)kaitaiObject.Blue;

            if (!object.ReferenceEquals(null, result.DebugName)
                && !object.ReferenceEquals(null, kaitaiObject.Tail))
            {
                result.PointCount = kaitaiObject.Tail.PointCount;
                result.FirstPoint = kaitaiObject.Tail.FirstPoint;
                result.SecondPoint = kaitaiObject.Tail.SecondPoint;
                result.ThirdPoint = kaitaiObject.Tail.ThirdPoint;

                foreach (var p in kaitaiObject.Tail.Points)
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
            }

            return result;
        }

        private static StandTilePoint Convert(Gen.StanBeta.BetaStandTilePoint kaitaiObject)
        {
            var result = new StandTilePoint();

            result.FloatX = kaitaiObject.X;
            result.FloatY = kaitaiObject.Y;
            result.FloatZ = kaitaiObject.Z;
            result.Link = (int)kaitaiObject.Link;

            return result;
        }

        private static StandFileFooter Convert(Gen.StanBeta.StandTileFooter kaitaiObject)
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
