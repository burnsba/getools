using System;
using System.Collections.Generic;
using System.Text;

namespace Getools.Lib.Game.Asset.SetupObject
{
    public class SetupObjectVehicle : SetupObjectGenericBase
    {
        private const int _dataSize = 136;

        public SetupObjectVehicle()
            : base(Propdef.Vehicle)
        {
        }

        public byte[] Data { get; set; } = new byte[_dataSize];

        public override string ToCInlineS32Array(string prefix = "")
        {
            var sb = new StringBuilder();

            sb.Append(prefix);
            AppendToCInlineS32Array(sb);

            return sb.ToString();
        }

        protected override void AppendToCInlineS32Array(StringBuilder sb)
        {
            base.AppendToCInlineS32Array(sb);

            var s32count = _dataSize / 4;
            int dataOffset = 0;
            for (int i = 0; i < s32count; i++, dataOffset += 4)
            {
                sb.Append(", ");
                sb.Append(Formatters.IntegralTypes.ToHex8(BitUtility.Read32Big(Data, dataOffset)));
            }
        }
    }
}
