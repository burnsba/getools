using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Getools.Lib.Game;

namespace Getools.Lib.Kaitai
{
    public class GbiVtxParser
    {
        public static List<GbiVtx> ParseVtxListFromMemory(byte[] bytes)
        {
            var kaitaiObject = Gen.Avtx.FromBytes(bytes);

            var results = new List<GbiVtx>();

            foreach (var v in kaitaiObject.Verteces)
            {
                results.Add(Convert(v));
            }

            return results;
        }

        private static GbiVtx Convert(Gen.Avtx.Vtx kaitaiObject)
        {
            var result = new GbiVtx()
            {
                Ob = new Coord3dshort()
                {
                    X = kaitaiObject.Ob.X,
                    Y = kaitaiObject.Ob.Y,
                    Z = kaitaiObject.Ob.Z,
                },
                Flag = kaitaiObject.Flag,
                TextureCoordinate = new Coord2dshort()
                {
                    X = kaitaiObject.TextureCoord.U,
                    Y = kaitaiObject.TextureCoord.V,
                },
                Normal = new Coord3dsbyte()
                {
                    X = kaitaiObject.Normal.X,
                    Y = kaitaiObject.Normal.Y,
                    Z = kaitaiObject.Normal.Z,
                },
                Alpha = kaitaiObject.Alpha,
            };

            return result;
        }
    }
}
