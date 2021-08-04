﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class StandTile
    {
        public const int SizeOfTileWithoutPoints = 8;
        public const int SizeOfBetaTileWithoutPoints = 12;

        public StandTile()
        {
        }

        /// <summary>
        /// 24 bits.
        /// </summary>
        public int InternalName { get; set; }

        public byte Room { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte Flags { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// 4 bits.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// Only used by citadel stan, otherwise not present. 16 bits.
        /// </summary>
        public short? UnknownBeta { get; set; } = null;

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// Should only be used for reading from binary file; <see cref="Points.Count"/> is used internally when possible.
        /// </summary>
        public byte PointCount { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// </summary>
        public byte HeaderC { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// </summary>
        public byte HeaderD { get; set; }

        /// <summary>
        /// 4 bits. (beta: 8 bits)
        /// </summary>
        public byte HeaderE { get; set; }

        public int OrderIndex { get; set; }

        public List<StandTilePoint> Points { get; set; } = new List<StandTilePoint>();

        // uses Points.Count instead of PointsCount property.
        public byte[] ToByteArray()
        {
            var results = new byte[SizeOfTileWithoutPoints + (Points.Count * StandTilePoint.SizeOf)];

            BitUtility.InsertLower24Big(results, 0, InternalName);
            results[3] = Room;

            results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
            results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));
            results[6] = (byte)((((byte)Points.Count & 0xf) << 4) | (HeaderC & 0xf));
            results[7] = (byte)(((HeaderD & 0xf) << 4) | (HeaderE & 0xf));

            int index = SizeOfTileWithoutPoints;
            for (int i=0; i<Points.Count; i++)
            {
                Array.Copy(Points[i].ToByteArray(), 0, results, index, StandTilePoint.SizeOf);
                index += StandTilePoint.SizeOf;
            }

            return results;
        }

        // uses Points.Count instead of PointsCount property.
        public byte[] ToBetaByteArray()
        {
            var results = new byte[SizeOfBetaTileWithoutPoints + (Points.Count * StandTilePoint.BetaSizeOf)];

            BitUtility.InsertLower24Big(results, 0, InternalName);
            results[3] = Room;

            results[4] = (byte)(((Flags & 0xf) << 4) | (R & 0xf));
            results[5] = (byte)(((G & 0xf) << 4) | (B & 0xf));

            BitUtility.InsertShortBig(results, 6, UnknownBeta ?? 0);

            results[8] = (byte)Points.Count;
            results[9] = HeaderC;
            results[10] = HeaderD;
            results[11] = HeaderE;

            int index = SizeOfBetaTileWithoutPoints;
            for (int i = 0; i < Points.Count; i++)
            {
                Array.Copy(Points[i].ToBetaByteArray(), 0, results, index, StandTilePoint.BetaSizeOf);
                index += StandTilePoint.BetaSizeOf;
            }

            return results;
        }

        public void AppendToBinaryStream(BinaryWriter stream)
        {
            var bytes = ToByteArray();
            stream.Write(bytes);
        }

        internal static StandTile ReadFromBinFile(BinaryReader br, int tileIndex)
        {
            var result = new StandTile();

            Byte b;

            b = br.ReadByte();
            result.InternalName = b << 16;
            b = br.ReadByte();
            result.InternalName |= b << 8;
            b = br.ReadByte();
            result.InternalName |= b;

            result.Room = br.ReadByte();

            // "Tile beginning with room 0 is the true way the file format ends, engine does not check for unstric string"
            if (result.Room == 0)
            {
                br.BaseStream.Seek(-4, SeekOrigin.Current);
                throw new Error.ExpectedStreamEndException();
            }

            b = br.ReadByte();
            result.Flags = (byte)((b >> 4) & 0xf);
            result.R = (byte)((b) & 0xf);

            b = br.ReadByte();
            result.G = (byte)((b >> 4) & 0xf);
            result.B = (byte)((b) & 0xf);

            b = br.ReadByte();
            result.PointCount = (byte)((b >> 4) & 0xf);
            result.HeaderC = (byte)((b) & 0xf);

            if (result.PointCount < 1)
            {
                throw new Exception("Tile is defined with zero points");
            }

            b = br.ReadByte();
            result.HeaderD = (byte)((b >> 4) & 0xf);
            result.HeaderE = (byte)((b) & 0xf);

            result.OrderIndex = tileIndex;

            // Done with tile header, now read points.

            for (int i = 0; i < result.PointCount; i++)
            {
                var point = StandTilePoint.ReadFromBinFile(br);
                result.Points.Add(point);
            }

            return result;
        }

        internal static StandTile ReadFromBetaBinFile(BinaryReader br, int tileIndex)
        {
            var result = new StandTile();

            Byte b;

            b = br.ReadByte();
            result.InternalName = b << 16;
            b = br.ReadByte();
            result.InternalName |= b << 8;
            b = br.ReadByte();
            result.InternalName |= b;

            result.Room = br.ReadByte();

            // "Tile beginning with room 0 is the true way the file format ends, engine does not check for unstric string"
            if (result.Room == 0)
            {
                br.BaseStream.Seek(-4, SeekOrigin.Current);
                throw new Error.ExpectedStreamEndException();
            }

            b = br.ReadByte();
            result.Flags = (byte)((b >> 4) & 0xf);
            result.R = (byte)((b) & 0xf);

            b = br.ReadByte();
            result.G = (byte)((b >> 4) & 0xf);
            result.B = (byte)((b) & 0xf);

            result.UnknownBeta = BitUtility.Read16Big(br);

            result.PointCount = br.ReadByte();

            if (result.PointCount < 1)
            {
                throw new Exception("Tile is defined with zero points");
            }

            result.HeaderC = br.ReadByte();
            result.HeaderD = br.ReadByte();
            result.HeaderE = br.ReadByte();

            result.OrderIndex = tileIndex;

            // Done with tile header, now read points.

            for (int i = 0; i < result.PointCount; i++)
            {
                var point = StandTilePoint.ReadFromBetaBinFile(br);
                result.Points.Add(point);
            }

            return result;
        }
    }
}
