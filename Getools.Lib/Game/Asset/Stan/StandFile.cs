﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandFile
    {
        public StandFile()
        {
            Footer = new StandFileFooter()
            {
                C = "unstric",
            };
        }

        public StandFileHeader Header { get; set; }
        public List<StandFileTile> Tiles { get; set; } = new List<StandFileTile>();
        public List<StandFilePointList> PointLists { get; set; } = new List<StandFilePointList>();
        public StandFileFooter Footer { get; set; }

        public void WriteToCFile(StreamWriter sw)
        {
            StandFileTile tile = null;
            StandFilePointList pointList = null;

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

            if (PointLists.Count() != count)
            {
                throw new InvalidOperationException($"Number of tiles ({count}) does not match number of points lists ({PointLists.Count()})");
            }

            for (int i = 0; i < count; i++)
            {
                tile = Tiles[i];
                pointList = PointLists[i];

                if (tile.OrderId != pointList.OrderId)
                {
                    pointList = PointLists.FirstOrDefault(x => x.OrderId == tile.OrderId);

                    if (object.ReferenceEquals(null, pointList))
                    {
                        throw new InvalidOperationException($"Could not fine a point list to go with tile \"{tile.Name}\". The part after the underscore in the name should be the same for both.");
                    }
                }

                sw.Write(tile.ToCDeclaration());
                sw.Write(pointList.ToCDeclaration());
                sw.WriteLine();
            }

            sw.Write(Footer.ToCDeclaration());
            sw.WriteLine();
            sw.WriteLine();
        }

        public void WriteToBinFile(BinaryWriter bw)
        {
            StandFileTile tile = null;
            StandFilePointList pointList = null;

            Header.AppendToBinaryStream(bw);

            var count = Tiles.Count();

            if (PointLists.Count() != count)
            {
                throw new InvalidOperationException($"Number of tiles ({count}) does not match number of points lists ({PointLists.Count()})");
            }

            for (int i = 0; i < count; i++)
            {
                tile = Tiles[i];
                pointList = PointLists[i];

                if (tile.OrderId != pointList.OrderId)
                {
                    pointList = PointLists.FirstOrDefault(x => x.OrderId == tile.OrderId);

                    if (object.ReferenceEquals(null, pointList))
                    {
                        throw new InvalidOperationException($"Could not fine a point list to go with tile \"{tile.Name}\". The part after the underscore in the name should be the same for both.");
                    }
                }

                tile.AppendToBinaryStream(bw);
                pointList.AppendToBinaryStream(bw);
            }

            Footer.AppendToBinaryStream(bw);
        }

        public static StandFile ReadFromBinFile(BinaryReader br, string name)
        {
            var result = new StandFile();

            result.Header = StandFileHeader.ReadFromBinFile(br, name);

            int tileIndex = 0;
            int safety = UInt16.MaxValue + 1;

            try
            {
                while (tileIndex < safety)
                {
                    var tile = StandFileTile.ReadFromBinFile(br, tileIndex);
                    var pointsList = StandFilePointList.ReadFromBinFile(br, tileIndex, tile.PointCount);

                    result.Tiles.Add(tile);
                    result.PointLists.Add(pointsList);

                    tileIndex++;
                }
            }
            catch (Error.ExpectedStreamEndException)
            {
            }

            result.Footer = StandFileFooter.ReadFromBinFile(br);

            return result;
        }
    }
}
